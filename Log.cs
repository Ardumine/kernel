public class Logger
{
    private Logger(){}

    private string Name {get;set;}
    public Logger(string _Name){
        Name = _Name;
    }
    public void LogI(string dado)
    {
        Console.WriteLine($"I[{DateTime.UtcNow} {Name}] {dado}");
    }

    /// <summary>
    /// Log a list item
    /// </summary>
    /// <param name="item"></param>
    public void LogL(string item)
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($"     {item}");
    }
}