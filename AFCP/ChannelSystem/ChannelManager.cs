using System.Text;
using Ardumine.AFCP.Core.Client;
using Ardumine.AFCP.Core.Client.RawComProt;

public class ChannelManager
{
    public static List<Channel> ChannelsOnLocal = new List<Channel>();

    public static byte[] ReadData(string URL)
    {
        var channel = ChannelsOnLocal.Where(channel => channel.URL == URL).FirstOrDefault();
        if (channel != null)
        {//Channel is on local kernel.
            return channel.Get();
        }
        return null;
    }

    public static Channel GetChannel(string URL, AFCPTCPClient client)
    {
        var channel = ChannelsOnLocal.Where(channel => channel.URL == URL).FirstOrDefault();
        if (channel != null && false)
        {//Channel is on local kernel.
            return channel;
        }
        else
        {
            var virtualChannel = new Channel(URL, client);
            virtualChannel.Local = false;
            virtualChannel.AFCP_ID = GetAFCP_ID(URL, client);

            return virtualChannel;
        }
    }
    private static ushort GetAFCP_ID(string URL, AFCPTCPClient client)
    {
        var channelData = Encoding.UTF8.GetBytes(URL);
        byte[] reqChannel = new byte[channelData.Length + 1];
        reqChannel[0] = 1;
        Array.Copy(channelData, 0, reqChannel, 1, reqChannel.Length - 1);

        var outp = client.Ask(MsgTypes.ChannelResolve, reqChannel);

        if (outp[0] == 0)
        {
            return Converter.ByteArrayToUshort(outp, 1);
        }
        else
        {
            return 0;
        }
    }

}