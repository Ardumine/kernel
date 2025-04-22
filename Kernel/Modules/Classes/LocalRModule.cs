using Kernel.AFCP;
using Kernel.Modules;
using Kernel.Modules.Base;

/// <summary>
/// This is only meant to be used by the kernel.
/// The modules CANNOT have DIRECT ACCESS with this class.
/// </summary>
internal class LocalRModuleDescriptor : IRModule
{
    internal LocalRModuleDescriptor()
    {

    }

    public required BaseImplement Implementation { get; internal set; }
    public MKCallReceiver? CallReceiver { get; internal set; }
    public IModulePermissions? ModulePermissions { get; internal set; }
    public ModuleChannel? ModuleChannel { get; internal set; }


    public string? StartAfter { get; internal set; }
    public bool? StartOnBoot { get; internal set; }
    public bool? Running { get; internal set; }

}