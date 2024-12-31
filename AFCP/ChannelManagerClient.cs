using System.Collections;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text.Json;
using AFCP.DataTreatment;
using AFCP.JsonConverters;
using AFCP.Packets;
using AFCP.Systems;

namespace AFCP;

public class ChannelManagerClient
{

    public bool Running { get; set; }
    public ChannelManager LocalChannelManager { get; set; }
    public ChannelManagerServer? ServerOwner { get; set; }
    public Guid RemoteGuid = new();

    private ConcurrentDictionary<uint, TaskCompletionSource<BasePacketAnswer>> _pendingRequests = new ConcurrentDictionary<uint, TaskCompletionSource<BasePacketAnswer>>();
    TcpClient tcpClient;
    NetworkStream TCPClientstream;
    ConnectSystem connectSystem;
    ChannelManagementSystem channelManagementSystem;
   
    private AutoResetEvent OnNewData = new AutoResetEvent(false);

    Queue<DataWritter> dataCache = new();
    void WriteData(DataWritter dataWritter)
    {
        dataCache.Enqueue(dataWritter);
        OnNewData.Set();

    }
    public ChannelManagerClient(string IP, int Port, ChannelManager _localChannelManager)
    {
        LocalChannelManager = _localChannelManager;
        Running = true;
        tcpClient = new TcpClient();


        tcpClient.Connect(IP, Port);
        TCPClientstream = tcpClient.GetStream();

        new Thread(() =>
       {
           while (Running)
           {
               while (dataCache.Count > 0)
               {
                   dataCache.Dequeue().Copy(TCPClientstream);
               }
               OnNewData.Reset();
           }
       })
        {
            Name = $"ChannelManagerClient {IP}:{Port} Send"
        }.Start();

        new Thread(handleTCPClient)
        {
            Name = $"ChannelManagerClient {IP}:{Port} Read"
        }.Start();


        channelManagementSystem = new(LocalChannelManager);
        connectSystem = new(LocalChannelManager, this);
    	File.Create("cc");

        Thread.Sleep(100);
        OnConnect();//Only when we connect as a client

    }

    internal ChannelManagerClient(TcpClient client, ChannelManager _localChannelManager)
    {

        LocalChannelManager = _localChannelManager;

        Running = true;
        tcpClient = client;

        channelManagementSystem = new(LocalChannelManager);
        connectSystem = new(LocalChannelManager, this);

        TCPClientstream = tcpClient.GetStream();


        new Thread(() =>
       {
           while (Running)
           {
               while (dataCache.Count > 0 && Running)
               {
                   try
                   {
                       dataCache.Dequeue().Copy(TCPClientstream);
                   }
                   catch
                   {
                       if (!tcpClient.Connected && Running)
                       {
                           ServerStop(RemoteGuid);
                       }

                       if (Running && tcpClient.Connected)
                       {
                           throw;
                       }
                   }
               }
               OnNewData.Reset();
           }
       })
        {
            Name = $"ChannelManagerClient Server Send"
        }.Start();

        new Thread(handleTCPClient)
        {
            Name = "ChannelManagerClient Server Read"
        }.Start();
    }

    private void OnConnect()
    {
        var connectResponse = SendRequest<PacketConnectAnswer>(MessagesTypes.ChannelConnectRequest, new PacketConnectRequest() { RemoteKernel = LocalChannelManager.LocalGuid, Disconnect = false });


        LocalChannelManager.ConnectedKernels.Add(new KernelDescriptor()
        {
            KernelGuid = connectResponse.RemoteKernel,
            channelManagerClient = this
        });

    }

    //Use this one when you are the one needing to stop the connection and not caused by an other kernel.
    public void RequestStopAndStop()
    {
        var connectResponse = SendRequest<PacketConnectAnswer>(MessagesTypes.ChannelConnectRequest, new PacketConnectRequest() { RemoteKernel = LocalChannelManager.LocalGuid, Disconnect = true });

        OnNewData.Set();

        LocalChannelManager.RemoveKernel(connectResponse.RemoteKernel);
        Running = false;
        tcpClient.Close();

        _pendingRequests.Clear();

    }

    //When another asks to stop, then this runs. Only if this client is an server and not a client, run this.
    public void ServerStop(Guid remoteKernel)
    {
        Console.WriteLine("Server stop");
        LocalChannelManager.RemoveKernel(remoteKernel);

        foreach (var item in _pendingRequests.Values)
        {
            item.SetResult(null!);
        }

        _pendingRequests.Clear();


        Running = false;
        OnNewData.Set();

        tcpClient.Close();
        ServerOwner?.Clients.Remove(this);
        tcpClient.Dispose();
        TCPClientstream.Dispose();
    }

    private uint LatestRequestID = 0;
    public T SendRequest<T>(byte RequestType, PacketBaseRequest Payload) where T : BasePacketAnswer
    {
        uint RequestID = LatestRequestID++;
        var tcs = new TaskCompletionSource<BasePacketAnswer>();
        _pendingRequests[RequestID] = tcs;

        var writter = GenerateRequestHeader(RequestID);

        writter.Write(RequestType);
        
        Payload.Serialize(writter);
    
        WriteData(writter);

        return (T)tcs.Task.Result;

    }




    private void handleTCPClient()
    {
        while (Running)
        {
            try
            {
                var reader = new DataReader(TCPClientstream);

                uint RequestID = reader.ReadUint();
                bool isAnswer = reader.ReadBool();



                if (!isAnswer)//Question
                {
                    var answerWritterFull = GenerateAnwserHeader(RequestID);

                    BasePacketAnswer answer = null!;

                    byte msgType = reader.ReadByte();
                    //answerWritterFull.WriteByte(msgType);
                    Console.WriteLine("msgType" + msgType);

                    if (msgType == MessagesTypes.ChannelConnectRequest)//1 = channel request sync
                    {
                        answer = connectSystem.Process(ParsePacketRequest<PacketConnectRequest>(reader));
                    }
                    else if (msgType == MessagesTypes.ChannelSyncRequest)//11 = channel request sync
                    {
                        answer = ParseChannelSyncRequestPacket(ParsePacketRequest<PacketSyncRequest>(reader));
                    }
                    else if (msgType == MessagesTypes.ChannelManagementRequest)//Channel management.
                    {
                        answer = channelManagementSystem.Process(ParsePacketRequest<PacketChannelManagementRequest>(reader));
                    }
                    else if (msgType == MessagesTypes.ChannelDataRequest)//15 = channel data
                    {
                        answer = ParseChannelRequestPacket(ParsePacketRequest<PacketChannelRequest>(reader));
                    }
                    else if (msgType == MessagesTypes.ChannelFunctionRequest)//16 = channel function
                    {
                        answer = ProcessFunctionRequestPacket(ParsePacketRequest<PacketFunctionRequest>(reader));
                    }

                    answerWritterFull.WriteObject(answer);
                    //answerWritterFull.Copy(TCPClientstream);
                    WriteData(answerWritterFull);

                }
                else
                {
                    if (_pendingRequests.TryRemove(RequestID, out var tcs))
                    {
                        var obj = reader.ReadObject<BasePacketAnswer>()!;
                        tcs.SetResult(obj);
                    }
                }
            }
            catch
            {
                if (!tcpClient.Connected && Running)
                {
                    ServerStop(RemoteGuid);
                }

                if (Running && tcpClient.Connected)
                {
                    throw;
                }


            }
        }
        Console.WriteLine("exit");
    }


    T ParsePacketRequest<T>(DataReader reader) where T : PacketBaseRequest
    {
        PacketBaseRequest aa = (PacketBaseRequest)Activator.CreateInstance(typeof(T));
        aa.Deserialize(reader);
        return (T)aa;
    }
    private BasePacketAnswer ParseChannelSyncRequestPacket(PacketSyncRequest data)
    {
        LocalChannelManager.AddChannelsSync(data.RemoteChannels, data.RemoteGuid);
        var packet = new PacketSyncAnswer()
        {
            RemoteChannels = LocalChannelManager.GetLocalChannelDescriptors(),
            RemoteGuid = LocalChannelManager.LocalGuid
        };


        return packet;

    }

    private PacketChannelAnswer ParseChannelRequestPacket(PacketChannelRequest data)
    {
        var answer = new PacketChannelAnswer();
        byte msgType = data.Type;
        string channelPath = data.channelPath;


        if (msgType == 0)//Read data from channel
        {
            Console.WriteLine("read data!!" + channelPath);
            answer.Result = ReadChannel<object>(channelPath);
        }
        else if (msgType == 1)//Write data to channel
        {
            LocalChannelManager.SetLocalValue(channelPath, data.Val);
        }
        return answer;

    }



    private BasePacketAnswer ProcessFunctionRequestPacket(PacketFunctionRequest data)
    {
        var answer = new PacketFunctionAnswer();
        string channelPath = data.ChannelPath;

        var funcOut = LocalChannelManager.RunLocalFunc(channelPath, data.FuncID, data.Params);
        answer.Out = funcOut;
        return answer;

    }



    private DataWritter GenerateAnwserHeader(uint RequestID)
    {
        var writter = new DataWritter();
        writter.Write(RequestID);
        writter.Write(true);
        return writter;
    }
    private DataWritter GenerateRequestHeader(uint RequestID)
    {
        var writter = new DataWritter();
        writter.Write(RequestID);
        writter.Write(false);
        return writter;

    }
    private T? ReadChannel<T>(string channelPath)
    {
        return LocalChannelManager.GetLocalValue<T>(channelPath);
    }




}
public static class MessagesTypes
{
    public static byte ChannelConnectRequest = 10;
    public static byte ChannelSyncRequest = 11;
    public static byte ChannelManagementRequest = 12;
    public static byte ChannelDataRequest = 15;
    public static byte ChannelFunctionRequest = 16;


}