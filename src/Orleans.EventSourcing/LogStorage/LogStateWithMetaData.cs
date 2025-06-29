using Forkleans.EventSourcing.Common;
using System;
using System.Collections.Generic;

namespace Forkleans.EventSourcing.LogStorage
{
    /// <summary>
    /// A class that extends grain state with versioning metadata, so that a grain with log-view consistency
    /// can use a standard storage provider.
    /// </summary>
    /// <typeparam name="TEntry">The type used for log entries</typeparam>
    [Serializable]
    [GenerateSerializer]
    public sealed class LogStateWithMetaDataAndETag<TEntry> : IGrainState<LogStateWithMetaData<TEntry>> where TEntry : class
    {
        /// <summary>
        /// Gets and Sets StateAndMetaData
        /// </summary>
        [Id(0)]
        public LogStateWithMetaData<TEntry> StateAndMetaData { get; set; }

        /// <summary>
        /// Gets and Sets Etag
        /// </summary>
        [Id(1)]
        public string ETag { get; set; }

        [Id(2)]
        public bool RecordExists { get; set; }

        public LogStateWithMetaData<TEntry> State { get => StateAndMetaData; set => StateAndMetaData = value; }

        /// <summary>
        /// Initializes a new instance of GrainStateWithMetaDataAndETag class
        /// </summary>
        public LogStateWithMetaDataAndETag()
        {
            StateAndMetaData = new LogStateWithMetaData<TEntry>();
        }

        /// <summary>
        /// Convert current GrainStateWithMetaDataAndETag object information to a string
        /// </summary>
        public override string ToString()
        {
            return string.Format("v{0} Flags={1} ETag={2} Data={3}", StateAndMetaData.GlobalVersion, StateAndMetaData.WriteVector, ETag, StateAndMetaData.Log);
        }
    }

    /// <summary>
    /// A class that extends grain state with versioning metadata, so that a log-consistent grain
    /// can use a standard storage provider.
    /// </summary>
    [Serializable]
    [GenerateSerializer]
    public sealed class LogStateWithMetaData<TEntry> where TEntry : class
    {
        /// <summary>
        /// The stored view of the log
        /// </summary>
        [Id(0)]
        public List<TEntry> Log { get; set; }

        /// <summary>
        /// The length of the log
        /// </summary>
        public int GlobalVersion { get { return Log.Count; } }

        /// <summary>
        /// Metadata that is used to avoid duplicate appends.
        /// Logically, this is a (string->bit) map, the keys being replica ids
        /// But this map is represented compactly as a simple string to reduce serialization/deserialization overhead
        /// Bits are read by <see cref="GetBit"/> and flipped by  <see cref="FlipBit"/>.
        /// Bits are toggled when writing, so that the retry logic can avoid appending an entry twice
        /// when retrying a failed append.
        /// </summary>
        [Id(1)]
        public string WriteVector { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogStateWithMetaData{TView}"/> class.
        /// </summary>
        public LogStateWithMetaData()
        {
            Log = [];
            WriteVector = "";
        }

        /// <summary>
        /// Gets one of the bits in <see cref="WriteVector"/>
        /// </summary>
        /// <param name="replica">The replica for which we want to look up the bit</param>
        /// <returns></returns>
        public bool GetBit(string replica)
        {
            return StringEncodedWriteVector.GetBit(WriteVector, replica);
        }

        /// <summary>
        /// Toggle one of the bits in <see cref="WriteVector"/> and return the new value.
        /// </summary>
        /// <param name="replica">The replica for which we want to flip the bit</param>
        /// <returns>the state of the bit after flipping it</returns>
        public bool FlipBit(string replica)
        {
            var str = WriteVector;
            var result = StringEncodedWriteVector.FlipBit(ref str, replica);
            WriteVector = str;
            return result;
        }
    }
}
