namespace Kernel.Modules.Base;

/// <summary>
/// This is only used when you create the module itself.
/// When you create an Module, you have an **Description.cs, where it stores the config to create the module implementatation.
/// </summary>
public interface IModuleLoadDescriptor
{
    /// <summary>
    /// Module name. It is used in the config.json to define the module type.
    /// Ex: "VidDownloader"
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Module friendly name.
    /// Ex: "Cool video downloader"
    /// </summary>
    public string FriendlyName { get; }
    
    /// <summary>
    /// The version of the module.
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// The class that implements the module functionality.
    /// Ceed to be derived from BaseImplement and the Module's functionality interface[InterfaceType].
    /// </summary>
    public Type ImplementType { get; }

    /// <summary>
    /// The interface that contatins the functions that other modules can call to this module.
    /// Need to be derived from IModuleInterface.
    /// </summary>
    public Type InterfaceType { get; }

}