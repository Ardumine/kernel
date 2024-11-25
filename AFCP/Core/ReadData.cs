public class DataReadFromRemote
{
    public DataReadFromRemote(ushort dataType, byte[] data){
        this.DataType = dataType;
        this.Data = data;
    }
    public ushort DataType { get; set; }
    public byte[] Data { get; set; }
}