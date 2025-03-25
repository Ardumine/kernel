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
    public BasePacketAnswer Process(PacketChannelManagementRequest req)
    {
        //When another kernel requests to notify when a channel changes it's data.
        if (req.MsgType == ChannelMessageSyncMessageTypes.AddEventChannel)
        {
            //Console.WriteLine($"Remote kernel {req.RequestingKernel} requested add event to {req.ChannelPath}");
            var remoteKernel = _kernelManager.GetKernel(req.RequestingKernel);
            if (remoteKernel == null)
            {
                throw new Exception($"ChannelManagementSystem has received a call to add event to channel, but the kernel {req.RequestingKernel} is not registred");
            }
            DataChannelDescriptor? channel = _channelManager.TryGetLocalChannelDescriptor(req.ChannelPath);

            channel?.AddNotifyToKernelOnChange(remoteKernel);
        }
        else if (req.MsgType == ChannelMessageSyncMessageTypes.RemoveEventChannel)
        {
            var remoteKernel = _kernelManager.GetKernel(req.RequestingKernel);
            if (remoteKernel == null)
            {
                throw new Exception($"ChannelManagementSystem has received a call to add event to channel, but the kernel {req.RequestingKernel} is not registred");
            }
            DataChannelDescriptor? channel = _channelManager.TryGetLocalChannelDescriptor(req.ChannelPath);

            channel?.RemoveNotifyToKernelOnChange(remoteKernel);
        }
        else if (req.MsgType == ChannelMessageSyncMessageTypes.ChannelDataChange)
        {
            //Console.WriteLine($"Event rec!!! {req.NewVal == null}");

            //When a remote kernel notifys when a remote channel changes it's data.
            _channelManager.OnRemoteDataChange(req.ChannelPath, req.NewVal);
        }

        return new PacketChannelManagementAnswer();
    }
}