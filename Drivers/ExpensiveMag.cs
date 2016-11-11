using System.IO.Ports;
using System.Threading;
using DemoSat2016Netduino_OnboardSD.Work_Items;

namespace DemoSat2016Netduino_OnboardSD.Drivers {
    public class ExpensiveMag {
        private readonly SerialPort _comPort;

        public ExpensiveMag(string comPort, int readTimeOut, int writeTimeOut) {
            _comPort = new SerialPort(comPort,Baud,Parity.None,8,StopBits.One) {
                ReadTimeout = readTimeOut,
                WriteTimeout = writeTimeOut,
                Handshake = Handshake.None
            };
            
        }

        public bool Begin() {
            Rebug.Print("Starting up expensive mag!");
            _comPort.Open();
            return true;
        }

        public byte[] Update() {

            _comPort.Flush();
            var command = new[] {(byte) 128};
            _comPort.Write(command, 0, command.Length);
            Thread.Sleep(1);

            var data = new byte[16];
            _comPort.Read(data, 0, data.Length);
            //for (var j = 0; j < data.Length; j++) {
            //    data[j] = (byte)_comPort.ReadByte();
            //}

            return data;
        }
        
        const int Baud = 9600;
    }
}