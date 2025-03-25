namespace Kernel.AFCP.Packets;
public class PacketConnectRequest : PacketBaseRequest
{
    public Guid SourceKernel { get; set; }
    //public List<DataChannelDescriptor> HostingChannels { get; init; }
    //public List<ModuleDescriptor> HostingModules { get; init; }
    public bool Disconnect { get; set; }//Is this a disconnect request? This is so we dont create another packet type just to do the basically same thing.

}

public class PacketConnectAnswer : BasePacketAnswer
{
    public Guid RemoteKernelGuid { get; set; }
}