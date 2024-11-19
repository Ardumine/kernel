using System.Net;
using System.Net.Sockets;
using Ardumine.AFCP.Core.Client;


namespace Ardumine.AFCP.Core.Server;

public class AFCPServerClient : AFCPTCPClient
{
    public AuthStateClientOnServer AuthState = AuthStateClientOnServer.NotAuth;
    public AFCPServerClient(IPEndPoint remIP, TcpClient tcpClient) : base(remIP, tcpClient)
    {
    }
}

public enum AuthStateClientOnServer
{
    /// <summary>
    /// Waiting for auth
    /// </summary>
    NotAuth,
    DoingAuth,
    AuthOK
}