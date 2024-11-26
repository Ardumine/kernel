using System.Net;
using System.Runtime.InteropServices;

namespace Ardumine.AFCP.Core.Client.RawComProt;
public abstract class IRawComProt
{
    public abstract void Connect(IPEndPoint ipa);
    public abstract void SendData(ushort DataType, byte[] Data);
    public abstract DataReadFromRemote ReadData(CancellationTokenSource _stopToken = null);
    public abstract void Close();
    public abstract void SendQuestion(ushort channelID, ushort channelQuestionID, byte[] data);

    public unsafe void EncodeUshort(ushort data, Stream stream)
    {
        byte[] arrBytes = new byte[2];

        Marshal.Copy((IntPtr)(byte*)&data, arrBytes, 0, 2);//(byte*)&

        stream.Write(arrBytes);
        stream.Flush();

    }
}

/// <summary>
/// Between [0, 200] is protocol data.
/// Between [200, 1200] is channel ID's.
/// Between [1400, 2400] is questions.
/// Between [2400, 3400] is a answer to a question.
/// </summary>
public class MsgTypes
{
    public const ushort Disconnect = 2;
    public const ushort Auth = 4;

    public const ushort MIIChaID = 200;//Min interval for channel ID
    public const ushort MXIChaID = 1200;//Max interval for channel ID

    public const ushort MIIQuestID = 1400;//Min interval for question ID
    public const ushort MXIQuestID = 2400;//Max interval for question ID

    public const ushort MIIAnsID = 2400;//Min interval for answer ID
    public const ushort MXIAnsID = 3400;//Max interval for answer ID

    public static MsgType GetType(ushort msgType)
    {
        if (msgType == Disconnect)
        {
            return MsgType.Disconnect;
        }
        else if (msgType == Auth)
        {
            return MsgType.Auth;
        }
        else if (msgType > MIIQuestID && msgType < MXIQuestID)
        {
            return MsgType.Question;
        }
        else if (msgType > MIIAnsID && msgType < MXIAnsID)
        {
            return MsgType.Answer;
        }
        return MsgType.Unknown;
    }

    public static bool IsAnswerQuestionRelated(ushort msgType)
    {
        return msgType > MIIQuestID && msgType < MXIQuestID || msgType > MIIAnsID && msgType < MXIAnsID;
    }

}
public enum MsgType
{
    Auth,
    Disconnect,
    Question,
    Answer,
    Unknown,
}