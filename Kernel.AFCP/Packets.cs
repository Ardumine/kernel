using Kernel.AFCP.DataTreatment;
using Kernel.AFCP.KASerializer.Atributes;

namespace Kernel.AFCP.Packets;


public class PacketBaseRequest
{

}

public class PacketConnectRequest : PacketBaseRequest
{
    public Guid SourceKernel { get; set; }
    //public List<DataChannelDescriptor> HostingChannels { get; init; }
    //public List<ModuleDescriptor> HostingModules { get; init; }
    public bool Disconnect { get; set; }//Is this a disconnect request? This is so we dont create another packet type just to do the basically same thing.

}


public class PacketChannelRequest : PacketBaseRequest
{
    public bool WriteData { get; set; }
    public required string channelPath { get; set; }

    public object? Val { get; set; }



}
public class PacketFunctionRequest : PacketBaseRequest
{
    public required string ChannelPath { get; set; }
    public uint FuncID { get; set; }


    public object?[]? Params { get; set; }




}

public class PacketSyncRequest : PacketBaseRequest
{
    public required Guid SourceKernelGuid { get; set; }

    public required ChannelDescriptor[] SourceHostingDataChannels { get; set; }

}

public class PacketChannelManagementRequest : PacketBaseRequest
{
    public required ChannelMessageSyncMessageTypes MsgType { get; set; }

    public required Guid RequestingKernel { get; set; }

    public required string ChannelPath { get; set; }

    public object? NewVal { get; set; }


}


public enum ChannelMessageSyncMessageTypes
{
    AddEventChannel, //Make the sending kernel receive an event when a channel changes its data
    RemoveEventChannel, //Make the remote kernel stop notify when a channel data change.
    ChannelDataChange,//When a kernel changes its data in a channel and notifys another kernels. 
}

public class BasePacketAnswer
{

}


public class PacketConnectAnswer : BasePacketAnswer
{
    public Guid RemoteKernelGuid { get; set; }
}

public class PacketChannelAnswer : BasePacketAnswer
{
    public required object? Result { get; set; }
}

public class PacketSyncAnswer : BasePacketAnswer
{
    public required Guid RemoteGuid { get; set; }
    public required ChannelDescriptor[] RemoteHostingChannels { get; set; }
}

public class PacketFunctionAnswer : BasePacketAnswer
{
    public object? Out { get; set; }
}

public class PacketChannelManagementAnswer : BasePacketAnswer
{
    public object? Out { get; set; }
}
