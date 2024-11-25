using System.Text;
using Ardumine.AFCP.Core.Client.RawComProt;

namespace Ardumine.AFCP.Core;

/// <summary>
/// Auths the client on the server
/// </summary>
public class ClientHandshaker
{
    private IRawComProt rawComProt { get; set; }
    public ClientHandshaker(IRawComProt _rawComProt)
    {
        this.rawComProt  = _rawComProt;
    }

    /// <summary>
    /// To make the client auth on server. After TCP connection, the client and server discuss Auth. 
    /// </summary>
    /// <param name="authPass">The password to authenticate</param>
    /// <returns>Did it authenthicate on the server?</returns>
    public bool DoAuth(bool ForceAuth, string authPass = "coolPassword")
    {
        rawComProt.SendData(MsgTypes.Auth, Encoding.UTF8.GetBytes($"h{(ForceAuth ? '\x1' : '\x0')}"));//h[hello](1/0)[client has auth]

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        var response = rawComProt.ReadData(cts).Data;//h[hello](1/0)[server need auth]
        if (response[1] == 1)
        {
            rawComProt.SendData(MsgTypes.Auth, Encoding.UTF8.GetBytes($"p{authPass}"));//p[password]...[(password)]
            response = rawComProt.ReadData(cts).Data;//c[check](1/0)[auth ok]
            if (response[1] == 1)
            {
                return true;
            }
            else
            {
                rawComProt.Close();
                return false;
            }
        }

        return true;//No auth needed, so OK
    }
}