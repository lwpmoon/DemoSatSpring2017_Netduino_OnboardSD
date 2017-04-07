using System;
using System.Globalization;
using System.Threading;
using DemoSatSpring2017Netduino_OnboardSD.Drivers;
using DemoSatSpring2017Netduino_OnboardSD.Flight_Computer;
using DemoSatSpring2017Netduino_OnboardSD.Work_Items;
//using FusionWare.SPOT.Hardware;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace DemoSatSpring2017Netduino_OnboardSD {
    
    
    //debug packets instead of usb debug
    
    public static class Program
    {
        public const int I2CclockSpeed = 100;

        public static void Main() {

            Debug.Print("[INIT] Beginning init.");

            //init i2c
            Debug.Print("[INIT] Initializing I2C communications bus");
            var i2cBus = I2CBus.Instance;
            Debug.Print("[INIT] I2C communications bus initialized.");

            //THIS SECTION CREATES / INITIALIZES THE SERIAL LOGGER
            

            var logger = new Logger();
            Debug.Print("[INIT] Starting logger.");
            logger.Start();

            Rebug.Print("[INIT] Starting clock.");
            Clock.Instance.Start();

            //THIS SECTION CREATES / INITIALIZES THE PRESSURE SENSOR
            //lcd.Write("Init BMP sensor.");
            Rebug.Print("[INIT] Initializing BMP Sensor...");
            var bmp280Loop = new PressureTempAltitudeUpdater(delay: 1000);

            //LCDFinish(lcd);

            Rebug.Print("[INIT] Initializing LiPo Fuel Gauge...");
            var battery = new BatteryStateUpdater();
            //battery.Start();

            Rebug.Print("[INIT] Initializing Heater Controler...");
            var heater = new HeaterUpdater();


            Rebug.Print("[INIT] Initializing BNO Sensor... ");
            var bno = new SerialBno(SerialPorts.COM1, 5000, 5000, SerialBno.Bno055OpMode.OperationModeNdof);
            var bnoloop = new SerialBnoUpdater(bno, delay: 1000);

            Rebug.Print("[INIT] Initializing tracker...");
            var tracker = new LightTracker(bno, PWMChannels.PWM_PIN_D5);

            Rebug.Print("[INIT] Initializing Light Sensor...");
            var lightSensor = new LightSensorUpdater();
            

            Rebug.Print("[INFO] Flight computer INIT Complete. Continuing with boot.");

            //THIS SECTION INITIALIZES AND STARTS THE MEMORY MONITOR
            Rebug.Print("[STARTUP] Starting memory monitor and status update...");
            MemoryMonitor.Instance.Start(ref logger);

            Rebug.Print("[STARTUP] Starting heater updater...");
            //heater.Start();

            ////THIS STARTS THE BNO SENSOR UPDATER
            Rebug.Print("[STARTUP] Starting bno sensor updates...");
            bnoloop.Start();

            Rebug.Print("[STARTUP] Starting battery status updater...");
            battery.Start();

            Rebug.Print("[STARTUP] Starting Solar tracker");
            tracker.Start();

            //THIS STARTS THE BNO SENSOR UPDATER
            Rebug.Print("[STARTUP] Starting bmp sensor updater...");
            bmp280Loop.Start();

            //THIS STARTS THE LIGHT SENSOR UPDATER
            Rebug.Print("[STARTUP] Starting light sensor updater...");
            lightSensor.Start();

            Rebug.Print("[SUCCESS] Flight computer boot successful.");
        }

        public static void custom_delay_usec(int microseconds)
        {
            long delayTime = microseconds*10;
            long delayStart = Microsoft.SPOT.Hardware.Utility.GetMachineTime().Ticks;
            while ((Microsoft.SPOT.Hardware.Utility.GetMachineTime().Ticks - delayStart) < delayTime) ;
        }
    }
}

