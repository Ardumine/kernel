namespace Kernel.Modules.Base.MKCalls.ResultTypes;

/// <summary>
/// The result of GetModuleInterfaceRequest
/// </summary>
/// <typeparam name="T">The module interface type</typeparam>
public class GetModuleInterfaceResult<T> : MKCallResult where T : IModuleInterface
{
    public required T? ModuleInterface { get; set; }
}
