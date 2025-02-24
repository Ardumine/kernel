namespace Kernel.Modules.Base;

public interface IDataChannelInterface<T>
{
    public void Set(T? value);
    public T? Get();

    public void AddChangeEvent(Action<T?> onChangeEvent);
    public void RemoveChangeEvent(Action<T?> onChangeEvent);

}