
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kernel.AFCP.FastMethod;

public struct KATransformerForType
{
    public required KAType Type { get; set; }


    public required MethodInfo methodSerialize { get; set; }
    public required ReturnValueDelegate FastSerialize { get; set; }

    public MethodInfo methodDeserialize { get; set; }
    public ReturnValueDelegate FastDeserialize { get; set; }

    public required bool CanhandleDerived { get; set; }
}



//To implement:
//https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/supported-types
public class Serializer
{

    private ConcurrentDictionary<Type, KATransformerForType> _typesTransformer = new();
    private ConcurrentDictionary<Type, KATransformerForType> _typesForDerived = new();







    public KATransformerForType GetTypeTransformer(KAType type)
    {
        KATransformerForType kASerializerType;
        if (!_typesTransformer.TryGetValue(type.DType, out kASerializerType!))
        {
            KATransformerForType transformer;
            if (type.TypeOfData == KASubTypes.String)
            {
                transformer = CreateStringTransformer(type);
            }
            else if (type.TypeOfData == KASubTypes.Class || type.TypeOfData == KASubTypes.ManagedStruct)
            {
                transformer = CreateClassTransformer(type);
                if (type.CanBeDerived)
                {
                    transformer = CreateDerivedTransformer(type);
                }
            }
            else if (type.TypeOfData == KASubTypes.UnmanagedList)
            {
                transformer = CreateUnmanagedListTransformer(type);
            }
            else if (type.TypeOfData == KASubTypes.ManagedList)
            {
                transformer = CreateManagedListTransformer(type);
            }
            else if (type.TypeOfData == KASubTypes.UnmanagedStruct)
            {
                transformer = CreateUnmanagedStructTransformer(type);
            }
            else if (type.TypeOfData == KASubTypes.Guid)
            {
                transformer = CreateGuidTransformer(type);
            }
            else if (type.TypeOfData == KASubTypes.Dict)
            {
                transformer = CreateDictTransformer(type);
            }
            else if (type.TypeOfData == KASubTypes.Unmanaged1DArray)
            {
                transformer = CreateUnmanagedArrayTransformer(type);
            }
            else if (type.TypeOfData == KASubTypes.Managed1DArray)
            {
                transformer = CreateManagedArrayTransformer(type);
            }
            else
            {
                throw new Exception($"No transfomer found for type {type.FullName}");
            }



            if (type.CanBeNull)
            {
                transformer = CreateNullTransformer(type, transformer);
            }

            _typesTransformer[type.DType] = transformer;
            return transformer;
        }

        return kASerializerType;
    }
    public KATransformerForType GetTypeTransformer(Type type)
    {
        KATransformerForType kASerializerType;
        if (!_typesTransformer.TryGetValue(type, out kASerializerType!))
        {
            return GetTypeTransformer(KAType.Get(type));
        }
        return kASerializerType;
    }

    public KATransformerForType GetTypeTransformerDerived(KAType type)
    {
        KATransformerForType kASerializerType;
        if (!_typesForDerived.TryGetValue(type.DType, out kASerializerType!))
        {
            KATransformerForType transformer;
            if (type.TypeOfData == KASubTypes.String)
            {
                transformer = CreateStringTransformer(type);
            }
            else if (type.TypeOfData == KASubTypes.Class || type.TypeOfData == KASubTypes.ManagedStruct)
            {
                transformer = CreateClassTransformer(type);
            }
            else if (type.TypeOfData == KASubTypes.UnmanagedList)
            {
                transformer = CreateUnmanagedListTransformer(type);
            }
            else if (type.TypeOfData == KASubTypes.ManagedList)
            {
                transformer = CreateManagedListTransformer(type);
            }
            else if (type.TypeOfData == KASubTypes.UnmanagedStruct)
            {
                transformer = CreateUnmanagedStructTransformer(type);
            }
            else if (type.TypeOfData == KASubTypes.Guid)
            {
                transformer = CreateGuidTransformer(type);
            }
            else if (type.TypeOfData == KASubTypes.Dict)
            {
                transformer = CreateDictTransformer(type);
            }
            else if (type.TypeOfData == KASubTypes.Unmanaged1DArray)
            {
                transformer = CreateUnmanagedArrayTransformer(type);
            }
            else if (type.TypeOfData == KASubTypes.Managed1DArray)
            {
                transformer = CreateManagedArrayTransformer(type);
            }
            else
            {
                throw new Exception($"No transfomer found for type {type.FullName}");
            }


            if (type.CanBeNull)
            {
                transformer = CreateNullTransformer(type, transformer);
            }
            _typesForDerived[type.DType] = transformer;
            return transformer;
        }

        return kASerializerType;
    }


    public MethodInfo GetMethodForSerialize(MethodInfo currentSerializer, KAType objType, KAType currentSerType)
    {
        KATransformerForType kASerializerType;
        if (objType.Equals(currentSerType))
        {
            if (SerializerTools.CanBeNull(objType.DType))
                return NullableSerializer.GetNullSerialiazer(objType, currentSerializer);
            else
                return currentSerializer;
        }
        else if (_typesTransformer.TryGetValue(objType.DType, out kASerializerType!))
        {
            return kASerializerType.methodSerialize;
        }
        else
        {
            return GetTypeTransformer(objType).methodSerialize;
        }
    }
    public MethodInfo GetMethodForDeserialize(MethodInfo currentDeserializer, KAType objType, KAType currentSerType)
    {
        KATransformerForType kASerializerType;
        if (objType.Equals(currentSerType))
        {
            if (SerializerTools.CanBeNull(objType.DType))
                return NullableSerializer.GetNullDeserialiazer(objType, currentDeserializer);
            else
                return currentDeserializer;
        }
        else if (_typesTransformer.TryGetValue(objType.DType, out kASerializerType!))
        {
            return kASerializerType.methodDeserialize;
        }
        else
        {
            return GetTypeTransformer(objType).methodDeserialize;
        }

    }


    //https://github.com/mono/mono/issues/9875


    public KATransformerForType CreateNullTransformer(KAType type, KATransformerForType originalTransformer)
    {
        var serializeMethod = NullableSerializer.GetNullSerialiazer(type, originalTransformer.methodSerialize);
        var deserializeMethod = NullableSerializer.GetNullDeserialiazer(type, originalTransformer.methodDeserialize);

        return new KATransformerForType()
        {
            Type = type,

            methodSerialize = serializeMethod,
            FastSerialize = new FastMethodInfo(serializeMethod).Delegate,

            methodDeserialize = deserializeMethod,
            FastDeserialize = new FastMethodInfo(deserializeMethod).Delegate,
            CanhandleDerived = false
        };
        
    }

    public KATransformerForType CreateDerivedTransformer(KAType type)
    {
        var serializeMethod = DerivedSerializer.GetDerivedSerialiazer(this, type);
        var deserializeMethod = DerivedSerializer.GetDerivedDeserialiazer(this, type);

        return new KATransformerForType()
        {
            Type = type,

            methodSerialize = serializeMethod,
            FastSerialize = new FastMethodInfo(serializeMethod).Delegate,

            methodDeserialize = deserializeMethod,
            FastDeserialize = new FastMethodInfo(deserializeMethod).Delegate,
            CanhandleDerived = true
        };
        
    }

    public KATransformerForType CreateClassTransformer(KAType type)
    {
        var serializeMethod = ClassSerializer.CreateClassSerializerMethod(this, type);
        var deserializeMethod = ClassSerializer.CreateDeserializerMethod(this, type);

        return new KATransformerForType()
        {
            Type = type,

            methodSerialize = serializeMethod,
            FastSerialize = new FastMethodInfo(serializeMethod).Delegate,

            methodDeserialize = deserializeMethod,
            FastDeserialize = new FastMethodInfo(deserializeMethod).Delegate,
            CanhandleDerived = false
        };

    }
    public KATransformerForType CreateUnmanagedStructTransformer(KAType type)
    {
        var serializeMethod = UnmanagedSerializer.GetUnamangedStructSerializer(type);
        var deserializeMethod = UnmanagedSerializer.GetUnamangedStructDeserializer(type);

        return new KATransformerForType()
        {
            Type = type,

            methodSerialize = serializeMethod,
            FastSerialize = new FastMethodInfo(serializeMethod).Delegate,

            methodDeserialize = deserializeMethod,
            FastDeserialize = new FastMethodInfo(deserializeMethod).Delegate,
            CanhandleDerived = false

        };
        

    }
    public KATransformerForType CreateStringTransformer(KAType type)
    {
        var serializeMethod = StringeSerializer.GetStringSerializer();
        var deserializeMethod = StringeSerializer.GetStringDeserializer();

        return new KATransformerForType()
        {
            Type = type,

            methodSerialize = serializeMethod,
            FastSerialize = new FastMethodInfo(serializeMethod).Delegate,

            methodDeserialize = deserializeMethod,
            FastDeserialize = new FastMethodInfo(deserializeMethod).Delegate,
            CanhandleDerived = false

        };
        
    }
    public KATransformerForType CreateGuidTransformer(KAType type)
    {
        var serializeMethod = GuidSerializer.GetGuidSerializer();
        var deserializeMethod = GuidSerializer.GetGuidDeserializer();

        return new KATransformerForType()
        {
            Type = type,

            methodSerialize = serializeMethod,
            FastSerialize = new FastMethodInfo(serializeMethod).Delegate,

            methodDeserialize = deserializeMethod,
            FastDeserialize = new FastMethodInfo(deserializeMethod).Delegate,
            CanhandleDerived = false

        };
        
    }

    public KATransformerForType CreateUnmanagedListTransformer(KAType type)
    {
        var serializeMethod = ListaSerializer.GetUnmanagedListSerializer(type);
        var deserializeMethod = ListaSerializer.GetUnmanagedListDeserializer(type);

        return new KATransformerForType()
        {
            Type = type,

            methodSerialize = serializeMethod,
            FastSerialize = new FastMethodInfo(serializeMethod).Delegate,

            methodDeserialize = deserializeMethod,
            FastDeserialize = new FastMethodInfo(deserializeMethod).Delegate,
            CanhandleDerived = false

        };
        
    }
    public KATransformerForType CreateManagedListTransformer(KAType type)
    {
        var serializeMethod = ListaSerializer.GetManagedListSerializer(this, type);
        var deserializeMethod = ListaSerializer.GetManagedListDeserializer(this, type);

        return new KATransformerForType()
        {
            Type = type,

            methodSerialize = serializeMethod,
            FastSerialize = new FastMethodInfo(serializeMethod).Delegate,

            methodDeserialize = deserializeMethod,
            FastDeserialize = new FastMethodInfo(deserializeMethod).Delegate,
            CanhandleDerived = false

        };
        
    }
    public KATransformerForType CreateUnmanagedSpanTransformer(KAType type)
    {
        var serializeMethod = SpanSerializer.GetUnmanagedSpanSerializer(type);
        var deserializeMethod = SpanSerializer.GetUnmanagedSpanDeserializer(type);

        return new KATransformerForType()
        {
            Type = type,

            methodSerialize = serializeMethod,
            FastSerialize = new FastMethodInfo(serializeMethod).Delegate,

            methodDeserialize = deserializeMethod,
            FastDeserialize = new FastMethodInfo(deserializeMethod).Delegate,
            CanhandleDerived = false

        };
        
    }
    public KATransformerForType CreateUnmanagedArrayTransformer(KAType type)
    {
        var serializeMethod = ArraySerializer.GetUnmanagedArraySerializer(type);
        var deserializeMethod = ArraySerializer.GetUnmanagedArrayDeserializer(type);

        return new KATransformerForType()
        {
            Type = type,

            methodSerialize = serializeMethod,
            FastSerialize = new FastMethodInfo(serializeMethod).Delegate,

            methodDeserialize = deserializeMethod,
            FastDeserialize = new FastMethodInfo(deserializeMethod).Delegate,
            CanhandleDerived = false

        };
    }
    public KATransformerForType CreateManagedArrayTransformer(KAType type)
    {
        var serializeMethod = ArraySerializer.GetManagedArraySerializer(this, type);
        var deserializeMethod = ArraySerializer.GetManagedArrayDeserializer(this, type);

        return new KATransformerForType()
        {
            Type = type,

            methodSerialize = serializeMethod,
            FastSerialize = new FastMethodInfo(serializeMethod).Delegate,

            methodDeserialize = deserializeMethod,
            FastDeserialize = new FastMethodInfo(deserializeMethod).Delegate,
            CanhandleDerived = false

        };
    }
    public KATransformerForType CreateDictTransformer(KAType type)
    {
        var serializeMethod = DictSerializer.CreateSerializer(this, type, type.SubHoldingType[0], type.SubHoldingType[1]);
        var deserializeMethod = DictSerializer.CreateDeserializer(this, type, type.SubHoldingType[0], type.SubHoldingType[1]);

        return new KATransformerForType()
        {
            Type = type,

            methodSerialize = serializeMethod,
            FastSerialize = new FastMethodInfo(serializeMethod).Delegate,

            methodDeserialize = deserializeMethod,
            FastDeserialize = new FastMethodInfo(deserializeMethod).Delegate,
            CanhandleDerived = false

        };
    }



    public void Serialize<T>(Stream stream, T? obj)
    {
        GetTypeTransformer(typeof(T)).FastSerialize(null!, [this, stream, obj]);
    }

    //DO NOT REMOVE!
    public void Serialize(Stream stream, object obj, Type type)
    {

        KATransformerForType kASerializerType;
        if (!_typesForDerived.TryGetValue(type, out kASerializerType!))
        {
            kASerializerType = GetTypeTransformerDerived(KAType.Get(type));
            _typesForDerived[type] = kASerializerType;
        }

        kASerializerType.FastSerialize(null!, [this, stream, obj]);
    }

    public T Deserialize<T>(Stream stream)
    {
        return (T)GetTypeTransformer(typeof(T)).FastDeserialize(null!, [this, stream]);
    }

    //DO NOT REMOVE!
    public object Deserialize(Stream stream, Type type)
    {
        KATransformerForType kASerializerType;
        if (!_typesForDerived.TryGetValue(type, out kASerializerType!))
        {
            kASerializerType = GetTypeTransformerDerived(KAType.Get(type));
            _typesForDerived[type] = kASerializerType;
        }

        return kASerializerType.FastDeserialize(null!, [this, stream]);
    }

    public unsafe T Deserialize<T>(Span<byte> data)
    {
        UnmanagedMemoryStream readStream = new UnmanagedMemoryStream((byte*)Unsafe.AsPointer(ref data.GetPinnableReference()), data.Length, data.Length, FileAccess.Read);

        return (T)GetTypeTransformer(typeof(T)).FastDeserialize(null!, [this, readStream]);
    }



}
