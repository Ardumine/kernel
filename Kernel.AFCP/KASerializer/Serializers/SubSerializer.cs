using System;
using System.IO;

using System.Collections;
using System.Collections.Generic;

namespace Kernel.AFCP.KASerializer.Serializers;


public class GuidSerializer : ISubSerializer
{
    public Type[] Types => [typeof(Guid)];

    //https://stackoverflow.com/questions/33292204/copying-a-system-guid-to-byte-without-allocating
    public unsafe void Serialize(object obj, Stream stream, KASerializer serializer, KAType type)
    {
        var guid = (Guid)obj;

        byte[] bytes = new byte[16];
        fixed (byte* pArray = bytes)
        {
            var pGuid = (long*)&guid;
            var pDest = (long*)pArray;
            pDest[0] = pGuid[0];
            pDest[1] = pGuid[1];
        }

        stream.Write(bytes);
    }
    public object Deserialize(Stream stream, KASerializer serializer, KAType type)
    {
        byte[] bytes = serializer.ReadByteArray(16, stream);
        return new Guid(bytes);
    }


}

public interface ISubSerializer
{
    public Type[] Types { get; }
    public void Serialize(object obj, Stream stream, KASerializer serializer, KAType type);
    public object Deserialize(Stream stream, KASerializer serializer, KAType type);
}