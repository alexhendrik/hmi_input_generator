using HMI_InputGenerator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMI_InputGenerator
{
    public class Program
    {
        private static int MSG_PER_SECOND = 10;
        private static double VEHICLE_ACCELERATION = 3.0;   // m/s^2
        private static double SWERVE_FACTOR = 1.0;
        private static double SWERVE_DIR = 1;               // OR -1
        private static double TARGET_PROXIMITY = 0.8;

        public static void Main(string[] args)
        {
            var Swerve = args.Contains("-swerve");
            var Stop = args.Contains("-stop");
            var Accel = args.Contains("-accel");

            List<VehicleData> inputs = new List<VehicleData>();

            inputs.Add(GetRandomMsg());
            var lastInput = inputs.Last();

            if (Accel)
            {
                var accelDelta = VEHICLE_ACCELERATION / MSG_PER_SECOND * 2.237 /* m/s to mph */;

                for (int i = 0; i < MSG_PER_SECOND * 3; i++)
                {
                    inputs.Add(new VehicleData()
                    {
                        LandSpeedMph = lastInput.LandSpeedMph + accelDelta,
                        HorizontalPosition = lastInput.HorizontalPosition,
                        ForwardProximity = lastInput.ForwardProximity
                    });
                    lastInput = inputs.Last();
                }
            }

            if (Swerve)
            {
                var swerveDelta = SWERVE_DIR * 2 * SWERVE_FACTOR / (5 /*sec*/ * MSG_PER_SECOND);

                while (lastInput.HorizontalPosition * SWERVE_DIR < SWERVE_FACTOR) 
                {
                    inputs.Add(new VehicleData()
                    {
                        LandSpeedMph = lastInput.LandSpeedMph,
                        HorizontalPosition = lastInput.HorizontalPosition + swerveDelta,
                        ForwardProximity = lastInput.ForwardProximity
                    });
                    lastInput = inputs.Last();
                }

                while (lastInput.HorizontalPosition * SWERVE_DIR > 0)
                {
                    inputs.Add(new VehicleData()
                    {
                        LandSpeedMph = lastInput.LandSpeedMph,
                        HorizontalPosition = lastInput.HorizontalPosition - swerveDelta,
                        ForwardProximity = lastInput.ForwardProximity
                    });
                    lastInput = inputs.Last();
                }
            }

            if (Stop)
            {
                var decelDelta = VEHICLE_ACCELERATION / MSG_PER_SECOND * 2.237 /* m/s to mph */;

                while (lastInput.LandSpeedMph > 1)
                {
                    var proximityDelta = TARGET_PROXIMITY / (MSG_PER_SECOND * 2 /*sec*/);

                    inputs.Add(new VehicleData()
                    {
                        LandSpeedMph = lastInput.LandSpeedMph - decelDelta,
                        HorizontalPosition = lastInput.HorizontalPosition,
                        ForwardProximity = lastInput.LandSpeedMph / (decelDelta * MSG_PER_SECOND) < 2 ? lastInput.ForwardProximity + proximityDelta : lastInput.ForwardProximity
                    });
                    lastInput = inputs.Last();

                }

                inputs.Add(new VehicleData()
                {
                    LandSpeedMph = 0,
                    HorizontalPosition = lastInput.HorizontalPosition,
                    ForwardProximity = TARGET_PROXIMITY
                });
            }

            File.WriteAllText("hmi_input.json", JsonConvert.SerializeObject(inputs));
        }

        private static VehicleData GetRandomMsg()
        {
            var rand = new Random();
            return new VehicleData()
            {
                LandSpeedMph = rand.Next(60,80),
                HorizontalPosition = 0,
                ForwardProximity = 0
            };
        }
    }
}
