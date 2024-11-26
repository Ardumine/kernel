using System.Net;
using System.Net.Sockets;

namespace Ardumine.AFCP.Core.Server;


public class AFCPTCPServer : BaseAFCPServer, IAFCPServer
{//Adaptative fast comunication protocol
    public override event EventHandler<OnDataRecArgs> OnDataRec;
    public override event EventHandler<AFCPServerClient> OnClientConnected;

    private TcpListener tcpListener;
    private Thread threadAccepter;
    private bool Running { get; set; }
    CancellationTokenSource stopToken = new CancellationTokenSource();

    private string PasswordAuth => "coolPassword";

    private Logger logger;
    public AFCPTCPServer(IPAddress iPEnd, Logger logger)
    {
        tcpListener = new(new IPEndPoint(iPEnd, 9492));
        threadAccepter = new Thread(AccepterHandlerThread);

        Running = false;
        this.logger = logger;
    }
    AuthSystem authSystem;
    DisconnectSystem disconnectSystem;

    public void Start()
    {
        authSystem = new();
        authSystem.logger = new Logger("Auth System");
        authSystem.Server = this;
        authSystem.Start();

        disconnectSystem = new();
        disconnectSystem.Server = this;
        disconnectSystem.Start();


        Running = true;
        tcpListener.Start();
        threadAccepter.Start();

        Thread.Sleep(10);//Let it have some time to simmer
    }

    public void Stop()
    {
        Running = false;
        stopToken.Cancel();
        Thread.Sleep(10);//Let it have some time to simmer
        foreach (var item in Clients)
        {
            DisconnectClientForce(item);
        }
        tcpListener.Stop();
    }

    private void AccepterHandlerThread()
    {
        while (Running)
        {
            try
            {
                var client = tcpListener.AcceptTcpClientAsync(stopToken.Token).AsTask().GetAwaiter().GetResult();
                HandleClient(new AFCPServerClient((IPEndPoint)client.Client.RemoteEndPoint, client){Name = "server"});
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private void HandleClient(AFCPServerClient client)
    {

        Clients.Add(client);
        OnClientConnected?.Invoke(this, client);
        client.OnDataRec += (s, e) =>
        {
            OnDataRec?.Invoke(this, new OnDataRecArgs()
            {
                Client = client,
                Data = e
            });
        };
    }

}


public class OnDataRecArgs : EventArgs
{
    public AFCPServerClient Client { get; set; }
    public DataReadFromRemote Data { get; set; }
}