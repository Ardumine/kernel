using Kernel.AFCP.Packets;

namespace Kernel.AFCP.Systems;
public class ConnectSystem
{
    public required ChannelManager channelManager { get; init; }
    public required ChannelManagerAfcpClientConector channelManagerClient { get; set; }
    public required KernelDescriptor LocalKernel { get; init; }
    public required KernelManager KernelManager { get; init; }


    public BasePacketAnswer Process(PacketConnectRequest a)
    {
        if (a.Disconnect)
        {
            // new Thread(() =>
            // {
            //    Thread.Sleep(100);//Let have some time to deliver
            KernelManager.OnRemoteKernelRequestDiconnect(KernelManager.GetKernel(a.SourceKernel));

            //}).Start();
            return new PacketConnectAnswer()
            {
                RemoteKernelGuid = LocalKernel.Guid
            };
        }
        else
        {
            KernelManager.OnRemoteKernelConnect(a.SourceKernel, channelManagerClient);


            //When a remote kernel connects to us 
            return new PacketConnectAnswer()
            {
                RemoteKernelGuid = LocalKernel.Guid
            };
        }

    }
}