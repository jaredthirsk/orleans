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
    public sealed class Codec_BaseData : global::Forkleans.Serialization.Serializers.AbstractTypeSerializer<global::TestProject.BaseData>
    {
        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override void Serialize<TBufferWriter>(ref global::Forkleans.Serialization.Buffers.Writer<TBufferWriter> writer, global::TestProject.BaseData instance)
        {
            global::Forkleans.Serialization.Codecs.StringCodec.WriteField(ref writer, 0U, instance.BaseValue);
        }

        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override void Deserialize<TReaderInput>(ref global::Forkleans.Serialization.Buffers.Reader<TReaderInput> reader, global::TestProject.BaseData instance)
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
                    instance.BaseValue = global::Forkleans.Serialization.Codecs.StringCodec.ReadValue(ref reader, header);
                    reader.ReadFieldHeader(ref header);
                }

                reader.ConsumeEndBaseOrEndObject(ref header);
                break;
            }
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ForkleansCodeGen", "9.0.0.0"), global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    public sealed class Copier_BaseData : global::Forkleans.Serialization.Cloning.IDeepCopier<global::TestProject.BaseData>, global::Forkleans.Serialization.Cloning.IBaseCopier<global::TestProject.BaseData>
    {
        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public global::TestProject.BaseData DeepCopy(global::TestProject.BaseData original, global::Forkleans.Serialization.Cloning.CopyContext context)
        {
            return context.DeepCopy(original);
        }

        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void DeepCopy(global::TestProject.BaseData input, global::TestProject.BaseData output, global::Forkleans.Serialization.Cloning.CopyContext context)
        {
            output.BaseValue = input.BaseValue;
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ForkleansCodeGen", "9.0.0.0"), global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    public sealed class Codec_DerivedData : global::Forkleans.Serialization.Codecs.IFieldCodec<global::TestProject.DerivedData>, global::Forkleans.Serialization.Serializers.IBaseCodec<global::TestProject.DerivedData>
    {
        private readonly global::System.Type _codecFieldType = typeof(global::TestProject.DerivedData);
        private readonly ForkleansCodeGen.TestProject.Codec_BaseData _baseTypeSerializer;
        private readonly global::Forkleans.Serialization.Activators.IActivator<global::TestProject.DerivedData> _activator;
        public Codec_DerivedData(global::Forkleans.Serialization.Serializers.ICodecProvider codecProvider, global::Forkleans.Serialization.Activators.IActivator<global::TestProject.DerivedData> _activator)
        {
            _baseTypeSerializer = ForkleansGeneratedCodeHelper.GetService<ForkleansCodeGen.TestProject.Codec_BaseData>(this, codecProvider);
            this._activator = ForkleansGeneratedCodeHelper.UnwrapService(this, _activator);
        }

        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Serialize<TBufferWriter>(ref global::Forkleans.Serialization.Buffers.Writer<TBufferWriter> writer, global::TestProject.DerivedData instance)
            where TBufferWriter : global::System.Buffers.IBufferWriter<byte>
        {
            _baseTypeSerializer.Serialize(ref writer, instance);
            writer.WriteEndBase();
            global::Forkleans.Serialization.Codecs.StringCodec.WriteField(ref writer, 1U, instance.DerivedValue);
        }

        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Deserialize<TReaderInput>(ref global::Forkleans.Serialization.Buffers.Reader<TReaderInput> reader, global::TestProject.DerivedData instance)
        {
            _baseTypeSerializer.Deserialize(ref reader, instance);
            uint id = 0U;
            global::Forkleans.Serialization.WireProtocol.Field header = default;
            while (true)
            {
                reader.ReadFieldHeader(ref header);
                if (header.IsEndBaseOrEndObject)
                    break;
                id += header.FieldIdDelta;
                if (id == 1U)
                {
                    instance.DerivedValue = global::Forkleans.Serialization.Codecs.StringCodec.ReadValue(ref reader, header);
                    reader.ReadFieldHeader(ref header);
                    if (header.IsEndBaseOrEndObject)
                        break;
                    id++;
                }

                reader.ConsumeUnknownField(ref header);
            }
        }

        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void WriteField<TBufferWriter>(ref global::Forkleans.Serialization.Buffers.Writer<TBufferWriter> writer, uint fieldIdDelta, global::System.Type expectedType, global::TestProject.DerivedData @value)
            where TBufferWriter : global::System.Buffers.IBufferWriter<byte>
        {
            if (@value is null || @value.GetType() == typeof(global::TestProject.DerivedData))
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
        public global::TestProject.DerivedData ReadValue<TReaderInput>(ref global::Forkleans.Serialization.Buffers.Reader<TReaderInput> reader, global::Forkleans.Serialization.WireProtocol.Field field)
        {
            if (field.IsReference)
                return ReferenceCodec.ReadReference<global::TestProject.DerivedData, TReaderInput>(ref reader, field);
            field.EnsureWireTypeTagDelimited();
            global::System.Type valueType = field.FieldType;
            if (valueType is null || valueType == _codecFieldType)
            {
                var result = _activator.Create();
                ReferenceCodec.RecordObject(reader.Session, result);
                Deserialize(ref reader, result);
                return result;
            }

            return reader.DeserializeUnexpectedType<TReaderInput, global::TestProject.DerivedData>(ref field);
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ForkleansCodeGen", "9.0.0.0"), global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    public sealed class Copier_DerivedData : global::Forkleans.Serialization.Cloning.IDeepCopier<global::TestProject.DerivedData>, global::Forkleans.Serialization.Cloning.IBaseCopier<global::TestProject.DerivedData>
    {
        private readonly ForkleansCodeGen.TestProject.Copier_BaseData _baseTypeCopier;
        private readonly global::Forkleans.Serialization.Activators.IActivator<global::TestProject.DerivedData> _activator;
        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public global::TestProject.DerivedData DeepCopy(global::TestProject.DerivedData original, global::Forkleans.Serialization.Cloning.CopyContext context)
        {
            if (context.TryGetCopy(original, out global::TestProject.DerivedData existing))
                return existing;
            if (original.GetType() != typeof(global::TestProject.DerivedData))
                return context.DeepCopy(original);
            var result = _activator.Create();
            context.RecordCopy(original, result);
            DeepCopy(original, result, context);
            return result;
        }

        public Copier_DerivedData(global::Forkleans.Serialization.Serializers.ICodecProvider codecProvider, global::Forkleans.Serialization.Activators.IActivator<global::TestProject.DerivedData> _activator)
        {
            _baseTypeCopier = ForkleansGeneratedCodeHelper.GetService<ForkleansCodeGen.TestProject.Copier_BaseData>(this, codecProvider);
            this._activator = ForkleansGeneratedCodeHelper.UnwrapService(this, _activator);
        }

        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void DeepCopy(global::TestProject.DerivedData input, global::TestProject.DerivedData output, global::Forkleans.Serialization.Cloning.CopyContext context)
        {
            _baseTypeCopier.DeepCopy(input, output, context);
            output.DerivedValue = input.DerivedValue;
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ForkleansCodeGen", "9.0.0.0"), global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    internal sealed class Metadata_TestProject : global::Forkleans.Serialization.Configuration.TypeManifestProviderBase
    {
        protected override void ConfigureInner(global::Forkleans.Serialization.Configuration.TypeManifestOptions config)
        {
            config.Serializers.Add(typeof(ForkleansCodeGen.TestProject.Codec_BaseData));
            config.Serializers.Add(typeof(ForkleansCodeGen.TestProject.Codec_DerivedData));
            config.Copiers.Add(typeof(ForkleansCodeGen.TestProject.Copier_BaseData));
            config.Copiers.Add(typeof(ForkleansCodeGen.TestProject.Copier_DerivedData));
        }
    }
}
#pragma warning restore CS1591, RS0016, RS0041
