namespace Kernel.AFCP.Packets;

public class PacketChannelRequest : PacketBaseRequest
{
    public bool WriteData { get; set; }
    public required string channelPath { get; set; }

    public object? Val { get; set; }

}

public class PacketChannelAnswer : BasePacketAnswer
{
    public required object? Result { get; set; }
}
