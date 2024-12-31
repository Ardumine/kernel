using AFCP;

public class KernelDescriptor{

    public Guid KernelGuid {get;set;}
    public List<string> SubscribedEvents = new();
    public required ChannelManagerClient channelManagerClient;

}