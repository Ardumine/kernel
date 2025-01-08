using System.Runtime.InteropServices;

namespace Kernel.AFCP;
public class Converter
{
    public static ushort ByteArrayToUshort(byte[] b, int offset = 0)
    {
        return (ushort)(b[0 + offset] | (b[1 + offset] << 8));//| (b[2] << 16) | (b[3] << 24)
    }
    public static unsafe byte[] UShortToByteArray(ushort a)//65,535 65 kilobytes
    {
        byte[] arrBytes = new byte[2];

        Marshal.Copy((IntPtr)(byte*)&a, arrBytes, 0, 2);
        return arrBytes;
    }




    public unsafe static uint ByteArrayToUInt(byte[] b, int offset = 0)
    {
        return (uint)(b[0 + offset] | (b[1 + offset] << 8) | (b[2 + offset] << 16) | (b[3 + offset] << 24));
    }
    public static unsafe byte[] UIntToByteArray(uint a)// 4,294,967,295 4 GigaBytes
    {
        byte[] arrBytes = new byte[4];
        Marshal.Copy((IntPtr)(byte*)&a, arrBytes, 0, 4);
        return arrBytes;
    }
}