using System.Security;
using Kernel.AFCP;
using Kernel.Modules;
using Kernel.Modules.Base;

/// <summary>
/// This is what receives the kernel calls from the modules.
/// THE MODULES CANNOT HAVE DIRECT ACCESS WITH THIS CLASS.
/// 
/// Currently this class is directly connected to the BaseImplement.MKConector(basically contradicting what I have just said).
/// This is not secure because Reflection can access the Properties of this class(MKConectorCallReceiver) and get the ChannelManager, ModuleManager and others.
/// If the modules can get access to them, they have access to other modules and channels(dangerous).
/// To prevent this, we can set the BaseImplement.MKConector to an derived class automatically created by ILGenerator.
/// In this derived class, the ILGenerator creates the same functions that IMKConector requires.
/// 'But how do we solve the problem with ChannelManager? The class created by ILGenerator is still a class and reflection can the access the properties of it!'
/// Heres the thing. Based on what I understand, each object in .NET has pointer(even if it is a object).
/// With the object pointer of a object, let's say ChannelManager, we can 'hard code' (using ILGenerator) that pointer to the functions that require this object.
/// Look here: https://stackoverflow.com/questions/4989681/place-an-object-on-top-of-stack-in-ilgenerator
/// MKConectorCreator was the first implementation of this, but I later disabled it beacause it makes debugging really hard.
/// </summary>
internal class MKConectorCallReceiver : IMKConector
{
    public required LocalRModuleDescriptor WorkingModule { private get; init; }

    public required ChannelManager channelManager { private get; init; }
    public required DataChannelManager dataChannelManager { private get; init; }
    public required ModuleManager moduleManager { private get; init; }

    public IDataChannelInterface<T>? GetChannelInterface<T>(string MPath)
    {
        var channelRealPath = WorkingModule.ModulePermissions?.SolveMPath(MPath);

        //Check if the module has permission to that channel
        if (channelRealPath == null)
            throw new SecurityException($"The module '{WorkingModule.Path}' does not have access to the channel MPath '{MPath}'");

        var dataChannel = dataChannelManager.GetDataChannelInterface<T>(channelRealPath);

        return DataChannelInterface<T>.FromAFCP(dataChannel);
    }

    public IExistingDataChannelDescriptor? GetDataChannel(string MPath)
    {
        var channelRealPath = WorkingModule.ModulePermissions?.SolveMPath(MPath);

        //Check if the module has permission to that channel
        if (channelRealPath == null)
            throw new SecurityException($"The module '{WorkingModule.Path}' does not have access to the channel MPath '{MPath}'");

        var afcpChannel = dataChannelManager.GetDataChannelDescriptor(channelRealPath);

        return ExistingDataChannelDescriptor.FromAFCP(afcpChannel);
    }

    public IExistingDataChannelDescriptor? InitiateChannel<T>(string MPath)
    {
        var channelRealPath = WorkingModule.ModulePermissions?.SolveMPath(MPath);

        //Check if the module has permission to that channel
        if (channelRealPath == null)
            throw new SecurityException($"The module '{WorkingModule.Path}' does not have access to the channel MPath '{MPath}'");

        var afcpChannel = dataChannelManager.CreateLocalDataChannel<T>(channelRealPath);

        return ExistingDataChannelDescriptor.FromAFCP(afcpChannel);

    }

    T IMKConector.GetModuleInterface<T>(string Path)
    {
        //We need to check if it has the permissions to do so.
        return moduleManager.GetModuleInterface<T>(Path);
    }
}