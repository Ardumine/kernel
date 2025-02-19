namespace Kernel.Modules.Base;
public interface IModuleInterface
{
    public void Prepare();
    public void Start();
    public void EndStop();
}