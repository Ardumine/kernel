using Kernel.AFCP.Packets;

namespace Kernel.AFCP;

/// <summary>
/// Manages the kernels
/// </summary>
public class KernelManager
{
    public required ChannelManager LocalChannelManager { get; set; }
    public required KernelDescriptor LocalKernel { get; init; }

    private List<ConnectedKernel> _ConnectedKernels;
    public KernelManager()
    {
        _ConnectedKernels = new List<ConnectedKernel>();
    }




    /// <summary>
    /// When we want to connect to another kernel
    /// </summary>
    /// <param name="IPAddress"></param>
    /// <param name="port"></param>
    public void Join(string IPAddress, int port)
    {
        var afcpClient = new ChannelManagerAfcpClientConector(IPAddress, port, LocalChannelManager, LocalKernel, this);

        var connectResponse = afcpClient.SendRequest<PacketConnectAnswer>(MessagesTypes.ChannelConnectRequest, new PacketConnectRequest()
        {
            SourceKernel = LocalKernel.Guid,
            Disconnect = false
        });

        var remoteKernelGuid = connectResponse.RemoteKernelGuid;

        ConnectedKernel remoteKernel = new ConnectedKernel()
        {
            Guid = remoteKernelGuid,
            ConnectionState = ConnectionState.WaitRemoteKernelSendHostingDataChannels,
            ConnectedVia = afcpClient
        };
        afcpClient.RemoteKernel = remoteKernel;

        _ConnectedKernels.Add(remoteKernel);

        ExchangeDataChannels(remoteKernel);

        LocalChannelManager.Externals[remoteKernelGuid] = afcpClient;
    }
    public void Stop(){
        foreach (var connectedKernel in _ConnectedKernels){
            connectedKernel.ConnectedVia.RequestDisconect();
        }
    }

    /// <summary>
    /// When we are server
    /// </summary>
    /// <param name="RemoteKernelGuid"></param>
    internal void OnRemoteKernelConnect(Guid RemoteKernelGuid, ChannelManagerAfcpClientConector ConnectedVia)
    {
        ConnectedKernel remoteKernel = new ConnectedKernel()
        {
            Guid = RemoteKernelGuid,
            ConnectionState = ConnectionState.WaitRemoteKernelSendHostingDataChannels,
            ConnectedVia = ConnectedVia
        };
        ConnectedVia.RemoteKernel = remoteKernel;

        _ConnectedKernels.Add(remoteKernel);
    }


    private void ExchangeDataChannels(ConnectedKernel remoteKernel)
    {
        var packetSendChannels = new PacketSyncRequest()
        {
            SourceHostingDataChannels = LocalChannelManager.GetLocalChannelDescriptors(),
            SourceKernelGuid = LocalKernel.Guid
        };

        var answer = remoteKernel.SendRequest<PacketSyncAnswer>(MessagesTypes.ChannelSyncRequest, packetSendChannels);
        //remoteKernel.HostingChannels.AddRange(answer.RemoteHostingChannels);

        LocalChannelManager.AddChannelsSync(answer.RemoteHostingChannels.ToList(), remoteKernel);
    }

    public void OnSyncChannelsPacketReceived(List<ChannelDescriptor> RemoteChannels, ConnectedKernel remoteKernel)
    {
        LocalChannelManager.AddChannelsSync(RemoteChannels, remoteKernel);

    }


    public ConnectedKernel? GetKernel(Guid guid)
    {
        return _ConnectedKernels.Where(kernel => kernel.Guid == guid).FirstOrDefault();
    }



    /// <summary>
    /// To disconnect another kernel from this one
    /// </summary>
    /// <param name="kernel"></param>
    internal void DiconnectKernel(ConnectedKernel kernel)
    {

    }
    /// <summary>
    /// A remote kernel requested to disconnect
    /// </summary>
    internal void OnRemoteKernelRequestDiconnect(ConnectedKernel? kernel)
    {
        Console.WriteLine("Remote requested diconnect.");
        LocalChannelManager?.RemoveKernel(kernel);
        kernel?.ConnectedVia.OnRemoteDisconnect();

    }


}