using Kernel.AFCP;

namespace Kernel.Modules.Base;

public abstract class RModule : ModuleDescriptor
{
    internal BaseImplement? baseImplement;
    public Guid ModuleGuid { get; set; }
    public List<ConfigChannelDescriptor>? hostingChannels { get; set; }
    public required List<ConfigChannelDescriptor> connectedChannels { get; set; }

    public bool StartOnBoot { get; set; }
    public string? startAfter { get; set; }
    public required Dictionary<string, object>? Config { get; set; }

    public BaseImplement? Implement
    {
        get
        {
            if (!IsLocal)
            {
                throw new Exception("Module not running in this kernel! Only the kernel that owns this module can run the functions internally. Even if you want to run functions from the module, always use GetInterface<[Module Interface]>([Path to the module])");
            }
            return baseImplement;
        }
        internal set
        {
            if (!IsLocal)
            {
                throw new Exception("Module not running in this kernel! Only the kernel that owns this module can run the functions internally. Even if you want to run functions from the module, always use GetInterface<[Module Interface]>([Path to the module])");
            }
            baseImplement = value;
        }
    }

    public bool Running { get; set; }
    public bool IsLocal { get; set; }


    public abstract ModuleDescription Description { get; }
    public ModuleChannel? ModuleChannel { get; internal set; }
}

public class ConfigChannelDescriptor
{
    public required string Path { get; set; }
    public required string Name { get; set; }
    public bool HighData {get;set;}
}

