using System;
using System.Runtime.CompilerServices;

#nullable enable
namespace Forkleans.Runtime
{
    [Serializable, GenerateSerializer, Immutable]
    internal readonly struct CorrelationId : IEquatable<CorrelationId>, IComparable<CorrelationId>, ISpanFormattable
    {
        [Id(0)]
        private readonly long id;
        private static long lastUsed;

        public CorrelationId(long value) => id = value;

        public CorrelationId(CorrelationId other) => id = other.id;

        public static CorrelationId GetNext() => new(System.Threading.Interlocked.Increment(ref lastUsed));

        public override int GetHashCode() => id.GetHashCode();

        public override bool Equals(object? obj) => obj is CorrelationId correlationId && Equals(correlationId);

        public bool Equals(CorrelationId other) => id == other.id;

        public static bool operator ==(CorrelationId lhs, CorrelationId rhs) => rhs.id == lhs.id;

        public static bool operator !=(CorrelationId lhs, CorrelationId rhs) => rhs.id != lhs.id;

        public int CompareTo(CorrelationId other) => id.CompareTo(other.id);

        public override string ToString() => id.ToString("X16");

        string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => id.ToString(format ?? "X16", formatProvider);

        bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            if (format.IsEmpty)
            {
                format = "X16";
            }

            return id.TryFormat(destination, out charsWritten, format, provider);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal long ToInt64() => id;
    }
}
