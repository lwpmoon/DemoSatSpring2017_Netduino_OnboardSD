using System;
using System.Threading;
using DemoSat2016Netduino_OnboardSD.Drivers;
using DemoSat2016Netduino_OnboardSD.Flight_Computer;
using Microsoft.SPOT;
using SecretLabs.NETMF.Hardware.Netduino;

namespace DemoSat2016Netduino_OnboardSD.Work_Items {
    public class ExpensiveMagUpdater {

        private readonly ExpensiveMag _expensiveMag;

        private readonly WorkItem _workItem;
        private readonly byte[] _dataArray;
        private readonly int _dataCount = 16; //binary
        private readonly int _metaDataCount = 2; //2 size, 1 start byte, 1 type byte
        private readonly int _sizeDataCount = 0;
        private readonly int _timeDataCount = 3; //1 8 byte time stamp
        private readonly int _delay;

        public ExpensiveMagUpdater(int delay = 30000) {

            _expensiveMag = new ExpensiveMag(SerialPorts.COM3,5000,5000);
            _expensiveMag.Begin();

            _dataArray = new byte[_dataCount + _metaDataCount + _timeDataCount + _sizeDataCount];
            _dataArray[0] = (byte)PacketType.StartByte; // start bit = 0xff
            _dataArray[1] = (byte)PacketType.EMagDump;

            _delay = delay;

            _workItem = new WorkItem(OnTaskExecute, ref _dataArray, loggable:true, persistent:true, pauseable:true);
        }

        private void OnTaskExecute() {
            
            var dataIndex = _metaDataCount;

            var time = BitConverter.GetBytes(Clock.Instance.ElapsedMilliseconds);

            _dataArray[dataIndex++] = time[0];
            _dataArray[dataIndex++] = time[1];
            _dataArray[dataIndex++] = time[2];

            var data = _expensiveMag.Update();

            if (data.Length != 16) {
                Debug.Print("This should never happen... except for the beginning");
            }
            //size, just in case
            //_dataArray[dataIndex++] = (byte)data.Length;

            for (int i = 0; i < data.Length; i++) {
                _dataArray[dataIndex++] = data[i];
            }
            Array.Copy(_dataArray, _workItem.PacketData, _dataArray.Length);

            Thread.Sleep(_delay);

        }

        public void Start() {
            _workItem.Start();
        }
        
    }
}