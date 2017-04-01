using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Math = System.Math;

namespace DemoSatSpring2017Netduino_OnboardSD.Drivers
{
    class Tsl2591
    {
        private enum Registers : byte
        {
            ENABLE = 0x00,
            CONTROL = 0x01,
            THRESHOLD_AILTL = 0x04, // ALS low threshold lower byte
            THRESHOLD_AILTH = 0x05, // ALS low threshold upper byte
            THRESHOLD_AIHTL = 0x06, // ALS high threshold lower byte
            THRESHOLD_AIHTH = 0x07, // ALS high threshold upper byte
            THRESHOLD_NPAILTL = 0x08, // No Persist ALS low threshold lower byte
            THRESHOLD_NPAILTH = 0x09, // etc
            THRESHOLD_NPAIHTL = 0x0A,
            THRESHOLD_NPAIHTH = 0x0B,
            PERSIST_FILTER = 0x0C,
            PACKAGE_PID = 0x11,
            DEVICE_ID = 0x12,
            DEVICE_STATUS = 0x13,
            CHAN0_LOW = 0x14,
            CHAN0_HIGH = 0x15,
            CHAN1_LOW = 0x16,
            CHAN1_HIGH = 0x17
        }

        private enum Bits : byte
        {
            READBIT = 0x01,
            COMMAND_BIT = 0xA0, // 1010 0000: bits 7 and 5 for 'command normal'
            CLEAR_INT = 0xE7,
            TEST_INT = 0xE4,
            WORD_BIT = 0x20,    // 1 = read/write word (rather than byte)
            BLOCK_BIT = 0x10    // 1 = using block read/write
        }

        private enum Enable : byte
        {
            POWEROFF = 0x00,
            POWERON = 0x01,
            AEN = 0x02,    // ALS Enable. This field activates ALS function. Writing a one activates the ALS. Writing a zero disables the ALS.
            AIEN = 0x10,    // ALS Interrupt Enable. When asserted permits ALS interrupts to be generated, subject to the persist filter.
            NPIEN =0x80    // No Persist Interrupt Enable. When asserted NP Threshold conditions will generate an interrupt, bypassing the persist filter
        }

        private enum Channels
        {
            FULLSPECTRUM = 0,
            INFRARED = 1, // channel 1
            VISIBLE = 2 // channel 0 - channel 1
        }

        public enum Gain : byte
        {
            LOW = 0x00,     // low gain (1x)
            MED = 0x10,     // medium gain (25x)
            HIGH = 0x20,    // medium gain (428x)
            MAX = 0x30      // max gain (9876x)
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
            _EVERY = 0x00,  // Every ALS cycle generates an interrupt
            _ANY = 0x01,    // Any value outside of threshold range
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

        private const float LUX_DF = 408.0F;
        private const float LUX_COEFB = 1.64F; // CH0 coefficient 
        private const float LUX_COEFC = 0.59F; // CH1 coefficient A
        private const float LUX_COEFD = 0.86F; // CH2 coefficient B

        private readonly byte _tsl2591Address = 0x29;
        //private Coefficients _coefficients;
        private bool _initialized;
        private Gain _gain;
        private Integrationtime _integration;


        private readonly I2CDevice.Configuration _slaveConfig;
        private I2CBus _bus;
        private const int TransactionTimeout = 1000; // ms
        private const int ClockSpeed = Program.I2CclockSpeed;
        
        public Tsl2591(I2CBus bus, byte address = 0x29, Gain gain = Gain.MED, Integrationtime inte = Integrationtime._100MS)
        {
            _bus = bus;
            _tsl2591Address = address;
            _slaveConfig = new I2CDevice.Configuration(_tsl2591Address, ClockSpeed);
            _initialized = false;

            _integration = inte;
            _gain = gain;

            while (!Init(gain, inte))
            {

                Debug.Print("TSL2591 Light Sensor not detected...");
            }

            SetCoefficents();
        }

        public bool Init(Gain gain, Integrationtime integrationtime)
        {
            byte[] whoami = { 0 };

            _bus.WriteRead(_slaveConfig, new[] { (byte)Registers.DEVICE_ID }, whoami, TransactionTimeout);

            if (whoami[0] != 0x50) return false;

            _bus.Write(_slaveConfig, new []{(byte) Bits.COMMAND_BIT, (byte) gain, (byte) integrationtime}, TransactionTimeout);

            return true;
        }

        private void enable()
        {
            if (!_initialized)
            {
                if (!(Init(_gain, _integration)))
                {
                    return;
                }
            }

            // Enable the device by setting the control bit to 0x01
            _bus.Write(_slaveConfig,new []{ (byte)Bits.COMMAND_BIT, (byte) Registers.ENABLE, (byte) Enable.POWERON, (byte) Enable.AEN, (byte) Enable.AIEN, (byte) Enable.NPIEN}, TransactionTimeout);
        }

        private void disable()
        {
            if (!_initialized)
            {
                if (!(Init(_gain, _integration)))
                {
                    return;
                }
            }

            // Disable the device by setting the control bit to 0x00
            //write8(TSL2591_COMMAND_BIT | TSL2591_REGISTER_ENABLE, TSL2591_ENABLE_POWEROFF);
            _bus.Write(_slaveConfig, new []{(byte) Bits.COMMAND_BIT, (byte) Registers.ENABLE, (byte) Enable.POWEROFF}, TransactionTimeout);
        }

        private void SetCoefficents()
        {
            
        }

        ulong calculateLux(ushort ch0, ushort ch1)
        {
            float atime, again;
            float cpl, lux1, lux2, lux;
            ulong chan0, chan1;

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
                case Gain.LOW:
                    again = 1.0F;
                    break;
                case Gain.MED:
                    again = 25.0F;
                    break;
                case Gain.HIGH:
                    again = 428.0F;
                    break;
                case Gain.MAX:
                    again = 9876.0F;
                    break;
                default:
                    again = 1.0F;
                    break;
            }

            // cpl = (ATIME * AGAIN) / DF
            cpl = (atime*again)/ LUX_DF;//TSL2591_LUX_DF;

            lux1 = ((float)ch0 - (LUX_COEFB * (float)ch1)) / cpl;
            lux2 = ((LUX_COEFC * (float)ch0) - (LUX_COEFD * (float)ch1)) / cpl;
            lux = lux1 > lux2 ? lux1 : lux2;

            // Alternate lux calculation
            //lux = ( (float)ch0 - ( 1.7F * (float)ch1 ) ) / cpl;

            // Signal I2C had no errors
            return (ulong)lux;
        }

        /*
        ulong getFullLuminosity()
        {
            if (!_initialized)
            {
                if (!(Init(_gain, _integration)))
                {
                    return 0;
                }
            }

            // Enable the device
            enable();

            // Wait x ms for ADC to complete
            for (var d = 0; d <= 100; d++)
            {
                Thread.Sleep(120);
            }

            byte[] x = new byte[1];
            byte holder;
            //x = read16(TSL2591_COMMAND_BIT | TSL2591_REGISTER_CHAN1_LOW);
            _bus.WriteRead(_slaveConfig, new[] {(byte) Bits.COMMAND_BIT, (byte) Registers.CHAN1_LOW}, x,
                TransactionTimeout);
            holder = (x << 16);
            //x |= read16(TSL2591_COMMAND_BIT | TSL2591_REGISTER_CHAN0_LOW);

            disable();

            return x;
        }

        uint16_t Adafruit_TSL2591::getLuminosity(uint8_t channel)
        {
            uint32_t x = getFullLuminosity();

            if (channel == TSL2591_FULLSPECTRUM)
            {
                // Reads two byte value from channel 0 (visible + infrared)
                return (x & 0xFFFF);
            }
            else if (channel == TSL2591_INFRARED)
            {
                // Reads two byte value from channel 1 (infrared)
                return (x >> 16);
            }
            else if (channel == TSL2591_VISIBLE)
            {
                // Reads all and subtracts out just the visible!
                return ((x & 0xFFFF) - (x >> 16));
            }

            // unknown channel!
            return 0;
        }

        void Adafruit_TSL2591::registerInterrupt(uint16_t lowerThreshold, uint16_t upperThreshold)
        {
            if (!_initialized)
            {
                if (!begin())
                {
                    return;
                }
            }

            enable();
            write8(TSL2591_COMMAND_BIT | TSL2591_REGISTER_THRESHOLD_NPAILTL, lowerThreshold);
            write8(TSL2591_COMMAND_BIT | TSL2591_REGISTER_THRESHOLD_NPAILTH, lowerThreshold >> 8);
            write8(TSL2591_COMMAND_BIT | TSL2591_REGISTER_THRESHOLD_NPAIHTL, upperThreshold);
            write8(TSL2591_COMMAND_BIT | TSL2591_REGISTER_THRESHOLD_NPAIHTH, upperThreshold >> 8);
            disable();
        }

        void Adafruit_TSL2591::registerInterrupt(uint16_t lowerThreshold, uint16_t upperThreshold, tsl2591Persist_t persist)
        {
            if (!_initialized)
            {
                if (!begin())
                {
                    return;
                }
            }

            enable();
            write8(TSL2591_COMMAND_BIT | TSL2591_REGISTER_PERSIST_FILTER, persist);
            write8(TSL2591_COMMAND_BIT | TSL2591_REGISTER_THRESHOLD_AILTL, lowerThreshold);
            write8(TSL2591_COMMAND_BIT | TSL2591_REGISTER_THRESHOLD_AILTH, lowerThreshold >> 8);
            write8(TSL2591_COMMAND_BIT | TSL2591_REGISTER_THRESHOLD_AIHTL, upperThreshold);
            write8(TSL2591_COMMAND_BIT | TSL2591_REGISTER_THRESHOLD_AIHTH, upperThreshold >> 8);
            disable();
        }

        public void clearInterrupt()
        {
            if (!_initialized)
            {
                if (!(Init(_gain, _integration)))
                {
                    return;
                }
            }

            enable();
            write8(TSL2591_CLEAR_INT);
            disable();
        }


        int getStatus()
        {
            if (!_initialized)
            {
                if (!(Init(_gain, _integration)))
                {
                    return 0;
                }
            }

            // Enable the device
            //enable();
            byte[] x = new byte[1];
            //x = read8(TSL2591_COMMAND_BIT | TSL2591_REGISTER_DEVICE_STATUS);
            _bus.WriteRead(_slaveConfig,new []{(byte)Bits.COMMAND_BIT}, x, TransactionTimeout);
            //disable();
            return x;
        }

        bool getEvent(sensors_event_t*event)
        {
            uint16_t ir, full;
            uint32_t lum = getFullLuminosity();
            /* Early silicon seems to have issues when there is a sudden jump in */
            /* light levels. :( To work around this for now sample the sensor 2x */
            /*lum = getFullLuminosity();
            ir = lum >> 16;
            full = lum & 0xFFFF;

            /* Clear the event */
            /*memset(event, 0, sizeof(sensors_event_t));

            event->version   = sizeof(sensors_event_t);
            event->sensor_id = _sensorID;
            event->type      = SENSOR_TYPE_LIGHT;
            event->timestamp = millis();

            /* Calculate the actual lux value */
            /* 0 = sensor overflow (too much light) */
            /*event->light = calculateLux(full, ir);

            return true;*/
        //}
    }
}
