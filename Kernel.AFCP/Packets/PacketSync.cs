namespace Kernel.AFCP.Packets;

public class PacketSyncRequest : PacketBaseRequest
{
    public required Guid SourceKernelGuid { get; set; }

    public required ChannelDescriptor[] SourceHostingDataChannels { get; set; }
}



public class PacketSyncAnswer : BasePacketAnswer
{
    public required Guid RemoteGuid { get; set; }
    public required ChannelDescriptor[] RemoteHostingChannels { get; set; }
}
