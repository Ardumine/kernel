using System.Text.Json;
using Kernel.Modules.Base;

internal class ModuleConfigManager : IModuleConfigManager
{
    public required Dictionary<string, object>? moduleConfig { get; init; }
    public T? GetConfigValue<T>(string name)
    {
        //If there is no module config then there isnt any entry at all. 
        if(moduleConfig == null) 
            return default;
        
        if (moduleConfig.TryGetValue(name, out var value))
        {
            return ((JsonElement)value).Deserialize<T>();
        }
        return default;

    }
}