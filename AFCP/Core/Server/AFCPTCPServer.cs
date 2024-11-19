using System.Net;
using System.Net.Sockets;
using System.Text;
using Ardumine.AFCP.Core.Client;

namespace Ardumine.AFCP.Core.Server;


public class AFCPTCPServer
{//Adaptative fast comunication protocol


    public bool UseAuth { get; set; }
    private TcpListener tcpListener;
    private Thread threadAccepter;
    private bool Running { get; set; }
    CancellationTokenSource stopToken = new CancellationTokenSource();

    private List<AFCPTCPClient> Clients = new();
    public AFCPTCPServer(IPAddress iPEnd)
    {
        tcpListener = new(new IPEndPoint(iPEnd, 9492));
        threadAccepter = new Thread(AccepterHandlerThread);

        Running = false;
    }

    public void Start()
    {
        Running = true;
        tcpListener.Start();
        threadAccepter.Start();
    }

    public void Stop()
    {
        Running = false;
        stopToken.Cancel();
    }

    private void AccepterHandlerThread()
    {
        while (Running)
        {
            try
            {
                var client = tcpListener.AcceptTcpClientAsync(stopToken.Token).AsTask().GetAwaiter().GetResult();
                HandleClient(new AFCPTCPClient((IPEndPoint)client.Client.RemoteEndPoint, client));
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private void HandleClient(AFCPTCPClient client)
    {
        Clients.Add(client);
        var dataRead = client.ReadData();
        if (Encoding.UTF8.GetString(dataRead) == "Haro? Hibachi, Benihana, Teriyaki...")
        {
            client.SendData(Encoding.UTF8.GetBytes("Nagasaki, Okinawa, Hokkaido...Yokohama"));
        }
    }

}
