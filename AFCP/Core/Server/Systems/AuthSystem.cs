using System.Text;
using Ardumine.AFCP.Core.Client.RawComProt;
using Ardumine.AFCP.Core.Server;

public class AuthSystem : BaseSystem
{
    public bool UseAuth { get; set; }
    public bool CanAuthAsGuest { get; set; }
    public string PasswordAuth => "coolPassword";
    public override void Start()
    {
        Server.OnClientConnected += OnClientConnected;
    }

    private void OnClientConnected(object sender, AFCPServerClient client)
    {
        Console.WriteLine("Client connected! server");
        client.AuthState = AuthStateClientOnServer.NewConnect;


        var dataRec = client.ReadChannelData(MsgTypes.Auth);

        if (dataRec[0] == 'h')
        {
            client.AuthState = AuthStateClientOnServer.DoingAuth;
            if (dataRec[1] == 1)
            {//If does not allow guest and server only allows authenthicated.
                dataRec = client.ReadChannelData(MsgTypes.Auth);
                var passwordRec = Encoding.UTF8.GetString(dataRec, 1, dataRec.Length - 1);
                if (passwordRec == PasswordAuth)
                {
                    client.rawComProt.SendData(MsgTypes.Auth, Encoding.UTF8.GetBytes($"c\x1"));//c[check](1/0)[auth ok]
                }
                else
                {
                    client.rawComProt.SendData(MsgTypes.Auth, Encoding.UTF8.GetBytes($"c\x0"));//c[check](1/0)[auth ok]
                    logger.LogW($"Client {client.remoteIP} disconnected during auth due bad password: {passwordRec}");
                    Server.DisconnectClientAuth(client);
                }
            }
        }
        else
        {
            logger.LogW($"Client {client.remoteIP} disconnected during auth due bad hello");
            Server.DisconnectClientAuth(client);

        }

    }
}