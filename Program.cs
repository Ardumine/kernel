using Ardumine.Module;

namespace Kernel;

internal class Program
{
    static List<Module> AvailableModules = new List<Module>();
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
        logger = new();
        logger.LogI("Starting kernel...");
        logger.LogI("Searching modules...");

        var mods = new[] {
            "Ardumine.Module.Lidar.YDLidar.YDLidar"
        };

        /*AvailableModules = ModuleManager.GetModules();
        foreach (var mod in AvailableModules)
        {
            logger.LogL($"Found module '{mod.Name}':{mod.Version}");
        }*/

        AvailableModules = new List<Module>();
        foreach (var modName in mods)
        {
            var mod = (Module)Activator.CreateInstance(Type.GetType(modName));
            if (mod != null)
            {
                logger.LogL($"Found module '{mod.Name}':{mod.Version}");
                AvailableModules.Add(mod);
            }
        }

        logger.LogI("Preparing modules...");
        foreach (var mod in AvailableModules)
        {
            mod.Prepare(logger);
        }


        logger.LogI("Starting modules...");
        foreach (var mod in AvailableModules)
        {
            mod.Start();
        }




        bool run = true;
        logger.LogI("Startup ended. Terminal mode.");
        while (run)
        {
            var cmd = Console.ReadLine();
            if (cmd == "exit" || cmd == "q" || cmd == "stop")
            {
                run = false;
            }
        }
        //logger.LogI("Kernel Panic: No more instructions");
        StopRunningModules();
        logger.LogI("Kernel stop");

    }
}