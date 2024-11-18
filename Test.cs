using Ardumine.Module.Lidar.YDLidar;

class Test
{
    static Logger logger;
    static List<ModuleInterface> ModuleInterfaces = new();
    public static void Init()
    {
        logger = new();
        
        var mods = new[] {
            "Ardumine.Module.Lidar.YDLidar.YDLidarInterface"
        };

        foreach (var modName in mods)
        {
            var mod = (ModuleInterface)Activator.CreateInstance(Type.GetType(modName), logger);
            if (mod != null)
            {
                logger.LogL($"Module path: {mod.RunningName}");
                ModuleInterfaces.Add(mod);
            }
        }
    }

    static T GetInterface<T>(string Path) where T : ModuleInterface
    {
        return (T)ModuleInterfaces.Where(mod => mod.RunningName == Path).First();
    }



    public static void Test1()
    {
        logger.LogI("Test...");
        var lidarInterface = GetInterface<YDLidarInterface>("lidar");
        lidarInterface.InternalFunction();
    }
}