
namespace Ardumine.Module.Lidar.YDLidar;
public class YDLidar : Module
{
    public YDLidar(){

    }
    public string Name { get => "Ardumine.Module.Lidar.YDLidar"; }
    public string FriendlyName { get => "YDLidar module for T Mini Pro"; }
    public string Version { get => "0.1.0"; }

    private Logger logger;
    public void Prepare(Logger _logger)
    {
        logger = _logger;
        logger.LogI("Preparing Lidar...");
    }

    public void Start()
    {
        logger.LogI("Starting Lidar...");

    }

    public void EndStop()
    {
        logger.LogI("Stoping Lidar...");
    }

}