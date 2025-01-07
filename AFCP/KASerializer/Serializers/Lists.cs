using System.Collections;
using System.Runtime.InteropServices;

namespace AFCP.KASerializer.Serializers.Lists;


public class ListSerializer : KADataSerializer
{
    public Type type { get; set; } = typeof(List<>);
    public Type DataType { get; set; } = null!;
    public bool SpecialDataType = true;

    public IList CreateList(Type myType)
    {
        Type genericListType = typeof(List<>).MakeGenericType(myType);
        return (IList)Activator.CreateInstance(genericListType)!;
    }

    public object Deserialize(Stream stream, KASerializer serializer, Type type)
    {

        var list = CreateList(DataType);

        var len = serializer.Deserialize<uint>(stream);
        for (int i = 0; i < len; i++)
        {
            list.Add(serializer.Deserialize(stream, DataType));
        }
        return list;
    }

    public void Serialize(object obj, Stream stream, KASerializer serializer)
    {
        if (!SpecialDataType)
        {
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
                serializer.Serialize(list[i]!, stream, DataType);
            }
        }


    }
}

public class ListSerializer<Ta> : KADataSerializer where Ta : struct
{
    public Type type { get; set; } = typeof(List<Ta>);
    public bool SpecialDataType { get; set; } = false;

    public object Deserialize(Stream stream, KASerializer serializer, Type type)
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



    public void Serialize(object obj, Stream stream, KASerializer serializer)
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
                serializer.Serialize(list[i]!, stream, typeof(Ta));
            }
        }


    }
}
