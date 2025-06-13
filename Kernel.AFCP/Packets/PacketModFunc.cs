namespace Kernel.AFCP.Packets;

public class PacketModFuncRequest : PacketBaseRequest
{
    public required string ChannelPath { get; set; }
    public uint FuncID { get; set; }

    public object?[]? Params { get; set; }
}

public class PacketModFuncAnswer : BasePacketAnswer
{
    public object? Out { get; set; }
}