using Ardumine.Module.Lidar.YDLidar;

class Tests
{
    static Logger logger;
    public static void InitTests()
    {
        logger = new Logger("Tests");
    }


    public static void Test1()
    {
        logger.LogI("Test begin");

        var lidar = ModuleHelper.GetConector<YDLidarConector>("/lidar");
        var lidar2 = ModuleHelper.GetConector<YDLidarConector>("/lidar2");

        lidar.SetMotorSpeed(57);//Good reference
        logger.LogI($"Motor speed: {lidar.MotorSpeed}");

        lidar2.SetMotorSpeed(80);
        logger.LogI($"Motor speed: {lidar2.MotorSpeed}");


        var lidarData = lidar.Read();
        logger.LogI($"Read from the lidar {lidarData.Count} points");
    }
}