using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using Kernel.AFCP.KASerializer.Serializers;
using Kernel.AFCP.KASerializer.Serializers.EnumSerializer;
using Kernel.AFCP.KASerializer.Serializers.ListSerliazer;
using Kernel.AFCP.KASerializer.Serializers.NumericSerliazer;
using Kernel.AFCP.KASerializer.Serializers.StringSerializer;
using Kernel.AFCP.KASerializer.Serializers.StructArraySerializer;
using Kernel.AFCP.KASerializer.Serializers.VectorSerializer;

using Kernel.AFCP.KASerializer.Atributes;
using Kernel.AFCP.FastMethod;

namespace Kernel.AFCP.KASerializer;

public struct KASerializer
{
    public ConcurrentDictionary<Type, ISubSerializer> subSerializers = new();
    public ConcurrentDictionary<Type, KAProperty[]> typeProps = new();

    private StructArraySerializer structArraySerializer;
    private EnumSerializer enumSerializer;
    private ListSerializer listSerializer;

    public KASerializer()
    {
        structArraySerializer = new();
        enumSerializer = new();
        listSerializer = new();
        AddDefaultSerializers();

    }

    public void AddDefaultSerializers()
    {
        AddSubSerializer(new IntSerializer());
        AddSubSerializer(new UIntSerializer());

        AddSubSerializer(new FloatSerializer());
        AddSubSerializer(new DoubleSerializer());

        AddSubSerializer(new StringSerializer());
        AddSubSerializer(new GuidSerializer());


        AddSubSerializer(new BoolSerializer());

        AddSubSerializer(new Vector2Serializer());
        AddSubSerializer(new Vector3Serializer());

        //AddSubSerializer(new EnumSerializer());


    }
    public void AddSubSerializer(ISubSerializer ser)
    {
        foreach (var type in ser.Types)
        {
            subSerializers[type] = ser;
        }
    }


    #region Data type writing
    private ConcurrentDictionary<byte[], Type> CachedTypesTo = new(ByteArrayComparer.Default);
    private ConcurrentDictionary<Type, byte[]> CachedTypesFrom = new();

    public void WriteDataType(Type type, Stream stream)
    {
        byte[] data;
        if (!CachedTypesFrom.TryGetValue(type, out data!))
        {
            CachedTypesFrom[type] = Tools.GetBytes(type.AssemblyQualifiedName);
            data = CachedTypesFrom[type];
        }
        WriteByteArray2Len(data, stream);
    }
    public Type ReadDataType(Stream stream)
    {
        var d = ReadByteArray2Len(stream);
        Type type;
        if (!CachedTypesTo.TryGetValue(d, out type!))
        {
            CachedTypesTo[d] = Type.GetType(Tools.GetString(d))!;
            return CachedTypesTo[d];

        }
        return type;
    }

    #endregion

    #region Byte array writing and reading
    public void WriteByteArray2Len(byte[] bytes, Stream stream)
    {
        stream.Write(Tools.GetBytes((ushort)bytes.Length));
        stream.Write(bytes);
    }

    public void WriteByteArray4Len(byte[] bytes, Stream stream)
    {
        stream.Write(Tools.GetBytes((uint)bytes.Length));
        stream.Write(bytes);
    }

    public byte[] ReadByteArray2Len(Stream stream)
    {
        byte[] bytesLen = new byte[2];
        stream.ReadExactly(bytesLen);
        uint len = Tools.GetUShort(bytesLen);

        byte[] bytes = new byte[len];
        stream.ReadExactly(bytes);

        return bytes;
    }

    public byte[] ReadByteArray4Len(Stream stream)
    {
        byte[] bytesLen = new byte[4];
        stream.ReadExactly(bytesLen);
        uint len = Tools.GetUInt(bytesLen);

        byte[] bytes = new byte[len];
        stream.ReadExactly(bytes);

        return bytes;
    }

    public byte[] ReadByteArray(int len, Stream stream)
    {
        byte[] bytes = new byte[len];
        stream.ReadExactly(bytes);

        return bytes;
    }

    public void WriteByteArray(byte[] bytes, bool BigData, Stream stream)
    {
        if (BigData)
        {
            WriteByteArray4Len(bytes, stream);
        }
        else
        {
            WriteByteArray2Len(bytes, stream);
        }
    }

    public byte[] ReadByteArray(bool BigData, Stream stream)
    {
        if (BigData)
        {
            return ReadByteArray4Len(stream);
        }
        else
        {
            return ReadByteArray2Len(stream);
        }
    }


    #endregion



    public void Serialize(object obj, Stream stream) //, bool WriteType = false
    {
        bool CanBeDerived = Attribute.IsDefined(obj.GetType(), typeof(CanBeDerived));

        Serialize(obj!, stream, obj.GetType(), new KAType()
        {
            DataType = obj.GetType(),
            IsList = false,
            IsLongLength = false,
            IsStructUnmanagedArray = false,
            CanBeDerived = CanBeDerived,
            IsArray = false
        });
    }


    public void Serialize(object obj, Stream stream, Type dataType, KAType type)
    {
        //Console.WriteLine("Serialize : " + type.Name);
        if (type.CanBeDerived && !type.IsList && !type.IsArray)
        {
            dataType = obj.GetType();
            WriteDataType(dataType, stream);
        }

        if (dataType.IsEnum)
        {
            enumSerializer.Serialize(obj, stream);
        }
        else if (type.IsList || type.IsArray)
        {
            listSerializer.Serialize(obj, stream, this, type);
        }
        else if (subSerializers.TryGetValue(dataType, out var subSerializer))
        {
            subSerializer.Serialize(obj, stream, this, type);
        }
        else if (IsClassOrStruct(dataType))
        {
            SerializeClass(obj, dataType, stream);
        }


        else
        {
            throw new Exception($"Unable to serialize type {dataType.FullName}");
        }

    }


    public T Deserialize<T>(Stream stream) where T : class
    {
        bool CanBeDerived = Attribute.IsDefined(typeof(T), typeof(CanBeDerived));

        return (T)Deserialize(stream, typeof(T), new KAType()
        {
            DataType = typeof(T),
            IsList = false,
            IsLongLength = false,
            IsStructUnmanagedArray = false,
            CanBeDerived = CanBeDerived,
            IsArray = false
        });
    }

    public object Deserialize(Stream stream, Type dataType, KAType type)
    {
        if (type.CanBeDerived && !type.IsList && !type.IsArray)
        {
            dataType = ReadDataType(stream);
        }
        if (dataType.IsEnum)
        {
            return enumSerializer.Deserialize(stream, this, dataType);
        }
        else if (type.IsList || type.IsArray)
        {
            return listSerializer.Deserialize(stream, this, type);
        }
        else if (subSerializers.TryGetValue(dataType, out var subSerializer))
        {
            return subSerializer.Deserialize(stream, this, type);
        }
        else if (IsClassOrStruct(dataType))
        {
            return DeserializeClass(dataType, stream);
        }

        else
        {
            throw new Exception($"Unable to deserialize type {dataType.FullName}");
        }

    }





    private ObjectActivator CreateInstance(Type type)
    {
        if (type == null)
        {
            throw new NullReferenceException("type");
        }
        ConstructorInfo? emptyConstructor = type.GetConstructor(Type.EmptyTypes);
        if (emptyConstructor == null)
        {
            throw new Exception($"Unable to create type for deserialization.: {type}");
        }
        var dynamicMethod = new DynamicMethod("CreateInstance", type, Type.EmptyTypes, true);
        ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
        ilGenerator.Emit(OpCodes.Nop);
        ilGenerator.Emit(OpCodes.Newobj, emptyConstructor);
        ilGenerator.Emit(OpCodes.Ret);
        return (ObjectActivator)dynamicMethod.CreateDelegate(typeof(ObjectActivator));
    }
    public delegate object ObjectActivator();


    public void SerializeClass(object obj, Type objType, Stream stream)
    {
        var props = GetPropertyForClass(objType);
        KAProperty prop;

        for (int i = 0; i < props.Length; i++)
        {
            prop = props[i];
            var val = prop.GetMethod.Invoke(obj);

            if (prop.Type.IsStructUnmanagedArray)
            {
                structArraySerializer.Serialize(val, prop.Type.ElementType!, stream, this, prop.Type.IsLongLength);
            }
            else
            {
                Serialize(val, stream, prop.Type.DataType, prop.Type);

            }
        }
    }

    public object DeserializeClass(Type objType, Stream stream)
    {
        var newObj = CreateInstance(objType)();
        var props = GetPropertyForClass(objType);
        KAProperty prop;
        for (int i = 0; i < props.Length; i++)
        {
            prop = props[i];
            if (prop.Type.IsStructUnmanagedArray)
            {
                prop.SetMethod.Invoke(newObj, structArraySerializer.Deserialize(prop.Type.ElementType!, stream, this, prop.Type.IsLongLength));
            }
            else
            {
                prop.SetMethod.Invoke(newObj, Deserialize(stream, prop.Type.DataType, prop.Type));
            }
        }
        return newObj;
    }




    public KAType GetKAType(Type propType, bool CanBeDerived, bool IsLongLength)
    {

        bool IsStructUnmanagedArray = propType.IsArray;
        bool IsList = typeof(IList).IsAssignableFrom(propType);
        bool IsArray = propType.IsArray;

        var elementType = IsList ? propType.GetGenericArguments()[0] : (propType.IsArray ? propType.GetElementType() : null);

        return new()
        {
            DataType = propType,
            IsList = IsList,
            IsArray = IsArray,
            IsLongLength = IsLongLength,
            IsStructUnmanagedArray = IsStructUnmanagedArray,
            CanBeDerived = CanBeDerived,
            ElementType = elementType
        };
    }
    public KAProperty[] GetPropertyForClass(Type type)
    {
        KAProperty[] Props;
        if (!typeProps.TryGetValue(type, out Props!))
        {
            var listProps = new List<KAProperty>();
            var refProps = type.GetProperties().ToList().OrderBy(s => s.Name).ToArray();
            for (int i = 0; i < refProps.Length; i++)
            {
                var prop = refProps[i];


                if (prop.GetMethod == null || prop.SetMethod == null)
                {
                    throw new Exception($"The property {prop.Name} in the class '{type.FullName}' does not have an public get or set accessor. Make sure you have a {{ get; set; }}.");
                }

                var propType = prop.PropertyType;

                bool IsStructUnmanagedArray = Attribute.IsDefined(prop, typeof(FastParseStructArray)) && propType.IsArray;
                bool IsList = propType.IsGenericType && typeof(IList).IsAssignableFrom(propType);
                bool IsArray = propType.IsArray;

                bool CanBeDerived = Attribute.IsDefined(prop, typeof(CanHaveOtherTypes));
                bool IsLongLength = Attribute.IsDefined(prop, typeof(IsLongLength));

                //Console.WriteLine(prop.Name + IsList + IsArray);

                var elementType = IsList ? propType.GetGenericArguments()[0] : (IsArray ? propType.GetElementType() : null);

                listProps.Add(new KAProperty()
                {
                    Name = prop.Name,
                    GetMethod = new(prop.GetMethod),
                    SetMethod = new(prop.SetMethod),
                    Type = new()
                    {
                        DataType = propType,
                        IsList = IsList,
                        IsArray = IsArray,

                        IsLongLength = IsLongLength,
                        IsStructUnmanagedArray = IsStructUnmanagedArray,
                        CanBeDerived = CanBeDerived,
                        ElementType = elementType,
                    }
                });



            }

            Props = listProps.ToArray();
        }
        return Props;
    }



    public bool IsClassOrStruct(Type type)
    {

        return type.IsValueType && !type.IsPrimitive || type.IsClass;
    }
}


public struct KAProperty
{
    public required string Name { get; set; }

    public FastMethodInfo GetMethod { get; set; }
    public FastMethodInfo SetMethod { get; set; }

    public required KAType Type { get; set; }
}

public struct KAType
{
    public required Type DataType { get; set; }

    public required bool CanBeDerived { get; set; }

    public required bool IsList { get; set; }
    public required bool IsArray { get; set; }
    public required bool IsStructUnmanagedArray { get; set; }

    public Type? ElementType { get; set; }

    public required bool IsLongLength { get; set; }
}