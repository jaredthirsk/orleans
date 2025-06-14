#pragma warning disable CS1591, RS0016, RS0041
[assembly: global::Forkleans.ApplicationPartAttribute("TestProject")]
[assembly: global::Forkleans.ApplicationPartAttribute("Forkleans.Core.Abstractions")]
[assembly: global::Forkleans.ApplicationPartAttribute("Forkleans.Serialization")]
[assembly: global::Forkleans.ApplicationPartAttribute("Forkleans.Core")]
[assembly: global::Forkleans.ApplicationPartAttribute("Forkleans.Runtime")]
[assembly: global::Forkleans.Serialization.Configuration.TypeManifestProviderAttribute(typeof(ForkleansCodeGen.TestProject.Metadata_TestProject))]
namespace ForkleansCodeGen.TestProject
{
    using global::Forkleans.Serialization.Codecs;
    using global::Forkleans.Serialization.GeneratedCodeHelpers;

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ForkleansCodeGen", "9.0.0.0"), global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    public sealed class Codec_DemoData : global::Forkleans.Serialization.Codecs.IFieldCodec<global::TestProject.DemoData>, global::Forkleans.Serialization.Serializers.IBaseCodec<global::TestProject.DemoData>
    {
        private readonly global::System.Type _codecFieldType = typeof(global::TestProject.DemoData);
        private readonly global::System.Type _type0 = typeof(int[]);
        private readonly global::Forkleans.Serialization.Codecs.ArrayCodec<int> _codec0;
        public Codec_DemoData(global::Forkleans.Serialization.Serializers.ICodecProvider codecProvider)
        {
            _codec0 = ForkleansGeneratedCodeHelper.GetService<global::Forkleans.Serialization.Codecs.ArrayCodec<int>>(this, codecProvider);
        }

        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Serialize<TBufferWriter>(ref global::Forkleans.Serialization.Buffers.Writer<TBufferWriter> writer, global::TestProject.DemoData instance)
            where TBufferWriter : global::System.Buffers.IBufferWriter<byte>
        {
            global::Forkleans.Serialization.Codecs.Int32Codec.WriteField(ref writer, 0U, instance.IntProp);
            global::Forkleans.Serialization.Codecs.DoubleCodec.WriteField(ref writer, 1U, instance.DoubleProp);
            global::Forkleans.Serialization.Codecs.FloatCodec.WriteField(ref writer, 1U, instance.FloatProp);
            global::Forkleans.Serialization.Codecs.Int64Codec.WriteField(ref writer, 1U, instance.LongProp);
            global::Forkleans.Serialization.Codecs.BoolCodec.WriteField(ref writer, 1U, instance.BoolProp);
            global::Forkleans.Serialization.Codecs.ByteCodec.WriteField(ref writer, 1U, instance.ByteProp);
            global::Forkleans.Serialization.Codecs.Int16Codec.WriteField(ref writer, 1U, instance.ShortProp);
            global::Forkleans.Serialization.Codecs.CharCodec.WriteField(ref writer, 1U, instance.CharProp);
            global::Forkleans.Serialization.Codecs.UInt32Codec.WriteField(ref writer, 1U, instance.UIntProp);
            global::Forkleans.Serialization.Codecs.UInt64Codec.WriteField(ref writer, 1U, instance.ULongProp);
            global::Forkleans.Serialization.Codecs.UInt16Codec.WriteField(ref writer, 1U, instance.UShortProp);
            global::Forkleans.Serialization.Codecs.SByteCodec.WriteField(ref writer, 1U, instance.SByteProp);
            global::Forkleans.Serialization.Codecs.DecimalCodec.WriteField(ref writer, 1U, instance.DecimalProp);
            global::Forkleans.Serialization.Codecs.DateTimeCodec.WriteField(ref writer, 1U, instance.DateTimeProp);
            global::Forkleans.Serialization.Codecs.DateTimeOffsetCodec.WriteField(ref writer, 1U, instance.DateTimeOffsetProp);
            global::Forkleans.Serialization.Codecs.TimeSpanCodec.WriteField(ref writer, 1U, instance.TimeSpanProp);
            global::Forkleans.Serialization.Codecs.GuidCodec.WriteField(ref writer, 1U, instance.GuidProp);
            _codec0.WriteField(ref writer, 1U, _type0, instance.IntArrayProp);
        }

        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Deserialize<TReaderInput>(ref global::Forkleans.Serialization.Buffers.Reader<TReaderInput> reader, global::TestProject.DemoData instance)
        {
            uint id = 0U;
            global::Forkleans.Serialization.WireProtocol.Field header = default;
            while (true)
            {
                reader.ReadFieldHeader(ref header);
                if (header.IsEndBaseOrEndObject)
                    break;
                id += header.FieldIdDelta;
                if (id == 0U)
                {
                    instance.IntProp = global::Forkleans.Serialization.Codecs.Int32Codec.ReadValue(ref reader, header);
                    reader.ReadFieldHeader(ref header);
                    if (header.IsEndBaseOrEndObject)
                        break;
                    id += header.FieldIdDelta;
                }

                if (id == 1U)
                {
                    instance.DoubleProp = global::Forkleans.Serialization.Codecs.DoubleCodec.ReadValue(ref reader, header);
                    reader.ReadFieldHeader(ref header);
                    if (header.IsEndBaseOrEndObject)
                        break;
                    id += header.FieldIdDelta;
                }

                if (id == 2U)
                {
                    instance.FloatProp = global::Forkleans.Serialization.Codecs.FloatCodec.ReadValue(ref reader, header);
                    reader.ReadFieldHeader(ref header);
                    if (header.IsEndBaseOrEndObject)
                        break;
                    id += header.FieldIdDelta;
                }

                if (id == 3U)
                {
                    instance.LongProp = global::Forkleans.Serialization.Codecs.Int64Codec.ReadValue(ref reader, header);
                    reader.ReadFieldHeader(ref header);
                    if (header.IsEndBaseOrEndObject)
                        break;
                    id += header.FieldIdDelta;
                }

                if (id == 4U)
                {
                    instance.BoolProp = global::Forkleans.Serialization.Codecs.BoolCodec.ReadValue(ref reader, header);
                    reader.ReadFieldHeader(ref header);
                    if (header.IsEndBaseOrEndObject)
                        break;
                    id += header.FieldIdDelta;
                }

                if (id == 5U)
                {
                    instance.ByteProp = global::Forkleans.Serialization.Codecs.ByteCodec.ReadValue(ref reader, header);
                    reader.ReadFieldHeader(ref header);
                    if (header.IsEndBaseOrEndObject)
                        break;
                    id += header.FieldIdDelta;
                }

                if (id == 6U)
                {
                    instance.ShortProp = global::Forkleans.Serialization.Codecs.Int16Codec.ReadValue(ref reader, header);
                    reader.ReadFieldHeader(ref header);
                    if (header.IsEndBaseOrEndObject)
                        break;
                    id += header.FieldIdDelta;
                }

                if (id == 7U)
                {
                    instance.CharProp = global::Forkleans.Serialization.Codecs.CharCodec.ReadValue(ref reader, header);
                    reader.ReadFieldHeader(ref header);
                    if (header.IsEndBaseOrEndObject)
                        break;
                    id += header.FieldIdDelta;
                }

                if (id == 8U)
                {
                    instance.UIntProp = global::Forkleans.Serialization.Codecs.UInt32Codec.ReadValue(ref reader, header);
                    reader.ReadFieldHeader(ref header);
                    if (header.IsEndBaseOrEndObject)
                        break;
                    id += header.FieldIdDelta;
                }

                if (id == 9U)
                {
                    instance.ULongProp = global::Forkleans.Serialization.Codecs.UInt64Codec.ReadValue(ref reader, header);
                    reader.ReadFieldHeader(ref header);
                    if (header.IsEndBaseOrEndObject)
                        break;
                    id += header.FieldIdDelta;
                }

                if (id == 10U)
                {
                    instance.UShortProp = global::Forkleans.Serialization.Codecs.UInt16Codec.ReadValue(ref reader, header);
                    reader.ReadFieldHeader(ref header);
                    if (header.IsEndBaseOrEndObject)
                        break;
                    id += header.FieldIdDelta;
                }

                if (id == 11U)
                {
                    instance.SByteProp = global::Forkleans.Serialization.Codecs.SByteCodec.ReadValue(ref reader, header);
                    reader.ReadFieldHeader(ref header);
                    if (header.IsEndBaseOrEndObject)
                        break;
                    id += header.FieldIdDelta;
                }

                if (id == 12U)
                {
                    instance.DecimalProp = global::Forkleans.Serialization.Codecs.DecimalCodec.ReadValue(ref reader, header);
                    reader.ReadFieldHeader(ref header);
                    if (header.IsEndBaseOrEndObject)
                        break;
                    id += header.FieldIdDelta;
                }

                if (id == 13U)
                {
                    instance.DateTimeProp = global::Forkleans.Serialization.Codecs.DateTimeCodec.ReadValue(ref reader, header);
                    reader.ReadFieldHeader(ref header);
                    if (header.IsEndBaseOrEndObject)
                        break;
                    id += header.FieldIdDelta;
                }

                if (id == 14U)
                {
                    instance.DateTimeOffsetProp = global::Forkleans.Serialization.Codecs.DateTimeOffsetCodec.ReadValue(ref reader, header);
                    reader.ReadFieldHeader(ref header);
                    if (header.IsEndBaseOrEndObject)
                        break;
                    id += header.FieldIdDelta;
                }

                if (id == 15U)
                {
                    instance.TimeSpanProp = global::Forkleans.Serialization.Codecs.TimeSpanCodec.ReadValue(ref reader, header);
                    reader.ReadFieldHeader(ref header);
                    if (header.IsEndBaseOrEndObject)
                        break;
                    id += header.FieldIdDelta;
                }

                if (id == 16U)
                {
                    instance.GuidProp = global::Forkleans.Serialization.Codecs.GuidCodec.ReadValue(ref reader, header);
                    reader.ReadFieldHeader(ref header);
                    if (header.IsEndBaseOrEndObject)
                        break;
                    id += header.FieldIdDelta;
                }

                if (id == 17U)
                {
                    instance.IntArrayProp = _codec0.ReadValue(ref reader, header);
                    reader.ReadFieldHeader(ref header);
                }

                reader.ConsumeEndBaseOrEndObject(ref header);
                break;
            }
        }

        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void WriteField<TBufferWriter>(ref global::Forkleans.Serialization.Buffers.Writer<TBufferWriter> writer, uint fieldIdDelta, global::System.Type expectedType, global::TestProject.DemoData @value)
            where TBufferWriter : global::System.Buffers.IBufferWriter<byte>
        {
            if (@value is null || @value.GetType() == typeof(global::TestProject.DemoData))
            {
                if (ReferenceCodec.TryWriteReferenceField(ref writer, fieldIdDelta, expectedType, @value))
                    return;
                writer.WriteStartObject(fieldIdDelta, expectedType, _codecFieldType);
                Serialize(ref writer, @value);
                writer.WriteEndObject();
            }
            else
                writer.SerializeUnexpectedType(fieldIdDelta, expectedType, @value);
        }

        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public global::TestProject.DemoData ReadValue<TReaderInput>(ref global::Forkleans.Serialization.Buffers.Reader<TReaderInput> reader, global::Forkleans.Serialization.WireProtocol.Field field)
        {
            if (field.IsReference)
                return ReferenceCodec.ReadReference<global::TestProject.DemoData, TReaderInput>(ref reader, field);
            field.EnsureWireTypeTagDelimited();
            global::System.Type valueType = field.FieldType;
            if (valueType is null || valueType == _codecFieldType)
            {
                var result = new global::TestProject.DemoData();
                ReferenceCodec.RecordObject(reader.Session, result);
                Deserialize(ref reader, result);
                return result;
            }

            return reader.DeserializeUnexpectedType<TReaderInput, global::TestProject.DemoData>(ref field);
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ForkleansCodeGen", "9.0.0.0"), global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    public sealed class Copier_DemoData : global::Forkleans.Serialization.Cloning.IDeepCopier<global::TestProject.DemoData>, global::Forkleans.Serialization.Cloning.IBaseCopier<global::TestProject.DemoData>
    {
        private readonly global::Forkleans.Serialization.Codecs.ArrayCopier<int> _copier0;
        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public global::TestProject.DemoData DeepCopy(global::TestProject.DemoData original, global::Forkleans.Serialization.Cloning.CopyContext context)
        {
            if (context.TryGetCopy(original, out global::TestProject.DemoData existing))
                return existing;
            if (original.GetType() != typeof(global::TestProject.DemoData))
                return context.DeepCopy(original);
            var result = new global::TestProject.DemoData();
            context.RecordCopy(original, result);
            DeepCopy(original, result, context);
            return result;
        }

        public Copier_DemoData(global::Forkleans.Serialization.Serializers.ICodecProvider codecProvider)
        {
            _copier0 = ForkleansGeneratedCodeHelper.GetService<global::Forkleans.Serialization.Codecs.ArrayCopier<int>>(this, codecProvider);
        }

        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void DeepCopy(global::TestProject.DemoData input, global::TestProject.DemoData output, global::Forkleans.Serialization.Cloning.CopyContext context)
        {
            output.IntProp = input.IntProp;
            output.DoubleProp = input.DoubleProp;
            output.FloatProp = input.FloatProp;
            output.LongProp = input.LongProp;
            output.BoolProp = input.BoolProp;
            output.ByteProp = input.ByteProp;
            output.ShortProp = input.ShortProp;
            output.CharProp = input.CharProp;
            output.UIntProp = input.UIntProp;
            output.ULongProp = input.ULongProp;
            output.UShortProp = input.UShortProp;
            output.SByteProp = input.SByteProp;
            output.DecimalProp = input.DecimalProp;
            output.DateTimeProp = input.DateTimeProp;
            output.DateTimeOffsetProp = input.DateTimeOffsetProp;
            output.TimeSpanProp = input.TimeSpanProp;
            output.GuidProp = input.GuidProp;
            output.IntArrayProp = _copier0.DeepCopy(input.IntArrayProp, context);
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ForkleansCodeGen", "9.0.0.0"), global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    internal sealed class Activator_DemoData : global::Forkleans.Serialization.Activators.IActivator<global::TestProject.DemoData>
    {
        public global::TestProject.DemoData Create() => new global::TestProject.DemoData();
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ForkleansCodeGen", "9.0.0.0"), global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    internal sealed class Metadata_TestProject : global::Forkleans.Serialization.Configuration.TypeManifestProviderBase
    {
        protected override void ConfigureInner(global::Forkleans.Serialization.Configuration.TypeManifestOptions config)
        {
            config.Serializers.Add(typeof(ForkleansCodeGen.TestProject.Codec_DemoData));
            config.Copiers.Add(typeof(ForkleansCodeGen.TestProject.Copier_DemoData));
            config.Activators.Add(typeof(ForkleansCodeGen.TestProject.Activator_DemoData));
        }
    }
}
#pragma warning restore CS1591, RS0016, RS0041
