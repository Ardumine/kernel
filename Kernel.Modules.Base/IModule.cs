using Kernel.Modules.Base;

namespace Kernel.Modules;

/// <summary>
/// A module that may or may not exist.
/// </summary>
public abstract class IModule
{
    public required virtual string Path { get; init; }
    public required virtual IModuleLoadDescription ModuleLoadDescription{ get; init; }

}
