using System.Net.Sockets;
using System.Text;

namespace AFCP.DataTreatment;
public class DataWritter
{
    public MemoryStream ms;

    public DataWritter()
    {
        ms = new MemoryStream();
    }
    public void Write(bool b)
    {
        ms.WriteByte(b ? (byte)1 : (byte)0);
    }

    public void Write(byte b)
    {
        ms.WriteByte(b);
    }
    public void Write(Guid guid)
    {
        ms.Write(guid.ToByteArray());
    }
    public void Write(uint data)
    {
        ms.Write(BitConverter.GetBytes(data));
    }
    public void Write(int data)
    {
        ms.Write(BitConverter.GetBytes(data));
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
        WriteByteArr(Encoding.UTF8.GetBytes(data));
    }

    public void WriteByteArr(byte[] bytes)
    {
        ms.Write(BitConverter.GetBytes((uint)bytes.Length));
        ms.Write(bytes);
    }

    public void WriteByteArr(Span<byte> bytes)
    {
        ms.Write(BitConverter.GetBytes((uint)bytes.Length));
        ms.Write(bytes);
    }


    public void Copy(NetworkStream stream)
    {
        //stream.Write(ms.ToArray());
        ms.Position = 0;
        byte[] buffer = new byte[32768];//https://stackoverflow.com/questions/230128/how-do-i-copy-the-contents-of-one-stream-to-another
        int read;
        while ((read = ms.Read(buffer, 0, buffer.Length)) > 0)
        {
            stream.Write(buffer, 0, read);
        }


    }

}