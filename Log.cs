public class Logger
{
    public void LogI(string dado)
    {
        Console.WriteLine($"I[{DateTime.UtcNow}] {dado}");
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