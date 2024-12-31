using Kernel.Modules.Base;

namespace Ardumine.Modules.SLAM.Hector;

public class HectorSLAMDescription : ModuleDescription
{
    public string Name => "HectorSLAM";

    public string FriendlyName => "Hector SLAM";

    public string Version => "0.1.0";//https://www.geeksforgeeks.org/how-to-check-if-a-given-point-lies-inside-a-polygon/

    public Type ImplementType => typeof(HectorSLAMImplement);
    public Type BaseType => typeof(HectorSLAM);

    public Type InterfaceType =>  typeof(IHectorSLAM);
}
