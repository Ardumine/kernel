public abstract class BaseSystem
{
    public required BaseAFCPServer Server;
    public abstract void Start();
    public required Logger logger { get; set; }

}