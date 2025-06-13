namespace Kernel.Modules.Base.MKCalls.ResultTypes;

/// <summary>
/// The result of GetDataChannelInterfaceRequest
/// </summary>
/// <typeparam name="T">The type of data that the channel is holding</typeparam>
public class GetDataChannelInterfaceResult<T> : MKCallResult
{
    public required IDataChannelInterface<T>? Result { get; set; }
}