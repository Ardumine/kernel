using Kernel.AFCP;
using Kernel.AFCP.KernelManagement;
using Kernel.Logging;
using Kernel.Modules;

namespace Kernel;
internal class Program
{
    private static void Main(string[] args)
    {
        Console.Title = "Starting...";
        Thread.CurrentThread.Name = "Kernel Main Thread";
        ILogger logger = new ConsoleLogger("Kernel");

        var localKernel = LocalRunningKernel.CreateLocalKernelDescriptor("4d120748-810c-41f0-b93e-66500092f567");
        using var channelManager = new ChannelManager(localKernel, logger.CreateSubLogger("ChannelManager"));
        var dataChannelManager = new DataChannelManager(channelManager, localKernel, logger.CreateSubLogger("DataChannelManager"));

        var kernelManager = new KernelManager(localKernel, channelManager, dataChannelManager, logger.CreateSubLogger("KernelManager"));

        //Create the AFCP server
        var AFCPServer = new AfcpServer(8000);

        var ChannelManagerAFCPServerConector = new ChannelManagerAFCPServerConector(AFCPServer, channelManager, dataChannelManager, kernelManager, logger.CreateSubLogger("ChannelManagerAFCPServerConector"))
        {
            LocalKernel = localKernel
        };

        var moduleManager = new ModuleManager(channelManager, dataChannelManager, localKernel, logger.CreateSubLogger("ModuleManager"));
        var moduleLoader = new ModuleLoader();

        var config = ConfigParser.Load("config.json");


        //Load the DLLs and get the available modules.
        var AvailableModules = moduleLoader.LoadModules(config.ModulesDlls);

        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            return moduleLoader.GetModuleAssembly(args.Name);//In case a module requires another module
        };

        logger.LogI("Starting AFCP...");
        AFCPServer.Start();

        foreach (var moduleConfig in config.StartupModules)
        {
            var modDesc = AvailableModules.Where(m => m.Name == moduleConfig.ModuleName).FirstOrDefault();

            if (modDesc == null)
            {
                throw new NullReferenceException($"The module load with the name {moduleConfig.ModuleName} does not exist. Make sure you have the correct name and the DLL is loaded in config.json/ModulesDlls.");
            }
            moduleManager.CreateLocalModuleInstance(modDesc, moduleConfig.Path, moduleConfig);
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


        logger.NewLine();
        logger.LogI("Stoping...");

        moduleManager.StopModulesViaTree();
        AFCPServer.Stop();
        kernelManager.Stop();
        logger.LogI("Done");

    }

}
