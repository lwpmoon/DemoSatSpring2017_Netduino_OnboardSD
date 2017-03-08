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
using test_bot_netduino;

namespace DemoSatSpring2017Netduino_OnboardSD {
    
    
    //debug packets instead of usb debug
    
    public static class Program
    {
        
        public static void Main() {
            I2CBus bus = new I2CBus();
            
            //THIS SECTION CREATES / INITIALIZES THE SERIAL LOGGER
            Debug.Print("Flight computer started successfully. Beginning INIT.");
            
            var logger = new Logger();
            Debug.Print("Starting logger.");
            logger.Start();

            Rebug.Print("Starting clock.");
            Clock.Instance.Start();

            //var tracker = new LightTracker(PWMChannels.PWM_PIN_D5, PWMChannels.PWM_PIN_D10,
            //                            Cpu.AnalogChannel.ANALOG_2, Cpu.AnalogChannel.ANALOG_0,
            //                            Cpu.AnalogChannel.ANALOG_3, Cpu.AnalogChannel.ANALOG_1);

            var tracker = new LightTracker(PWMChannels.PWM_PIN_D5, Cpu.AnalogChannel.ANALOG_0,
                                        Cpu.AnalogChannel.ANALOG_1);
            tracker.Start();


            //THIS SECTION CREATES/INITIALIZES THE PRESSURE SENSOR
            //lcd.Write("Init BMP sensor.");
            Rebug.Print("Initializing BMP Sensor ");
            var bmploop = new PressureTempAltitudeUpdater(bus, delay: 1000);
            //LCDFinish(lcd);

            //THIS SECTION CREATES/INITIALIZES THE SERIAL BNO 100HZ UPDATER
            
            Rebug.Print("Initializing BNO Sensor ");
            var bno = new SerialBno(SerialPorts.COM1, 5000, 5000, SerialBno.Bno055OpMode.OperationModeNdof);
            var bnoloop = new SerialBnoUpdater(bno, delay: 1000);

            
            Rebug.Print("Initializing BNO calibration display loop");
            var printBnoCalib = new BNOCalibUpdate(bno, delay: 1000);
            //LCDFinish(lcd, "Done.");

           

            //Thread.Sleep(5000);
            //lcd.Write("Init complete...");
            Rebug.Print("Flight computer INIT Complete. Continuing with boot.");
            //LCDFinish(lcd, "Continuing boot.");

            //THIS SECTION INITIALIZES AND STARTS THE MEMORY MONITOR
            //lcd.Write("Start memory monitor...");
            Rebug.Print("Starting memory monitor...");
            MemoryMonitor.Instance.Start(ref logger);
            //LCDFinish(lcd, "Done.");

            
            //LCDFinish(lcd, "Done.");

            ////THIS STARTS THE BNO SENSOR UPDATE
            //lcd.Write("Start bno loop...");
            Rebug.Print("Starting bno sensor updates...");
            bnoloop.Start();
            //LCDFinish(lcd, "Done.");

            //THIS STARTS THE BNO SENSOR UPDATE
            //lcd.Write("Start bmp loop");
            //TODO BNP180 discontinued... Update at later time
            Rebug.Print("Starting bmp sensor updates...");
            bmploop.Start();
            //LCDFinish(lcd);

            //lcd.Write("Boot successful!");
            //LCDFinish(lcd,"Entering run state.");

            Rebug.Print("Flight computer boot successful.");
            printBnoCalib.Start();
        }

        public static void custom_delay_usec(int microseconds)
        {
            long delayTime = microseconds*10;
            long delayStart = Microsoft.SPOT.Hardware.Utility.GetMachineTime().Ticks;
            while ((Microsoft.SPOT.Hardware.Utility.GetMachineTime().Ticks - delayStart) < delayTime) ;
        }
    }
}

