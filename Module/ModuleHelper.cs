using Ardumine.Module.Base;

public static class ModuleHelper
{
    public static List<ModuleDescription> AvailableModules = new();


    public static List<IModuleInterface> RunningModuleImplements = new();
    //public static List<IModuleInterface> RunningModuleConectors = new();
    public static List<Module> RunningModules = new();


    /*public static void ReloadConector(List<Module> modulesConectorsToLoad)
    {
        RunningModuleConectors.Clear();
        foreach (var mod in modulesConectorsToLoad)
        {
            var conector = (IModuleInterface)Activator.CreateInstance(Type.GetType(mod.description.NameConector));
            if (conector != null)
            {
                conector.Path = mod.Path;
                conector.guid = mod.guid;
                RunningModuleConectors.Add(conector);
            }
        }
    }*/
    public static T GetConector<T>(string Path) where T : class, IModuleInterface
    {
        // return (T)RunningModuleConectors.Where(mod => {
        //     Console.WriteLine(mod.Path + mod.GetType().Name);
        //     return mod.Path == Path;
        // }).First();//mod.Path == Path
        return ModuleProxy<T>.CreateProxy(Path);
    }


    static IModuleInterface GetImplement(string Path)
    {
        return RunningModuleImplements.Where(mod => mod.Path == Path).First();
    }

    static T GetImplement<T>(string Path) where T : BaseImplement
    {
        return (T)RunningModuleImplements.Where(mod => mod.Path == Path).First();
    }

    /*public static T Run<T>(string Path, string funcName)
    {
        //Console.WriteLine($"Running in {Path} func {funcName}");
        return (T)SimulateRunServer(Path, funcName, null);
    }
*/

    public static object Run(string Path, string funcName, params object[] parameters)
    {
        //Console.WriteLine($"Running in {Path} func {funcName}");
        return SimulateRunServer(Path, funcName, parameters);
    }
    public static object RunParam(string Path, string funcName, object[] parameters)
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
        mod.guid = Guid.NewGuid();
        return mod;
    }

    public static BaseImplement CreateImplementInstance(Module mod)
    {
        var implement = (BaseImplement)Activator.CreateInstance(Type.GetType(mod.description.NameImplement));
        implement.Path = mod.Path;
        implement.logger = new Logger($"Mod {mod.Path}");
        implement.guid = mod.guid;

        return implement;
    }

    public static T1 CreateConector<T1>(Module mod) where T1 : class, IModuleInterface
    {
        // inst.Path = mod.Path;
        // inst.guid = mod.guid;
        var cc = ModuleProxy<T1>.CreateProxy(mod.Path);
        return cc;

    }

    public static void AddModule(ModuleDescription desc, string Path)
    {
        var mod = CreateModuleInstance(desc, Path);
        RunningModules.Add(mod);

        RunningModuleImplements.Add(CreateImplementInstance(mod));
        //RunningModuleConectors.Add(CreateConector<IModuleInterface>(mod));

    }
}

public class ModuleProxy<T> : System.Reflection.DispatchProxy where T : class, IModuleInterface
{
    private string Path { get; set; }

    protected override object Invoke(System.Reflection.MethodInfo targetMethod, object[] args)
    {
        return ModuleHelper.RunParam(Path, targetMethod.Name, args);
    }

    public static T CreateProxy(string Path)
    {
        var proxy = Create<T, ModuleProxy<T>>() as ModuleProxy<T>;
        proxy.Path = Path;
        return proxy as T;
    }
}