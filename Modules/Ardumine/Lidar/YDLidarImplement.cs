
namespace Ardumine.Module.Lidar.YDLidar;
public class YDLidarImplement : ModuleBase, YDLidarInterface
{
    private Logger logger;
    public YDLidarImplement(Logger _logger)
    {
        logger = _logger;
        Path = "lidar";

    }
    public override void Prepare()
    {
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

    public void InternalFunction(int num)
    {
        logger.LogI($"Function ran! Num is {num}");
    }


}