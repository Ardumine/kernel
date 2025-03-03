using System.Numerics;
using Ardumine.Modules.YDLidar;
using BaseSLAM;
using HectorSLAM.Main;
using Kernel.Modules.Base;

namespace Ardumine.Modules.SLAM.Hector;
public class HectorSLAMImplement : BaseImplement, IHectorSLAM
{
    private IDataChannelInterface<Vector3>? posChannel;
    private IDataChannelInterface<byte[]>? mapChannel;
    private IDataChannelInterface<LidarPoint[]>? lidarDataChannel;

    HectorSLAMProcessor? hectorSlam;
    //int tam = 1000;//90 //Medido em cm. 1m = 100cm 1dm = 100mm

    /// <summary>
    /// MapSize: sizeXsize
    /// In meters
    /// </summary>
    public readonly int SizeMap = 2;

    public override void Prepare()
    {
        InitiateChannel<Vector3>("#slamPos");
        InitiateChannel<byte[]>("#SLAMmap");


        posChannel = GetChannelInterface<Vector3>("#slamPos");
        mapChannel = GetChannelInterface<byte[]>("#SLAMmap");
        lidarDataChannel = GetChannelInterface<LidarPoint[]>("#lidarData");

        //int hsMapSide = tam;//tam / hsMapSide -> 10 
        //int hsMapSide = 500;
        var startPose = new Vector3(SizeMap / 2, SizeMap / 2, 0.0f);

        int hsMapSide = SizeMap * 10;
        hectorSlam = new HectorSLAMProcessor(SizeMap / hsMapSide, new System.Drawing.Point(hsMapSide, hsMapSide), startPose, 4, 4)
        {
            MinDistanceDiffForMapUpdate = 0.1f,
            MinAngleDiffForMapUpdate = MathEx.DegToRad(8),//15,       7
        };

        hectorSlam.MapRep.Maps[0].EstimateIterations = 24;
        hectorSlam.MapRep.Maps[1].EstimateIterations = 4;
        hectorSlam.MapRep.Maps[2].EstimateIterations = 4;
        hectorSlam.MapRep.Maps[3].EstimateIterations = 4;




    }
    public bool ThreadsRunning { get; set; }
    private DateTime TimeSinceLastUpdate { get; set; }

    public override void Start()
    {
        ThreadsRunning = true;
        lidarDataChannel?.AddChangeEvent(OnLidarData!);

        var thFake = new Thread(() =>
        {
            while (ThreadsRunning)
            {
                Logger.LogI("Created fake data!");
                posChannel?.Set(new Vector3(2, 5, 90));
                Thread.Sleep(1000);
            }
        })
        {
            Name = "Thread HectorSLAM fake pos"
        };
        thFake.Start();

    }

    private void OnLidarData(LidarPoint[] points)
    {
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
                var map = hectorSlam?.MapRep.Maps[0];
                if(map == null) return;

                byte[] FinalData = new byte[map.mapArray.Length];

                for (int i = 0; i < FinalData.Length; i++)
                {
                    FinalData[i] = (byte)(127 - MathF.Sign(map.mapArray[i].Value) * 127);
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
        ThreadsRunning = false;

        lidarDataChannel?.RemoveChangeEvent(OnLidarData!);
        hectorSlam?.Dispose();
    }

}