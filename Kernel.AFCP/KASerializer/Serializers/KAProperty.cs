using Kernel.AFCP.FastMethod;

namespace Kernel.AFCP.KASerializer.Serializers;

public struct KAProperty
{
    public string Name { get; set; }
    public Type type { get; set; }
    public FastMethodInfo GetMethod { get; set; }
    public FastMethodInfo SetMethod { get; set; }
    public  bool CanChangeType { get; set; }
    public  bool IsListOrArray { get; set; }

}