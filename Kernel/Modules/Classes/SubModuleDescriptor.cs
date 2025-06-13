using Kernel.AFCP;
using Kernel.Modules.Base;
using Kernel.Modules.Permissions;

namespace Kernel.Modules;

/// <summary>
/// This is only meant to be used by the kernel. It represents a sub-module made by an parent Module.
/// The modules CANNOT have DIRECT ACCESS with this class.
/// </summary>
internal class SubModuleDescriptor : ModuleDescriptor
{
    public required ModuleDescriptor ParentModule { get; internal set; }

    internal SubModuleDescriptor()
    {

    }
  
}