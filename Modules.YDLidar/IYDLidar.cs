using Kernel.Modules.Base;

namespace Ardumine.Modules.YDLidar;

public interface IYDLidar: IModuleInterface
{
}
public struct LidarPoint{
    public float AngleRad {get;set;}
    public float Distance {get;set;}//CM

}