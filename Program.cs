using Ardumine.Module;

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

        AvailableModules = ModuleManager.GetModules();
        foreach (var mod in AvailableModules)
        {
            logger.LogL($"Found module '{mod.Name}':{mod.Version}");
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
            if (cmd == "exit")
            {
                run = false;
            }
        }
        //logger.LogI("Kernel Panic: No more instructions");
        StopRunningModules();
        logger.LogI("Kernel stop");

    }
}