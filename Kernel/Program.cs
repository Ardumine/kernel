using Kernel.AFCP;
using Kernel.Logging;
using Kernel.Modules;
using Kernel.Modules.Base;

namespace Kernel;
internal class Program
{
    static List<IModuleLoadDescription> AvailableModules = new();
    private static void Main(string[] args)
    {
        Console.Title = "Starting...";
        Thread.CurrentThread.Name = "Main Thread";
        var logger = new Logger("kernel");

        var LocalKernel = KernelDescriptor.CreateLocalKernelDescriptor("4d120748-810c-41f0-b93e-66500092f567");
        var channelManager = new ChannelManager(LocalKernel);

        var KernelManager = new KernelManager()
        {
            LocalKernel = LocalKernel,
            LocalChannelManager = channelManager
        };

        //Create the AFCP server
        var AFCPServer = new AfcpServer(8000);

        var ChannelManagerAFCPServerConector = new ChannelManagerAFCPServerConector(AFCPServer, channelManager, KernelManager)
        {
            LocalKernel = KernelManager.LocalKernel
        };


        var moduleManager = new ModuleManager(channelManager, KernelManager);
        var moduleLoader = new ModuleLoader();


        var config = ConfigParser.Load("config.json");


        //Load the DLLs and get the available modules.
        AvailableModules = moduleLoader.LoadModules(config.ModulesDlls);

        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            return moduleLoader.GetModuleAssembly(args.Name);//In case a module requires another module
        };

        logger.LogI("Starting AFCP...");
        AFCPServer.Start();

        foreach (var moduleConfig in config.StartupModules)
        {
            var modDesc = AvailableModules.Where(m => m.Name == moduleConfig.ModuleName).FirstOrDefault();
            var mod = moduleManager.CreateLocalModuleInstance(modDesc!, moduleConfig.Path, moduleConfig);
        }

        foreach (var mod in logger.CreateIterator(moduleManager.LocalRModules, "Preparing modules..."))
        {
            var impl = moduleManager.GetLocalImplement(mod);
            impl?.Prepare();
        }

        moduleManager.StartModulesViaTree();

        logger.LogOK("Kernel started!");
        Console.Title = "Ok!";
        Console.ReadLine();//Wait for stop signal


        logger.Space();
        logger.LogI("Stop");

        moduleManager.StopModulesViaTree();
        AFCPServer.Stop();
        logger.LogI("Done");
    }

}
