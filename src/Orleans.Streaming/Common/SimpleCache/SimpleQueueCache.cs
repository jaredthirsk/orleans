using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Forkleans.Runtime;
using Forkleans.Streams;

namespace Forkleans.Providers.Streams.Common
{
    internal class CacheBucket
    {
        // For backpressure detection we maintain a histogram of 10 buckets.
        // Every bucket records how many items are in the cache in that bucket
        // and how many cursors are poinmting to an item in that bucket.
        // We update the NumCurrentItems when we add and remove cache item (potentially opening or removing a bucket)
        // We update NumCurrentCursors every time we move a cursor
        // If the first (most outdated bucket) has at least one cursor pointing to it, we say we are under back pressure (in a full cache).
        internal int NumCurrentItems { get; private set; }
        internal int NumCurrentCursors { get; private set; }

        internal void UpdateNumItems(int val)
        {
            NumCurrentItems = NumCurrentItems + val;
        }
        internal void UpdateNumCursors(int val)
        {
            NumCurrentCursors = NumCurrentCursors + val;
        }
    }

    internal class SimpleQueueCacheItem
    {
        internal IBatchContainer Batch;
        internal bool DeliveryFailure;
        internal StreamSequenceToken SequenceToken;
        internal CacheBucket CacheBucket;
    }

    /// <summary>
    /// A queue cache that keeps items in memory.
    /// </summary>
    public partial class SimpleQueueCache : IQueueCache
    {
        private readonly LinkedList<SimpleQueueCacheItem> cachedMessages;
        private readonly int maxCacheSize;
        private readonly ILogger logger;
        private readonly List<CacheBucket> cacheCursorHistogram; // for backpressure detection
        private const int NUM_CACHE_HISTOGRAM_BUCKETS = 10;
        private readonly int CACHE_HISTOGRAM_MAX_BUCKET_SIZE;

        /// <summary>
        /// Gets the number of items in the cache.
        /// </summary>
        public int Size => cachedMessages.Count;

        /// <inheritdoc />
        public int GetMaxAddCount()
        {
            return CACHE_HISTOGRAM_MAX_BUCKET_SIZE;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleQueueCache"/> class.
        /// </summary>
        /// <param name="cacheSize">Size of the cache.</param>
        /// <param name="logger">The logger.</param>
        public SimpleQueueCache(int cacheSize, ILogger logger)
        {
            cachedMessages = new LinkedList<SimpleQueueCacheItem>();
            maxCacheSize = cacheSize;

            this.logger = logger;
            cacheCursorHistogram = new List<CacheBucket>();
            CACHE_HISTOGRAM_MAX_BUCKET_SIZE = Math.Max(cacheSize / NUM_CACHE_HISTOGRAM_BUCKETS, 1); // we have 10 buckets
        }

        /// <inheritdoc />
        public virtual bool IsUnderPressure()
        {
            return cacheCursorHistogram.Count >= NUM_CACHE_HISTOGRAM_BUCKETS;
        }


        /// <inheritdoc />
        public virtual bool TryPurgeFromCache(out IList<IBatchContainer> purgedItems)
        {
            purgedItems = null;
            if (cachedMessages.Count == 0) return false; // empty cache
            if (cacheCursorHistogram.Count == 0) return false;  // no cursors yet - zero consumers basically yet.
            if (cacheCursorHistogram[0].NumCurrentCursors > 0) return false; // consumers are still active in the oldest bucket - fast path

            var allItems = new List<IBatchContainer>();
            while (cacheCursorHistogram.Count > 0 && cacheCursorHistogram[0].NumCurrentCursors == 0)
            {
                List<IBatchContainer> items = DrainBucket(cacheCursorHistogram[0]);
                allItems.AddRange(items);
                cacheCursorHistogram.RemoveAt(0); // remove the last bucket
            }
            purgedItems = allItems;

            LogDebugTryPurgeFromCache(allItems.Count);

            return true;
        }

        private List<IBatchContainer> DrainBucket(CacheBucket bucket)
        {
            var itemsToRelease = new List<IBatchContainer>(bucket.NumCurrentItems);
            // walk all items in the cache starting from last
            // and remove from the cache the oness that reside in the given bucket until we jump to a next bucket
            while (bucket.NumCurrentItems > 0)
            {
                SimpleQueueCacheItem item = cachedMessages.Last.Value;
                if (item.CacheBucket.Equals(bucket))
                {
                    if (!item.DeliveryFailure)
                    {
                        itemsToRelease.Add(item.Batch);
                    }
                    bucket.UpdateNumItems(-1);
                    cachedMessages.RemoveLast();
                }
                else
                {
                    // this item already points into the next bucket, so stop.
                    break;
                }
            }
            return itemsToRelease;
        }

        /// <inheritdoc />
        public virtual void AddToCache(IList<IBatchContainer> msgs)
        {
            if (msgs == null) throw new ArgumentNullException(nameof(msgs));

            LogDebugAddToCache(msgs.Count);
            foreach (var message in msgs)
            {
                Add(message, message.SequenceToken);
            }
        }

        /// <inheritdoc />
        public virtual IQueueCacheCursor GetCacheCursor(StreamId streamId, StreamSequenceToken token)
        {
            var cursor = new SimpleQueueCacheCursor(this, streamId, logger);
            InitializeCursor(cursor, token);
            return cursor;
        }

        internal void InitializeCursor(SimpleQueueCacheCursor cursor, StreamSequenceToken sequenceToken)
        {
            LogDebugInitializeCursor(cursor, sequenceToken);

            // Nothing in cache, unset token, and wait for more data.
            if (cachedMessages.Count == 0)
            {
                UnsetCursor(cursor, sequenceToken);
                return;
            }

            // if no token is provided, set token to item at end of cache
            sequenceToken = sequenceToken ?? cachedMessages.First?.Value?.SequenceToken;

            // If sequenceToken is too new to be in cache, unset token, and wait for more data.
            if (sequenceToken.Newer(cachedMessages.First.Value.SequenceToken))
            {
                UnsetCursor(cursor, sequenceToken);
                return;
            }

            LinkedListNode<SimpleQueueCacheItem> lastMessage = cachedMessages.Last;
            // Check to see if offset is too old to be in cache
            if (sequenceToken.Older(lastMessage.Value.SequenceToken))
            {
                throw new QueueCacheMissException(sequenceToken, cachedMessages.Last.Value.SequenceToken, cachedMessages.First.Value.SequenceToken);
            }

            // Now the requested sequenceToken is set and is also within the limits of the cache.

            // Find first message at or below offset
            // Events are ordered from newest to oldest, so iterate from start of list until we hit a node at a previous offset, or the end.
            LinkedListNode<SimpleQueueCacheItem> node = cachedMessages.First;
            while (node != null && node.Value.SequenceToken.Newer(sequenceToken))
            {
                // did we get to the end?
                if (node.Next == null) // node is the last message
                    break;

                // if sequenceId is between the two, take the higher
                if (node.Next.Value.SequenceToken.Older(sequenceToken))
                    break;

                node = node.Next;
            }

            // return cursor from start.
            SetCursor(cursor, node);
        }

        internal void RefreshCursor(SimpleQueueCacheCursor cursor, StreamSequenceToken sequenceToken)
        {
            LogDebugRefreshCursor(cursor, sequenceToken);

            // set if unset
            if (!cursor.IsSet)
            {
                InitializeCursor(cursor, cursor.SequenceToken ?? sequenceToken);
            }
        }

        /// <summary>
        /// Acquires the next message in the cache at the provided cursor
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="batch"></param>
        /// <returns></returns>
        internal bool TryGetNextMessage(SimpleQueueCacheCursor cursor, out IBatchContainer batch)
        {
            LogDebugTryGetNextMessage(cursor);

            batch = null;
            if (!cursor.IsSet) return false;

            // If we are at the end of the cache unset cursor and move offset one forward
            if (cursor.Element == cachedMessages.First)
            {
                UnsetCursor(cursor, null);
            }
            else // Advance to next:
            {
                AdvanceCursor(cursor, cursor.Element.Previous);
            }

            batch = cursor.Element?.Value.Batch;
            return (batch != null);
        }

        private void AdvanceCursor(SimpleQueueCacheCursor cursor, LinkedListNode<SimpleQueueCacheItem> item)
        {
            LogDebugUpdateCursor(cursor, item.Value.Batch);

            cursor.Element.Value.CacheBucket.UpdateNumCursors(-1); // remove from prev bucket
            cursor.Set(item);
            cursor.Element.Value.CacheBucket.UpdateNumCursors(1);  // add to next bucket
        }

        internal void SetCursor(SimpleQueueCacheCursor cursor, LinkedListNode<SimpleQueueCacheItem> item)
        {
            LogDebugSetCursor(cursor, item.Value.Batch);

            cursor.Set(item);
            cursor.Element.Value.CacheBucket.UpdateNumCursors(1);  // add to next bucket
        }

        internal void UnsetCursor(SimpleQueueCacheCursor cursor, StreamSequenceToken token)
        {
            LogDebugUnsetCursor(cursor);

            if (cursor.IsSet)
            {
                cursor.Element.Value.CacheBucket.UpdateNumCursors(-1);
            }
            cursor.UnSet(token);
        }

        private void Add(IBatchContainer batch, StreamSequenceToken sequenceToken)
        {
            if (batch == null) throw new ArgumentNullException(nameof(batch));
            // this should never happen, but just in case
            if (Size >= maxCacheSize) throw new CacheFullException();

            CacheBucket cacheBucket;
            if (cacheCursorHistogram.Count == 0)
            {
                cacheBucket = new CacheBucket();
                cacheCursorHistogram.Add(cacheBucket);
            }
            else
            {
                cacheBucket = cacheCursorHistogram[cacheCursorHistogram.Count - 1]; // last one
            }

            if (cacheBucket.NumCurrentItems == CACHE_HISTOGRAM_MAX_BUCKET_SIZE) // last bucket is full, open a new one
            {
                cacheBucket = new CacheBucket();
                cacheCursorHistogram.Add(cacheBucket);
            }

            // Add message to linked list
            var item = new SimpleQueueCacheItem
            {
                Batch = batch,
                SequenceToken = sequenceToken,
                CacheBucket = cacheBucket
            };

            cachedMessages.AddFirst(new LinkedListNode<SimpleQueueCacheItem>(item));
            cacheBucket.UpdateNumItems(1);
        }

        [LoggerMessage(
            Level = LogLevel.Debug,
            Message = "TryPurgeFromCache: purged {PurgedItemsCount} items from cache."
        )]
        private partial void LogDebugTryPurgeFromCache(int purgedItemsCount);

        [LoggerMessage(
            Level = LogLevel.Debug,
            Message = "AddToCache: added {ItemCount} items to cache."
        )]
        private partial void LogDebugAddToCache(int itemCount);

        [LoggerMessage(
            Level = LogLevel.Debug,
            Message = "InitializeCursor: {Cursor} to sequenceToken {SequenceToken}"
        )]
        private partial void LogDebugInitializeCursor(SimpleQueueCacheCursor cursor, StreamSequenceToken sequenceToken);

        [LoggerMessage(
            Level = LogLevel.Debug,
            Message = "RefreshCursor: {RefreshCursor} to sequenceToken {SequenceToken}"
        )]
        private partial void LogDebugRefreshCursor(SimpleQueueCacheCursor refreshCursor, StreamSequenceToken sequenceToken);

        [LoggerMessage(
            Level = LogLevel.Debug,
            Message = "TryGetNextMessage: {Cursor}"
        )]
        private partial void LogDebugTryGetNextMessage(SimpleQueueCacheCursor cursor);

        [LoggerMessage(
            Level = LogLevel.Debug,
            Message = "UpdateCursor: {Cursor} to item {Item}"
        )]
        private partial void LogDebugUpdateCursor(SimpleQueueCacheCursor cursor, IBatchContainer item);

        [LoggerMessage(
            Level = LogLevel.Debug,
            Message = "SetCursor: {Cursor} to item {Item}"
        )]
        private partial void LogDebugSetCursor(SimpleQueueCacheCursor cursor, IBatchContainer item);

        [LoggerMessage(
            Level = LogLevel.Debug,
            Message = "UnsetCursor: {Cursor}"
        )]
        private partial void LogDebugUnsetCursor(SimpleQueueCacheCursor cursor);
    }
}
