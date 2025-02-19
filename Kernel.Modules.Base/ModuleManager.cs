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
        var channelDesc = ResolvePathForMod(mod, Path);
        if (channelDesc == null)
        {
            throw new Exception("Error: no channel registred with that name: " + Path);
        }
        return channelManager.CreateLocalDataChannel<T>(channelDesc.Path, channelDesc.HighData);
    }

    public DataChannelDescriptor? GetDataChannel(RModule mod, string Path)
    {
        var channelDesc = ResolvePathForMod(mod, Path);
        if (channelDesc == null)
        {
            throw new Exception("Error: no channel registred with that name: " + Path);
        }
        return channelManager.GetDataChannelDescriptor(channelDesc.Path);
    }

    public DataChannelInterface<T> GetInterfaceForChannel<T>(RModule mod, string Path)
    {

        var channelDesc = ResolvePathForMod(mod, Path);
        if (channelDesc == null)
        {
            throw new Exception("Error: no channel registred with that name: " + Path);
        }
        return channelManager.GetInterfaceForChannel<T>(channelDesc.Path);

    }



    private ConfigChannelDescriptor? ResolvePathForMod(RModule mod, string path)
    {
        if (path[0] == '#')//Is a relative path for the module?
        {
            return mod.connectedChannels.Where(ch => ch.Name == path)?.FirstOrDefault();
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

