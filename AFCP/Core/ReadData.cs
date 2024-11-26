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