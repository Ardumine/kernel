using System.Net;

namespace Ardumine.AFCP.Core.Client.RawComProt;
public interface IRawComProt
{
    public void Connect(IPEndPoint ipa);
    public void SendData(ushort DataType, byte[] Data);
    public DataReadFromRemote ReadData(CancellationTokenSource _stopToken = null);
    public void Close();
}

/// <summary>
/// Above 40 is channel ID's.
/// </summary>
public class MsgTypes
{
    public static ushort Auth => 4;
    public static ushort Disconnect => 2;
}