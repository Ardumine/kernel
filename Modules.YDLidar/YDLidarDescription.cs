using Kernel.Modules.Base;

namespace Ardumine.Modules.YDLidar;

public class YDLidarDescription : ModuleDescription
{
    public string Name => "YDLidar";

    public string FriendlyName => "conector";

    public string Version => "0.1.0";

    public Type ImplementType => typeof(YDLidarImplement);
    public Type BaseType => typeof(YDLidar);

    public Type InterfaceType => typeof(IYDLidar);
}
