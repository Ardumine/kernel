using System.Text;
using System.Text.Json;
using AFCP;
using AFCP.DataTreatment;
using AFCP.Packets;

namespace AFCP.Systems;
public class ChannelManagementSystem
{
    ChannelManager channelManager;

    public ChannelManagementSystem(ChannelManager _channelManager)
    {
        channelManager = _channelManager;
    }
    public BasePacketAnswer Process(PacketChannelManagementRequest a)
    {
        //When another kernel requests to notify when a channel changes it's data.
        if (a.MsgType == ChannelMessageSyncMessageTypes.AddEventChannel)
        {
            DataChannel? channel = channelManager.TryGetLocalChannel(a.ChannelPath);
            channel?.AddNotifyToKernelOnChange(a.RequestingKernel);
        }
        else if (a.MsgType == ChannelMessageSyncMessageTypes.RemoveEventChannel)
        {
            DataChannel? channel = channelManager.TryGetLocalChannel(a.ChannelPath);
            channel?.RemoveNotifyToKernelOnChange(a.RequestingKernel);
        }
        else if (a.MsgType == ChannelMessageSyncMessageTypes.ChannelDataChange)
        {
            //When a remote kernel notifys when a remote channel changes it's data.
            //DataChannel? channel = channelManager.GetDataChannel();
            //var interf = channelManager.GetInterfaceForChannel(a.ChannelPath);

            object? myData = a.NewVal is JsonElement jsonElement
              ? jsonElement.Deserialize(Type.GetType(a?.DataType!)!, DataWritter.JsonOptions)
             : Convert.ChangeType(a.NewVal, Type.GetType(a?.DataType!)!);
            channelManager.OnRemoteDataChange(a!.ChannelPath, myData);

            //interf?.OnRemoteChange(myData);
        }

        return new PacketChannelManagementAnswer();
    }
}