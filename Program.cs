using Ardumine.Module;
using Ardumine.Module.Base;
using Ardumine.Module.Lidar.YDLidar;

namespace Kernel;

internal class Program
{
    public static List<IModuleInterface> RunningModuleImplements = new();
    public static List<Module> RunningModules = new();

    public static List<ModuleDescription> AvailableModules = new();


    static Logger logger;

    static void StopRunningModules()
    {
        logger.LogI("Stoping modules...");
        foreach (var mod in RunningModuleImplements)
        {
            mod.EndStop();
        }
    }

    static void AddModule(ModuleDescription desc, string Path)
    {
        var ModLidar = ModuleHelper.CreateModuleInstance(desc, Path);
        RunningModules.Add(ModLidar);
        RunningModuleImplements.Add(ModuleHelper.CreateImplementInstance(ModLidar));
    }
    private static void Main(string[] args)
    {
        logger = new("Kernel");
        logger.LogI("Starting kernel...");

        logger.LogI("Searching modules implements...");

        RunningModuleImplements = new List<IModuleInterface>();



        var descLidar = new YDLidarDescription();


        AvailableModules.Add(descLidar);
        AddModule(descLidar, "lidar");
        AddModule(descLidar, "lidar2");

        ModuleHelper.ReloadInterfaces(RunningModules);



        logger.LogI("Preparing modules...");
        foreach (var mod in RunningModuleImplements)
        {
            mod.Prepare();
        }


        logger.LogI("Starting modules...");
        foreach (var mod in RunningModuleImplements)
        {
            mod.Start();
        }


        Tests.InitTests();


        bool run = false;
        logger.LogI("Startup ended. Terminal mode.");
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
        logger.LogI("Kernel stop");

    }



}