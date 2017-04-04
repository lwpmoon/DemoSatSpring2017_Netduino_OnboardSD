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
        private readonly int _dataCount = 13; //8 + 2 + 2  Todo: Update data counts
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
            //ToDo: Make dataArray for LightSensor dump

            _lightSensor = new Tsl2591();

            _delay = delay;


            _workItem = new WorkItem(TSL2591Control, false, true, true);
        }

        private void TSL2591Control()
        {
            var time = BitConverter.GetBytes(Clock.Instance.ElapsedMilliseconds);

            var lum = _lightSensor.GetFullLuminosity();

            Debug.Print("Lum: " + lum);

            var dataIndex = _metaDataCount;

            _dataArray[dataIndex++] = time[0];
            _dataArray[dataIndex++] = time[1];
            _dataArray[dataIndex++] = time[2];

            Thread.Sleep(_delay);
        }

        public void Start()
        {
            _workItem.Start();
        }
    }
}
