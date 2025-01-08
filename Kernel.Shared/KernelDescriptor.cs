using Kernel.AFCP;
using Kernel.Modules.Base;

namespace Kernel.Shared;

public class KernelDescriptor
{
    public Guid KernelGuid { get; private set; }
    public List<DataChannelDescriptor> Channels { get; private set; }

    public List<Module> HostingModules { get; private set; }
}
