using Kernel.AFCP;

public class KernelDescriptor
{

    public required Guid KernelGuid { get; init; }

    //public required List<ModuleDescriptor> HostingModules { get; init; }
    //public required List<DataChannelDescriptor> HostingChannels { get; init; }

    public required ChannelManagerClient channelManagerClient { get; init; }

}