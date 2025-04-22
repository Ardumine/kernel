namespace Kernel.Modules.Base.MKCalls.ResultTypes;

/// <summary>
/// The result from an call to InitiateDataChannelRequest
/// </summary>
/// <typeparam name="T">The type of data that the channel is holding</typeparam>
public class InitiateDataChannelResult<T> : MKCallResult
{
    public required IExistingDataChannelDescriptor? Result {get;set;}
}