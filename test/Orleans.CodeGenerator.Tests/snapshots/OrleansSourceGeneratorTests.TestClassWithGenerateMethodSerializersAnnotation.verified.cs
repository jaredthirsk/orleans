#pragma warning disable CS1591, RS0016, RS0041
[assembly: global::Forkleans.ApplicationPartAttribute("TestProject")]
[assembly: global::Forkleans.ApplicationPartAttribute("Forkleans.Core.Abstractions")]
[assembly: global::Forkleans.ApplicationPartAttribute("Forkleans.Serialization")]
[assembly: global::Forkleans.ApplicationPartAttribute("Forkleans.Core")]
[assembly: global::Forkleans.ApplicationPartAttribute("Forkleans.Runtime")]
[assembly: global::Forkleans.Serialization.Configuration.TypeManifestProviderAttribute(typeof(ForkleansCodeGen.TestProject.Metadata_TestProject))]
namespace ForkleansCodeGen
{
    using global::Forkleans.Serialization.Codecs;
    using global::Forkleans.Serialization.GeneratedCodeHelpers;

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ForkleansCodeGen", "9.0.0.0"), global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    [global::Forkleans.CompoundTypeAliasAttribute("inv", typeof(global::Forkleans.Runtime.GrainReference), typeof(global::IMyGrain), "6D39E404")]
    public sealed class Invokable_IMyGrain_GrainReference_6D39E404 : global::Forkleans.Runtime.TaskRequest<string>
    {
        public string arg0;
        global::IMyGrain _target;
        private static readonly global::System.Reflection.MethodInfo MethodBackingField = ForkleansGeneratedCodeHelper.GetMethodInfoOrDefault(typeof(global::IMyGrain), "SayHello", null, new[] { typeof(string) });
        public override int GetArgumentCount() => 1;
        public override string GetMethodName() => "SayHello";
        public override string GetInterfaceName() => "IMyGrain";
        public override string GetActivityName() => "IMyGrain/SayHello";
        public override global::System.Type GetInterfaceType() => typeof(global::IMyGrain);
        public override global::System.Reflection.MethodInfo GetMethod() => MethodBackingField;
        public override void SetTarget(global::Forkleans.Serialization.Invocation.ITargetHolder holder) => _target = holder.GetTarget<global::IMyGrain>();
        public override object GetTarget() => _target;
        public override void Dispose()
        {
            arg0 = default;
            _target = default;
        }

        public override object GetArgument(int index)
        {
            switch (index)
            {
                case 0:
                    return arg0;
                default:
                    return ForkleansGeneratedCodeHelper.InvokableThrowArgumentOutOfRange(index, 0);
            }
        }

        public override void SetArgument(int index, object value)
        {
            switch (index)
            {
                case 0:
                    arg0 = (string)value;
                    return;
                default:
                    ForkleansGeneratedCodeHelper.InvokableThrowArgumentOutOfRange(index, 0);
                    return;
            }
        }

        protected override global::System.Threading.Tasks.Task<string> InvokeInner() => _target.SayHello(arg0);
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ForkleansCodeGen", "9.0.0.0"), global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    internal sealed class Proxy_IMyGrain : global::Forkleans.Runtime.GrainReference, global::IMyGrain
    {
        public Proxy_IMyGrain(global::Forkleans.Runtime.GrainReferenceShared arg0, global::Forkleans.Runtime.IdSpan arg1) : base(arg0, arg1)
        {
        }

        global::System.Threading.Tasks.Task<string> global::IMyGrain.SayHello(string arg0)
        {
            var request = new ForkleansCodeGen.Invokable_IMyGrain_GrainReference_6D39E404();
            request.arg0 = arg0;
            return base.InvokeAsync<string>(request).AsTask();
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ForkleansCodeGen", "9.0.0.0"), global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    public sealed class Codec_Invokable_IMyGrain_GrainReference_6D39E404 : global::Forkleans.Serialization.Codecs.IFieldCodec<ForkleansCodeGen.Invokable_IMyGrain_GrainReference_6D39E404>
    {
        private readonly global::System.Type _codecFieldType = typeof(ForkleansCodeGen.Invokable_IMyGrain_GrainReference_6D39E404);
        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Serialize<TBufferWriter>(ref global::Forkleans.Serialization.Buffers.Writer<TBufferWriter> writer, ForkleansCodeGen.Invokable_IMyGrain_GrainReference_6D39E404 instance)
            where TBufferWriter : global::System.Buffers.IBufferWriter<byte>
        {
            global::Forkleans.Serialization.Codecs.StringCodec.WriteField(ref writer, 0U, instance.arg0);
        }

        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Deserialize<TReaderInput>(ref global::Forkleans.Serialization.Buffers.Reader<TReaderInput> reader, ForkleansCodeGen.Invokable_IMyGrain_GrainReference_6D39E404 instance)
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
                    instance.arg0 = global::Forkleans.Serialization.Codecs.StringCodec.ReadValue(ref reader, header);
                    reader.ReadFieldHeader(ref header);
                }

                reader.ConsumeEndBaseOrEndObject(ref header);
                break;
            }
        }

        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void WriteField<TBufferWriter>(ref global::Forkleans.Serialization.Buffers.Writer<TBufferWriter> writer, uint fieldIdDelta, global::System.Type expectedType, ForkleansCodeGen.Invokable_IMyGrain_GrainReference_6D39E404 @value)
            where TBufferWriter : global::System.Buffers.IBufferWriter<byte>
        {
            if (@value is null)
            {
                ReferenceCodec.WriteNullReference(ref writer, fieldIdDelta);
                return;
            }

            ReferenceCodec.MarkValueField(writer.Session);
            writer.WriteStartObject(fieldIdDelta, expectedType, _codecFieldType);
            Serialize(ref writer, @value);
            writer.WriteEndObject();
        }

        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ForkleansCodeGen.Invokable_IMyGrain_GrainReference_6D39E404 ReadValue<TReaderInput>(ref global::Forkleans.Serialization.Buffers.Reader<TReaderInput> reader, global::Forkleans.Serialization.WireProtocol.Field field)
        {
            if (field.IsReference)
                return ReferenceCodec.ReadReference<ForkleansCodeGen.Invokable_IMyGrain_GrainReference_6D39E404, TReaderInput>(ref reader, field);
            field.EnsureWireTypeTagDelimited();
            var result = new ForkleansCodeGen.Invokable_IMyGrain_GrainReference_6D39E404();
            ReferenceCodec.MarkValueField(reader.Session);
            Deserialize(ref reader, result);
            return result;
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ForkleansCodeGen", "9.0.0.0"), global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    public sealed class Copier_Invokable_IMyGrain_GrainReference_6D39E404 : global::Forkleans.Serialization.Cloning.IDeepCopier<ForkleansCodeGen.Invokable_IMyGrain_GrainReference_6D39E404>
    {
        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ForkleansCodeGen.Invokable_IMyGrain_GrainReference_6D39E404 DeepCopy(ForkleansCodeGen.Invokable_IMyGrain_GrainReference_6D39E404 original, global::Forkleans.Serialization.Cloning.CopyContext context)
        {
            if (original is null)
                return null;
            var result = new ForkleansCodeGen.Invokable_IMyGrain_GrainReference_6D39E404();
            result.arg0 = original.arg0;
            return result;
        }
    }
}

namespace ForkleansCodeGen.TestProject
{
    using global::Forkleans.Serialization.Codecs;
    using global::Forkleans.Serialization.GeneratedCodeHelpers;

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ForkleansCodeGen", "9.0.0.0"), global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    internal sealed class Metadata_TestProject : global::Forkleans.Serialization.Configuration.TypeManifestProviderBase
    {
        protected override void ConfigureInner(global::Forkleans.Serialization.Configuration.TypeManifestOptions config)
        {
            config.Serializers.Add(typeof(ForkleansCodeGen.Codec_Invokable_IMyGrain_GrainReference_6D39E404));
            config.Copiers.Add(typeof(ForkleansCodeGen.Copier_Invokable_IMyGrain_GrainReference_6D39E404));
            config.InterfaceProxies.Add(typeof(ForkleansCodeGen.Proxy_IMyGrain));
            config.Interfaces.Add(typeof(global::IMyGrain));
            var n1 = config.CompoundTypeAliases.Add("inv");
            var n2 = n1.Add(typeof(global::Forkleans.Runtime.GrainReference));
            var n3 = n2.Add(typeof(global::IMyGrain));
            n3.Add("6D39E404", typeof(ForkleansCodeGen.Invokable_IMyGrain_GrainReference_6D39E404));
        }
    }
}
#pragma warning restore CS1591, RS0016, RS0041
