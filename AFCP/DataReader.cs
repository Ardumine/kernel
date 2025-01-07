using System.Text;

namespace AFCP.DataTreatment;
public class DataReader
{
    public Stream stream;
    public DataReader(Stream _stream)
    {
        stream = _stream;
    }
    public bool ReadBool()
    {
        return ReadByteArrRaw(1)[0] == 1;
    }
    public byte ReadByte()
    {
        return ReadByteArrRaw(1)[0];
    }


    public Guid ReadGuid()
    {
        return new Guid(ReadByteArrRaw(16));
    }


    public uint ReadUint()
    {
        byte[] arr = new byte[4];
        stream.ReadExactly(arr);

        return BitConverter.ToUInt32(arr);
    }
    public int ReadInt()
    {
        byte[] lenArr = new byte[4];
        stream.ReadExactly(lenArr);

        return BitConverter.ToInt32(lenArr);
    }


    private byte[] ReadByteArrRaw(int len)
    {
        byte[] dados = new byte[len];

        int lenDadosRecebido = 0;
        while (lenDadosRecebido != len)
        {
            lenDadosRecebido += stream.Read(dados, lenDadosRecebido, (int)(len - lenDadosRecebido));//int32 = 4 bytes
        }

        return dados;
    }
    //https://stackoverflow.com/questions/20273556/fast-string-to-byte-conversion
    public unsafe string ReadStringA()
    {
        string txt = "";
        int len = 0;
        byte[] data = ReadByteArr();
        fixed (byte* bptr = data)
        {
            char* cptr = (char*)(bptr + 0);
            txt = new string(cptr, 0, len / 2);
        }
        return txt;
    }

   public unsafe string ReadString()
    {
        byte[] data = ReadByteArr();
     
        return Encoding.UTF8.GetString(data);
    }

    public byte[] ReadByteArr()
    {
        byte[] lenArr = new byte[4];
        stream.ReadExactly(lenArr);
        uint TamDadosPRec = BitConverter.ToUInt32(lenArr);

        byte[] dados = new byte[TamDadosPRec];

        int lenDadosRecebido = 0;
        while (lenDadosRecebido != TamDadosPRec)
        {
            lenDadosRecebido += stream.Read(dados, lenDadosRecebido, (int)(TamDadosPRec - lenDadosRecebido));//int32 = 4 bytes
        }

        return dados;
    }

    public MemoryStream ReadByteArrAsStream()
    {
        MemoryStream ms = new();
        byte[] lenArr = new byte[4];
        stream.ReadExactly(lenArr);
        uint TamDadosPRec = BitConverter.ToUInt32(lenArr);

        byte[] dados = new byte[TamDadosPRec];

        int lenDadosRecebido = 0;
        while (lenDadosRecebido != TamDadosPRec)
        {
            lenDadosRecebido += stream.Read(dados, lenDadosRecebido, (int)(TamDadosPRec - lenDadosRecebido));//int32 = 4 bytes
            ms.Write(dados, lenDadosRecebido, (int)(TamDadosPRec - lenDadosRecebido));
        }

        return ms;
    }

}