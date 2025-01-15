
using Kernel.AFCP.KASerializer.Atributes;

namespace Kernel.AFCP;
public class DataChannelInterface : ChannelBase
{
    public required virtual DataChannelDescriptor DataChannel { get; set; }
    internal virtual void OnRemoteChange(object? NewVal)
    {

    }


    public virtual object? Data { get; set; }
}
public abstract class DataChannelInterface<T> : DataChannelInterface
{
    public List<Action<T?>> Events = new();

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
    private ChannelManager channelManager;
    public RemoteDataChannelInterface(ChannelManager channelManager_, string path_)
    {
        Path = path_;
        channelManager = channelManager_;
        Init();
    }
    public void Init()
    {
        //Add event on channel run
        channelManager.AddEventRemoteDataChange(Path, (s, e) =>
        {
            Parallel.For(0, Events.Count, (i) =>
            {
                var evente = Events[i];
                T? newData = (T?)e.NewData;
                evente(newData);
            });

        });
    }



    public override object? Data { get => Get(); set => Set((T?)value); }

    public override void AddEvent(Action<T?> evente)
    {
        if (Events.Count == 0)
        {
            if (!IsLocal)
            {
                //Console.WriteLine("Add notify!");
                channelManager.AddEventRemote(OwningKernel, Path);
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
                channelManager.RemoveEventRemote(OwningKernel, Path);
            }
        }

    }

    public override void Set(T? Data)
    {
        if (!IsLocal)
        {
            channelManager.SetValueRemote(OwningKernel, Path, Data, HighSpeed);
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
            return channelManager.GetValueRemote<T>(OwningKernel, Path, HighSpeed);
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
        throw new NotImplementedException();
    }
}
public class LocalDataChannelInterface<T> : DataChannelInterface<T>
{

    private ChannelManager channelManager;
    public LocalDataChannelInterface(ChannelManager channelManager_)
    {
        channelManager = channelManager_;

    }

    private T? LocalValue;
    public override object? Data { get => Get(); set => Set((T?)value); }


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
            channelManager.NoitifyChannelDataChanged(DataChannel, LocalValue);
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
}


public class DataChannelDescriptor : ChannelBase
{
    public required string? DataType { get; set; }

    public List<KernelDescriptor> KernelsToNotify = new List<KernelDescriptor>();
    //public required DataChannelInterface<object> channelInterface { get; set; }
    public void AddNotifyToKernelOnChange(KernelDescriptor kernelGuid)
    {
        KernelsToNotify.Add(kernelGuid);
    }

    public void RemoveNotifyToKernelOnChange(KernelDescriptor kernelGuid)
    {
        KernelsToNotify.Add(kernelGuid);
    }

    public List<KernelDescriptor> KernelsSubscribed = new List<KernelDescriptor>();

    public DataChannelDescriptor()
    {
    }



}

public class ModuleChannel : ChannelBase
{
    [IgnoreParse]
    public Func<uint, object?[]?, object?>? LocalTargetFunction { get; set; }

    [IgnoreParse]
    private ChannelManager channelManager { get; set; }

    public ModuleChannel(){}
    public ModuleChannel(ChannelManager channelManager_)
    {
        channelManager = channelManager_;
        Init();

    }


    public void Init()
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
            return channelManager.RunFuncRemote(OwningKernel, Path, FuncID, param);
        }
    }

}

public class ChannelBase : ChannelDescriptor
{
    //public required ChannelManager ParentChannelManager;//NOT THE CAHNNEL MAMANGER THAT OWNS THIS CHANNEL IN A GLOBAL LEVEL!!

}

public class ChannelDescriptor
{
    public required bool IsLocal { get; set; }

    public required string Path { get; set; }

    public required Guid OwningKernel { get; set; }

    public bool IsDataChannel { get; set; }

    public required bool HighSpeed = false;
}