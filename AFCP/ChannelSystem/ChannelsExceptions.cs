[Serializable]
public class ChannelsExceptions : Exception
{
    public ChannelsExceptions() : base() { }
    public ChannelsExceptions(string message) : base(message) { }
    public ChannelsExceptions(string message, Exception inner) : base(message, inner) { }
}