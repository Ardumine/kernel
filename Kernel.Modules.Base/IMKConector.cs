namespace Kernel.Modules.Base;

//This class allows modules to get channels and other modules.
//Basically an API between the module and the Kernel.

/// <summary>
/// This connects the modules with the kernel.
/// You can think of it like the System Calls.
/// </summary>
public interface IMKConector
{
    public IExistingDataChannelDescriptor? GetDataChannel(string MPath);
    public IExistingDataChannelDescriptor? InitiateChannel<T>(string MPath);
    public IDataChannelInterface<T>? GetChannelInterface<T>(string MPath);
    public T GetModuleInterface<T>(string MPath) where T : class, IModuleInterface;
}
