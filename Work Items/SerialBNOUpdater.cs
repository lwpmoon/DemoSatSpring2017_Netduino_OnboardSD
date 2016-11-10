using System;
using System.Collections;
using System.Threading;
using DemoSat2016Netduino_OnboardSD.Drivers;
using DemoSat2016Netduino_OnboardSD.Flight_Computer;
using DemoSat2016Netduino_OnboardSD.Utility;
using SecretLabs.NETMF.Hardware.Netduino;

namespace DemoSat2016Netduino_OnboardSD.Work_Items {
    public class SerialBnoUpdater {

        private readonly SerialBno _bnoSensor;

        //private BNOData _bnoData;

        private readonly WorkItem _workItem;
        private readonly byte[] _dataArray;
        private readonly int _dataCount = 58; //6*3*3(6vectors*3axis*3bytes/axis + 4(calib)
        private readonly int _metaDataCount = 2; //2 size, 1 start byte, 1 type byte
        private readonly int _timeDataCount = 3; //1 8 byte time stamp
        private readonly int _precision;
        private readonly int _delay;

        public SerialBnoUpdater(int sigFigs = 3, int delay = 30000) {


            _bnoSensor = new SerialBno(SerialPorts.COM3,5000,5000,SerialBno.Bno055OpMode.OperationModeNdof);

            _dataArray = new byte[_dataCount + _metaDataCount + _timeDataCount]; 
            _dataArray[0] = (byte)PacketType.StartByte; // start bit = 0xff
            _dataArray[1] = (byte)PacketType.BnoDump;

            _delay = delay;
            _precision = (int)System.Math.Pow(10, sigFigs - 1);
            
            _workItem = new WorkItem(BnoUpdater, ref _dataArray, loggable:true, persistent:true, pauseable:true);

            _bnoSensor.Begin();
        }

        private void BnoUpdater()
        {
            
            var dataIndex = _metaDataCount;

            var time = BitConverter.GetBytes(Clock.Instance.ElapsedMilliseconds);

            _dataArray[dataIndex++] = time[0];
            _dataArray[dataIndex++] = time[1];
            _dataArray[dataIndex++] = time[2];
            
            var accelVec = _bnoSensor.read_vector(SerialBno.Bno055VectorType.VectorAccelerometer);
            var gravVec = _bnoSensor.read_vector(SerialBno.Bno055VectorType.VectorGravity);
            var gyroVec = _bnoSensor.read_vector(SerialBno.Bno055VectorType.VectorGyroscope);
            var linAccVec = _bnoSensor.read_vector(SerialBno.Bno055VectorType.VectorLinearaccel);
            var eulerVec = _bnoSensor.read_vector(SerialBno.Bno055VectorType.VectorEuler);
            var magVec = _bnoSensor.read_vector(SerialBno.Bno055VectorType.VectorMagnetometer);

            //3 bytes each component, 3 components each vector, 6 vectors 
            //= 54 bytes + 3 bytes for time + 2 bytes for metadata + 
            //4 bytes calib = 63 bytes per update, 58 bytes of sensor data (54 + 4)
            var test = new ArrayList {accelVec, gravVec, linAccVec, gyroVec, magVec, eulerVec};
            foreach (Vector vector in test) {
                for(var i = 0; i < 3; i++) {
                    var component = vector.InnerArray[i] * _precision;
                    _dataArray[dataIndex++] = (component < 0 ? (byte) 1 : (byte) 0);
                    component = (float) Math.Abs(component);
                    _dataArray[dataIndex++] = (byte)(((short)component >> 8) & 0xFF);
                    _dataArray[dataIndex++] = (byte)((short)component & 0xFF);
                }
            }
            var calib = _bnoSensor.GetCalibration();
            _dataArray[dataIndex++] = calib[0];
            _dataArray[dataIndex++] = calib[1];
            _dataArray[dataIndex++] = calib[2];
            _dataArray[dataIndex] = calib[3];
            Array.Copy(_dataArray, _workItem.PacketData, _dataArray.Length);

            Thread.Sleep(_delay);
        }

        public void Start() {
            _workItem.Start();
        }
        
    }
}