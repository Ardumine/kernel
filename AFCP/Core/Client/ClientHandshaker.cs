using System.Text;
using Ardumine.AFCP.Core.Client;
using Ardumine.AFCP.Core.Client.RawComProt;

namespace Ardumine.AFCP.Core;

/// <summary>
/// Auths the client on the server
/// </summary>
public class ClientHandshaker
{
    private AFCPTCPClient client { get; set; }
    public ClientHandshaker(AFCPTCPClient _rawComProt)
    {
        this.client = _rawComProt;
    }

    /// <summary>
    /// To make the client auth on server. After TCP connection, the client and server discuss Auth. 
    /// </summary>
    /// <param name="authPass">The password to authenticate</param>
    /// <returns>Did it authenthicate on the server?</returns>
    public bool DoAuth(bool ForceAuth, string authPass = "coolPassword")
    {
        client.rawComProt.SendData(MsgTypes.Auth, Encoding.UTF8.GetBytes($"h{(ForceAuth ? '\x1' : '\x0')}"));//h[hello](1/0)[client has auth]

        var response = client.ReadChannelData(MsgTypes.Auth);
        if (response[1] == 1)
        {
            client.rawComProt.SendData(MsgTypes.Auth, Encoding.UTF8.GetBytes($"p{authPass}"));//p[password]...[(password)]
            response = client.ReadChannelData(MsgTypes.Auth);//c[check](1/0)[auth ok]

            if (response[1] == 1)
            {
                return true;
            }
            else
            {
                client.Close();
                return false;
            }
        }

        return true;//No auth needed, so OK
    }
}