using System.Net;
using System.Net.Sockets;

namespace Ardumine.AFCP.Core.Server;


public class AFCPTCPServer : BaseAFCPServer, IAFCPServer
{//Adaptative fast comunication protocol
    public override event EventHandler<OnDataRecArgs> OnDataRec;
    public override event EventHandler<AFCPServerClient> OnClientConnected;
    public override event EventHandler<OnQuestionRecArgs> OnQuestionRec;

    private TcpListener tcpListener;
    private Thread threadAccepter;
    private bool Running { get; set; }
    CancellationTokenSource stopToken = new CancellationTokenSource();

    private string PasswordAuth => "coolPassword";

    private Logger logger;
#pragma warning disable CS8600,CS8601, CS8618

    public AFCPTCPServer(IPAddress iPEnd, Logger logger)
    {
        tcpListener = new(new IPEndPoint(iPEnd, 9492));
        threadAccepter = new Thread(AccepterHandlerThread);

        Running = false;
        this.logger = logger;
    }
#pragma warning restore CS8600,CS8601, CS8618

    AuthSystem authSystem;
    DisconnectSystem disconnectSystem;

    public void Start()
    {
        authSystem = new()
        {
            logger = new Logger("Auth System"),
            Server = this
        };
        authSystem.Start();

        disconnectSystem = new(){
            logger = new Logger("Disconnect System"),
            Server = this
        };
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

        foreach (var item in Clients.ToList())
        {
            DisconnectClientForce(item, false);
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
                HandleClient(new AFCPServerClient(client.Client.RemoteEndPoint, client));
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

        client.OnQuestionRec += (s, e) =>
        {
            OnQuestionRec?.Invoke(this, new OnQuestionRecArgs()
            {
                Client = client,
                Question = e
            });
        };
    }

}


public class OnDataRecArgs : EventArgs
{
    public required AFCPServerClient Client { get; set; }
    public required DataReadFromRemote Data { get; set; }
}

public class OnQuestionRecArgs : EventArgs
{
    public required AFCPServerClient Client { get; set; }
    public required QuestionFromRemote Question { get; set; }
}