
using System;
using System.IO;

namespace Kernel.AFCP.KASerializer.Serializers.NumericSerliazer;

public class IntSerializer : ISubSerializer
{
    public Type[] Types => [typeof(int)];

    public void Serialize(object obj, Stream stream, KASerializer serializer, KAType type)
    {
        stream.Write(Tools.GetBytes((int)obj));
    }
    public object Deserialize(Stream stream, KASerializer serializer, KAType type)
    {
        byte[] bytes = serializer.ReadByteArray(4, stream);
        return Tools.GetInt(bytes);
    }
}
public class UIntSerializer : ISubSerializer
{
    public Type[] Types => [typeof(uint)];


    public void Serialize(object obj, Stream stream, KASerializer serializer, KAType type)
    {
        stream.Write(Tools.GetBytes((uint)obj));
    }
    public object Deserialize(Stream stream, KASerializer serializer, KAType type)
    {
        byte[] bytes = serializer.ReadByteArray(4, stream);
        return Tools.GetUInt(bytes);
    }


}





public class FloatSerializer : ISubSerializer
{
    public Type[] Types => [typeof(float), typeof(Single)];


    public void Serialize(object obj, Stream stream, KASerializer serializer, KAType type)
    {
        stream.Write(Tools.GetBytes((float)obj));
    }
    public object Deserialize(Stream stream, KASerializer serializer, KAType type)
    {
        byte[] bytes = serializer.ReadByteArray(4, stream);
        return Tools.GetFloat(bytes);
    }


}
public class DoubleSerializer : ISubSerializer
{
    public Type[] Types => [typeof(double), typeof(Double)];


    public void Serialize(object obj, Stream stream, KASerializer serializer, KAType type)
    {
        stream.Write(Tools.GetBytes((double)obj));
    }
    public object Deserialize(Stream stream, KASerializer serializer, KAType type)
    {
        byte[] bytes = serializer.ReadByteArray(8, stream);
        return Tools.GetDouble(bytes);
    }


}



public class BoolSerializer : ISubSerializer
{
    public Type[] Types => [typeof(bool), typeof(Boolean)];


    public void Serialize(object obj, Stream stream, KASerializer serializer, KAType type)
    {
        stream.WriteByte(((bool)obj) ? (byte)1 : (byte)0);
    }
    public object Deserialize(Stream stream, KASerializer serializer, KAType type)
    {
        return serializer.ReadByteArray(1, stream)[0] == 1;
    }


}

