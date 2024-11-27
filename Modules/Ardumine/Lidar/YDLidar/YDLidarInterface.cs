namespace Ardumine.Module.Lidar.YDLidar;

//This file must be shared between the implement and the interface
public interface YDLidarInterface : IModuleInterface
{
    public void SetMotorSpeed(int speed);
    public int MotorSpeed { get; }
    
    public List<LidarPoint> Read(); 
}
public struct LidarPoint
{
    public float Angle { get; set; }
    public float RangeCM { get; set; }
}