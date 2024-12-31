
using AFCP;
using YDLidarSDK;

namespace Ardumine.Modules.YDLidar;
public class YDLidarImplement : Kernel.Modules.Base.BaseImplement, IYDLidar
{
    public CYdLidar lidarClass = new CYdLidar();
    private bool Run { get; set; }

    private DataChannelInterface<LidarPoint[]>? ChannelLidarData;

    public override void Prepare()
    {
        InitiateChannel<LidarPoint[]>("#lidarData");
        ChannelLidarData = GetChannelInterface<LidarPoint[]>("#lidarData");
    }

    public override void Start()
    {
        var config = SelfMod.Config!;
        Logger.LogI("Starting lidar...");

        ConnectLidar(((System.Text.Json.JsonElement)config["Port"]).ToString());
        Run = true;
        
        var thread = new Thread(Loop){
            Name = "YDLidar loop"
        };
        thread.Start();
    }


    public void ConnectLidar(string porta)
    {
        YDLidarSDK.YDLidarSDK.os_init();
        //string port = "/dev/ttyUSB0";//COM14
        int baudrate = 230400;


        bool isSingleChannel = false;

        float frequency = 10.0f;

        int optname = (int)LidarProperty.LidarPropSerialPort;
        lidarClass.setlidaropt(optname, porta);


        //////////////////////int property/////////////////
        /// lidar baudrate
        lidarClass.setlidaropt((int)LidarProperty.LidarPropSerialBaudrate, baudrate);
        /// tof lidar
        int optval = (int)LidarTypeID.TYPE_TRIANGLE;
        lidarClass.setlidaropt((int)LidarProperty.LidarPropLidarType, optval);
        /// device type
        //optval = LidarProperty.ser;
        //lidarClass.setlidaropt((int)LidarProperty.LidarPropDeviceType, optval);
        /// sample rate
        optval = isSingleChannel ? 3 : 4;
        lidarClass.setlidaropt((int)LidarProperty.LidarPropSampleRate, optval);
        /// abnormal count
        optval = 4;
        lidarClass.setlidaropt((int)LidarProperty.LidarPropAbnormalCheckCount, optval);
        /// Intenstiy bit count
        optval = 8;
        lidarClass.setlidaropt((int)LidarProperty.LidarPropIntenstiyBit, optval);

        //////////////////////bool property/////////////////
        /// fixed angle resolution
        bool b_optvalue = true;
        lidarClass.setlidaropt((int)LidarProperty.LidarPropFixedResolution, b_optvalue);
        b_optvalue = false;
        /// rotate 180
        lidarClass.setlidaropt((int)LidarProperty.LidarPropReversion, b_optvalue);
        /// Counterclockwise
        lidarClass.setlidaropt((int)LidarProperty.LidarPropInverted, b_optvalue);
        b_optvalue = true;
        lidarClass.setlidaropt((int)LidarProperty.LidarPropAutoReconnect, b_optvalue);
        /// one-way communication
        lidarClass.setlidaropt((int)LidarProperty.LidarPropSingleChannel, isSingleChannel);
        /// intensity
        b_optvalue = true;
        lidarClass.setlidaropt((int)LidarProperty.LidarPropIntenstiy, b_optvalue);
        /// Motor DTR
        b_optvalue = false;
        lidarClass.setlidaropt((int)LidarProperty.LidarPropSupportMotorDtrCtrl, b_optvalue);
        /// HeartBeat
        b_optvalue = false;
        lidarClass.setlidaropt((int)LidarProperty.LidarPropSupportHeartBeat, b_optvalue);

        //////////////////////float property/////////////////
        /// unit: °
        float f_optvalue = 180.0f;
        lidarClass.setlidaropt((int)LidarProperty.LidarPropMaxAngle, f_optvalue);
        f_optvalue = -180.0f;
        lidarClass.setlidaropt((int)LidarProperty.LidarPropMinAngle, f_optvalue);
        /// unit: m
        f_optvalue = 64.0f;
        lidarClass.setlidaropt((int)LidarProperty.LidarPropMaxRange, f_optvalue);
        f_optvalue = 0.01f;
        lidarClass.setlidaropt((int)LidarProperty.LidarPropMinRange, f_optvalue);
        /// unit: Hz
        lidarClass.setlidaropt((int)LidarProperty.LidarPropScanFrequency, frequency);

        Run = lidarClass.initialize();
        if (Run)
        {
            Run = lidarClass.turnOn();
        }
        else
        {
            Logger.LogW("error:" + lidarClass.DescribeError());
        }

    }
    public override void EndStop()
    {
        Run = false;
        Thread.Sleep(100);
        lidarClass.disconnecting();
    }

    void Loop()
    {
        LaserScan scan = new LaserScan();
        while (Run)
        {
            if (lidarClass.doProcessSimple(scan))
            {
                ProcessScan(scan);
            }
        }
        if (Run)
        {
            Logger.LogW("The YDLidar loop stopped and there was no instruction to stop.");
        }
    }
    void ProcessScan(LaserScan scan)
    {
        var rawScanPoints = scan.points;

        List<LidarPoint> points = new();
        for (int i = 0; i < rawScanPoints.Count; i++)
        {
            float angle = rawScanPoints[i].angle;

            float hit = rawScanPoints[i].range * 100; //* 100;//1000 metros   100 dm   10 cm//em yd é em metros //em cm
                                                      //ang_max = Math.Max(ang_max, angle);


            if (!(float.IsNaN(angle) || float.IsNaN(hit) || hit == 0))
            {
                points.Add(new()
                {
                    Angle = angle,
                    Distance = hit
                });
            }

        }
        ChannelLidarData?.Set(points.ToArray());
    }


}