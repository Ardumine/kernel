
using System.Diagnostics;

namespace Ardumine.Module.Lidar.YDLidar;
public class YDLidarInterfacer : ModuleInterface, YDLidarInterface
{
    public string Path { get; set;}

    public YDLidarInterfacer()
    {
        Path = "lidar";
    }
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

    public void InternalFunction(int num)
    {
        ModuleHelper.Run(Path, System.Reflection.MethodBase.GetCurrentMethod().Name, num);
    }
}