using Ardumine.Module.Base;

namespace Ardumine.Module.AFCPClientTest;

class AFCPClientTest : Base.Module
{
    public Guid guid { get; set; }
    public required string Path { get; set; }
    public ModuleDescription description => new AFCPClientTestDescription();
}
