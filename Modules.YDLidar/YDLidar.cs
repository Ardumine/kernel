using Kernel.Modules.Base;

namespace Ardumine.Modules.YDLidar;

public class YDLidar : RModule
{
    public override ModuleDescription Description => new YDLidarDescription();
}
