using Kernel.AFCP.KernelManagement;
using Kernel.AFCP.Packets;
using Kernel.Logging;

namespace Kernel.AFCP;

public class ChannelManager : IDisposable
{
    internal List<ExistingChannel> Channels = new List<ExistingChannel>();
    private RunningKernel LocalKernel;
    private ILogger _logger;

    //I need to fix this.
    public DataChannelManager? dataChannelManager { get; set; }

    public ChannelManager(RunningKernel localKernel, ILogger logger)
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


    internal void AddLocalChannel(ExistingChannel channel)
    {
        //Checks
        if (IsChannelPathTaken(channel.Path))
            throw new ChannelPathAlreadyTaken($"The path {channel.Path} is already taken by another channel.");

        if (!channel.IsLocal)
            throw new InvalidDataException($"Attemped to create a local channel where the IsLocal flag is set to false.");




        //NoitifyNewChannelCreateLocally(channel);
        Channels.Add(channel);
    }


    /// <summary>
    /// Creates a local module channel
    /// </summary>
    /// <param name="path">The path fot the module channel</param>
    /// <param name="func">Function to map the calls by index to params(func index, func params, func return val)</param>
    /// <exception cref="ChannelPathAlreadyTaken">Thrown if the Path is already taken.</exception>
    public void CreateLocalModuleChannel(string path, Func<uint, object?[]?, object?> func)
    {
        //Checks
        if (IsChannelPathTaken(path))
        {
            throw new ChannelPathAlreadyTaken($"The path {path} is already taken by another channel.");
        }

        var channel = new ModuleChannel(this)
        {
            IsLocal = true,
            Path = path,
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

    public void DeleteLocalModuleChannel(string path)
    {
        var modChannel = GetModuleChannel(path);
        if (modChannel == null)
        {
            throw new NullReferenceException($"The module with the Path {path} was not found.");
        }
        modChannel.StopLocalEvents();
        Channels.Remove(modChannel);

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
                DataChannelInterface ch = (DataChannelInterface)Activator.CreateInstance(makeme, [dataChannelManager, channel.Path])!;


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

    [Obsolete("Add manually the event, see Channels.StartLocalEvents")]
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

    public void RemoveEventFunctionRequest(string ChannelPath, FunctionRequestEventHandler handler)
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