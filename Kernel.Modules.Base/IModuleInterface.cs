namespace Kernel.Modules.Base;
public interface IModuleInterface
{
    public string Path { get; set; }
    public Guid Guid { get; set; }
    public void Prepare();
    public void Start();
    public void EndStop();
}