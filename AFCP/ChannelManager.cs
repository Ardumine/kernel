using System.Text.Json;
using AFCP.DataTreatment;
using AFCP.Packets;

namespace AFCP;
public class ChannelManager
{
    public static ChannelManager? Defaulte { get; set; }
    public required List<KernelDescriptor> ConnectedKernels;
    private List<ChannelBase> Channels = new List<ChannelBase>();

    public Guid LocalGuid { get; set; }


    /// <summary>
    /// Maps a Kernel GUID to a client.
    /// </summary>
    private Dictionary<Guid, ChannelManagerClient> Externals = new Dictionary<Guid, ChannelManagerClient>();



    public ChannelManager()
    {
        LocalGuid = Guid.NewGuid();
    }
    public void Stop()
    {
        foreach (ChannelManagerClient external in Externals.Values)
        {
            external.RequestStopAndStop();
        }
    }

    public void RemoveKernel(Guid kernelGuid)
    {
        Channels.RemoveAll(ch => ch.OwningKernel == kernelGuid);
        Channels.ForEach(ch =>
        {
            if (ch is DataChannel)
            {
                ((DataChannel)ch).KernelsSubscribed.Remove(kernelGuid);
            }
        });

        //Remove every event
        ConnectedKernels.RemoveAll(k => k.KernelGuid == kernelGuid);

    }

    public DataChannel CreateLocalDataChannel<T>(string Path, bool highSpeed)
    {
        var interf = new LocalDataChannelInterface<T>(this)
        {
            DataType = typeof(T).AssemblyQualifiedName,
            HighSpeed = highSpeed,
            IsLocal = true,
            OwningKernel = LocalGuid,
            Path = Path
        };

        var channel = new DataChannel()
        {
            DataType = typeof(T).AssemblyQualifiedName,
            HighSpeed = highSpeed,
            IsLocal = true,
            OwningKernel = LocalGuid,
            IsDataChannel = true,
            Path = Path,
        };

        dataChannelInterfaces.Add(interf);
        Channels.Add(channel);

        NoitifyNewChannelCreateLocally(channel);
        return channel;
    }




    public void CreateLocalModuleChannel(string Path, Func<uint, object?[]?, object?> func, bool highSpeed)
    {
        var channel = new ModuleChannel(this)
        {
            IsLocal = true,
            Path = Path,
            OwningKernel = LocalGuid,
            IsDataChannel = false,
            LocalTargetFunction = func,
            DataType = null,
            HighSpeed = highSpeed,

        };

        Channels.Add(channel);
        //NoitifyNewChannelCreateLocally(channel);
    }

    public void NoitifyNewChannelCreateLocally(ChannelDescriptor descriptor)
    {
        foreach (var item in Externals.Values)
        {
            //item.SendRequest(12, );
        }
    }
    public void NoitifyChannelDataChanged(string channelPath, string dataType, object? newVal)
    {
        var channel = GetDataChannel(channelPath)!;
        Parallel.For(0, channel.KernelsToNotify.Count, (i) =>
         {
             NoitifyChannelDataChanged(channel.KernelsToNotify[i], channelPath, dataType, newVal);
         });
    }

    public void NoitifyChannelDataChanged(Guid KernelGuid, string channelPath, string dataType, object? newVal)
    {
        var kernel = ConnectedKernels.Where(k => k.KernelGuid == KernelGuid).FirstOrDefault();
        if (kernel is not null)
        {
            kernel.channelManagerClient.SendRequest<PacketChannelManagementAnswer>(MessagesTypes.ChannelManagementRequest, new PacketChannelManagementRequest()
            {
                ChannelPath = channelPath,
                MsgType = ChannelMessageSyncMessageTypes.ChannelDataChange,
                NewVal = newVal,
                DataType = dataType
            });
        }
    }

    public DataChannel? GetDataChannel(string? Path)
    {
        var channel = Channels.Where(ch => ch.Path == Path && ch.IsDataChannel).FirstOrDefault();
        return (DataChannel?)channel;
    }

    public DataChannel? TryGetLocalChannel(string Path)
    {
        return (DataChannel?)Channels.Where(ch => ch.Path == Path && ch.IsLocal).FirstOrDefault();
    }


    public ModuleChannel GetFunctionChannel(string Path)
    {
        return (ModuleChannel)Channels.Where(ch => ch.Path == Path).First();
    }
    public ModuleChannel GetModuleChannel(string path)
    {
        return (ModuleChannel)Channels.Where(ch => ch.Path == path).First();
    }

    public void Join(string IPAddress, int port)
    {

        var client = new ChannelManagerClient(IPAddress, port, this);

        var packet = new PacketSyncRequest()
        {
            RemoteGuid = LocalGuid,
            RemoteChannels = GetLocalChannelDescriptors()
        };
        var answer = client.SendRequest<PacketSyncAnswer>(MessagesTypes.ChannelSyncRequest, packet);
        
        AddChannelsSync(answer.RemoteChannels, answer.RemoteGuid);
        Externals[answer.RemoteGuid] = client;
    }

    private void CreateConnectionForKernel(Guid kernel){
        
    }

    public ChannelDescriptor[] GetLocalChannelDescriptors()
    {
        var descriptors = new List<ChannelDescriptor>();
        foreach (var channel in Channels.Where(ch => ch.IsLocal))
        {
            descriptors.Add(channel);
        }
        return descriptors.ToArray();
    }


    public void AddChannelsSync(ChannelDescriptor[] remoteChannels, Guid RemoteGuid)
    {
        foreach (var channel in remoteChannels)
        {
            if (channel.IsDataChannel)
            {
                Channels.Add(new DataChannel()
                {
                    IsLocal = false,
                    Path = channel.Path,
                    OwningKernel = RemoteGuid,
                    IsDataChannel = true,
                    DataType = channel.DataType,
                    HighSpeed = channel.HighSpeed
                });
            }
            else
            {
                Channels.Add(new ModuleChannel(this)
                {
                    IsLocal = false,
                    Path = channel.Path,
                    OwningKernel = RemoteGuid,
                    IsDataChannel = false,
                    DataType = channel.DataType,
                    HighSpeed = channel.HighSpeed
                });
            }
        }
    }


    //When channel is remote, we use this functions to talk to the remote kernel.
    public void SetValueRemote(Guid OwningChannelManager, string channelPath, object? newVal, bool highData)
    {
        PacketChannelRequest packet = new PacketChannelRequest()
        {
            Type = 1,
            channelPath = channelPath,
            Val = newVal
        };


        Externals[OwningChannelManager].SendRequest<PacketChannelAnswer>(MessagesTypes.ChannelDataRequest, packet);
    }
    public void AddEventRemote(Guid OwningChannelManager, string channelPath)
    {
        PacketChannelManagementRequest packet = new PacketChannelManagementRequest()
        {
            MsgType = ChannelMessageSyncMessageTypes.AddEventChannel,
            ChannelPath = channelPath,
            RequestingKernel = LocalGuid
        };

        Externals[OwningChannelManager].SendRequest<PacketChannelManagementAnswer>(MessagesTypes.ChannelManagementRequest, packet);
    }
    public void RemoveEventRemote(Guid OwningChannelManager, string channelPath)
    {
        PacketChannelManagementRequest packet = new PacketChannelManagementRequest()
        {
            MsgType = ChannelMessageSyncMessageTypes.RemoveEventChannel,
            ChannelPath = channelPath,
            RequestingKernel = LocalGuid
        };

        Externals[OwningChannelManager].SendRequest<PacketChannelManagementAnswer>(MessagesTypes.ChannelManagementRequest, packet);
    }


    public T? GetValueRemote<T>(Guid OwningChannelManager, string channelPath, bool HighData)
    {
        PacketChannelRequest packet = new PacketChannelRequest()
        {
            Type = 0,
            channelPath = channelPath,
        };


        var res = Externals[OwningChannelManager].SendRequest<PacketChannelAnswer>(MessagesTypes.ChannelDataRequest, packet);

        T? myData = res.Result is JsonElement jsonElement
         ? JsonSerializer.Deserialize<T>(jsonElement, DataWritter.JsonOptions)!
         : (T?)res.Result;

        return myData;//Simulate
    }

    public object? RunFuncRemote(Guid OwningChannelManager, string channelPath, uint FuncID, object?[]? Params)
    {
        PacketFunctionRequest packet = new PacketFunctionRequest()
        {
            ChannelPath = channelPath,
            FuncID = FuncID,
            Params = Params,
        };


        return Externals[OwningChannelManager].SendRequest<PacketFunctionAnswer>(MessagesTypes.ChannelFunctionRequest, packet).Out;//Simulate
    }

    //When a remote channel asks to change a local value
    public void SetLocalValue<T>(string channelPath, T newVal)
    {
        GetInterfaceForChannel<T>(channelPath).Set(newVal);
    }
    public T? GetLocalValue<T>(string channelPath)
    {
        var aa = dataChannelInterfaces.Where(inter => inter.Path == channelPath).FirstOrDefault();

        return (T?)aa?.Data;
    }

    public List<DataChannelInterface> dataChannelInterfaces = new();
    public DataChannelInterface<T> GetInterfaceForChannel<T>(string Path)
    {
        var mod = GetDataChannel(Path);
        var aa = dataChannelInterfaces.Where(inter => inter.Path == Path).FirstOrDefault();
        if (aa == null && !mod!.IsLocal)
        {
            return new RemoteDataChannelInterface<T>(this, Path)
            {
                DataType = mod.DataType,
                HighSpeed = mod.HighSpeed,
                IsLocal = mod.IsLocal,
                Path = Path,
                OwningKernel = mod.OwningKernel,
            };;
        }
        return (DataChannelInterface<T>)aa!;
    }

    public DataChannelInterface GetInterfaceForChannel(string Path)
    {
        var mod = GetDataChannel(Path)!;
        var aa = dataChannelInterfaces.Where(inter => inter.Path == Path).FirstOrDefault();
        if (aa == null && !mod.IsLocal)
        {
            aa = new DataChannelInterface()
            {
                DataType = mod.DataType,
                HighSpeed = mod.HighSpeed,
                IsLocal = mod.IsLocal,
                Path = mod.Path,
                OwningKernel = mod.OwningKernel,
            };
            //dataChannelInterfaces.Add(aa);
        }
        return aa!;
    }


    #region Channel function running
    public class FunctionRequestEventArgs : EventArgs
    {
        public required string ChannelPath { get; set; }
        public required uint FunctionID { get; set; }
        public object?[]? Params { get; set; }

        //public object? ReturnValue { get; set; }
    }

    public delegate object? FunctionRequestEventHandler(object sender, FunctionRequestEventArgs args);
    public event FunctionRequestEventHandler? OnFunctionRunRequest;


    public void AddEventFunctionRequest(string ChannelPath, FunctionRequestEventHandler handler)
    {
        OnFunctionRunRequest += (s, e) =>
        {
            if (e.ChannelPath == ChannelPath)
            {
                return handler.Invoke(s, e);
            }
            return null;
        };
    }

    public object? RunLocalFunc(string channelPath, uint FuncID, object?[]? param)
    {
        return OnFunctionRunRequest?.Invoke(this, new()
        {
            ChannelPath = channelPath,
            FunctionID = FuncID,
            Params = param
        });
        //return GetLocalFuncChannel(channelPath).Run(FuncID, param);
    }
    #endregion

    #region Channel data event
    public class ChannelDataEventArgs : EventArgs
    {
        public required string ChannelPath { get; set; }
        public required object? NewData { get; set; }

    }

    public delegate void ChannelDataEventHandler(object sender, ChannelDataEventArgs args);
    public event ChannelDataEventHandler? OnRemoteChannelDataChange;

    internal void OnRemoteDataChange(string channel, object? newData)
    {
        OnRemoteChannelDataChange?.Invoke(this, new()
        {
            ChannelPath = channel,
            NewData = newData
        });
    }
    public void AddEventRemoteDataChange(string ChannelPath, ChannelDataEventHandler handler)
    {
        OnRemoteChannelDataChange += (s, e) =>
        {
            if (e.ChannelPath == ChannelPath)
            {
                handler.Invoke(s, e);
            }
        };
    }

    #endregion


    private ModuleChannel GetLocalFuncChannel(string Path)
    {
        return (ModuleChannel)Channels.Where(ch => ch.Path == Path && ch.IsLocal).First();
    }



}