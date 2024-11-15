
namespace Ardumine.Module;
public class Module
{
    public string Name { get; }
    public string FriendlyName { get; }
    public string Version { get; }

    public void Prepare(Logger logger){}

    public void Start(){}
    public void EndStop(){}

}