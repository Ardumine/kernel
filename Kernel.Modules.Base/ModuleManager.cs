using Kernel.Modules.Base;
using Kernel.AFCP;
using Kernel.AFCP.FastMethod;

namespace Kernel.Modules;

public class ModuleManager
{
    public List<Module> RunningModules = new List<Module>();
    public ChannelManager channelManager;
    public ModuleManager(ChannelManager _channelManager)
    {
        channelManager = _channelManager;
    }



    /// <summary>
    /// Create a module and run locally
    /// </summary>
    /// <param name="desc"></param>
    /// <param name="path"></param>
    public Module CreateModuleRunningLocal(ModuleDescription desc, string path)
    {
        var mod = CreateModuleType(desc);
        mod.Guid = Guid.NewGuid();
        mod.Path = path;
        mod.IsOnLocal = true;

        mod.Channel = CreateChannelForMod(mod);
        CreateImplementType(mod);

        RunningModules.Add(mod);
        return mod;
    }

    //Path, methods idx
    Dictionary<string, ReturnValueDelegate[]> CacheMethods = new();
    //map func idx 
    private ModuleChannel CreateChannelForMod(Module mod)
    {
        CreateCache(mod);
        channelManager.CreateLocalModuleChannel(mod.Path, (fID, Params) =>
        {
            return RunFuncLocal(mod.Path, fID, Params);
        }, true);


        return channelManager.GetModuleChannel(mod.Path);
    }
    private IEnumerable<System.Reflection.MethodInfo> GetMethods(Type type)
    {
        foreach (var method in type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static |
                                                   System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly))
        {
            yield return method;
        }
        if (type.IsInterface)
        {
            foreach (var iface in type.GetInterfaces())
            {
                foreach (var method in GetMethods(iface))
                {
                    yield return method;
                }
            }
        }
    }



    public DataChannelDescriptor CreateLocalDataChannel<T>(Module mod, string Path)
    {
        var channelDesc = ResolvePathForMod(mod, Path);
        if (channelDesc == null)
        {
            throw new Exception("Error: no channel registred with that name: " + Path);
        }
        return channelManager.CreateLocalDataChannel<T>(channelDesc.Path, channelDesc.HighData);
    }

    public DataChannelDescriptor? GetDataChannel(Module mod, string Path)
    {
        var channelDesc = ResolvePathForMod(mod, Path);
        if (channelDesc == null)
        {
            throw new Exception("Error: no channel registred with that name: " + Path);
        }
        return channelManager.GetDataChannel(channelDesc.Path);
    }

    public DataChannelInterface<T> GetInterfaceForChannel<T>(Module mod, string Path)
    {

        var channelDesc = ResolvePathForMod(mod, Path);
        if (channelDesc == null)
        {
            throw new Exception("Error: no channel registred with that name: " + Path);
        }
        return channelManager.GetInterfaceForChannel<T>(channelDesc.Path);

    }



    private ConfigChannelDescriptor? ResolvePathForMod(Module mod, string path)
    {
        if (path[0] == '#')//Is a relative path for the module?
        {
            return mod.connectedChannels.Where(ch => ch.Name == path)?.FirstOrDefault();
        }
        return null;
    }


    void CreateCache(Module mod)
    {
        uint I = 0;
        var mets = GetMethods(mod.Description.InterfaceType).ToArray();
        CacheMethods[mod.Path] = new ReturnValueDelegate[mets.Length];
        foreach (var item in mets)
        {
            CacheMethods[mod.Path][I++] = new FastMethodInfo(item).Delegate;
        }
    }

    private object? RunFuncLocal(string Path, uint FuncID, object?[]? Params)
    {
        var impl = GetLocalImplement(Path);

        return CacheMethods[Path][FuncID](impl, Params!);
    }



    private Module CreateModuleType(ModuleDescription desc)
    {
        return (Activator.CreateInstance(desc.BaseType) as Module)!;
    }


    private void CreateImplementType(Module mod)
    {
        BaseImplement impl = (Activator.CreateInstance(mod.Description.ImplementType) as BaseImplement)!;


        impl!.Guid = mod.Guid;
        impl.Path = mod.Path;
        impl.moduleManager = this;
        impl.SelfMod = mod;

        impl.Logger = new Logging.Logger($"{mod.Path}");

        mod.Implement = impl;
    }



    public Module GetModule(string Path)
    {
        return RunningModules.Where(m => m.Path == Path).First();
    }
    public BaseImplement GetLocalImplement(string Path)
    {
        return RunningModules.Where(mod => mod.Path == Path).First().Implement!;
    }

    public BaseImplement GetLocalImplement(Module mod)
    {
        return mod.Implement!;
    }



    public T GetConector<T>(string Path) where T : class, IModuleInterface
    {
        return ModuleProxy<T>.CreateProxy<T>(channelManager.GetFunctionChannel(Path));
    }





}

