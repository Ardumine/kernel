public abstract class BaseSystem
{
    public BaseAFCPServer Server;
    public abstract void Start();
    public Logger logger { get; set; }

}