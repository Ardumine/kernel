namespace Kernel.Modules.Base.MKCalls.RequestTypes.SubModules;

/// <summary>
/// An request from an module to start an sub-module
/// </summary>
public class StartSubModuleRequest : MKCallRequest
{
    public required string SubModulePath { get; set; }

}
