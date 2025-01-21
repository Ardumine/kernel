namespace Kernel.AFCP.KASerializer.Serializers.StringSerializer;
public class StringSerializer : ISubSerializer
{
    public Type[] Types => [typeof(string), typeof(String)];


    public void Serialize(object obj, Stream stream, KASerializer serializer, KAType type)
    {
        serializer.WriteByteArray(Tools.GetBytes((string)obj), type.IsLongLength, stream);
    }
    public object Deserialize(Stream stream, KASerializer serializer, KAType type)
    {
        byte[] bytes = serializer.ReadByteArray(type.IsLongLength, stream);
        return Tools.GetString(bytes);
    }


}