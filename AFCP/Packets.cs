using AFCP.DataTreatment;
using AFCP.KASerializer.Atributes;

namespace AFCP.Packets;

public class PacketBaseRequestAK
{
    [CanHaveOtherTypes]
    public PacketBaseRequest req { get; set; }
}

public class PacketBaseRequest
{

}

public class PacketConnectRequest : PacketBaseRequest
{
    public Guid RemoteKernel { get; set; }
    public bool Disconnect { get; set; }//Is this a disconnect request? This is so we dont create another packet type just to do the basically same thing.

}


public class PacketChannelRequest : PacketBaseRequest
{
    public byte Type { get; set; }
    public required string channelPath { get; set; }

    [CanHaveOtherTypes]
    public object? Val { get; set; }



}
public class PacketFunctionRequest : PacketBaseRequest
{
    public required string ChannelPath { get; set; }
    public uint FuncID { get; set; }

    [CanHaveOtherTypes]

    public object?[]? Params { get; set; }




}

public class PacketSyncRequest : PacketBaseRequest
{
    public Guid RemoteGuid { get; set; }
    public required List<ChannelDescriptor> RemoteChannels { get; set; }

}

public class PacketChannelManagementRequest : PacketBaseRequest
{
    public byte MsgType { get; set; }//ChannelMessageSyncMessageTypes

    public Guid RequestingKernel { get; set; }

    public required string ChannelPath { get; set; }

    [CanHaveOtherTypes]
    public object? NewVal { get; set; }

    public string? DataType { get; set; }

}


public static class ChannelMessageSyncMessageTypes
{
    public static byte AddEventChannel => 0; //Make the sending kernel receive an event when a channel changes its data
    public static byte RemoveEventChannel => 1; //Make the remote kernel stop notify when a channel data change.
    public static byte ChannelDataChange = 2;//When a kernel changes its data in a channel and notifys another kernels. 
}

public class BasePacketAnswerAK
{
    [CanHaveOtherTypes]
    public BasePacketAnswer ans {get;set;}
}

public class BasePacketAnswer
{

}


public class PacketConnectAnswer : BasePacketAnswer
{
    public Guid RemoteKernel { get; set; }
}

public class PacketChannelAnswer : BasePacketAnswer
{
    [CanHaveOtherTypes]
    public object? Result { get; set; }
}

public class PacketSyncAnswer : BasePacketAnswer
{
    public Guid RemoteGuid { get; set; }
    public required List<ChannelDescriptor> RemoteChannels { get; set; }
}

public class PacketFunctionAnswer : BasePacketAnswer
{
    [CanHaveOtherTypes]
    public object? Out { get; set; }
}

public class PacketChannelManagementAnswer : BasePacketAnswer
{
    [CanHaveOtherTypes]
    public object? Out { get; set; }
}
