using System;
using DemoSatSpring2017Netduino_OnboardSD.Utility;
using Microsoft.SPOT.Hardware;

namespace DemoSatSpring2017Netduino_OnboardSD.Drivers
{
    class LiPoFuelGauge
    {
        private I2CBus _bus;
        private readonly I2CDevice.Configuration _slaveConfig;
        private const int TransactionTimeout = 1000;


        public LiPoFuelGauge(I2CBus bus, byte address = (byte) Registers.Max17043Address)
        {
            _bus = bus;
            _slaveConfig = new I2CDevice.Configuration(address, 100);
        }

        public double GetVCell()
        {
            byte[] data = new byte[2];
            
            _bus.ReadRegister(_slaveConfig, (byte) Registers.Soc, data, TransactionTimeout);
            byte msb = data[0];
            byte lsb = data[1];
            var value = (msb << 4) | (lsb >> 4);
            
            return Tools.map(value, 0x000, 0xFFF, 0, 50000) / 10000.0;
            
            //return value * 0.00125;
        }

        public double GetSoC()
        {
            byte[] data = new byte[2];

            _bus.ReadRegister(_slaveConfig, (byte)Registers.Soc, data, TransactionTimeout);
            byte msb = data[0];
            byte lsb = data[1];

            double holder  = lsb / 256.0;
            return msb + holder;
        }

        int GetVersion()
        {
            byte msb = 0;
            byte lsb = 0;

            //readRegister(VERSION_REGISTER, MSB, LSB);

            return (msb << 8) | lsb;
        }

        byte GetCompensateValue()
        {
            byte msb = 0;
            byte lsb = 0;

            ReadConfigRegister(ref msb, ref lsb);
            return msb;
        }

        /// <summary>
        /// Reset as if power had been removed.
        /// </summary>
        void Reset()
        {
            byte[] data = new byte[2];
            data[0] = 0x00;
            data[1] = 0x54;
            _bus.WriteRegister(_slaveConfig, (byte)Registers.Mode, data, TransactionTimeout);
        }

        /// <summary>
        /// Restart fuel-gauge calculations.
        /// </summary>
        void QuickStart()
        {
            byte[] data = new byte[2];
            data[0] = 0x40;
            data[1] = 0x00;
            _bus.WriteRegister(_slaveConfig, (byte) Registers.Mode, data, TransactionTimeout);
        }
        /// <summary>
        /// Reads current configuration of fuel-gauge
        /// </summary>
        /// <param name="msb"></param>
        /// <param name="lsb"></param>
        void ReadConfigRegister(ref byte msb, ref byte lsb)
        {
            if (msb != 0 || lsb != 0)throw new ApplicationException("Ref msb or lsb values were not 0 as expectd...");
            byte[] data = new byte[2];
            _bus.ReadRegister(_slaveConfig, (byte) Registers.Config, data, TransactionTimeout);
            msb = data[0];
            lsb = data[1];
        }

        private enum Registers : byte
        {
            Max17043Address = 0x36,
            /// <summary>
            /// Reports 12-bit A/D measurement of battery voltage.
            /// </summary>
            Vcell = 0x02,
            /// <summary>
            /// Reports 16-bit SOC result calculated by ModelGauge algorithm.
            /// </summary>
            Soc = 0x04,
            /// <summary>
            /// Sends special commands to the IC.
            /// </summary>
            Mode = 0x06,
            /// <summary>
            /// Returns IC version.
            /// </summary>
            Version = 0x08,
            /// <summary>
            /// Battery compensation. Adjusts IC performance based on application conditions.
            /// </summary>
            Config = 0x0C,
            /// <summary>
            /// Sends special commands to the IC.
            /// </summary>
            Command = 0xFE 
	}
    }
}
