using System.Reflection;
using System.Reflection.Emit;
using Kernel.AFCP;
using Kernel.AFCP.FastMethod;
using Kernel.AFCP.KernelManagement;
using Kernel.Modules.Base;
using Kernel.Modules.Base.MKCalls;
using Kernel.Modules.Permissions;

namespace Kernel.Modules;

internal class ModuleCreator(ModuleManager _moduleManager, List<IModuleLoadDescriptor> _moduleLoadDescriptors, ChannelManager _channelManager, DataChannelManager _dataChannelManager, LocalRunningKernel _localKernel)
{
    internal delegate MKCallResult MKCallDelegate(MKCallRequest req);


    /// <summary>
    /// Create a local module instance.
    /// </summary>
    /// <param name="desc"></param>
    /// <param name="path"></param>
    internal LocalRModuleDescriptor CreateLocalModuleInstance(IModuleLoadDescriptor desc, string path, ConfigFile.StartupModuleConfig moduleStartupConfig)
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
        unsafe
        {
            //Set up the MK call handler/receiver

            var callReceiver = new MKCallReceiver()
            {
                WorkingModule = mod,
                channelManager = _channelManager,
                moduleManager = _moduleManager,
                dataChannelManager = _dataChannelManager
            };
            mod.CallReceiver = callReceiver;

            //Set up the MK client
            //We use a pointer to a the function of the callReceiver.CallHandler.
            //Because it is not static, here(somehow) we create a static function that already has  
            var callClient = new MKCallClient((delegate*<MKCallRequest, MKCallResult>)CreateMKCallHandler(callReceiver));
            mod.Implementation.MKConector = callClient;



            mod.Implementation.ConfigManager = new ModuleConfigManager()
            {
                moduleConfig = moduleStartupConfig.Config
            };
        }

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
        mod.StartAfter = moduleStartupConfig.startAfter ?? string.Empty;
        mod.StartOnBoot = moduleStartupConfig.StartOnBoot;

        return mod;
    }


    internal SubModuleDescriptor CreateSubModuleInstance(LocalRModuleDescriptor parentModule, IModuleLoadDescriptor desc, string path)
    {
        var mod = new SubModuleDescriptor()
        {
            Path = path,
            HostingKernel = _localKernel.Guid,
            IsLocal = true,
            ModuleLoadDescription = desc,
            Implementation = CreateLocalModImplement(desc, path),
            ParentModule = parentModule
        };

        //Set up kernel calls
        unsafe
        {
            //Set up the MK call handler/receiver

            var callReceiver = new MKCallReceiver()
            {
                WorkingModule = mod,
                channelManager = _channelManager,
                moduleManager = _moduleManager,
                dataChannelManager = _dataChannelManager
            };
            mod.CallReceiver = callReceiver;

            //Set up the MK client
            //We use a pointer to a the function of the callReceiver.CallHandler.
            //Because it is not static, here(somehow) we create a static function that already has  
            var callClient = new MKCallClient((delegate*<MKCallRequest, MKCallResult>)CreateMKCallHandler(callReceiver));
            mod.Implementation.MKConector = callClient;
        }

        CreateModuleChannel(mod, desc);
        mod.ModuleChannel = _channelManager.GetModuleChannel(mod.Path);

        return mod;
    }

    private IntPtr CreateMKCallHandler(MKCallReceiver callReceiver)
    {
        AssemblyBuilder ab = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("MyAssembly"), AssemblyBuilderAccess.Run);
        ModuleBuilder mob = ab.DefineDynamicModule("MyModule");
        TypeBuilder tb = mob.DefineType("MyType", TypeAttributes.Public | TypeAttributes.Class);


        //Create a field holding the MKCallReceiver
        FieldBuilder mcrField = tb.DefineField("mcr", typeof(MKCallReceiver), FieldAttributes.Public | FieldAttributes.Static);
        {
            MethodBuilder mcrMethod = tb.DefineMethod("setmcr", MethodAttributes.Public | MethodAttributes.Static, typeof(void), [typeof(MKCallReceiver)]);
            ILGenerator il = mcrMethod.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Stsfld, mcrField);
            il.Emit(OpCodes.Ret);
        }

        //Create the call method
        {
            MethodBuilder MKCallHndPMethod = tb.DefineMethod("MKCallHndP", MethodAttributes.Public | MethodAttributes.Static, typeof(MKCallResult), [typeof(MKCallRequest)]);

            ILGenerator il = MKCallHndPMethod.GetILGenerator();

            il.Emit(OpCodes.Ldsfld, mcrField);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, typeof(MKCallReceiver).GetMethod("CallHandler")!);
            il.Emit(OpCodes.Ret);
        }

        Type builtType = tb.CreateType();

        //Set the value of the call handler
        builtType.GetMethod("setmcr")!.Invoke(null, [callReceiver]);

        var invokeSquareIt = (MKCallDelegate)builtType.GetMethod("MKCallHndP")!.CreateDelegate(typeof(MKCallDelegate));
        return invokeSquareIt.Method.MethodHandle.GetFunctionPointer();

    }

    private void CreateModuleChannel(LocalRModuleDescriptor mod, IModuleLoadDescriptor desc)
    {
        CreateMethodCache(mod, desc);

        _channelManager.CreateLocalModuleChannel(mod.Path, (fID, Params) =>
        {
            return RunFuncLocal(mod.Path, fID, Params);
        });
    }


    public IModuleLoadDescriptor? TryGetModuleLoadDescriptor(string moduleName)
    {
        return _moduleLoadDescriptors.Where(m => m.Name == moduleName).FirstOrDefault();
    }


    //Path, methods[idx]
    Dictionary<string, ReturnValueDelegate[]> CacheMethods = new();

    private IEnumerable<MethodInfo> GetMethods(Type type)
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
    private void CreateMethodCache(LocalRModuleDescriptor mod, IModuleLoadDescriptor desc)
    {
        uint I = 0;
        var mets = GetMethods(desc.InterfaceType).ToArray();
        CacheMethods[mod.Path] = new ReturnValueDelegate[mets.Length];
        foreach (var item in mets)
        {
            CacheMethods[mod.Path][I++] = new FastMethodInfo(item).Delegate;
        }
    }



    private BaseImplement CreateLocalModImplement(IModuleLoadDescriptor desc, string ModulePath)
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