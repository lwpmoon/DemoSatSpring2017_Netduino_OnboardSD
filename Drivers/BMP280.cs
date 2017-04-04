using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Math = System.Math;

namespace DemoSatSpring2017Netduino_OnboardSD.Drivers
{
    /// <summary>
    /// Driver for the Bosch BMx280 sensors. This driver can be used for both
    ///  BME and BMP varients of the 280
    /// </summary>
    public class Bmp280
    {
        private const byte BMP280_Address = 0x77;

        public const byte ChipId = 0x58;

        //Registers confirmed
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

            ChipId = 0xD0,
            Version = 0xD1,
            SoftReset = 0xE0,

            Cal26 = 0xE1,

            Control = 0xF4,
            Config = 0xF5,
            PressureData = 0xF7,
            TempData = 0xFA
        }

        //Mode now irrelevant
        public enum Mode : byte
        {
            Ultralowpower = 0,
            Standard = 1,
            Highres = 2,
            Ultrahighres = 3
        }

        //Filter now irrelevant
        public enum Filter : byte
        {
            Off = 0,
            X2 = 1,
            X4 = 2,
            X8 = 3,
            X16 = 4
        }

        /// <summary>
        /// Stucture to hold factory compensation data
        /// </summary>
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

        private Bmp280CompData _bmp280Compensations;

        private I2CDevice.Configuration _slaveConfig;

        private const int TransactionTimeout = 1000;

        private int t_fine;

        public Bmp280(byte address = BMP280_Address, Mode mode = Mode.Ultrahighres)
        {
            while (!Init(mode))
            {

                Debug.Print("BMP280 sensor not detected...");
            }
            Debug.Print("BMP280 Initialized.");
        }

        public bool Init(Mode mode)
        {
            _slaveConfig = new I2CDevice.Configuration(BMP280_Address, 100);
            //Thread.Sleep(100);

            var buffer = new byte[1];

            I2CBus.Instance.ReadRegister(_slaveConfig, (byte)Registers.ChipId, buffer, 1000);

            if (buffer[0] != ChipId)return false;
            

            ReadCoefficients();

            //This doesn't affect any of the values we are getting back. Still not sure what it does.
            I2CBus.Instance.WriteRegister(_slaveConfig, (byte)Registers.Control, 0x3F, TransactionTimeout);//Bug: Don't know what mode this puts it in... Test with and without...

            return true;
        }

        //Logic is sound but not pulling correct data back. Problem lies within...
        private void ReadCoefficients()
        {
            _bmp280Compensations.DigT1 = read16_LE((byte) Registers.CompDigT1);
            _bmp280Compensations.DigT2 = readS16_LE((byte) Registers.CompDigT2);
            _bmp280Compensations.DigT3 = readS16_LE((byte) Registers.CompDigT3);

            _bmp280Compensations.DigP1 = read16_LE((byte)Registers.CompDigP1);
            _bmp280Compensations.DigP2 = readS16_LE((byte)Registers.CompDigP2);
            _bmp280Compensations.DigP3 = readS16_LE((byte)Registers.CompDigP3);
            _bmp280Compensations.DigP4 = readS16_LE((byte)Registers.CompDigP4);
            _bmp280Compensations.DigP5 = readS16_LE((byte)Registers.CompDigP5);
            _bmp280Compensations.DigP6 = readS16_LE((byte)Registers.CompDigP6);
            _bmp280Compensations.DigP7 = readS16_LE((byte)Registers.CompDigP7);
            _bmp280Compensations.DigP8 = readS16_LE((byte)Registers.CompDigP8);
            _bmp280Compensations.DigP9 = readS16_LE((byte)Registers.CompDigP9);
        }


        /// <summary>
        /// Function to return Tempurature in DegC. Output value of “5123” equals 51.23 DegC.
        /// </summary>
        /// <returns>Returns temperature in DegC with resolution of 0.01 DegC</returns>
        public float readTemperature()
        {
            #region Data Sheet V2
            //uint var1, var2, T;

            //var adc_T = ReadRawTemp();

            //var1 = ((((adc_T >> 3) - ((uint)_bmp280Compensations.DigT1 << 1))) * ((uint)_bmp280Compensations.DigT2)) >> 11;


            //var2 = (((((adc_T >> 4) - ((uint)_bmp280Compensations.DigT1)) *
            //          ((adc_T >> 4) - ((uint)_bmp280Compensations.DigT1))) >> 12) * ((uint)_bmp280Compensations.DigT3)) >>
            //       14;

            //var t_fine = var1 + var2;
            //T = (t_fine * 5 + 128) >> 8;
            //return T; 
            #endregion

            #region Data Sheet V1

            //var t = ReadRawTemp();
            //var td = (double)t;

            //var var1 = ((td) / 16384.0 - ((double)_bmp280Compensations.DigT1) / 1024.0) *
            //           ((double)_bmp280Compensations.DigT2);

            //var var2 = (((td) / 131072.0 - ((double)_bmp280Compensations.DigT1) / 8192.0) *
            //            (td / 131072.0 - ((double)_bmp280Compensations.DigT1) / 8192.0)) *
            //           ((double)_bmp280Compensations.DigT3);

            //return ((var1 + var2) / 5120.0);

            #endregion


            #region Adafruit
            //Math checks out. temp data is wrong

            //int adc_T = (int)read24((byte)Registers.TempData);
            int adc_T = (int)read24((byte)Registers.TempData);
            adc_T >>= 4;

            int var1 = ((((adc_T >> 3) - ((int) _bmp280Compensations.DigT1 << 1))) * ((int) _bmp280Compensations.DigT2)) >> 11;

            int var2 = (((((adc_T >> 4) - ((int) _bmp280Compensations.DigT1)) *
                          ((adc_T >> 4) - ((int) _bmp280Compensations.DigT1))) >> 12) *
                        ((int) _bmp280Compensations.DigT3)) >> 14;

            t_fine = var1 + var2;

            float T = (t_fine * 5 + 128) >> 8;

            return T/100;

            #endregion
        }

        /// <summary>
        /// Returns pressure in Pa as unsigned 32 bit integer in Q24.8 format (24 integer bits and 8 fractional bits).
        /// Output value of “24674867” represents 24674867/256 = 96386.2 Pa = 963.862 hPa
        /// </summary>
        /// <returns></returns>
        public float readPressure()
        {
            #region Old
            //var t_fine = GetTemperature() * 5120.0;
            //var pressureBuffer = ReadRawPressure();

            //var adcP1 = (byte) pressureBuffer[0];
            //var adcP2 = (byte) pressureBuffer[1];
            //var adcP3 = (byte) pressureBuffer[2];
            //var P = (((adcP1 << 8) + adcP2) << 4) + (adcP3 >> 14);
            //var Pd = (double)P;

            //var var1 = ((double)t_fine / 2.0) - 64000.0;
            //var var2 = var1 * var1 * ((double)_bmp280Compensations.DigP6) / 32768.0;
            //var2 = var2 + var1 * ((double)_bmp280Compensations.DigP5) * 2.0;
            //var2 = (var2 / 4.0) + (((double)_bmp280Compensations.DigP4) * 65536.0);
            //var1 = (((double) _bmp280Compensations.DigP3)*var1*var1/524288.0 +
            //        ((double) _bmp280Compensations.DigP2)*var1)/524288.0;
            //var1 = (1.0 + var1 / 32768.0) * ((double)_bmp280Compensations.DigP1);

            ////Avoid exception caused by division by zero
            //if (var1 == 0.0)
            //{
            //    return 0;
            //}

            //var p = 1048576.0 - Pd;
            //p = (p - (var2 / 4096.0)) * 6250.0 / var1;
            //var1 = ((double)_bmp280Compensations.DigP9) * p * p / 2147483648.0;
            //var2 = p * ((double)_bmp280Compensations.DigP8) / 32768.0;
            //p = p + (var1 + var2 + ((double)_bmp280Compensations.DigP7)) / 16.0;
            //return p; 
            #endregion

            long var1, var2, p;

            // Must be done first to get the t_fine variable set up
            readTemperature();

            int adc_P = (int)read24((byte)Registers.PressureData);
            adc_P >>= 4;

            var1 = ((long)t_fine) - 128000;
            var2 = var1 * var1 * (long)_bmp280Compensations.DigP6;
            var2 = var2 + ((var1 * (long)_bmp280Compensations.DigP5) << 17);
            var2 = var2 + (((long)_bmp280Compensations.DigP4) << 35);
            var1 = ((var1 * var1 * (long)_bmp280Compensations.DigP3) >> 8) +
              ((var1 * (long)_bmp280Compensations.DigP2) << 12);
            var1 = (((((long)1) << 47) + var1)) * ((long)_bmp280Compensations.DigP1) >> 33;

            if (var1 == 0)
            {
                return 0;  // avoid exception caused by division by zero
            }
            p = 1048576 - adc_P;
            p = (((p << 31) - var2) * 3125) / var1;
            var1 = (((long)_bmp280Compensations.DigP9) * (p >> 13) * (p >> 13)) >> 25;
            var2 = (((long)_bmp280Compensations.DigP8) * p) >> 19;

            p = ((p + var1 + var2) >> 8) + (((long)_bmp280Compensations.DigP7) << 4);
            return (float)p / 256;

        }

        //Math checks out. Altitude data is wrong. Problem not here...
        /// <summary>
        /// Method to read the current altitude.
        /// </summary>
        /// <param name="seaLevelhPa">Sea Level compensation to be set the morning of launch</param>
        /// <returns>Altitude in (meters)</returns>
        public double readAltitude(float seaLevelhPa)
        {
            double altitude;

            float pressure = readPressure(); // in Si units for Pascal
            pressure /= 100;

            altitude = (44330 * (1.0 - Math.Pow(pressure / seaLevelhPa, 0.1903)));

            return altitude;
        }


        //Logic for these read methods looks sound but there is a plroblem with how the data is being retrieved.
        //Data is not returned with the expected values.
        //All read methods use equivalent logic to the Adafruit Librarie.

        private ushort read16(byte reg)
        {
            ushort value;

            byte[] buffer = new byte[2];
            I2CBus.Instance.ReadRegister(_slaveConfig, (byte)reg, buffer, TransactionTimeout);
            value = (ushort)((buffer[0] << 8) | buffer[1]);

            return value;
        }

        private ushort read16_LE(byte reg)
        {
            return read16(reg);
        }

        private short readS16_LE(byte reg)
        {
            ushort temp = read16(reg);
            return (short)((temp >> 8) | (temp << 8));
        }

        private uint read24(byte reg)
        {
            uint value;

            byte[] buffer = new byte[3];
            I2CBus.Instance.ReadRegister(_slaveConfig, (byte)reg, buffer, TransactionTimeout);

            value = buffer[0];
            value <<= 8;
            value |= buffer[1];
            value <<= 8;
            value |= buffer[2];

            return value;
        }

    }
}
