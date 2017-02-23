using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace DemoSat2016Netduino_OnboardSD
{
    public class ContServo : IDisposable
    {

        private PWM servo;
        private int _leftTicksTaken;
        private int _rightTicksTaken;

        private int _maxClockwise = 1300;
        private int _maxCounterClockwise = 1700;

        public int get_ticks()
        {
            return _rightTicksTaken - _leftTicksTaken;
        }

        public ContServo(Cpu.PWMChannel servo_pin)
        {
            servo = new PWM(servo_pin, 20, 0, false);
            servo.Start();


        }

        public void go_to_zero()
        {
            int test = -get_ticks();
            if (test > 0)
            {
                for (int i = 0; i < test; i++)
                {
                    go_clockwise_one_tick();
                }
            }
            else if (test < 0)
            {
                for (int i = 0; i < -test; i++)
                {
                    go_counterclockwise_one_tick();
                }
            }

        }


        public void go_clockwise_one_tick()
        {
            servo.Duration = (uint)1300;
            servo.Start();
            Thread.Sleep(20);
            servo.Stop();
            Thread.Sleep(200);
            _rightTicksTaken++;

            // Debug.Print("Right ticks taken " + _rightTicksTaken);
            // Debug.Print("Total Tick count: " + get_ticks());
        }

        public void go_counterclockwise_one_tick()
        {
            servo.Duration = (uint)2100;
            servo.Start();
            Thread.Sleep(20);
            servo.Stop();
            Thread.Sleep(200);
            _leftTicksTaken++;
            // Debug.Print("Left ticks taken " + _leftTicksTaken);
            // Debug.Print("Total Tick count: " + get_ticks());
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
        public double map(double s, double a1, double a2, double b1, double b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

        public void reset_ticks()
        {
            _leftTicksTaken = 0;
            _rightTicksTaken = 0;
        }

    }
    public class Servo : IDisposable
    {
        /// <summary>
        /// PWM handle
        /// </summary>
        private PWM servo;

        /// <summary>
        /// Timings range
        /// </summary>
        private readonly int[] range = new int[2];

        /// <summary>
        /// Set servo inversion
        /// </summary>
        public bool inverted = false;

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
            servo = new PWM(channelPin, 20000, 1500, Microsoft.SPOT.Hardware.PWM.ScaleFactor.Microseconds, false)
            {
                Period = 20000
            };
            Degree = initialPosition;
            // Typical settings
            range[0] = 500;
            range[1] = 2100;
        }

        public void Dispose()
        {
            disengage();
            servo.Dispose();
        }

        /// <summary>
        /// Allow the user to set cutom timings
        /// </summary>
        /// <param name="fullLeft"></param>
        /// <param name="fullRight"></param>
        public void setRange(int fullLeft, int fullRight)
        {
            range[1] = fullLeft;
            range[0] = fullRight;
        }

        /// <summary>
        /// Disengage the servo. 
        /// The servo motor will stop trying to maintain an angle
        /// </summary>
        public void disengage()
        {
            // See what the Netduino team say about this... 
            servo.DutyCycle = 0; //SetDutyCycle(0);
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
                if (inverted)
                    value = 180 - value;

                // Set the pulse
                //servo.SetPulse(20000, (uint)map((long)value, 0, 180, range[0], range[1]));
                servo.Duration = (uint)map((long)value, 0, 180, range[0], range[1]);
                servo.Start();
                _current = (int)value;
            }
        }


        private long map(long x, long in_min, long in_max, long out_min, long out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
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
