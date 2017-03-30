using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace DemoSatSpring2017Netduino_OnboardSD.Drivers
{
    public class I2CBus : IDisposable
    {
        /*public static I2CBus _instance = null;
        public static readonly object locker = new object();*/

        public static I2CBus Instance {
            get {
                lock (Locker)
                {
                    return _instance ?? (_instance = new I2CBus());
                }
            } 
            
        }

        private static I2CBus _instance = null;
        private static readonly object Locker = new object();



        //private readonly I2CDevice _slaveDevice;
        private readonly I2CDevice _slaveDevice;

        public I2CBus() {
            _slaveDevice = new I2CDevice(new I2CDevice.Configuration(0, 0));
        }

        public void Dispose() {
            _slaveDevice.Dispose();
        }

        public void WriteRead(I2CDevice.Configuration config, byte[] writeBuffer, byte[] readBuffer,
            int transactionTimeout)
        {
            // Set i2c device configuration.
            _slaveDevice.Config = config;

            // create an i2c write transaction to be sent to the device.
            var writeXAction = new I2CDevice.I2CTransaction[] {I2CDevice.CreateWriteTransaction(writeBuffer)};

            lock (Locker)
            {
                // the i2c data is sent here to the device.
                var transferred = _slaveDevice.Execute(writeXAction, transactionTimeout);

                // make sure the data was sent.
                if (transferred != writeBuffer.Length)
                    Debug.Print("Could not write to I2C Device!");
            }

            // Set i2c device configuration.
            _slaveDevice.Config = config;

            // create an i2c read transaction to be sent to the device.
            var readXAction = new I2CDevice.I2CTransaction[] {I2CDevice.CreateReadTransaction(readBuffer)};

            lock (Locker)
            {
                // the i2c data is received here from the device.
                var transferred = _slaveDevice.Execute(readXAction, transactionTimeout);

                // make sure the data was received.
                if (transferred != readBuffer.Length)
                    Debug.Print("Could not read from I2C Device!");
            }
        }

        public void Write(I2CDevice.Configuration config, byte[] writeBuffer, int transactionTimeout)
        {
            // Set i2c device configuration.
            _slaveDevice.Config = config;

            // create an i2c write transaction to be sent to the device.
            var writeXAction = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(writeBuffer) };

            lock (Locker) {
                // the i2c data is sent here to the device.
                var transferred = _slaveDevice.Execute(writeXAction, transactionTimeout);

                // make sure the data was sent.
                if (transferred != writeBuffer.Length)
                    Debug.Print("Could not write to I2C Device!");
            }
        }

        public void Read(I2CDevice.Configuration config, byte[] readBuffer, int transactionTimeout)
        {
            // Set i2c device configuration.
            _slaveDevice.Config = config;

            // create an i2c read transaction to be sent to the device.
            var readXAction = new I2CDevice.I2CTransaction[] { I2CDevice.CreateReadTransaction(readBuffer) };

            lock (Locker)
            {
                // the i2c data is received here from the device.
                var transferred = _slaveDevice.Execute(readXAction, transactionTimeout);

                // make sure the data was received.
                if (transferred != readBuffer.Length)
                    //throw new Exception("Could not read from device.");
                    Debug.Print("Could not read from I2C Device!");
            }
        }

      
        public void ReadRegister(I2CDevice.Configuration config, byte register, byte[] readBuffer, int transactionTimeout)
        {
            byte[] registerBuffer = { register };
            Write(config, registerBuffer, transactionTimeout);
            Read(config, readBuffer, transactionTimeout);
        }

     
        public void WriteRegister(I2CDevice.Configuration config, byte register, byte[] writeBuffer, int transactionTimeout)
        {
            byte[] registerBuffer = { register };
            Write(config, registerBuffer, transactionTimeout);
            Write(config, writeBuffer, transactionTimeout);
        }

        public void WriteRegister(I2CDevice.Configuration config, byte register, byte value, int transactionTimeout)
        {
            byte[] writeBuffer = { register, value };
            Write(config, writeBuffer, transactionTimeout);
        }
    }
}