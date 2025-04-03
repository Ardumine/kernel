namespace Kernel.AFCP.KernelManagement;

public class KernelDescriptor
{

    public required Guid Guid { get; init; }
    public required bool IsLocal { get; init; }
    public override string ToString(){
        return Guid.ToString();
    }


}