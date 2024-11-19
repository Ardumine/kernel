using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Ardumine.AFCP.Core.Client;


//This is for the raw communication protocol. Dont use it for normal use. It is for standard TCP communication with stability.
public class AFCPTCPClient
{
    public IPEndPoint remoteIP;

    private TcpClient TCPclient;
    private NetworkStream networkStream;
    public AFCPTCPClient(IPAddress ipa, int port = 9492)
    {
        remoteIP = new(ipa, port);
    }
    public AFCPTCPClient(IPEndPoint remIP, TcpClient tcpClient)
    {
        remoteIP = remIP;
        TCPclient = tcpClient;
        networkStream = TCPclient.GetStream();
    }
    public void Connect()
    {
        TCPclient.Connect(remoteIP);
        networkStream = TCPclient.GetStream();
    }

    #region Data send
    public void SendData(byte[] data)
    {
        EncodeByteArray(data, (ushort)data.Length, networkStream);
    }

    //Code from Freedom VPN. Internal Ardumine C# project. It may seek GitHub if I feel good.
    private byte[] arrBytesLen = new byte[2];
    private unsafe void EncodeByteArray(byte[] arrIn, ushort arrLen, Stream stream)
    {
        Marshal.Copy((IntPtr)(byte*)&arrLen, arrBytesLen, 0, 2);//(byte*)&
        stream.Write(arrBytesLen);
        stream.Write(arrIn, 0, arrLen);
    }

    #endregion


    #region Data read

    //Code from Freedom VPN. Internal Ardumine C# project. It may seek GitHub if I feel good.
    public byte[] ReadData()
    {
        byte[] BufferLenRec = new byte[2];
        networkStream.Read(BufferLenRec, 0, 2);//int32 = 4 bytes; int16(short) = 2(-32,768 to 32,767); ushort = 2 bytes(0 to 65,535)
        int TamDadosPRec = ByteArrayToUshort(BufferLenRec);

        byte[] dados = new byte[TamDadosPRec];

        int lenDadosRecibdo = 0;
        while (lenDadosRecibdo != TamDadosPRec)
        {
            lenDadosRecibdo += networkStream.Read(dados, lenDadosRecibdo, TamDadosPRec - lenDadosRecibdo);//int32 = 4 bytes
        }

        return dados;
    }
    public static ushort ByteArrayToUshort(byte[] b)
    {
        return (ushort)(b[0] | (b[1] << 8));//| (b[2] << 16) | (b[3] << 24)

    }

    #endregion

}