using Kernel.AFCP.Packets;

namespace Kernel.AFCP.Systems;
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
            //Console.WriteLine("Add event!!!!");
            DataChannelDescriptor? channel = channelManager.TryGetLocalChannel(a.ChannelPath);
            channel?.AddNotifyToKernelOnChange(channelManager.GetKernel(a.RequestingKernel)!);
        }
        else if (a.MsgType == ChannelMessageSyncMessageTypes.RemoveEventChannel)
        {
            DataChannelDescriptor? channel = channelManager.TryGetLocalChannel(a.ChannelPath);
            channel?.RemoveNotifyToKernelOnChange(channelManager.GetKernel(a.RequestingKernel)!);
        }
        else if (a.MsgType == ChannelMessageSyncMessageTypes.ChannelDataChange)
        {
            //Console.WriteLine($"Event rec!!! {a.NewVal == null}");
            //When a remote kernel notifys when a remote channel changes it's data.
            //DataChannel? channel = channelManager.GetDataChannel();
            //var interf = channelManager.GetInterfaceForChannel(a.ChannelPath);


            channelManager.OnRemoteDataChange(a.ChannelPath, a.NewVal);

            //interf?.OnRemoteChange(myData);
        }

        return new PacketChannelManagementAnswer();
    }
}