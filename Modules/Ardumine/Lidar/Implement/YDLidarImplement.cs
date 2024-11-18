
namespace Ardumine.Module.Lidar.YDLidar;
public class YDLidarImplement : ModuleBase, YDLidarInterface
{
    private Logger logger;
    public YDLidarImplement(Logger _logger, string _Path)
    {
        logger = _logger;
        Path = _Path;

    }
    public override void Prepare()
    {
        logger.LogI("Preparing Lidar...");
    }

    public override void Start()
    {
        logger.LogI("Starting Lidar...");
    }

    public override void EndStop()
    {
        logger.LogI("Stoping Lidar...");
    }

    public void SetMotorSpeed(int speed)
    {
        logger.LogI($"Motor speed set to {speed}");
    }


}