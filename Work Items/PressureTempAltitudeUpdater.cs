using System;
using System.Threading;
using DemoSatSpring2017Netduino_OnboardSD.Drivers;
using DemoSatSpring2017Netduino_OnboardSD.Flight_Computer;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Math = System.Math;

namespace DemoSatSpring2017Netduino_OnboardSD.Work_Items {
    public class PressureTempAltitudeUpdater {
        private readonly Bmp280 _bmpSensor;

        private readonly WorkItem _workItem;
        private readonly byte[] _dataArray;
        private readonly int _dataCount = 13; //8 + 3 + 2
        private readonly int _metaDataCount = 2;
        private readonly int _timeDataCount = 3;
        private readonly int _delay;
        private readonly int _precision;

        //Default value provided by the Adafruit library. Update the day of launch for accuracy.

        public PressureTempAltitudeUpdater(int sigFigs = 4, int delay = 30000, float seaLevelhPa = 1013.25f) {
            _bmpSensor = new Bmp280(seaLevelhPa);
            _dataArray = new byte[_dataCount + _metaDataCount + _timeDataCount];
            _dataArray[0] = (byte)PacketType.StartByte; // start bit = 0xff
            _dataArray[1] = (byte)PacketType.BmpDump;

            _delay = delay;
            _precision = (int) Math.Pow(10, sigFigs - 1);

            _workItem = new WorkItem(BmpUpdater, ref _dataArray, true, true, true);

            Rebug.Print("[SUCCESS] BMP280 Sensor and Updator initialized.");
        }

        private void BmpUpdater() {

            var dataIndex = _metaDataCount;

            var time = BitConverter.GetBytes(Clock.Instance.ElapsedMilliseconds);

            _dataArray[dataIndex++] = time[0];
            _dataArray[dataIndex++] = time[1];
            _dataArray[dataIndex++] = time[2];

            
            _bmpSensor.UpdateMeasurements();

            //Debug.Print("Pres: " + _bmpSensor.Pressure + " Pa");//Should be ~82,000 Pascal...
            //Debug.Print("Temp: " + _bmpSensor.Temp + " *C");//Should be ~20 Celsius...
            //Debug.Print("Alt: " + _bmpSensor.Altitude + " m");//Should be ~1760 meters...
            
            //add pressure to data array (4 bytes)
            var pressureBytes = BitConverter.GetBytes(_bmpSensor.Pressure);
            for (int i = 0; i < 8; i++) {
                _dataArray[dataIndex++] = pressureBytes[i];
            }

            //add temp data (needs sign) to data (3 bytes)
            _dataArray[dataIndex++] = (_bmpSensor.Temp < 0 ? (byte)1 : (byte)0);
            var uTemp = (float)Math.Abs(_bmpSensor.Temp);
            _dataArray[dataIndex++] = (byte)(((short)uTemp >> 8) & 0xFF);
            _dataArray[dataIndex++] = (byte)((short)uTemp & 0xFF);

            //add altitude data (can be unsigned, less than 65536) (2 bytes)
            var altitude = (ushort) _bmpSensor.Altitude;
            _dataArray[dataIndex++] = (byte)(((short)altitude >> 8) & 0xFF);
            _dataArray[dataIndex] = (byte)((short)altitude & 0xFF);

            Array.Copy(_dataArray, _workItem.PacketData, _dataArray.Length);
            
            Thread.Sleep(_delay);
        }

        public void Start() {
            _workItem.Start();
            Rebug.Print("[SUCCESS] BMP280 sensor update started.");
        }
    }
}