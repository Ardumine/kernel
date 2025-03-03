using Kernel.AFCP;
using Kernel.AFCP.Packets;

public class KernelDescriptor
{
    public static KernelDescriptor CreateLocalKernelDescriptor()
    {
        return new KernelDescriptor()
        {
            Guid = Guid.NewGuid()
        };
    }

    public static KernelDescriptor CreateLocalKernelDescriptor(string guid)
    {
        return new KernelDescriptor()
        {
            Guid = Guid.Parse(guid)
        };
    }

    public required Guid Guid { get; init; }
    public ConnectionState ConnectionState { get; set; }

    //public  List<ModuleDescriptor> HostingModules { get; } = new();
    //public List<ChannelDescriptor> HostingChannels { get; set; } = new();


}

/// <summary>
/// Its a kernel that we are connected to
/// </summary>
public class ConnectedKernel : KernelDescriptor
{
    public required ChannelManagerAfcpClientConector ConnectedVia { get; init; }

    public T SendRequest<T>(byte RequestType, PacketBaseRequest Payload) where T : BasePacketAnswer
    {
        return ConnectedVia.SendRequest<T>(RequestType, Payload);
    }
}

public enum ConnectionState
{
    WaitRemoteKernelSendHostingDataChannels
}