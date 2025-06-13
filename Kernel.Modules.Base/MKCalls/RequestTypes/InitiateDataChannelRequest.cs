namespace Kernel.Modules.Base.MKCalls.RequestTypes;

/// <summary>
/// An request from an module to get the initiate a data channel
/// </summary>
/// <typeparam name="T">The type of data that the channel is holding</typeparam>
public class InitiateDataChannelRequest<T> : MKCallRequest
{
    public required string ChannelPath { get; set; }
}