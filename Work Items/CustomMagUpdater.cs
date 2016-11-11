using System;
using DemoSat2016Netduino_OnboardSD.Flight_Computer;
using Microsoft.SPOT.Hardware;

namespace DemoSat2016Netduino_OnboardSD.Work_Items
{
    public class CustomMagUpdater {
        private static AnalogInput _magPin;

        private readonly WorkItem _workItem;
        private readonly byte[] _dataArray;
        private readonly int _dataCount;

        private const int MetaDataCount = 2;
        private const int TimeDataCount = 6;

        public CustomMagUpdater(int dataCount, Cpu.AnalogChannel magPin)
        {
            _magPin = new AnalogInput(magPin);
            _dataCount = dataCount;
            Rebug.Print("Initializing high frequency custom magnetometer update cycle");
            _dataArray = new byte[dataCount + MetaDataCount + TimeDataCount];
            _workItem = new WorkItem(DumpMagData, ref _dataArray, loggable: true, persistent: true, pauseable: true);

            _dataArray[0] = (byte)PacketType.StartByte;
            _dataArray[1] = (byte)PacketType.FMagDump;
        }

        private void DumpMagData()
        {
            var currentDataIndex = MetaDataCount;

            var time = BitConverter.GetBytes(Clock.Instance.ElapsedMilliseconds);
            _dataArray[currentDataIndex++] = time[0];
            _dataArray[currentDataIndex++] = time[1];
            _dataArray[currentDataIndex++] = time[2];

            for (var i = 0; i < _dataCount / 2; i++)
            {
                var raw = (short)(_magPin.Read());
                var msb = (byte)((raw >> 8) & 0xFF);
                var lsb = (byte)(raw & 0xff);

                _dataArray[currentDataIndex++] = msb;
                _dataArray[currentDataIndex++] = lsb;
            }
        }

        public void Start()
        {
            _workItem.Start();
        }
    }
}