using Kernel.AFCP;
using Kernel.Logging;
namespace Kernel.Modules.Base;

public abstract class BaseImplement
{
    public required ModuleManager ModuleManager { get; set; }
    public required Logger Logger { get;  set; }
    public required RModule SelfMod { get;  set;}

    public required IModuleConector moduleConector { get; set; }
   

    public abstract void Prepare();
    public abstract void Start();
    public abstract void EndStop();

    public DataChannelDescriptor? GetDataChannel(string Path)
    {
        return ModuleManager.GetDataChannel(SelfMod, Path);
    }
    public DataChannelDescriptor InitiateChannel<T>(string Path)
    {
        return ModuleManager.CreateLocalDataChannel<T>(SelfMod, Path);
    }
    public DataChannelInterface<T> GetChannelInterface<T>(string Path)
    {
        return ModuleManager.GetInterfaceForChannel<T>(SelfMod, Path);
    }


}