namespace Kernel.Modules.Base;

/// <summary>
/// This indicates an existing channel.
/// </summary>
public interface IExistingChannel : IChannelState
{
    /// <summary>
    /// Where this channel is located
    /// </summary>
    public Guid OwningKernelGUID { get; }
}