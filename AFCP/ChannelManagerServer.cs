using System.Net;
using System.Net.Sockets;

namespace AFCP;
public class ChannelManagerServer
{
    public bool Running { get; private set; }

    private TcpListener tcpListener;
    public List<ChannelManagerClient> Clients = new List<ChannelManagerClient>();
    private ChannelManager channelManager;
    public ChannelManagerServer(int port, ChannelManager _channelManager)
    {
        tcpListener = new(IPAddress.Any, port);
        channelManager = _channelManager;
    }
    public void Start()
    {
        tcpListener.Start();
        Running = true;
        new Thread(tcpHandler){
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
                Clients.Add(new ChannelManagerClient(client, channelManager){ServerOwner = this});
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