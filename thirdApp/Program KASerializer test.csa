using System.Diagnostics;
using System.Text.Json;
using AFCP.KASerializer;
using AFCP.KASerializer.Atributes;

public class TestClass
{
    public int Param1 { get; set; }

    public List<int> SubClasses { get; set; } = new();

    public List<TestSubClass> testSubClasses { get; set; } = new();
    [CanHaveOtherTypes]
    public object Obj { get; set; }

}


public class TestSubClass
{
    public string Param3 { get; set; }
}

//https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/built-in-types

internal class Program
{
    static void PrintArr(byte[] arr)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            Console.Write(arr[i] + " ");
        }
        Console.WriteLine();
    }
    private static void Main(string[] args)
    {
        var ser = new KASerializer();


        ser.GenerateCacheForType(typeof(TestClass));

        MemoryStream ms = new MemoryStream();

        var obj = new TestClass()
        {
            Obj = new TestSubClass()
            {
                Param3 = "testtesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttest"
            }
        };

        ser.Serialize(obj, ms);

        var data = ms.ToArray();

        PrintArr(data);

        var deser = ser.Deserialize<TestClass>(new MemoryStream(data));

        Console.WriteLine(((TestSubClass)deser.Obj).Param3);



    }

    static void BenchMark(KASerializer ser)
    {

        var obj = new TestClass()
        {
            Param1 = 2,
            SubClasses = new()
            {
                2,5,6,7,4,2,4,6,5,4,9,5,6,6,
                2,5,6,7,4,2,4,6,5,4,9,5,6,6,
                2,5,6,7,4,2,4,6,5,4,9,5,6,6,
                2,5,6,7,4,2,4,6,5,4,9,5,6,6,
                2,5,6,7,4,2,4,6,5,4,9,5,6,6,
                2,5,6,7,4,2,4,6,5,4,9,5,6,6,
                2,5,6,7,4,2,4,6,5,4,9,5,6,6,
                2,5,6,7,4,2,4,6,5,4,9,5,6,6,
                2,5,6,7,4,2,4,6,5,4,9,5,6,6,
                2,5,6,7,4,2,4,6,5,4,9,5,6,6,
                2,5,6,7,4,2,4,6,5,4,9,5,6,6,
                2,5,6,7,4,2,4,6,5,4,9,5,6,6,
                2,5,6,7,4,2,4,6,5,4,9,5,6,6,
                2,5,6,7,4,2,4,6,5,4,9,5,6,6,
                2,5,6,7,4,2,4,6,5,4,9,5,6,6,
                2,5,6,7,4,2,4,6,5,4,9,5,6,6,
                2,5,6,7,4,2,4,6,5,4,9,5,6,6,
                2,5,6,7,4,2,4,6,5,4,9,5,6,6,
                2,5,6,7,4,2,4,6,5,4,9,5,6,6,
                2,5,6,7,4,2,4,6,5,4,9,5,6,6,
            },
            testSubClasses = new(){
                new(){
                    Param3 = "aaaaaaaaaaa",
                },
                new(){
                    Param3 = "aaaaaaaaaaa",
                },
                new(){
                    Param3 = "aaaaaaaaaaa",
                },
                new(){
                    Param3 = "aaaaaaaaaaa",
                },
                new(){
                    Param3 = "aaaaaaaaaaa",
                },
                new(){
                    Param3 = "aaaaaaaaaaa",
                },
                new(){
                    Param3 = "aaaaaaaaaaa",
                },
                new(){
                    Param3 = "aaaaaaaaaaa",
                },
                new(){
                    Param3 = "aaaaaaaaaaa",
                },
                new(){
                    Param3 = "aaaaaaaaaaa",
                },
                new(){
                    Param3 = "aaaaaaaaaaa",
                },
                new(){
                    Param3 = "aaaaaaaaaaa",
                },
                new(){
                    Param3 = "aaaaaaaaaaa",
                },

            }
        };

        int num = 1000000;
        var sw = Stopwatch.StartNew();



        var ms = new MemoryStream();
        var ms2 = new MemoryStream();

        sw.Restart();

        {
            for (int i = 0; i < num; i++)
            {
                ser.Serialize(obj, ms);
            }
            Console.WriteLine($"A: {sw.ElapsedMilliseconds:000000} \t {(float)num / sw.ElapsedMilliseconds * 1000}Ser/Sec \t {(float)sw.ElapsedMilliseconds / num}MS/Ser \t  Len: {ms.Position}");
            long aMs = sw.ElapsedMilliseconds;





            sw.Restart();

            for (int i = 0; i < num; i++)
            {
                ms2.Write(JsonSerializer.SerializeToUtf8Bytes(obj));
            }
            Console.WriteLine($"J: {sw.ElapsedMilliseconds:000000} \t {(float)num / sw.ElapsedMilliseconds * 1000}Ser/Sec \t {(float)sw.ElapsedMilliseconds / num}MS/Ser \t  Len: {ms2.Position}");
            long jMs = sw.ElapsedMilliseconds;


            ms.Clear();//Tools.cs in thirdApp
            ms2.Clear();//Tools.cs in thirdApp

            ser.Serialize(obj, ms);
            ms2.Write(JsonSerializer.SerializeToUtf8Bytes(obj));

            byte[] data = ms.ToArray();
            // PrintArr(data);

            var des = (TestClass)ser.Deserialize(new MemoryStream(data), typeof(TestClass));
            Console.WriteLine($"A: LenDados: {data.Length} array len: {des.SubClasses.Count} VelDif(maior melhor A): {(float)jMs / aMs}X Dif: {jMs - aMs}");

        }


        Console.WriteLine();

        {
            sw.Restart();

            for (int i = 0; i < num; i++)
            {
                ms.Position = 0;
                ser.Deserialize<TestClass>(ms);
            }
            Console.WriteLine($"A: {sw.ElapsedMilliseconds:000000} \t {(float)num / sw.ElapsedMilliseconds * 1000}Des/Sec \t {(float)sw.ElapsedMilliseconds / num}MS/Ser");
            long aMs = sw.ElapsedMilliseconds;


            sw.Restart();

            for (int i = 0; i < num; i++)
            {
                ms2.Position = 0;
                JsonSerializer.Deserialize<TestClass>(ms2);
            }
            Console.WriteLine($"J: {sw.ElapsedMilliseconds:000000} \t {(float)num / sw.ElapsedMilliseconds * 1000}Des/Sec \t {(float)sw.ElapsedMilliseconds / num}MS/Ser");
            long jMs = sw.ElapsedMilliseconds;

            Console.WriteLine($"VelDif(maior melhor A): {(float)jMs / aMs}X Dif: {jMs - aMs}");


        }
    }


}