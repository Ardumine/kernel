using System.Net;
using System.Net.Sockets;
using Ardumine.AFCP.Core.Client;


namespace Ardumine.AFCP.Core.Server;

public class AFCPServerClient : AFCPTCPClient
{
    public AuthStateClientOnServer AuthState = AuthStateClientOnServer.NotAuth;
    public AFCPServerClient(IPEndPoint remIP, TcpClient tcpClient) : base(remIP, tcpClient)
    {
        Name = "Server";
    }

   

}

public enum AuthStateClientOnServer
{
    /// <summary>
    /// When its first connection. Waiting for auth message from client.
    /// </summary>
    NewConnect,
    NotAuth,
    DoingAuth,
    AuthOK
}