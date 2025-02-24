using Kernel.Modules;

public class StartModBranch
{
    public required IModule Module { get; set; }
    public List<StartModBranch> Dependers { get; set; } = new();
}