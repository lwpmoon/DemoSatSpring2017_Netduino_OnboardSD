using DemoSatSpring2017Netduino_OnboardSD.Flight_Computer;
using DemoSatSpring2017Netduino_OnboardSD.Utility;
using DemoSatSpring2017Netduino_OnboardSD.Work_Items;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Math = System.Math;

namespace DemoSatSpring2017Netduino_OnboardSD.Drivers
{
    public class LightTracker
    {

        private const double Tolerance = 0.005;

        private int i = 0;

        // private TestServo servo = null;
        private readonly WorkItem _trackAction;
        private readonly ContServo _panServo;


        private readonly SerialBno _bno;
        private Vector _currentHeading;
        private readonly Vector _zeroPointHeading;
        private double _degreesToMove;

        private enum Direction { Left, Right, None, Error }

        public LightTracker(SerialBno bno, Cpu.PWMChannel panPin) {

            _bno = bno;
            _panServo = new ContServo(panPin);
            //s_panServo.reset_ticks();

            _zeroPointHeading = bno.read_vector(SerialBno.Bno055VectorType.VectorEuler);

            _trackAction = new WorkItem(PointEast, false,true,true);

        }

        public void Start()
        {
            FlightComputer.Instance.Execute(_trackAction);
            Debug.Print("Zero point Heading: X: " + _zeroPointHeading.X + " Y: " + _zeroPointHeading.Y + " Z: " + _zeroPointHeading.Z + "\n");
        }

        public void StopDemo()
        {
            //TODO: SetRepeat function does not currently exists
            //_trackAction.SetRepeat(false);
        }

        //private void UpdateSensors() {
        //    _currentHeading = _bno.read_vector(SerialBno.Bno055VectorType.VectorEuler);
        //    _degreesToMove = GetHeadingError(_zeroPointHeading.X, _currentHeading.X);

        //    //var calib = _bno.GetCalibration();
        //    //Debug.Print("Calib S: " + calib[0] + ", G: " + calib[1] + ", A: " + calib[2] + ", M: " + calib[3]);
        //    //Debug.Print("Current Heading: X: " + _currentHeading.X + " Y: " + _currentHeading.Y + " Z: " + _currentHeading.Z);
        //    //_lastHeading = _currentHeading;
        //    //Thread.Sleep(100);
        //}
        private static double GetHeadingError(double initial, double final)
        {
            var diff = final - initial;
            var absDiff = Math.Abs(diff);

            if (absDiff <= 180) return absDiff == 180 ? absDiff : diff;
            if (final > initial) return absDiff - 360;
            return 360 - absDiff;
        }

        //private Direction GetXDirection(bool update)
        //{
        //    if (update) UpdateSensors();

        //    _degreesToMove = GetHeadingError(_zeroPointHeading.X, _currentHeading.X);
        //    Debug.Print("Delta: " + _degreesToMove);
        //    if(_degreesToMove < 1) return Direction.None;
        //    return _degreesToMove > 0 ? Direction.Left : Direction.Right;


        //}
        //private void Pan()
        //{
            
        //    //switch (GetXDirection(false))
        //    //{
        //    //    case Direction.Left:
        //    //        _panServo.Recover(degreesToMove);
        //    //        _panServo.CounterClockwise();
        //    //        break;
        //    //    case Direction.Right:
        //    //        _panServo.Clockwise();
        //    //        break;
        //    //    case Direction.None:
        //    //        _panServo.Stop();
        //    //        break;
        //    //    case Direction.Error:
        //    //        Debug.Print("Something went wrong in the tilt logic - returned Direction.Error");
        //    //        break;
        //    //}
           
           

        //}


        private void PointEast()
        {
            _currentHeading = _bno.read_vector(SerialBno.Bno055VectorType.VectorEuler);
            _degreesToMove = GetHeadingError(_zeroPointHeading.X, _currentHeading.X);
            _panServo.Recover(_degreesToMove);

            //if (i == 0)
            //{
            //    _panServo.Recover(180);
            //    i = 1;
            //}
            //else
            //{
            //    _panServo.Recover(0);
            //    i = 0;
            //}

        }
        
    }
}