using Kernel.Modules.Base;

namespace Modules.USB.USBPort;

public interface IUSBPort : ISubModuleInterface
{
    public void SetName(string name);
}
