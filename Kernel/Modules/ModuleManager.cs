using Kernel.Modules.Base;
using Kernel.AFCP;

namespace Kernel.Modules;

public class ModuleManager
{
    internal List<LocalRModule> LocalRModules = new List<LocalRModule>();
    private ChannelManager channelManager;
    private ModuleActivator moduleActivator { get; set; }
    private KernelManager KernelManager { get; set; }

    public ModuleManager(ChannelManager _channelManager, KernelManager _KernelManager)
    {
        channelManager = _channelManager;
        KernelManager = _KernelManager;

        moduleActivator = new()
        {
            channelManager = channelManager,
            moduleManager = this,
            localKernel = _KernelManager.LocalKernel,
        };
    }
    internal LocalRModule CreateLocalModuleInstance(IModuleLoadDescription desc, string path, ConfigFile.StartupModuleConfig ModuleConfig)
    {
        var rMod = moduleActivator.CreateLocalModuleInstance(desc, path, ModuleConfig);
        LocalRModules.Add(rMod);
        return rMod;
    }
    /*
        //DO NOT REMOVE
        public DataChannelDescriptor? CreateLocalDataChannel<T>(LocalRModule? mod, string Path)
        {
            if (mod == null)
                return null;
            var realChannelPath = ResolveChannelPathForMod(mod, Path);
            if (realChannelPath == null)
            {
                throw new Exception("Error: no channel registred with that name: " + Path);
            }
            return channelManager.CreateLocalDataChannel<T>(realChannelPath);
        }


        /// <summary>
        /// When a module implement needs requests the descriptor for an data channel, this runs.
        /// DO NOT REMOVE. IT IS CALLED IN RELFECTION!
        /// </summary>
        /// <param name="mod">The module implement that is requesting</param>
        /// <param name="MPath">The MPath to resolve (#channel1)</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public DataChannelDescriptor? GetDataChannelDescriptorForModule(LocalRModule? mod, string MPath)
        {
            if (mod == null)
                return null;
            var realChannelPath = ResolveChannelPathForMod(mod, MPath);
            if (realChannelPath == null)
            {
                throw new Exception("Error: no channel registred with the name: " + MPath);
            }
            return channelManager.GetDataChannel(realChannelPath);
        }

        public DataChannelInterface<T>? GetChannelInterface<T>(LocalRModule? mod, string Path)
        {
            if (mod == null)
                return null;
            var realChannelPath = ResolveChannelPathForMod(mod, Path);
            if (realChannelPath == null)
            {
                throw new Exception("Error: no channel registred with the name: " + Path);
            }
            return channelManager.GetDataChannelInterface<T>(realChannelPath);

        }



        private string? ResolveChannelPathForMod(LocalRModule mod, string path)
        {
            if (path[0] == '#')//Is a relative path for the module?
            {
                return mod.connectedChannels.Where(ch => ch.Name == path)?.FirstOrDefault()?.Path;
            }
            return null;
        }



    */


    internal LocalRModule? GetModule(string Path)
    {
        return LocalRModules.Where(m => m.Path == Path).FirstOrDefault();
    }
    public BaseImplement? GetLocalImplement(string Path)
    {
        return GetModule(Path)?.Implementation;
    }


    internal BaseImplement? GetLocalImplement(LocalRModule mod)
    {
        return mod.Implementation;
    }




    public T GetModuleInterface<T>(string Path) where T : class, IModuleInterface
    {
        var channel = channelManager.GetModuleChannel(Path);
        return ModuleProxy<T>.CreateProxy<T>(channel);
    }


    public void StartModulesViaTree(string startingModule = "kernel")
    {
        var modListOrder = CreateBootTree(startingModule);

        //Start the modules that has the "StartOnBoot" flag
        foreach (var _mod in modListOrder)
        {
            var mod = (LocalRModule)_mod;
            if (mod.StartOnBoot == true)
            {
                var impl = GetLocalImplement(mod);
                impl?.Start();
                mod.Running = true;
            }

        }
    }


    
    public void StopModulesViaTree(string startingModule = "kernel")
    {
        var modListOrder = CreateStopTree(startingModule);

        foreach (var _mod in modListOrder)
        {
            var mod = (LocalRModule)_mod;
            if (mod.Running == true)
            {
                Console.WriteLine($"Stop: {mod.Path}");
                var impl = GetLocalImplement(mod);
                impl?.EndStop();
                mod.Running = false;
            }

        }
    }

    public List<IModule> CreateBootTree(string startingModule = "kernel")
    {
        var tree = GetBootTree(startingModule);

        var modsOrder = new List<IModule>();
        FlattenTree(tree, modsOrder);

        return modsOrder;
    }

    public List<IModule> CreateStopTree(string startingModule = "kernel")
    {
        var tree = GetBootTree(startingModule);

        var modsOrder = new List<IModule>();
        FlattenTree(tree, modsOrder);
        
        modsOrder.Reverse();

        return modsOrder;
    }

    public void FlattenTree(List<StartModBranch> tree, List<IModule> mods)
    {
        foreach (var branch in tree)
        {
            mods.Add(branch.Module);
            FlattenTree(branch.Dependers, mods);
        }
    }



    public void PrintTree(List<StartModBranch> tree, int i = 0)
    {
        i++;
        foreach (var branch in tree)
        {
            Console.WriteLine($"{new string('\t', i)} {branch.Module.Path}");
            PrintTree(branch.Dependers, i);
        }
    }
    public List<StartModBranch> GetBootTree(string path)
    {
        var tree = new List<StartModBranch>();
        foreach (var mod in LocalRModules.Where(m => m.StartAfter == path))
        {
            tree.Add(new()
            {
                Module = mod,
                Dependers = GetBootTree(mod.Path)
            });
        }
        return tree;
    }
}

