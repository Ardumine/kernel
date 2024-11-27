using System.Reflection;

public interface IThing
{
    public string Path { get; set; }
}
public interface IImplement : IThing
{

}

public interface ICar : IThing
{
    public void SetSpeed(int speed);
    public void SetName(string name);
    public string GetName();
    public string Name { get; set; }
}

//Im trying to get rid of this. 
//All the methods that ICar has should be implemented automatically, running Runner.RunMethod with the parameters and the function name.

public class CarImplement : ICar, IImplement
{
    public string GetName()
    {
        return Name;
    }

    public void SetName(string name)
    {
        Name = name;
        Console.WriteLine($"Set name: {name}");
    }

    public void SetSpeed(int speed)
    {
        Console.WriteLine($"Set speed: {speed}");
    }
    public string Name { get; set; }
    public string Path { get; set; }
}


public class Runner
{

    static List<IImplement> implements = new List<IImplement>();
    public static void CreateCar(string Name)
    {
        var car = new CarImplement();
        car.Path = "/" + Name;
        car.Name = Name;

        implements.Add(car);
    }


    public static object RunMethod(string Path, string Name, object[] parameters)
    {
        var impl = implements.Where(im => im.Path == Path).First();

        var type = impl.GetType();
        var myMethod = type.GetMethod(Name);


        //Console.WriteLine($"Running method {Name} with {parameters.Length} parameters on car {Path}");
        return myMethod.Invoke(impl, parameters);
    }
}



class HelloDispatchProxy<T> : DispatchProxy where T : class, IThing
{
    private string Path { get; set; }

    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        // Code here to track time, log call etc.
        //var result = targetMethod.Invoke(Target, args);
        return Runner.RunMethod(Path, targetMethod.Name, args);
    }

    public static T CreateProxy(string Path)
    {
        var proxy = Create<T, HelloDispatchProxy<T>>() as HelloDispatchProxy<T>;
        proxy.Path = Path;
        return proxy as T;
    }
}

class Programe
{
    public static void Maine()
    {
        Runner.CreateCar("car1");
        Runner.CreateCar("car2");

        var car1 = HelloDispatchProxy<ICar>.CreateProxy("/car1");
        var car2 = HelloDispatchProxy<ICar>.CreateProxy("/car2");

        car1.SetSpeed(80);
        car2.SetSpeed(90);

        Console.WriteLine($"Car1 Name: {car1.Name}");
        Console.WriteLine($"Car2 Name: {car2.Name}");


    }

}