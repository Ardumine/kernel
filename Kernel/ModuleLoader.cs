using System.Reflection;
using Kernel.Modules.Base;

namespace Kernel;

public class ModuleLoader
{
    private List<Assembly> ModulesAssemblies = new List<Assembly>();
    public List<ModuleDescription> LoadModules(string[] pluginPaths)
    {
        return pluginPaths.SelectMany(pluginPath =>
        {
            Assembly pluginAssembly = LoadModuleDLL(pluginPath);
            ModulesAssemblies.Add(pluginAssembly);

            var modules = GetModulesOfAssembly(pluginAssembly);
            return modules;
        }).ToList();
    }
    public Assembly? GetModuleAssembly(string Name)
    {
        return ModulesAssemblies.Where(asemb => asemb.FullName == Name).FirstOrDefault();
    }

    private Assembly LoadModuleDLL(string pluginLocation)
    {
        return Assembly.LoadFile(pluginLocation);
    }

    private IEnumerable<ModuleDescription> GetModulesOfAssembly(Assembly assembly)
    {
        var types = assembly.GetTypes();

        foreach (var type in types)
        {
            if (typeof(ModuleDescription).IsAssignableFrom(type))
            {
                ModuleDescription? result = Activator.CreateInstance(type) as ModuleDescription;
                if (result != null)
                {
                    yield return result;
                }
            }
        }

        if (types.Length == 0)
        {
            string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
            throw new ApplicationException(
                $"Can't find any ModuleDescription in {assembly} from {assembly.Location}. Make sure the description for the module is public and not static.\n" +
                $"Available types: {availableTypes}");
        }
    }

}