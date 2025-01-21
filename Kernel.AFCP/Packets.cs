using Kernel.AFCP.DataTreatment;
using Kernel.AFCP.KASerializer.Atributes;

namespace Kernel.AFCP.Packets;


public interface PacketBaseRequest
{

}

public class PacketConnectRequest : PacketBaseRequest
{
    public Guid RemoteKernel { get; set; }
    //public List<DataChannelDescriptor> HostingChannels { get; init; }
    //public List<ModuleDescriptor> HostingModules { get; init; }
    public bool Disconnect { get; set; }//Is this a disconnect request? This is so we dont create another packet type just to do the basically same thing.

}


public class PacketChannelRequest : PacketBaseRequest
{
    public bool WriteData { get; set; }
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

    [CanHaveOtherTypes]
    public required ChannelDescriptor[] RemoteChannels { get; set; }

}

public class PacketChannelManagementRequest : PacketBaseRequest
{
    public ChannelMessageSyncMessageTypes MsgType { get; set; }

    public Guid RequestingKernel { get; set; }

    public required string ChannelPath { get; set; }

    [CanHaveOtherTypes]
    public object? NewVal { get; set; }


}


public enum ChannelMessageSyncMessageTypes
{
    AddEventChannel, //Make the sending kernel receive an event when a channel changes its data
    RemoveEventChannel, //Make the remote kernel stop notify when a channel data change.
    ChannelDataChange,//When a kernel changes its data in a channel and notifys another kernels. 
}

public interface BasePacketAnswer
{

}


public class PacketConnectAnswer : BasePacketAnswer
{
    public Guid RemoteKernel { get; set; }
    //public List<DataChannelDescriptor> HostingChannels { get; init; }
    //public List<ModuleDescriptor> HostingModules { get; init; }

}

public class PacketChannelAnswer : BasePacketAnswer
{
    [CanHaveOtherTypes]
    public required object? Result { get; set; }
}

public class PacketSyncAnswer : BasePacketAnswer
{
    public Guid RemoteGuid { get; set; }
    [CanHaveOtherTypes]
    public required ChannelDescriptor[] RemoteChannels { get; set; }
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
