using Kernel.Modules.Base;

namespace Ardumine.Modules.YDLidar;

public class YDLidar : Module
{
    public override ModuleDescription Description => new YDLidarDescription();
}
