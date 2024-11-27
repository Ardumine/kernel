using System.Text;
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
    private void OnQuestionRec(object? sender, OnQuestionRecArgs args)
    {
        //Console.WriteLine("Req to " + args.Data.QuestionChannelID);
        var data = args.Question.Data;

        if (MsgTypes.GetType(args.Question.QuestionChannelID) == MsgType.ChannelResolve)
        {
            if (data[0] == 0x1)//1: resolve
            {
                var url = Encoding.UTF8.GetString(data, 1, data.Length - 1);
                var channel = Channels.Where(channel => channel.URL == url).FirstOrDefault();
                if (channel == null)
                {
                    args.Question.Answer([40]);//40: not found
                    return;
                }


                var channelData = Converter.UShortToByteArray(channel.AFCP_ID);

                byte[] oute = new byte[channelData.Length + 1];
                oute[0] = 0;//No error

                Array.Copy(channelData, 0, oute, 1, channelData.Length);
                //Console.WriteLine($"Rec mand: {string.Join(" ", oute)}");

                args.Question.Answer(oute);


            }
            else if (data[0] == 0x2)//2: resolve and get
            {
                var url = Encoding.UTF8.GetString(data, 1, data.Length - 1);
                var channel = Channels.Where(channel => channel.URL == url).FirstOrDefault();
                if (channel == null)
                {
                    args.Question.Answer([40]);//40: not found
                    return;
                }
                var channelData = channel.Get();
                byte[] oute = new byte[channelData.Length + 1];
                oute[0] = 0;
                Array.Copy(channelData, 0, oute, 1, channelData.Length);
                args.Question.Answer(oute);
            }

        }
        if (IDChannelGenerator.Contains(args.Question.QuestionChannelID))
        {
            var channel = Channels.Where(channel => channel.AFCP_ID == args.Question.QuestionChannelID).FirstOrDefault();
            if (channel != null)
            {
                if (args.Question.Data.Length == 0)
                {
                    args.Question.Answer(channel.Get());
                }

            }

        }
    }

    private void OnAFCPDataRec(object? sender, OnDataRecArgs args)
    {
        if (IDChannelGenerator.Contains(args.Data.MsgType))
        {
            var channel = Channels.Where(channel => channel.AFCP_ID == args.Data.MsgType).FirstOrDefault();
            if (channel != null)
            {
                if (args.Data.Data.Length != 0)
                {
                    channel.Set(args.Data.Data);
                }
            }
        }
    }
    public void CreateChannel(string URL, ChannelPermission perms)
    {
        Channel channel = new Channel(URL, perms);
        int id = IDChannelGenerator.ManualAdd(499);//IDChannelGenerator.GenerateID();
        if (id == -1)
        {//Error
            throw new ChannelsExceptions("Timeout or error to generate ID. Too many channels?");
        }
        channel.AFCP_ID = (ushort)id;
        channel.Local = true;
        ChannelManager.ChannelsOnLocal.Add(channel);
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
        ChannelManager.ChannelsOnLocal.Remove(channel);

    }


    public void Stop()
    {
        AFCPServer.OnDataRec -= OnAFCPDataRec;
        AFCPServer.OnQuestionRec -= OnQuestionRec;
    }

}
