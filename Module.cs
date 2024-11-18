
namespace Ardumine.Module;
public interface Module
{

    public string Name { get => "Ardumine.Module.Base"; }
    public string FriendlyName { get => "Base"; }
    public string Version { get => "0.1.0"; }

    public void Prepare(Logger logger);

    public void Start();
    public void EndStop();

}