using Kernel.AFCP;
using Kernel.AFCP.Packets;

public class KernelDescriptor
{
    public static LocalRunningKernel CreateLocalKernelDescriptor()
    {
        return new LocalRunningKernel()
        {
            Guid = Guid.NewGuid()
        };
    }

    public static LocalRunningKernel CreateLocalKernelDescriptor(string guid)
    {
        return new LocalRunningKernel()
        {
            Guid = Guid.Parse(guid),
        };
    }

    public required Guid Guid { get; init; }


}

/// <summary>
/// It's a kernel that we are connected to
/// </summary>
public abstract class RunningKernel : KernelDescriptor
{
    public abstract T? SendRequest<T>(byte RequestType, PacketBaseRequest Payload) where T : BasePacketAnswer;
    public abstract void OnRemoteDisconnect();
    public abstract void RequestDisconect();


}

/// <summary>
/// A remote kernel that we are connected to
/// </summary>
public class RemoteRunningKernel : RunningKernel
{
    public required ChannelManagerAfcpClientConector ConnectedVia { get; init; }



    public override T? SendRequest<T>(byte RequestType, PacketBaseRequest Payload) where T : class
    {
        return ConnectedVia.SendRequest<T>(RequestType, Payload);
    }

    public override void OnRemoteDisconnect()
    {
        ConnectedVia.OnRemoteDisconnect();
    }

    public override void RequestDisconect()
    {
        ConnectedVia.RequestDisconect();
    }
}

/// <summary>
/// An interface to the local kernel
/// </summary>
public class LocalRunningKernel : RunningKernel
{
    public override T? SendRequest<T>(byte RequestType, PacketBaseRequest Payload) where T : class
    {
        throw new NotImplementedException();
    }

    public override void OnRemoteDisconnect()
    {
        throw new NotImplementedException();
    }
    
    public override void RequestDisconect()
    {
        throw new NotImplementedException();
    }

}

