namespace Kernel.AFCP.Packets;

public class PacketChannelManagementRequest : PacketBaseRequest
{
    public required ChannelMessageSyncMessageTypes MsgType { get; set; }

    public required Guid RequestingKernel { get; set; }

    public required string ChannelPath { get; set; }

    public object? NewVal { get; set; }
}


public class PacketChannelManagementAnswer : BasePacketAnswer
{
    //public object? Out { get; set; }
}

public enum ChannelMessageSyncMessageTypes
{
    AddEventChannel, //Make the sending kernel receive an event when a channel changes its data
    RemoveEventChannel, //Make the remote kernel stop notify when a channel data change.
    ChannelDataChange,//When a kernel changes its data in a channel and notifys another kernels. 
}
