using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Text;
using BaseSLAM;
using HectorSLAM.Map;
using HectorSLAM.Matcher;
using System.Xml.Serialization;
using System.IO;
using Newtonsoft.Json;

namespace HectorSLAM.Main
{
    /// <summary>
    /// Hector SLAM processor
    /// </summary>
    public class HectorSLAMProcessor : IDisposable
    {
        private readonly Vector3 startPose;
        private ScanMatcher scanMatcher;

        /// <summary>
        /// Multi-level map
        /// </summary>
        public MapRepMultiMap MapRep { get; set; }

        /// <summary>
        /// Last map update pose
        /// </summary>
        public Vector3 LastMapUpdatePose { get; set; }

        /// <summary>
        /// Last scan match pose
        /// </summary>
        public Vector3 MatchPose { get; set; }

        /// <summary>
        /// Average match timing in milliseconds
        /// </summary>
        public float MatchTiming { get; set; }

        /// <summary>
        /// Average map update timing in milliseconds
        /// </summary>
        public float UpdateTiming { get; set; }

        /// <summary>
        /// Distance difference (in meters) required for map update
        /// </summary>
        public float MinDistanceDiffForMapUpdate { get; set; } = 0.3f;

        /// <summary>
        /// Angular difference (in radians) required for map update
        /// </summary>
        public float MinAngleDiffForMapUpdate { get; set; } = 0.13f;
        public int numThreads_;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mapResolution">Meters per pixel</param>
        /// <param name="mapSize">Map size in pixels</param>
        /// <param name="startPose">Start pose (X and Y in meters, Z in degrees)</param>
        /// <param name="numDepth">Number of maps</param>
        /// <param name="numThreads">Number of processing threads</param>
        public HectorSLAMProcessor(float mapResolution, Point mapSize, Vector3 startPose, int numDepth, int numThreads)
        {
            this.startPose = startPose;

            MapRep = new MapRepMultiMap(mapResolution, mapSize, numDepth, Vector2.Zero);

            numThreads_ = numThreads;
            scanMatcher = new ScanMatcher(numThreads_);

            // Set initial poses
            MatchPose = startPose;
            LastMapUpdatePose = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        }

        /// <summary>
        /// Update map with new scan data and search for the best pose estimate.
        /// </summary>
        /// <param name="scan">Scanned cloud points</param>
        /// <param name="poseHintWorld">Pose hint</param>
        /// <param name="mapWithoutMatching">Map without matching ?</param>
        /// <returns>true if map was updated, false if not</returns>
        public bool Update(ScanCloud scan, Vector3 poseHintWorld, bool mapWithoutMatching = false)
        {
            // Do position matching or not ?
            if (!mapWithoutMatching)
            {
                // Match and measure the performance
                var watch = Stopwatch.StartNew();
                MatchPose = scanMatcher.MatchData(MapRep, scan, poseHintWorld);

                // Calculate average timing
                MatchTiming = (3.0f * MatchTiming + (float)watch.Elapsed.TotalMilliseconds) / 4.0f;
            }
            else
            {
                MatchPose = poseHintWorld;
            }

            // Update map(s) when:
            //    Map hasn't been updated yet
            //    Position or rotation has changed significantly.
            //    Mapping is requested.
            if (Vector2.DistanceSquared(MatchPose.ToVector2(), LastMapUpdatePose.ToVector2()) > MinDistanceDiffForMapUpdate.Sqr() ||
                (MathEx.DegDiff(MatchPose.Z, LastMapUpdatePose.Z) > MinAngleDiffForMapUpdate) ||
                mapWithoutMatching)
            {
                var watch = Stopwatch.StartNew();
                MapRep.UpdateByScan(scan, MatchPose);

                // Calculate average timing
                UpdateTiming = (3.0f * UpdateTiming + (float)watch.Elapsed.TotalMilliseconds) / 4.0f;

                // Remember update pose
                LastMapUpdatePose = MatchPose;

                // Notify about update
                return true;
            }

            return false;
        }
        public void Load(string path)
        {//OLD OLD OLD code. but works. Happy I wrote it.

            //XmlSerializer formatter = new XmlSerializer(typeof(HectorSLAMProcessor));
            //FileStream aFile = new FileStream(path, FileMode.Open);
            //byte[] buffer = new byte[aFile.Length];
            //aFile.Read(buffer, 0, (int)aFile.Length);
            //MemoryStream stream = new MemoryStream(buffer);
            //JsonSerializer serializer = new();
            var data = JsonConvert.DeserializeObject<HectorSaveData>(File.ReadAllText(path))!;
            MapRep.SetMaps(data.Maps);
            LastMapUpdatePose = data.LastMapUpdatePose;
            MatchPose = data.MatchPose;
        }

        class HectorSaveData
        {
            public required List<LogOddsCell[]> Maps { get; set; }
            public Vector3 MatchPose { get; set; }
            public Vector3 LastMapUpdatePose { get; set; }
        }
        public void Save(string path)
        {
            //FileStream outFile = File.Create(path);
            var data = new HectorSaveData
            {
                Maps = MapRep.GetMaps(),
                LastMapUpdatePose = LastMapUpdatePose,
                MatchPose = MatchPose
            };

            //XmlSerializer formatter = new XmlSerializer(typeof(HectorSLAMProcessor));
            //formatter.Serialize(outFile, processor);
            File.WriteAllText(path, JsonConvert.SerializeObject(data));
        }

        /// <summary>
        /// Reset
        /// </summary>
        public void Reset()
        {
            MapRep.Reset();

            // Set initial poses
            MatchPose = startPose;
            LastMapUpdatePose = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        }

        /// <summary>
        /// Dispose function
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Internal disposing function.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                scanMatcher.Dispose();
            }
        }
    }
}
