public class Logger
{
    private Logger() { Name= ""; }

    private string Name { get; set; }
    public Logger(string _Name)
    {
        Name = _Name;
    }

    public string GetTime(){
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

    public void Space(){
        Console.WriteLine();
    }
}