using System;
using System.Threading;
using DemoSatSpring2017Netduino_OnboardSD.Drivers;
using DemoSatSpring2017Netduino_OnboardSD.Flight_Computer;
using Microsoft.SPOT;
using Math = System.Math;

namespace DemoSatSpring2017Netduino_OnboardSD.Work_Items
{
    class BatteryStateUpdater
    {
        private readonly LiPoFuelGauge _batterySensor;
        private readonly WorkItem _workItem;
        private readonly byte[] _dataArray;
        private readonly int _dataCount = 7; //3 + 2 + 2 Todo: Confirm data
        private readonly int _metaDataCount = 2;
        private readonly int _timeDataCount = 3;
        private readonly int _delay;
        public BatteryStateUpdater(int delay = 5000)
        {
            _batterySensor = new LiPoFuelGauge();
            _delay = delay;
            _dataArray = new byte[_dataCount + _metaDataCount + _timeDataCount];
            _dataArray[0] = (byte)PacketType.StartByte;
            _dataArray[1] = (byte)PacketType.BatDump;
            _workItem = new WorkItem(BatteryUpdater, ref _dataArray, true, true, true);

            while (!Init())
            {

                Rebug.Print("MAX17043 (LiPoFuelGauge) sensor not detected...");
            }

            _batterySensor.QuickStart();
            Rebug.Print("MAX17043 (LiPoFuelGauge) Initialized.");
        }

        public bool Init()
        {
            var version = _batterySensor.GetVersion();
            return (version == 3);
        }

        private void BatteryUpdater()
        {
            var dataIndex = _metaDataCount;

            var time = BitConverter.GetBytes(Clock.Instance.ElapsedMilliseconds);
            var voltage = _batterySensor.GetVCell();
            var percent = _batterySensor.GetSOC();

            //Debug.Print("BatteryState...");
            //Debug.Print("Voltage: " + voltage);
            //Debug.Print("Percent: " + percent);

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
