using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Math = System.Math;

namespace DemoSatSpring2017Netduino_OnboardSD.Drivers
{
    /// <summary>
    /// Driver for the Bosch BMx280 sensors. This driver can be used for both
    ///  BME and BMP varients of the 280
    /// </summary>
    class Bmp280
    {
        private enum Registers : byte
        {
            CompDigT1 = 0x88,
            CompDigT2 = 0x8A,
            CompDigT3 = 0x8C,
            CompDigP1 = 0x8E,
            CompDigP2 = 0x90,
            CompDigP3 = 0x92,
            CompDigP4 = 0x94,
            CompDigP5 = 0x96,
            CompDigP6 = 0x98,
            CompDigP7 = 0x9A,
            CompDigP8 = 0x9C,
            CompDigP9 = 0x9E,
            Id = 0xD0,
            Reset = 0xE0,
            Cal26 = 0xE1,
            Status = 0xF3,
            Control = 0xF4,
            Config = 0xF5,
            Temperature_msb = 0xFA,
            Temperature_lsb = 0xFB,
            Temperature_xlsb = 0xFC,
            Pressure_msb = 0xF7,
            Pressure_lsb = 0xF8,
            Pressure_xlsb = 0xF9
        }

        public enum Mode : byte
        {
            Ultralowpower = 0,
            Standard = 1,
            Highres = 2,
            Ultrahighres = 3
        }

        public enum Filter : byte
        {
            Off = 0,
            X2 = 1,
            X4 = 2,
            X8 = 3,
            X16 = 4
        }



        struct Bmp280CompData
        {
            public ushort DigT1;
            public short DigT2;
            public short DigT3;
            public ushort DigP1;
            public short DigP2;
            public short DigP3;
            public short DigP4;
            public short DigP5;
            public short DigP6;
            public short DigP7;
            public short DigP8;
            public short DigP9;
        }

        private readonly byte Bmp280Address = 0x77;
        public const double SensorsPressureSealevelhpa = 1013.25;//Update this the morning of launch!!!
        Mode _mode = Mode.Ultrahighres;
        private Bmp280CompData _bmp280Compensations;

        private readonly I2CDevice.Configuration _slaveConfig;
        private I2CBus _bus;
        private const int TransactionTimeout = 1000; // ms

        public Bmp280(I2CBus bus, byte address = 0x77, Mode mode = Mode.Ultralowpower)
        {
            _bus = bus;
            Bmp280Address = address;
            _slaveConfig = new I2CDevice.Configuration(Bmp280Address, 100);
            while (!Init(mode))
            {

                Debug.Print("BMP280 sensor not detected...");
            }
        }

        public bool Init(Mode mode = Mode.Standard)
        {
            if ((mode > Mode.Ultrahighres) || (mode < 0))
                _mode = Mode.Standard;
            else
                _mode = mode;

            byte[] whoami = { 0 };

            _bus.WriteRead(_slaveConfig, new[] { (byte)Registers.Id }, whoami, TransactionTimeout);

            if (whoami[0] != 0x58) return false;

            ReadCompensations();

            return true;
        }

        private void ReadCompensations()
        {

            var buffer = new byte[2];

            _bus.WriteRead(_slaveConfig, new[] { (byte)Registers.CompDigT1 }, buffer, TransactionTimeout);
            _bmp280Compensations.DigT1 = (ushort)((buffer[0] << 8) | buffer[1]);

            _bus.WriteRead(_slaveConfig, new[] { (byte)Registers.CompDigT2 }, buffer, TransactionTimeout);
            _bmp280Compensations.DigT2 = (short)((buffer[0] << 8) | buffer[1]);

            _bus.WriteRead(_slaveConfig, new[] { (byte)Registers.CompDigT3 }, buffer, TransactionTimeout);
            _bmp280Compensations.DigT3 = (short)((buffer[0] << 8) | buffer[1]);

            _bus.WriteRead(_slaveConfig, new[] { (byte)Registers.CompDigP1 }, buffer, TransactionTimeout);
            _bmp280Compensations.DigP1 = (ushort)((buffer[0] << 8) | buffer[1]);

            _bus.WriteRead(_slaveConfig, new[] { (byte)Registers.CompDigP2 }, buffer, TransactionTimeout);
            _bmp280Compensations.DigP2 = (short)((buffer[0] << 8) | buffer[1]);

            _bus.WriteRead(_slaveConfig, new[] { (byte)Registers.CompDigP3 }, buffer, TransactionTimeout);
            _bmp280Compensations.DigP3 = (short)((buffer[0] << 8) | buffer[1]);

            _bus.WriteRead(_slaveConfig, new[] { (byte)Registers.CompDigP4 }, buffer, TransactionTimeout);
            _bmp280Compensations.DigP4 = (short)((buffer[0] << 8) | buffer[1]);

            _bus.WriteRead(_slaveConfig, new[] { (byte)Registers.CompDigP5 }, buffer, TransactionTimeout);
            _bmp280Compensations.DigP5 = (short)((buffer[0] << 8) | buffer[1]);

            _bus.WriteRead(_slaveConfig, new[] { (byte)Registers.CompDigP6 }, buffer, TransactionTimeout);
            _bmp280Compensations.DigP6 = (short)((buffer[0] << 8) | buffer[1]);

            _bus.WriteRead(_slaveConfig, new[] { (byte)Registers.CompDigP7 }, buffer, TransactionTimeout);
            _bmp280Compensations.DigP7 = (short)((buffer[0] << 8) | buffer[1]);

            _bus.WriteRead(_slaveConfig, new[] { (byte)Registers.CompDigP8 }, buffer, TransactionTimeout);
            _bmp280Compensations.DigP8 = (short)((buffer[0] << 8) | buffer[1]);

            _bus.WriteRead(_slaveConfig, new[] { (byte)Registers.CompDigP9 }, buffer, TransactionTimeout);
            _bmp280Compensations.DigP9 = (short)((buffer[0] << 8) | buffer[1]);
        }

        private int ReadRawTemp()
        {
            var tempBuffer = new byte[3];
            
            _bus.WriteRead(_slaveConfig, new []{(byte) Registers.Temperature_msb}, tempBuffer, TransactionTimeout);
        
            return (((tempBuffer[0] << 8) + tempBuffer[1]) << 4) + (tempBuffer[2] >> 14);
            
        }

        private byte[] ReadRawPressure()
        {
            var pressureBuffer = new byte[3];

            _bus.WriteRead(_slaveConfig,new [] {(byte) Registers.Pressure_msb}, pressureBuffer, TransactionTimeout);
            return pressureBuffer;
        }

        /// <summary>
        /// Function to return Tempurature in DegC. Output value of “5123” equals 51.23 DegC.
        /// </summary>
        /// <returns>Returns temperature in DegC with resolution of 0.01 DegC</returns>
        public double GetTemperature()
        {
            var t = ReadRawTemp();
            var td = (double) t;

            var var1 = ((td)/16384.0 - ((double) _bmp280Compensations.DigT1)/1024.0)*
                       ((double) _bmp280Compensations.DigT2);

            var var2 = (((td)/131072.0 - ((double) _bmp280Compensations.DigT1)/8192.0)*
                        (td/131072.0 - ((double) _bmp280Compensations.DigT1)/8192.0))*
                       ((double) _bmp280Compensations.DigT3);

            return ((var1 + var2)/5120.0);
        }

        /// <summary>
        /// Returns pressure in Pa as unsigned 32 bit integer in Q24.8 format (24 integer bits and 8 fractional bits).
        /// Output value of “24674867” represents 24674867/256 = 96386.2 Pa = 963.862 hPa
        /// </summary>
        /// <returns></returns>
        public double GetPressure()
        {
            var t_fine = GetTemperature() * 5120.0;
            var pressureBuffer = ReadRawPressure();

            var adcP1 = (byte) pressureBuffer[0];
            var adcP2 = (byte) pressureBuffer[1];
            var adcP3 = (byte) pressureBuffer[2];
            var P = (((adcP1 << 8) + adcP2) << 4) + (adcP3 >> 14);
            var Pd = (double)P;

            var var1 = ((double)t_fine / 2.0) - 64000.0;
            var var2 = var1 * var1 * ((double)_bmp280Compensations.DigP6) / 32768.0;
            var2 = var2 + var1 * ((double)_bmp280Compensations.DigP5) * 2.0;
            var2 = (var2 / 4.0) + (((double)_bmp280Compensations.DigP4) * 65536.0);
            var1 = (((double) _bmp280Compensations.DigP3)*var1*var1/524288.0 +
                    ((double) _bmp280Compensations.DigP2)*var1)/524288.0;
            var1 = (1.0 + var1 / 32768.0) * ((double)_bmp280Compensations.DigP1);

            //Avoid exception caused by division by zero
            if (var1 == 0.0)
            {
                return 0;
            }

            var p = 1048576.0 - Pd;
            p = (p - (var2 / 4096.0)) * 6250.0 / var1;
            var1 = ((double)_bmp280Compensations.DigP9) * p * p / 2147483648.0;
            var2 = p * ((double)_bmp280Compensations.DigP8) / 32768.0;
            p = p + (var1 + var2 + ((double)_bmp280Compensations.DigP7)) / 16.0;
            return p;
        }

        public double GetCurrentAltitude()
        {
            return PressureToAltitude(SensorsPressureSealevelhpa, GetPressure(), GetTemperature());
        }

        public static double PressureToAltitude(double seaLevel, double atmospheric, double temp)
        {
            /* Hyposometric formula:                      */
            /*                                            */
            /*     ((P0/P)^(1/5.257) - 1) * (T + 273.15)  */
            /* h = -------------------------------------  */
            /*                   0.0065                   */
            /*                                            */
            /* where: h   = height (in meters)            */
            /*        P0  = sea-level pressure (in hPa)   */
            /*        P   = atmospheric pressure (in hPa) */
            /*        T   = temperature (in �C)           */

            return ((Math.Pow((seaLevel / atmospheric), 0.190223F) - 1.0F) * (temp + 273.15F)) / 0.0065;
        }
    }
}
