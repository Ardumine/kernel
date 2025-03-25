using Kernel.AFCP.KernelManagement;
using Kernel.AFCP.Packets;

namespace Kernel.AFCP.Systems;
public class ConnectSystem
{
    public required ChannelManagerAfcpClientConector channelManagerClient { get; set; }
    public required LocalRunningKernel LocalKernel { get; init; }
    public required KernelManager KernelManager { get; init; }


    public BasePacketAnswer Process(PacketConnectRequest a)
    {
        if (a.Disconnect)
        {
            var kernel = KernelManager.GetKernel(a.SourceKernel);

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
            KernelManager.OnRemoteKernelConnect(a.SourceKernel, channelManagerClient);


            //When a remote kernel connects to us 
            return new PacketConnectAnswer()
            {
                RemoteKernelGuid = LocalKernel.Guid
            };
        }

    }
}