using System.Net;
using System.Net.Sockets;
using Ardumine.AFCP.Core.Client.RawComProt;

namespace Ardumine.AFCP.Core.Client;


//This is for the raw communication protocol. Dont use it for normal use. It is for standard TCP communication with stability.
public class AFCPTCPClient : IAFCPClient
{

    public string Name {get;set;}
    public event EventHandler<DataReadFromRemote> OnDataRec;

    ClientHandshaker clientHandshaker;
    CancellationTokenSource StopToken = new CancellationTokenSource();
    public bool UseAuth { get; set; }


    public TCPRawComProt rawComProt { get; set; }
    public IPEndPoint remoteIP;
    public bool Run { get; private set; }

    private IDGenerator QuestionIDGenerator;
    private Thread thReadData;
    public AFCPTCPClient(IPAddress ipa, bool doAuth = false, int port = 9492)
    {
        remoteIP = new(ipa, port);
        UseAuth = doAuth;
        rawComProt = new TCPRawComProt();
        clientHandshaker = new(this);
        thReadData = new(FuncReadData);
        QuestionIDGenerator = new(1400, 2400);
    }

    public AFCPTCPClient(IPEndPoint remIP, TcpClient tcpClient)
    {
        thReadData = new(FuncReadData);
        remoteIP = remIP;
        rawComProt = new(tcpClient);
        thReadData.Start();
        Run = true;
        QuestionIDGenerator = new(1400, 2400);
    }


    public bool Connect()
    {
        rawComProt.Connect(remoteIP);
        Run = true;
        thReadData.Start();
        return clientHandshaker.DoAuth(true);
    }

    class ChannelEventHandler
    {
        public AutoResetEvent autoResetEvent;
        public ushort QuestionChannelID { get; set; }
        public ushort MessageChannel { get; set; }

        public int TimeoutMS { get; set; }
        public byte[] Data { get; set; }
    }
    List<ChannelEventHandler> handlers = new();
    public void TripResetEvent(ushort Channel, ushort questionChannelID, byte[] Data)
    {
        handlers.Where(handler => handler.MessageChannel == Channel && handler.QuestionChannelID == questionChannelID).ToList().ForEach((handler) =>
        {
            handler.Data = Data;
            handler.autoResetEvent.Set();
        });
    }

    private ChannelEventHandler AddResetEvent(ushort Channel, int TimeoutMS = 1000)
    {
        var cc = new ChannelEventHandler() { autoResetEvent = new AutoResetEvent(false), MessageChannel = Channel, TimeoutMS = TimeoutMS };
        handlers.Add(cc);
        return cc;
    }

    private ChannelEventHandler AddResetEvent(ushort Channel, ushort questionChannelID, int TimeoutMS = 1000)
    {
        var cc = new ChannelEventHandler() { autoResetEvent = new AutoResetEvent(false), MessageChannel = Channel, TimeoutMS = TimeoutMS, QuestionChannelID = questionChannelID };
        handlers.Add(cc);
        return cc;
    }

    private byte[] WaitResetEvent(ChannelEventHandler handler)
    {
        if (!handler.autoResetEvent.WaitOne(handler.TimeoutMS))
        {//Não recebeu dados
            return null;
        }
        return handler.Data;

    }

    private void FuncReadData()
    {
        while (Run)
        {
            try
            {
                var dataRec = rawComProt.ReadData(StopToken);
            
                if(dataRec.MsgType > 1400 && dataRec.MsgType < 2400){
                    Console.WriteLine($"Channel question rec: {dataRec.QuestionChannelID} Question ID:{dataRec.MsgType}");
                }

                    TripResetEvent(dataRec.MsgType, dataRec.QuestionChannelID, dataRec.Data);
                OnDataRec?.Invoke(this, dataRec);
            }
            catch { }
        }
        Console.WriteLine("exit" + Name);
    }



    public void Close()
    {
        rawComProt.SendData(MsgTypes.Disconnect, [0]);
        Thread.Sleep(50);
        Run = false;
        StopToken.Cancel();
        rawComProt.Close();
    }


    public byte[] ReadChannelData(ushort MsgType, int timoutMS = 5000)
    {
        var resEvent = AddResetEvent(MsgType, timoutMS);
        return WaitResetEvent(resEvent);
    }

    public byte[] ReadChannelData(ushort MsgType, ushort questionChannelID, int timoutMS = 5000)
    {
        var resEvent = AddResetEvent(MsgType, questionChannelID, timoutMS);
        return WaitResetEvent(resEvent);
    }




    //We ask something
    //We ask something to a channel.
    public byte[] Ask(ushort channelQuestionID, byte[] question)
    {
        //IDQuestion: [1400, 2400] Generated randomly. To not confuse the answer of other questions with out question to this channel. 

        //channelQuestion: [200, 1200] The channel to ask.

        ushort IDQuestion = (ushort)QuestionIDGenerator.GenerateID();
        rawComProt.SendQuestion(IDQuestion, channelQuestionID, question);
        var answer = ReadChannelData(IDQuestion, channelQuestionID);
        QuestionIDGenerator.DeleteID(IDQuestion);

        return answer;
    }


    public void Answer()
    {

    }

}