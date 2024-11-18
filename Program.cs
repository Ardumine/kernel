using Ardumine.Module;
using Ardumine.Module.Base;
using Ardumine.Module.Lidar.YDLidar;

namespace Kernel;

internal class Program
{
    static Logger logger;

    static void StopRunningModules()
    {
        logger.LogI("Stoping modules...");
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

        ModuleHelper.AvailableModules.Add(descLidar);


        ModuleHelper.AddModule(descLidar, "/lidar");
        ModuleHelper.AddModule(descLidar, "/lidar2");


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


        bool run = false;
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
        logger.LogOK("Kernel stop");

    }



}