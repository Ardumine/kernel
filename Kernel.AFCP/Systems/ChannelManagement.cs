using Kernel.AFCP.Packets;

namespace Kernel.AFCP.Systems;

public class ChannelManagementSystem
{
    private ChannelManager _channelManager;
    private KernelManager _kernelManager;


    public ChannelManagementSystem(ChannelManager channelManager, KernelManager kernelManager)
    {
        _channelManager = channelManager;
        _kernelManager = kernelManager;

    }
    public BasePacketAnswer Process(PacketChannelManagementRequest a)
    {
        //When another kernel requests to notify when a channel changes it's data.
        if (a.MsgType == ChannelMessageSyncMessageTypes.AddEventChannel)
        {
            var remoteKernel = _kernelManager.GetKernel(a.RequestingKernel);
            if (remoteKernel == null)
            {
                throw new Exception($"ChannelManagementSystem has received a call to add event to channel, but the kernel {a.RequestingKernel} is not registred");
            }
            DataChannelDescriptor? channel = _channelManager.TryGetLocalChannel(a.ChannelPath);

            channel?.AddNotifyToKernelOnChange(remoteKernel);
        }
        else if (a.MsgType == ChannelMessageSyncMessageTypes.RemoveEventChannel)
        {
            var remoteKernel = _kernelManager.GetKernel(a.RequestingKernel);
            if (remoteKernel == null)
            {
                throw new Exception($"ChannelManagementSystem has received a call to add event to channel, but the kernel {a.RequestingKernel} is not registred");
            }
            DataChannelDescriptor? channel = _channelManager.TryGetLocalChannel(a.ChannelPath);

            channel?.RemoveNotifyToKernelOnChange(remoteKernel);
        }
        else if (a.MsgType == ChannelMessageSyncMessageTypes.ChannelDataChange)
        {
            //Console.WriteLine($"Event rec!!! {a.NewVal == null}");

            //When a remote kernel notifys when a remote channel changes it's data.
            _channelManager.OnRemoteDataChange(a.ChannelPath, a.NewVal);
        }

        return new PacketChannelManagementAnswer();
    }
}