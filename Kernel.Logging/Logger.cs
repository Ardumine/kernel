namespace Kernel.Logging;
public class ConsoleLogger : ILogger
{
    public ConsoleLogger(string name, ILogger? parent = null) : base(name, parent)
    {
    }


    /*  /// <summary>
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

    */



    public override void NewLine()
    {
        Console.WriteLine();
    }

    private void PrintHeader()
    {
        var ant = Console.BackgroundColor;
        var antx = Console.ForegroundColor;

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"[{GetTime()} {_Name}] ");//DateTime.UtcNow
        Console.BackgroundColor = ant;
        Console.ForegroundColor = antx;
    }


    public override void PrintMessage(string message, ConsoleColor color, string Letter)
    {
        //Lock to not allow printing at the same time.
        lock (_lockObject)
        {
            var ant = Console.BackgroundColor;
            var antx = Console.ForegroundColor;

            Console.ForegroundColor = color;

            Console.Write(Letter);
            PrintHeader();
            Console.WriteLine(message);

            Console.BackgroundColor = ant;
            Console.ForegroundColor = antx;
        }
    }

    public override ILogger CreateSubLogger(string subName)
    {
        return new ConsoleLogger($"{_Name}.{subName}", this);
    }

    internal override void PrintRawNewLine(string text)
    {
        Console.WriteLine(text);
    }
}
