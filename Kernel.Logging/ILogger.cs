namespace Kernel.Logging;

public abstract class ILogger
{
    internal string _Name { get; set; }
    internal readonly object _lockObject;

    public ILogger(string name, ILogger? parent = null)
    {
        _Name = name;

        if (parent == null)
        {
            _lockObject = new();
        }
        else { _lockObject = parent._lockObject; }
    }


    public string GetTime()
    {
        return DateTime.UtcNow.ToShortTimeString();
    }
    public void LogI(string msg)
    {
        PrintMessage(msg, ConsoleColor.Cyan, "I");
    }

    public void LogOK(string msg)
    {
        PrintMessage(msg, ConsoleColor.Green, "O");
    }

    public void LogW(string msg)
    {
        PrintMessage(msg, ConsoleColor.Yellow, "W");
    }

    public void LogE(string msg)
    {
        PrintMessage(msg, ConsoleColor.Red, "E");
    }





    public abstract void NewLine();
    internal abstract void PrintRawNewLine(string text);

    public abstract void PrintMessage(string message, ConsoleColor color, string Letter);
    public abstract ILogger CreateSubLogger(string subName);


    public LogIterator<T> CreateIterator<T>(IEnumerable<T> list, string Description)
    {
        return new LogIterator<T>(list, this, Description);
    }


}