using System;
using System.Threading;
using DemoSat2016Netduino_OnboardSD.Drivers;
using DemoSat2016Netduino_OnboardSD.Flight_Computer;
using Microsoft.SPOT.Hardware;
using test_bot_netduino;

namespace DemoSat2016Netduino_OnboardSD.Work_Items {
    public class PressureTempAltitudeUpdater {
        private readonly Bmp180 _bmpSensor;

        private readonly WorkItem _workItem;
        private readonly byte[] _dataArray;
        private readonly int _dataCount = 13; //8 + 3 + 2
        private readonly int _metaDataCount = 2;
        private readonly int _timeDataCount = 3;
        private readonly int _delay;
        private readonly int _precision;

        public PressureTempAltitudeUpdater(I2CBus bus, int sigFigs = 4, int delay = 30000) {
            _bmpSensor = new Bmp180(bus);
            _dataArray = new byte[_dataCount + _metaDataCount + _timeDataCount];
            _dataArray[0] = (byte)PacketType.StartByte; // start bit = 0xff
            _dataArray[1] = (byte)PacketType.BmpDump;

            _delay = delay;
            _precision = (int) Math.Pow(10, sigFigs - 1);

            _workItem = new WorkItem(BmpUpdater, ref _dataArray, true, true, true);

            _bmpSensor.Init();
        }

        private void BmpUpdater() {

            var dataIndex = _metaDataCount;

            var time = BitConverter.GetBytes(Clock.Instance.ElapsedMilliseconds);

            _dataArray[dataIndex++] = time[0];
            _dataArray[dataIndex++] = time[1];
            _dataArray[dataIndex++] = time[2];

            var pressure = _bmpSensor.GetPressure();
            var temp = _bmpSensor.GetTemperature() * _precision; //precision because 4 sig figs go into decimals.
            var altitude = Bmp180.PressureToAltitude(Bmp180.SensorsPressureSealevelhpa, pressure, temp);

            //add pressure to data array (8 bytes)
            var pressureBytes = BitConverter.GetBytes(pressure);
            for (int i = 0; i < 8; i++) {
                _dataArray[dataIndex++] = pressureBytes[i];
            }

            //add temp data (needs sign) to data (3 bytes)
            _dataArray[dataIndex++] = (temp < 0 ? (byte)1 : (byte)0);
            temp = (float)Math.Abs(temp);
            _dataArray[dataIndex++] = (byte)(((short)temp >> 8) & 0xFF);
            _dataArray[dataIndex++] = (byte)((short)temp & 0xFF);

            //add altitude data (can be unsigned, less than 65536) (2 bytes)
            altitude = (ushort) altitude;
            _dataArray[dataIndex++] = (byte)(((short)altitude >> 8) & 0xFF);
            _dataArray[dataIndex] = (byte)((short)altitude & 0xFF);

            Array.Copy(_dataArray, _workItem.PacketData, _dataArray.Length);
            Thread.Sleep(_delay);
        }

        public void Start() {
            _workItem.Start();
        }
    }
}