
using Kernel.AFCP;
using Kernel.AFCP.FastMethod;
using Kernel.Modules.Base;

namespace Kernel.Modules;

public class ModuleActivator
{
    public required ModuleManager moduleManager { get; set; }
    public required ChannelManager channelManager { get; set; }
    public required KernelDescriptor localKernel { get; set; }


    /// <summary>
    /// Create a local module instance.
    /// </summary>
    /// <param name="desc"></param>
    /// <param name="path"></param>
    internal LocalRModule CreateLocalModuleInstance(IModuleLoadDescription desc, string path, ConfigFile.StartupModuleConfig moduleStartupConfig)
    {
        var mod = new LocalRModule()
        {
            ModuleGuid = Guid.NewGuid(),
            Path = path,
            HostingKernel = localKernel.Guid,
            IsLocal = true,
            ModuleLoadDescription = desc,
            Implementation = CreateLocalModImplement(desc, path)
        };

        //Set up kernel calls
        mod.CallReceiver = new MKConectorCallReceiver()
        {
            WorkingModule = mod,
            channelManager = channelManager,
            moduleManager = moduleManager
        };
        mod.Implementation.ConfigManager = new ModuleConfigManager(){
            moduleConfig = moduleStartupConfig.Config
        };
        mod.Implementation.MKConector = mod.CallReceiver;//For now
        //MKConectorCreator.GetModuleConector(moduleManager, ModulePath);



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
        mod.ModuleChannel = channelManager.GetModuleChannel(mod.Path);

        //Config
        mod.StartAfter = moduleStartupConfig.startAfter;
        mod.StartOnBoot = moduleStartupConfig.StartOnBoot;

        return mod;
    }


    private void CreateModuleChannel(LocalRModule mod, IModuleLoadDescription desc)
    {
        CreateMethodCache(mod, desc);

        channelManager.CreateLocalModuleChannel(mod.Path, (fID, Params) =>
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
    void CreateMethodCache(LocalRModule mod, IModuleLoadDescription desc)
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

        impl.Logger = new Logging.Logger($"{ModulePath}");

        return impl;
    }




    private object? RunFuncLocal(string Path, uint FuncID, object?[]? Params)
    {
        var impl = moduleManager.GetLocalImplement(Path);

        if(impl == null){
            throw new Exception($"Cannot run FuncID: {FuncID}");
        }

        return CacheMethods[Path][FuncID](impl, Params!);
    }



}