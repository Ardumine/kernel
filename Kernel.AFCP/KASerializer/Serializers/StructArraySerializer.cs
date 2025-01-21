using System;
using System.IO;
using System.Collections.Concurrent;
using System.Reflection;
using Kernel.AFCP.KASerializer.Atributes;
using Kernel.AFCP.FastMethod;

namespace Kernel.AFCP.KASerializer.Serializers.StructArraySerializer;
public class StructArraySerializer
{
    private ConcurrentDictionary<Type, FastMethodInfo> ArraySerializers = new();
    private ConcurrentDictionary<Type, FastMethodInfo> ArrayDeserializers = new();

    public void Serialize(object obj, Type elementType, Stream stream, KASerializer serializer, bool LongLength)
    {
        FastMethodInfo met;
        if (!ArraySerializers.TryGetValue(elementType, out met!))
        {
            if (!UnmanagedType.IsUnManaged(elementType))
            {
                throw new Exception($"The type {elementType} is unmanaged! Make sure when you use {nameof(FastParseStructArray)} the type of the array is unmanaged!");
            }
            MethodInfo method = GetType().GetMethod(nameof(StructArrayToByteArray), BindingFlags.NonPublic | BindingFlags.Static)!;
            MethodInfo generic = method.MakeGenericMethod(elementType!);

            met = new(generic);
            ArraySerializers[elementType] = met;
        }
            serializer.WriteByteArray((byte[])met.Invoke(null!, obj)!,LongLength, stream);

 
    }
    public object Deserialize(Type elementType, Stream stream, KASerializer serializer, bool LongLength)
    {
        FastMethodInfo met;
        if (!ArrayDeserializers.TryGetValue(elementType, out met!))
        {
            if (!UnmanagedType.IsUnManaged(elementType))
            {
                throw new Exception($"The type {elementType} is unmanaged! Make sure when you use {nameof(FastParseStructArray)} the type of the array is unmanaged!");
            }

            MethodInfo method = GetType().GetMethod(nameof(ByteArrayToStructArray), BindingFlags.NonPublic | BindingFlags.Static)!;
            MethodInfo generic = method.MakeGenericMethod(elementType!);

            met = new(generic);
            ArrayDeserializers[elementType] = met;
        }

        byte[]     dados = serializer.ReadByteArray(LongLength, stream);

        return met.Invoke(null!, dados);

    }


    private static unsafe byte[] StructArrayToByteArray<T>(T[] array)where T: unmanaged
    {
        int objLen = sizeof(T);
        int arrLen = array.Length;

        int byteArrLen = objLen * arrLen;
        var byteArr = new byte[byteArrLen];

        fixed (void* structPtr = array)
        //fixed (void* bytesPtr = byteArr)
        {
            //Buffer.MemoryCopy(structPtr, bytesPtr, byteArrLen, byteArrLen);
            System.Runtime.InteropServices.Marshal.Copy(new IntPtr(structPtr), byteArr, 0, byteArrLen);

        }


        return byteArr;
    }

    private static unsafe T[] ByteArrayToStructArray<T>(byte[] byteArray) where T: unmanaged
    {
        int objLen = sizeof(T);
        int arrLen = byteArray.Length;

        int structArrLen = arrLen / objLen;
        T[] structArray = new T[structArrLen];

        fixed (void* structPtr = structArray)
        //fixed (void* bytesPtr = byteArray)
        {
            //Buffer.MemoryCopy(bytesPtr, structPtr, structArrLen, structArrLen);
            System.Runtime.InteropServices.Marshal.Copy(byteArray, 0, (nint)structPtr, structArrLen);

        }


        return structArray;
    }


}