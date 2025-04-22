using Kernel.AFCP;
using Kernel.AFCP.FastMethod;
using Kernel.AFCP.KernelManagement;
using Kernel.Modules.Base;
using Kernel.Modules.Base.MKCalls;

namespace Kernel.Modules;

public class ModuleActivator(ModuleManager _moduleManager, ChannelManager _channelManager, DataChannelManager _dataChannelManager, LocalRunningKernel _localKernel)
{


    /// <summary>
    /// Create a local module instance.
    /// </summary>
    /// <param name="desc"></param>
    /// <param name="path"></param>
    internal LocalRModuleDescriptor CreateLocalModuleInstance(IModuleLoadDescription desc, string path, ConfigFile.StartupModuleConfig moduleStartupConfig)
    {
        var mod = new LocalRModuleDescriptor()
        {
            Path = path,
            HostingKernel = _localKernel.Guid,
            IsLocal = true,
            ModuleLoadDescription = desc,
            Implementation = CreateLocalModImplement(desc, path)
        };

        //Set up kernel calls
        var callReceiver = new MKCallReceiver()
        {
            WorkingModule = mod,
            channelManager = _channelManager,
            moduleManager = _moduleManager,
            dataChannelManager = _dataChannelManager
        };
        mod.CallReceiver = callReceiver;

        //Set up the MK client
        var callClient = new MKCallClient(callReceiver.CallHandler);
        
        mod.Implementation.MKConector = callClient;

        mod.Implementation.ConfigManager = new ModuleConfigManager()
        {
            moduleConfig = moduleStartupConfig.Config
        };

        //Set up permissions
        var perms = new IModulePermissions();
        foreach (var channel in moduleStartupConfig.connectedChannels)
        {
            perms.AddPerm(new ConnectChannelPermission()
            {
                RealPath = channel.Path,
                MPath = channel.Name
            });
        }

        foreach (var channel in moduleStartupConfig.hostingChannels)
        {
            //Add the permission to host it
            perms.AddPerm(new HostChannelPermission()
            {
                RealPath = channel.Path,
                MPath = channel.Name
            });

            //Add a permission to connect
            perms.AddPerm(new ConnectChannelPermission()
            {
                RealPath = channel.Path,
                MPath = channel.Name
            });
        }
        mod.ModulePermissions = perms;

        //Create the module channel
        CreateModuleChannel(mod, desc);
        mod.ModuleChannel = _channelManager.GetModuleChannel(mod.Path);

        //Config
        mod.StartAfter = moduleStartupConfig.startAfter;
        mod.StartOnBoot = moduleStartupConfig.StartOnBoot;

        return mod;
    }


    private void CreateModuleChannel(LocalRModuleDescriptor mod, IModuleLoadDescription desc)
    {
        CreateMethodCache(mod, desc);

        _channelManager.CreateLocalModuleChannel(mod.Path, (fID, Params) =>
        {
            return RunFuncLocal(mod.Path, fID, Params);
        });
    }

    //Path, methods[idx]
    Dictionary<string, ReturnValueDelegate[]> CacheMethods = new();

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
    void CreateMethodCache(LocalRModuleDescriptor mod, IModuleLoadDescription desc)
    {
        uint I = 0;
        var mets = GetMethods(desc.InterfaceType).ToArray();
        CacheMethods[mod.Path] = new ReturnValueDelegate[mets.Length];
        foreach (var item in mets)
        {
            CacheMethods[mod.Path][I++] = new FastMethodInfo(item).Delegate;
        }
    }



    private BaseImplement CreateLocalModImplement(IModuleLoadDescription desc, string ModulePath)
    {
        var impl = Activator.CreateInstance(desc.ImplementType) as BaseImplement;
        if (impl == null)
        {
            throw new Exception($"Error to create implement type for the module '{desc.Name}', path '{ModulePath}'. Implement object was null.");
        }

        impl.logger = new Logging.ConsoleLogger($"{ModulePath}");

        return impl;
    }




    private object? RunFuncLocal(string Path, uint FuncID, object?[]? Params)
    {
        var impl = _moduleManager.GetLocalImplement(Path);

        if (impl == null)
        {
            throw new Exception($"Cannot run FuncID: {FuncID}");
        }

        return CacheMethods[Path][FuncID](impl, Params);
    }



}