using Kernel.AFCP.KernelManagement;
using Kernel.AFCP.Packets;

namespace Kernel.AFCP.Systems;
public class ConnectSystem
{
    public required ChannelManagerAfcpClientConector channelManagerClient { get; set; }
    public required LocalRunningKernel LocalKernel { get; init; }
    public required KernelManager KernelManager { get; init; }


    public BasePacketAnswer Process(PacketConnectRequest connectPacket)
    {
        if (connectPacket.Disconnect)
        {
            var kernel = KernelManager.GetKernel(connectPacket.SourceKernelGuid);

            if (kernel != null)
            {
                if(kernel is RemoteRunningKernel remoteKernel)
                    KernelManager.OnRemoteKernelRequestDiconnect(remoteKernel);
            }

            return new PacketConnectAnswer()
            {
                RemoteKernelGuid = LocalKernel.Guid
            };
        }
        else
        {
            KernelManager.OnRemoteKernelConnect(connectPacket.SourceKernelGuid, channelManagerClient);


            //When a remote kernel connects to us 
            return new PacketConnectAnswer()
            {
                RemoteKernelGuid = LocalKernel.Guid
            };
        }

    }
}