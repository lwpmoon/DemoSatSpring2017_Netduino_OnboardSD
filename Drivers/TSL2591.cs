using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace DemoSatSpring2017Netduino_OnboardSD.Drivers
{
    public class Tsl2591
    {
        
        //This is the last priority when integrating the sensors. Not really needed...

        //private Coefficients _coefficients;
        private bool _initialized;
        private Gain _gain;
        private Integrationtime _integration;


        private readonly I2CDevice.Configuration _slaveConfig;
        private const int TransactionTimeout = 1000; // ms
        private const int ClockSpeed = Program.I2CclockSpeed;
        

        public Tsl2591(byte address = 0x29, Gain gain = Gain.Med, Integrationtime inte = Integrationtime._100MS)
        {
            _slaveConfig = new I2CDevice.Configuration(address, ClockSpeed);
            _initialized = false;

            while (!Init(gain, inte))
            {

                Debug.Print("TSL2591 Light Sensor not detected...");
                Thread.Sleep(500);
            }
        }

        public bool Init(Gain gain, Integrationtime integrationtime)
        {
            byte[] whoami = { 0 };

            I2CBus.Instance.ReadRegister(_slaveConfig, (byte)Bits.CommandBit | (byte)Registers.DeviceId, whoami, TransactionTimeout);

            if (whoami[0] != 0x50) return false;

            _initialized = true;

            _gain = gain;
            _integration = integrationtime;

            SetGainAndTiming();

            //EnableSensor();

            return true;
        }

        private void SetGainAndTiming()
        {
            if (!_initialized)
            {
                if (!(Init(_gain, _integration)))
                {
                    return;
                }
            }
            EnableSensor();
            byte[]data = new byte[2];
            data[0] = (byte) Bits.CommandBit | (byte) Registers.Control;
            data[1] = (byte)((byte)_integration | (byte) _gain);
            //write8(TSL2591_COMMAND_BIT | TSL2591_REGISTER_CONTROL, _integration | _gain);
            I2CBus.Instance.Write(_slaveConfig, data, TransactionTimeout);
            //DisableSensor();
        }

        //Refactored. Not tested
        private void EnableSensor()
        {
            if (!_initialized)
            {
                if (!(Init(_gain, _integration)))
                {
                    return;
                }
            }

            // Enable the device by setting the control bit to 0x01
            //I2CBus.Instance.Write(_slaveConfig,new []{ (byte)((byte)Bits.COMMAND_BIT | (byte) Registers.ENABLE | (byte) Enable.POWERON | (byte) Enable.AEN | (byte) Enable.AIEN | (byte) Enable.NPIEN)}, TransactionTimeout);
            I2CBus.Instance.Write(_slaveConfig,new []{ (byte)((byte)Bits.CommandBit | (byte) Registers.Enable | (byte) Enable.Poweron)}, TransactionTimeout);
        }

        private void DisableSensor()
        {
            if (!_initialized)
            {
                if (!(Init(_gain, _integration)))
                {
                    return;
                }
            }

            // DisableSensor the device by setting the control bit to 0x00
            //write8(TSL2591_COMMAND_BIT | TSL2591_REGISTER_ENABLE, TSL2591_ENABLE_POWEROFF);
            I2CBus.Instance.Write(_slaveConfig, new []{(byte)((byte) Bits.CommandBit | (byte) Registers.Enable | (byte) Enable.Poweroff)}, TransactionTimeout);
        }

        private ulong CalculateLux(ushort ch0, ushort ch1)
        {
            float atime, again;
            // Check for overflow conditions first
            if ((ch0 == 0xFFFF) | (ch1 == 0xFFFF))
            {
                // Signal an overflow
                return 0;
            }

            // Note: This algorithm is based on preliminary coefficients
            // provided by AMS and may need to be updated in the future

            switch (_integration)
            {
                case Integrationtime._100MS:
                    atime = 100.0F;
                    break;
                case Integrationtime._200MS:
                    atime = 200.0F;
                    break;
                case Integrationtime._300MS:
                    atime = 300.0F;
                    break;
                case Integrationtime._400MS:
                    atime = 400.0F;
                    break;
                case Integrationtime._500MS:
                    atime = 500.0F;
                    break;
                case Integrationtime._600MS:
                    atime = 600.0F;
                    break;
                default: // 100ms
                    atime = 100.0F;
                    break;
            }

            switch (_gain)
            {
                case Gain.Low:
                    again = 1.0F;
                    break;
                case Gain.Med:
                    again = 25.0F;
                    break;
                case Gain.High:
                    again = 428.0F;
                    break;
                case Gain.Max:
                    again = 9876.0F;
                    break;
                default:
                    again = 1.0F;
                    break;
            }

            // cpl = (ATIME * AGAIN) / DF
            var cpl = (atime*again)/ LuxDf;

            var lux1 = (ch0 - (LuxCoefb * ch1)) / cpl;
            var lux2 = ((LuxCoefc * ch0) - (LuxCoefd * ch1)) / cpl;
            var lux = lux1 > lux2 ? lux1 : lux2;

            // Alternate lux calculation
            //lux = ( (float)ch0 - ( 1.7F * (float)ch1 ) ) / cpl;

            // Signal I2C had no errors
            return (ulong)lux;
        }

        //Refactored. Not tested
        public uint GetFullLuminosity()
        {
            if (!_initialized)
            {
                if (!(Init(_gain, _integration)))
                {
                    return 0;
                }
            }

            EnableSensor();

            Thread.Sleep(112);
           
            var ch0 = new byte[2];
            var ch1 = new byte[2];
            I2CBus.Instance.ReadRegister(_slaveConfig, (byte)Bits.CommandBit | (byte)Bits.WordBit | (byte)Registers.Chan0Low, ch0, 1000);
            I2CBus.Instance.ReadRegister(_slaveConfig, (byte)Bits.CommandBit | (byte)Bits.WordBit | (byte)Registers.Chan1Low, ch1, 1000);
            var broadband = (ushort)((ch0[1] << 8) | ch0[0]);
            var infrared = (ushort)((ch1[1] << 8) | ch1[0]);

            //DisableSensor();

            return (uint)CalculateLux(broadband, infrared);

            #region Unused
            //byte[] writeBuffer = new byte[2];
            //byte[] readBuffer = new byte[2];

            //writeBuffer[0] = (byte)Bits.COMMAND_BIT;
            //writeBuffer[1] = (byte)Registers.CHAN1_LOW;
            //var reg = (writeBuffer[0] | writeBuffer[1]);

            //I2CBus.Instance.ReadRegister(_slaveConfig, (byte) reg, readBuffer, TransactionTimeout);

            //uint t = readBuffer[0];
            //uint x = readBuffer[1];
            //x = (x << 8);
            //var data = (x | t);


            //writeBuffer[0] = (byte)Bits.COMMAND_BIT;
            //writeBuffer[1] = (byte)Registers.CHAN0_LOW;
            //reg = (byte)(writeBuffer[0] | writeBuffer[1]);

            //I2CBus.Instance.ReadRegister(_slaveConfig, (byte) reg, readBuffer, TransactionTimeout);

            ////byte data2 = readBuffer[0];

            //t = readBuffer[0];
            //x = readBuffer[1];
            //x = (byte)(x << 8);
            //byte data2 = (byte)(x | t);

            //var value = (data << 16) | (data2);

            //DisableSensor();

            //return (uint)value; 
            #endregion
        }

        #region Unused
        //Refactored. Not tested
        //private ushort getLuminosity(byte channel)
        //{
        //    uint x = GetFullLuminosity();

        //    if (channel == (byte)Channels.FULLSPECTRUM)
        //    {
        //        // Reads two byte value from channel 0 (visible + infrared)
        //        return (ushort)(x & 0xFFFF);
        //    }
        //    else if (channel == (byte)Channels.INFRARED)
        //    {
        //        // Reads two byte value from channel 1 (infrared)
        //        return (ushort)(x >> 16);
        //    }
        //    else if (channel == (byte)Channels.VISIBLE)
        //    {
        //        // Reads all and subtracts out just the visible!
        //        return (ushort)((x & 0xFFFF) - (x >> 16));
        //    }

        //    // unknown channel!
        //    return 0;
        //}

        //private void registerInterrupt(ushort lowerThreshold, ushort upperThreshold)
        //{
        //    if (!_initialized)
        //    {
        //        Integrationtime inte = _integration;
        //        Gain gain = _gain;

        //        while (!Init(gain, inte))
        //        {
        //            return;
        //        }
        //    }

        //    EnableSensor();
        //    //write8(TSL2591_COMMAND_BIT | TSL2591_REGISTER_THRESHOLD_NPAILTL, lowerThreshold);
        //    //write8(TSL2591_COMMAND_BIT | TSL2591_REGISTER_THRESHOLD_NPAILTH, lowerThreshold >> 8);
        //    //write8(TSL2591_COMMAND_BIT | TSL2591_REGISTER_THRESHOLD_NPAIHTL, upperThreshold);
        //    //write8(TSL2591_COMMAND_BIT | TSL2591_REGISTER_THRESHOLD_NPAIHTH, upperThreshold >> 8);
        //    byte value = (((byte) (Bits.COMMAND_BIT) | (byte) (Registers.THRESHOLD_NPAILTH)));
        //    I2CBus.Instance.WriteRegister(_slaveConfig, (byte)value, (byte)lowerThreshold, TransactionTimeout);

        //    DisableSensor();
        //}

        //void Adafruit_TSL2591::registerInterrupt(uint16_t lowerThreshold, uint16_t upperThreshold, tsl2591Persist_t persist)
        //{
        //    if (!_initialized)
        //    {
        //        if (!begin())
        //        {
        //            return;
        //        }
        //    }

        //    EnableSensor();
        //    write8(TSL2591_COMMAND_BIT | TSL2591_REGISTER_PERSIST_FILTER, persist);
        //    write8(TSL2591_COMMAND_BIT | TSL2591_REGISTER_THRESHOLD_AILTL, lowerThreshold);
        //    write8(TSL2591_COMMAND_BIT | TSL2591_REGISTER_THRESHOLD_AILTH, lowerThreshold >> 8);
        //    write8(TSL2591_COMMAND_BIT | TSL2591_REGISTER_THRESHOLD_AIHTL, upperThreshold);
        //    write8(TSL2591_COMMAND_BIT | TSL2591_REGISTER_THRESHOLD_AIHTH, upperThreshold >> 8);
        //    DisableSensor();
        //}

        //Refactored. Not tested
        //private void clearInterrupt()
        //{
        //    if (!_initialized)
        //    {
        //        if (!(Init(_gain, _integration)))
        //        {
        //            return;
        //        }
        //    }

        //    //EnableSensor();
        //    I2CBus.Instance.Write(_slaveConfig, new []{(byte)Bits.CLEAR_INT}, TransactionTimeout );
        //    //DisableSensor();
        //}

        //Refactored. Not tested
        //private byte getStatus()
        //{
        //    if (!_initialized)
        //    {
        //        if (!(Init(_gain, _integration)))
        //        {
        //            return 0;
        //        }
        //    }

        //    // Enable the device
        //    //EnableSensor();
        //    byte[] x= new byte[1];
        //    //x = read8(TSL2591_COMMAND_BIT | TSL2591_REGISTER_DEVICE_STATUS);
        //    byte value = (byte) Bits.COMMAND_BIT | (byte) Registers.DEVICE_STATUS;
        //    I2CBus.Instance.ReadRegister(_slaveConfig, value, x, TransactionTimeout);
        //    //DisableSensor();
        //    return x[0];
        //}

        //bool getEvent(sensors_event_t*event)
        //{
        //    uint16_t ir, full;
        //    uint32_t lum = getFullLuminosity();
        //    /* Early silicon seems to have issues when there is a sudden jump in */
        //    /* light levels. :( To work around this for now sample the sensor 2x */
        //    /*lum = getFullLuminosity();
        //    ir = lum >> 16;
        //    full = lum & 0xFFFF;

        //    /* Clear the event */
        //    memset(event, 0, sizeof(sensors_event_t));

        //    event->version   = sizeof(sensors_event_t);
        //    event->sensor_id = _sensorID;
        //    event->type      = SENSOR_TYPE_LIGHT;
        //    event->timestamp = millis();

        //    /* Calculate the actual lux value */
        //    /* 0 = sensor overflow (too much light) */
        //    event->light = calculateLux(full, ir);

        //    return true;
        //} 
        #endregion

        #region Trunk
        private enum Registers : byte
        {
            Enable = 0x00,
            Control = 0x01,
            //THRESHOLD_AILTL = 0x04, // ALS low threshold lower byte
            //THRESHOLD_AILTH = 0x05, // ALS low threshold upper byte
            //THRESHOLD_AIHTL = 0x06, // ALS high threshold lower byte
            //THRESHOLD_AIHTH = 0x07, // ALS high threshold upper byte
            //THRESHOLD_NPAILTL = 0x08, // No Persist ALS low threshold lower byte
            //THRESHOLD_NPAILTH = 0x09, // etc
            //THRESHOLD_NPAIHTL = 0x0A,
            //THRESHOLD_NPAIHTH = 0x0B,
            //PERSIST_FILTER = 0x0C,
            //PACKAGE_PID = 0x11,
            DeviceId = 0x12,
            //DEVICE_STATUS = 0x13,
            Chan0Low = 0x14,
            //CHAN0_HIGH = 0x15,
            Chan1Low = 0x16
            //CHAN1_HIGH = 0x17
        }

        private enum Bits : byte
        {
            //READBIT = 0x01,
            CommandBit = 0xA0, // 1010 0000: bits 7 and 5 for 'command normal'
            //CLEAR_INT = 0xE7,
            //TEST_INT = 0xE4,
            WordBit = 0x20    // 1 = read/write word (rather than byte)
            //BLOCK_BIT = 0x10    // 1 = using block read/write
        }

        private enum Enable : byte
        {
            Poweroff = 0x00,
            Poweron = 0x01
            //AEN = 0x02,    // ALS Enable. This field activates ALS function. Writing a one activates the ALS. Writing a zero disables the ALS.
            //AIEN = 0x10,    // ALS Interrupt Enable. When asserted permits ALS interrupts to be generated, subject to the persist filter.
            //NPIEN = 0x80    // No Persist Interrupt Enable. When asserted NP Threshold conditions will generate an interrupt, bypassing the persist filter
        }

        //private enum Channels
        //{
        //    FULLSPECTRUM = 0,
        //    INFRARED = 1, // channel 1
        //    VISIBLE = 2 // channel 0 - channel 1
        //}

        public enum Gain : byte
        {
            Low = 0x00,     // low gain (1x)
            Med = 0x10,     // medium gain (25x)
            High = 0x20,    // medium gain (428x)
            Max = 0x30      // max gain (9876x)
        }

        public enum Integrationtime : byte
        {
            _100MS = 0x00,
            _200MS = 0x01,
            _300MS = 0x02,
            _400MS = 0x03,
            _500MS = 0x04,
            _600MS = 0x05
        }

        public enum Persistant : byte
        {
            //  bit 7:4: 0
            Every = 0x00,  // Every ALS cycle generates an interrupt
            Any = 0x01,    // Any value outside of threshold range
            _2 = 0x02,      // 2 consecutive values out of range
            _3 = 0x03,      // 3 consecutive values out of range
            _5 = 0x04,      // 5 consecutive values out of range
            _10 = 0x05,     // 10 consecutive values out of range
            _15 = 0x06,     // 15 consecutive values out of range
            _20 = 0x07,     // 20 consecutive values out of range
            _25 = 0x08,     // 25 consecutive values out of range
            _30 = 0x09,     // 30 consecutive values out of range
            _35 = 0x0A,     // 35 consecutive values out of range
            _40 = 0x0B,     // 40 consecutive values out of range
            _45 = 0x0C,     // 45 consecutive values out of range
            _50 = 0x0D,     // 50 consecutive values out of range
            _55 = 0x0E,     // 55 consecutive values out of range
            _60 = 0x0F      // 60 consecutive values out of range
        }

        private const float LuxDf = 408.0F;
        private const float LuxCoefb = 1.64F; // CH0 coefficient 
        private const float LuxCoefc = 0.59F; // CH1 coefficient A
        private const float LuxCoefd = 0.86F; // CH2 coefficient B 
        #endregion
    }
}
