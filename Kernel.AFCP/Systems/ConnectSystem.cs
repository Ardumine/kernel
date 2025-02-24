using Kernel.AFCP.Packets;

namespace Kernel.AFCP.Systems;
public class ConnectSystem
{
    public required ChannelManager channelManager { get; init; }
    public required ChannelManagerAfcpClientConector channelManagerClient { get; set; }
    public required KernelDescriptor LocalKernel { get; init; }


    public BasePacketAnswer Process(PacketConnectRequest a)
    {

        if (a.Disconnect)
        {
           // new Thread(() =>
           // {
            //    Thread.Sleep(100);//Let have some time to deliver
                channelManagerClient.OnRemoteDisconnect(a.RemoteKernel);

            //}).Start();
            return new PacketConnectAnswer()
            {
                RemoteKernel = LocalKernel.Guid
            };
        }
        else
        {

            channelManager.ConnectedKernels.Add(new KernelDescriptor()
            {
                Guid = a.RemoteKernel,
                ConnectedVia = channelManagerClient,

            });

            return new PacketConnectAnswer()
            {
                RemoteKernel = LocalKernel.Guid
            };
        }

    }
}