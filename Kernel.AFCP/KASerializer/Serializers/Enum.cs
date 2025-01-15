using System;
using System.IO;
namespace Kernel.AFCP.KASerializer.Serializers.EnumSerializer;

public class EnumSerializer
{
    public void Serialize(object obj, Stream stream)
    {
        stream.Write(Tools.GetBytes((int)obj));
    }
    public object Deserialize(Stream stream, KASerializer serializer, Type dataType)
    {
        byte[] bytes = serializer.ReadByteArray(4, stream);
        return Enum.ToObject(dataType, Tools.GetInt(bytes));
    }
}
