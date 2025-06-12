using Kernel.AFCP;
using Kernel.Modules.Base;
using Kernel.Modules.Permissions;

namespace Kernel.Modules;
/// <summary>
/// This is only meant to be used by the kernel. It represents a Running Module.
/// The modules CANNOT have DIRECT ACCESS with this class.
/// </summary>
internal class LocalRModuleDescriptor : IRModule
{

    public required BaseImplement Implementation { get; internal set; }
    public MKCallReceiver? CallReceiver { get; internal set; }
    public IModulePermissions? ModulePermissions { get; internal set; }
    public ModuleChannel? ModuleChannel { get; internal set; }


    public string StartAfter { get; internal set; } = string.Empty;
    public bool StartOnBoot { get; internal set; } = false;
    public bool IsPrepared { get; internal set; } = false;
    public bool Running { get; internal set; } = false;


    internal LocalRModuleDescriptor()
    {

    }
    
}