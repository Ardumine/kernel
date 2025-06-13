using Kernel.Modules.Base;
using Kernel.AFCP;
using Kernel.AFCP.KernelManagement;
using Kernel.Logging;

namespace Kernel.Modules;

public class ModuleManager
{
    internal List<ModuleDescriptor> LocalRModules = new List<ModuleDescriptor>();
    private ChannelManager _channelManager;
    internal ModuleCreator? ModuleCreator { get; set; }
    private DataChannelManager _dataChannelManager;

    private ILogger _logger;

    public ModuleManager(ChannelManager channelManager, DataChannelManager dataChannelManager, LocalRunningKernel localKernel, ILogger logger)
    {
        _channelManager = channelManager;
        _logger = logger;
        _dataChannelManager = dataChannelManager;

    }


    internal ModuleDescriptor CreateLocalModuleInstance(IModuleLoadDescriptor desc, string path, ConfigFile.StartupModuleConfig ModuleConfig)
    {
        if (ModuleCreator == null)
            throw new NotSupportedException("Cannot create modules because ModuleCreator was not set");

        var rMod = ModuleCreator.CreateLocalModuleInstance(desc, path, ModuleConfig);
        rMod._logger = _logger.CreateSubLogger($"Internal{path}");

        LocalRModules.Add(rMod);
        return rMod;
    }

    internal SubModuleDescriptor CreateSubModuleInstance(ModuleDescriptor parentModule, IModuleLoadDescriptor desc, string path, bool start = false)
    {
        if (ModuleCreator == null)
            throw new NotSupportedException("Cannot create modules because ModuleCreator was not set");

        var subMod = ModuleCreator.CreateSubModuleInstance(parentModule, desc, path);
        subMod._logger = _logger.CreateSubLogger($"Internal{path}");

        LocalRModules.Add(subMod);

        subMod.Prepare();

        if (start)
        {
            subMod.Start();
        }

        return subMod;
    }

    internal void TryGet()
    {

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

    /// <summary>
    /// Gets a local Module Descriptor
    /// </summary>
    /// <param name="Path">The Path of the Module</param>
    /// <returns>The Module descriptor. If null then it was not found.</returns>
    internal ModuleDescriptor? GetLocalModuleDescriptor(string Path)
    {
        return LocalRModules.Where(m => m.Path == Path).FirstOrDefault();
    }

    /// <summary>
    /// Gets the implement of a local Module
    /// </summary>
    /// <param name="Path">The Path of the Module</param>
    /// <returns>The implement of the Module</returns>
    internal BaseImplement? GetLocalImplement(string Path)
    {
        return GetLocalModuleDescriptor(Path)?.Implementation;
    }

    /// <summary>
    /// Gets the implement of a local Module
    /// </summary>
    /// <param name="mod">The Local Module DEscriptor</param>
    /// <returns>The implement of the Module</returns>
    internal BaseImplement? GetLocalImplement(ModuleDescriptor mod)
    {
        return mod.Implementation;
    }



    /// <summary>
    /// Gets the interface for an Module
    /// </summary>
    /// <typeparam name="T">The interface type of the Module(derived of IModuleInterface)</typeparam>
    /// <param name="Path">The Path of the Module</param>
    /// <returns>The interface for the Module</returns>
    /// <exception cref="NullReferenceException">If the Path of the Module is not found, then thrown.</exception>
    public T GetModuleInterface<T>(string Path) where T : class, IModuleInterface
    {
        var channel = _channelManager.GetModuleChannel(Path);
        if (channel == null)
        {
            throw new NullReferenceException($"The module with the Path {Path} was not found.");
        }
        return ModuleProxy<T>.CreateProxy<T>(channel);
    }

    /// <summary>
    /// Gets the interface for an Module
    /// </summary>
    /// <typeparam name="T">The interface type of the Module(derived of IModuleInterface)</typeparam>
    /// <param name="Path">The Path of the Module</param>
    /// <returns>The interface for the Module</returns>
    /// <exception cref="NullReferenceException">If the Path of the Module is not found, then thrown.</exception>
    public T GetSubModuleInterface<T>(string Path) where T : class, ISubModuleInterface
    {
        var channel = _channelManager.GetModuleChannel(Path);
        if (channel == null)
        {
            throw new NullReferenceException($"The module with the Path {Path} was not found.");
        }
        return ModuleProxy<T>.CreateProxy<T>(channel);
    }

    internal void DeleteModule(ModuleDescriptor mod)
    {
        mod.InternalDelete();

        if (!mod.IsDeleted)
        {
            foreach (var subMod in mod.SubModules)
            {
                if (!subMod.IsDeleted)
                {
                    _logger?.LogW($"The module did not delete the sub-module '{subMod.Path}'! Deleting manually...");
                    DeleteModule(subMod);
                }
            }

        }

        _channelManager.DeleteLocalModuleChannel(mod.Path);

        LocalRModules.Remove(mod);
    }


    #region Boot tree
    public void StartModulesViaTree(string startingModule = "kernel")
    {
        var modListOrder = CreateBootTree(startingModule);

        //Start the modules that has the "StartOnBoot" flag
        foreach (var _mod in modListOrder)
        {
            var mod = (ModuleDescriptor)_mod;

            if (mod.StartOnBoot)
            {
                mod.Prepare();
                mod.Start();
            }

        }
    }


    public void StopAndDeleteModulesViaTree(string startingModule = "kernel")
    {
        var modListOrder = CreateStopTree(startingModule);

        foreach (var _mod in modListOrder)
        {
            var mod = (ModuleDescriptor)_mod;
            if (mod.IsRunning)
            {
                _logger.LogI($"Stopping '{mod.Path}'...");
                mod.Stop();
            }
        }

        foreach (var _mod in modListOrder)
        {
            var mod = (ModuleDescriptor)_mod;
            if (!mod.IsDeleted)
            {
                _logger.LogI($"Deleting '{mod.Path}'...");
                DeleteModule(mod);
            }
        }


        var runningModules = LocalRModules.Where(mod => mod.IsRunning);
        if (runningModules.Any())
        {
            _logger.LogW("It appears that some modules are still running after stopping via tree. Stopping them anyways...");
            foreach (var mod in runningModules)
            {
                mod.Stop();
            }
        }

        var nonDeletedModules = LocalRModules.Where(mod => !mod.IsDeleted);
        if (nonDeletedModules.Any())
        {
            _logger.LogW("It appears that some modules are not deleted after deleting via tree. Deleting them anyways...");
            foreach (var mod in nonDeletedModules)
            {
                DeleteModule(mod);
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

    #endregion
}

