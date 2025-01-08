
using System.Numerics;
using Kernel.AFCP;
using Ardumine.Modules.YDLidar;
using BaseSLAM;
using HectorSLAM.Main;

namespace Ardumine.Modules.SLAM.Hector;
public class HectorSLAMImplement : Kernel.Modules.Base.BaseImplement, IHectorSLAM
{
    DataChannelInterface<Vector3>? posChannel;
    DataChannelInterface<byte[]>? mapChannel;
    DataChannelInterface<LidarPoint[]>? lidarDataChannel;

    HectorSLAMProcessor? hectorSlam;
    int tam = 1000;//90 //Medido em cm. 1m = 100cm 1dm = 100mm


    public override void Prepare()
    {
        InitiateChannel<Vector3>("#slamPos");
        InitiateChannel<byte[]>("#SLAMmap");


        posChannel = GetChannelInterface<Vector3>("#slamPos");
        mapChannel = GetChannelInterface<byte[]>("#SLAMmap");
        lidarDataChannel = GetChannelInterface<LidarPoint[]>("#lidarData");

        int hsMapSide = tam;//tam / hsMapSide -> 10 

        var startPose = new Vector3(tam / 2, tam / 2, 0.0f);


        hectorSlam = new HectorSLAMProcessor(tam / hsMapSide, new System.Drawing.Point(hsMapSide, hsMapSide), startPose, 3, 4)
        {
            MinDistanceDiffForMapUpdate = 5f,//0.04//2
            MinAngleDiffForMapUpdate = MathEx.DegToRad(4),//15,       7
        };

        hectorSlam.MapRep.Maps[0].EstimateIterations = 17;//7
        hectorSlam.MapRep.Maps[1].EstimateIterations = 6;
        hectorSlam.MapRep.Maps[2].EstimateIterations = 3;



    }
    public bool Running { get; set; }
    private DateTime TimeSinceLastUpdate { get; set; }

    public override void Start()
    {
        Running = true;
        lidarDataChannel?.AddEvent(OnLidarData!);

        var thFake = new Thread(() =>
         {
             while (Running)
             {
                 posChannel?.Set(new Vector3(2, 5, 90));
                 Thread.Sleep(1000);
             }
         });

    }

    private void OnLidarData(LidarPoint[] points)
    {
        //var points = (LidarPoint[])e!;
        ScanCloud scanCloud = new ScanCloud()
        {
            //  Pose = hectorSlam.MatchPose
        };

        for (int iRay = 0; iRay < points.Length; iRay++)
        {
            var ray = points[iRay];
            scanCloud.Points.Add(new Vector2()
            {
                X = ray.Distance * MathF.Cos(ray.Angle),
                Y = ray.Distance * MathF.Sin(ray.Angle),
            });
        }

        hectorSlam?.Update(scanCloud, hectorSlam.MatchPose);
        posChannel?.Set(hectorSlam!.MatchPose);

        //Logger.LogI("Hector pose" + hectorSlam?.MatchPose);

        if ((DateTime.Now - TimeSinceLastUpdate).Seconds > 5)
        {
            new Thread(() =>
            {
                byte[] FinalData = new byte[tam * tam];

                var dados_mapa = hectorSlam?.MapRep.Maps[0].mapArray!;
                for (int i = 0; i < dados_mapa.Length; i++)
                {
                    FinalData[i] = (byte)(127 - MathF.Sign(dados_mapa[i].Value) * 127);
                }
                mapChannel?.Set(FinalData);
            })
            {
                Name = "Hector data send"
            }.Start();
            TimeSinceLastUpdate = DateTime.Now;
        }

    }
    public override void EndStop()
    {
        Running = false;

        lidarDataChannel?.RemoveEvent(OnLidarData!);
        hectorSlam?.Dispose();

    }

}