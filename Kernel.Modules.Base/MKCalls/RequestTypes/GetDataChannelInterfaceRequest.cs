namespace Kernel.Modules.Base.MKCalls.RequestTypes;

/// <summary>
/// An request from an module to get the interface to a channel's interface(conector)
/// </summary>
/// <typeparam name="T">The type of data that the channel is holding</typeparam>
public class GetDataChannelInterfaceRequest<T> : MKCallRequest
{
    public required string ChannelPath { get; set; }

}