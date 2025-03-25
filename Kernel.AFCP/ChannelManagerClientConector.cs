using System.Collections.Concurrent;

using Kernel.AFCP.DataTreatment;
using Kernel.AFCP.KernelManagement;
using Kernel.AFCP.Packets;
using Kernel.AFCP.Systems;

namespace Kernel.AFCP;

public class ChannelManagerAfcpClientConector
{
    public ChannelManager LocalChannelManager { get; init; }
    public RemoteRunningKernel? RemoteKernel { get; set; }



    private AfcpTCPClient _afcpClient;
    private ConcurrentDictionary<uint, TaskCompletionSource<BasePacketAnswer>> _pendingRequests = new ConcurrentDictionary<uint, TaskCompletionSource<BasePacketAnswer>>();
    private ConnectSystem _connectSystem;
    private ChannelManagementSystem _channelManagementSystem;
    private Serializer _Serializer;
    private KernelManager KernelManager;
    private LocalRunningKernel LocalKernel;

    private bool Running { get; set; } = false;

    //Are we a server?
    public ChannelManagerAfcpClientConector(AfcpTCPClient client, ChannelManager _localChannelManager, LocalRunningKernel _LocalKernel, KernelManager _KernelManager)
    {
        _afcpClient = client;
        LocalChannelManager = _localChannelManager;
        _Serializer = new();


        _afcpClient.OnDataRec += OnDataRec;
        _afcpClient.OnClientDisconnect += OnDisconnect;

        LocalKernel = _LocalKernel;
        KernelManager = _KernelManager;

        _channelManagementSystem = new ChannelManagementSystem(LocalChannelManager, KernelManager);
        _connectSystem = new()
        {
            channelManagerClient = this,
            LocalKernel = LocalKernel,
            KernelManager = KernelManager
        };
        Running = true;
    }

    //Are we a client?
    public ChannelManagerAfcpClientConector(string IP, int Port, ChannelManager _localChannelManager, LocalRunningKernel _LocalKernel, KernelManager _KernelManager)
    {
        LocalChannelManager = _localChannelManager;
        _Serializer = new();
        _afcpClient = new AfcpTCPClient();
        _afcpClient.Connect(IP, Port);


        _afcpClient.OnDataRec += OnDataRec;
        _afcpClient.OnClientDisconnect += OnDisconnect;

        LocalKernel = _LocalKernel;
        KernelManager = _KernelManager;

        _channelManagementSystem = new ChannelManagementSystem(LocalChannelManager, KernelManager);
        _connectSystem = new()
        {
            channelManagerClient = this,
            LocalKernel = LocalKernel,
            KernelManager = KernelManager
        };
        Running = true;

    }






    public void RequestDisconect()
    {
        var connectResponse = SendRequest<PacketConnectAnswer>(MessagesTypes.ChannelConnectRequest, new PacketConnectRequest()
        {
            SourceKernel = LocalKernel.Guid,
            Disconnect = true
        });

        _afcpClient.OnDisconnect(DisconnectReason.RequestedBySelf);

        Running = false;

    }


    //When another asks ToList() us stop, then this runs. Only if this client is an server and not a client, run this.
    public void OnRemoteDisconnect()
    {
        foreach (var item in _pendingRequests.Values)
        {
            item.SetResult(null!);
        }

        _pendingRequests.Clear();
        _afcpClient.OnDisconnect(DisconnectReason.RequestedByRemote);
        Running = false;

    }


    private void OnDisconnect(DisconnectReason reason)
    {
        if (RemoteKernel != null)
        {
            LocalChannelManager.RemoveKernel(RemoteKernel);
        }

        _pendingRequests.Clear();
        Running = false;

    }

    private void OnDataRec(DataReader reader)
    {
        uint RequestID = reader.ReadUint();
        bool isAnswer = reader.ReadBool();
        byte msgType = reader.ReadByte();


        if (!isAnswer)//Request/Question
        {
            var answerWritterFull = GenerateAnwserHeader(RequestID);
            answerWritterFull.Write(msgType);

            BasePacketAnswer answer;

            //answerWritterFull.WriteByte(msgType);
            //Console.WriteLine("Received msgType Req: " + msgType);
            if (msgType == MessagesTypes.ChannelConnectRequest)//1 = channel request sync
            {
                answer = _connectSystem.Process(ParsePacketRequest<PacketConnectRequest>(reader));
            }
            else if (msgType == MessagesTypes.ChannelSyncRequest)//11 = channel request sync
            {
                answer = ParseChannelSyncRequestPacket(ParsePacketRequest<PacketSyncRequest>(reader));
            }
            else if (msgType == MessagesTypes.ChannelManagementRequest)//12 Channel management.
            {
                answer = _channelManagementSystem.Process(ParsePacketRequest<PacketChannelManagementRequest>(reader));
            }
            else if (msgType == MessagesTypes.ChannelDataRequest)//15 = channel data
            {
                answer = ParseChannelRequestPacket(ParsePacketRequest<PacketChannelRequest>(reader));
            }
            else if (msgType == MessagesTypes.ChannelFunctionRequest)//16 = channel function
            {
                answer = ProcessFunctionRequestPacket(ParsePacketRequest<PacketModFuncRequest>(reader));
            }
            else
            {
                throw new Exception($"Unknow message type! Msg type received: {msgType}");
            }



            _Serializer.Serialize(answerWritterFull.memoryStream, answer);
            //answerWritterFull.Copy(TCPClientstream);
            _afcpClient.WriteData(answerWritterFull);

        }
        else
        {
            if (_pendingRequests.TryRemove(RequestID, out var tcs))//When it receives data and it is an awsner.
            {
                BasePacketAnswer Obj;
                if (msgType == MessagesTypes.ChannelConnectRequest)//1 = channel request sync
                {
                    Obj = _Serializer.Deserialize<PacketConnectAnswer>(reader.stream);
                }
                else if (msgType == MessagesTypes.ChannelSyncRequest)//11 = channel request sync
                {
                    Obj = _Serializer.Deserialize<PacketSyncAnswer>(reader.stream);
                }
                else if (msgType == MessagesTypes.ChannelManagementRequest)//12 Channel management.
                {
                    Obj = _Serializer.Deserialize<PacketChannelManagementAnswer>(reader.stream);
                }
                else if (msgType == MessagesTypes.ChannelDataRequest)//15 = channel data
                {
                    Obj = _Serializer.Deserialize<PacketChannelAnswer>(reader.stream);
                }
                else if (msgType == MessagesTypes.ChannelFunctionRequest)//16 = channel function
                {
                    Obj = _Serializer.Deserialize<PacketModFuncAnswer>(reader.stream);
                }
                else
                {
                    throw new Exception($"Unknow message type! Msg type received: {msgType}");
                }

                tcs.SetResult(Obj);
            }
        }

    }


    private uint LatestRequestID = 0;
    public T? SendRequest<T>(byte RequestType, PacketBaseRequest Payload) where T : BasePacketAnswer
    {
        uint RequestID = LatestRequestID++;
        var tcs = new TaskCompletionSource<BasePacketAnswer>();
        _pendingRequests[RequestID] = tcs;

        var writter = GenerateRequestHeader(RequestID);

        //Console.WriteLine($"Request type send: {RequestType}");
        writter.Write(RequestType);

        _Serializer.Serialize(writter.memoryStream, Payload);

        _afcpClient.WriteData(writter);
        while (!tcs.Task.IsCompleted && Running)
            tcs.Task.Wait(500);
        if (!Running)
        {
            return default;
        }
        _pendingRequests.Remove(RequestID, out var _);

        return (T)tcs.Task.Result;

    }


    T ParsePacketRequest<T>(DataReader reader) where T : PacketBaseRequest
    {
        return _Serializer.Deserialize<T>(reader.stream);
    }


    /// <summary>
    /// When we are requested to send our channels as a client
    /// From server to client(US)
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private BasePacketAnswer ParseChannelSyncRequestPacket(PacketSyncRequest data)
    {
        KernelManager.OnSyncChannelsPacketReceived(data.SourceHostingDataChannels.ToList(), RemoteKernel!);
        var packet = new PacketSyncAnswer()
        {
            RemoteHostingChannels = LocalChannelManager.GetLocalChannelDescriptors(),
            RemoteGuid = LocalKernel.Guid
        };


        return packet;

    }

    private PacketChannelAnswer ParseChannelRequestPacket(PacketChannelRequest data)
    {
        var answer = new PacketChannelAnswer()
        {
            Result = 0
        };
        string channelPath = data.channelPath;


        if (!data.WriteData)//Read data from channel
        {
            answer.Result = ReadChannel<object>(channelPath);
        }
        else//Write data ToList() channel  if (msgType == 1)
        {
            LocalChannelManager.SetLocalValue(channelPath, data.Val);
        }
        return answer;

    }



    private BasePacketAnswer ProcessFunctionRequestPacket(PacketModFuncRequest data)
    {
        var answer = new PacketModFuncAnswer();
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