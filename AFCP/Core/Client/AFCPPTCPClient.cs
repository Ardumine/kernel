using System.Net;
using System.Net.Sockets;
using Ardumine.AFCP.Core.Client.RawComProt;

namespace Ardumine.AFCP.Core.Client;


//This is for the raw communication protocol. Dont use it for normal use. It is for standard TCP communication with stability.
public class AFCPTCPClient : IAFCPClient
{
    public event EventHandler<DataReadFromRemote> OnDataRec;

    ClientHandshaker clientHandshaker;
    CancellationTokenSource StopToken = new CancellationTokenSource();
    public bool UseAuth { get; set; }


    public TCPRawComProt rawComProt { get; set; }
    public IPEndPoint remoteIP;
    public bool Run { get; private set; }

    private Thread thReadData;
    public AFCPTCPClient(IPAddress ipa, bool doAuth = false, int port = 9492)
    {
        remoteIP = new(ipa, port);
        UseAuth = doAuth;
        rawComProt = new TCPRawComProt();
        clientHandshaker = new(this);
        thReadData = new(FuncReadData);
    }

    public AFCPTCPClient(IPEndPoint remIP, TcpClient tcpClient)
    {
        thReadData = new(FuncReadData);
        remoteIP = remIP;
        rawComProt = new(tcpClient);
        thReadData.Start();
        Run = true;
    }


    public bool Connect()
    {
        rawComProt.Connect(remoteIP);
        Run = true;
        thReadData.Start();
        Thread.Sleep(100);
        return clientHandshaker.DoAuth(true);
    }

    class ChannelEventHandler
    {
        public AutoResetEvent autoResetEvent;
        public ushort Channel { get; set; }
        public int TimeoutMS { get; set; }
        public byte[] Data { get; set; }
    }
    List<ChannelEventHandler> handlers = new();
    public void TripResetEvent(int Channel, byte[] Data)
    {
        handlers.Where(handler => handler.Channel == Channel).ToList().ForEach((handler) =>
        {
            handler.Data = Data;
            handler.autoResetEvent.Set();
        });
    }

    private ChannelEventHandler AddResetEvent(ushort Channel, int TimeoutMS = 1000)
    {
        var cc = new ChannelEventHandler() { autoResetEvent = new AutoResetEvent(false), Channel = Channel, TimeoutMS = TimeoutMS };
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
                OnDataRec?.Invoke(this, dataRec);
                TripResetEvent(dataRec.DataType, dataRec.Data);
            }
            catch { }
        }
        Console.WriteLine("Loop quit!");
    }



    public void Close()
    {
        rawComProt.SendData(MsgTypes.Disconnect, [0]);
        Thread.Sleep(50);
        Run = false;
        StopToken.Cancel();
        rawComProt.Close();
    }


    public byte[] ReadChannelData(ushort DataType, int timoutMS = 5000)
    {
        var resEvent = AddResetEvent(DataType, timoutMS);
        return WaitResetEvent(resEvent);
    }


}