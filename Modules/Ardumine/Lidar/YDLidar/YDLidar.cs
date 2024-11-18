using Ardumine.Module.Base;

namespace Ardumine.Module.Lidar.YDLidar;

class YDLidar : Base.Module
{
    public Guid guid { get; set; }
    public string Path { get; set; }
    public ModuleDescription description => new YDLidarDescription();
}
