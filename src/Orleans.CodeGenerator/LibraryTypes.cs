#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Forkleans.CodeGenerator.SyntaxGeneration;

namespace Forkleans.CodeGenerator
{
    internal sealed class LibraryTypes
    {
        private readonly ConcurrentDictionary<ITypeSymbol, bool> _shallowCopyableTypes = new(SymbolEqualityComparer.Default);

        public static LibraryTypes FromCompilation(Compilation compilation, CodeGeneratorOptions options) => new LibraryTypes(compilation, options);

        private LibraryTypes(Compilation compilation, CodeGeneratorOptions options)
        {
            Compilation = compilation;
            ApplicationPartAttribute = Type("Forkleans.ApplicationPartAttribute");
            Action_2 = Type("System.Action`2");
            TypeManifestProviderBase = Type("Forkleans.Serialization.Configuration.TypeManifestProviderBase");
            Field = Type("Forkleans.Serialization.WireProtocol.Field");
            FieldCodec_1 = Type("Forkleans.Serialization.Codecs.IFieldCodec`1");
            AbstractTypeSerializer = Type("Forkleans.Serialization.Serializers.AbstractTypeSerializer`1");
            DeepCopier_1 = Type("Forkleans.Serialization.Cloning.IDeepCopier`1");
            ShallowCopier = Type("Forkleans.Serialization.Cloning.ShallowCopier`1");
            CompoundTypeAliasAttribute = Type("Forkleans.CompoundTypeAliasAttribute");
            CopyContext = Type("Forkleans.Serialization.Cloning.CopyContext");
            MethodInfo = Type("System.Reflection.MethodInfo");
            Func_2 = Type("System.Func`2");
            GenerateMethodSerializersAttribute = Type("Forkleans.GenerateMethodSerializersAttribute");
            GenerateSerializerAttribute = Type("Forkleans.GenerateSerializerAttribute");
            SerializationCallbacksAttribute = Type("Forkleans.SerializationCallbacksAttribute");
            IActivator_1 = Type("Forkleans.Serialization.Activators.IActivator`1");
            IBufferWriter = Type("System.Buffers.IBufferWriter`1");
            IdAttributeTypes = options.IdAttributes.Select(Type).ToArray();
            ConstructorAttributeTypes = options.ConstructorAttributes.Select(Type).ToArray();
            AliasAttribute = Type("Forkleans.AliasAttribute");
            IInvokable = Type("Forkleans.Serialization.Invocation.IInvokable");
            InvokeMethodNameAttribute = Type("Forkleans.InvokeMethodNameAttribute");
            RuntimeHelpers = Type("System.Runtime.CompilerServices.RuntimeHelpers");
            InvokableCustomInitializerAttribute = Type("Forkleans.InvokableCustomInitializerAttribute");
            DefaultInvokableBaseTypeAttribute = Type("Forkleans.DefaultInvokableBaseTypeAttribute");
            GenerateCodeForDeclaringAssemblyAttribute = Type("Forkleans.GenerateCodeForDeclaringAssemblyAttribute");
            InvokableBaseTypeAttribute = Type("Forkleans.InvokableBaseTypeAttribute");
            ReturnValueProxyAttribute = Type("Forkleans.Invocation.ReturnValueProxyAttribute");
            RegisterSerializerAttribute = Type("Forkleans.RegisterSerializerAttribute");
            ResponseTimeoutAttribute = Type("Forkleans.ResponseTimeoutAttribute");
            GeneratedActivatorConstructorAttribute = Type("Forkleans.GeneratedActivatorConstructorAttribute");
            SerializerTransparentAttribute = Type("Forkleans.SerializerTransparentAttribute");
            RegisterActivatorAttribute = Type("Forkleans.RegisterActivatorAttribute");
            RegisterConverterAttribute = Type("Forkleans.RegisterConverterAttribute");
            RegisterCopierAttribute = Type("Forkleans.RegisterCopierAttribute");
            UseActivatorAttribute = Type("Forkleans.UseActivatorAttribute");
            SuppressReferenceTrackingAttribute = Type("Forkleans.SuppressReferenceTrackingAttribute");
            OmitDefaultMemberValuesAttribute = Type("Forkleans.OmitDefaultMemberValuesAttribute");
            ITargetHolder = Type("Forkleans.Serialization.Invocation.ITargetHolder");
            TypeManifestProviderAttribute = Type("Forkleans.Serialization.Configuration.TypeManifestProviderAttribute");
            NonSerializedAttribute = Type("System.NonSerializedAttribute");
            ObsoleteAttribute = Type("System.ObsoleteAttribute");
            BaseCodec_1 = Type("Forkleans.Serialization.Serializers.IBaseCodec`1");
            BaseCopier_1 = Type("Forkleans.Serialization.Cloning.IBaseCopier`1");
            ArrayCodec = Type("Forkleans.Serialization.Codecs.ArrayCodec`1");
            ArrayCopier = Type("Forkleans.Serialization.Codecs.ArrayCopier`1");
            Reader = Type("Forkleans.Serialization.Buffers.Reader`1");
            TypeManifestOptions = Type("Forkleans.Serialization.Configuration.TypeManifestOptions");
            Task = Type("System.Threading.Tasks.Task");
            Task_1 = Type("System.Threading.Tasks.Task`1");
            this.Type = Type("System.Type");
            _uri = Type("System.Uri");
            _int128 = TypeOrDefault("System.Int128");
            _uInt128 = TypeOrDefault("System.UInt128");
            _half = TypeOrDefault("System.Half");
            _dateOnly = TypeOrDefault("System.DateOnly");
            _dateTimeOffset = Type("System.DateTimeOffset");
            _bitVector32 = Type("System.Collections.Specialized.BitVector32");
            _compareInfo = Type("System.Globalization.CompareInfo");
            _cultureInfo = Type("System.Globalization.CultureInfo");
            _version = Type("System.Version");
            _timeOnly = TypeOrDefault("System.TimeOnly");
            Guid = Type("System.Guid");
            ICodecProvider = Type("Forkleans.Serialization.Serializers.ICodecProvider");
            ValueSerializer = Type("Forkleans.Serialization.Serializers.IValueSerializer`1");
            ValueTask = Type("System.Threading.Tasks.ValueTask");
            ValueTask_1 = Type("System.Threading.Tasks.ValueTask`1");
            ValueTypeGetter_2 = Type("Forkleans.Serialization.Utilities.ValueTypeGetter`2");
            ValueTypeSetter_2 = Type("Forkleans.Serialization.Utilities.ValueTypeSetter`2");
            Writer = Type("Forkleans.Serialization.Buffers.Writer`1");
            FSharpSourceConstructFlagsOrDefault = TypeOrDefault("Microsoft.FSharp.Core.SourceConstructFlags");
            FSharpCompilationMappingAttributeOrDefault = TypeOrDefault("Microsoft.FSharp.Core.CompilationMappingAttribute");
            StaticCodecs = new List<WellKnownCodecDescription>
                {
                    new(compilation.GetSpecialType(SpecialType.System_Object), Type("Forkleans.Serialization.Codecs.ObjectCodec")),
                    new(compilation.GetSpecialType(SpecialType.System_Boolean), Type("Forkleans.Serialization.Codecs.BoolCodec")),
                    new(compilation.GetSpecialType(SpecialType.System_Char), Type("Forkleans.Serialization.Codecs.CharCodec")),
                    new(compilation.GetSpecialType(SpecialType.System_Byte), Type("Forkleans.Serialization.Codecs.ByteCodec")),
                    new(compilation.GetSpecialType(SpecialType.System_SByte), Type("Forkleans.Serialization.Codecs.SByteCodec")),
                    new(compilation.GetSpecialType(SpecialType.System_Int16), Type("Forkleans.Serialization.Codecs.Int16Codec")),
                    new(compilation.GetSpecialType(SpecialType.System_Int32), Type("Forkleans.Serialization.Codecs.Int32Codec")),
                    new(compilation.GetSpecialType(SpecialType.System_Int64), Type("Forkleans.Serialization.Codecs.Int64Codec")),
                    new(compilation.GetSpecialType(SpecialType.System_UInt16), Type("Forkleans.Serialization.Codecs.UInt16Codec")),
                    new(compilation.GetSpecialType(SpecialType.System_UInt32), Type("Forkleans.Serialization.Codecs.UInt32Codec")),
                    new(compilation.GetSpecialType(SpecialType.System_UInt64), Type("Forkleans.Serialization.Codecs.UInt64Codec")),
                    new(compilation.GetSpecialType(SpecialType.System_String), Type("Forkleans.Serialization.Codecs.StringCodec")),
                    new(compilation.CreateArrayTypeSymbol(compilation.GetSpecialType(SpecialType.System_Byte), 1), Type("Forkleans.Serialization.Codecs.ByteArrayCodec")),
                    new(compilation.GetSpecialType(SpecialType.System_Single), Type("Forkleans.Serialization.Codecs.FloatCodec")),
                    new(compilation.GetSpecialType(SpecialType.System_Double), Type("Forkleans.Serialization.Codecs.DoubleCodec")),
                    new(compilation.GetSpecialType(SpecialType.System_Decimal), Type("Forkleans.Serialization.Codecs.DecimalCodec")),
                    new(compilation.GetSpecialType(SpecialType.System_DateTime), Type("Forkleans.Serialization.Codecs.DateTimeCodec")),
                    new(Type("System.TimeSpan"), Type("Forkleans.Serialization.Codecs.TimeSpanCodec")),
                    new(Type("System.DateTimeOffset"), Type("Forkleans.Serialization.Codecs.DateTimeOffsetCodec")),
                    new(TypeOrDefault("System.DateOnly"), TypeOrDefault("Forkleans.Serialization.Codecs.DateOnlyCodec")),
                    new(TypeOrDefault("System.TimeOnly"), TypeOrDefault("Forkleans.Serialization.Codecs.TimeOnlyCodec")),
                    new(Type("System.Guid"), Type("Forkleans.Serialization.Codecs.GuidCodec")),
                    new(Type("System.Type"), Type("Forkleans.Serialization.Codecs.TypeSerializerCodec")),
                    new(Type("System.ReadOnlyMemory`1").Construct(compilation.GetSpecialType(SpecialType.System_Byte)), Type("Forkleans.Serialization.Codecs.ReadOnlyMemoryOfByteCodec")),
                    new(Type("System.Memory`1").Construct(compilation.GetSpecialType(SpecialType.System_Byte)), Type("Forkleans.Serialization.Codecs.MemoryOfByteCodec")),
                    new(Type("System.Net.IPAddress"), Type("Forkleans.Serialization.Codecs.IPAddressCodec")),
                    new(Type("System.Net.IPEndPoint"), Type("Forkleans.Serialization.Codecs.IPEndPointCodec")),
                    new(TypeOrDefault("System.UInt128"), TypeOrDefault("Forkleans.Serialization.Codecs.UInt128Codec")),
                    new(TypeOrDefault("System.Int128"), TypeOrDefault("Forkleans.Serialization.Codecs.Int128Codec")),
                    new(TypeOrDefault("System.Half"), TypeOrDefault("Forkleans.Serialization.Codecs.HalfCodec")),
                    new(Type("System.Uri"), Type("Forkleans.Serialization.Codecs.UriCodec")),
                }.Where(desc => desc.UnderlyingType is { } && desc.CodecType is { }).ToArray();
            WellKnownCodecs = new WellKnownCodecDescription[]
            {
                    new(Type("System.Exception"), Type("Forkleans.Serialization.ExceptionCodec")),
                    new(Type("System.Collections.Generic.Dictionary`2"), Type("Forkleans.Serialization.Codecs.DictionaryCodec`2")),
                    new(Type("System.Collections.Generic.List`1"), Type("Forkleans.Serialization.Codecs.ListCodec`1")),
                    new(Type("System.Collections.Generic.HashSet`1"), Type("Forkleans.Serialization.Codecs.HashSetCodec`1")),
                    new(compilation.GetSpecialType(SpecialType.System_Nullable_T), Type("Forkleans.Serialization.Codecs.NullableCodec`1")),
            };
            StaticCopiers = new WellKnownCopierDescription[]
            {
                    new(compilation.GetSpecialType(SpecialType.System_Object), Type("Forkleans.Serialization.Codecs.ObjectCopier")),
                    new(compilation.CreateArrayTypeSymbol(compilation.GetSpecialType(SpecialType.System_Byte), 1), Type("Forkleans.Serialization.Codecs.ByteArrayCopier")),
                    new(Type("System.ReadOnlyMemory`1").Construct(compilation.GetSpecialType(SpecialType.System_Byte)), Type("Forkleans.Serialization.Codecs.ReadOnlyMemoryOfByteCopier")),
                    new(Type("System.Memory`1").Construct(compilation.GetSpecialType(SpecialType.System_Byte)), Type("Forkleans.Serialization.Codecs.MemoryOfByteCopier")),
            };
            WellKnownCopiers = new WellKnownCopierDescription[]
            {
                    new(Type("System.Exception"), Type("Forkleans.Serialization.ExceptionCodec")),
                    new(Type("System.Collections.Generic.Dictionary`2"), Type("Forkleans.Serialization.Codecs.DictionaryCopier`2")),
                    new(Type("System.Collections.Generic.List`1"), Type("Forkleans.Serialization.Codecs.ListCopier`1")),
                    new(Type("System.Collections.Generic.HashSet`1"), Type("Forkleans.Serialization.Codecs.HashSetCopier`1")),
                    new(compilation.GetSpecialType(SpecialType.System_Nullable_T), Type("Forkleans.Serialization.Codecs.NullableCopier`1")),
            };
            Exception = Type("System.Exception");
            ImmutableAttributes = options.ImmutableAttributes.Select(Type).ToArray();
            TimeSpan = Type("System.TimeSpan");
            _ipAddress = Type("System.Net.IPAddress");
            _ipEndPoint = Type("System.Net.IPEndPoint");
            CancellationToken = Type("System.Threading.CancellationToken");
            CancellationTokenSource = Type("System.Threading.CancellationTokenSource");
            _immutableContainerTypes = new[]
            {
                    compilation.GetSpecialType(SpecialType.System_Nullable_T),
                    Type("System.Tuple`1"),
                    Type("System.Tuple`2"),
                    Type("System.Tuple`3"),
                    Type("System.Tuple`4"),
                    Type("System.Tuple`5"),
                    Type("System.Tuple`6"),
                    Type("System.Tuple`7"),
                    Type("System.Tuple`8"),
                    Type("System.ValueTuple`1"),
                    Type("System.ValueTuple`2"),
                    Type("System.ValueTuple`3"),
                    Type("System.ValueTuple`4"),
                    Type("System.ValueTuple`5"),
                    Type("System.ValueTuple`6"),
                    Type("System.ValueTuple`7"),
                    Type("System.ValueTuple`8"),
                    Type("System.Collections.Immutable.ImmutableArray`1"),
                    Type("System.Collections.Immutable.ImmutableDictionary`2"),
                    Type("System.Collections.Immutable.ImmutableHashSet`1"),
                    Type("System.Collections.Immutable.ImmutableList`1"),
                    Type("System.Collections.Immutable.ImmutableQueue`1"),
                    Type("System.Collections.Immutable.ImmutableSortedDictionary`2"),
                    Type("System.Collections.Immutable.ImmutableSortedSet`1"),
                    Type("System.Collections.Immutable.ImmutableStack`1"),
                };

            LanguageVersion = (compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions)?.LanguageVersion;
            GenerateSerializerAttributes = options.GenerateSerializerAttributes.Select(compilation.GetTypeByMetadataName).ToArray();

            INamedTypeSymbol Type(string metadataName)
            {
                var result = compilation.GetTypeByMetadataName(metadataName);
                if (result is null)
                {
                    throw new InvalidOperationException("Cannot find type with metadata name " + metadataName);
                }

                return result;
            }

            INamedTypeSymbol? TypeOrDefault(string metadataName)
            {
                var result = compilation.GetTypeByMetadataName(metadataName);
                return result;
            }
        }

        public INamedTypeSymbol Action_2 { get; private set; }
        public INamedTypeSymbol TypeManifestProviderBase { get; private set; }
        public INamedTypeSymbol Field { get; private set; }
        public INamedTypeSymbol DeepCopier_1 { get; private set; }
        public INamedTypeSymbol ShallowCopier { get; private set; }
        public INamedTypeSymbol FieldCodec_1 { get; private set; }
        public INamedTypeSymbol AbstractTypeSerializer { get; private set; }
        public INamedTypeSymbol Func_2 { get; private set; }
        public INamedTypeSymbol CompoundTypeAliasAttribute { get; private set; }
        public INamedTypeSymbol GenerateMethodSerializersAttribute { get; private set; }
        public INamedTypeSymbol GenerateSerializerAttribute { get; private set; }
        public INamedTypeSymbol IActivator_1 { get; private set; }
        public INamedTypeSymbol IBufferWriter { get; private set; }
        public INamedTypeSymbol IInvokable { get; private set; }
        public INamedTypeSymbol ITargetHolder { get; private set; }
        public INamedTypeSymbol TypeManifestProviderAttribute { get; private set; }
        public INamedTypeSymbol NonSerializedAttribute { get; private set; }
        public INamedTypeSymbol ObsoleteAttribute { get; private set; }
        public INamedTypeSymbol BaseCodec_1 { get; private set; }
        public INamedTypeSymbol BaseCopier_1 { get; private set; }
        public INamedTypeSymbol ArrayCodec { get; private set; }
        public INamedTypeSymbol ArrayCopier { get; private set; }
        public INamedTypeSymbol Reader { get; private set; }
        public INamedTypeSymbol TypeManifestOptions { get; private set; }
        public INamedTypeSymbol Task { get; private set; }
        public INamedTypeSymbol Task_1 { get; private set; }
        public INamedTypeSymbol Type { get; private set; }
        private INamedTypeSymbol _uri;
        private INamedTypeSymbol? _dateOnly;
        private INamedTypeSymbol _dateTimeOffset;
        private INamedTypeSymbol? _timeOnly;
        public INamedTypeSymbol MethodInfo { get; private set; }
        public INamedTypeSymbol ICodecProvider { get; private set; }
        public INamedTypeSymbol ValueSerializer { get; private set; }
        public INamedTypeSymbol ValueTask { get; private set; }
        public INamedTypeSymbol ValueTask_1 { get; private set; }
        public INamedTypeSymbol ValueTypeGetter_2 { get; private set; }
        public INamedTypeSymbol ValueTypeSetter_2 { get; private set; }
        public INamedTypeSymbol Writer { get; private set; }
        public INamedTypeSymbol[] IdAttributeTypes { get; private set; }
        public INamedTypeSymbol[] ConstructorAttributeTypes { get; private set; }
        public INamedTypeSymbol AliasAttribute { get; private set; }
        public WellKnownCodecDescription[] StaticCodecs { get; private set; }
        public WellKnownCodecDescription[] WellKnownCodecs { get; private set; }
        public WellKnownCopierDescription[] StaticCopiers { get; private set; }
        public WellKnownCopierDescription[] WellKnownCopiers { get; private set; }
        public INamedTypeSymbol RegisterCopierAttribute { get; private set; }
        public INamedTypeSymbol RegisterSerializerAttribute { get; private set; }
        public INamedTypeSymbol ResponseTimeoutAttribute { get; private set; }
        public INamedTypeSymbol RegisterConverterAttribute { get; private set; }
        public INamedTypeSymbol RegisterActivatorAttribute { get; private set; }
        public INamedTypeSymbol UseActivatorAttribute { get; private set; }
        public INamedTypeSymbol SuppressReferenceTrackingAttribute { get; private set; }
        public INamedTypeSymbol OmitDefaultMemberValuesAttribute { get; private set; }
        public INamedTypeSymbol CopyContext { get; private set; }
        public INamedTypeSymbol CancellationToken { get; private set; }
        public INamedTypeSymbol CancellationTokenSource { get; }
        public INamedTypeSymbol Guid { get; private set; }
        public Compilation Compilation { get; private set; }
        public INamedTypeSymbol TimeSpan { get; private set; }
        private INamedTypeSymbol _ipAddress;
        private INamedTypeSymbol _ipEndPoint;
        private INamedTypeSymbol[] _immutableContainerTypes;
        private INamedTypeSymbol _bitVector32;
        private INamedTypeSymbol _compareInfo;
        private INamedTypeSymbol _cultureInfo;
        private INamedTypeSymbol _version;
        private INamedTypeSymbol? _int128;
        private INamedTypeSymbol? _uInt128;
        private INamedTypeSymbol? _half;
        private INamedTypeSymbol[]? _regularShallowCopyableTypes;
        private INamedTypeSymbol[] RegularShallowCopyableType => _regularShallowCopyableTypes ??= new List<INamedTypeSymbol?>
        {
            TimeSpan,
            _dateOnly,
            _timeOnly,
            _dateTimeOffset,
            Guid,
            _bitVector32,
            _compareInfo,
            _cultureInfo,
            _version,
            _ipAddress,
            _ipEndPoint,
            CancellationToken,
            Type,
            _uri,
            _uInt128,
            _int128,
            _half
        }.Where(t => t is {}).ToArray()!;

        public INamedTypeSymbol[] ImmutableAttributes { get; private set; }
        public INamedTypeSymbol Exception { get; private set; }
        public INamedTypeSymbol ApplicationPartAttribute { get; private set; }
        public INamedTypeSymbol InvokeMethodNameAttribute { get; private set; }
        public INamedTypeSymbol InvokableCustomInitializerAttribute { get; private set; }
        public INamedTypeSymbol InvokableBaseTypeAttribute { get; private set; }
        public INamedTypeSymbol ReturnValueProxyAttribute { get; private set; }
        public INamedTypeSymbol DefaultInvokableBaseTypeAttribute { get; private set; }
        public INamedTypeSymbol GenerateCodeForDeclaringAssemblyAttribute { get; private set; }
        public INamedTypeSymbol SerializationCallbacksAttribute { get; private set; }
        public INamedTypeSymbol GeneratedActivatorConstructorAttribute { get; private set; }
        public INamedTypeSymbol SerializerTransparentAttribute { get; private set; }
        public INamedTypeSymbol? FSharpCompilationMappingAttributeOrDefault { get; private set; }
        public INamedTypeSymbol? FSharpSourceConstructFlagsOrDefault { get; private set; }
        public INamedTypeSymbol RuntimeHelpers { get; private set; }

        public LanguageVersion? LanguageVersion { get; private set; }

        public INamedTypeSymbol?[] GenerateSerializerAttributes { get; }

        public bool IsShallowCopyable(ITypeSymbol type)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_Boolean:
                case SpecialType.System_Char:
                case SpecialType.System_SByte:
                case SpecialType.System_Byte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                case SpecialType.System_Decimal:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_String:
                case SpecialType.System_DateTime:
                    return true;
            }

            if (_shallowCopyableTypes.TryGetValue(type, out var result))
            {
                return result;
            }

            foreach (var shallowCopyable in RegularShallowCopyableType)
            {
                if (SymbolEqualityComparer.Default.Equals(shallowCopyable, type))
                {
                    return _shallowCopyableTypes[type] = true;
                }
            }

            if (type.IsSealed && type.HasAnyAttribute(ImmutableAttributes))
            {
                return _shallowCopyableTypes[type] = true;
            }

            if (type.HasBaseType(Exception))
            {
                return _shallowCopyableTypes[type] = true;
            }

            if (!(type is INamedTypeSymbol namedType))
            {
                return _shallowCopyableTypes[type] = false;
            }

            if (namedType.IsTupleType)
            {
                return _shallowCopyableTypes[type] = AreShallowCopyable(namedType.TupleElements);
            }
            else if (namedType.IsGenericType)
            {
                var def = namedType.ConstructedFrom;
                foreach (var t in _immutableContainerTypes)
                {
                    if (SymbolEqualityComparer.Default.Equals(t, def))
                        return _shallowCopyableTypes[type] = AreShallowCopyable(namedType.TypeArguments);
                }
            }
            else
            {
                if (type.TypeKind == TypeKind.Enum)
                {
                    return _shallowCopyableTypes[type] = true;
                }

                if (type.TypeKind == TypeKind.Struct && !namedType.IsUnboundGenericType)
                {
                    return _shallowCopyableTypes[type] = IsValueTypeFieldsShallowCopyable(type);
                }
            }

            return _shallowCopyableTypes[type] = false;
        }

        private bool IsValueTypeFieldsShallowCopyable(ITypeSymbol type)
        {
            foreach (var field in type.GetDeclaredInstanceMembers<IFieldSymbol>())
            {
                if (field.Type is not INamedTypeSymbol fieldType)
                {
                    return false;
                }

                if (SymbolEqualityComparer.Default.Equals(type, fieldType))
                {
                    return false;
                }

                if (!IsShallowCopyable(fieldType))
                {
                    return false;
                }
            }

            return true;
        }

        private bool AreShallowCopyable(ImmutableArray<ITypeSymbol> types)
        {
            foreach (var t in types)
                if (!IsShallowCopyable(t))
                    return false;

            return true;
        }

        private bool AreShallowCopyable(ImmutableArray<IFieldSymbol> fields)
        {
            foreach (var f in fields)
                if (!IsShallowCopyable(f.Type))
                    return false;

            return true;
        }
    }

    internal static class LibraryExtensions
    {
        public static WellKnownCodecDescription? FindByUnderlyingType(this WellKnownCodecDescription[] values, ISymbol type)
        {
            foreach (var c in values)
                if (SymbolEqualityComparer.Default.Equals(c.UnderlyingType, type))
                    return c;

            return null;
        }

        public static WellKnownCopierDescription? FindByUnderlyingType(this WellKnownCopierDescription[] values, ISymbol type)
        {
            foreach (var c in values)
                if (SymbolEqualityComparer.Default.Equals(c.UnderlyingType, type))
                    return c;

            return null;
        }

        public static bool HasScopedKeyword(this LibraryTypes libraryTypes) => libraryTypes.LanguageVersion is null or >= LanguageVersion.CSharp11;
    }
}
