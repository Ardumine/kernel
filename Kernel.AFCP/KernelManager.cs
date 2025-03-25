using Kernel.AFCP.KernelManagement;
using Kernel.AFCP.Packets;

namespace Kernel.AFCP;

/// <summary>
/// Manages the kernels
/// </summary>
public class KernelManager
{
    public required ChannelManager LocalChannelManager { get; set; }
    public LocalRunningKernel LocalKernel { get; private set; }

    /// <summary>
    /// Kernels that we acknowlage existing
    /// </summary>
    private List<RunningKernel> _ExistingKernels;

    public KernelManager(LocalRunningKernel localKernel)
    {
        LocalKernel = localKernel;
        _ExistingKernels = new List<RunningKernel>();

        _ExistingKernels.Add(LocalKernel);
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

        var remoteKernel = new RemoteRunningKernel()
        {
            Guid = remoteKernelGuid,
            ConnectedVia = afcpClient,
            IsLocal = false
        };
        afcpClient.RemoteKernel = remoteKernel;

        _ExistingKernels.Add(remoteKernel);

        ExchangeDataChannels(remoteKernel);

    }

    /// <summary>
    /// Disconnect the kernels.
    /// </summary>
    public void Stop()
    {
        foreach (var connectedKernel in _ExistingKernels){
            if(connectedKernel is RemoteRunningKernel remoteKernel){
                remoteKernel.RequestDisconect();
            }
        }
        _ExistingKernels.Clear();
    }

    /// <summary>
    /// When we are server
    /// </summary>
    /// <param name="RemoteKernelGuid"></param>
    internal void OnRemoteKernelConnect(Guid RemoteKernelGuid, ChannelManagerAfcpClientConector ConnectedVia)
    {
        var remoteKernel = new RemoteRunningKernel()
        {
            Guid = RemoteKernelGuid,
            ConnectedVia = ConnectedVia,
            IsLocal = false

        };
        ConnectedVia.RemoteKernel = remoteKernel;

        _ExistingKernels.Add(remoteKernel);
    }


    private void ExchangeDataChannels(RemoteRunningKernel remoteKernel)
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


    /// <summary>
    /// Gets a kernel from a Guid.
    /// </summary>
    /// <param name="guid">The kernel guid</param>
    /// <returns>The kernel</returns>
    public RunningKernel? GetKernel(Guid guid)
    {
        return _ExistingKernels.Where(kernel => kernel.Guid == guid).FirstOrDefault();
    }

    /// <summary>
    /// When a remote kernel requests to disconnect
    /// </summary>
    /// <param name="kernel"></param>
    internal void OnRemoteKernelRequestDiconnect(RemoteRunningKernel kernel)
    {
        Console.WriteLine("Remote requested diconnect.");
        LocalChannelManager?.RemoveKernel(kernel);
        kernel.OnRemoteDisconnect();

    }
}