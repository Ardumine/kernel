namespace AFCP.KASerializer.Serializers.Numeric;

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
