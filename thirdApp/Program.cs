public class TestClass
{
    public int Param1 { get; set; }
}


public class IntSerializer : CustomSerializer
{
    public Type type => typeof(int);
    public byte[] Serialize(object obj)
    {
        return BitConverter.GetBytes((int)obj);
    }
}

public interface CustomSerializer
{
    public Type type { get; }
    public byte[] Serialize(object obj);
}


public abstract class Serializer
{
    public void Check(){
        UInt128 a = (UInt128)04;
    }

    public virtual void Serialize(object obj)
    {

    }
}

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("sucata");
    }
}