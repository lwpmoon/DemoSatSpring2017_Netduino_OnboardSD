using System.Threading;
using DemoSat2016Netduino_OnboardSD.Drivers;
using DemoSat2016Netduino_OnboardSD.Flight_Computer;
using DemoSat2016Netduino_OnboardSD.Work_Items;
using MicroLiquidCrystal;
using Microsoft.SPOT;

namespace DemoSat2016Netduino_OnboardSD {
    public class BNOCalibUpdate {
        private readonly SerialBno _bno;
        private readonly int _delay;
        private readonly WorkItem _workItem;
        private readonly byte[] _dataArray = new []{(byte)0};
        private readonly Lcd _lcd;

        public BNOCalibUpdate(SerialBno bno, Lcd lcd, int delay) {
            _lcd = lcd;
            _bno = bno;
            _delay = delay;
            _workItem = new WorkItem(UpdateCalib, ref _dataArray, loggable:false, persistent:true, pauseable:true);
        }

        private void UpdateCalib() {

            var time = Clock.Instance.ElapsedMilliseconds/1000;
            var test = _bno.GetCalibration();
            _lcd.Clear();
            _lcd.Write("S:" + test[0] + " G:" + test[1] + " A:" + test[2] + " M:" + test[3]);
            _lcd.SetCursorPosition(0,1);
            _lcd.Write("T+ " + time + " s");
            Thread.Sleep(_delay);
        }

        public void Start() {
            _workItem.Start();
        }
    }
}