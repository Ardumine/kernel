using Ardumine.Module.Lidar.YDLidar;

class Tests
{
    static Logger logger;
  
    public static void InitTests(){
        logger = new Logger("Tests");
    }

    static T GetInterface<T>(string Path) where T : ModuleInterface
    {
        return (T)ModuleHelper.ModuleInterfaces.Where(mod => mod.Path == Path).First();
    }


    public static void Test1()
    {
        logger.LogI("Test...");
        var lidarInterface = GetInterface<YDLidarInterfacer>("lidar");
        lidarInterface.InternalFunction(57);//Good reference
    }
}