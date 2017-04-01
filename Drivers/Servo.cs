using System;
using System.Threading;
using Microsoft.SPOT.Hardware;

namespace DemoSatSpring2017Netduino_OnboardSD.Drivers
{
    public class ContServo : IDisposable
    {

        private readonly PWM _servo;
        private double _lastRecoveryAmount;

    
        public ContServo(Cpu.PWMChannel servoPin)
        {
            _servo = new PWM(servoPin, 20, 0, false);

            _servo.Start();


        }

        public void Recover(double amount) {

            var recoveryDifference = amount - _lastRecoveryAmount;
            if (Math.Abs(recoveryDifference) < 1) return;
            
            _servo.Duration = (uint) (recoveryDifference < 0 ? 2200 : 1100);
            _servo.Start();
            //how much different since the last recover?

            Thread.Sleep(Math.Abs((int)recoveryDifference) * 8);
            _servo.Stop();
            _lastRecoveryAmount = amount;

        }

        public void Clockwise() {
            _servo.Duration = 1300;
            _servo.Start();
        }

        public void CounterClockwise() {
            _servo.Duration = 2100;
            _servo.Start();
        }

        public void Stop() {
            _servo.Stop();
        }

      
        public void Dispose()
        {
            
        }
    }
    public class Servo : IDisposable
    {
        /// <summary>
        /// PWM handle
        /// </summary>
        private readonly PWM _servo;

        /// <summary>
        /// Timings range
        /// </summary>
        private readonly int[] _range = new int[2];

        /// <summary>
        /// Set servo inversion
        /// </summary>
        public bool Inverted = false;

        private int _current;

        /// <summary>
        /// Create the PWM Channel, set it low and configure timings
        /// </summary>
        /// <param name="channelPin"></param>
        /// <param name="initialPosition"></param>
        public Servo(Cpu.PWMChannel channelPin, double initialPosition)
        {
            // Init the PWM pin
            // servo = new PWM((Cpu.PWMChannel)channelPin, 20000, 1500, PWM.ScaleFactor.Microseconds, false);
            _servo = new PWM(channelPin, 20000, 1500, PWM.ScaleFactor.Microseconds, false)
            {
                Period = 20000
            };
            Degree = initialPosition;
            // Typical settings
            _range[0] = 500;
            _range[1] = 2100;
        }

        public void Dispose()
        {
            Disengage();
            _servo.Dispose();
        }

        /// <summary>
        /// Allow the user to set cutom timings
        /// </summary>
        /// <param name="fullLeft"></param>
        /// <param name="fullRight"></param>
        public void SetRange(int fullLeft, int fullRight)
        {
            _range[1] = fullLeft;
            _range[0] = fullRight;
        }

        /// <summary>
        /// Disengage the servo. 
        /// The servo motor will stop trying to maintain an angle
        /// </summary>
        public void Disengage()
        {
            // See what the Netduino team say about this... 
            _servo.DutyCycle = 0; //SetDutyCycle(0);
        }

        /// <summary>
        /// Set the servo degree
        /// </summary>
        public double Degree
        {
            get { return _current; }
            set
            {
                // Range checks
                if (value > 180)
                    value = 180;

                if (value < 0)
                    value = 0;

                // Are we inverted?
                if (Inverted)
                    value = 180 - value;

                // Set the pulse
                //servo.SetPulse(20000, (uint)map((long)value, 0, 180, range[0], range[1]));
                _servo.Duration = (uint)Map((long)value, 0, 180, _range[0], _range[1]);
                _servo.Start();
                _current = (int)value;
            }
        }


        private long Map(long x, long inMin, long inMax, long outMin, long outMax)
        {
            return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }
    }
    //    public class ServoController : IDisposable
    //    {
    //        /// <summary>
    //        /// PWM handle
    //        /// </summary>
    //        private PWM servo;

    //        /// <summary>
    //        /// Timings range
    //        /// </summary>      
    //        private int[] range = new int[2];

    //        /// <summary>
    //        /// Set servo inversion
    //        /// </summary>    
    //        public bool inverted = false;

    //        private double _current = 90;

    //        /// <summary>
    //        /// Create the PWM Channel, set it low and configure timings
    //        /// Changes here PWM Channel requires Channel, Period, Duration, 
    //        /// Scale and Inversion on instantiation.
    //        /// </summary>
    //        /// <param name="pin"></param>
    //        public ServoController(Cpu.PWMChannel pin)
    //        {
    //            // Initialize the PWM Channel, set to pin 5 as default.            
    //            servo = new PWM(
    //                pin,
    //                20000,
    //                1500,
    //                Microsoft.SPOT.Hardware.PWM.ScaleFactor.Microseconds,
    //                false);

    //            // Full range for FS90 servo is 0 - 3000.
    //            // For safety limits I set the default just above/below that.
    //            range[0] = 600;
    //            range[1] = 2400;
    //        }

    //        /// <summary>
    //        /// Allow for consumer to set own range.
    //        /// </summary>
    //        /// <param name="leftStop", "rightStop"></param>
    //        public void SetRange(int leftStop, int rightStop)
    //        {
    //            range[1] = leftStop;
    //            range[0] = rightStop;
    //        }

    //        /// <summary>
    //        /// Dispose implementation.
    //        /// </summary>
    //        public void Dispose()
    //        {
    //            Disengage();
    //            servo.Dispose();
    //        }

    //        /// <summary>
    //        /// Disengage the servo. 
    //        /// The servo motor will stop, and try to maintain an angle
    //        /// </summary>
    //        public void Disengage()
    //        {
    //            servo.DutyCycle = 0; //SetDutyCycle(0);
    //        }


    //        /// <summary>
    //        /// Set the servo degree
    //        /// </summary>
    //        public double Degree
    //        {
    //            get { return _current; }
    //            set
    //            {
    //                if (value > 180)
    //                    value = 180;

    //                if (value < 0)
    //                    value = 0;

    //                // Are we inverted?
    //                if (inverted)
    //                    value = 180 - value;

    //                // Set duration "pulse" and Start the servo. 
    //                // Changes here are PWM.Duration and PWM.Start() instead of PWM.SetPulse().   
    //                _current = value;
    //                servo.Duration = (uint)map((long)_current, 0, 180, range[0], range[1]);
    //                Debug.Print("Current position: " + _current + " Duration: " + servo.Duration);
    //                servo.Start();
    //                servo.Stop();
    //            }
    //        }

    //        /// <summary>
    //        /// Used internally to map a value of one scale to another
    //        /// </summary>
    //        /// <param name="x"></param>
    //        /// <param name="in_min"></param>
    //        /// <param name="in_max"></param>
    //        /// <param name="out_min"></param>
    //        /// <param name="out_max"></param>
    //        /// <returns></returns>

    //        private long map(long x, long in_min, long in_max, long out_min, long out_max)
    //        {
    //            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    //        }
    //    }
    //    internal class Servo : IDisposable {

    //        private readonly PWM _servoPin;
    //        private readonly int[] range = new int[2];
    //        public bool Inverted = false;
    //        private double _last = 0;

    //        public Servo(Cpu.PWMChannel servoPin) {

    //            _servoPin = new PWM(servoPin, 50, 0, false);
    //            range[0] = 500;
    //            range[1] = 2200;
    //            disengage();
    //        }

    //        public void Dispose() {
    //            disengage();
    //            _servoPin.Dispose();
    //        }

    //        public void disengage() {
    //            Degree = 90;
    //            _servoPin.DutyCycle = 0;
    //        }

    //        public void setRange(int fullLeft, int fullRight) {
    //            range[1] = fullLeft;
    //            range[0] = fullRight;
    //        }

    //        public double Degree {
    //            get { return _last; }
    //            set {
    //                if (value > 180) value = 180;
    //                if (value < 0) value = 0;
    //                if (Inverted) value = 180 - value;
    //                _servoPin.Duration = (uint)map((long)value, 0, 180, range[0], range[1]);
    //                //Debug.Print(value.ToString());
    //                _servoPin.Start();
    //                _last = value;

    //            }
    //        }

    //        public long map(long x, long in_min, long in_max, long out_min, long out_max) {
    //            return (x - in_min)*(out_max - out_min)/(in_max - in_min) + out_min;
    //        }
    //}
}
