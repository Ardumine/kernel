using System.Reflection;
using Ardumine.Module;
using Kernel;

class ModuleHelper
{
    public static List<ModuleInterface> ModuleInterfaces = new();


    public static void ReloadInterfaces(List<string> modulesInterfacesToLoad)
    {
        ModuleInterfaces.Clear();
        foreach (var modName in modulesInterfacesToLoad)
        {
            var mod = (ModuleInterface)Activator.CreateInstance(Type.GetType(modName));
            if (mod != null)
            {
                ModuleInterfaces.Add(mod);
            }
        }
    }



    static ModuleBase GetImplement(string Path)
    {
        return Program.AvailableModules.Where(mod => mod.Path == Path).First();
    }

    static T GetImplement<T>(string Path) where T : ModuleBase
    {
        return (T)Program.AvailableModules.Where(mod => mod.Path == Path).First();
    }

    public static object Run(string Path, string funcName)
    {
        //Console.WriteLine($"Running in {Path} func {funcName}");
        return SimulateRunServer(Path, funcName, null);
    }
    public static object Run(string Path, string funcName, params object[] parameters)
    {
        //Console.WriteLine($"Running in {Path} func {funcName}");
        return SimulateRunServer(Path, funcName, parameters);
    }

    static object SimulateRunServer(string Path, string funcName, object?[]? parameters = null)
    {
        var impl = GetImplement(Path);
        var myMethod = impl.GetType().GetMethod(funcName);
        return myMethod.Invoke(impl, parameters);
    }
}