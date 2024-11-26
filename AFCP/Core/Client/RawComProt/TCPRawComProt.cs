
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Ardumine.AFCP.Core.Client.RawComProt;

public class TCPRawComProt : IRawComProt
{

    private NetworkStream networkStream;
    private TcpClient TCPClient;
    public CancellationTokenSource StopToken = new CancellationTokenSource();
    public TCPRawComProt()
    {

    }
    public TCPRawComProt(TcpClient _tcpClient)
    {
        TCPClient = _tcpClient;
        networkStream = TCPClient.GetStream();

    }
    public override void Connect(IPEndPoint remoteIP)
    {
        TCPClient = new();
        TCPClient.Connect(remoteIP);
        networkStream = TCPClient.GetStream();
    }
    #region Data send

    /// <summary>
    /// Sends data to the remote. It handles data sizes auto.
    /// </summary>
    /// <param name="data">Data to send</param>
    public override void SendData(ushort MsgType, byte[] data)
    {
        EncodeByteArray(data, MsgType, (ushort)data.Length, networkStream);
    }
    public override void SendQuestion(ushort channelID, ushort channelQuestionID, byte[] data)
    {
        EncodeUshort((ushort)data.Length, networkStream);
        EncodeUshort(channelID, networkStream);

        EncodeUshort(channelQuestionID, networkStream);
        networkStream.Write(data);
        networkStream.Flush();

    }

    //Code from Freedom VPN. Internal Ardumine C# project. It may seek GitHub if I feel good.
    private unsafe void EncodeByteArray(byte[] arrIn, ushort MsgType, ushort arrLen, Stream stream)
    {
        byte[] arrBytesLen = new byte[2];
        byte[] arrBytesMsgType = new byte[2];

        Marshal.Copy((IntPtr)(byte*)&arrLen, arrBytesLen, 0, 2);//(byte*)&
        Marshal.Copy((IntPtr)(byte*)&MsgType, arrBytesMsgType, 0, 2);//(byte*)&

        stream.Write(arrBytesLen);
        stream.Write(arrBytesMsgType);
        stream.Write(arrIn, 0, arrLen);
    }




    #endregion

    #region Data read

    //Code from Freedom VPN. Internal Ardumine C# project. It may seek GitHub if I feel good.

    /// <summary>
    /// Waits until it receives data from the remote. It handles data sizes auto.
    /// </summary>
    /// <returns>Message type and Received byte array</returns>
    public override DataReadFromRemote ReadData(CancellationTokenSource _stopToken = null)
    {
        if (_stopToken == null) _stopToken = StopToken;
        byte[] BufferLenRec = new byte[2];
        byte[] BufferMsgType = new byte[2];

        networkStream.ReadAsync(BufferLenRec, 0, 2, _stopToken.Token).GetAwaiter().GetResult();//int32 = 4 bytes; int16(short) = 2(-32,768 to 32,767); ushort = 2 bytes(0 to 65,535)
        int TamDadosPRec = Converter.ByteArrayToUshort(BufferLenRec);


        networkStream.ReadAsync(BufferMsgType, 0, 2, _stopToken.Token).GetAwaiter().GetResult();//int32 = 4 bytes; int16(short) = 2(-32,768 to 32,767); ushort = 2 bytes(0 to 65,535)
        ushort msgChannel =  Converter.ByteArrayToUshort(BufferMsgType);


        ushort questionChannelID = 0;
        if (MsgTypes.IsAnswerQuestionRelated(msgChannel))//Could be a question or a answer.
        {
            byte[] BufferQuestionChannelID = new byte[2];//What is the channel to ask?

            networkStream.ReadAsync(BufferQuestionChannelID, 0, 2, _stopToken.Token).GetAwaiter().GetResult();
            questionChannelID =  Converter.ByteArrayToUshort(BufferQuestionChannelID);

        }



        byte[] dados = new byte[TamDadosPRec];
        int lenDadosRecibdo = 0;
        while (lenDadosRecibdo != TamDadosPRec)
        {
            lenDadosRecibdo += networkStream.ReadAsync(dados, lenDadosRecibdo, TamDadosPRec - lenDadosRecibdo, _stopToken.Token).GetAwaiter().GetResult();//int32 = 4 bytes
        }
        return new DataReadFromRemote(msgChannel, questionChannelID, dados);
    }
    

    #endregion
    public override void Close()
    {
        StopToken.Cancel();
        networkStream.Close();
        TCPClient.Close();
    }


}