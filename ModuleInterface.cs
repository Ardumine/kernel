public interface ModuleInterface
{
    public string Path { get; set; }
    public  void Prepare();

    public  void Start();
    public  void EndStop();
}