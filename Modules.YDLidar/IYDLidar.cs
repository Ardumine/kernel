using Kernel.Modules.Base;

namespace Ardumine.Modules.YDLidar;

public interface IYDLidar: IModuleInterface
{
}
public struct LidarPoint{

    /// <summary>
    /// Angle in radians
    /// </summary>
    public float Angle {get;set;}

    /// <summary>
    /// Point distance in CM
    /// </summary>
    public float Distance {get;set;}
}