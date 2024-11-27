using Ardumine.Module.Lidar.YDLidar;
using System;
using System.Reflection;
using System.Reflection.Emit;





class Tests
{
    static Logger logger;
    public static void InitTests()
    {
        logger = new Logger("Tests");
    }

    public static void Test2()
    {

    }

    public static void Test1()
    {
        logger.LogOK("Test begin");

        var lidar1 = ModuleHelper.GetConector<YDLidarInterface>("/lidar");
        var lidar2 = ModuleHelper.GetConector<YDLidarInterface>("/lidar2");

        lidar1.SetMotorSpeed(57);//Good reference57
        logger.LogI($"Lidar1 Motor speed: {lidar1.MotorSpeed}");

        lidar2.SetMotorSpeed(80);
        logger.LogI($"Lidar2 Motor speed: {lidar2.MotorSpeed}");


        var lidarData = lidar1.Read();
        logger.LogI($"Read from lidar1 {lidarData.Count} points");
    }
}