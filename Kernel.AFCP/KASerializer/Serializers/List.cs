using System.Collections;

namespace Kernel.AFCP.KASerializer.Serializers.ListSerliazer;
public class ListSerializer
{


    public IList CreateList(Type myType)
    {
        Type genericListType = typeof(List<>).MakeGenericType(myType);
        return (IList)Activator.CreateInstance(genericListType)!;
    }

    //https://stackoverflow.com/questions/33292204/copying-a-system-guid-to-byte-without-allocating
    public unsafe void Serialize(object obj, Stream stream, KASerializer serializer, KAType prop)
    {
        if (prop.IsArray)
        {
            var list = (Array)obj;

            stream.Write(Tools.GetBytes((uint)list.Length));

            for (int i = 0; i < list.Length; i++)
            {
                var it = list.GetValue(i)!;
                serializer.Serialize(it, stream, prop.ElementType!, serializer.GetKAType(prop.ElementType!, prop.IsLongLength,  prop.CanBeDerived));
            }
        }
        else
        {
            IList list = (IList)obj;

            stream.Write(Tools.GetBytes((uint)list.Count));

            for (int i = 0; i < list.Count; i++)
            {
                var it = list[i]!;
                serializer.Serialize(it, stream, prop.ElementType!, serializer.GetKAType(prop.ElementType!, prop.IsLongLength, prop.CanBeDerived));
            }
        }

    }
    public object Deserialize(Stream stream, KASerializer serializer, KAType prop)
    {
        if (prop.IsArray)
        {
            var len = Tools.GetUInt(serializer.ReadByteArray(4, stream));
            var array = Array.CreateInstance(prop.ElementType!, len);

            for (int i = 0; i < len; i++)
            {
                array.SetValue(serializer.Deserialize(stream, prop.ElementType!, serializer.GetKAType(prop.ElementType!, prop.IsLongLength, prop.CanBeDerived)), i);
            }
            return array;
        }


        else
        {
            var list = CreateList(prop.ElementType!);

            var len = Tools.GetUInt(serializer.ReadByteArray(4, stream));

            for (int i = 0; i < len; i++)
            {
                list.Add(serializer.Deserialize(stream, prop.ElementType!, serializer.GetKAType(prop.ElementType!, prop.IsLongLength, prop.CanBeDerived)));
            }
            return list;

        }

    }


}

