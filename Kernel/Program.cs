using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Channels;
using Kernel.AFCP;
using Kernel.Logging;
using Kernel.Modules;
using Kernel.Modules.Base;
using Kernel.Shared;

namespace Kernel;
internal class Program
{
    static List<ModuleDescription> AvailableModules = new();
    static List<KernelDescriptor> ConnectedKernels = new ();
    static ModuleManager? moduleManager { get; set; }
    private static void Main(string[] args)
    {
        var logger = new Logger("kernel");

        var channelManager = new ChannelManager() { ConnectedKernels = ConnectedKernels };
        ChannelManager.Defaulte = channelManager;

        var channelServer = new ChannelManagerServer(8000, channelManager);
        moduleManager = new ModuleManager(channelManager);
        var moduleLoader = new ModuleLoader();

        var config = ConfigParser.Load("config.json");

        //Load the DLLs and get the available modules.
        AvailableModules = moduleLoader.LoadModules(config.ModulesDlls);

        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            return moduleLoader.GetModuleAssembly(args.Name);//In case a module requires another module
        };

        logger.LogI("Starting AFCP...");
        channelServer.Start();

        foreach (var moduleConfig in config.StartupModules)
        {
            var modDesc = AvailableModules.Where(m => m.Name == moduleConfig.ModuleName).FirstOrDefault();
            var mod = moduleManager.CreateModuleRunningLocal(modDesc!, moduleConfig.Path);
            mod.StartOnBoot = moduleConfig.StartOnBoot;
            mod.Config = moduleConfig.Config;

            mod.connectedChannels =
            [
                .. moduleConfig.connectedChannels == null ? new() :  moduleConfig.connectedChannels,
                .. moduleConfig.hostingChannels == null ? new() :  moduleConfig.hostingChannels,
            ];
            mod.hostingChannels = moduleConfig.hostingChannels;

            mod.startAfter = moduleConfig.startAfter;
        }

        foreach (var mod in logger.CreateIterator(moduleManager.RunningModules, "Preparing modules..."))
        {
            var impl = moduleManager.GetLocalImplement(mod);
            impl.Prepare();
        }

        string startingTrigger = "kernel";
        StartersTreeStart(startingTrigger);


        logger.LogOK("Kernel started!");
        Console.ReadLine();//Wait for stop signal


        logger.Space();
        logger.LogI("Stop");

        foreach (var mod in logger.CreateIterator(moduleManager.RunningModules.Where(m => m.Running), "Stoping modules..."))
        {
            var impl = moduleManager.GetLocalImplement(mod);
            impl.EndStop();
            mod.Running = false;
        }

        channelServer.Stop();
    }
    static void StartersTreeStart(string path)
    {
        if (path != "kernel")
        {
            var implMod = moduleManager?.GetLocalImplement(path)!;
            var mod = moduleManager?.GetModule(path)!;
            mod.Running = true;
            implMod.Start();
        }

        foreach (var mod in moduleManager!.RunningModules.Where(m => m.startAfter == path))
        {
            StartersTreeStart(mod.Path);
        }

    }

}
