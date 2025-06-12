using Kernel.Modules.Base;

namespace Ardumine.Modules.YDLidarSDK;

public class YDLidarDescription : IModuleLoadDescriptor
{
    public string Name => "YDLidar";

    public string FriendlyName => "YDLidar conector";

    public string Version => "0.1.0";

    public Type ImplementType => typeof(YDLidarImplement);
    public Type InterfaceType => typeof(IYDLidar);
}
