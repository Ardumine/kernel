using Kernel.Modules.Base;

namespace Kernel.Modules;
internal class DataChannelInterface<T> : IDataChannelInterface<T>
{
    private Kernel.AFCP.DataChannelInterface<T>? afcpChannel { get; set; }


    public T? Get()
    {
        if(afcpChannel == null) return default;

        return afcpChannel.Get();
    }
    public void Set(T? value)
    {
        afcpChannel?.Set(value);
    }


    public void AddChangeEvent(Action<T?> onChangeEvent)
    {
        afcpChannel?.AddEvent(onChangeEvent);
    }
    public void RemoveChangeEvent(Action<T?> onChangeEvent)
    {
        afcpChannel?.RemoveEvent(onChangeEvent);
    }

    internal static DataChannelInterface<T>? FromAFCP(Kernel.AFCP.DataChannelInterface<T>? afcpChannel)
    {
        if (afcpChannel == null)
            return null;
        return new DataChannelInterface<T>
        {
            afcpChannel = afcpChannel
        };
    }


}