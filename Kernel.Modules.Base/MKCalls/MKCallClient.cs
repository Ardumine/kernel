using System.Reflection;
using System.Reflection.Emit;
using Kernel.Modules.Base.MKCalls.RequestTypes;
using Kernel.Modules.Base.MKCalls.ResultTypes;

namespace Kernel.Modules.Base.MKCalls;

/// <summary>
/// From the modules to the kernel. Each modules has an call client and an handler.
/// </summary>
public sealed unsafe class MKCallClient
{
    private Func<MKCallRequest, MKCallResult>? kernelCallFunc;
    public MKCallClient(delegate* <MKCallRequest, MKCallResult> func)
    {
        kernelCallFunc = (req) =>
        {
            return func(req);
        };
    }



    public T MakeCall<T>(MKCallRequest request) where T : MKCallResult
    {
        if (kernelCallFunc == null)
        {
            throw new ArgumentNullException("MKCallClient: Cannot invoke the kernel funcion because it is null");
        }

        //Call the kernel
        var result = kernelCallFunc(request);

        if (result.GetType() != typeof(T))
        {
            throw new ArgumentException($"The type of answer from the kernel does not correspond to the requested one");
        }

        return (T)result;
    }

    public IExistingDataChannelDescriptor? InitiateDataChannel<T>(string channelPath)
    {
        return MakeCall<InitiateDataChannelResult<T>>(new InitiateDataChannelRequest<T>()
        {
            ChannelPath = channelPath
        }).Result;
    }

    public IDataChannelInterface<T>? GetDataChannelInterface<T>(string channelPath)
    {
        return MakeCall<GetDataChannelInterfaceResult<T>>(new GetDataChannelInterfaceRequest<T>()
        {
            ChannelPath = channelPath
        }).Result;
    }
    public T? GetModuleInterface<T>(string MPath) where T : class, IModuleInterface
    {
        return MakeCall<GetModuleInterfaceResult<T>>(new GetModuleInterfaceRequest<T>()
        {
            ModulePath = MPath
        }).ModuleInterface;
    }


}