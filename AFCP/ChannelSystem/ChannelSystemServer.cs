using Ardumine.AFCP.Core.Client.RawComProt;
using Ardumine.AFCP.Core.Server;

public class ChannelSystemServer
{
    private List<Channel> Channels = new List<Channel>();
    private BaseAFCPServer AFCPServer;
    private IDGenerator IDChannelGenerator;
    public ChannelSystemServer(BaseAFCPServer baseAFCPServer)
    {
        AFCPServer = baseAFCPServer;
        IDChannelGenerator = new(MsgTypes.MIIChaID, MsgTypes.MXIChaID);
    }

    public void Start()
    {
        AFCPServer.OnDataRec += OnAFCPDataRec;
        AFCPServer.OnQuestionRec += OnQuestionRec;
    }
    private void OnQuestionRec(object sender, OnQuestionRecArgs args)
    {
        Console.WriteLine("Question rec!");
        args.Data.Answer([0, 1, 2]);
    }
    private void OnAFCPDataRec(object sender, OnDataRecArgs args)
    {
        if (IDChannelGenerator.Contains(args.Data.MsgType))
        {
            var channel = Channels.Where(channel => channel.AFCP_ID == args.Data.MsgType).FirstOrDefault();
            if (channel == null)
            {
                //Error
            }
        }
    }
    public void CreateChannel(string URL, ChannelPermission perms)
    {
        Channel channel = new Channel(URL, perms);
        int id = IDChannelGenerator.GenerateID();
        if (id == -1)
        {//Error
            throw new ChannelsExceptions("Timeout or error to generate ID. Too many channels?");
        }
        channel.AFCP_ID = (ushort)id;
        Channels.Add(channel);
        Console.WriteLine($"Channel {channel.URL} with ID{channel.AFCP_ID}");
    }

    public void DeleteChannel(string URL)
    {
        var channel = Channels.Where(channel => channel.URL == URL).FirstOrDefault();
        if (channel == null)
        {
            throw new ChannelsExceptions("Channel URL not found. Is the channel on this channel system server?");
        }

        IDChannelGenerator.DeleteID(channel.AFCP_ID);
        Channels.Remove(channel);
    }


    public void Stop()
    {
        AFCPServer.OnDataRec -= OnAFCPDataRec;
    }

}
