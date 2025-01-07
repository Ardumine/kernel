namespace AFCP.KASerializer.Serializers.Guid;


public class GuidSerializer : KADataSerializer
{
    public Type type => typeof(System.Guid);
    public object Deserialize(Stream stream, KASerializer serializer, Type type)
    {
        byte[] arr = new byte[16];
        stream.ReadExactly(arr);

        return new System.Guid(arr);
    }

    //https://stackoverflow.com/questions/33292204/copying-a-system-guid-to-byte-without-allocating
    public unsafe void Serialize(object obj, Stream stream, KASerializer serializer)
    {
        var guid = (System.Guid)obj;

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
}