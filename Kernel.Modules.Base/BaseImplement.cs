using Kernel.Logging;
namespace Kernel.Modules.Base;

public abstract class BaseImplement
{
    public required ILogger logger { get; set; }
    public required IMKConector MKConector { get; set; }
    public required IModuleConfigManager ConfigManager { get; set; }


    public abstract void Prepare();
    public abstract void Start();
    public abstract void EndStop();

    public IExistingDataChannelDescriptor? GetDataChannelDescriptor(string Path)
    {
        return MKConector.GetDataChannel(Path);
    }
    public IExistingDataChannelDescriptor? InitiateChannel<T>(string Path)
    {
        return MKConector.InitiateChannel<T>(Path);
    }
    public IDataChannelInterface<T>? GetChannelInterface<T>(string Path)
    {
        return MKConector.GetChannelInterface<T>(Path);
    }
    public T GetModuleInterface<T>(string Path) where T : class, IModuleInterface
    {
        return MKConector.GetModuleInterface<T>(Path);
    }

    public T? GetConfigValue<T>(string name){
        var val = ConfigManager.GetConfigValue<T>(name);
        
        return val;
    }

}