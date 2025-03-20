using System.Net.Sockets;
namespace Kernel.AFCP;

public class ChannelManagerAFCPServerConector
{
    public required LocalRunningKernel LocalKernel { get; init; }

    private KernelManager _KernelManager;
    private AfcpServer AFCPServer;
    private ChannelManager ChannelManager;


    public ChannelManagerAFCPServerConector(AfcpServer AFCPServer, ChannelManager channelManager, KernelManager kernelManager)
    {
        this.AFCPServer = AFCPServer;
        this._KernelManager = kernelManager;

        ChannelManager = channelManager;

        Init();
    }
    private void Init()
    {
        AFCPServer.OnTCPClientConnected += AFCPServer_OnTCPClientConnected;
    }
    private void AFCPServer_OnTCPClientConnected(TcpClient tcpClient)
    {
        Console.WriteLine("Client connected!");
        var afcpClient = new AfcpTCPClient(tcpClient, "Server", 0);
        var client = new ChannelManagerAfcpClientConector(afcpClient, ChannelManager, LocalKernel, _KernelManager);
    }
}