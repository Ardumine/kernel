namespace Kernel.Modules.Base;

public interface IModuleConfigManager{
    /// <summary>
    /// Get a value of the config via entry
    /// </summary>
    /// <param name="name">The config entry name</param>
    /// <returns>The value. If null then it does not exist</returns>
    public T? GetConfigValue<T>(string name);
}