using System.Collections;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Kernel.Modules.Base;

public class TestClass
{
    public int Param1 { get; set; }

    // public List<int> SubClasses { get; set; } = new();

    //public List<TestSubClass> testSubClasses { get; set; } = new();
    [CanHaveOtherTypes]
    public object Obj { get; set; }

}

/// <summary>
/// When the property can have another types. This includes derivation.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class CanHaveOtherTypes : Attribute
{
}

public class TestSubClass
{
    public string Param3 { get; set; }
}

//https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/built-in-types

public class BoolSerializer : KADataSerializer
{
    public Type type => typeof(bool);
    public object Deserialize(Stream stream, KASerializer serializer, Type type)
    {
        byte[] arr = new byte[1];
        stream.ReadExactly(arr);

        return arr[0] == 1;
    }


    public void Serialize(object obj, Stream stream, KASerializer serializer)
    {
        stream.WriteByte(((bool)obj) ? (byte)1 : (byte)0);
    }
}

public class ByteSerializer : KADataSerializer
{
    public Type type => typeof(byte);
    public object Deserialize(Stream stream, KASerializer serializer, Type type)
    {
        byte[] arr = new byte[1];
        stream.ReadExactly(arr);

        return arr[0];
    }


    public void Serialize(object obj, Stream stream, KASerializer serializer)
    {
        stream.WriteByte((byte)obj);
    }
}

public class ByteArraySerializer : KADataSerializer
{
    public Type type => typeof(byte[]);
    public object Deserialize(Stream stream, KASerializer serializer, Type type)
    {
        return serializer.DeserializeByteArray4(stream);
    }


    public void Serialize(object obj, Stream stream, KASerializer serializer)
    {
        serializer.SerializeByteArray4((byte[])obj, stream);
    }
}

public class StringSerializer : KADataSerializer
{
    public Type type => typeof(string);
    public object Deserialize(Stream stream, KASerializer serializer, Type type)
    {
        return Tools.GetString(serializer.DeserializeByteArray4(stream));
    }


    public void Serialize(object obj, Stream stream, KASerializer serializer)
    {
        byte[] bytes = Tools.GetBytes((string)obj);
        serializer.SerializeByteArray4(bytes, stream);
    }
}


public class CharSerializer : KADataSerializer
{
    public Type type => typeof(char);
    public object Deserialize(Stream stream, KASerializer serializer, Type type)
    {
        byte[] arr = new byte[2];
        stream.ReadExactly(arr);

        return BitConverter.ToChar(arr);
    }


    public void Serialize(object obj, Stream stream, KASerializer serializer)
    {
        stream.Write(BitConverter.GetBytes((char)obj));
    }
}

//Decimal

public class DoubleSerializer : KADataSerializer
{
    public Type type => typeof(double);
    public object Deserialize(Stream stream, KASerializer serializer, Type type)
    {
        byte[] arr = new byte[8];
        stream.ReadExactly(arr);

        return BitConverter.ToDouble(arr);
    }


    public void Serialize(object obj, Stream stream, KASerializer serializer)
    {
        stream.Write(BitConverter.GetBytes((double)obj));
    }
}

public class FloatSerializer : KADataSerializer
{
    public Type type => typeof(float);
    public object Deserialize(Stream stream, KASerializer serializer, Type type)
    {
        byte[] arr = new byte[4];
        stream.ReadExactly(arr);

        return BitConverter.ToSingle(arr);
    }


    public void Serialize(object obj, Stream stream, KASerializer serializer)
    {
        stream.Write(BitConverter.GetBytes((float)obj));
    }
}

public class IntSerializer : KADataSerializer
{
    public Type type => typeof(int);
    public object Deserialize(Stream stream, KASerializer serializer, Type type)
    {
        byte[] arr = new byte[4];
        stream.ReadExactly(arr);

        return Tools.GetInt(arr);
    }


    public void Serialize(object obj, Stream stream, KASerializer serializer)
    {
        stream.Write(Tools.GetBytes((int)obj));
    }
}

public class UIntSerializer : KADataSerializer
{
    public Type type => typeof(uint);
    public object Deserialize(Stream stream, KASerializer serializer, Type type)
    {
        byte[] arr = new byte[4];
        stream.ReadExactly(arr);

        return Tools.GetUInt(arr);
    }


    public void Serialize(object obj, Stream stream, KASerializer serializer)
    {
        stream.Write(Tools.GetBytes((uint)obj));
    }
}

//nint nuint

public class LongSerializer : KADataSerializer
{
    public Type type => typeof(long);
    public object Deserialize(Stream stream, KASerializer serializer, Type type)
    {
        byte[] arr = new byte[8];
        stream.ReadExactly(arr);

        return BitConverter.ToInt64(arr);
    }


    public void Serialize(object obj, Stream stream, KASerializer serializer)
    {
        stream.Write(BitConverter.GetBytes((long)obj));
    }
}

public class ULongSerializer : KADataSerializer
{
    public Type type => typeof(ulong);
    public object Deserialize(Stream stream, KASerializer serializer, Type type)
    {
        byte[] arr = new byte[8];
        stream.ReadExactly(arr);

        return BitConverter.ToUInt64(arr);
    }


    public void Serialize(object obj, Stream stream, KASerializer serializer)
    {
        stream.Write(BitConverter.GetBytes((ulong)obj));
    }
}

public class ShortSerializer : KADataSerializer
{
    public Type type => typeof(short);
    public object Deserialize(Stream stream, KASerializer serializer, Type type)
    {
        byte[] arr = new byte[2];
        stream.ReadExactly(arr);

        return BitConverter.ToInt16(arr);
    }


    public void Serialize(object obj, Stream stream, KASerializer serializer)
    {
        stream.Write(BitConverter.GetBytes((short)obj));
    }
}

public class UShortSerializer : KADataSerializer
{
    public Type type => typeof(ushort);
    public object Deserialize(Stream stream, KASerializer serializer, Type type)
    {
        byte[] arr = new byte[2];
        stream.ReadExactly(arr);

        return BitConverter.ToUInt16(arr);
    }


    public void Serialize(object obj, Stream stream, KASerializer serializer)
    {
        stream.Write(BitConverter.GetBytes((ushort)obj));
    }
}

public class ListSerializer : KADataSerializer
{
    public Type type { get; set; } = typeof(List<>);
    public Type DataType { get; set; } = null!;
    public bool SpecialDataType = true;

    public IList CreateList(Type myType)
    {
        Type genericListType = typeof(List<>).MakeGenericType(myType);
        return (IList)Activator.CreateInstance(genericListType)!;
    }

    public object Deserialize(Stream stream, KASerializer serializer, Type type)
    {

        var list = CreateList(DataType);

        var len = serializer.Deserialize<uint>(stream);
        for (int i = 0; i < len; i++)
        {
            list.Add(serializer.Deserialize(stream, DataType));
        }
        return list;
    }

    public void Serialize(object obj, Stream stream, KASerializer serializer)
    {
        if (!SpecialDataType)
        {
        }
        else
        {
            var list = (IList)obj;

            stream.Write(Tools.GetBytes((uint)list.Count));
            //var t = list.GetType().GetGenericArguments()[0];
            //var t =  list.GetType().GetGenericTypeDefinition();
            ///Console.WriteLine(t + "   " + DataType);
            for (int i = 0; i < list.Count; i++)
            {
                serializer.Serialize(list[i]!, stream, DataType);
            }
        }


    }
}

public class ListSerializer<Ta> : KADataSerializer where Ta : struct
{
    public Type type { get; set; } = typeof(List<Ta>);
    public bool SpecialDataType { get; set; } = false;

    public object Deserialize(Stream stream, KASerializer serializer, Type type)
    {

        // var list = new List<Ta>();
        var recData = serializer.DeserializeByteArray4(stream);

        var totalLen = recData.Length;
        int lenToParse = totalLen / Marshal.SizeOf(typeof(Ta));
        var data = new Ta[lenToParse];


        Buffer.BlockCopy(recData, 0, data, 0, lenToParse);

        //for (int i = 0; i < len; i++)
        //{
        //   list.Add((Ta)serializer.Deserialize(ms, typeof(Ta)));
        //}
        return data.ToList();
    }



    public void Serialize(object obj, Stream stream, KASerializer serializer)
    {
        if (!SpecialDataType)
        {

            var list = ((List<Ta>)obj).ToArray();


            int totalBytes = list.Length * Marshal.SizeOf(typeof(Ta));
            byte[] byteArray = new byte[totalBytes];

            Buffer.BlockCopy(list, 0, byteArray, 0, totalBytes);
            serializer.SerializeByteArray4(byteArray, stream);
        }
        else
        {
            var list = (IList)obj;

            stream.Write(Tools.GetBytes((uint)list.Count));
            //var t = list.GetType().GetGenericArguments()[0];
            //var t =  list.GetType().GetGenericTypeDefinition();
            ///Console.WriteLine(t + "   " + DataType);
            for (int i = 0; i < list.Count; i++)
            {
                serializer.Serialize(list[i]!, stream, typeof(Ta));
            }
        }


    }
}

public interface KADataSerializer
{
    public Type type { get; }
    public void Serialize(object obj, Stream stream, KASerializer serializer);
    public object Deserialize(Stream stream, KASerializer serializer, Type type);
}



public struct KAProperty
{
    public required string Name { get; set; }
    public Type type { get; set; }
    public FastMethodInfo GetMethod { get; set; }
    public FastMethodInfo SetMethod { get; set; }
    public required bool CanChangeType { get; set; }
}

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

        AddCustomSerializer(new ListSerializer());
    }
    public void AddCustomSerializer(KADataSerializer serializer)
    {
        if (!TypesSerializers.ContainsKey(serializer.type))
            TypesSerializers[serializer.type] = serializer;
    }
    public void GenerateCacheForType(Type type)
    {
        if (CachedTypes.ContainsKey(type))
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


        var props = new List<KAProperty>();
        //Get get methods and cache for type
        foreach (var prop in type.GetProperties().ToList().OrderBy(pr => pr.Name))
        {
            if (!CachedTypes.ContainsKey(prop.PropertyType) && prop.PropertyType != typeof(object) && !TypesSerializers.ContainsKey(prop.PropertyType))//&& IsComplex(prop.PropertyType) 
            {
                // if (IsList(prop.PropertyType))
                //{
                //     GenerateCacheForType(prop.PropertyType.GetGenericArguments()[0]);
                // }
                // else
                //  {
                GenerateCacheForType(prop.PropertyType);
                // }
            }
            if (prop.GetMethod == null || prop.SetMethod == null)
            {
                throw new NotImplementedException($"The property '{prop.Name}' in '{prop.DeclaringType?.FullName}' does not have set or get method. Make sure you have a {{get; set;}} on the declaring property.");
            }


            props.Add(new()
            {
                GetMethod = new(prop.GetMethod),
                SetMethod = new(prop.SetMethod),
                type = prop.PropertyType,
                //CanChangeType = prop.PropertyType == typeof(object),
                CanChangeType = Attribute.IsDefined(prop, typeof(CanHaveOtherTypes)),
                Name = prop.Name
            });
        }
        CachedTypes[type] = props.ToArray();
    }


    public void SerializeType(Type type, Stream stream)
    {
        SerializeByteArray2(Tools.GetBytes(type.FullName!), stream);
    }
    public Type DeserializeType(Stream stream)
    {
        var name = Tools.GetString(DeserializeByteArray2(stream));
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


    public void Serialize(object obj, Stream stream, Type type)
    {
        if (TypesSerializers.TryGetValue(type, out var customSerializer))
        {
            customSerializer.Serialize(obj, stream, this);
        }
        else
        {
            //if (IsComplex(type))
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
                    if (prop.CanChangeType)
                    {
                        var a = prop.GetMethod.Invoke(obj);
                        var objType = a.GetType();

                        SerializeType(objType, stream);
                        Serialize(a, stream, objType);

                    }
                    else
                    {
                        Serialize(prop.GetMethod.Invoke(obj), stream, prop.type);
                    }
                }
                //}
            }
            //else
            {
                //    throw new NotImplementedException($"The type {type.FullName} is not totally implemented");
            }

        }

    }
    public void Serialize<T>(T obj, Stream stream)
    {
        Serialize(obj!, stream, typeof(T));
    }

    public object Deserialize(Stream stream, Type type)
    {
        // if (!IsComplex(type))
        //{

        if (TypesSerializers.TryGetValue(type, out var customSerializer))
        {
            return customSerializer.Deserialize(stream, this, type);
        }
        //}
        var obj = Activator.CreateInstance(type)!;

        var cachedType = CachedTypes[type];


        for (int i = 0; i < cachedType.Length; i++)
        {
            var prop = cachedType[i];


            if (TypesSerializers.TryGetValue(prop.type, out var customSerializer_) && !prop.CanChangeType)
            {
                prop.SetMethod.Invoke(obj, customSerializer_.Deserialize(stream, this, prop.type));
            }
            else
            {
                if (prop.CanChangeType)
                {
                    var dataType = DeserializeType(stream);
                    //GenerateCacheForType(dataType);

                    prop.SetMethod.Invoke(obj, Deserialize(stream, dataType));
                }
                else
                {
                    prop.SetMethod.Invoke(obj, Deserialize(stream, prop.type));
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
        return (T)Deserialize(stream, typeof(T));
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



    private bool IsComplex(Type typeIn)
    {
        return !(typeIn.IsSubclassOf(typeof(ValueType)) || typeIn.Equals(typeof(string)) || typeIn.Equals(typeof(byte[]))) || IsList(typeIn);//|| typeIn.IsPrimitive 
    }
    private bool IsList(Type typeIn)
    {
        return typeIn.IsGenericType && typeIn.GetGenericTypeDefinition() == typeof(List<>) && typeIn != typeof(byte[]);
    }
    
}


internal class Program
{
    static void PrintArr(byte[] arr)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            Console.Write(arr[i] + " ");
        }
        Console.WriteLine();
    }
    private static void Main(string[] args)
    {
        var ser = new KASerializer();


        ser.GenerateCacheForType(typeof(TestClass));

        MemoryStream ms = new MemoryStream();

        var obj = new TestClass()
        {
            Obj = new TestSubClass()
            {
                Param3 = "testtesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttest"
            }
        };

        ser.Serialize(obj, ms);

        var data = ms.ToArray();

        PrintArr(data);

        var deser = ser.Deserialize<TestClass>(new MemoryStream(data));

        Console.WriteLine(((TestSubClass)deser.Obj).Param3);



    }
}