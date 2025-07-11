using System.Reflection;
using System.Reflection.Emit;
using Kernel.Modules.Base.MKCalls.RequestTypes;
using Kernel.Modules.Base.MKCalls.RequestTypes.SubModules;
using Kernel.Modules.Base.MKCalls.ResultTypes;

namespace Kernel.Modules.Base.MKCalls;

/// <summary>
/// From the modules to the kernel. Each modules has an call client and an handler.
/// </summary>
public sealed unsafe class MKCallClient
{
    private Func<MKCallRequest, MKCallResult>? kernelCallFunc;
    public MKCallClient(delegate*<MKCallRequest, MKCallResult> func)
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


    public T? GetModuleInterface<T>(string MPath) where T : class, IModuleInterface
    {
        return MakeCall<GetModuleInterfaceResult<T>>(new GetModuleInterfaceRequest<T>()
        {
            ModulePath = MPath
        }).ModuleInterface;
    }

    #region Sub-Modules 
    public T? CreateSubModule<T>(string MPath, string moduleName, bool StartOnCreation) where T : class, ISubModuleInterface
    {
        return MakeCall<CreateSubModuleInterfaceResult<T>>(new CreateSubModuleRequest<T>()
        {
            SubModulePath = MPath,
            SubModuleName = moduleName,
            StartOnCreation = StartOnCreation
        }).SubModuleInterface;
    }


    public void StartSubModule(string path)
    {
        MakeCall<NoValueResult>(new StartSubModuleRequest()
        {
            SubModulePath = path,
        });
    }


    public void StopSubModule(string path)
    {
        MakeCall<NoValueResult>(new StopSubModuleRequest()
        {
            SubModulePath = path,
        });
    }



    public void DeleteSubModule(string path)
    {
        MakeCall<NoValueResult>(new DeleteSubModuleRequest()
        {
            SubModulePath = path,
        });
    }

    #endregion
}