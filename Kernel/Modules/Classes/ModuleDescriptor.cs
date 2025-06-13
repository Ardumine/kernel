using Kernel.AFCP;
using Kernel.Logging;
using Kernel.Modules.Base;
using Kernel.Modules.Permissions;

namespace Kernel.Modules;

/// <summary>
/// This is only meant to be used by the kernel. It represents a Running Module.
/// The modules CANNOT have DIRECT ACCESS with this class.
/// </summary>
internal class ModuleDescriptor : IRModule
{
    public required BaseImplement Implementation { get; internal set; }

    public MKCallReceiver? CallReceiver { get; internal set; }
    public IModulePermissions? ModulePermissions { get; internal set; }
    public ModuleChannel? ModuleChannel { get; internal set; }


    public string StartAfter { get; internal set; } = string.Empty;
    public bool StartOnBoot { get; internal set; } = false;

    public bool IsPrepared { get; private set; } = false;
    public bool IsRunning { get; private set; } = false;
    public bool IsDeleted { get; private set; } = false;

    public List<SubModuleDescriptor> SubModules = new List<SubModuleDescriptor>();


    internal ILogger? _logger { get; set; }

    internal ModuleDescriptor()
    {

    }

    public void Prepare()
    {
        if (!IsPrepared)
        {
            Implementation.Prepare();
            IsPrepared = true;
        }
    }

    public void Start()
    {
        if (!IsRunning)
        {
            Implementation.Start();
            IsRunning = true;
        }
    }

    public void Stop()
    {
        //The module is also in charge of stopping it's sub-modules
        if (IsRunning)
        {
            Implementation.EndStop();
            IsRunning = false;

            foreach (var subMod in SubModules)
            {
                if (subMod.IsRunning)
                {
                    _logger?.LogW($"The module did not stop the sub-module '{subMod.Path}'! Stopping manually...");
                    subMod.Stop();
                }
            }
        }
    }


    /// <summary>
    /// Please use the ModuleManager.DeleteModule
    /// </summary>
    public void InternalDelete()
    {
        if (IsRunning)
            Stop();


        if (!IsDeleted)
        {
            Implementation.Delete();
            IsDeleted = true;
        }
        //tell module manager to delete the entry
    }

}