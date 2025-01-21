using System.Net;
using System.Net.Sockets;

namespace Kernel.AFCP;
public class AfcpServer
{
    public bool Running { get; private set; }

    private TcpListener tcpListener;

    public delegate void OnTCPClientConnectedEvent(TcpClient client);
    public event OnTCPClientConnectedEvent? OnTCPClientConnected;


    //private ChannelManager channelManager;
    public AfcpServer(int port)
    {
        tcpListener = new(IPAddress.Any, port);
        //channelManager = _channelManager;
    }
    public void Start()
    {
        tcpListener.Start();
        Running = true;
        new Thread(tcpHandler)
        {
            Name = $"ChannelManagerServer {tcpListener.LocalEndpoint}"
        }.Start();
    }

    public void Stop()
    {
        Running = false;
        tcpListener.Stop();
    }

    private void tcpHandler()
    {
        while (Running)
        {
            try
            {
                var client = tcpListener.AcceptTcpClient();
                if (OnTCPClientConnected != null)
                {
                    OnTCPClientConnected.Invoke(client);
                }
            }
            catch
            {
                if (Running)
                {
                    throw;
                }
            }
        }
    }


}