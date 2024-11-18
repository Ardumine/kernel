using System.Reflection;
using Ardumine.Module;
using Kernel;

class ModuleHelper
{
    static ModuleBase GetImplement(string Path) 
    {
        return Program.AvailableModules.Where(mod => mod.RunningName == Path).First();
    }

    static T GetImplement<T>(string Path) where T : ModuleBase
    {
        return (T)Program.AvailableModules.Where(mod => mod.RunningName == Path).First();
    }


    public static object Run(string Path, string funcName, object ?[]? parameters = null)
    {
        //Console.WriteLine($"Running in {Path} func {funcName}");
        return SimulateRunServer(Path, funcName, parameters);
    }

    static object SimulateRunServer(string Path, string funcName, object ?[]? parameters = null)
    {
        var impl = GetImplement(Path);
        var myMethod = impl.GetType().GetMethod(funcName);
        return myMethod.Invoke(impl, parameters);
    }
}