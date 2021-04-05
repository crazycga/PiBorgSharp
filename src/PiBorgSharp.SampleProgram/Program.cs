using System;
using PiBorgSharp;
using PiBorgSharp.UltraBorg;

namespace PiBorgSharp.SampleProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            MyLogger_class log = new MyLogger_class();
            UltraBorg_class myBorg = new UltraBorg_class(log);
            UBReading_class firstReadings = new UBReading_class(myBorg, log);

            firstReadings.EnumerateCurrentEnvironment(myBorg, UBReading_class.ScanType.FullScan, log);

            log.WriteLog("Sensor 1: " + firstReadings.sensor1Reading.ToString());
            log.WriteLog("Min 1: " + firstReadings.servo1Min.ToString());
            log.WriteLog("Max 1: " + firstReadings.servo1Max.ToString());
            log.WriteLog("Boot 1: " + firstReadings.servo1Boot.ToString());

            log.WriteLog("Sensor 2: " + firstReadings.sensor2Reading.ToString());
            log.WriteLog("Min 2: " + firstReadings.servo2Min.ToString());
            log.WriteLog("Max 2: " + firstReadings.servo2Max.ToString());
            log.WriteLog("Boot 2: " + firstReadings.servo2Boot.ToString());

            log.WriteLog("Sensor 3: " + firstReadings.sensor3Reading.ToString());
            log.WriteLog("Min 3: " + firstReadings.servo3Min.ToString());
            log.WriteLog("Max 3: " + firstReadings.servo3Max.ToString());
            log.WriteLog("Boot 3: " + firstReadings.servo3Boot.ToString());

            log.WriteLog("Sensor 4: " + firstReadings.sensor4Reading.ToString());
            log.WriteLog("Min 4: " + firstReadings.servo4Min.ToString());
            log.WriteLog("Max 4: " + firstReadings.servo4Max.ToString());
            log.WriteLog("Boot 4: " + firstReadings.servo4Boot.ToString());

            uint s1;
            uint s2;
            uint s3;
            uint s4;

            while (true)
            {
                s1 = myBorg.GetDistance(1, UltraBorg_class.FilterType.Unfiltered);
                s2 = myBorg.GetDistance(2, UltraBorg_class.FilterType.Unfiltered);
                s3 = myBorg.GetDistance(3, UltraBorg_class.FilterType.Unfiltered);
                s4 = myBorg.GetDistance(4, UltraBorg_class.FilterType.Unfiltered);

                Console.WriteLine("1: " + s1.ToString() + " 2: " + s2.ToString() + " 3: " + s3.ToString() + " 4: " + s4.ToString());
            }

        }
    }
}
