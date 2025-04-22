using Kernel.AFCP.KernelManagement;
using Kernel.AFCP.Packets;
using Kernel.Logging;

namespace Kernel.AFCP;

public class DataChannelManager
{
    private ChannelManager _channelManager;
    private LocalRunningKernel _localKernel;
    private ILogger _logger;

    public DataChannelManager(ChannelManager channelManager, LocalRunningKernel localKernel, ILogger logger)
    {
        _channelManager = channelManager;
        _localKernel = localKernel;
        _logger = logger;
        channelManager.dataChannelManager = this;
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
        if (_channelManager.IsChannelPathTaken(Path))
        {
            throw new ChannelPathAlreadyTaken($"The path {Path} is already taken by another channel.");
        }

        var interf = new LocalDataChannelInterface<T>(this)
        {
            IsLocal = true,
            OwningKernel = _localKernel.Guid,
            Path = Path,
            OwningKernelDesc = _localKernel,
            Exists = true,
            IsDataChannel = true,
            DataType = typeof(T).AssemblyQualifiedName!
        };
        _channelManager.AddLocalChannel(interf);

        return interf;
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
            SourceKernelGuid = _localKernel.Guid
        });
    }



    /// <summary>
    /// Gets a data channel descriptor by Path
    /// </summary>
    /// <param name="Path">The Path for the channel</param>
    /// <returns>The channel descriptor. Returns null when the channel is not found.</returns>
    public DataChannelDescriptor? GetDataChannelDescriptor(string? Path)
    {
        var channel = _channelManager.Channels.Where(ch => ch.Path == Path && ch.IsDataChannel && ch is DataChannelDescriptor).FirstOrDefault();
        return (DataChannelDescriptor?)channel;
    }


    /// <summary>
    /// Gets a local data channel descriptor by Path
    /// </summary>
    /// <param name="Path">The Path for the channel</param>
    /// <returns>The channel descriptor. Returns null when the channel is not found.</returns>
    public DataChannelDescriptor? GetLocalDataChannelDescriptor(string Path)
    {
        return (DataChannelDescriptor?)_channelManager.Channels.Where(ch => ch.Path == Path && ch.IsLocal && ch.IsDataChannel).FirstOrDefault();
    }

    public DataChannelInterface<T>? GetDataChannelInterface<T>(string Path)
    {
        var dataChannel = GetDataChannelDescriptor(Path);
        if (dataChannel == null)
        {
            throw new NullReferenceException($"The data Channel with the Path {Path} was not found.");
        }
        var channelInterface = (DataChannelInterface<T>?)_channelManager.Channels.Where(inter => inter.Path == Path && inter is DataChannelInterface<T>).FirstOrDefault();
        return channelInterface;
    }

    public DataChannelInterface? GetDataChannelInterface(string Path)
    {
        var dataChannel = GetDataChannelDescriptor(Path);
        if (dataChannel == null)
        {
            throw new NullReferenceException($"The data Channel with the Path {Path} was not found.");
        }

        var channelInterface = (DataChannelInterface?)_channelManager.Channels.Where(inter => inter.Path == Path && inter is DataChannelInterface).FirstOrDefault();
        return channelInterface;
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
        DataChannelInterface? dataChannel = (DataChannelInterface?)_channelManager.Channels.Where(inter => inter.Path == channelPath && inter is DataChannelInterface).FirstOrDefault();

        return (T?)dataChannel?.ValueAsObj;
    }


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
            SourceKernelGuid = _localKernel.Guid
        };

        OwningKernel.SendRequest<PacketChannelAnswer>(MessagesTypes.ChannelDataRequest, packet);
    }

    public void AddEventRemote(RunningKernel OwningKernel, string channelPath)
    {
        PacketChannelManagementRequest packet = new PacketChannelManagementRequest()
        {
            MsgType = ChannelMessageSyncMessageTypes.AddEventChannel,
            ChannelPath = channelPath,
            SourceKernelGuid = _localKernel.Guid
        };


        OwningKernel.SendRequest<PacketChannelManagementAnswer>(MessagesTypes.ChannelManagementRequest, packet);
    }
    public void RemoveEventRemote(RunningKernel OwningKernel, string channelPath)
    {
        PacketChannelManagementRequest packet = new PacketChannelManagementRequest()
        {
            MsgType = ChannelMessageSyncMessageTypes.RemoveEventChannel,
            ChannelPath = channelPath,
            SourceKernelGuid = _localKernel.Guid
        };

        OwningKernel.SendRequest<PacketChannelManagementAnswer>(MessagesTypes.ChannelManagementRequest, packet);
    }
    public T? GetValueRemote<T>(RunningKernel OwningKernel, string channelPath)
    {
        PacketChannelRequest packet = new PacketChannelRequest()
        {
            WriteData = false,
            channelPath = channelPath,
            SourceKernelGuid = _localKernel.Guid
        };

        var res = OwningKernel.SendRequest<PacketChannelAnswer>(MessagesTypes.ChannelDataRequest, packet);

        return (T?)res?.Result;//Simulate
    }

}