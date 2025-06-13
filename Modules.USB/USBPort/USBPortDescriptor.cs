using Kernel.Modules.Base;

namespace Modules.USB.USBPort;

public class USBPortDescriptor : IModuleLoadDescriptor
{

    public string Name => "USBPort";

    public string FriendlyName => "USBPort";

    public string Version => "0.1.0";

    public Type ImplementType => typeof(USBPortImplement);

    public Type InterfaceType => typeof(IUSBPort);
}


