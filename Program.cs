using System.Globalization;
using System.Threading;
using DemoSat2016Netduino_OnboardSD.Drivers;
using DemoSat2016Netduino_OnboardSD.Flight_Computer;
using DemoSat2016Netduino_OnboardSD.Work_Items;
using FusionWare.SPOT.Hardware;
using MicroLiquidCrystal;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace DemoSat2016Netduino_OnboardSD {
    
    
    //debug packets instead of usb debug
    
    public static class Program
    {
        //public static LiquidCrystalI2C Lcd;
        private static void LCDFinish(Lcd lcd, string message) {
            lcd.SetCursorPosition(0, 1);
            lcd.Write(message);
            Thread.Sleep(600);
            lcd.Clear();
        }
        public static void Main() {

            var bus = new I2CBus();

            var lcdProvider = new MCP23008LcdTransferProvider(bus);
            var lcd = new Lcd(lcdProvider);
            lcd.Begin(16, 2);
            lcd.Backlight = true;
            lcd.Write("Payload McPayload Face v1.0");
            for (int i = 0; i < 12; i++) {
                lcd.ScrollDisplayLeft();
                Thread.Sleep(200);
            }
            Thread.Sleep(1000);
            lcd.Clear();
            LCDFinish(lcd, "ACC DemoSat 2016");

            //THIS SECTION CREATES / INITIALIZES THE SERIAL LOGGER
            Debug.Print("Flight computer started successfully. Beginning INIT.");
            
            //Lcd = new LiquidCrystalI2C(0x01, 16, 2);
            //Lcd.write("Testing!");
            lcd.Write("Starting logger.");
            var logger = new Logger();
            Debug.Print("Starting logger.");
            logger.Start();
            LCDFinish(lcd, "Done.");


            lcd.Write("Starting clock...");
            Rebug.Print("Starting clock.");
            Clock.Instance.Start();
            LCDFinish(lcd, "Done.");

            //THIS SECTION CREATES/INITIALIZES THE PRESSURE SENSOR
            //lcd.Write("Init BMP sensor.");
            //Rebug.Print("Initializing BMP Sensor ");
            //var bmploop = new PressureTempAltitudeUpdater(bus, delay: 1000);
            //LCDFinish(lcd);

            //THIS SECTION CREATES/INITIALIZES THE SERIAL BNO 100HZ UPDATER
            lcd.Write("Init BNO sensor...");
            Rebug.Print("Initializing BNO Sensor ");
            var bno = new SerialBno(SerialPorts.COM1, 5000, 5000, SerialBno.Bno055OpMode.OperationModeNdof);
            var bnoloop = new SerialBnoUpdater(bno, delay: 1000);
            LCDFinish(lcd, "Done.");

            ////Starts up the expensive mag
            lcd.Write("Init exp. mag...");
            Rebug.Print("Initializing expensive magnetometer on com3");
            var expensiveMagLoop = new ExpensiveMagUpdater(delay: 1000);
            LCDFinish(lcd, "Done.");

            lcd.Write("Init calib disp.");
            Rebug.Print("Initializing BNO calibration display loop");
            var printBnoCalib = new BNOCalibUpdate(bno, lcd, delay: 1000);
            LCDFinish(lcd, "Done.");

            //////THIS SECTION CREATES/INITIALIZES THE MAGNETOMETER UPDATER
            var mag_dump_size = 18432;
            lcd.Write("Init fast mag...");
            Rebug.Print("Initializing fast mag dump collector with a size of " + mag_dump_size + "bytes.");
            var customMagLoop = new CustomMagUpdater(mag_dump_size, AnalogChannels.ANALOG_PIN_A0);
            LCDFinish(lcd, "Done.");

            //Thread.Sleep(5000);
            lcd.Write("Init complete...");
            Rebug.Print("Flight computer INIT Complete. Continuing with boot.");
            LCDFinish(lcd, "Continuing boot.");

            //THIS SECTION INITIALIZES AND STARTS THE MEMORY MONITOR
            lcd.Write("Start memory monitor...");
            Rebug.Print("Starting memory monitor...");
            MemoryMonitor.Instance.Start(ref logger);
            LCDFinish(lcd, "Done.");

            ////THIS STARTS THE Mag dump update
            lcd.Write("Start f.mag loop...");
            Rebug.Print("Starting fast mag dump...");
            customMagLoop.Start();
            LCDFinish(lcd, "Done.");

            ////THIS STARTS THE BNO SENSOR UPDATE
            lcd.Write("Start bno loop...");
            Rebug.Print("Starting bno sensor updates...");
            bnoloop.Start();
            LCDFinish(lcd, "Done.");

            //THIS STARTS THE BNO SENSOR UPDATE
            //lcd.Write("Start bmp loop");
            //Rebug.Print("Starting bmp sensor updates...");
            //bmploop.Start();
            //LCDFinish(lcd);

            //THIS STARTS THE EXPENSIVE MAG UPDATE
            lcd.Write("Start e.mag loop...");
            Rebug.Print("Starting expensive mag updates...");
            expensiveMagLoop.Start();
            LCDFinish(lcd, "Done.");

            lcd.Write("Boot successful!");
            LCDFinish(lcd,"Entering run state.");

            Rebug.Print("Flight computer boot successful.");
            printBnoCalib.Start();
        }
        public static void custom_delay_usec(int microseconds)
        {
            long delayTime = microseconds * 10;
            long delayStart = Microsoft.SPOT.Hardware.Utility.GetMachineTime().Ticks;
            while ((Microsoft.SPOT.Hardware.Utility.GetMachineTime().Ticks - delayStart) < delayTime) ;
        }
    }
}

