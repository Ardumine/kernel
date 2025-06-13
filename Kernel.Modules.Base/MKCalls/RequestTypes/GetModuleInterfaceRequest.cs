namespace Kernel.Modules.Base.MKCalls.RequestTypes;

/// <summary>
/// An request from an module to get another module's interface(conector)
/// </summary>
/// <typeparam name="T">The module interface type</typeparam>
public class GetModuleInterfaceRequest<T> : MKCallRequest where T : class, IModuleInterface
{
    public required string ModulePath { get; set; }
}
