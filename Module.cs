
namespace Ardumine.Module;
public class ModuleBase
{
    public virtual string Name { get => "Ardumine.Module.Base"; }
    public virtual string FriendlyName { get => "Base"; }
    public virtual string Version { get => "0.1.0"; }
    public string RunningName { get; set; }

    public virtual void Prepare(Logger logger)
    {
        Console.WriteLine(2);
    }

    public virtual void Start() { }
    public virtual void EndStop()
    {

    }
}

public class Module
{

    public string Name { get => "Ardumine.Module.Base"; }
    public string FriendlyName { get => "Base"; }
    public string Version { get => "0.1.0"; }
    public bool Running { get; set; } = false;
    public bool Enabled { get; set; } = false;

}