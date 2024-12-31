using AFCP;
using AFCP.Packets;

namespace AFCP.Systems;
public class ConnectSystem
{
    ChannelManager channelManager;
    ChannelManagerClient channelManagerClient;

    public ConnectSystem(ChannelManager _channelManager, ChannelManagerClient _channelManagerClient)
    {
        channelManager = _channelManager;
        channelManagerClient = _channelManagerClient;
    }
    public BasePacketAnswer Process(PacketConnectRequest a)
    {

        if (a.Disconnect)
        {
            new Thread(() =>
            {
                Thread.Sleep(100);//Let have some time to deliver
                channelManagerClient.ServerStop(a.RemoteKernel);


            }).Start();
            return new PacketConnectAnswer()
            {
                RemoteKernel = channelManager.LocalGuid
            };

        }
        else
        {

            channelManager.ConnectedKernels.Add(new KernelDescriptor()
            {
                KernelGuid = a.RemoteKernel,
                channelManagerClient = channelManagerClient
            });
            channelManagerClient.RemoteGuid = a.RemoteKernel;

            return new PacketConnectAnswer()
            {
                RemoteKernel = channelManager.LocalGuid
            };
        }

    }
}