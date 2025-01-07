using AFCP.FastMethod;

namespace AFCP.KASerializer.Serializers;

public struct KAProperty
{
    public required string Name { get; set; }
    public Type type { get; set; }
    public FastMethodInfo GetMethod { get; set; }
    public FastMethodInfo SetMethod { get; set; }
    public required bool CanChangeType { get; set; }
}