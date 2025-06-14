using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Forkleans.Internal;

namespace Forkleans.Runtime.Utilities
{
    internal static class AsyncEnumerable
    {
        internal static readonly object InitialValue = new();
        internal static readonly object DisposedValue = new();
    }

    internal sealed class AsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly object _updateLock = new();
        private readonly Func<T, T, bool> _updateValidator;
        private readonly Action<T> _onPublished;
        private Element _current;
        
        public AsyncEnumerable(T initialValue, Func<T, T, bool> updateValidator, Action<T> onPublished)
        {
            _updateValidator = updateValidator;
            _current = new Element(initialValue);
            _onPublished = onPublished;
            onPublished(initialValue);
        }

        public bool TryPublish(T value) => TryPublish(new Element(value)) == PublishResult.Success;
        
        public void Publish(T value)
        {
            switch (TryPublish(new Element(value)))
            {
                case PublishResult.Success:
                    return;
                case PublishResult.InvalidUpdate:
                    ThrowInvalidUpdate();
                    break;
                case PublishResult.Disposed:
                    ThrowDisposed();
                    break;
            }
        }

        public bool TryPublish<TState>(Func<T, TState, T> updateFunc, TState state) => TryPublishCore(updateFunc, state) == PublishResult.Success;
        
        public void Publish<TState>(Func<T, TState, T> updateFunc, TState state)
        {
            switch (TryPublishCore(updateFunc, state))
            {
                case PublishResult.Success:
                    return;
                case PublishResult.InvalidUpdate:
                    ThrowInvalidUpdate();
                    break;
                case PublishResult.Disposed:
                    ThrowDisposed();
                    break;
            }
        }

        private PublishResult TryPublish(Element newItem)
        {
            if (_current.IsDisposed) return PublishResult.Disposed;

            lock (_updateLock)
            {
                if (_current.IsDisposed) return PublishResult.Disposed;

                if (_current.IsValid && newItem.IsValid && !_updateValidator(_current.Value, newItem.Value))
                {
                    return PublishResult.InvalidUpdate;
                }

                var curr = _current;
                Interlocked.Exchange(ref _current, newItem);
                if (newItem.IsValid) _onPublished(newItem.Value);
                curr.SetNext(newItem);

                return PublishResult.Success;
            }
        }

        private PublishResult TryPublishCore<TState>(Func<T, TState, T> updateFunc, TState state)
        {
            if (_current.IsDisposed) return PublishResult.Disposed;

            lock (_updateLock)
            {
                if (_current.IsDisposed) return PublishResult.Disposed;

                var curr = _current;
                var newItem = new Element(updateFunc(curr.Value, state));

                if (curr.IsValid && newItem.IsValid && !_updateValidator(curr.Value, newItem.Value))
                {
                    return PublishResult.InvalidUpdate;
                }

                Interlocked.Exchange(ref _current, newItem);
                if (newItem.IsValid) _onPublished(newItem.Value);
                curr.SetNext(newItem);

                return PublishResult.Success;
            }
        }

        public void Dispose()
        {
            if (_current.IsDisposed) return;

            lock (_updateLock)
            {
                if (_current.IsDisposed) return;

                TryPublish(Element.CreateDisposed());
            }
        }

        [DoesNotReturn]
        private static void ThrowInvalidUpdate() => throw new ArgumentException("The value was not valid.");

        [DoesNotReturn]
        private static void ThrowDisposed() => throw new ObjectDisposedException("This instance has been disposed.");

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new AsyncEnumerator(_current, cancellationToken);

        private enum PublishResult
        {
            Success,
            InvalidUpdate,
            Disposed
        }

        private sealed class AsyncEnumerator : IAsyncEnumerator<T>
        {
            private readonly CancellationTokenSource _cts;
            private Element _current;

            public AsyncEnumerator(Element initial, CancellationToken cancellationToken)
            {
                _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                if (!initial.IsValid)
                {
                    _current = initial;
                }
                else
                {
                    var result = Element.CreateInitial();
                    result.SetNext(initial);
                    _current = result;
                }
            }

            T IAsyncEnumerator<T>.Current => _current.Value;

            async ValueTask<bool> IAsyncEnumerator<T>.MoveNextAsync()
            {
                if (_current.IsDisposed)
                {
                    return false;
                }

                _current = await _current.NextAsync().WaitAsync(_cts.Token);
                return _current.IsValid;
            }

            async ValueTask IAsyncDisposable.DisposeAsync()
            {
                await _cts.CancelAsync().SuppressThrowing();
                _cts.Dispose();
            }
        }

        private sealed class Element
        {
            private readonly TaskCompletionSource<Element> _next;
            private readonly object _value;

            public Element(T value) : this(value, new TaskCompletionSource<Element>(TaskCreationOptions.RunContinuationsAsynchronously))
            {
            }

            private Element(object value, TaskCompletionSource<Element> next)
            {
                _value = value;
                _next = next;
            }

            public static Element CreateInitial() => new(
                AsyncEnumerable.InitialValue,
                new TaskCompletionSource<Element>(TaskCreationOptions.RunContinuationsAsynchronously));

            public static Element CreateDisposed()
            {
                var tcs = new TaskCompletionSource<Element>(TaskCreationOptions.RunContinuationsAsynchronously);
                tcs.SetException(new ObjectDisposedException("This instance has been disposed"));
                return new Element(AsyncEnumerable.DisposedValue, tcs);
            }

            public bool IsValid => !IsInitial && !IsDisposed;

            public T Value
            {
                get
                {
                    if (IsInitial) ThrowInvalidInstance();
                    ObjectDisposedException.ThrowIf(IsDisposed, this);
                    if (_value is T typedValue) return typedValue;
                    return default;
                }
            }

            public bool IsInitial => ReferenceEquals(_value, AsyncEnumerable.InitialValue);
            public bool IsDisposed => ReferenceEquals(_value, AsyncEnumerable.DisposedValue);

            public Task<Element> NextAsync() => _next.Task;

            public void SetNext(Element next) => _next.SetResult(next);

            private static void ThrowInvalidInstance() => throw new InvalidOperationException("This instance does not have a value set.");
        }
    }
}
