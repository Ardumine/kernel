namespace Kernel.Modules.Base.MKCalls.RequestTypes;

/// <summary>
/// An request from an module to create an sub-module
/// </summary>
/// <typeparam name="T">The sub-module interface type</typeparam>
public class CreateSubModuleInterfaceRequest<T> : MKCallRequest where T : class, ISubModuleInterface
{
    public required string SubModulePath { get; set; }
    public required string SubModuleName { get; set; }

}
