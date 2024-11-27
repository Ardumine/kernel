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

        var lidar = ModuleHelper.GetConector<YDLidarInterface>("/lidar");
        // var lidar2 = ModuleHelper.GetConector<YDLidarInterface>("/lidar2");

        lidar.SetMotorSpeed(200);//Good reference57
                                 //logger.LogI($"Motor speed: {lidar.MotorSpeed}");

        //lidar2.SetMotorSpeed();//80
        // logger.LogI($"Motor speed: {lidar2.MotorSpeed}");


        //var lidarData = lidar.Read();
        //logger.LogI($"Read from the lidar {lidarData.Count} points");
    }
}