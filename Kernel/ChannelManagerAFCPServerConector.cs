using System.Net;
using System.Net.Sockets;
using Kernel.AFCP.KernelManagement;
using Kernel.Logging;
namespace Kernel.AFCP;

public class ChannelManagerAFCPServerConector
{
    public required LocalRunningKernel LocalKernel { get; init; }

    private KernelManager _KernelManager;
    private AfcpServer _AfcpServer;
    private ChannelManager _channelManager;
    private DataChannelManager _dataChannelManager;

    private ILogger _logger;


    public ChannelManagerAFCPServerConector(AfcpServer AFCPServer, ChannelManager channelManager, DataChannelManager dataChannelManager, KernelManager kernelManager, ILogger logger)
    {
        _logger = logger;
        _dataChannelManager = dataChannelManager;
        _AfcpServer = AFCPServer;
        _KernelManager = kernelManager;

        _channelManager = channelManager;

        Init();
    }
    private void Init()
    {
        _AfcpServer.OnTCPClientConnected += AFCPServer_OnTCPClientConnected;
    }
    private void AFCPServer_OnTCPClientConnected(TcpClient tcpClient)
    {
        _logger.LogI("Client connected!");
        var ip = ((IPEndPoint)tcpClient.Client.RemoteEndPoint!).Address.ToString();


        var afcpClient = new AfcpTCPClient(tcpClient, "Server", 0, _logger.CreateSubLogger($"AfcpTCPClient{ip}"));
        var client = new ChannelManagerAfcpClientConector(afcpClient, _channelManager, _dataChannelManager, LocalKernel, _KernelManager, _logger.CreateSubLogger($"ChannelManagerAfcpClientConector"));
    }
}