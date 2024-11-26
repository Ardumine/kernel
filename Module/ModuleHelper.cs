using Ardumine.Module.Base;

public static class ModuleHelper
{
    public static List<ModuleDescription> AvailableModules = new();


    public static List<IModuleInterface> RunningModuleImplements = new();
    public static List<IModuleInterface> RunningModuleConectors = new();
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
    public static T GetConector<T>(string Path) where T : IModuleInterface
    {
        return (T)RunningModuleConectors.Where(mod => true).First();//mod.Path == Path
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
    public static object RunReturn(string Path, string funcName)
    {
        Console.WriteLine($"Running in {Path} func {funcName}");
        return SimulateRunServer(Path, funcName, null);
    }

    public static void RunNormal(string Path, string funcName)
    {
        Console.WriteLine($"Running in {Path} func {funcName}");
        SimulateRunServer(Path, funcName, null);
    }
    public static object RunNormal(string Path, string funcName, params object[] parameters)
    {
        //Console.WriteLine($"Running in {Path} func {funcName}");
        return SimulateRunServer(Path, funcName, parameters);
    }

        public static object RunNormal( object[] parameters)
    {
        Console.WriteLine($"Running in {parameters.Length} ");
        return "aa";
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
    public static IModuleInterface CreateConector(Module mod)
    {
        var dynamicType = DynamicTypeBuilder.CreateTypeWithInterface();
        var instance = Activator.CreateInstance(dynamicType);//as IMyInterface
        return instance as IModuleInterface;
        /*
        var conector = (IModuleInterface)Activator.CreateInstance(Type.GetType(mod.description.NameConector));
        if (conector != null)
        {
            conector.Path = mod.Path;
            conector.guid = mod.guid;
        }
        return conector;*/
    }

    public static void AddModule(ModuleDescription desc, string Path)
    {
        var mod = CreateModuleInstance(desc, Path);
        RunningModules.Add(mod);

        RunningModuleImplements.Add(CreateImplementInstance(mod));
        RunningModuleConectors.Add(CreateConector(mod));

    }
}