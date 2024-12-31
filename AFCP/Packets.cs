using System.Text.Json.Serialization;
using AFCP.DataTreatment;
using AFCP.JsonConverters;

namespace AFCP.Packets;


[JsonDerivedType(typeof(PacketConnectRequest), typeDiscriminator: "1")]
[JsonDerivedType(typeof(PacketChannelRequest), typeDiscriminator: "2")]
[JsonDerivedType(typeof(PacketSyncRequest), typeDiscriminator: "3")]
[JsonDerivedType(typeof(PacketFunctionRequest), typeDiscriminator: "4")]
[JsonDerivedType(typeof(PacketChannelManagementRequest), typeDiscriminator: "5")]
public abstract class PacketBaseRequest
{
    public abstract void Serialize(DataWritter writter);

    public abstract void Deserialize(DataReader reader);

}

public class PacketConnectRequest : PacketBaseRequest
{
    public Guid RemoteKernel { get; set; }
    public bool Disconnect { get; set; }//Is this a disconnect request? This is so we dont create another packet type just to do the basically same thing.


    public override void Serialize(DataWritter writter)
    {
        writter.Write(RemoteKernel);
        writter.Write(Disconnect);
    }
    public override void Deserialize(DataReader reader)
    {
        RemoteKernel = reader.ReadGuid();
        Disconnect = reader.ReadBool();
    }


}


public class PacketChannelRequest : PacketBaseRequest
{
    public byte Type { get; set; }
    public required string channelPath { get; set; }

    [JsonConverter(typeof(ObjectToInferredTypesConverter))]
    public object? Val { get; set; }


    public override void Serialize(DataWritter writter)
    {
        writter.Write(Type);
        writter.Write(channelPath);
        writter.WriteObject(Val);
    }
    public override void Deserialize(DataReader reader)
    {
        Type = reader.ReadByte();
        channelPath = reader.ReadString();
        Val = reader.ReadObject<object>();
    }
}
public class PacketFunctionRequest : PacketBaseRequest
{
    public required string ChannelPath { get; set; }
    public uint FuncID { get; set; }

    [JsonConverter(typeof(ObjectToInferredTypesConverter))]

    public object?[]? Params { get; set; }



    public override void Serialize(DataWritter writter)
    {
        writter.Write(ChannelPath);
        writter.Write(FuncID);
        writter.WriteObject(Params);
    }

    public override void Deserialize(DataReader reader)
    {
        ChannelPath = reader.ReadString();
        FuncID = reader.ReadUint();
        Params = reader.ReadObject<object?[]?>();
    }

}

public class PacketSyncRequest : PacketBaseRequest
{
    public Guid RemoteGuid { get; set; }
    public required ChannelDescriptor[] RemoteChannels { get; set; }


    public override void Serialize(DataWritter writter)
    {
        writter.Write(RemoteGuid);
        writter.WriteObject(RemoteChannels);
    }


    public override void Deserialize(DataReader reader)
    {
        RemoteGuid = reader.ReadGuid();
        RemoteChannels = reader.ReadObject<ChannelDescriptor[]>()!;
    }
}

public class PacketChannelManagementRequest : PacketBaseRequest
{
    public ChannelMessageSyncMessageTypes MsgType { get; set; }

    public Guid RequestingKernel { get; set; }

    public required string ChannelPath { get; set; }

    [JsonConverter(typeof(ObjectToInferredTypesConverter))]
    public object? NewVal { get; set; }

    public string? DataType { get; set; }


    public override void Serialize(DataWritter writter)
    {
        writter.Write((int)MsgType);
        writter.Write(RequestingKernel);
        writter.Write(ChannelPath);
        writter.Write(DataType!);
        writter.WriteObject(NewVal);

    }

    public override void Deserialize(DataReader reader)
    {
        MsgType = (ChannelMessageSyncMessageTypes)reader.ReadInt();
        RequestingKernel = reader.ReadGuid();
        ChannelPath = reader.ReadString();
        var t = reader.ReadString();
        NewVal = reader.ReadObject(Type.GetType(t)!);
    }
}


public enum ChannelMessageSyncMessageTypes
{
    AddEventChannel, //Make the sending kernel receive an event when a channel changes its data
    RemoveEventChannel, //Make the remote kernel stop notify when a channel data change.
    ChannelDataChange//When a kernel changes its data in a channel and notifys another kernels. 
}

[JsonDerivedType(typeof(PacketConnectAnswer), typeDiscriminator: "1")]
[JsonDerivedType(typeof(PacketChannelAnswer), typeDiscriminator: "2")]
[JsonDerivedType(typeof(PacketSyncAnswer), typeDiscriminator: "3")]
[JsonDerivedType(typeof(PacketFunctionAnswer), typeDiscriminator: "4")]
[JsonDerivedType(typeof(PacketChannelManagementAnswer), typeDiscriminator: "5")]
public class BasePacketAnswer
{

}


public class PacketConnectAnswer : BasePacketAnswer
{
    public Guid RemoteKernel { get; set; }
}

public class PacketChannelAnswer : BasePacketAnswer
{
    [JsonConverter(typeof(ObjectToInferredTypesConverter))]
    public object? Result { get; set; }
}

public class PacketSyncAnswer : BasePacketAnswer
{
    public Guid RemoteGuid { get; set; }
    public required ChannelDescriptor[] RemoteChannels { get; set; }
}

public class PacketFunctionAnswer : BasePacketAnswer
{
    [JsonConverter(typeof(ObjectToInferredTypesConverter))]
    public object? Out { get; set; }
}

public class PacketChannelManagementAnswer : BasePacketAnswer
{
    [JsonConverter(typeof(ObjectToInferredTypesConverter))]
    public object? Out { get; set; }
}
