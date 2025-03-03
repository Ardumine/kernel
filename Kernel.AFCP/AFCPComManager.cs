using Kernel.AFCP.Packets;

namespace Kernel.AFCP;

/// <summary>
/// Handles the communication between kernels on a High Level
/// </summary>
public class AFCPComManager
{
    
    public AFCPComManager(){

    }
    /// <summary>
    /// Send request to remote kernel
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void SendRequest<T>(byte RequestType, PacketBaseRequest Payload, KernelDescriptor kernel)
    {

    }
}