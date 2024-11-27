using Ardumine.AFCP.Core.Client.RawComProt;

public class DataReadFromRemote
{
    public DataReadFromRemote(ushort msgType, ushort questionChannelID, byte[] data)
    {
        MsgType = msgType;
        Data = data;
        
        QuestionChannelID = questionChannelID;
    }

    public ushort MsgType { get; set; }
    public byte[] Data { get; set; }
    public ushort QuestionChannelID { get; set; }

}


public class QuestionFromRemote : DataReadFromRemote
{
    public QuestionFromRemote(ushort msgType, ushort questionChannelID, byte[] data, IRawComProt prot) : base(msgType, questionChannelID, data) 
    {
        MsgType = msgType;
        Data = data;
        
        QuestionChannelID = questionChannelID;
        comProt = prot;
    }


    public IRawComProt comProt { get; set; }


    public void Answer(byte[] answer){
        comProt.SendQuestion((ushort)(MsgType + 1000), QuestionChannelID, answer);
    }

}