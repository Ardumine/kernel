using System.Collections.Concurrent;

using Kernel.AFCP.DataTreatment;
using Kernel.AFCP.Packets;
using Kernel.AFCP.Systems;

namespace Kernel.AFCP;

public class ChannelManagerClient
{

    public ChannelManager LocalChannelManager { get; set; }
    //public AfcpServer? ServerOwner { get; set; }
    // public Guid RemoteGuid = new();

    private AfcpClient afcpClient;

    private ConcurrentDictionary<uint, TaskCompletionSource<BasePacketAnswer>> _pendingRequests = new ConcurrentDictionary<uint, TaskCompletionSource<BasePacketAnswer>>();

    public KernelDescriptor RemoteKernel { get; set; }
    ConnectSystem connectSystem;
    ChannelManagementSystem channelManagementSystem;
    AFCP.KASerializer.KASerializer Serializer;

    public ChannelManagerClient(AfcpClient cl, ChannelManager _localChannelManager)
    {
        afcpClient = cl;
        LocalChannelManager = _localChannelManager;
        Serializer = new();


        
        afcpClient.OnDataRec += OnDataRec;
        afcpClient.OnClientDisconnect += OnDisconnect;


        channelManagementSystem = new(LocalChannelManager);
        connectSystem = new(LocalChannelManager, this);
    }


    public ChannelManagerClient(string IP, int Port, ChannelManager _localChannelManager)
    {

        LocalChannelManager = _localChannelManager;
        Serializer = new();
        afcpClient = new AfcpClient();
        afcpClient.Connect(IP, Port);


        afcpClient.OnDataRec += OnDataRec;
        afcpClient.OnClientDisconnect += OnDisconnect;


        channelManagementSystem = new(LocalChannelManager);
        connectSystem = new(LocalChannelManager, this);

        Thread.Sleep(100);
        OnConnect();//Only when we connect as a client

    }
   




    private void OnConnect()
    {
        HandShake();

    }
    private void HandShake()
    {
        var connectResponse = SendRequest<PacketConnectAnswer>(MessagesTypes.ChannelConnectRequest, new PacketConnectRequest()
        {
            //HostingChannels = LocalChannelManager.GetLocalChannelDescriptors().Where(t => t.IsDataChannel).Select(t => t as DataChannelDescriptor).ToList()!,
            RemoteKernel = LocalChannelManager.LocalGuid,
            Disconnect = false
        });

        var remoteKernel = new KernelDescriptor()
        {
            KernelGuid = connectResponse.RemoteKernel,
            ConnectedVia = this,
            //HostingChannels = connectResponse.HostingChannels,
            //HostingModules = connectResponse.HostingModules,
            channelManagerClient = this
        };
        RemoteKernel = remoteKernel;
        LocalChannelManager.ConnectedKernels.Add(remoteKernel);
    }


    public void RequestDisconect()
    {
        var connectResponse = SendRequest<PacketConnectAnswer>(MessagesTypes.ChannelConnectRequest, new PacketConnectRequest() { RemoteKernel = LocalChannelManager.LocalGuid, Disconnect = true });

        afcpClient.OnDisconnect(DisconnectReason.RequestedBySelf);


    }

    //When another asks to us stop, then this runs. Only if this client is an server and not a client, run this.
    public void OnRemoteDisconnect(Guid remoteKernel)
    {
        Console.WriteLine("Remote requested diconnect.");
        LocalChannelManager.RemoveKernel(LocalChannelManager.GetKernel(remoteKernel)!);

        foreach (var item in _pendingRequests.Values)
        {
            item.SetResult(null!);
        }

        _pendingRequests.Clear();
        afcpClient.OnDisconnect(DisconnectReason.RequestedByRemote);


    }


    private void OnDisconnect(DisconnectReason reason)
    {
        LocalChannelManager.RemoveKernel(RemoteKernel);

        _pendingRequests.Clear();
    }
    private void OnDataRec(DataReader reader)
    {
        uint RequestID = reader.ReadUint();
        bool isAnswer = reader.ReadBool();
        byte msgType = reader.ReadByte();



        if (!isAnswer)//Question
        {
            var answerWritterFull = GenerateAnwserHeader(RequestID);
            answerWritterFull.Write(msgType);

            BasePacketAnswer answer = null!;

            //answerWritterFull.WriteByte(msgType);
            //Console.WriteLine("Requested msgType: " + msgType);
            if (msgType == MessagesTypes.ChannelConnectRequest)//1 = channel request sync
            {
                answer = connectSystem.Process(ParsePacketRequest<PacketConnectRequest>(reader));
            }
            else if (msgType == MessagesTypes.ChannelSyncRequest)//11 = channel request sync
            {
                answer = ParseChannelSyncRequestPacket(ParsePacketRequest<PacketSyncRequest>(reader));
            }
            else if (msgType == MessagesTypes.ChannelManagementRequest)//12 Channel management.
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
            else
            {
                throw new Exception($"Unknow message type! Msg type received: {msgType}");
            }



            Serializer.Serialize(answer, answerWritterFull.memoryStream, false);
            //answerWritterFull.Copy(TCPClientstream);
            afcpClient.WriteData(answerWritterFull);

        }
        else
        {
            if (_pendingRequests.TryRemove(RequestID, out var tcs))//When it receives data and it is an awsner.
            {
                BasePacketAnswer Obj;
                if (msgType == MessagesTypes.ChannelConnectRequest)//1 = channel request sync
                {
                    Obj = Serializer.Deserialize<PacketConnectAnswer>(reader.stream, false)!;
                }
                else if (msgType == MessagesTypes.ChannelSyncRequest)//11 = channel request sync
                {
                    Obj = Serializer.Deserialize<PacketSyncAnswer>(reader.stream, false)!;
                }
                else if (msgType == MessagesTypes.ChannelManagementRequest)//12 Channel management.
                {
                    Obj = Serializer.Deserialize<PacketChannelManagementAnswer>(reader.stream, false)!;
                }
                else if (msgType == MessagesTypes.ChannelDataRequest)//15 = channel data
                {
                    Obj = Serializer.Deserialize<PacketChannelAnswer>(reader.stream, false)!;
                }
                else if (msgType == MessagesTypes.ChannelFunctionRequest)//16 = channel function
                {
                    Obj = Serializer.Deserialize<PacketFunctionAnswer>(reader.stream, false)!;
                }
                else
                {
                    throw new Exception($"Unknow message type! Msg type received: {msgType}");
                }

                //var obj = reader.ReadObject<BasePacketAnswer>()!;
                //obj = Serializer.Deserialize<BasePacketAnswer>(reader.stream, false)!;
                tcs.SetResult(Obj);
            }
        }

    }


    private uint LatestRequestID = 0;
    public T SendRequest<T>(byte RequestType, PacketBaseRequest Payload) where T : BasePacketAnswer
    {
        uint RequestID = LatestRequestID++;
        var tcs = new TaskCompletionSource<BasePacketAnswer>();
        _pendingRequests[RequestID] = tcs;

        var writter = GenerateRequestHeader(RequestID);

        //Console.WriteLine($"Request type send: {RequestType}");
        writter.Write(RequestType);

        Serializer.Serialize(Payload, writter.memoryStream, false);

        afcpClient.WriteData(writter);

        return (T)tcs.Task.Result;

    }


    T ParsePacketRequest<T>(DataReader reader) where T : class, PacketBaseRequest
    {
        return Serializer.Deserialize<T>(reader.stream, false)!;
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
        var answer = new PacketChannelAnswer()
        {
            Result = 0
        };
        string channelPath = data.channelPath;


        if (!data.WriteData)//Read data from channel
        {
            answer.Result = ReadChannel<object>(channelPath);
        }
        else//Write data to channel  if (msgType == 1)
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