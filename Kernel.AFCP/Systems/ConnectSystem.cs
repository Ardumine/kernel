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
            var kernel = KernelManager.GetKernel(a.SourceKernel);

            if (kernel != null)
                KernelManager.OnRemoteKernelRequestDiconnect(kernel);

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