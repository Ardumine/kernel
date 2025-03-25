using Kernel.AFCP.Packets;

namespace Kernel.AFCP.KernelManagement;

/// <summary>
/// It's a kernel that exists
/// </summary>
public abstract class RunningKernel : KernelDescriptor
{
    public abstract T? SendRequest<T>(byte RequestType, PacketBaseRequest Payload) where T : BasePacketAnswer;
}
