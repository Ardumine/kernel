using System.Text;
using Ardumine.AFCP.Core.Client.RawComProt;
using Ardumine.AFCP.Core.Server;

public class DisconnectSystem : BaseSystem
{
    public override void Start()
    {
        Server.OnDataRec += OnDataRec;
    }
    private void OnDataRec(object sender, OnDataRecArgs data){
        if(data.Data.DataType == MsgTypes.Disconnect){
            data.Client.Close();
            Server.DisconnectClientAuth(data.Client);
        }
    }
}