using System.Collections;
using System.Runtime.InteropServices;

namespace Kernel.AFCP.KASerializer.Serializers.Lists;


public class ListSerializer : KADataSerializer
{
    public Type type { get; set; } = typeof(List<>);
    public Type DataType { get; set; } = null!;

    public IList CreateList(Type myType)
    {
        Type genericListType = typeof(List<>).MakeGenericType(myType);
        return (IList)Activator.CreateInstance(genericListType)!;
    }

    public object Deserialize(Stream stream, KASerializer serializer, Type type, KAProperty prop)
    {
        var list = CreateList(DataType);

        var len = serializer.Deserialize<uint>(stream);
        for (int i = 0; i < len; i++)
        {
            list.Add(serializer.Deserialize(stream, DataType,  new KAProperty(){
                CanChangeType = prop.CanChangeType,
            }));
        }
        return list;
    }

    public void Serialize(object obj, Stream stream, KASerializer serializer, KAProperty prop)
    {
        var list = (IList)obj;
        stream.Write(Tools.GetBytes((uint)list.Count));
        //var t = list.GetType().GetGenericArguments()[0];
        //var t =  list.GetType().GetGenericTypeDefinition();
        ///Console.WriteLine(t + "   " + DataType);
        for (int i = 0; i < list.Count; i++)
        {
            var it = list[i]!;
            serializer.Serialize(it, stream, DataType, new KAProperty(){
                CanChangeType = prop.CanChangeType,
            });
        }


    }
}

public class ListSerializer<Ta> : KADataSerializer where Ta : struct
{
    public Type type { get; set; } = typeof(List<Ta>);
    public bool SpecialDataType { get; set; } = false;

    public object Deserialize(Stream stream, KASerializer serializer, Type type, KAProperty prop)
    {

        // var list = new List<Ta>();
        var recData = serializer.DeserializeByteArray4(stream);

        var totalLen = recData.Length;
        int lenToParse = totalLen / Marshal.SizeOf(typeof(Ta));
        var data = new Ta[lenToParse];


        Buffer.BlockCopy(recData, 0, data, 0, lenToParse);

        //for (int i = 0; i < len; i++)
        //{
        //   list.Add((Ta)serializer.Deserialize(ms, typeof(Ta)));
        //}
        return data.ToList();
    }



    public void Serialize(object obj, Stream stream, KASerializer serializer, KAProperty prop)
    {
        if (!SpecialDataType)
        {

            var list = ((List<Ta>)obj).ToArray();


            int totalBytes = list.Length * Marshal.SizeOf(typeof(Ta));
            byte[] byteArray = new byte[totalBytes];

            Buffer.BlockCopy(list, 0, byteArray, 0, totalBytes);
            serializer.SerializeByteArray4(byteArray, stream);
        }
        else
        {
            var list = (IList)obj;

            stream.Write(Tools.GetBytes((uint)list.Count));
            //var t = list.GetType().GetGenericArguments()[0];
            //var t =  list.GetType().GetGenericTypeDefinition();
            ///Console.WriteLine(t + "   " + DataType);
            for (int i = 0; i < list.Count; i++)
            {
                serializer.Serialize(list[i]!, stream, typeof(Ta), prop);
            }
        }


    }
}



public class ArraySerializer : KADataSerializer
{
    public Type type { get; set; } = typeof(object[]);
    public Type DataType { get; set; } = null!;
    public bool SpecialDataType = true;

    public Array CreateArray(Type myType, int len)
    {
        return Array.CreateInstance(myType, len);
    }

    public object Deserialize(Stream stream, KASerializer serializer, Type type, KAProperty prop)
    {
        var len = serializer.Deserialize<uint>(stream);

        var arr = CreateArray(DataType, (int)len);

        for (int i = 0; i < len; i++)
        {
            arr.SetValue(serializer.Deserialize(stream, DataType, new KAProperty(){
                CanChangeType = prop.CanChangeType,
            }), i);
        }
        return arr;
    }

    public void Serialize(object obj, Stream stream, KASerializer serializer, KAProperty prop)
    {

        var list = (Array)obj;

        stream.Write(Tools.GetBytes((uint)list.Length));
        //var t = list.GetType().GetGenericArguments()[0];
        //var t =  list.GetType().GetGenericTypeDefinition();
        ///Console.WriteLine(t + "   " + DataType);
        for (int i = 0; i < list.Length; i++)
        {
            serializer.Serialize(list.GetValue(i)!, stream, DataType, new KAProperty(){
                CanChangeType = prop.CanChangeType,
            });
        }

    }
}



public class ArraySerializer<Ta> : KADataSerializer where Ta : struct
{
    public Type type { get; set; } = typeof(Ta[]);
    public bool SpecialDataType { get; set; } = false;

    public object Deserialize(Stream stream, KASerializer serializer, Type type, KAProperty prop)
    {

        var recData = serializer.DeserializeByteArray4(stream);

        var totalLen = recData.Length;
        int lenToParse = totalLen / Marshal.SizeOf(typeof(Ta));
        var data = new Ta[lenToParse];


        Buffer.BlockCopy(recData, 0, data, 0, lenToParse);

        //for (int i = 0; i < len; i++)
        //{
        //   list.Add((Ta)serializer.Deserialize(ms, typeof(Ta)));
        //}
        return data;
    }



    public void Serialize(object obj, Stream stream, KASerializer serializer, KAProperty prop)
    {
        if (!SpecialDataType)
        {

            var list = ((List<Ta>)obj).ToArray();


            int totalBytes = list.Length * Marshal.SizeOf(typeof(Ta));
            byte[] byteArray = new byte[totalBytes];

            Buffer.BlockCopy(list, 0, byteArray, 0, totalBytes);
            serializer.SerializeByteArray4(byteArray, stream);
        }
        else
        {
            var list = (IList)obj;

            stream.Write(Tools.GetBytes((uint)list.Count));
            //var t = list.GetType().GetGenericArguments()[0];
            //var t =  list.GetType().GetGenericTypeDefinition();
            ///Console.WriteLine(t + "   " + DataType);
            for (int i = 0; i < list.Count; i++)
            {
                serializer.Serialize(list[i]!, stream, typeof(Ta), prop);
            }
        }


    }
}

