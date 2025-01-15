using System;
using System.IO;
using System.Numerics;

namespace Kernel.AFCP.KASerializer.Serializers.VectorSerializer;
public class Vector2Serializer : ISubSerializer
{
    public Type[] Types => [typeof(Vector2)];

    //https://stackoverflow.com/questions/33292204/copying-a-system-guid-to-byte-without-allocating
    public unsafe void Serialize(object obj, Stream stream, KASerializer serializer, KAType type)
    {
        var vec = (Vector2)obj;

        byte[] bytes = new byte[8];//4 + 4
        fixed (byte* p = bytes)
        {
            float* f = (float*)p;
            f[0] = vec.X;
            f[1] = vec.Y;
        }
        stream.Write(bytes);

    }
    public unsafe object Deserialize(Stream stream, KASerializer serializer, KAType type)
    {
        byte[] arr = serializer.ReadByteArray(8, stream);
        var vec = new Vector2();

        fixed (byte* p = arr)
        {
            float* f = (float*)p;
            vec[0] = f[0];
            vec[1] = f[1];
        }
        return vec;
    }


}


public class Vector3Serializer : ISubSerializer
{
    public Type[] Types => [typeof(Vector3)];

    //https://stackoverflow.com/questions/33292204/copying-a-system-guid-to-byte-without-allocating
    public unsafe void Serialize(object obj, Stream stream, KASerializer serializer, KAType type)
    {
        var vec = (Vector3)obj;

        byte[] bytes = new byte[12];//4 + 4 + 4
        fixed (byte* p = bytes)
        {
            float* f = (float*)p;
            f[0] = vec.X;
            f[1] = vec.Y;
            f[2] = vec.Z;

        }
        stream.Write(bytes);

    }
    public unsafe object Deserialize(Stream stream, KASerializer serializer, KAType type)
    {
        byte[] arr = serializer.ReadByteArray(12, stream);
        var vec = new Vector3();

        fixed (byte* p = arr)
        {
            float* f = (float*)p;
            vec.X = f[0];
            vec.Y = f[1];
            vec.Z = f[2];
        }
        return vec;
    }


}

