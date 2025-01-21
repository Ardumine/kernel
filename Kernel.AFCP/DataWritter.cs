using System.Net.Sockets;
using System.Text;
using Kernel.AFCP.KASerializer;

namespace Kernel.AFCP.DataTreatment;
public class DataWritter: IDisposable
{
    public MemoryStream memoryStream;

    public DataWritter()
    {
        memoryStream = new MemoryStream();
    }
    public void Write(bool b)
    {
        memoryStream.WriteByte(b ? (byte)1 : (byte)0);
    }

    public void Write(byte b)
    {
        memoryStream.WriteByte(b);
    }
    public void Write(Guid guid)
    {
        memoryStream.Write(guid.ToByteArray());
    }
    public void Write(uint data)
    {
        memoryStream.Write(Tools.GetBytes(data));
    }
    public void Write(int data)
    {
        memoryStream.Write(Tools.GetBytes(data));
    }


    string TreatString(string? data){
        return (data != null) ? data : "";
    }

    public unsafe void Writeeeee(string? data)
    {
        data = TreatString(data);
        int len = data.Length;
        byte[] dataArr = new byte[len];
        fixed (void* ptr = data)
        {
            System.Runtime.InteropServices.Marshal.Copy(new IntPtr(ptr), dataArr, 0, len);
        }
        WriteByteArr(dataArr);
    }
  public unsafe void Write(string? data)
    {
        data = TreatString(data);
        WriteByteArr(Tools.GetBytes(data));
    }

    public void WriteByteArr(byte[] bytes)
    {
        memoryStream.Write(Tools.GetBytes((uint)bytes.Length));
        memoryStream.Write(bytes);
    }

    public void WriteByteArr(Span<byte> bytes)
    {
        memoryStream.Write(Tools.GetBytes((uint)bytes.Length));
        memoryStream.Write(bytes);
    }


    public void Copy(NetworkStream stream)
    {
        //stream.Write(ms.ToArray());
        memoryStream.Position = 0;
        byte[] buffer = new byte[32768];//https://stackoverflow.com/questions/230128/how-do-i-copy-the-contents-of-one-stream-to-another
        int read;
        while ((read = memoryStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            stream.Write(buffer, 0, read);
        }


    }

    public void Dispose()
    {
        memoryStream.Dispose();
    }
}