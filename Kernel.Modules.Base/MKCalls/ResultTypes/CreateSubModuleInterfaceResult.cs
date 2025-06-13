namespace Kernel.Modules.Base.MKCalls.ResultTypes;

/// <summary>
/// The result of CreateSubModuleInterfaceRequest
/// </summary>
/// <typeparam name="T">The sub-module interface type</typeparam>
public class CreateSubModuleInterfaceResult<T> : MKCallResult where T : ISubModuleInterface
{
    public required T? SubModuleInterface { get; set; }
}
