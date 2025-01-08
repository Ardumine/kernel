using System.Collections.Concurrent;
using Kernel.AFCP.KASerializer.Atributes;
using Kernel.AFCP.KASerializer.Serializers;
using Kernel.AFCP.KASerializer.Serializers.Guid;
using Kernel.AFCP.KASerializer.Serializers.Lists;
using Kernel.AFCP.KASerializer.Serializers.Numeric;
using Kernel.AFCP.KASerializer.Serializers.Text;
using Kernel.AFCP.KASerializer.Serializers.Vector;

namespace Kernel.AFCP.KASerializer;

public class KASerializer
{


    private ConcurrentDictionary<Type, KAProperty[]> CachedTypes = new();
    private ConcurrentDictionary<Type, KADataSerializer> TypesSerializers = new();
    private ConcurrentDictionary<string, Type> TypeNameMap = new();


    public KASerializer()
    {
        AddDefaultSerializer();
    }

    public void AddDefaultSerializer()
    {
        AddCustomSerializer(new ByteArraySerializer());
        AddCustomSerializer(new StringSerializer());

        AddCustomSerializer(new BoolSerializer());

        AddCustomSerializer(new ByteSerializer());
        AddCustomSerializer(new CharSerializer());

        AddCustomSerializer(new DoubleSerializer());
        AddCustomSerializer(new FloatSerializer());

        AddCustomSerializer(new IntSerializer());
        AddCustomSerializer(new UIntSerializer());

        AddCustomSerializer(new LongSerializer());
        AddCustomSerializer(new ULongSerializer());
        AddCustomSerializer(new ShortSerializer());
        AddCustomSerializer(new UShortSerializer());
        AddCustomSerializer(new ListSerializer<bool>());

        AddCustomSerializer(new ListSerializer<double>());
        AddCustomSerializer(new ListSerializer<float>());

        AddCustomSerializer(new ListSerializer<int>());
        AddCustomSerializer(new ListSerializer<uint>());
        AddCustomSerializer(new ListSerializer<long>());
        AddCustomSerializer(new ListSerializer<ulong>());
        AddCustomSerializer(new ListSerializer<short>());
        AddCustomSerializer(new ListSerializer<ushort>());


        AddCustomSerializer(new Vector2Serializer());
        AddCustomSerializer(new Vector3Serializer());

        AddCustomSerializer(new GuidSerializer());


        AddCustomSerializer(new ListSerializer());
    }
    public void AddCustomSerializer(KADataSerializer serializer)
    {
        if (!TypesSerializers.ContainsKey(serializer.type))
            TypesSerializers[serializer.type] = serializer;
    }
    public void GenerateCacheForType(Type type)
    {
        if (type != null && CachedTypes.ContainsKey(type))
        {
            return;
        }
        if (type == null)
        {
            return;
        }
        if (IsList(type))
        {
            GenerateCacheForType(type.GetGenericArguments()[0]);

            AddCustomSerializer(new ListSerializer()
            {
                type = type,
                DataType = type.GetGenericArguments()[0]
            });
            return;

        }
        if (IsArray(type))
        {

            GenerateCacheForType(type.GetElementType()!);

            AddCustomSerializer(new ArraySerializer()
            {
                type = type,
                DataType = type.GetElementType()!
            });
            return;

        }

        var props = new List<KAProperty>();
        //Get get methods and cache for type
        foreach (var prop in type.GetProperties().ToList().OrderBy(pr => pr.Name))
        {

            if (prop.GetMethod == null || prop.SetMethod == null)
            {
                throw new NotImplementedException($"The property '{prop.Name}' in '{prop.DeclaringType?.FullName}' does not have set or get method. Make sure you have a {{get; set;}} on the declaring property.");
            }


            if (!CachedTypes.ContainsKey(prop.PropertyType) && !TypesSerializers.ContainsKey(prop.PropertyType) && prop.PropertyType != typeof(object))//&& IsComplex(prop.PropertyType)   
            {
                // if (IsList(prop.PropertyType))
                //{
                //     GenerateCacheForType(prop.PropertyType.GetGenericArguments()[0]);
                // }
                // else
                //  {
                //Console.WriteLine("C: " + prop.PropertyType + prop.Name);
                GenerateCacheForType(prop.PropertyType);
                // }
            }


            props.Add(new()
            {
                GetMethod = new(prop.GetMethod),
                SetMethod = new(prop.SetMethod),
                type = prop.PropertyType,
                CanChangeType = Attribute.IsDefined(prop, typeof(CanHaveOtherTypes)) ,
                Name = prop.Name,
                IsListOrArray = IsList(prop.PropertyType) || IsArray(prop.PropertyType)
            });
        }
        CachedTypes[type] = props.ToArray();
    }


    public void SerializeType(Type? type, Stream stream)
    {
        if (type == null)
        {
            SerializeByteArray2(Tools.GetBytes("null"), stream);
        }
        else
        {
            SerializeByteArray2(Tools.GetBytes(type?.AssemblyQualifiedName), stream);
        }
    }
    public Type? DeserializeType(Stream stream)
    {
        var name = Tools.GetString(DeserializeByteArray2(stream));
        if (name == "null")
        {
            return null;
        }
        Type typeOut;
        if (TypeNameMap.TryGetValue(name, out typeOut!))
        {
            return typeOut;
        }
        else
        {
            typeOut = Type.GetType(name)!;
            TypeNameMap[name] = typeOut;
            return typeOut;
        }
    }


    public void Serialize(object obj, Stream stream, Type type, KAProperty fontProp)
    {
        //Console.WriteLine("Ser: " + obj.GetType() + "    " + fontProp.Name + fontProp.CanChangeType + "       " + type + fontProp.IsListOrArray);
        if (fontProp.CanChangeType && !fontProp.IsListOrArray)
        {
            SerializeType(obj.GetType(), stream);
            type = obj.GetType();
        
        }

        if (TypesSerializers.TryGetValue(type, out var customSerializer))
        {
            customSerializer.Serialize(obj, stream, this, fontProp);
        }
        else
        {

            KAProperty[] cachedType;
            if (!CachedTypes.TryGetValue(type, out cachedType!))
            {
                GenerateCacheForType(type);
                cachedType = CachedTypes[type];
            }

            for (int i = 0; i < cachedType.Length; i++)
            {
                var prop = cachedType[i];
                var val = prop.GetMethod.Invoke(obj);
                //Console.WriteLine($"        {prop.Name} {prop.IsListOrArray} {prop.CanChangeType}");
                if (prop.CanChangeType && !prop.IsListOrArray)
                {
                    var objType = val?.GetType();
                    SerializeType(objType, stream);

                    if (val != null)
                    {
                        Serialize(val, stream, objType!, prop);
                    }
                }
                else
                {
                    Serialize(val, stream, prop.type, prop);
                }
            }
            //}

            //    throw new NotImplementedException($"The type {type.FullName} is not totally implemented");

        }

    }
    public void Serialize<T>(T obj, Stream stream)
    {
        Serialize(obj!, stream, typeof(T), new KAProperty()
        {
            CanChangeType = false,
            IsListOrArray = false,
            Name = ""
        });
    }

    public object Deserialize(Stream stream, Type type, KAProperty fontProp)
    {
        if (fontProp.CanChangeType && !fontProp.IsListOrArray)
        {
            type = DeserializeType(stream)!;
        }
        if (TypesSerializers.TryGetValue(type, out var customSerializer))
        {
            return customSerializer.Deserialize(stream, this, type, fontProp);
        }

        var obj = Activator.CreateInstance(type)!;

        KAProperty[] cachedType;
        if (!CachedTypes.TryGetValue(type, out cachedType!))
        {
            GenerateCacheForType(type);
            cachedType = CachedTypes[type];
        }

        for (int i = 0; i < cachedType.Length; i++)
        {
            var prop = cachedType[i];


            if (TypesSerializers.TryGetValue(prop.type, out var customSerializer_) && !prop.CanChangeType)
            {
                prop.SetMethod.Invoke(obj, customSerializer_.Deserialize(stream, this, prop.type, prop));
            }
            else
            {
                if (prop.CanChangeType && !prop.IsListOrArray)
                {
                    var dataType = DeserializeType(stream);
                    //GenerateCacheForType(dataType);
                    if (dataType == null)
                    {
                        //prop.SetMethod.Invoke(obj, null);
                    }
                    else
                    {
                        prop.SetMethod.Invoke(obj, Deserialize(stream, dataType, prop));
                    }
                }
                else
                {
                    prop.SetMethod.Invoke(obj, Deserialize(stream, prop.type, prop));
                }
                //if (IsComplex(prop.type))
                {
                }
                //else
                {
                    //throw new NotImplementedException($"The complex type '{type.FullName}' is not implemented.");
                }

            }

        }
        return obj;

    }
    public T Deserialize<T>(Stream stream)
    {
        return (T)Deserialize(stream, typeof(T), new KAProperty()
        {
            CanChangeType = false,
            IsListOrArray = false,
            Name = ""
        });
    }


    public byte[] DeserializeByteArray4(Stream stream)
    {
        byte[] lenArr = new byte[4];
        stream.ReadExactly(lenArr);
        uint TamDadosPRec = Tools.GetUInt(lenArr);

        byte[] dados = new byte[TamDadosPRec];

        int lenDadosRecebido = 0;
        while (lenDadosRecebido != TamDadosPRec)
        {
            lenDadosRecebido += stream.Read(dados, lenDadosRecebido, (int)(TamDadosPRec - lenDadosRecebido));//int32 = 4 bytes
        }


        return dados;
    }
    public byte[] DeserializeByteArray2(Stream stream)
    {
        byte[] lenArr = new byte[2];
        stream.ReadExactly(lenArr);
        uint TamDadosPRec = Tools.GetUShort(lenArr);

        byte[] dados = new byte[TamDadosPRec];

        int lenDadosRecebido = 0;
        while (lenDadosRecebido != TamDadosPRec)
        {
            lenDadosRecebido += stream.Read(dados, lenDadosRecebido, (int)(TamDadosPRec - lenDadosRecebido));//int32 = 4 bytes
        }


        return dados;
    }

    public void SerializeByteArray4(byte[] data, Stream stream)
    {
        stream.Write(Tools.GetBytes((uint)data.Length));
        stream.Write(data);
    }
    public void SerializeByteArray2(byte[] data, Stream stream)
    {
        stream.Write(Tools.GetBytes((ushort)data.Length));
        stream.Write(data);
    }


    public bool IsList(Type typeIn)
    {
        return typeIn.IsGenericType && typeIn.GetGenericTypeDefinition() == typeof(List<>) && typeIn != typeof(byte[]);
    }
    public bool IsArray(Type typeIn)
    {
        return typeIn.IsArray && typeIn != typeof(byte[]);
    }
    
    private bool IsComplex(Type typeIn)
    {
        return !(typeIn.IsSubclassOf(typeof(ValueType)) || typeIn.Equals(typeof(string)) || typeIn.Equals(typeof(byte[]))) || IsList(typeIn);//|| typeIn.IsPrimitive 
    }
 

}
