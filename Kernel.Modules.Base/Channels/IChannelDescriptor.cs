namespace Kernel.Modules.Base;

/// <summary>
/// This is only a descriptor.
/// The channel may or may not exist.
/// </summary>

public interface IChannelDescriptor
{
    public string Path { get; }
}