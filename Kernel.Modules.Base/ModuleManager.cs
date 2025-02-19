using Kernel.Modules.Base;
using Kernel.AFCP;
using Kernel.AFCP.FastMethod;

namespace Kernel.Modules;

public class ModuleManager
{
    public List<RModule> LocalModulesInstances = new List<RModule>();
    private ChannelManager channelManager;
    public ModuleActivator moduleActivator { private set; get; }
    public ModuleManager(ChannelManager _channelManager)
    {
        channelManager = _channelManager;
        moduleActivator = new()
        {
            channelManager = channelManager,
            moduleManager = this
        };
    }
    public RModule CreateLocalModuleInstance(ModuleDescription desc, string path)
    {
        var rMod = moduleActivator.CreateLocalModuleInstance(desc, path);
        LocalModulesInstances.Add(rMod);
        return rMod;
    }





    public DataChannelDescriptor CreateLocalDataChannel<T>(RModule mod, string Path)
    {
        var realChannelPath = ResolveChannelPathForMod(mod, Path);
        if (realChannelPath == null)
        {
            throw new Exception("Error: no channel registred with that name: " + Path);
        }
        return channelManager.CreateLocalDataChannel<T>(realChannelPath);
    }

    public DataChannelDescriptor? GetDataChannel(RModule mod, string Path)
    {
        var realChannelPath = ResolveChannelPathForMod(mod, Path);
        if (realChannelPath == null)
        {
            throw new Exception("Error: no channel registred with the name: " + Path);
        }
        return channelManager.GetDataChannel(realChannelPath);
    }

    public DataChannelInterface<T> GetInterfaceForChannel<T>(RModule mod, string Path)
    {
        var realChannelPath = ResolveChannelPathForMod(mod, Path);
        if (realChannelPath == null)
        {
            throw new Exception("Error: no channel registred with the name: " + Path);
        }
        return channelManager.GetInterfaceForChannel<T>(realChannelPath);

    }



    private string? ResolveChannelPathForMod(RModule mod, string path)
    {
        if (path[0] == '#')//Is a relative path for the module?
        {
            return mod.connectedChannels.Where(ch => ch.Name == path)?.FirstOrDefault()?.Path;
        }
        return null;
    }






    public RModule GetModule(string Path)
    {
        return LocalModulesInstances.Where(m => m.Path == Path).First();
    }
    public BaseImplement GetLocalImplement(string Path)
    {
        return LocalModulesInstances.Where(mod => mod.Path == Path).First().Implement!;
    }

    public BaseImplement GetLocalImplement(RModule mod)
    {
        return mod.Implement!;
    }



    public T GetConector<T>(string Path) where T : class, IModuleInterface
    {
        return ModuleProxy<T>.CreateProxy<T>(channelManager.GetFunctionChannel(Path));
    }





}

