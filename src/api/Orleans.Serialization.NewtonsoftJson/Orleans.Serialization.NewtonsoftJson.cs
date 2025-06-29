//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
namespace Forkleans.Serialization
{
    [Alias("json.net")]
    public partial class NewtonsoftJsonCodec : Serializers.IGeneralizedCodec, Codecs.IFieldCodec, Cloning.IGeneralizedCopier, Cloning.IDeepCopier, ITypeFilter
    {
        public const string WellKnownAlias = "json.net";
        public NewtonsoftJsonCodec(System.Collections.Generic.IEnumerable<Serializers.ICodecSelector> serializableTypeSelectors, System.Collections.Generic.IEnumerable<Serializers.ICopierSelector> copyableTypeSelectors, Microsoft.Extensions.Options.IOptions<NewtonsoftJsonCodecOptions> options) { }

        object Cloning.IDeepCopier.DeepCopy(object input, Cloning.CopyContext context) { throw null; }

        bool Cloning.IGeneralizedCopier.IsSupportedType(System.Type type) { throw null; }

        object Codecs.IFieldCodec.ReadValue<TInput>(ref Buffers.Reader<TInput> reader, WireProtocol.Field field) { throw null; }

        void Codecs.IFieldCodec.WriteField<TBufferWriter>(ref Buffers.Writer<TBufferWriter> writer, uint fieldIdDelta, System.Type expectedType, object value) { }

        bool? ITypeFilter.IsTypeAllowed(System.Type type) { throw null; }

        bool Serializers.IGeneralizedCodec.IsSupportedType(System.Type type) { throw null; }
    }

    public partial class NewtonsoftJsonCodecOptions
    {
        public System.Func<System.Type, bool?> IsCopyableType { get { throw null; } set { } }

        public System.Func<System.Type, bool?> IsSerializableType { get { throw null; } set { } }

        public Newtonsoft.Json.JsonSerializerSettings SerializerSettings { get { throw null; } set { } }
    }

    public static partial class SerializationHostingExtensions
    {
        public static ISerializerBuilder AddNewtonsoftJsonSerializer(this ISerializerBuilder serializerBuilder, System.Func<System.Type, bool> isSupported, Newtonsoft.Json.JsonSerializerSettings jsonSerializerSettings = null) { throw null; }

        public static ISerializerBuilder AddNewtonsoftJsonSerializer(this ISerializerBuilder serializerBuilder, System.Func<System.Type, bool> isSupported, System.Action<Microsoft.Extensions.Options.OptionsBuilder<NewtonsoftJsonCodecOptions>> configureOptions) { throw null; }

        public static ISerializerBuilder AddNewtonsoftJsonSerializer(this ISerializerBuilder serializerBuilder, System.Func<System.Type, bool> isSerializable, System.Func<System.Type, bool> isCopyable, System.Action<Microsoft.Extensions.Options.OptionsBuilder<NewtonsoftJsonCodecOptions>> configureOptions) { throw null; }
    }
}