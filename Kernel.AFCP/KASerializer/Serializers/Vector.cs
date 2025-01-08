using System.Numerics;

namespace Kernel.AFCP.KASerializer.Serializers.Vector;

public class Vector2Serializer : KADataSerializer
{
    public Type type => typeof(Vector2);
    public unsafe object Deserialize(Stream stream, KASerializer serializer, Type type, KAProperty prop)
    {
        byte[] arr = new byte[8];
        var vec = new Vector2();
        stream.ReadExactly(arr);
        fixed (byte* p = arr)
        {
            float* f = (float*)p;
            vec[0] = f[0];
            vec[1] = f[1];
        }
        return  vec;
    }


    public unsafe void Serialize(object obj, Stream stream, KASerializer serializer, KAProperty prop)
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
}

public class Vector3Serializer : KADataSerializer
{
    public Type type => typeof(Vector3);
    public unsafe object Deserialize(Stream stream, KASerializer serializer, Type type, KAProperty prop)
    {
        byte[] arr = new byte[12];
        var vec = new Vector3();
        stream.ReadExactly(arr);
        fixed (byte* p = arr)
        {
            float* f = (float*)p;
            vec[0] = f[0];
            vec[1] = f[1];
            vec[2] = f[2];
        }
        return  vec;
    }


    public unsafe void Serialize(object obj, Stream stream, KASerializer serializer, KAProperty prop)
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
}