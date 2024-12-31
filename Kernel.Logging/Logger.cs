namespace Kernel.Logging;
public class Logger
{
    private Logger() { Name = ""; }

    private string Name { get; set; }
    public Logger(string _Name)
    {
        Name = _Name;
    }

    public string GetTime()
    {
        return DateTime.UtcNow.ToShortTimeString();
    }
    public void LogI(string dado)
    {
        var ant = Console.BackgroundColor;
        var antx = Console.ForegroundColor;

        Console.ForegroundColor = ConsoleColor.Cyan;
        //Console.ForegroundColor = ConsoleColor.Black;

        Console.WriteLine($"I[{GetTime()} {Name}] {dado}");
        Console.BackgroundColor = ant;
        Console.ForegroundColor = antx;


    }

    public void LogOK(string dado)
    {
        var ant = Console.BackgroundColor;
        var antx = Console.ForegroundColor;

        Console.ForegroundColor = ConsoleColor.Green;
        //Console.ForegroundColor = ConsoleColor.Black;

        Console.WriteLine($"I[{GetTime()} {Name}] {dado}");
        Console.BackgroundColor = ant;
        Console.ForegroundColor = antx;


    }

    public void LogW(string dado)
    {
        var ant = Console.BackgroundColor;
        var antx = Console.ForegroundColor;

        Console.ForegroundColor = ConsoleColor.Yellow;
        //Console.ForegroundColor = ConsoleColor.Black;

        Console.WriteLine($"I[{DateTime.UtcNow} {Name}] {dado}");
        Console.BackgroundColor = ant;
        Console.ForegroundColor = antx;


    }


    /// <summary>
    /// Log a list item
    /// </summary>
    /// <param name="item"></param>
    public void LogL(string item)
    {
        var ant = Console.BackgroundColor;
        var antx = Console.ForegroundColor;

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"     {item}");

        Console.BackgroundColor = ant;
        Console.ForegroundColor = antx;

    }


    public LogIterator<T> CreateIterator<T>(IEnumerable<T> list, string Description)
    {
        return new LogIterator<T>(list, this, Description);
    }
    public void Space()
    {
        Console.WriteLine();
    }
    public void ClearCurrentConsoleLine()
    {
        Console.CursorLeft = 0;
        Console.Write(new string(' ', Console.WindowWidth));
        Console.CursorLeft = 0;
    }




}
public class LogIterator<T> : IEnumerable<T>
{
    private IEnumerable<T> list { get; set; }
    private Logger logger { get; set; }
    private string Description { get; set; }
    public LogIterator(IEnumerable<T> _list, Logger _logger, string _description)
    {
        list = _list;
        logger = _logger;
        Description = _description;
    }
    public IEnumerator<T> GetEnumerator()
    {
        
        int i = 0;
        foreach (var item in list)
        {
            int max = list.Count();

            UpdateScreen(i + 1, max);
            yield return item;
            i++;
        }
        
        Console.WriteLine("");


    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    private void UpdateScreen(int current, int max)
    {
        Console.ForegroundColor = ConsoleColor.White;
        logger.ClearCurrentConsoleLine();
        Console.Write($"[{Description}] {current}/{max} {(float)current / max * 100.0f:0.00}%");
    }
}