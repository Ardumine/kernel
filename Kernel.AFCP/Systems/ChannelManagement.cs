using Kernel.AFCP.Packets;

namespace Kernel.AFCP.Systems;

public class ChannelManagementSystem
{
    private KernelManager _kernelManager;


    public ChannelManagementSystem(  KernelManager kernelManager)
    {
        _kernelManager = kernelManager;
    }

    public BasePacketAnswer Process(PacketChannelManagementRequest req)
    {
        throw new NotImplementedException("Removed data channels from the kernel code");
    }
}