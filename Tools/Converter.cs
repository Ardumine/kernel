using System.Runtime.InteropServices;

public class Converter
{
    public static ushort ByteArrayToUshort(byte[] b, int offset = 0)
    {
        return (ushort)(b[0 +offset] | (b[1 + offset] << 8));//| (b[2] << 16) | (b[3] << 24)
    }
    public static unsafe byte[] UShortToByteArray(ushort a)
    {
        byte[] arrBytes = new byte[2];

        Marshal.Copy((IntPtr)(byte*)&a, arrBytes, 0, 2);
        return arrBytes;
    }
}