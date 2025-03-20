using Kernel.AFCP.Packets;

namespace Kernel.AFCP;

/// <summary>
/// Manages the kernels
/// </summary>
public class KernelManager
{
    public required ChannelManager LocalChannelManager { get; set; }
    public required LocalRunningKernel LocalKernel { get; init; }

    private List<RunningKernel> _ConnectedKernels;
    public KernelManager()
    {
        _ConnectedKernels = new List<RunningKernel>();
    }

    /// <summary>
    /// When we want to connect to another kernel
    /// </summary>
    /// <param name="IPAddress">The IP of the remote kernel</param>
    /// <param name="port">The port of the remote kernel</param>
    public void Join(string IPAddress, int port)
    {
        var afcpClient = new ChannelManagerAfcpClientConector(IPAddress, port, LocalChannelManager, LocalKernel, this);

        var connectResponse = afcpClient.SendRequest<PacketConnectAnswer>(MessagesTypes.ChannelConnectRequest, new PacketConnectRequest()
        {
            SourceKernel = LocalKernel.Guid,
            Disconnect = false
        });

        if (connectResponse == null)
        {
            throw new NullReferenceException("The answer for Join was null");
        }
        var remoteKernelGuid = connectResponse.RemoteKernelGuid;

        RunningKernel remoteKernel = new RemoteRunningKernel()
        {
            Guid = remoteKernelGuid,
            ConnectedVia = afcpClient
        };
        afcpClient.RemoteKernel = remoteKernel;

        _ConnectedKernels.Add(remoteKernel);

        ExchangeDataChannels(remoteKernel);

    }

    public void Stop(){
        foreach (var connectedKernel in _ConnectedKernels){
            connectedKernel.RequestDisconect();
        }
    }

    /// <summary>
    /// When we are server
    /// </summary>
    /// <param name="RemoteKernelGuid"></param>
    internal void OnRemoteKernelConnect(Guid RemoteKernelGuid, ChannelManagerAfcpClientConector ConnectedVia)
    {
        RunningKernel remoteKernel = new RemoteRunningKernel()
        {
            Guid = RemoteKernelGuid,
            ConnectedVia = ConnectedVia
        };
        ConnectedVia.RemoteKernel = remoteKernel;

        _ConnectedKernels.Add(remoteKernel);
    }


    private void ExchangeDataChannels(RunningKernel remoteKernel)
    {
        var packetSendChannels = new PacketSyncRequest()
        {
            SourceHostingDataChannels = LocalChannelManager.GetLocalChannelDescriptors(),
            SourceKernelGuid = LocalKernel.Guid
        };

        var answer = remoteKernel.SendRequest<PacketSyncAnswer>(MessagesTypes.ChannelSyncRequest, packetSendChannels);
        if (answer == null)
        {
            throw new NullReferenceException("The answer for ExchangeDataChannels was null");
        }
        LocalChannelManager.AddChannelsSync(answer.RemoteHostingChannels.ToList(), remoteKernel);
    }

    public void OnSyncChannelsPacketReceived(List<ChannelDescriptor> RemoteChannels, RunningKernel remoteKernel)
    {
        LocalChannelManager.AddChannelsSync(RemoteChannels, remoteKernel);
    }


    public RunningKernel? GetKernel(Guid guid)
    {
        return _ConnectedKernels.Where(kernel => kernel.Guid == guid).FirstOrDefault();
    }



    /// <summary>
    /// To disconnect another kernel from this one
    /// </summary>
    /// <param name="kernel"></param>
    internal void DiconnectKernel(RunningKernel kernel)
    {

    }


    /// <summary>
    /// When a remote kernel requests to disconnect
    /// </summary>
    /// <param name="kernel"></param>
    internal void OnRemoteKernelRequestDiconnect(RunningKernel kernel)
    {
        Console.WriteLine("Remote requested diconnect.");
        LocalChannelManager?.RemoveKernel(kernel);
        kernel.OnRemoteDisconnect();

    }


}