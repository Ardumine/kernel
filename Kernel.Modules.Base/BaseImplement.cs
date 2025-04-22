using Kernel.Logging;
using Kernel.Modules.Base.MKCalls;
namespace Kernel.Modules.Base;

public abstract class BaseImplement
{
    public required ILogger logger { get; set; }
    public required MKCallClient MKConector { get; set; }
    public required IModuleConfigManager ConfigManager { get; set; }


    public abstract void Prepare();
    public abstract void Start();
    public abstract void EndStop();

 
    public IExistingDataChannelDescriptor? InitiateChannel<T>(string Path)
    {
        return MKConector.InitiateDataChannel<T>(Path);
    }
    public IDataChannelInterface<T>? GetChannelInterface<T>(string Path)
    {
        return MKConector.GetDataChannelInterface<T>(Path);
    }
    public T? GetModuleInterface<T>(string Path) where T : class, IModuleInterface
    {
        return MKConector.GetModuleInterface<T>(Path);
    }

    public T? GetConfigValue<T>(string name){
        var val = ConfigManager.GetConfigValue<T>(name);
        
        return val;
    }

}