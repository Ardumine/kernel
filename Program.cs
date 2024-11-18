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
        logger = new();
        logger.LogI("Starting kernel...");
        logger.LogI("Searching modules...");

        var modulesToLoad = File.ReadAllLines("config/modules");


        AvailableModules = new List<ModuleBase>();
        foreach (var modName in modulesToLoad)
        {
            var mod = (ModuleBase)Activator.CreateInstance(Type.GetType(modName), logger);
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

        Test.Init();

        Test.Test1();

        bool run = true;
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
                Test.Test1();
            }
        }
        //logger.LogI("Kernel Panic: No more instructions");
        StopRunningModules();
        logger.LogI("Kernel stop");

    }



}