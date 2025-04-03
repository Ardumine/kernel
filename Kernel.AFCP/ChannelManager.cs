using Kernel.AFCP.KernelManagement;
using Kernel.AFCP.Packets;
using Kernel.Logging;

namespace Kernel.AFCP;
public class ChannelManager : IDisposable
{
    private List<ChannelBase> Channels = new List<ChannelBase>();
    private RunningKernel LocalKernel;
    private Logger _logger;


    public ChannelManager(RunningKernel localKernel, Logger logger)
    {
        LocalKernel = localKernel;
        _logger = logger;
    }


    /// <summary>
    /// Check if a channel exists by a path
    /// </summary>
    /// <param name="Path">The path of the channel</param>
    /// <returns>If the channel exists or not</returns>
    public bool IsChannelPathTaken(string Path)
    {
        return Channels.Any(item => item.Path == Path);
    }


    /// <summary>
    /// Creates a local data channel
    /// </summary>
    /// <typeparam name="T">The data channel value type</typeparam>
    /// <param name="Path">The path of the channel</param>
    /// <returns>The descriptor for the created channel.</returns>
    /// <exception cref="ChannelPathAlreadyTaken">Thrown if the Path is already taken.</exception>
    public DataChannelDescriptor? CreateLocalDataChannel<T>(string Path)
    {
        //Checks
        if (IsChannelPathTaken(Path))
        {
            throw new ChannelPathAlreadyTaken($"The path {Path} is already taken by another channel.");
        }

        var interf = new LocalDataChannelInterface<T>(this)
        {
            IsLocal = true,
            OwningKernel = LocalKernel.Guid,
            Path = Path,
            OwningKernelDesc = LocalKernel,
            Exists = true,
            IsDataChannel = true,
            DataType = typeof(T).AssemblyQualifiedName!
        };

        Channels.Add(interf);

        //NoitifyNewChannelCreateLocally(channel);
        return interf;
    }

    /// <summary>
    /// Creates a local module channel
    /// </summary>
    /// <param name="Path">The path fot the module channel</param>
    /// <param name="func">Function to map the calls by index to params(func index, func params, func return val)</param>
    /// <exception cref="ChannelPathAlreadyTaken">Thrown if the Path is already taken.</exception>
    public void CreateLocalModuleChannel(string Path, Func<uint, object?[]?, object?> func)
    {
        //Checks
        if (IsChannelPathTaken(Path))
        {
            throw new ChannelPathAlreadyTaken($"The path {Path} is already taken by another channel.");
        }

        var channel = new ModuleChannel(this)
        {
            IsLocal = true,
            Path = Path,
            OwningKernel = LocalKernel.Guid,
            IsDataChannel = false,
            LocalTargetFunction = func,
            OwningKernelDesc = LocalKernel,
            Exists = true,
            DataType = "module channel no type"
        };
        channel.StartLocalEvents();

        Channels.Add(channel);
    }


    public void NoitifyChannelDataChanged(DataChannelDescriptor channel, object? newVal)
    {
        foreach (var kernel in channel.KernelsToNotify)
        {
            NoitifyChannelDataChanged(kernel, channel.Path, newVal);
        }

    }

    public void NoitifyChannelDataChanged(RunningKernel kernel, string channelPath, object? newVal)
    {
        kernel.SendRequest<PacketChannelManagementAnswer>(MessagesTypes.ChannelManagementRequest, new PacketChannelManagementRequest()
        {
            ChannelPath = channelPath,
            MsgType = ChannelMessageSyncMessageTypes.ChannelDataChange,
            NewVal = newVal,
            SourceKernelGuid = LocalKernel.Guid
        });
    }


    /// <summary>
    /// Gets a data channel descriptor by Path
    /// </summary>
    /// <param name="Path">The Path for the channel</param>
    /// <returns>The channel descriptor. Returns null when the channel is not found.</returns>
    public DataChannelDescriptor? GetDataChannelDescriptor(string? Path)
    {
        var channel = Channels.Where(ch => ch.Path == Path && ch.IsDataChannel && ch is DataChannelDescriptor).FirstOrDefault();
        return (DataChannelDescriptor?)channel;
    }


    /// <summary>
    /// Gets a local data channel descriptor by Path
    /// </summary>
    /// <param name="Path">The Path for the channel</param>
    /// <returns>The channel descriptor. Returns null when the channel is not found.</returns>
    public DataChannelDescriptor? TryGetLocalChannelDescriptor(string Path)
    {
        return (DataChannelDescriptor?)Channels.Where(ch => ch.Path == Path && ch.IsLocal).FirstOrDefault();
    }


    /// <summary>
    /// Gets a module channel by Path
    /// </summary>
    /// <param name="Path">The Path for the channel</param>
    /// <returns>The module channel. Returns null when the channel is not found.</returns>
    public ModuleChannel? GetModuleChannel(string Path)
    {
        return (ModuleChannel?)Channels.Where(ch => ch.Path == Path).FirstOrDefault();
    }


    #region Sync

    /// <summary>
    /// Gets the descriptors for the local channels. Used for syncing between Kernels.
    /// </summary>
    /// <returns>The channel descriptors.</returns>
    public ChannelDescriptor[] GetLocalChannelDescriptors()
    {
        var descriptors = new List<ChannelDescriptor>();
        foreach (var channel in Channels.Where(ch => ch.IsLocal))
        {
            if (channel.IsDataChannel)
            {
                var dataChannel = (DataChannelDescriptor)channel;
                descriptors.Add(new ChannelDescriptor()
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
                descriptors.Add(new ChannelDescriptor()
                {
                    IsLocal = false,
                    Path = channel.Path,
                    OwningKernel = LocalKernel.Guid,
                    IsDataChannel = false,
                    DataType = "module channel no type 2"
                });
            }
        }
        return descriptors.ToArray();
    }


    /// <summary>
    /// Adds a list channels that was sent by a remote kernel. Used for syncing between Kernels.
    /// </summary>
    /// <param name="remoteChannels">The remote kernel channels</param>
    /// <param name="RemoteKernel">The remote kernel</param>
    /// <exception cref="ChannelPathAlreadyTaken">Thrown if the Path is already taken.</exception>
    public void AddChannelsSync(List<ChannelDescriptor> remoteChannels, RunningKernel RemoteKernel)
    {
        foreach (var channel in remoteChannels)
        {
            if (IsChannelPathTaken(channel.Path))
            {
                throw new ChannelPathAlreadyTaken($"Error in adding list of channels. The path is already taken. Path: {channel.Path}");
            }

            if (channel.IsDataChannel)
            {
                var d1 = typeof(RemoteDataChannelInterface<>);
                Type[] typeArgs = { Type.GetType(channel.DataType)! };
                var makeme = d1.MakeGenericType(typeArgs);
                DataChannelInterface ch = (DataChannelInterface)Activator.CreateInstance(makeme, [this, channel.Path])!;


                ch.IsLocal = false;
                ch.OwningKernel = RemoteKernel.Guid;
                ch.IsDataChannel = true;
                ch.OwningKernelDesc = RemoteKernel;
                ch.Exists = true;
                ch.DataType = channel.DataType;

                Channels.Add(ch);
            }
            else
            {
                Channels.Add(new ModuleChannel(this)
                {
                    IsLocal = false,
                    Path = channel.Path,
                    OwningKernel = RemoteKernel.Guid,
                    IsDataChannel = false,
                    OwningKernelDesc = RemoteKernel,
                    Exists = true,
                    DataType = "module channel no type 3"
                });
            }
        }
    }

    #endregion


    /// <summary>
    /// When this kernel changes the value of another's kernel channel, we must notify the channel owner of this change.
    /// </summary>
    /// <param name="OwningKernel">The remote kernel</param>
    /// <param name="channelPath">The data channel</param>
    /// <param name="newVal">The new value for the channel</param>
    public void SetValueRemote(RunningKernel OwningKernel, string channelPath, object? newVal)
    {
        PacketChannelRequest packet = new PacketChannelRequest()
        {
            WriteData = true,
            channelPath = channelPath,
            Val = newVal,
            SourceKernelGuid = LocalKernel.Guid
        };

        OwningKernel.SendRequest<PacketChannelAnswer>(MessagesTypes.ChannelDataRequest, packet);
    }
    public void AddEventRemote(RunningKernel OwningKernel, string channelPath)
    {
        PacketChannelManagementRequest packet = new PacketChannelManagementRequest()
        {
            MsgType = ChannelMessageSyncMessageTypes.AddEventChannel,
            ChannelPath = channelPath,
            SourceKernelGuid = LocalKernel.Guid
        };


        OwningKernel.SendRequest<PacketChannelManagementAnswer>(MessagesTypes.ChannelManagementRequest, packet);
    }
    public void RemoveEventRemote(RunningKernel OwningKernel, string channelPath)
    {
        PacketChannelManagementRequest packet = new PacketChannelManagementRequest()
        {
            MsgType = ChannelMessageSyncMessageTypes.RemoveEventChannel,
            ChannelPath = channelPath,
            SourceKernelGuid = LocalKernel.Guid
        };

        OwningKernel.SendRequest<PacketChannelManagementAnswer>(MessagesTypes.ChannelManagementRequest, packet);
    }


    public T? GetValueRemote<T>(RunningKernel OwningKernel, string channelPath)
    {
        PacketChannelRequest packet = new PacketChannelRequest()
        {
            WriteData = false,
            channelPath = channelPath,
            SourceKernelGuid = LocalKernel.Guid
        };

        var res = OwningKernel.SendRequest<PacketChannelAnswer>(MessagesTypes.ChannelDataRequest, packet);

        return (T?)res?.Result;//Simulate
    }

    public object? RunFuncRemote(RunningKernel OwningKernel, string channelPath, uint FuncID, object?[]? Params)
    {
        PacketModFuncRequest packet = new PacketModFuncRequest()
        {
            ChannelPath = channelPath,
            FuncID = FuncID,
            Params = Params,
            SourceKernelGuid = LocalKernel.Guid
        };


        return OwningKernel.SendRequest<PacketModFuncAnswer>(MessagesTypes.ChannelFunctionRequest, packet)?.Out;//Simulate
    }


    //When a remote channel asks to change a local value
    public void SetLocalValue<T>(string channelPath, T newVal)
    {
        var channel = GetDataChannelInterface<T>(channelPath);
        if (channel != null)
        {
            channel.Set(newVal);
        }
        else
        {
            _logger.LogE($"Error setting local data channel value. Data Channel path: {channelPath}");
        }
    }
    public T? GetLocalValue<T>(string channelPath)
    {
        DataChannelInterface? dataChannel = (DataChannelInterface?)Channels.Where(inter => inter.Path == channelPath && inter is DataChannelInterface).FirstOrDefault();

        return (T?)dataChannel?.ValueAsObj;
    }

    public DataChannelInterface<T>? GetDataChannelInterface<T>(string Path)
    {
        var dataChannel = GetDataChannelDescriptor(Path);
        if (dataChannel == null)
        {
            throw new NullReferenceException($"The data Channel with the Path {Path} was not found.");
        }
        var channelInterface = (DataChannelInterface<T>?)Channels.Where(inter => inter.Path == Path && inter is DataChannelInterface<T>).FirstOrDefault();
        return channelInterface;
    }

    public DataChannelInterface? GetDataChannelInterface(string Path)
    {
        var dataChannel = GetDataChannelDescriptor(Path);
        if (dataChannel == null)
        {
            throw new NullReferenceException($"The data Channel with the Path {Path} was not found.");
        }

        var channelInterface = (DataChannelInterface?)Channels.Where(inter => inter.Path == Path && inter is DataChannelInterface).FirstOrDefault();
        return channelInterface;
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



    public void RemoveKernel(RunningKernel kernel)
    {
        Channels.RemoveAll(ch => ch.OwningKernel == kernel.Guid);
        Channels.ForEach(ch =>
        {
            if (ch is DataChannelDescriptor)
            {
                ((DataChannelDescriptor)ch).KernelsToNotify.Remove(kernel);
            }
        });
    }


    public void Dispose()
    {
        Channels.Clear();
    }
}