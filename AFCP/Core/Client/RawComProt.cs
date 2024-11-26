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
/// Between [200, 1200] is channel ID's.
/// Between [1400, 2400] is questions.
/// </summary>
public class MsgTypes
{
    public static ushort Auth => 4;
    public static ushort Disconnect => 2;
}