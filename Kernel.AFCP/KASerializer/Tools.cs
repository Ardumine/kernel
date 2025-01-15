
using System;
using System.IO;

namespace Kernel.AFCP.KASerializer;

public static class Tools
{
    public static unsafe byte[] GetBytes(int value)
    {
        byte[] buffer = new byte[4];
        fixed (byte* bufferRef = buffer)
        {
            *(int*)bufferRef = value;
        }
        return buffer;
    }
    public static unsafe byte[] GetBytes(uint value)
    {
        byte[] buffer = new byte[4];
        fixed (byte* bufferRef = buffer)
        {
            *(uint*)bufferRef = value;
        }
        return buffer;
    }
    public static unsafe byte[] GetBytes(ushort value)
    {
        byte[] buffer = new byte[2];
        fixed (byte* bufferRef = buffer)
        {
            *(ushort*)bufferRef = value;
        }
        return buffer;
    }
    public static unsafe byte[] GetBytes(float value)
    {
        byte[] buffer = new byte[4];
        fixed (byte* bufferRef = buffer)
        {
            *(float*)bufferRef = value;
        }
        return buffer;
    }
    public static unsafe byte[] GetBytes(double value)
    {
        byte[] buffer = new byte[8];
        fixed (byte* bufferRef = buffer)
        {
            *(double*)bufferRef = value;
        }
        return buffer;
    }
    public static unsafe byte[] GetBytes(decimal value)
    {
        byte[] buffer = new byte[16];
        fixed (byte* bufferRef = buffer)
        {
            *(decimal*)bufferRef = value;
        }
        return buffer;
    }



    /// <summary>
    /// Reads 4 bytes
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static unsafe float GetFloat(byte[] bytes)
    {
        float val;
        fixed (byte* bufferRef = bytes)
        {
            val = *(float*)bufferRef;
        }
        return val;
    }

    /// <summary>
    /// Reads 8 bytes
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static unsafe double GetDouble(byte[] bytes)
    {
        double val;
        fixed (byte* bufferRef = bytes)
        {
            val = *(double*)bufferRef;
        }
        return val;
    }
    //https://stackoverflow.com/questions/4326125/faster-way-to-convert-byte-array-to-int

    public static int GetInt(byte[] b)
    {
        return b[0] | (b[1] << 8) | (b[2] << 16) | (b[3] << 24);
    }

    public static uint GetUInt(byte[] b)
    {
        return (uint)(b[0] | (b[1] << 8) | (b[2] << 16) | (b[3] << 24));
    }

    public static uint GetUShort(byte[] b)
    {
        return (ushort)(b[0] | (b[1] << 8));
    }

    //https://stackoverflow.com/questions/11654562/how-to-convert-byte-array-to-string
    //https://stackoverflow.com/questions/16072709/converting-string-to-byte-array-in-c-sharp
    //https://stackoverflow.com/questions/20273556/fast-string-to-byte-conversion
    public static unsafe byte[] GetBytes(string? str)
    {
        if (str == null)
        {
            return [];
        }
        int arrLen = str.Length * sizeof(char);
        byte[] bytes = new byte[arrLen];

        fixed (void* ptrBytes = bytes)
        fixed (void* ptrStr = str)
        {
            System.Runtime.InteropServices.Marshal.Copy(new IntPtr(ptrStr), bytes, 0, bytes.Length);
            //Buffer.MemoryCopy(ptrStr, ptrBytes, arrLen, arrLen);
        }
        return bytes;

        //byte[] bytes = new byte[str.Length * sizeof(char)];
        //Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
        //return bytes;
    }

    public static unsafe string GetString(Span<byte> bytes)//Unicode
    {
        int len = bytes.Length;
        fixed (byte* bptr = bytes)
        {
            char* cptr = (char*)bptr;
            return new string(cptr, 0, len / 2);
        }

        //char[] chars = new char[bytes.Length / sizeof(char)];
        //Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
        //return new string(chars);
    }
    public static void Clear(this MemoryStream source)
    {
        byte[] buffer = source.GetBuffer();
        Array.Clear(buffer, 0, buffer.Length);
        source.Position = 0;
        source.SetLength(0);
    }

}