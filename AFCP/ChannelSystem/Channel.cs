public class Channel
{

    public string URL { get; private set; }
    public ChannelPermission Perms { get; private set; }
    public ushort AFCP_ID { get; set; }
    public Channel(string _URL)
    {
        URL = _URL;
    }

    public Channel(string _URL, ChannelPermission _perms)
    {
        URL = _URL;
        Perms = _perms;
    }

    /// <summary>
    /// Only the owner of the channel should call this.
    /// </summary>
    public void Allocate()
    {

    }

    public void Set(byte[] data)
    {
        CurrentValue = data;
    }
    public byte[] Get()
    {
        return CurrentValue;
    }

    public byte[] CurrentValue;//Just for testing
}
public enum ChannelPermission
{
    Read,

    Write,
    ReadWrite
}