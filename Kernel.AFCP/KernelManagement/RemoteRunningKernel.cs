using Kernel.AFCP;
using Kernel.AFCP.Packets;

namespace Kernel.AFCP.KernelManagement;

/// <summary>
/// A remote kernel that we are connected to
/// </summary>
public class RemoteRunningKernel : RunningKernel
{
    public required ChannelManagerAfcpClientConector ConnectedVia { get; init; }



    public override T? SendRequest<T>(byte RequestType, PacketBaseRequest Payload) where T : class
    {
        return ConnectedVia.SendRequest<T>(RequestType, Payload);
    }

    public  void OnRemoteDisconnect()
    {
        ConnectedVia.OnRemoteDisconnect();
    }

    public  void RequestDisconect()
    {
        ConnectedVia.RequestDisconect();
    }
}