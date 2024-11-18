public interface IModuleInterface
{
    public string Path { get; set; }
    public Guid guid { get; set; }
    public void Prepare();
    public void Start();
    public void EndStop();
}