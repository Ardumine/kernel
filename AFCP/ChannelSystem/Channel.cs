using Ardumine.AFCP.Core.Client;

public class Channel
{

    public string URL { get; private set; }
    public ChannelPermission Perms { get; private set; }
    public ushort AFCP_ID { get; set; }
    public bool Local { get; set; }
    private AFCPTCPClient ?Client;
    public Channel(string _URL, AFCPTCPClient client)
    {
        URL = _URL;
        Client = client;
    }

    public Channel(string _URL, ChannelPermission _perms)
    {
        URL = _URL;
        Perms = _perms;
    }


    public void Set(byte[] data)
    {
        if (Local)
        {
            CurrentValueLocal = data;
        }
        else
        {
            Client?.Post(AFCP_ID, data);
        }
    }
    public byte[] Get()
    {
        if (Local)
        {
            return CurrentValueLocal;
        }
        else
        {
            //    var ans = AFCPClient.Ask(499, []);
            return Client?.Ask(AFCP_ID, []);
        }
    }

    private byte[] CurrentValueLocal = [16, 1, 5, 89];//Just for testing
}
public enum ChannelPermission
{
    Read,

    Write,
    ReadWrite
}