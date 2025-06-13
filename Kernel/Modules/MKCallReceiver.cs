using System.Security;
using Kernel.AFCP;
using Kernel.Modules.Base;
using Kernel.Modules.Base.MKCalls;
using Kernel.Modules.Base.MKCalls.RequestTypes;
using Kernel.Modules.Base.MKCalls.RequestTypes.SubModules;
using Kernel.Modules.Base.MKCalls.ResultTypes;

namespace Kernel.Modules;

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
/// 
/// 22/05/2025 UPDATE:
/// Modules now have an client, MKCallClient, to interact with this class, MKCallReceiver. MKCallClient calls this class via kernelCallFunc. I think that calling via IL is the best method for object security.
/// </summary>

public sealed class MKCallReceiver  
{
    internal ModuleDescriptor? WorkingModule { private get; init; }
    public required ChannelManager channelManager { private get; init; }
    public required DataChannelManager dataChannelManager { private get; init; }
    public required ModuleManager moduleManager { private get; init; }


    public IDataChannelInterface<T>? GetDataChannelInterface<T>(string MPath)
    {
        var channelRealPath = WorkingModule?.ModulePermissions?.SolveMPath(MPath);

        //Check if the module has permission to that channel
        if (channelRealPath == null)
            throw new SecurityException($"The module '{WorkingModule?.Path}' does not have access to the channel MPath '{MPath}'");

        var dataChannel = dataChannelManager.GetDataChannelInterface<T>(channelRealPath);

        return DataChannelInterface<T>.FromAFCP(dataChannel);
    }

    public IExistingDataChannelDescriptor? InitiateDataChannel<T>(string path)
    {
        var channelRealPath = WorkingModule?.ModulePermissions?.SolveMPath(path);

        //Check if the module has permission to that channel
        if (channelRealPath == null)
            throw new SecurityException($"The module '{WorkingModule?.Path}' does not have access to the channel MPath '{path}'");

        var afcpChannel = dataChannelManager.CreateLocalDataChannel<T>(channelRealPath);

        return ExistingDataChannelDescriptor.FromAFCP(afcpChannel);

    }

    public T GetModuleInterface<T>(string path) where T : class, IModuleInterface
    {
        //We need to check if it has the permissions to do so.
        return moduleManager.GetModuleInterface<T>(path);
    }

    public T GetSubModuleInterface<T>(string path) where T : class, ISubModuleInterface
    {
        //We need to check if it has the permissions to do so.
        return moduleManager.GetSubModuleInterface<T>(path);
    }

    public void CreateSubModuleInterface(string path, string moduleName, bool start)
    {
        if (moduleManager.ModuleCreator == null)
            throw new NotSupportedException("Cannot create sub-modules because ModuleCreator was not set");


        var loadDescriptor = moduleManager.ModuleCreator.TryGetModuleLoadDescriptor(moduleName);
        if (loadDescriptor == null)
            throw new KeyNotFoundException($"The module load descriptor '{moduleName}' was not found");


        moduleManager.CreateSubModuleInstance(WorkingModule!, loadDescriptor, path, start);
    }



    /// <summary>
    /// DO NOT REMOVE. Used by the 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="SecurityException"></exception>
    public MKCallResult CallHandler(MKCallRequest request)
    {
        var requestType = request.GetType();
        requestType = requestType.IsGenericType ? requestType.GetGenericTypeDefinition() : requestType;

        if (requestType == typeof(InitiateDataChannelRequest<>))
        {
            return InitiateDataChannelRequestHandler((dynamic)request);
        }
        else if (requestType == typeof(GetDataChannelInterfaceRequest<>))
        {
            return GetDataChannelInterfaceHandler((dynamic)request);
        }
        else if (requestType == typeof(GetModuleInterfaceRequest<>))
        {
            return GetModuleInterfaceRequestHandler((dynamic)request);
        }

        else if (requestType == typeof(CreateSubModuleRequest<>))
        {
            return CreateSubModuleRequestHandler((dynamic)request);
        }
        else if (requestType == typeof(StartSubModuleRequest))
        {
            return StartSubModuleRequestHandler((dynamic)request);
        }
        else if (requestType == typeof(StopSubModuleRequest))
        {
            return StopSubModuleRequestHandler((dynamic)request);
        }
        else if (requestType == typeof(DeleteSubModuleRequest))
        {
            return DeleteSubModuleRequestHandler((dynamic)request);
        }
        else
        {
            throw new NotSupportedException($"The request type '{requestType}' is not supported or not implemented.");
        }
    }


    public MKCallResult InitiateDataChannelRequestHandler<T>(InitiateDataChannelRequest<T> req)
    {
        return new InitiateDataChannelResult<T>()
        {
            Result = InitiateDataChannel<T>(req.ChannelPath)
        };
    }


    public MKCallResult GetDataChannelInterfaceHandler<T>(GetDataChannelInterfaceRequest<T> req)
    {
        return new GetDataChannelInterfaceResult<T>()
        {
            Result = GetDataChannelInterface<T>(req.ChannelPath)
        };
    }

    public MKCallResult GetModuleInterfaceRequestHandler<T>(GetModuleInterfaceRequest<T> req) where T : class, IModuleInterface
    {
        return new GetModuleInterfaceResult<T>()
        {
            ModuleInterface = GetModuleInterface<T>(req.ModulePath)
        };
    }


    #region Sub-Modules
    public MKCallResult CreateSubModuleRequestHandler<T>(CreateSubModuleRequest<T> req) where T : class, ISubModuleInterface
    {
        CreateSubModuleInterface(req.SubModulePath, req.SubModuleName, req.StartOnCreation);
        return new CreateSubModuleInterfaceResult<T>()
        {
            SubModuleInterface = GetSubModuleInterface<T>(req.SubModulePath)
        };
    }
    public MKCallResult StartSubModuleRequestHandler(StartSubModuleRequest req)
    {
        //TODO: Checks to check if the module is an sub-module of the parent module and the action can be performed

        var modImpl = moduleManager.GetLocalModuleDescriptor(req.SubModulePath);
        if (modImpl == null)
        {
            throw new KeyNotFoundException($"The channel with the path '{req.SubModulePath}' was not found!");
        }
        modImpl.Start();
        return new NoValueResult();
    }
    public MKCallResult StopSubModuleRequestHandler(StopSubModuleRequest req)
    {
        //TODO: Checks to check if the module is an sub-module of the parent module and the action can be performed

        var modImpl = moduleManager.GetLocalModuleDescriptor(req.SubModulePath);
        if (modImpl == null)
        {
            throw new KeyNotFoundException($"The channel with the path '{req.SubModulePath}' was not found!");
        }
        modImpl.Stop();
        return new NoValueResult();
    }
    public MKCallResult DeleteSubModuleRequestHandler(DeleteSubModuleRequest req)
    {
        //TODO: Checks to check if the module is an sub-module of the parent module and the action can be performed

        var modImpl = moduleManager.GetLocalModuleDescriptor(req.SubModulePath);
        if (modImpl == null)
        {
            throw new KeyNotFoundException($"The channel with the path '{req.SubModulePath}' was not found!");
        }
        moduleManager.DeleteModule(modImpl);
        return new NoValueResult();
    }


    #endregion

}