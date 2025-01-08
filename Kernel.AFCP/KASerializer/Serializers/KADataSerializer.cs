namespace Kernel.AFCP.KASerializer.Serializers;

public interface KADataSerializer
{
    public Type type { get; }
    public void Serialize(object obj, Stream stream, KASerializer serializer, KAProperty prop);
    public object Deserialize(Stream stream, KASerializer serializer, Type type, KAProperty prop);
}


