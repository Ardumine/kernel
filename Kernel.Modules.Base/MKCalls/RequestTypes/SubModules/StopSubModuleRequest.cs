namespace Kernel.Modules.Base.MKCalls.RequestTypes.SubModules;

/// <summary>
/// An request from an module to stop an sub-module
/// </summary>
public class StopSubModuleRequest : MKCallRequest
{
    public required string SubModulePath { get; set; }

}
