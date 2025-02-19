using Kernel.Modules.Base;

namespace Ardumine.Modules.SLAM.Hector;

public class HectorSLAM : RModule
{
    public override ModuleDescription Description => new HectorSLAMDescription();
}
