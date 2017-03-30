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
            
            
            //THIS SECTION CREATES / INITIALIZES THE SERIAL LOGGER
            Debug.Print("Flight computer started successfully. Beginning INIT.");
            
            var logger = new Logger();
            Debug.Print("Starting logger.");
            logger.Start();

            Rebug.Print("Starting clock.");
            Clock.Instance.Start();

            //THIS SECTION CREATES/INITIALIZES THE PRESSURE SENSOR
            //lcd.Write("Init BMP sensor.");
            Rebug.Print("Initializing BMP Sensor ");
            var bmp280Loop = new PressureTempAltitudeUpdater(delay: 1000);

            //LCDFinish(lcd);

            Rebug.Print("Initializing Heater Controler ");
            var heater = new HeaterUpdater();

            //THIS SECTION CREATES/INITIALIZES THE SERIAL BNO 100HZ UPDATER
            
            Rebug.Print("Initializing BNO Sensor ");
            var bno = new SerialBno(SerialPorts.COM1, 5000, 5000, SerialBno.Bno055OpMode.OperationModeNdof);
            var bnoloop = new SerialBnoUpdater(bno, delay: 1000);


            var tracker = new LightTracker(bno, PWMChannels.PWM_PIN_D5);
            tracker.Start();

            Rebug.Print("Flight computer INIT Complete. Continuing with boot.");

            //THIS SECTION INITIALIZES AND STARTS THE MEMORY MONITOR
            Rebug.Print("Starting memory monitor...");
            MemoryMonitor.Instance.Start(ref logger);

            Rebug.Print("Starting heater...");
            //heater.Start();
            
            ////THIS STARTS THE BNO SENSOR UPDATE
            Rebug.Print("Starting bno sensor updates...");
            //bnoloop.Start();

            //THIS STARTS THE BNO SENSOR UPDATE
            //Rebug.Print("Starting bmp sensor updates...");
            bmp280Loop.Start();

            Debug.Print("Flight computer boot successful.");
        }

        public static void custom_delay_usec(int microseconds)
        {
            long delayTime = microseconds*10;
            long delayStart = Microsoft.SPOT.Hardware.Utility.GetMachineTime().Ticks;
            while ((Microsoft.SPOT.Hardware.Utility.GetMachineTime().Ticks - delayStart) < delayTime) ;
        }
    }
}

