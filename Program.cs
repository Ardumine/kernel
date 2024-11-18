using Ardumine.Module;

namespace Kernel;

internal class Program
{
    public static List<ModuleBase> AvailableModules = new();

    static Logger logger;

    static void StopRunningModules()
    {
        logger.LogI("Stoping modules...");
        foreach (var mod in AvailableModules)
        {
            mod.EndStop();
        }
    }
    private static void Main(string[] args)
    {
        logger = new("Kernel");
        logger.LogI("Starting kernel...");
        logger.LogI("Searching modules...");

        var modulesToLoad = File.ReadAllLines("config/modules");


        AvailableModules = new List<ModuleBase>();
        foreach (var modStr in modulesToLoad)
        {
            var configMod = modStr.Split(",");
            var mod = (ModuleBase)Activator.CreateInstance(Type.GetType(configMod[0]), new Logger($"Mod {configMod[1]}"), configMod[1]);
            if (mod != null)
            {
                logger.LogL($"Found module '{mod.Name}':{mod.Version}");
                AvailableModules.Add(mod);
            }
        }


        logger.LogI("Preparing modules...");
        foreach (var mod in AvailableModules)
        {
            mod.Prepare();
        }


        logger.LogI("Starting modules...");
        foreach (var mod in AvailableModules)
        {
            mod.Start();
        }


        var mods = new[] {
            "Ardumine.Module.Lidar.YDLidar.YDLidarInterfacer"
        };


        ModuleHelper.ReloadInterfaces(mods.ToList());
        Tests.InitTests();



        Tests.Test1();

        bool run = false;
        logger.LogI("Startup ended. Terminal mode.");
        Console.WriteLine();
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
        //logger.LogI("Kernel Panic: No more instructions");
        StopRunningModules();
        logger.LogI("Kernel stop");

    }



}