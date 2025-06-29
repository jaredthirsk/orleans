using System;
using System.Buffers;
using Forkleans.Serialization.Buffers;
using Forkleans.Serialization.Cloning;
using Forkleans.Serialization.GeneratedCodeHelpers;
using Forkleans.Serialization.WireProtocol;

namespace Forkleans.Serialization.Codecs
{
    /// <summary>
    /// Serializer for <see cref="ValueTuple"/>.
    /// </summary>
    [RegisterSerializer]
    public sealed class ValueTupleCodec : IFieldCodec<ValueTuple>
    {
        /// <inheritdoc />
        public void WriteField<TBufferWriter>(ref Writer<TBufferWriter> writer, uint fieldIdDelta, Type expectedType, ValueTuple value) where TBufferWriter : IBufferWriter<byte>
        {
            ReferenceCodec.MarkValueField(writer.Session);
            writer.WriteFieldHeader(fieldIdDelta, expectedType, value.GetType(), WireType.VarInt);
            writer.WriteVarUInt7(0);
        }

        /// <inheritdoc />
        public ValueTuple ReadValue<TInput>(ref Reader<TInput> reader, Field field)
        {
            field.EnsureWireType(WireType.VarInt);

            ReferenceCodec.MarkValueField(reader.Session);
            var length = reader.ReadVarUInt32();
            if (length != 0) throw new UnexpectedLengthPrefixValueException(nameof(ValueTuple), 0, length);

            return default;
        }
    }

    /// <summary>
    /// Serializer for <see cref="ValueTuple{T1}"/>.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    [RegisterSerializer]
    public sealed class ValueTupleCodec<T> : IFieldCodec<ValueTuple<T>>
    {
        private readonly Type ElementType1 = typeof(T);

        private readonly IFieldCodec<T> _valueCodec;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTupleCodec{T}"/> class.
        /// </summary>
        /// <param name="valueCodec">The value codec.</param>
        public ValueTupleCodec(IFieldCodec<T> valueCodec)
        {
            _valueCodec = ForkleansGeneratedCodeHelper.UnwrapService(this, valueCodec);
        }

        /// <inheritdoc />
        public void WriteField<TBufferWriter>(
            ref Writer<TBufferWriter> writer,
            uint fieldIdDelta,
            Type expectedType,
            ValueTuple<T> value) where TBufferWriter : IBufferWriter<byte>
        {
            ReferenceCodec.MarkValueField(writer.Session);
            writer.WriteFieldHeader(fieldIdDelta, expectedType, value.GetType(), WireType.TagDelimited);

            _valueCodec.WriteField(ref writer, 1, ElementType1, value.Item1);

            writer.WriteEndObject();
        }

        /// <inheritdoc />
        public ValueTuple<T> ReadValue<TInput>(ref Reader<TInput> reader, Field field)
        {
            field.EnsureWireTypeTagDelimited();
            ReferenceCodec.MarkValueField(reader.Session);
            var item1 = default(T);
            uint fieldId = 0;
            while (true)
            {
                var header = reader.ReadFieldHeader();
                if (header.IsEndBaseOrEndObject)
                {
                    break;
                }

                fieldId += header.FieldIdDelta;
                switch (fieldId)
                {
                    case 1:
                        item1 = _valueCodec.ReadValue(ref reader, header);
                        break;
                    default:
                        reader.ConsumeUnknownField(header);
                        break;
                }
            }

            return new ValueTuple<T>(item1);
        }
    }

    /// <summary>
    /// Copier for <see cref="ValueTuple"/>.
    /// </summary>
    [RegisterCopier]
    public sealed class ValueTupleCopier : IDeepCopier<ValueTuple>, IOptionalDeepCopier
    {
        /// <inheritdoc />
        public bool IsShallowCopyable() => true;

        /// <inheritdoc />
        object IDeepCopier.DeepCopy(object input, CopyContext context) => input;

        /// <inheritdoc />
        public ValueTuple DeepCopy(ValueTuple input, CopyContext context) => input;
    }

    /// <summary>
    /// Copier for <see cref="ValueTuple{T}"/>.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    [RegisterCopier]
    public sealed class ValueTupleCopier<T> : IDeepCopier<ValueTuple<T>>, IOptionalDeepCopier
    {
        private readonly IDeepCopier<T> _copier;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTupleCopier{T}"/> class.
        /// </summary>
        /// <param name="copier">The copier.</param>
        public ValueTupleCopier(IDeepCopier<T> copier) => _copier = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier);

        /// <inheritdoc />
        public bool IsShallowCopyable() => _copier is null;

        object IDeepCopier.DeepCopy(object input, CopyContext context) => IsShallowCopyable() ? input : DeepCopy((ValueTuple<T>)input, context);

        /// <inheritdoc />
        public ValueTuple<T> DeepCopy(ValueTuple<T> input, CopyContext context)
        {
            if (_copier != null) input.Item1 = _copier.DeepCopy(input.Item1, context);
            return input;
        }
    }

    /// <summary>
    /// Serializer for <see cref="ValueTuple{T1, T2}"/>
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    [RegisterSerializer]
    public sealed class ValueTupleCodec<T1, T2> : IFieldCodec<ValueTuple<T1, T2>>
    {
        private readonly Type ElementType1 = typeof(T1);
        private readonly Type ElementType2 = typeof(T2);

        private readonly IFieldCodec<T1> _item1Codec;
        private readonly IFieldCodec<T2> _item2Codec;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTupleCodec{T1, T2}"/> class.
        /// </summary>
        /// <param name="item1Codec">The <typeparamref name="T1"/> codec.</param>
        /// <param name="item2Codec">The <typeparamref name="T2"/> codec.</param>
        public ValueTupleCodec(IFieldCodec<T1> item1Codec, IFieldCodec<T2> item2Codec)
        {
            _item1Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item1Codec);
            _item2Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item2Codec);
        }

        /// <inheritdoc />
        public void WriteField<TBufferWriter>(
            ref Writer<TBufferWriter> writer,
            uint fieldIdDelta,
            Type expectedType,
            (T1, T2) value) where TBufferWriter : IBufferWriter<byte>
        {
            ReferenceCodec.MarkValueField(writer.Session);
            writer.WriteFieldHeader(fieldIdDelta, expectedType, value.GetType(), WireType.TagDelimited);

            _item1Codec.WriteField(ref writer, 1, ElementType1, value.Item1);
            _item2Codec.WriteField(ref writer, 1, ElementType2, value.Item2);

            writer.WriteEndObject();
        }

        /// <inheritdoc />
        public (T1, T2) ReadValue<TInput>(ref Reader<TInput> reader, Field field)
        {
            field.EnsureWireTypeTagDelimited();
            ReferenceCodec.MarkValueField(reader.Session);
            (T1, T2) res = default;
            uint fieldId = 0;
            while (true)
            {
                var header = reader.ReadFieldHeader();
                if (header.IsEndBaseOrEndObject)
                {
                    break;
                }

                fieldId += header.FieldIdDelta;
                switch (fieldId)
                {
                    case 1: res.Item1 = _item1Codec.ReadValue(ref reader, header); break;
                    case 2: res.Item2 = _item2Codec.ReadValue(ref reader, header); break;
                    default:
                        reader.ConsumeUnknownField(header);
                        break;
                }
            }

            return res;
        }
    }

    /// <summary>
    /// Copier for <see cref="ValueTuple{T1, T2}"/>
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    [RegisterCopier]
    public sealed class ValueTupleCopier<T1, T2> : IDeepCopier<ValueTuple<T1, T2>>, IOptionalDeepCopier
    {
        private readonly IDeepCopier<T1> _copier1;
        private readonly IDeepCopier<T2> _copier2;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTupleCopier{T1, T2}"/> class.
        /// </summary>
        /// <param name="copier1">The copier for <typeparamref name="T1"/>.</param>
        /// <param name="copier2">The copier for <typeparamref name="T2"/>.</param>
        public ValueTupleCopier(IDeepCopier<T1> copier1, IDeepCopier<T2> copier2)
        {
            _copier1 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier1);
            _copier2 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier2);
        }

        public bool IsShallowCopyable() => _copier1 is null && _copier2 is null;

        object IDeepCopier.DeepCopy(object input, CopyContext context) => IsShallowCopyable() ? input : DeepCopy(((T1, T2))input, context);

        /// <inheritdoc />
        public ValueTuple<T1, T2> DeepCopy(ValueTuple<T1, T2> input, CopyContext context)
        {
            if (_copier1 != null) input.Item1 = _copier1.DeepCopy(input.Item1, context);
            if (_copier2 != null) input.Item2 = _copier2.DeepCopy(input.Item2, context);
            return input;
        }
    }

    /// <summary>
    /// Serializer for <see cref="ValueTuple{T1, T2, T3}"/>.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    [RegisterSerializer]
    public sealed class ValueTupleCodec<T1, T2, T3> : IFieldCodec<ValueTuple<T1, T2, T3>>
    {
        private readonly Type ElementType1 = typeof(T1);
        private readonly Type ElementType2 = typeof(T2);
        private readonly Type ElementType3 = typeof(T3);

        private readonly IFieldCodec<T1> _item1Codec;
        private readonly IFieldCodec<T2> _item2Codec;
        private readonly IFieldCodec<T3> _item3Codec;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTupleCodec{T1, T2, T3}"/> class.
        /// </summary>
        /// <param name="item1Codec">The <typeparamref name="T1"/> codec.</param>
        /// <param name="item2Codec">The <typeparamref name="T2"/> codec.</param>
        /// <param name="item3Codec">The <typeparamref name="T3"/> codec.</param>
        public ValueTupleCodec(
            IFieldCodec<T1> item1Codec,
            IFieldCodec<T2> item2Codec,
            IFieldCodec<T3> item3Codec)
        {
            _item1Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item1Codec);
            _item2Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item2Codec);
            _item3Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item3Codec);
        }

        /// <inheritdoc />
        public void WriteField<TBufferWriter>(
            ref Writer<TBufferWriter> writer,
            uint fieldIdDelta,
            Type expectedType,
            (T1, T2, T3) value) where TBufferWriter : IBufferWriter<byte>
        {
            ReferenceCodec.MarkValueField(writer.Session);
            writer.WriteFieldHeader(fieldIdDelta, expectedType, value.GetType(), WireType.TagDelimited);

            _item1Codec.WriteField(ref writer, 1, ElementType1, value.Item1);
            _item2Codec.WriteField(ref writer, 1, ElementType2, value.Item2);
            _item3Codec.WriteField(ref writer, 1, ElementType3, value.Item3);

            writer.WriteEndObject();
        }

        /// <inheritdoc />
        public (T1, T2, T3) ReadValue<TInput>(ref Reader<TInput> reader, Field field)
        {
            field.EnsureWireTypeTagDelimited();
            ReferenceCodec.MarkValueField(reader.Session);
            (T1, T2, T3) res = default;
            uint fieldId = 0;
            while (true)
            {
                var header = reader.ReadFieldHeader();
                if (header.IsEndBaseOrEndObject)
                {
                    break;
                }

                fieldId += header.FieldIdDelta;
                switch (fieldId)
                {
                    case 1: res.Item1 = _item1Codec.ReadValue(ref reader, header); break;
                    case 2: res.Item2 = _item2Codec.ReadValue(ref reader, header); break;
                    case 3: res.Item3 = _item3Codec.ReadValue(ref reader, header); break;
                    default:
                        reader.ConsumeUnknownField(header);
                        break;
                }
            }

            return res;
        }
    }

    /// <summary>
    /// Copier for <see cref="ValueTuple{T1, T2, T3}"/>.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    [RegisterCopier]
    public sealed class ValueTupleCopier<T1, T2, T3> : IDeepCopier<ValueTuple<T1, T2, T3>>, IOptionalDeepCopier
    {
        private readonly IDeepCopier<T1> _copier1;
        private readonly IDeepCopier<T2> _copier2;
        private readonly IDeepCopier<T3> _copier3;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTupleCopier{T1, T2, T3}"/> class.
        /// </summary>
        /// <param name="copier1">The <typeparamref name="T1"/> copier.</param>
        /// <param name="copier2">The <typeparamref name="T2"/> copier.</param>
        /// <param name="copier3">The <typeparamref name="T3"/> copier.</param>
        public ValueTupleCopier(
            IDeepCopier<T1> copier1,
            IDeepCopier<T2> copier2,
            IDeepCopier<T3> copier3)
        {
            _copier1 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier1);
            _copier2 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier2);
            _copier3 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier3);
        }

        public bool IsShallowCopyable() => _copier1 is null && _copier2 is null && _copier3 is null;

        object IDeepCopier.DeepCopy(object input, CopyContext context) => IsShallowCopyable() ? input : DeepCopy(((T1, T2, T3))input, context);

        /// <inheritdoc />
        public ValueTuple<T1, T2, T3> DeepCopy(ValueTuple<T1, T2, T3> input, CopyContext context)
        {
            if (_copier1 != null) input.Item1 = _copier1.DeepCopy(input.Item1, context);
            if (_copier2 != null) input.Item2 = _copier2.DeepCopy(input.Item2, context);
            if (_copier3 != null) input.Item3 = _copier3.DeepCopy(input.Item3, context);
            return input;
        }
    }

    /// <summary>
    /// Serializer for <see cref="ValueTuple{T1, T2, T3, T4}"/>.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    /// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
    [RegisterSerializer]
    public sealed class ValueTupleCodec<T1, T2, T3, T4> : IFieldCodec<ValueTuple<T1, T2, T3, T4>>
    {
        private readonly Type ElementType1 = typeof(T1);
        private readonly Type ElementType2 = typeof(T2);
        private readonly Type ElementType3 = typeof(T3);
        private readonly Type ElementType4 = typeof(T4);

        private readonly IFieldCodec<T1> _item1Codec;
        private readonly IFieldCodec<T2> _item2Codec;
        private readonly IFieldCodec<T3> _item3Codec;
        private readonly IFieldCodec<T4> _item4Codec;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTupleCodec{T1, T2, T3, T4}"/> class.
        /// </summary>
        /// <param name="item1Codec">The <typeparamref name="T1"/> codec.</param>
        /// <param name="item2Codec">The <typeparamref name="T2"/> codec.</param>
        /// <param name="item3Codec">The <typeparamref name="T3"/> codec.</param>
        /// <param name="item4Codec">The <typeparamref name="T4"/> codec.</param>
        public ValueTupleCodec(
            IFieldCodec<T1> item1Codec,
            IFieldCodec<T2> item2Codec,
            IFieldCodec<T3> item3Codec,
            IFieldCodec<T4> item4Codec)
        {
            _item1Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item1Codec);
            _item2Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item2Codec);
            _item3Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item3Codec);
            _item4Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item4Codec);
        }

        /// <inheritdoc />
        public void WriteField<TBufferWriter>(
            ref Writer<TBufferWriter> writer,
            uint fieldIdDelta,
            Type expectedType,
            (T1, T2, T3, T4) value) where TBufferWriter : IBufferWriter<byte>
        {
            ReferenceCodec.MarkValueField(writer.Session);
            writer.WriteFieldHeader(fieldIdDelta, expectedType, value.GetType(), WireType.TagDelimited);

            _item1Codec.WriteField(ref writer, 1, ElementType1, value.Item1);
            _item2Codec.WriteField(ref writer, 1, ElementType2, value.Item2);
            _item3Codec.WriteField(ref writer, 1, ElementType3, value.Item3);
            _item4Codec.WriteField(ref writer, 1, ElementType4, value.Item4);

            writer.WriteEndObject();
        }

        /// <inheritdoc />
        public (T1, T2, T3, T4) ReadValue<TInput>(ref Reader<TInput> reader, Field field)
        {
            field.EnsureWireTypeTagDelimited();
            ReferenceCodec.MarkValueField(reader.Session);
            (T1, T2, T3, T4) res = default;
            uint fieldId = 0;
            while (true)
            {
                var header = reader.ReadFieldHeader();
                if (header.IsEndBaseOrEndObject)
                {
                    break;
                }

                fieldId += header.FieldIdDelta;
                switch (fieldId)
                {
                    case 1: res.Item1 = _item1Codec.ReadValue(ref reader, header); break;
                    case 2: res.Item2 = _item2Codec.ReadValue(ref reader, header); break;
                    case 3: res.Item3 = _item3Codec.ReadValue(ref reader, header); break;
                    case 4: res.Item4 = _item4Codec.ReadValue(ref reader, header); break;
                    default:
                        reader.ConsumeUnknownField(header);
                        break;
                }
            }

            return res;
        }
    }

    /// <summary>
    /// Copier for <see cref="ValueTuple{T1, T2, T3, T4}"/>.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    /// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
    [RegisterCopier]
    public sealed class ValueTupleCopier<T1, T2, T3, T4> : IDeepCopier<ValueTuple<T1, T2, T3, T4>>, IOptionalDeepCopier
    {
        private readonly IDeepCopier<T1> _copier1;
        private readonly IDeepCopier<T2> _copier2;
        private readonly IDeepCopier<T3> _copier3;
        private readonly IDeepCopier<T4> _copier4;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTupleCopier{T1, T2, T3, T4}"/> class.
        /// </summary>
        /// <param name="copier1">The <typeparamref name="T1"/> copier.</param>
        /// <param name="copier2">The <typeparamref name="T2"/> copier.</param>
        /// <param name="copier3">The <typeparamref name="T3"/> copier.</param>
        /// <param name="copier4">The <typeparamref name="T4"/> copier.</param>
        public ValueTupleCopier(
            IDeepCopier<T1> copier1,
            IDeepCopier<T2> copier2,
            IDeepCopier<T3> copier3,
            IDeepCopier<T4> copier4)
        {
            _copier1 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier1);
            _copier2 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier2);
            _copier3 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier3);
            _copier4 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier4);
        }

        public bool IsShallowCopyable() => _copier1 is null && _copier2 is null && _copier3 is null && _copier4 is null;

        object IDeepCopier.DeepCopy(object input, CopyContext context) => IsShallowCopyable() ? input : DeepCopy(((T1, T2, T3, T4))input, context);

        /// <inheritdoc />
        public ValueTuple<T1, T2, T3, T4> DeepCopy(ValueTuple<T1, T2, T3, T4> input, CopyContext context)
        {
            if (_copier1 != null) input.Item1 = _copier1.DeepCopy(input.Item1, context);
            if (_copier2 != null) input.Item2 = _copier2.DeepCopy(input.Item2, context);
            if (_copier3 != null) input.Item3 = _copier3.DeepCopy(input.Item3, context);
            if (_copier4 != null) input.Item4 = _copier4.DeepCopy(input.Item4, context);
            return input;
        }
    }

    /// <summary>
    /// Serializer for <see cref="ValueTuple{T1, T2, T3, T4, T5}"/>.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    /// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
    /// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
    [RegisterSerializer]
    public sealed class ValueTupleCodec<T1, T2, T3, T4, T5> : IFieldCodec<ValueTuple<T1, T2, T3, T4, T5>>
    {
        private readonly Type ElementType1 = typeof(T1);
        private readonly Type ElementType2 = typeof(T2);
        private readonly Type ElementType3 = typeof(T3);
        private readonly Type ElementType4 = typeof(T4);
        private readonly Type ElementType5 = typeof(T5);

        private readonly IFieldCodec<T1> _item1Codec;
        private readonly IFieldCodec<T2> _item2Codec;
        private readonly IFieldCodec<T3> _item3Codec;
        private readonly IFieldCodec<T4> _item4Codec;
        private readonly IFieldCodec<T5> _item5Codec;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTupleCodec{T1, T2, T3, T4, T5}"/> class.
        /// </summary>
        /// <param name="item1Codec">The <typeparamref name="T1"/> codec.</param>
        /// <param name="item2Codec">The <typeparamref name="T2"/> codec.</param>
        /// <param name="item3Codec">The <typeparamref name="T3"/> codec.</param>
        /// <param name="item4Codec">The <typeparamref name="T4"/> codec.</param>
        /// <param name="item5Codec">The <typeparamref name="T5"/> codec.</param>
        public ValueTupleCodec(
            IFieldCodec<T1> item1Codec,
            IFieldCodec<T2> item2Codec,
            IFieldCodec<T3> item3Codec,
            IFieldCodec<T4> item4Codec,
            IFieldCodec<T5> item5Codec)
        {
            _item1Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item1Codec);
            _item2Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item2Codec);
            _item3Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item3Codec);
            _item4Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item4Codec);
            _item5Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item5Codec);
        }

        /// <inheritdoc />
        public void WriteField<TBufferWriter>(ref Writer<TBufferWriter> writer,
            uint fieldIdDelta,
            Type expectedType,
            (T1, T2, T3, T4, T5) value) where TBufferWriter : IBufferWriter<byte>
        {
            ReferenceCodec.MarkValueField(writer.Session);
            writer.WriteFieldHeader(fieldIdDelta, expectedType, value.GetType(), WireType.TagDelimited);

            _item1Codec.WriteField(ref writer, 1, ElementType1, value.Item1);
            _item2Codec.WriteField(ref writer, 1, ElementType2, value.Item2);
            _item3Codec.WriteField(ref writer, 1, ElementType3, value.Item3);
            _item4Codec.WriteField(ref writer, 1, ElementType4, value.Item4);
            _item5Codec.WriteField(ref writer, 1, ElementType5, value.Item5);

            writer.WriteEndObject();
        }

        /// <inheritdoc />
        public (T1, T2, T3, T4, T5) ReadValue<TInput>(ref Reader<TInput> reader, Field field)
        {
            field.EnsureWireTypeTagDelimited();
            ReferenceCodec.MarkValueField(reader.Session);
            (T1, T2, T3, T4, T5) res = default;
            uint fieldId = 0;
            while (true)
            {
                var header = reader.ReadFieldHeader();
                if (header.IsEndBaseOrEndObject)
                {
                    break;
                }

                fieldId += header.FieldIdDelta;
                switch (fieldId)
                {
                    case 1: res.Item1 = _item1Codec.ReadValue(ref reader, header); break;
                    case 2: res.Item2 = _item2Codec.ReadValue(ref reader, header); break;
                    case 3: res.Item3 = _item3Codec.ReadValue(ref reader, header); break;
                    case 4: res.Item4 = _item4Codec.ReadValue(ref reader, header); break;
                    case 5: res.Item5 = _item5Codec.ReadValue(ref reader, header); break;
                    default:
                        reader.ConsumeUnknownField(header);
                        break;
                }
            }

            return res;
        }
    }

    /// <summary>
    /// Copier for <see cref="ValueTuple{T1, T2, T3, T4, T5}"/>.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    /// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
    /// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
    [RegisterCopier]
    public sealed class ValueTupleCopier<T1, T2, T3, T4, T5> : IDeepCopier<ValueTuple<T1, T2, T3, T4, T5>>, IOptionalDeepCopier
    {
        private readonly IDeepCopier<T1> _copier1;
        private readonly IDeepCopier<T2> _copier2;
        private readonly IDeepCopier<T3> _copier3;
        private readonly IDeepCopier<T4> _copier4;
        private readonly IDeepCopier<T5> _copier5;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTupleCopier{T1, T2, T3, T4, T5}"/> class.
        /// </summary>
        /// <param name="copier1">The <typeparamref name="T1"/> copier.</param>
        /// <param name="copier2">The <typeparamref name="T2"/> copier.</param>
        /// <param name="copier3">The <typeparamref name="T3"/> copier.</param>
        /// <param name="copier4">The <typeparamref name="T4"/> copier.</param>
        /// <param name="copier5">The <typeparamref name="T5"/> copier.</param>
        public ValueTupleCopier(
            IDeepCopier<T1> copier1,
            IDeepCopier<T2> copier2,
            IDeepCopier<T3> copier3,
            IDeepCopier<T4> copier4,
            IDeepCopier<T5> copier5)
        {
            _copier1 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier1);
            _copier2 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier2);
            _copier3 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier3);
            _copier4 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier4);
            _copier5 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier5);
        }

        public bool IsShallowCopyable() => _copier1 is null && _copier2 is null && _copier3 is null && _copier4 is null && _copier5 is null;

        object IDeepCopier.DeepCopy(object input, CopyContext context) => IsShallowCopyable() ? input : DeepCopy(((T1, T2, T3, T4, T5))input, context);

        /// <inheritdoc />
        public ValueTuple<T1, T2, T3, T4, T5> DeepCopy(ValueTuple<T1, T2, T3, T4, T5> input, CopyContext context)
        {
            if (_copier1 != null) input.Item1 = _copier1.DeepCopy(input.Item1, context);
            if (_copier2 != null) input.Item2 = _copier2.DeepCopy(input.Item2, context);
            if (_copier3 != null) input.Item3 = _copier3.DeepCopy(input.Item3, context);
            if (_copier4 != null) input.Item4 = _copier4.DeepCopy(input.Item4, context);
            if (_copier5 != null) input.Item5 = _copier5.DeepCopy(input.Item5, context);
            return input;
        }
    } 

    /// <summary>
    /// Serializer for <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/>.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    /// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
    /// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
    /// <typeparam name="T6">The type of the tuple's sixth component.</typeparam>
    [RegisterSerializer]
    public sealed class ValueTupleCodec<T1, T2, T3, T4, T5, T6> : IFieldCodec<ValueTuple<T1, T2, T3, T4, T5, T6>>
    {
        private readonly Type ElementType1 = typeof(T1);
        private readonly Type ElementType2 = typeof(T2);
        private readonly Type ElementType3 = typeof(T3);
        private readonly Type ElementType4 = typeof(T4);
        private readonly Type ElementType5 = typeof(T5);
        private readonly Type ElementType6 = typeof(T6);

        private readonly IFieldCodec<T1> _item1Codec;
        private readonly IFieldCodec<T2> _item2Codec;
        private readonly IFieldCodec<T3> _item3Codec;
        private readonly IFieldCodec<T4> _item4Codec;
        private readonly IFieldCodec<T5> _item5Codec;
        private readonly IFieldCodec<T6> _item6Codec;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTupleCodec{T1, T2, T3, T4, T5, T6}"/> class.
        /// </summary>
        /// <param name="item1Codec">The <typeparamref name="T1"/> codec.</param>
        /// <param name="item2Codec">The <typeparamref name="T2"/> codec.</param>
        /// <param name="item3Codec">The <typeparamref name="T3"/> codec.</param>
        /// <param name="item4Codec">The <typeparamref name="T4"/> codec.</param>
        /// <param name="item5Codec">The <typeparamref name="T5"/> codec.</param>
        /// <param name="item6Codec">The <typeparamref name="T6"/> codec.</param>
        public ValueTupleCodec(
            IFieldCodec<T1> item1Codec,
            IFieldCodec<T2> item2Codec,
            IFieldCodec<T3> item3Codec,
            IFieldCodec<T4> item4Codec,
            IFieldCodec<T5> item5Codec,
            IFieldCodec<T6> item6Codec)
        {
            _item1Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item1Codec);
            _item2Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item2Codec);
            _item3Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item3Codec);
            _item4Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item4Codec);
            _item5Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item5Codec);
            _item6Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item6Codec);
        }

        /// <inheritdoc />
        public void WriteField<TBufferWriter>(ref Writer<TBufferWriter> writer,
            uint fieldIdDelta,
            Type expectedType,
            (T1, T2, T3, T4, T5, T6) value) where TBufferWriter : IBufferWriter<byte>
        {
            ReferenceCodec.MarkValueField(writer.Session);
            writer.WriteFieldHeader(fieldIdDelta, expectedType, value.GetType(), WireType.TagDelimited);

            _item1Codec.WriteField(ref writer, 1, ElementType1, value.Item1);
            _item2Codec.WriteField(ref writer, 1, ElementType2, value.Item2);
            _item3Codec.WriteField(ref writer, 1, ElementType3, value.Item3);
            _item4Codec.WriteField(ref writer, 1, ElementType4, value.Item4);
            _item5Codec.WriteField(ref writer, 1, ElementType5, value.Item5);
            _item6Codec.WriteField(ref writer, 1, ElementType6, value.Item6);


            writer.WriteEndObject();
        }

        /// <inheritdoc />
        public (T1, T2, T3, T4, T5, T6) ReadValue<TInput>(ref Reader<TInput> reader, Field field)
        {
            field.EnsureWireTypeTagDelimited();
            ReferenceCodec.MarkValueField(reader.Session);
            (T1, T2, T3, T4, T5, T6) res = default;
            uint fieldId = 0;
            while (true)
            {
                var header = reader.ReadFieldHeader();
                if (header.IsEndBaseOrEndObject)
                {
                    break;
                }

                fieldId += header.FieldIdDelta;
                switch (fieldId)
                {
                    case 1: res.Item1 = _item1Codec.ReadValue(ref reader, header); break;
                    case 2: res.Item2 = _item2Codec.ReadValue(ref reader, header); break;
                    case 3: res.Item3 = _item3Codec.ReadValue(ref reader, header); break;
                    case 4: res.Item4 = _item4Codec.ReadValue(ref reader, header); break;
                    case 5: res.Item5 = _item5Codec.ReadValue(ref reader, header); break;
                    case 6: res.Item6 = _item6Codec.ReadValue(ref reader, header); break;
                    default:
                        reader.ConsumeUnknownField(header);
                        break;
                }
            }

            return res;
        }
    }
    
    /// <summary>
    /// Copier for <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/>.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    /// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
    /// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
    /// <typeparam name="T6">The type of the tuple's sixth component.</typeparam>
    [RegisterCopier]
    public sealed class ValueTupleCopier<T1, T2, T3, T4, T5, T6> : IDeepCopier<ValueTuple<T1, T2, T3, T4, T5, T6>>, IOptionalDeepCopier
    {
        private readonly IDeepCopier<T1> _copier1;
        private readonly IDeepCopier<T2> _copier2;
        private readonly IDeepCopier<T3> _copier3;
        private readonly IDeepCopier<T4> _copier4;
        private readonly IDeepCopier<T5> _copier5;
        private readonly IDeepCopier<T6> _copier6;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTupleCopier{T1, T2, T3, T4, T5, T6}"/> class.
        /// </summary>
        /// <param name="copier1">The <typeparamref name="T1"/> copier.</param>
        /// <param name="copier2">The <typeparamref name="T2"/> copier.</param>
        /// <param name="copier3">The <typeparamref name="T3"/> copier.</param>
        /// <param name="copier4">The <typeparamref name="T4"/> copier.</param>
        /// <param name="copier5">The <typeparamref name="T5"/> copier.</param>
        /// <param name="copier6">The <typeparamref name="T6"/> copier.</param>
        public ValueTupleCopier(
            IDeepCopier<T1> copier1,
            IDeepCopier<T2> copier2,
            IDeepCopier<T3> copier3,
            IDeepCopier<T4> copier4,
            IDeepCopier<T5> copier5,
            IDeepCopier<T6> copier6)
        {
            _copier1 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier1);
            _copier2 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier2);
            _copier3 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier3);
            _copier4 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier4);
            _copier5 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier5);
            _copier6 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier6);
        }

        public bool IsShallowCopyable() => _copier1 is null && _copier2 is null && _copier3 is null && _copier4 is null && _copier5 is null && _copier6 is null;

        object IDeepCopier.DeepCopy(object input, CopyContext context) => IsShallowCopyable() ? input : DeepCopy(((T1, T2, T3, T4, T5, T6))input, context);

        /// <inheritdoc />
        public ValueTuple<T1, T2, T3, T4, T5, T6> DeepCopy(ValueTuple<T1, T2, T3, T4, T5, T6> input, CopyContext context)
        {
            if (_copier1 != null) input.Item1 = _copier1.DeepCopy(input.Item1, context);
            if (_copier2 != null) input.Item2 = _copier2.DeepCopy(input.Item2, context);
            if (_copier3 != null) input.Item3 = _copier3.DeepCopy(input.Item3, context);
            if (_copier4 != null) input.Item4 = _copier4.DeepCopy(input.Item4, context);
            if (_copier5 != null) input.Item5 = _copier5.DeepCopy(input.Item5, context);
            if (_copier6 != null) input.Item6 = _copier6.DeepCopy(input.Item6, context);
            return input;
        }
    } 

    /// <summary>
    /// Serializer for <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/>.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    /// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
    /// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
    /// <typeparam name="T6">The type of the tuple's sixth component.</typeparam>
    /// <typeparam name="T7">The type of the tuple's seventh component.</typeparam>
    [RegisterSerializer]
    public sealed class ValueTupleCodec<T1, T2, T3, T4, T5, T6, T7> : IFieldCodec<ValueTuple<T1, T2, T3, T4, T5, T6, T7>>
    {
        private readonly Type ElementType1 = typeof(T1);
        private readonly Type ElementType2 = typeof(T2);
        private readonly Type ElementType3 = typeof(T3);
        private readonly Type ElementType4 = typeof(T4);
        private readonly Type ElementType5 = typeof(T5);
        private readonly Type ElementType6 = typeof(T6);
        private readonly Type ElementType7 = typeof(T7);

        private readonly IFieldCodec<T1> _item1Codec;
        private readonly IFieldCodec<T2> _item2Codec;
        private readonly IFieldCodec<T3> _item3Codec;
        private readonly IFieldCodec<T4> _item4Codec;
        private readonly IFieldCodec<T5> _item5Codec;
        private readonly IFieldCodec<T6> _item6Codec;
        private readonly IFieldCodec<T7> _item7Codec;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTupleCodec{T1, T2, T3, T4, T5, T6, T7}"/> class.
        /// </summary>
        /// <param name="item1Codec">The <typeparamref name="T1"/> codec.</param>
        /// <param name="item2Codec">The <typeparamref name="T2"/> codec.</param>
        /// <param name="item3Codec">The <typeparamref name="T3"/> codec.</param>
        /// <param name="item4Codec">The <typeparamref name="T4"/> codec.</param>
        /// <param name="item5Codec">The <typeparamref name="T5"/> codec.</param>
        /// <param name="item6Codec">The <typeparamref name="T6"/> codec.</param>
        /// <param name="item7Codec">The <typeparamref name="T7"/> codec.</param>
        public ValueTupleCodec(
            IFieldCodec<T1> item1Codec,
            IFieldCodec<T2> item2Codec,
            IFieldCodec<T3> item3Codec,
            IFieldCodec<T4> item4Codec,
            IFieldCodec<T5> item5Codec,
            IFieldCodec<T6> item6Codec,
            IFieldCodec<T7> item7Codec)
        {
            _item1Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item1Codec);
            _item2Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item2Codec);
            _item3Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item3Codec);
            _item4Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item4Codec);
            _item5Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item5Codec);
            _item6Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item6Codec);
            _item7Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item7Codec);
        }

        /// <inheritdoc />
        public void WriteField<TBufferWriter>(ref Writer<TBufferWriter> writer,
            uint fieldIdDelta,
            Type expectedType,
            (T1, T2, T3, T4, T5, T6, T7) value) where TBufferWriter : IBufferWriter<byte>
        {
            ReferenceCodec.MarkValueField(writer.Session);
            writer.WriteFieldHeader(fieldIdDelta, expectedType, value.GetType(), WireType.TagDelimited);

            _item1Codec.WriteField(ref writer, 1, ElementType1, value.Item1);
            _item2Codec.WriteField(ref writer, 1, ElementType2, value.Item2);
            _item3Codec.WriteField(ref writer, 1, ElementType3, value.Item3);
            _item4Codec.WriteField(ref writer, 1, ElementType4, value.Item4);
            _item5Codec.WriteField(ref writer, 1, ElementType5, value.Item5);
            _item6Codec.WriteField(ref writer, 1, ElementType6, value.Item6);
            _item7Codec.WriteField(ref writer, 1, ElementType7, value.Item7);


            writer.WriteEndObject();
        }

        /// <inheritdoc />
        public (T1, T2, T3, T4, T5, T6, T7) ReadValue<TInput>(
            ref Reader<TInput> reader,
            Field field)
        {
            field.EnsureWireTypeTagDelimited();
            ReferenceCodec.MarkValueField(reader.Session);
            (T1, T2, T3, T4, T5, T6, T7) res = default;
            uint fieldId = 0;
            while (true)
            {
                var header = reader.ReadFieldHeader();
                if (header.IsEndBaseOrEndObject)
                {
                    break;
                }

                fieldId += header.FieldIdDelta;
                switch (fieldId)
                {
                    case 1: res.Item1 = _item1Codec.ReadValue(ref reader, header); break;
                    case 2: res.Item2 = _item2Codec.ReadValue(ref reader, header); break;
                    case 3: res.Item3 = _item3Codec.ReadValue(ref reader, header); break;
                    case 4: res.Item4 = _item4Codec.ReadValue(ref reader, header); break;
                    case 5: res.Item5 = _item5Codec.ReadValue(ref reader, header); break;
                    case 6: res.Item6 = _item6Codec.ReadValue(ref reader, header); break;
                    case 7: res.Item7 = _item7Codec.ReadValue(ref reader, header); break;
                    default:
                        reader.ConsumeUnknownField(header);
                        break;
                }
            }

            return res;
        }
    }

    /// <summary>
    /// Copier for <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/>.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    /// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
    /// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
    /// <typeparam name="T6">The type of the tuple's sixth component.</typeparam>
    /// <typeparam name="T7">The type of the tuple's seventh component.</typeparam>
    [RegisterCopier]
    public sealed class ValueTupleCopier<T1, T2, T3, T4, T5, T6, T7> : IDeepCopier<ValueTuple<T1, T2, T3, T4, T5, T6, T7>>, IOptionalDeepCopier
    {
        private readonly IDeepCopier<T1> _copier1;
        private readonly IDeepCopier<T2> _copier2;
        private readonly IDeepCopier<T3> _copier3;
        private readonly IDeepCopier<T4> _copier4;
        private readonly IDeepCopier<T5> _copier5;
        private readonly IDeepCopier<T6> _copier6;
        private readonly IDeepCopier<T7> _copier7;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTupleCopier{T1, T2, T3, T4, T5, T6, T7}"/> class.
        /// </summary>
        /// <param name="copier1">The <typeparamref name="T1"/> copier.</param>
        /// <param name="copier2">The <typeparamref name="T2"/> copier.</param>
        /// <param name="copier3">The <typeparamref name="T3"/> copier.</param>
        /// <param name="copier4">The <typeparamref name="T4"/> copier.</param>
        /// <param name="copier5">The <typeparamref name="T5"/> copier.</param>
        /// <param name="copier6">The <typeparamref name="T6"/> copier.</param>
        /// <param name="copier7">The <typeparamref name="T7"/> copier.</param>
        public ValueTupleCopier(
            IDeepCopier<T1> copier1,
            IDeepCopier<T2> copier2,
            IDeepCopier<T3> copier3,
            IDeepCopier<T4> copier4,
            IDeepCopier<T5> copier5,
            IDeepCopier<T6> copier6,
            IDeepCopier<T7> copier7)
        {
            _copier1 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier1);
            _copier2 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier2);
            _copier3 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier3);
            _copier4 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier4);
            _copier5 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier5);
            _copier6 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier6);
            _copier7 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier7);
        }

        public bool IsShallowCopyable() => _copier1 is null && _copier2 is null && _copier3 is null && _copier4 is null && _copier5 is null && _copier6 is null && _copier7 is null;

        object IDeepCopier.DeepCopy(object input, CopyContext context) => IsShallowCopyable() ? input : DeepCopy(((T1, T2, T3, T4, T5, T6, T7))input, context);

        /// <inheritdoc />
        public ValueTuple<T1, T2, T3, T4, T5, T6, T7> DeepCopy(ValueTuple<T1, T2, T3, T4, T5, T6, T7> input, CopyContext context)
        {
            if (_copier1 != null) input.Item1 = _copier1.DeepCopy(input.Item1, context);
            if (_copier2 != null) input.Item2 = _copier2.DeepCopy(input.Item2, context);
            if (_copier3 != null) input.Item3 = _copier3.DeepCopy(input.Item3, context);
            if (_copier4 != null) input.Item4 = _copier4.DeepCopy(input.Item4, context);
            if (_copier5 != null) input.Item5 = _copier5.DeepCopy(input.Item5, context);
            if (_copier6 != null) input.Item6 = _copier6.DeepCopy(input.Item6, context);
            if (_copier7 != null) input.Item7 = _copier7.DeepCopy(input.Item7, context);
            return input;
        }
    } 

    /// <summary>
    /// Serializer for <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, T8}"/>.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    /// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
    /// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
    /// <typeparam name="T6">The type of the tuple's sixth component.</typeparam>
    /// <typeparam name="T7">The type of the tuple's seventh component.</typeparam>
    /// <typeparam name="T8">The type of the tuple's eighth component.</typeparam>
    [RegisterSerializer]
    public sealed class ValueTupleCodec<T1, T2, T3, T4, T5, T6, T7, T8> : IFieldCodec<ValueTuple<T1, T2, T3, T4, T5, T6, T7, T8>> where T8 : struct
    {
        private readonly Type ElementType1 = typeof(T1);
        private readonly Type ElementType2 = typeof(T2);
        private readonly Type ElementType3 = typeof(T3);
        private readonly Type ElementType4 = typeof(T4);
        private readonly Type ElementType5 = typeof(T5);
        private readonly Type ElementType6 = typeof(T6);
        private readonly Type ElementType7 = typeof(T7);
        private readonly Type ElementType8 = typeof(T8);

        private readonly IFieldCodec<T1> _item1Codec;
        private readonly IFieldCodec<T2> _item2Codec;
        private readonly IFieldCodec<T3> _item3Codec;
        private readonly IFieldCodec<T4> _item4Codec;
        private readonly IFieldCodec<T5> _item5Codec;
        private readonly IFieldCodec<T6> _item6Codec;
        private readonly IFieldCodec<T7> _item7Codec;
        private readonly IFieldCodec<T8> _item8Codec;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTupleCodec{T1, T2, T3, T4, T5, T6, T7, T8}"/> class.
        /// </summary>
        /// <param name="item1Codec">The <typeparamref name="T1"/> codec.</param>
        /// <param name="item2Codec">The <typeparamref name="T2"/> codec.</param>
        /// <param name="item3Codec">The <typeparamref name="T3"/> codec.</param>
        /// <param name="item4Codec">The <typeparamref name="T4"/> codec.</param>
        /// <param name="item5Codec">The <typeparamref name="T5"/> codec.</param>
        /// <param name="item6Codec">The <typeparamref name="T6"/> codec.</param>
        /// <param name="item7Codec">The <typeparamref name="T7"/> codec.</param>
        /// <param name="item8Codec">The <typeparamref name="T8"/> codec.</param>
        public ValueTupleCodec(
            IFieldCodec<T1> item1Codec,
            IFieldCodec<T2> item2Codec,
            IFieldCodec<T3> item3Codec,
            IFieldCodec<T4> item4Codec,
            IFieldCodec<T5> item5Codec,
            IFieldCodec<T6> item6Codec,
            IFieldCodec<T7> item7Codec,
            IFieldCodec<T8> item8Codec)
        {
            _item1Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item1Codec);
            _item2Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item2Codec);
            _item3Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item3Codec);
            _item4Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item4Codec);
            _item5Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item5Codec);
            _item6Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item6Codec);
            _item7Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item7Codec);
            _item8Codec = ForkleansGeneratedCodeHelper.UnwrapService(this, item8Codec);
        }

        /// <inheritdoc />
        public void WriteField<TBufferWriter>(
            ref Writer<TBufferWriter> writer,
            uint fieldIdDelta,
            Type expectedType,
            ValueTuple<T1, T2, T3, T4, T5, T6, T7, T8> value) where TBufferWriter : IBufferWriter<byte>
        {
            ReferenceCodec.MarkValueField(writer.Session);
            writer.WriteFieldHeader(fieldIdDelta, expectedType, value.GetType(), WireType.TagDelimited);

            _item1Codec.WriteField(ref writer, 1, ElementType1, value.Item1);
            _item2Codec.WriteField(ref writer, 1, ElementType2, value.Item2);
            _item3Codec.WriteField(ref writer, 1, ElementType3, value.Item3);
            _item4Codec.WriteField(ref writer, 1, ElementType4, value.Item4);
            _item5Codec.WriteField(ref writer, 1, ElementType5, value.Item5);
            _item6Codec.WriteField(ref writer, 1, ElementType6, value.Item6);
            _item7Codec.WriteField(ref writer, 1, ElementType7, value.Item7);
            _item8Codec.WriteField(ref writer, 1, ElementType8, value.Rest);

            writer.WriteEndObject();
        }

        /// <inheritdoc />
        public ValueTuple<T1, T2, T3, T4, T5, T6, T7, T8> ReadValue<TInput>(ref Reader<TInput> reader,
            Field field)
        {
            field.EnsureWireTypeTagDelimited();
            ReferenceCodec.MarkValueField(reader.Session);
            ValueTuple<T1, T2, T3, T4, T5, T6, T7, T8> res = default;
            uint fieldId = 0;
            while (true)
            {
                var header = reader.ReadFieldHeader();
                if (header.IsEndBaseOrEndObject)
                {
                    break;
                }

                fieldId += header.FieldIdDelta;
                switch (fieldId)
                {
                    case 1: res.Item1 = _item1Codec.ReadValue(ref reader, header); break;
                    case 2: res.Item2 = _item2Codec.ReadValue(ref reader, header); break;
                    case 3: res.Item3 = _item3Codec.ReadValue(ref reader, header); break;
                    case 4: res.Item4 = _item4Codec.ReadValue(ref reader, header); break;
                    case 5: res.Item5 = _item5Codec.ReadValue(ref reader, header); break;
                    case 6: res.Item6 = _item6Codec.ReadValue(ref reader, header); break;
                    case 7: res.Item7 = _item7Codec.ReadValue(ref reader, header); break;
                    case 8: res.Rest = _item8Codec.ReadValue(ref reader, header); break;
                    default:
                        reader.ConsumeUnknownField(header);
                        break;
                }
            }

            return res;
        }
    }

    /// <summary>
    /// Copier for <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, T8}"/>.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    /// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
    /// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
    /// <typeparam name="T6">The type of the tuple's sixth component.</typeparam>
    /// <typeparam name="T7">The type of the tuple's seventh component.</typeparam>
    /// <typeparam name="T8">The type of the tuple's eighth component.</typeparam>
    [RegisterCopier]
    public sealed class ValueTupleCopier<T1, T2, T3, T4, T5, T6, T7, T8> : IDeepCopier<ValueTuple<T1, T2, T3, T4, T5, T6, T7, T8>>, IOptionalDeepCopier where T8 : struct
    {
        private readonly IDeepCopier<T1> _copier1;
        private readonly IDeepCopier<T2> _copier2;
        private readonly IDeepCopier<T3> _copier3;
        private readonly IDeepCopier<T4> _copier4;
        private readonly IDeepCopier<T5> _copier5;
        private readonly IDeepCopier<T6> _copier6;
        private readonly IDeepCopier<T7> _copier7;
        private readonly IDeepCopier<T8> _copier8;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTupleCopier{T1, T2, T3, T4, T5, T6, T7, T8}"/> class.
        /// </summary>
        /// <param name="copier1">The <typeparamref name="T1"/> copier.</param>
        /// <param name="copier2">The <typeparamref name="T2"/> copier.</param>
        /// <param name="copier3">The <typeparamref name="T3"/> copier.</param>
        /// <param name="copier4">The <typeparamref name="T4"/> copier.</param>
        /// <param name="copier5">The <typeparamref name="T5"/> copier.</param>
        /// <param name="copier6">The <typeparamref name="T6"/> copier.</param>
        /// <param name="copier7">The <typeparamref name="T7"/> copier.</param>
        /// <param name="copier8">The <typeparamref name="T8"/> copier.</param>
        public ValueTupleCopier(
            IDeepCopier<T1> copier1,
            IDeepCopier<T2> copier2,
            IDeepCopier<T3> copier3,
            IDeepCopier<T4> copier4,
            IDeepCopier<T5> copier5,
            IDeepCopier<T6> copier6,
            IDeepCopier<T7> copier7,
            IDeepCopier<T8> copier8)
        {
            _copier1 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier1);
            _copier2 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier2);
            _copier3 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier3);
            _copier4 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier4);
            _copier5 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier5);
            _copier6 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier6);
            _copier7 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier7);
            _copier8 = ForkleansGeneratedCodeHelper.GetOptionalCopier(copier8);
        }

        public bool IsShallowCopyable() => _copier1 is null && _copier2 is null && _copier3 is null && _copier4 is null && _copier5 is null && _copier6 is null && _copier7 is null && _copier8 is null;

        object IDeepCopier.DeepCopy(object input, CopyContext context) => IsShallowCopyable() ? input : DeepCopy((ValueTuple<T1, T2, T3, T4, T5, T6, T7, T8>)input, context);

        /// <inheritdoc />
        public ValueTuple<T1, T2, T3, T4, T5, T6, T7, T8> DeepCopy(ValueTuple<T1, T2, T3, T4, T5, T6, T7, T8> input, CopyContext context)
        {
            if (_copier1 != null) input.Item1 = _copier1.DeepCopy(input.Item1, context);
            if (_copier2 != null) input.Item2 = _copier2.DeepCopy(input.Item2, context);
            if (_copier3 != null) input.Item3 = _copier3.DeepCopy(input.Item3, context);
            if (_copier4 != null) input.Item4 = _copier4.DeepCopy(input.Item4, context);
            if (_copier5 != null) input.Item5 = _copier5.DeepCopy(input.Item5, context);
            if (_copier6 != null) input.Item6 = _copier6.DeepCopy(input.Item6, context);
            if (_copier7 != null) input.Item7 = _copier7.DeepCopy(input.Item7, context);
            if (_copier8 != null) input.Rest = _copier8.DeepCopy(input.Rest, context);
            return input;
        }
    } 
}
