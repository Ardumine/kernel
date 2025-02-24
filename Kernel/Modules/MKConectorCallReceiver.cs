using System.Security;
using Kernel.AFCP;
using Kernel.Modules;
using Kernel.Modules.Base;

/// <summary>
/// This is what receives the kernel calls from the modules.
/// The modules CANNOT have DIRECT ACCESS with this class.
/// </summary>
internal class MKConectorCallReceiver : IMKConector
{
    public required LocalRModule WorkingModule { get; init; }

    public required ChannelManager channelManager { get; init; }
    public required ModuleManager moduleManager { get; init; }

    public IDataChannelInterface<T>? GetChannelInterface<T>(string MPath)
    {
        var channelRealPath = WorkingModule.ModulePermissions?.SolveMPath(MPath);

        //Check if the module has permission to that channel
        if (channelRealPath == null)
            throw new SecurityException($"The module '{WorkingModule.Path}' does not have access to the channel MPath '{MPath}'");

        var dataChannel = channelManager.GetDataChannelInterface<T>(channelRealPath);

        return DataChannelInterface<T>.FromAFCP(dataChannel);
    }

    public IExistingDataChannelDescriptor? GetDataChannel(string MPath)
    {
        var channelRealPath = WorkingModule.ModulePermissions?.SolveMPath(MPath);

        //Check if the module has permission to that channel
        if (channelRealPath == null)
            throw new SecurityException($"The module '{WorkingModule.Path}' does not have access to the channel MPath '{MPath}'");

        var afcpChannel = channelManager.GetDataChannel(channelRealPath);

        return ExistingDataChannelDescriptor.FromAFCP(afcpChannel);
    }

    public IExistingDataChannelDescriptor? InitiateChannel<T>(string MPath)
    {
        var channelRealPath = WorkingModule.ModulePermissions?.SolveMPath(MPath);

        //Check if the module has permission to that channel
        if (channelRealPath == null)
            throw new SecurityException($"The module '{WorkingModule.Path}' does not have access to the channel MPath '{MPath}'");

        var afcpChannel = channelManager.CreateLocalDataChannel<T>(channelRealPath);

        return ExistingDataChannelDescriptor.FromAFCP(afcpChannel);

    }

    T IMKConector.GetModuleInterface<T>(string Path)
    {
        //We need to check if it has the permissions to do so.
        return moduleManager.GetModuleInterface<T>(Path);
    }
}