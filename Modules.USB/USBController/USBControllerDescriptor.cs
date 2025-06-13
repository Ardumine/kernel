using Kernel.Modules.Base;

namespace Modules.USB.USBController;

public class USBControllerDescriptor : IModuleLoadDescriptor
{

    public string Name => "USBController";

    public string FriendlyName => "USBController";

    public string Version => "0.1.0";

    public Type ImplementType => typeof(USBControllerImplement);

    public Type InterfaceType => typeof(IUSBController);
}


