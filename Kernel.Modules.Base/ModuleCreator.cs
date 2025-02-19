
using Kernel.AFCP;
using Kernel.AFCP.FastMethod;
using Kernel.Modules.Base;

namespace Kernel.Modules;

public class ModuleActivator
{
    public required ModuleManager moduleManager { get; set; }
    public required ChannelManager channelManager { get; set; }

    /// <summary>
    /// Create a local module instance.
    /// </summary>
    /// <param name="desc"></param>
    /// <param name="path"></param>
    public RModule CreateLocalModuleInstance(ModuleDescription desc, string path)
    {
        var mod = CreateModuleType(desc);
        mod.ModuleGuid = Guid.NewGuid();
        mod.Path = path;
        mod.IsLocal = true;

        mod.ModuleChannel = CreateRModuleChannel(mod);
        CreateImplementType(mod);

        return mod;
    }

    //Path, methods idx
    Dictionary<string, ReturnValueDelegate[]> CacheMethods = new();
    private ModuleChannel CreateRModuleChannel(RModule mod)
    {
        CreateMethodCache(mod);
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
    void CreateMethodCache(RModule mod)
    {
        uint I = 0;
        var mets = GetMethods(mod.Description.InterfaceType).ToArray();
        CacheMethods[mod.Path] = new ReturnValueDelegate[mets.Length];
        foreach (var item in mets)
        {
            CacheMethods[mod.Path][I++] = new FastMethodInfo(item).Delegate;
        }
    }

    private RModule CreateModuleType(ModuleDescription desc)
    {
        return (Activator.CreateInstance(desc.BaseType) as RModule)!;
    }

    private void CreateImplementType(RModule mod)
    {
        var impl = Activator.CreateInstance(mod.Description.ImplementType) as BaseImplement;
        if (impl == null)
        {
            throw new Exception($"Error to create implement type for the module {mod.Description.Name}. Implement object was null.");
        }

        impl.ModuleManager = moduleManager;
        impl.SelfMod = mod;

        impl.Logger = new Logging.Logger($"{mod.Path}");

        mod.Implement = impl;
    }


    private object? RunFuncLocal(string Path, uint FuncID, object?[]? Params)
    {
        var impl = moduleManager.GetLocalImplement(Path);

        return CacheMethods[Path][FuncID](impl, Params!);
    }



}