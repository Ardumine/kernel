using Ardumine.Module.Base;

namespace Ardumine.Module.Lidar.YDLidar;

class YDLidarDescription : ModuleDescription
{
    public string Name => "YDLidarConector";

    public string FriendlyName => "YDLidar conector";

    public string Version => "0.1.0";

    public string NameImplement => typeof(YDLidarImplement).FullName;


    public string NameBase  => typeof(YDLidar).FullName; 

}
