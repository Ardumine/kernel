namespace Kernel.Modules.Base;

/// <summary>
/// A module that exists.
/// </summary>
public abstract class IRModule : IModule
{
    /// <summary>
    /// The kernel that is hosting this module.
    /// </summary>
    public required virtual Guid HostingKernel { get; init; }

    /// <summary>
    /// Is the module running on this kernel.
    /// </summary>
    public required virtual bool IsLocal { get; init; }
}