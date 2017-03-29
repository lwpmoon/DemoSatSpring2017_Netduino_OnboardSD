using System;
using System.Threading;
using DemoSatSpring2017Netduino_OnboardSD.Drivers;
using Microsoft.SPOT;
using Math = System.Math;

namespace DemoSatSpring2017Netduino_OnboardSD.Work_Items
{
    class HeaterUpdater
    {
        private readonly  HeaterControler _heatercontroler;

        private readonly WorkItem _workItem;
        private readonly byte[] _dataArray;
        private readonly int _dataCount = 13; //8 + 3 + 2  Todo: Update this later
        private readonly int _metaDataCount = 2;
        private readonly int _timeDataCount = 3;
        private readonly int _delay;
        private readonly int _precision;

        public HeaterUpdater(int sigFigs = 4, int delay = 500)
        {
            _dataArray = new byte[_dataCount + _metaDataCount + _timeDataCount];
            
            _heatercontroler = new HeaterControler();

            _delay = delay;
            _precision = (int)Math.Pow(10, sigFigs - 1);

            _workItem = new WorkItem(HeaterControl, true, true, true);
        }

        private void HeaterControl()
        {
            _heatercontroler.Update();
            Thread.Sleep(_delay);
        }

        public void Start()
        {
            _workItem.Start();
        }
    }
}
