using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Ardumine.AFCP.Core.Client;
using Ardumine.AFCP.Core.Client.RawComProt;

namespace Ardumine.AFCP.Core.Server;


public class AFCPTCPServer : BaseAFCPServer, IAFCPServer
{//Adaptative fast comunication protocol

    public override event EventHandler<AFCPServerClient> OnClientConnected;

    public bool UseAuth { get; set; }
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
    public void Start()
    {
        authSystem = new();
        authSystem.logger = new Logger("Auth System");
        authSystem.Server = this;
        authSystem.Start();

        Running = true;
        tcpListener.Start();
        threadAccepter.Start();

        Thread.Sleep(10);//Let it have some time to simmer
        UseAuth = true;

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
                HandleClient(new AFCPServerClient((IPEndPoint)client.Client.RemoteEndPoint, client));

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
        /*
        bool authed = DoAuth(client);
        if (authed)
        {
            var dataRead = client.rawComProt.ReadData().Data;
            if (Encoding.UTF8.GetString(dataRead) == "Haro? Hibachi, Benihana, Teriyaki...")
            {
                client.rawComProt.SendData(60, Encoding.UTF8.GetBytes("Nagasaki, Okinawa, Hokkaido...Yokohama"));
            }
        }*/
    }

    public bool DoAuth(AFCPServerClient client)
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        var dataRec = client.rawComProt.ReadData(cts).Data;
        if (dataRec[0] == 'h')
        {
            client.rawComProt.SendData(MsgTypes.Auth, Encoding.UTF8.GetBytes($"h{(UseAuth ? '\x1' : '\x0')}"));//h[hello](1/0)[server need auth]  
            dataRec = client.rawComProt.ReadData(cts).Data; 
            var passwordRec = Encoding.UTF8.GetString(dataRec, 1, dataRec.Length - 1);
            if (passwordRec == PasswordAuth)
            {
                client.rawComProt.SendData(MsgTypes.Auth, Encoding.UTF8.GetBytes($"c\x1"));//c[check](1/0)[auth ok]
                return true;
            }
            else
            {
                client.rawComProt.SendData(MsgTypes.Auth, Encoding.UTF8.GetBytes($"c\x0"));//c[check](1/0)[auth ok]

                logger.LogW($"Client {client.remoteIP} disconnected during auth due bad password: {passwordRec}");

                DisconnectClientAuth(client);
                return false;
            }
        }
        else
        {
            logger.LogW($"Client {client.remoteIP} disconnected during auth due bad hello");
            DisconnectClientAuth(client);
            return false;
        }
    }


   

}
