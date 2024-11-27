using System.Net;
using Ardumine.AFCP.Core.Server;
using Ardumine.Module.AFCPClientTest;
using Ardumine.Module.Lidar.YDLidar;

namespace Kernel;
internal class Program
{
    static Logger? logger;

    static void StopRunningModules()
    {
        logger?.LogI("Stoping modules...");
        foreach (var mod in ModuleHelper.RunningModuleImplements)
        {
            mod.EndStop();
        }
    }

    private static void Main(string[] args)
    {
        logger = new("Kernel");

        logger.LogI("Starting kernel...");

        logger.LogI("Searching modules implements...");


        var descLidar = new YDLidarDescription();
        var descAFCPTest = new AFCPClientTestDescription();

        //Add to available modules
        ModuleHelper.AvailableModules.Add(descLidar);

        //Create testing modules
        ModuleHelper.AddModule(descLidar, "/lidar");
        ModuleHelper.AddModule(descLidar, "/lidar2");

        ModuleHelper.AddModule(descAFCPTest, "/afcpTest");




        logger.LogI("Preparing modules...");
        foreach (var mod in ModuleHelper.RunningModuleImplements)
        {
            mod.Prepare();
        }

        logger.LogI("Starting modules...");
        foreach (var mod in ModuleHelper.RunningModuleImplements)
        {
            mod.Start();
        }


        Tests.InitTests();
        logger.LogI("Starting AFCP server...");
        var AFCP = new AFCPTCPServer(IPAddress.Any, new Logger("Servidor AFCP"));
        AFCP.Start();

        var channelSystem = new ChannelSystemServer(AFCP);
        channelSystem.Start();  
        channelSystem.CreateChannel("/oi", ChannelPermission.ReadWrite);

        bool run = true;
        logger.LogOK("Startup ended. Terminal mode.");
        Console.WriteLine();

        Tests.Test1();

        while (run)
        {
            var cmd = Console.ReadLine();
            if (cmd == "exit" || cmd == "q" || cmd == "stop")
            {

                run = false;
            }
            if (cmd == "t" || cmd == "test")
            {
                Tests.Test1();
            }
        }
        Console.WriteLine();

        //logger.LogI("Kernel Panic: No more instructions");
        StopRunningModules();

        channelSystem.Stop();
        AFCP.Stop();
        
        logger.LogOK("Kernel stop");

    }



}