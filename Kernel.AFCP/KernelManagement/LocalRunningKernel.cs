using Kernel.AFCP.Packets;

namespace Kernel.AFCP.KernelManagement;

/// <summary>
/// An interface to the local kernel
/// </summary>
public class LocalRunningKernel : RunningKernel
{
    public static LocalRunningKernel CreateLocalKernelDescriptor()
    {
        return new LocalRunningKernel()
        {
            Guid = Guid.NewGuid(),
            IsLocal = true
        };
    }

    public static LocalRunningKernel CreateLocalKernelDescriptor(string guid)
    {
        return new LocalRunningKernel()
        {
            Guid = Guid.Parse(guid),
            IsLocal = true
        };
    }

    public override T? SendRequest<T>(byte msgType, PacketBaseRequest Payload) where T : class
    {
        throw new NotImplementedException($"Tried to send an request to the local kernel. Message type: {msgType}");
    }
}