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
    [global::Forkleans.CompoundTypeAliasAttribute("inv", typeof(global::Forkleans.Runtime.GrainReference), typeof(global::TestProject.IGrainA), "11405B98")]
    public sealed class Invokable_IGrainA_GrainReference_11405B98 : global::Forkleans.Runtime.TaskRequest<string>
    {
        public string arg0;
        global::TestProject.IGrainA _target;
        private static readonly global::System.Reflection.MethodInfo MethodBackingField = ForkleansGeneratedCodeHelper.GetMethodInfoOrDefault(typeof(global::TestProject.IGrainA), "MethodA", null, new[] { typeof(string) });
        public override int GetArgumentCount() => 1;
        public override string GetMethodName() => "MethodA";
        public override string GetInterfaceName() => "TestProject.IGrainA";
        public override string GetActivityName() => "IGrainA/MethodA";
        public override global::System.Type GetInterfaceType() => typeof(global::TestProject.IGrainA);
        public override global::System.Reflection.MethodInfo GetMethod() => MethodBackingField;
        public override void SetTarget(global::Forkleans.Serialization.Invocation.ITargetHolder holder) => _target = holder.GetTarget<global::TestProject.IGrainA>();
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

        protected override global::System.Threading.Tasks.Task<string> InvokeInner() => _target.MethodA(arg0);
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ForkleansCodeGen", "9.0.0.0"), global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    internal sealed class Proxy_IGrainA : global::Forkleans.Runtime.GrainReference, global::TestProject.IGrainA
    {
        public Proxy_IGrainA(global::Forkleans.Runtime.GrainReferenceShared arg0, global::Forkleans.Runtime.IdSpan arg1) : base(arg0, arg1)
        {
        }

        global::System.Threading.Tasks.Task<string> global::TestProject.IGrainA.MethodA(string arg0)
        {
            var request = new ForkleansCodeGen.TestProject.Invokable_IGrainA_GrainReference_11405B98();
            request.arg0 = arg0;
            return base.InvokeAsync<string>(request).AsTask();
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ForkleansCodeGen", "9.0.0.0"), global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    [global::Forkleans.CompoundTypeAliasAttribute("inv", typeof(global::Forkleans.Runtime.GrainReference), typeof(global::TestProject.IGrainB), "6B5D7809")]
    public sealed class Invokable_IGrainB_GrainReference_6B5D7809 : global::Forkleans.Runtime.TaskRequest<string>
    {
        public string arg0;
        global::TestProject.IGrainB _target;
        private static readonly global::System.Reflection.MethodInfo MethodBackingField = ForkleansGeneratedCodeHelper.GetMethodInfoOrDefault(typeof(global::TestProject.IGrainB), "MethodB", null, new[] { typeof(string) });
        public override int GetArgumentCount() => 1;
        public override string GetMethodName() => "MethodB";
        public override string GetInterfaceName() => "TestProject.IGrainB";
        public override string GetActivityName() => "IGrainB/MethodB";
        public override global::System.Type GetInterfaceType() => typeof(global::TestProject.IGrainB);
        public override global::System.Reflection.MethodInfo GetMethod() => MethodBackingField;
        public override void SetTarget(global::Forkleans.Serialization.Invocation.ITargetHolder holder) => _target = holder.GetTarget<global::TestProject.IGrainB>();
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

        protected override global::System.Threading.Tasks.Task<string> InvokeInner() => _target.MethodB(arg0);
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ForkleansCodeGen", "9.0.0.0"), global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    internal sealed class Proxy_IGrainB : global::Forkleans.Runtime.GrainReference, global::TestProject.IGrainB
    {
        public Proxy_IGrainB(global::Forkleans.Runtime.GrainReferenceShared arg0, global::Forkleans.Runtime.IdSpan arg1) : base(arg0, arg1)
        {
        }

        global::System.Threading.Tasks.Task<string> global::TestProject.IGrainB.MethodB(string arg0)
        {
            var request = new ForkleansCodeGen.TestProject.Invokable_IGrainB_GrainReference_6B5D7809();
            request.arg0 = arg0;
            return base.InvokeAsync<string>(request).AsTask();
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ForkleansCodeGen", "9.0.0.0"), global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    public sealed class Codec_Invokable_IGrainA_GrainReference_11405B98 : global::Forkleans.Serialization.Codecs.IFieldCodec<ForkleansCodeGen.TestProject.Invokable_IGrainA_GrainReference_11405B98>
    {
        private readonly global::System.Type _codecFieldType = typeof(ForkleansCodeGen.TestProject.Invokable_IGrainA_GrainReference_11405B98);
        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Serialize<TBufferWriter>(ref global::Forkleans.Serialization.Buffers.Writer<TBufferWriter> writer, ForkleansCodeGen.TestProject.Invokable_IGrainA_GrainReference_11405B98 instance)
            where TBufferWriter : global::System.Buffers.IBufferWriter<byte>
        {
            global::Forkleans.Serialization.Codecs.StringCodec.WriteField(ref writer, 0U, instance.arg0);
        }

        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Deserialize<TReaderInput>(ref global::Forkleans.Serialization.Buffers.Reader<TReaderInput> reader, ForkleansCodeGen.TestProject.Invokable_IGrainA_GrainReference_11405B98 instance)
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
        public void WriteField<TBufferWriter>(ref global::Forkleans.Serialization.Buffers.Writer<TBufferWriter> writer, uint fieldIdDelta, global::System.Type expectedType, ForkleansCodeGen.TestProject.Invokable_IGrainA_GrainReference_11405B98 @value)
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
        public ForkleansCodeGen.TestProject.Invokable_IGrainA_GrainReference_11405B98 ReadValue<TReaderInput>(ref global::Forkleans.Serialization.Buffers.Reader<TReaderInput> reader, global::Forkleans.Serialization.WireProtocol.Field field)
        {
            if (field.IsReference)
                return ReferenceCodec.ReadReference<ForkleansCodeGen.TestProject.Invokable_IGrainA_GrainReference_11405B98, TReaderInput>(ref reader, field);
            field.EnsureWireTypeTagDelimited();
            var result = new ForkleansCodeGen.TestProject.Invokable_IGrainA_GrainReference_11405B98();
            ReferenceCodec.MarkValueField(reader.Session);
            Deserialize(ref reader, result);
            return result;
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ForkleansCodeGen", "9.0.0.0"), global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    public sealed class Copier_Invokable_IGrainA_GrainReference_11405B98 : global::Forkleans.Serialization.Cloning.IDeepCopier<ForkleansCodeGen.TestProject.Invokable_IGrainA_GrainReference_11405B98>
    {
        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ForkleansCodeGen.TestProject.Invokable_IGrainA_GrainReference_11405B98 DeepCopy(ForkleansCodeGen.TestProject.Invokable_IGrainA_GrainReference_11405B98 original, global::Forkleans.Serialization.Cloning.CopyContext context)
        {
            if (original is null)
                return null;
            var result = new ForkleansCodeGen.TestProject.Invokable_IGrainA_GrainReference_11405B98();
            result.arg0 = original.arg0;
            return result;
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ForkleansCodeGen", "9.0.0.0"), global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    public sealed class Codec_Invokable_IGrainB_GrainReference_6B5D7809 : global::Forkleans.Serialization.Codecs.IFieldCodec<ForkleansCodeGen.TestProject.Invokable_IGrainB_GrainReference_6B5D7809>
    {
        private readonly global::System.Type _codecFieldType = typeof(ForkleansCodeGen.TestProject.Invokable_IGrainB_GrainReference_6B5D7809);
        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Serialize<TBufferWriter>(ref global::Forkleans.Serialization.Buffers.Writer<TBufferWriter> writer, ForkleansCodeGen.TestProject.Invokable_IGrainB_GrainReference_6B5D7809 instance)
            where TBufferWriter : global::System.Buffers.IBufferWriter<byte>
        {
            global::Forkleans.Serialization.Codecs.StringCodec.WriteField(ref writer, 0U, instance.arg0);
        }

        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Deserialize<TReaderInput>(ref global::Forkleans.Serialization.Buffers.Reader<TReaderInput> reader, ForkleansCodeGen.TestProject.Invokable_IGrainB_GrainReference_6B5D7809 instance)
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
        public void WriteField<TBufferWriter>(ref global::Forkleans.Serialization.Buffers.Writer<TBufferWriter> writer, uint fieldIdDelta, global::System.Type expectedType, ForkleansCodeGen.TestProject.Invokable_IGrainB_GrainReference_6B5D7809 @value)
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
        public ForkleansCodeGen.TestProject.Invokable_IGrainB_GrainReference_6B5D7809 ReadValue<TReaderInput>(ref global::Forkleans.Serialization.Buffers.Reader<TReaderInput> reader, global::Forkleans.Serialization.WireProtocol.Field field)
        {
            if (field.IsReference)
                return ReferenceCodec.ReadReference<ForkleansCodeGen.TestProject.Invokable_IGrainB_GrainReference_6B5D7809, TReaderInput>(ref reader, field);
            field.EnsureWireTypeTagDelimited();
            var result = new ForkleansCodeGen.TestProject.Invokable_IGrainB_GrainReference_6B5D7809();
            ReferenceCodec.MarkValueField(reader.Session);
            Deserialize(ref reader, result);
            return result;
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ForkleansCodeGen", "9.0.0.0"), global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    public sealed class Copier_Invokable_IGrainB_GrainReference_6B5D7809 : global::Forkleans.Serialization.Cloning.IDeepCopier<ForkleansCodeGen.TestProject.Invokable_IGrainB_GrainReference_6B5D7809>
    {
        [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ForkleansCodeGen.TestProject.Invokable_IGrainB_GrainReference_6B5D7809 DeepCopy(ForkleansCodeGen.TestProject.Invokable_IGrainB_GrainReference_6B5D7809 original, global::Forkleans.Serialization.Cloning.CopyContext context)
        {
            if (original is null)
                return null;
            var result = new ForkleansCodeGen.TestProject.Invokable_IGrainB_GrainReference_6B5D7809();
            result.arg0 = original.arg0;
            return result;
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ForkleansCodeGen", "9.0.0.0"), global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    internal sealed class Metadata_TestProject : global::Forkleans.Serialization.Configuration.TypeManifestProviderBase
    {
        protected override void ConfigureInner(global::Forkleans.Serialization.Configuration.TypeManifestOptions config)
        {
            config.Serializers.Add(typeof(ForkleansCodeGen.TestProject.Codec_Invokable_IGrainA_GrainReference_11405B98));
            config.Serializers.Add(typeof(ForkleansCodeGen.TestProject.Codec_Invokable_IGrainB_GrainReference_6B5D7809));
            config.Copiers.Add(typeof(ForkleansCodeGen.TestProject.Copier_Invokable_IGrainA_GrainReference_11405B98));
            config.Copiers.Add(typeof(ForkleansCodeGen.TestProject.Copier_Invokable_IGrainB_GrainReference_6B5D7809));
            config.InterfaceProxies.Add(typeof(ForkleansCodeGen.TestProject.Proxy_IGrainA));
            config.InterfaceProxies.Add(typeof(ForkleansCodeGen.TestProject.Proxy_IGrainB));
            config.Interfaces.Add(typeof(global::TestProject.IGrainA));
            config.Interfaces.Add(typeof(global::TestProject.IGrainB));
            config.InterfaceImplementations.Add(typeof(global::TestProject.RealGrain));
            var n1 = config.CompoundTypeAliases.Add("inv");
            var n2 = n1.Add(typeof(global::Forkleans.Runtime.GrainReference));
            var n3 = n2.Add(typeof(global::TestProject.IGrainA));
            n3.Add("11405B98", typeof(ForkleansCodeGen.TestProject.Invokable_IGrainA_GrainReference_11405B98));
            var n5 = n2.Add(typeof(global::TestProject.IGrainB));
            n5.Add("6B5D7809", typeof(ForkleansCodeGen.TestProject.Invokable_IGrainB_GrainReference_6B5D7809));
        }
    }
}
#pragma warning restore CS1591, RS0016, RS0041
