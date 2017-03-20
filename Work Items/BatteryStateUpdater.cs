using System;
using System.Threading;
using DemoSatSpring2017Netduino_OnboardSD.Drivers;
using DemoSatSpring2017Netduino_OnboardSD.Flight_Computer;
using Microsoft.SPOT;

namespace DemoSatSpring2017Netduino_OnboardSD.Work_Items
{
    class BatteryStateUpdater
    {
        private readonly LiPoFuelGauge _batterySensor;
        private readonly WorkItem _workItem;
        private readonly byte[] _dataArray;
        private readonly int _dataCount = 7; //3 + 2 + 2
        private readonly int _metaDataCount = 2;
        private readonly int _timeDataCount = 3;
        private readonly int _delay;
        public BatteryStateUpdater(I2CBus bus, int delay = 15000)
        {
            _batterySensor = new LiPoFuelGauge(bus);
            _delay = delay;
            _dataArray = new byte[_dataCount + _metaDataCount + _timeDataCount];

            _workItem = new WorkItem(BatteryUpdater, ref _dataArray, true, true, true);
        }

        private void BatteryUpdater()
        {
            var dataIndex = _metaDataCount;

            var time = BitConverter.GetBytes(Clock.Instance.ElapsedMilliseconds);

            var voltage = _batterySensor.GetVCell();
            var percent = _batterySensor.GetSoC();

            Debug.Print("BatteryState...");
            Debug.Print("Voltage: " + voltage);
            Debug.Print("Percent: " + percent);

            _dataArray[dataIndex++] = time[0];
            _dataArray[dataIndex++] = time[1];
            _dataArray[dataIndex++] = time[2];

            //add battery voltage data (can be unsigned, less than 65536) (2 bytes)
            voltage = (ushort)voltage;
            _dataArray[dataIndex++] = (byte)(((short)voltage >> 8) & 0xFF);
            _dataArray[dataIndex] = (byte)((short)voltage & 0xFF);

            //add battery percent remaining data (can be unsigned, less than 65536) (2 bytes)
            percent = (ushort)percent;
            _dataArray[dataIndex++] = (byte)(((short)percent >> 8) & 0xFF);
            _dataArray[dataIndex] = (byte)((short)percent & 0xFF);

            Array.Copy(_dataArray, _workItem.PacketData, _dataArray.Length);
            Thread.Sleep(_delay);
        }

        public void Start()
        {
            _workItem.Start();
        }
    }
}
