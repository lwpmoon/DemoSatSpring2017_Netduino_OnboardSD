using System;
using System.Threading;
using DemoSatSpring2017Netduino_OnboardSD.Drivers;
using DemoSatSpring2017Netduino_OnboardSD.Flight_Computer;
using Microsoft.SPOT;

namespace DemoSatSpring2017Netduino_OnboardSD.Work_Items
{
    class LightSensorUpdater
    {
        private readonly Tsl2591 _lightSensor;

        private readonly WorkItem _workItem;
        private readonly byte[] _dataArray;
        private readonly int _dataCount = 4; // Todo: Update data counts
        private readonly int _metaDataCount = 2;
        private readonly int _timeDataCount = 3;
        private readonly int _delay;

        /// <summary>
        /// Light sensor control loop.
        /// </summary>
        /// <param name="delay">Exexecution interval</param>
        public LightSensorUpdater(int delay = 1000)
        {
            _dataArray = new byte[_dataCount + _metaDataCount + _timeDataCount];
            _dataArray[0] = (byte)PacketType.StartByte; // start bit = 0xff
            _dataArray[1] = (byte) PacketType.LuminosityUpdate;

            _lightSensor = new Tsl2591();

            _delay = delay;


            _workItem = new WorkItem(TSL2591Control, ref _dataArray, true, true, true);
            Rebug.Print("[SUCCESS] Luminosity sensor and periodic update initialized.");
        }

        private void TSL2591Control()
        {
            var time = BitConverter.GetBytes(Clock.Instance.ElapsedMilliseconds);

            var lum = _lightSensor.GetFullLuminosity();

            var lumBytes = BitConverter.GetBytes(lum);

            //Rebug.Print("Lum: " + lum);

            var dataIndex = _metaDataCount;

            //time
            _dataArray[dataIndex++] = time[0];
            _dataArray[dataIndex++] = time[1];
            _dataArray[dataIndex++] = time[2];

            //data
            _dataArray[dataIndex++] = lumBytes[0];
            _dataArray[dataIndex++] = lumBytes[1];
            _dataArray[dataIndex++] = lumBytes[2];
            _dataArray[dataIndex] = lumBytes[3];


            Thread.Sleep(_delay);
        }

        public void Start()
        {
            _workItem.Start();
            Rebug.Print("[SUCCESS] Luminosity update started.");

        }
    }
}
