
namespace Kernel;
interface Animal
{
    public void DoThing();

}
class Cat : Animal
{
    public void DoThing()
    {
        Console.WriteLine("Meow!");
    }
}
class Dog : Animal
{
    public void DoThing()
    {
        Console.WriteLine("Woof!");
    }
}
public class Program2
{
    private static void Main2(string[] args)
    {
        var cat = (Animal)Activator.CreateInstance(Type.GetType("Kernel.Cat"));
        cat.DoThing();//Not implemented!


        var dog = (Animal)Activator.CreateInstance(Type.GetType("Kernel.Dog"));
        dog.DoThing();//Not implemented!

    }
}