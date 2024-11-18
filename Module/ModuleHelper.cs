using Ardumine.Module.Base;
using Kernel;

class ModuleHelper
{
    public static List<IModuleInterface> ModuleInterfacers = new();

    public static void ReloadInterfaces(List<Module> modulesInterfacesToLoad)
    {
        ModuleInterfacers.Clear();
        foreach (var mod in modulesInterfacesToLoad)
        {
            var modInterfacer = (IModuleInterface)Activator.CreateInstance(Type.GetType(mod.description.NameInterfacer));
            if (modInterfacer != null)
            {
                modInterfacer.Path = mod.Path;
                ModuleInterfacers.Add(modInterfacer);
            }
        }
    }
    public static T GetInterface<T>(string Path) where T : IModuleInterface
    {
        return (T)ModuleInterfacers.Where(mod => mod.Path == Path).First();
    }


    static IModuleInterface GetImplement(string Path)
    {
        return Program.RunningModuleImplements.Where(mod => mod.Path == Path).First();
    }

    static T GetImplement<T>(string Path) where T : BaseImplement
    {
        return (T)Program.RunningModuleImplements.Where(mod => mod.Path == Path).First();
    }

    public static T Run<T>(string Path, string funcName)
    {
        //Console.WriteLine($"Running in {Path} func {funcName}");
        return (T)SimulateRunServer(Path, funcName, null);
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


    public static object GetVar(string Path, string varName)
    {
        //Console.WriteLine($"Running in {Path} func {funcName}");
        return SimulateGetVarServer(Path, varName);
    }


    static object SimulateGetVarServer(string Path, string varName)
    {
        var impl = GetImplement(Path);
        var myMethod = impl.GetType().GetMethod(varName);
        return myMethod.Invoke(impl, null);
    }

    static object SimulateRunServer(string Path, string funcName, object?[]? parameters = null)
    {
        var impl = GetImplement(Path);
        var myMethod = impl.GetType().GetMethod(funcName);
        return myMethod.Invoke(impl, parameters);
    }


    public static Module CreateModuleInstance(ModuleDescription desc, string Path)
    {
        var mod = (Module)Activator.CreateInstance(Type.GetType(desc.NameBase));
        mod.Path = Path;
        return mod;
    }

    public static BaseImplement CreateImplementInstance(Module mod)
    {
        var a = (BaseImplement)Activator.CreateInstance(Type.GetType(mod.description.NameImplement));
        a.Path = mod.Path;
        a.logger = new Logger($"Mod {mod.Path}");
        return a;
    }

}