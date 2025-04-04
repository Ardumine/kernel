using Kernel.AFCP.KASerializer.Atributes;
using Kernel.AFCP.KernelManagement;


namespace Kernel.AFCP;
public class DataChannelInterface : DataChannelDescriptor
{
    internal virtual void OnRemoteChange(object? NewVal)
    {

    }
    public virtual object? ValueAsObj { get; set; }



}

public abstract class DataChannelInterface<T> : DataChannelInterface
{
    public List<Action<T?>> Events = new();
    public virtual T? Data { get; set; }

    public override object? ValueAsObj
    {
        get => Get(); set => Set((T?)value);
    }

    public virtual void AddEvent(Action<T?> evente)
    {
        //DO NOT CHANGE THIS. MAKE SURE YOU ARE GETTING A REMOTE OR LOCAL DATA INTERFACE!!!!
        throw new NotImplementedException();
    }
    public virtual void RemoveEvent(Action<T?> evente)
    {
        //DO NOT CHANGE THIS. MAKE SURE YOU ARE GETTING A REMOTE OR LOCAL DATA INTERFACE!!!!
        throw new NotImplementedException();
    }

    public virtual void Set(T? data)
    {
        //DO NOT CHANGE THIS. MAKE SURE YOU ARE GETTING A REMOTE OR LOCAL DATA INTERFACE!!!!
        throw new NotImplementedException();
    }
    public virtual T? Get()
    {
        //DO NOT CHANGE THIS. MAKE SURE YOU ARE GETTING A REMOTE OR LOCAL DATA INTERFACE!!!!
        throw new NotImplementedException();
    }

    //When a remote kernel notifys when a remote channel changes it's data.
    //Channel A is in kernel A.
    //Channel A's data changes.
    //Previously, we had sent a command to that kernel to request to notify this kernel when the channel's data changes.
    //If it changes, that kernel sends an event.
    //When this kernel receives it, it runs all the local events on when that channel data's changes.   
}



public class RemoteDataChannelInterface<T> : DataChannelInterface<T>, IDisposable
{
    private DataChannelManager _dataChannelManager;
    public RemoteDataChannelInterface(DataChannelManager dataChannelManager, string path_)
    {
        Path = path_;
        _dataChannelManager = dataChannelManager;
        Init();
    }
    public void Init()
    {
        //Add event on channel run
        _dataChannelManager.AddEventRemoteDataChange(Path, (s, e) =>
        {
            Parallel.For(0, Events.Count, (i) =>
            {
                var evente = Events[i];
                T? newData = (T?)e.NewData;
                evente(newData);
            });

        });
    }



    public override T? Data { get => Get(); set => Set(value); }

    public override void AddEvent(Action<T?> evente)
    {
        if (Events.Count == 0)
        {
            if (!IsLocal)
            {
                //Console.WriteLine("Add notify!");
                _dataChannelManager.AddEventRemote(OwningKernelDesc, Path);
            }
        }
        Events.Add(evente);
    }
    public override void RemoveEvent(Action<T?> evente)
    {
        Events.Remove(evente);
        if (Events.Count == 0)
        {
            if (!IsLocal)
            {
                _dataChannelManager.RemoveEventRemote(OwningKernelDesc, Path);
            }
        }

    }

    public override void Set(T? Data)
    {
        if (!IsLocal)
        {
            _dataChannelManager.SetValueRemote(OwningKernelDesc, Path, Data);
        }

        Parallel.For(0, Events.Count, (i) =>
        {
            Events[i](Data);
        });
    }

    public override T? Get()
    {
        if (!IsLocal)
        {
            return _dataChannelManager.GetValueRemote<T>(OwningKernelDesc, Path);
        }
        //DONT REMOVE THIS. IF YOU GET HIS THEN YOU ARE DOING SOMETHING WRONG
        throw new NotImplementedException();
    }

    //When a remote kernel notifys when a remote channel changes it's data.
    //Channel A is in kernel A.
    //Channel A's data changes.
    //Previously, we had sent a command to that kernel to request to notify this kernel when the channel's data changes.
    //If it changes, that kernel sends an event.
    //When this kernel receives it, it runs all the local events on when that channel data's changes.   

    internal override void OnRemoteChange(object? NewVal)
    {
        Parallel.For(0, Events.Count, (i) =>
        {
            Events[i]((T?)NewVal);
        });
    }

    public void Dispose()
    {
        Events.Clear();
    }
}
public class LocalDataChannelInterface<T> : DataChannelInterface<T>, IDisposable
{
    private DataChannelManager _dataChannelManager;
    public LocalDataChannelInterface(DataChannelManager dataChannelManager)
    {
        _dataChannelManager = dataChannelManager;
    }

    private T? LocalValue;
    public override T? Data { get => Get(); set => Set(value); }


    public override void AddEvent(Action<T?> evente)
    {
        Events.Add(evente);
    }
    public override void RemoveEvent(Action<T?> evente)
    {
        Events.Remove(evente);
    }

    public override void Set(T? Data)
    {
        if (IsLocal)
        {
            LocalValue = Data;
            _dataChannelManager.NoitifyChannelDataChanged(this, LocalValue);
            //Console.WriteLine("Notify....");
        }
        Parallel.For(0, Events.Count, (i) =>
        {
            Events[i](Data);
        });
    }

    public override T? Get()
    {
        if (IsLocal)
        {
            return LocalValue;
        }
        //DONT REMOVE THIS. IF YOU GET HIS THEN YOU ARE DOING SOMETHING WRONG
        throw new NotImplementedException();
    }

    //When a remote kernel notifys when a remote channel changes it's data.
    //Channel A is in kernel A.
    //Channel A's data changes.
    //Previously, we had sent a command to that kernel to request to notify this kernel when the channel's data changes.
    //If it changes, that kernel sends an event.
    //When this kernel receives it, it runs all the local events on when that channel data's changes.   

    internal override void OnRemoteChange(object? NewVal)
    {
        Parallel.For(0, Events.Count, (i) =>
        {
            Events[i]((T?)NewVal);
        });
    }

    public void Dispose()
    {
        Events.Clear();
    }
}


public class DataChannelDescriptor : ExistingChannel
{
    public List<RunningKernel> KernelsToNotify = new List<RunningKernel>();
    public DataChannelDescriptor()
    {
    }

    public void AddNotifyToKernelOnChange(RunningKernel kernel)
    {
        KernelsToNotify.Add(kernel);
    }

    public void RemoveNotifyToKernelOnChange(RunningKernel kernel)
    {
        KernelsToNotify.Remove(kernel);
    }
}



public class ModuleChannelDescriptor : ExistingChannel
{
    public ModuleChannelDescriptor() : base()
    {

    }
}
public class ModuleChannel : ModuleChannelDescriptor
{
    [IgnoreParse]
    public Func<uint, object?[]?, object?>? LocalTargetFunction { get; set; }

    [IgnoreParse]
    private ChannelManager channelManager { get; set; }

    public ModuleChannel(ChannelManager channelManager_) : base()
    {
        channelManager = channelManager_;
    }


    /// <summary>
    /// Only run this when a module is local.
    /// </summary>
    public void StartLocalEvents()
    {
        //Add event on channel run
        channelManager.AddEventFunctionRequest(Path, (s, e) =>
        {
            return Run(e.FunctionID, e.Params);
        });
    }

    //Get by event(cache) or get by request
    public object? Run(uint FuncID, object?[]? param)
    {
        if (IsLocal)
        {
            return LocalTargetFunction!(FuncID, param);
        }
        else
        {
            return channelManager.RunFuncRemote(OwningKernelDesc, Path, FuncID, param);
        }
    }

}

public class ExistingChannel : ChannelDescriptor
{
    public ExistingChannel() : base() { OwningKernelDesc = null!;}
    public RunningKernel OwningKernelDesc { get; internal set; }
    public bool Exists { get; internal set; }
}

public class ChannelDescriptor
{
    public required bool IsLocal { get; set; }

    public required string Path { get; set; }

    public required Guid OwningKernel { get; set; }

    public required bool IsDataChannel { get; set; }
    public required string DataType { get; set; }

}