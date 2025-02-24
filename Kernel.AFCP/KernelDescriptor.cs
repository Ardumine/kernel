using Kernel.AFCP;

public class KernelDescriptor
{
    public static KernelDescriptor CreateLocalKernelDescriptor(){
        return new KernelDescriptor(){
            Guid = Guid.NewGuid()
        };
    }
    public required Guid Guid { get; init; }

    //public required List<ModuleDescriptor> HostingModules { get; init; }
    //public required List<DataChannelDescriptor> HostingChannels { get; init; }

    public ChannelManagerAfcpClientConector? ConnectedVia { get; init; }

}