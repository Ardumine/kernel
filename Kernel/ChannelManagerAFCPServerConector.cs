using System.Net.Sockets;
namespace Kernel.AFCP;

public class ChannelManagerAFCPServerConector
{
    public AfcpServer AFCPServer { get; private set; }
    public ChannelManager ChannelManager { get; private set; }
    public required KernelDescriptor LocalKernel { get; init; }


    public ChannelManagerAFCPServerConector(AfcpServer AFCPServer, ChannelManager channelManager)
    {
        this.AFCPServer = AFCPServer;
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
        var afcpClient = new AfcpClient(tcpClient, "Server", 0);
        var client = new ChannelManagerAfcpClientConector(afcpClient, ChannelManager, LocalKernel);
    }
}