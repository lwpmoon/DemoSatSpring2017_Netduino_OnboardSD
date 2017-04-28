using System;
using System.Threading;
using DemoSatSpring2017Netduino_OnboardSD.Drivers;
using DemoSatSpring2017Netduino_OnboardSD.Flight_Computer;
using Microsoft.SPOT;
using Math = System.Math;

namespace DemoSatSpring2017Netduino_OnboardSD.Work_Items
{

    class HeaterUpdater
    {
        private readonly  HeaterControler _heatercontroler;

        private readonly WorkItem _workItem;
        private readonly byte[] _dataArray;
        private readonly int _dataCount = 13; //8 + 2 + 2  Todo: Confirm data
        private readonly int _metaDataCount = 2;
        private readonly int _timeDataCount = 3;
        private readonly int _delay;

        /// <summary>
        /// Heater control loop.
        /// </summary>
        /// <param name="delay">Exexecution interval</param>
        public HeaterUpdater(int delay = 5000)
        {
            _dataArray = new byte[_dataCount + _metaDataCount + _timeDataCount];
            _dataArray[0] = (byte)PacketType.StartByte; // start bit = 0xff
            _dataArray[1] = (byte)PacketType.HeatDump;

            _heatercontroler = new HeaterControler();

            _delay = delay;
            

            _workItem = new WorkItem(HeaterControl, ref _dataArray, true, true, true);
            Rebug.Print("[SUCCESS] Heater controller initialized.");
        }

        private void HeaterControl()
        {
            //_heatercontroler.Update();
            var time = BitConverter.GetBytes(Clock.Instance.ElapsedMilliseconds);
            var heaterTemp = _heatercontroler.GetHeaterTemp();
            var temperature = _heatercontroler.GetTemp();
            _heatercontroler.SetHeater(temperature, heaterTemp);
            Rebug.Print("Heater: "+heaterTemp + "  Internal: "+ temperature);

            var dataIndex = _metaDataCount;

            _dataArray[dataIndex++] = time[0];
            _dataArray[dataIndex++] = time[1];
            _dataArray[dataIndex++] = time[2];

            var tempBytes = BitConverter.GetBytes(temperature);
            //add temp data (can be unsigned, less than 65536) (2 bytes)
            _dataArray[dataIndex++] = tempBytes[0];
            _dataArray[dataIndex++] = tempBytes[1];
            //_dataArray[dataIndex++] = (byte)(((short)temperature >> 8) & 0xFF);
            //_dataArray[dataIndex] = (byte)((short)temperature & 0xFF);

            //add battery percent remaining data (can be unsigned, less than 65536) (2 bytes)
            var heaterTempBytes = BitConverter.GetBytes(heaterTemp);
            _dataArray[dataIndex++] = heaterTempBytes[0];
            _dataArray[dataIndex] = heaterTempBytes[1];
            //_dataArray[dataIndex++] = (byte)(((short)heaterTemp >> 8) & 0xFF);
            //_dataArray[dataIndex] = (byte)((short)heaterTemp & 0xFF);

            Array.Copy(_dataArray, _workItem.PacketData, _dataArray.Length);
            Thread.Sleep(_delay);
        }

        public void Start()
        {
            _workItem.Start();
            Rebug.Print("[SUCCESS] Heater controller started.");
        }
    }
}
