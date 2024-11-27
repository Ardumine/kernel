
namespace Ardumine.Module.Lidar.YDLidar;
public class YDLidarConectore : IModuleInterface, YDLidarInterface
{
    public string Path { get; set; }

    public int MotorSpeed => (int)ModuleHelper.GetVar(Path, System.Reflection.MethodBase.GetCurrentMethod().Name);

    public Guid guid { get; set; }

    public void Prepare()
    {
        ModuleHelper.Run(Path, System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void Start()
    {
        ModuleHelper.Run(Path, System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void EndStop()
    {
        ModuleHelper.Run(Path, System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void SetMotorSpeed(int speed)//int speed
    {
        ModuleHelper.Run(Path, System.Reflection.MethodBase.GetCurrentMethod().Name, 200);
    }

    //public List<LidarPoint> Read()
    //{
       // return ModuleHelper.Run<List<LidarPoint>>(Path, System.Reflection.MethodBase.GetCurrentMethod().Name);
   // }
}