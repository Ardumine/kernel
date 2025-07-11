using Kernel.Logging;
using Kernel.Modules.Base.MKCalls;
namespace Kernel.Modules.Base;

public abstract class BaseImplement : IModuleInterface
{
    public required ILogger logger { get; set; }
    public required MKCallClient MKConector { get; set; }
    public required IModuleConfigManager ConfigManager { get; set; }
    public required string MPath { get; set; }

    public abstract void Prepare();
    public abstract void Start();
    public abstract void EndStop();

    public virtual void Delete()
    {
        logger.LogW("Delete was not implemented!");
    }


 
    public T? GetModuleInterface<T>(string Path) where T : class, IModuleInterface
    {
        return MKConector.GetModuleInterface<T>(Path);
    }


    public T? CreateSubModule<T>(string Path, string moduleName, bool startOnCreation = true) where T : class, ISubModuleInterface
    {
        return MKConector.CreateSubModule<T>(Path, moduleName, startOnCreation);
    }

    public void StartSubModule(string Path)
    {
        MKConector.StartSubModule(Path);
    }
    public void StopSubModule(string Path)
    {
        MKConector.StopSubModule(Path);
    }
    public void DeleteSubModule(string Path)
    {
        MKConector.DeleteSubModule(Path);
    }


    public T? GetConfigValue<T>(string name)
    {
        var val = ConfigManager.GetConfigValue<T>(name);

        return val;
    }

}