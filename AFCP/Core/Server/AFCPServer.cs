using Ardumine.AFCP.Core.Client.RawComProt;
using Ardumine.AFCP.Core.Server;

public interface IAFCPServer
{

   
}
public abstract class BaseAFCPServer{
    public abstract event EventHandler<AFCPServerClient> OnClientConnected;

     public List<AFCPServerClient> Clients = new();

    /// <summary>
    /// When a client doesnt do the auth, we disconnect. Do not call unless Auth related.
    /// </summary>
    public void DisconnectClientAuth(AFCPServerClient client)
    {
        client.rawComProt.SendData(MsgTypes.Auth, [26, 25, 255]);
        client.Close();
        Clients.Remove(client);
    }
}