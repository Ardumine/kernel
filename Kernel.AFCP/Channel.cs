using Kernel.AFCP.KASerializer.Atributes;
using Kernel.AFCP.KernelManagement;
using static Kernel.AFCP.ChannelManager;


namespace Kernel.AFCP;

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


    public object? OnFunctionRunRequestEvent(object s, FunctionRequestEventArgs e)
    {
        if (e.ChannelPath == Path)
        {
            return Run(e.FunctionID, e.Params);
        }
        return null;
    }
    /// <summary>
    /// Only run this when a module is local.
    /// </summary>
    public void StartLocalEvents()
    {
        //Add event on channel run
        channelManager.OnFunctionRunRequest += OnFunctionRunRequestEvent;
    }

    public void StopLocalEvents()
    {
        //Remove event on channel run
        channelManager.OnFunctionRunRequest -= OnFunctionRunRequestEvent;
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
    public ExistingChannel() : base() { OwningKernelDesc = null!; }
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