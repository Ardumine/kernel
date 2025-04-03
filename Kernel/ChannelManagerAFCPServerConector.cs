using System.Net;
using System.Net.Sockets;
using Kernel.AFCP.KernelManagement;
using Kernel.Logging;
namespace Kernel.AFCP;

public class ChannelManagerAFCPServerConector
{
    public required LocalRunningKernel LocalKernel { get; init; }

    private KernelManager _KernelManager;
    private AfcpServer AFCPServer;
    private ChannelManager ChannelManager;
    private ILogger _logger;


    public ChannelManagerAFCPServerConector(AfcpServer AFCPServer, ChannelManager channelManager, KernelManager kernelManager, ILogger logger)
    {
        _logger = logger;
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
        _logger.LogI("Client connected!");
        var ip = ((IPEndPoint)tcpClient.Client.RemoteEndPoint!).Address.ToString();


        var afcpClient = new AfcpTCPClient(tcpClient, "Server", 0, _logger.CreateSubLogger($"AfcpTCPClient{ip}"));
        var client = new ChannelManagerAfcpClientConector(afcpClient, ChannelManager, LocalKernel, _KernelManager, _logger.CreateSubLogger($"ChannelManagerAfcpClientConector"));
    }
}