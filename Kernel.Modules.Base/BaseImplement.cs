using Kernel.AFCP;
using Kernel.Logging;
namespace Kernel.Modules.Base;

public abstract class BaseImplement : IModuleInterface
{
    public required ModuleManager moduleManager { get; set; }

    public required string Path { get; set; }
    public Guid Guid { get; set; }
    public required Logger Logger { get; set; }
    public required Module SelfMod { get; set; }

    public abstract void Prepare();
    public abstract void Start();

    public abstract void EndStop();

    public DataChannelDescriptor? GetDataChannel(string Path)
    {
        return moduleManager.GetDataChannel(SelfMod, Path);
    }
    public DataChannelDescriptor InitiateChannel<T>(string Path)
    {
        return moduleManager.CreateLocalDataChannel<T>(SelfMod, Path);
    }
    public DataChannelInterface<T> GetChannelInterface<T>(string Path)
    {
        return moduleManager.GetInterfaceForChannel<T>(SelfMod, Path);
    }


}