namespace Kernel.AFCP.KASerializer.Serializers.Text;


public class BoolSerializer : KADataSerializer
{
    public Type type => typeof(bool);
    public object Deserialize(Stream stream, KASerializer serializer, Type type, KAProperty prop)
    {
        byte[] arr = new byte[1];
        stream.ReadExactly(arr);

        return arr[0] == 1;
    }


    public void Serialize(object obj, Stream stream, KASerializer serializer, KAProperty prop)
    {
        stream.WriteByte(((bool)obj) ? (byte)1 : (byte)0);
    }
}

public class ByteSerializer : KADataSerializer
{
    public Type type => typeof(byte);
    public object Deserialize(Stream stream, KASerializer serializer, Type type, KAProperty prop)
    {
        byte[] arr = new byte[1];
        stream.ReadExactly(arr);

        return arr[0];
    }


    public void Serialize(object obj, Stream stream, KASerializer serializer, KAProperty prop)
    {
        stream.WriteByte((byte)obj);
    }
}

public class ByteArraySerializer : KADataSerializer
{
    public Type type => typeof(byte[]);
    public object Deserialize(Stream stream, KASerializer serializer, Type type, KAProperty prop)
    {
        return serializer.DeserializeByteArray4(stream);
    }


    public void Serialize(object obj, Stream stream, KASerializer serializer, KAProperty prop)
    {
        serializer.SerializeByteArray4((byte[])obj, stream);
    }
}

public class StringSerializer : KADataSerializer
{
    public Type type => typeof(string);
    public object Deserialize(Stream stream, KASerializer serializer, Type type, KAProperty prop)
    {
        return Tools.GetString(serializer.DeserializeByteArray4(stream));
    }


    public void Serialize(object obj, Stream stream, KASerializer serializer, KAProperty prop)
    {
        byte[] bytes = Tools.GetBytes((string)obj);
        serializer.SerializeByteArray4(bytes, stream);
    }
}


public class CharSerializer : KADataSerializer
{
    public Type type => typeof(char);
    public object Deserialize(Stream stream, KASerializer serializer, Type type, KAProperty prop)
    {
        byte[] arr = new byte[2];
        stream.ReadExactly(arr);

        return BitConverter.ToChar(arr);
    }


    public void Serialize(object obj, Stream stream, KASerializer serializer, KAProperty prop)
    {
        stream.Write(BitConverter.GetBytes((char)obj));
    }
}
