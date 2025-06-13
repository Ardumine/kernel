namespace Kernel.AFCP.Packets;

public class PacketBaseRequest
{
    public required Guid SourceKernelGuid { get; set; }
}

public class BasePacketAnswer
{
    public required Guid RemoteKernelGuid { get; set; }

}