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

        var kernelManager = new KernelManager(localKernel, channelManager, logger.CreateSubLogger("KernelManager"));

        //Create the AFCP server
        var AFCPServer = new AfcpServer(8000);

        var ChannelManagerAFCPServerConector = new ChannelManagerAFCPServerConector(AFCPServer, channelManager, kernelManager, logger.CreateSubLogger("ChannelManagerAFCPServerConector"))
        {
            LocalKernel = localKernel
        };

        var moduleManager = new ModuleManager(channelManager, localKernel, logger.CreateSubLogger("ModuleManager"));
        var moduleLoader = new ModuleLoader();

        var config = ConfigParser.Load("config.json");



        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            return moduleLoader.GetModuleAssembly(args.Name);//In case a module requires another module
        };


        //Load the DLLs and get the available modules
        var availableModuleLoadDescriptiors = moduleLoader.LoadModules(config.ModulesDlls);

        var moduleCreator = new ModuleCreator(moduleManager, availableModuleLoadDescriptiors, channelManager, localKernel);

        moduleManager.ModuleCreator = moduleCreator;

        logger.LogI("Starting AFCP...");
        AFCPServer.Start();

        foreach (var moduleConfig in config.StartupModules)
        {
            var modLoadDesc = moduleCreator.TryGetModuleLoadDescriptor(moduleConfig.ModuleName);

            if (modLoadDesc == null)
                throw new NullReferenceException($"The module load with the name {moduleConfig.ModuleName} does not exist. Make sure you have the correct name and the DLL is loaded in config.json/ModulesDlls.");
            moduleManager.CreateLocalModuleInstance(modLoadDesc, moduleConfig.Path, moduleConfig);
        }

        moduleManager.StartModulesViaTree();

        logger.LogOK("Kernel started!");
        Console.Title = "Ok!";
        Console.ReadLine();//Wait for stop signal


        logger.NewLine();
        logger.LogI("Stoping...");

        moduleManager.StopAndDeleteModulesViaTree();
        AFCPServer.Stop();
        kernelManager.Stop();
        logger.LogI("Done");

    }

}
