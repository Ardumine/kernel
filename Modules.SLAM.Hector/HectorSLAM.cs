using Kernel.Modules.Base;

namespace Ardumine.Modules.SLAM.Hector;

public class HectorSLAM : Module
{
    public override ModuleDescription Description => new HectorSLAMDescription();
}
