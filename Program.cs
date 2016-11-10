using DemoSat2016Netduino_OnboardSD.Drivers;
using DemoSat2016Netduino_OnboardSD.Flight_Computer;
using DemoSat2016Netduino_OnboardSD.Work_Items;
using Microsoft.SPOT;

namespace DemoSat2016Netduino_OnboardSD {
    
    
    //debug packets instead of usb debug
    
    public static class Program
    {
        public static LiquidCrystalI2C Lcd = new LiquidCrystalI2C(0,16,2);
        
        public static void Main() {

            Lcd.write("Testing!");
            
            //THIS SECTION CREATES / INITIALIZES THE SERIAL LOGGER
            Debug.Print("Flight computer started successfully. Beginning INIT.");

            var logger = new Logger();
            Debug.Print("Starting logger...");
            logger.Start();


            Rebug.Print("Starting stopwatch");
            Clock.Instance.Start();

            Rebug.Print("Recording time-sync packet");
            var timeSync = new TimeSync(delay:10000);
            timeSync.Run();

            //THIS SECTION CREATES/INITIALIZES THE SERIAL BNO 100HZ UPDATER
            Rebug.Print("Initializing BNO Sensor ");
            var bnoloop = new SerialBnoUpdater(delay:1000);


            //THIS SECTION CREATES/INITIALIZES THE SERIAL BNO 100HZ UPDATER
            Rebug.Print("Initializing BMP Sensor ");
            var bmploop = new PressureTempAltitudeUpdater(delay:1000);

            ////THIS SECTION CREATES/INITIALIZES THE MAGNETOMETER UPDATER
            var mag_dump_size = 18432;
            Rebug.Print("Initializing fast mag dump collector with a size of " + mag_dump_size + "bytes.");
            var magDumpLoop = new CustomMagUpdater(mag_dump_size);

            //Thread.Sleep(5000);
            Rebug.Print("Flight computer INIT Complete. Continuing with boot.");

            //THIS SECTION INITIALIZES AND STARTS THE MEMORY MONITOR
            Rebug.Print("Starting memory monitor...");
            MemoryMonitor.Instance.Start(ref logger);

            ////THIS STARTS THE Mag dump update
            Rebug.Print("Starting fast mag dump...");
            magDumpLoop.Start();

            //THIS STARTS THE BNO SENSOR UPDATE
            Rebug.Print("Starting bno sensor updates...");
            bnoloop.Start();

            //THIS STARTS THE BNO SENSOR UPDATE
            Rebug.Print("Starting bmp sensor updates...");
            bmploop.Start();

            Rebug.Print("Flight computer boot successful.");
        }
        public static void custom_delay_usec(int microseconds)
        {
            long delayTime = microseconds * 10;
            long delayStart = Microsoft.SPOT.Hardware.Utility.GetMachineTime().Ticks;
            while ((Microsoft.SPOT.Hardware.Utility.GetMachineTime().Ticks - delayStart) < delayTime) ;
        }
    }
}

