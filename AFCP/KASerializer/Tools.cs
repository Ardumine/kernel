using System.Text;
namespace AFCP.KASerializer;
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
    public static unsafe byte[] GetBytes(string ?str)
    {
        if (str == null)
        {
            return [];
        }
        byte[] bytes = new byte[str.Length * sizeof(char)];

        fixed (void* ptr = str)
        {
            System.Runtime.InteropServices.Marshal.Copy(new IntPtr(ptr), bytes, 0, bytes.Length);
        }
        return bytes;

        //byte[] bytes = new byte[str.Length * sizeof(char)];
        //Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
        //return bytes;
    }

    public static unsafe string GetString(byte[] bytes)//Unicode
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