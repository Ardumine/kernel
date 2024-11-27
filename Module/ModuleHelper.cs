using Ardumine.Module.Base;

public static class ModuleHelper
{
    public static List<ModuleDescription> AvailableModules = new();


    public static List<IModuleInterface> RunningModuleImplements = new();
    //public static List<IModuleInterface> RunningModuleConectors = new();
    public static List<Module> RunningModules = new();


    
    public static T GetConector<T>(string Path) where T : class, IModuleInterface
    {
     
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

    
    public static object RunParam(string Path, string funcName, object[] parameters)
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
        var cc = ModuleProxy<T1>.CreateProxy(mod.Path);
        return cc;

    }

    public static void AddModule(ModuleDescription desc, string Path)
    {
        var mod = CreateModuleInstance(desc, Path);
        RunningModules.Add(mod);

        RunningModuleImplements.Add(CreateImplementInstance(mod));

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