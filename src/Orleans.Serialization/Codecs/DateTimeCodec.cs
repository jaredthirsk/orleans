using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Forkleans.Serialization.Buffers;
using Forkleans.Serialization.WireProtocol;

namespace Forkleans.Serialization.Codecs
{
    /// <summary>
    /// Serializer for <see cref="DateTime"/>.
    /// </summary>
    [RegisterSerializer]
    public sealed class DateTimeCodec : IFieldCodec<DateTime>
    {
        void IFieldCodec<DateTime>.WriteField<TBufferWriter>(ref Writer<TBufferWriter> writer, uint fieldIdDelta, Type expectedType, DateTime value)
        {
            ReferenceCodec.MarkValueField(writer.Session);
            writer.WriteFieldHeader(fieldIdDelta, expectedType, typeof(DateTime), WireType.Fixed64);
            writer.WriteInt64(value.ToBinary());
        }

        /// <summary>
        /// Writes a field without type info (expected type is statically known).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteField<TBufferWriter>(ref Writer<TBufferWriter> writer, uint fieldIdDelta, DateTime value) where TBufferWriter : IBufferWriter<byte>
        {
            ReferenceCodec.MarkValueField(writer.Session);
            writer.WriteFieldHeaderExpected(fieldIdDelta, WireType.Fixed64);
            writer.WriteInt64(value.ToBinary());
        }

        /// <inheritdoc/>
        DateTime IFieldCodec<DateTime>.ReadValue<TInput>(ref Reader<TInput> reader, Field field) => ReadValue(ref reader, field);

        /// <summary>
        /// Reads a <see cref="DateTime"/> value.
        /// </summary>
        /// <typeparam name="TInput">The reader input type.</typeparam>
        /// <param name="reader">The reader.</param>
        /// <param name="field">The field.</param>
        /// <returns>The <see cref="DateTime"/> value.</returns>
        public static DateTime ReadValue<TInput>(ref Reader<TInput> reader, Field field)
        {
            ReferenceCodec.MarkValueField(reader.Session);
            field.EnsureWireType(WireType.Fixed64);
            return DateTime.FromBinary(reader.ReadInt64());
        }
    }
}