using Kernel.Modules.Base;

namespace Ardumine.Modules.SLAM.Hector;

public class HectorSLAMDescription : IModuleLoadDescriptor
{
    public string Name => "HectorSLAM";

    public string FriendlyName => "Hector SLAM";

    public string Version => "0.1.0";

    public Type ImplementType => typeof(HectorSLAMImplement);
    public Type InterfaceType =>  typeof(IHectorSLAM);
}
