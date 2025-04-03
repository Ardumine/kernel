namespace Kernel.AFCP.Packets;

public class PacketSyncRequest : PacketBaseRequest
{
    public required ChannelDescriptor[] SourceHostingDataChannels { get; set; }
}



public class PacketSyncAnswer : BasePacketAnswer
{
    public required ChannelDescriptor[] RemoteHostingChannels { get; set; }
}
