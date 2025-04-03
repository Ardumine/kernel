using System.Diagnostics;
using Kernel.AFCP;
using Kernel.AFCP.KernelManagement;
using Kernel.Logging;
using Kernel.Modules;
using Kernel.Modules.Base;

namespace Kernel;
internal class Program
{
    private static void Main(string[] args)
    {
        Console.Title = "Starting...";
        Thread.CurrentThread.Name = "Kernel Main Thread";
        var logger = new Logger("Kernel");

        var localKernel = LocalRunningKernel.CreateLocalKernelDescriptor("4d120748-810c-41f0-b93e-66500092f567");
        using var channelManager = new ChannelManager(localKernel, logger.CreateSubLogger("ChannelManager"));

        var KernelManager = new KernelManager(localKernel, logger.CreateSubLogger("KernelManager")) 
        {
            LocalChannelManager = channelManager
        };

        //Create the AFCP server
        var AFCPServer = new AfcpServer(8000);

        var ChannelManagerAFCPServerConector = new ChannelManagerAFCPServerConector(AFCPServer, channelManager, KernelManager, logger.CreateSubLogger("ChannelManagerAFCPServerConector"))
        {
            LocalKernel = localKernel
        };

        var moduleManager = new ModuleManager(channelManager, localKernel, logger.CreateSubLogger("ModuleManager"));
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
            
            if(modDesc == null){
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


        logger.Space();
        logger.LogI("Stop");

        moduleManager.StopModulesViaTree();
        AFCPServer.Stop();
        KernelManager.Stop();
        logger.LogI("Done");

    }

}
