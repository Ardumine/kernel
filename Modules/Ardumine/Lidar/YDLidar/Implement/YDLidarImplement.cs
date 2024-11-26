using Ardumine.Module.Base;

namespace Ardumine.Module.Lidar.YDLidar;
public class YDLidarImplement : YDLidarInterface, BaseImplement
{
    private int _MotorSpeed;
    public int MotorSpeed => _MotorSpeed;

    public string Path { get; set; }
    public Guid guid { get; set; }
    public Logger logger { get; set; }

    public void Prepare()
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

    public void SetMotorSpeed(object[] speeda)//int speed
    {
        int speed = 1231;
        _MotorSpeed = speed;
        logger.LogI($"Motor speed set to {speed}");
    }

    public List<LidarPoint> Read()
    {
        Random rnd = new Random();
        List<LidarPoint> points = new();
        for (int i = 0; i < rnd.Next(400, 500); i++)
        {
            points.Add(new LidarPoint()
            {
                Angle = (float)(rnd.NextDouble() * Math.PI),
                RangeCM = (float)(rnd.NextDouble() * 1000)
            });
        }
        return points;
    }
}