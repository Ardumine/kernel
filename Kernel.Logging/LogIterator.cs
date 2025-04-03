namespace Kernel.Logging;

public class LogIterator<T> : IEnumerable<T>
{
    private IEnumerable<T> _list { get; set; }
    private ILogger _logger { get; set; }
    private string _description { get; set; }
    public LogIterator(IEnumerable<T> lista, ILogger logger, string description)
    {
        _list = lista;
        _logger = logger;
        _description = description;
    }
    public IEnumerator<T> GetEnumerator()
    {
        int i = 0;
        foreach (var item in _list)
        {
            int max = _list.Count();

            UpdateScreen(i + 1, max);
            yield return item;
            i++;
        }

        _logger.NewLine();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    private void UpdateScreen(int current, int max)
    {
        //Console.ForegroundColor = ConsoleColor.White;
        _logger.PrintRawNewLine($"[{_description}] {current}/{max} {(float)current / max * 100.0f:0.00}%");
    }
}