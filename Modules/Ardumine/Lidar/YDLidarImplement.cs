
namespace Ardumine.Module.Lidar.YDLidar;
public class YDLidarImplement : ModuleBase
{
    private Logger logger;
    public override void Prepare(Logger _logger)
    {
        logger = _logger;
        logger.LogI("Preparing Lidar...");
        RunningName = "lidar";

    }

    public void Start()
    {
        logger.LogI("Starting Lidar...");
    }

    public void EndStop()
    {
        logger.LogI("Stoping Lidar...");
    }

    public void InternalFunction(){
        logger.LogI("Function ran!");
    }
}