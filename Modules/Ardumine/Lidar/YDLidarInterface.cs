
namespace Ardumine.Module.Lidar.YDLidar;
public class YDLidarInterface : ModuleInterface
{
    private Logger logger;
    public YDLidarInterface(Logger _logger){
        RunningName = "lidar";
        logger = _logger;

    }
    public void Prepare()
    {
        logger.LogI("Running prepare on server...");
    }

    public void Start()
    {
        logger.LogI("Running start on server...");
    }

    public void EndStop()
    {
        logger.LogI("Running stop on server...");
    }

    public void InternalFunction()
    {
        logger.LogI("Interface: Running function on server...");
        ModuleHelper.Run(RunningName, "InternalFunction");
    }
}