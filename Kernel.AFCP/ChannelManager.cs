using Kernel.AFCP.Packets;

namespace Kernel.AFCP;
public class ChannelManager
{
    private List<ChannelBase> Channels = new List<ChannelBase>();

    private  KernelDescriptor LocalKernel;



    /// <summary>
    /// Maps a Kernel GUID to a client.
    /// </summary>
    internal Dictionary<Guid, ChannelManagerAfcpClientConector> Externals = new Dictionary<Guid, ChannelManagerAfcpClientConector>();



    public ChannelManager(KernelDescriptor localKernel)
    {
        LocalKernel = localKernel;
    }
    public void Stop()
    {
        foreach (ChannelManagerAfcpClientConector client in Externals.Values)
        {
            client.RequestDisconect();
        }
    }

    public void RemoveKernel(ConnectedKernel? kernel)
    {
        Channels.RemoveAll(ch => ch.OwningKernel == kernel?.Guid);
        Channels.ForEach(ch =>
        {
            if (ch is DataChannelDescriptor)
            {
                ((DataChannelDescriptor)ch).KernelsToNotify.Remove(kernel!);
            }
        });
    }


    public DataChannelDescriptor? CreateLocalDataChannel<T>(string Path)
    {
        var channel = new DataChannelDescriptor()
        {
            DataType = typeof(T).AssemblyQualifiedName,
            IsLocal = true,
            OwningKernel = LocalKernel.Guid,
            IsDataChannel = true,
            Path = Path,
        };
        var interf = new LocalDataChannelInterface<T>(this)
        {
            IsLocal = true,
            OwningKernel = LocalKernel.Guid,
            Path = Path,
            DataChannel = channel
        };

        dataChannelInterfaces.Add(interf);
        Channels.Add(channel);

        NoitifyNewChannelCreateLocally(channel);
        return channel;
    }




    public void CreateLocalModuleChannel(string Path, Func<uint, object?[]?, object?> func)
    {
        var channel = new ModuleChannel(this)
        {
            IsLocal = true,
            Path = Path,
            OwningKernel = LocalKernel.Guid,
            IsDataChannel = false,
            LocalTargetFunction = func
        };
        channel.StartLocalEvents();


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
    public void NoitifyChannelDataChanged(DataChannelDescriptor channel, object? newVal)
    {
        foreach (var kernel in channel.KernelsToNotify)
        {
            NoitifyChannelDataChanged(kernel, channel.Path, newVal);
        }


    }

    public void NoitifyChannelDataChanged(ConnectedKernel kernel, string channelPath, object? newVal)
    {
        kernel.ConnectedVia.SendRequest<PacketChannelManagementAnswer>(MessagesTypes.ChannelManagementRequest, new PacketChannelManagementRequest()
        {
            ChannelPath = channelPath,
            MsgType = ChannelMessageSyncMessageTypes.ChannelDataChange,
            NewVal = newVal,
            RequestingKernel = LocalKernel.Guid
        });
    }

    public DataChannelDescriptor? GetDataChannel(string? Path)
    {
        var channel = Channels.Where(ch => ch.Path == Path && ch.IsDataChannel && ch is DataChannelDescriptor).FirstOrDefault();
        return (DataChannelDescriptor?)channel;
    }

    public DataChannelDescriptor? TryGetLocalChannel(string Path)
    {
        return (DataChannelDescriptor?)Channels.Where(ch => ch.Path == Path && ch.IsLocal).FirstOrDefault();
    }


    public ModuleChannel GetModuleChannel(string Path)
    {
        return (ModuleChannel)Channels.Where(ch => ch.Path == Path).First();
    }




    public ChannelDescriptor[] GetLocalChannelDescriptors()
    {
        var descriptors = new List<ChannelDescriptor>();
        foreach (var channel in Channels.Where(ch => ch.IsLocal))
        {
            if (channel.IsDataChannel)
            {
                var dataChannel = (DataChannelDescriptor)channel;
                descriptors.Add(new DataChannelDescriptor()
                {
                    IsLocal = false,
                    Path = dataChannel.Path,
                    OwningKernel = LocalKernel.Guid,
                    IsDataChannel = true,
                    DataType = dataChannel.DataType
                });
            }
            else
            {
                descriptors.Add(new ModuleChannelDescriptor()
                {
                    IsLocal = false,
                    Path = channel.Path,
                    OwningKernel = LocalKernel.Guid,
                    IsDataChannel = false
                });
            }
        }
        return descriptors.ToArray();
    }


    public void AddChannelsSync(List<ChannelDescriptor> remoteChannels, ConnectedKernel RemoteKernel)
    {
        foreach (var channel in remoteChannels)
        {
            if (channel.IsDataChannel)
            {
                var dataChannel = (DataChannelDescriptor)channel;
                Channels.Add(new DataChannelDescriptor()
                {
                    IsLocal = false,
                    Path = dataChannel.Path,
                    OwningKernel = RemoteKernel.Guid,
                    IsDataChannel = true,
                    DataType = dataChannel.DataType
                });
            }
            else
            {
                Channels.Add(new ModuleChannel(this)
                {
                    IsLocal = false,
                    Path = channel.Path,
                    OwningKernel = RemoteKernel.Guid,
                    IsDataChannel = false
                });
            }
        }
    }


    //When channel is remote, we use this functions to talk to the remote kernel.
    public void SetValueRemote(Guid OwningChannelManager, string channelPath, object? newVal)
    {
        PacketChannelRequest packet = new PacketChannelRequest()
        {
            WriteData = true,
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
            RequestingKernel = LocalKernel.Guid
        };


        Externals[OwningChannelManager].SendRequest<PacketChannelManagementAnswer>(MessagesTypes.ChannelManagementRequest, packet);
    }
    public void RemoveEventRemote(Guid OwningChannelManager, string channelPath)
    {
        PacketChannelManagementRequest packet = new PacketChannelManagementRequest()
        {
            MsgType = ChannelMessageSyncMessageTypes.RemoveEventChannel,
            ChannelPath = channelPath,
            RequestingKernel = LocalKernel.Guid
        };

        Externals[OwningChannelManager].SendRequest<PacketChannelManagementAnswer>(MessagesTypes.ChannelManagementRequest, packet);
    }


    public T? GetValueRemote<T>(Guid OwningChannelManager, string channelPath)
    {
        PacketChannelRequest packet = new PacketChannelRequest()
        {
            WriteData = false,
            channelPath = channelPath,
        };


        var res = Externals[OwningChannelManager].SendRequest<PacketChannelAnswer>(MessagesTypes.ChannelDataRequest, packet);


        return (T?)res.Result;//Simulate
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
        GetDataChannelInterface<T>(channelPath).Set(newVal);
    }
    public T? GetLocalValue<T>(string channelPath)
    {
        DataChannelInterface? dataChannel = dataChannelInterfaces.Where(inter => inter.Path == channelPath).FirstOrDefault();

        return (T?)dataChannel?.ValueAsObj;
    }

    public List<DataChannelInterface> dataChannelInterfaces = new();
    public DataChannelInterface<T> GetDataChannelInterface<T>(string Path)
    {
        var dataChannel = GetDataChannel(Path);
        var channelInterface = dataChannelInterfaces.Where(inter => inter.Path == Path).FirstOrDefault();
        if (channelInterface == null && !dataChannel!.IsLocal)
        {
            return new RemoteDataChannelInterface<T>(this, Path)
            {
                IsLocal = dataChannel.IsLocal,
                Path = Path,
                OwningKernel = dataChannel.OwningKernel,
                DataChannel = dataChannel
            };
        }
        return (DataChannelInterface<T>)channelInterface!;
    }

    public DataChannelInterface GetDataChannelInterface(string Path)
    {
        var dataChannel = GetDataChannel(Path)!;
        var channelInterface = dataChannelInterfaces.Where(inter => inter.Path == Path).FirstOrDefault();
        if (channelInterface == null && !dataChannel.IsLocal)
        {
            channelInterface = new DataChannelInterface()
            {
                IsLocal = dataChannel.IsLocal,
                Path = dataChannel.Path,
                OwningKernel = dataChannel.OwningKernel,
                DataChannel = dataChannel
            };
        }
        return channelInterface!;
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