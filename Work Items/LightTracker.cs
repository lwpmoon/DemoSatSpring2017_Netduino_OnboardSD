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


        public LightTracker(SerialBno bno, Cpu.PWMChannel panPin) {

            _bno = bno;
            _panServo = new ContServo(panPin);

            _zeroPointHeading = bno.read_vector(SerialBno.Bno055VectorType.VectorEuler);

            _trackAction = new WorkItem(PointEast, false,true,true);
            Rebug.Print("[SUCCESS] Solar tracker initialized.");
        }

        public void Start()
        {
            FlightComputer.Instance.Execute(_trackAction);
            Rebug.Print("[INFO] Zero point Heading: X: " + _zeroPointHeading.X + " Y: " + _zeroPointHeading.Y + " Z: " + _zeroPointHeading.Z + "\n");
            Rebug.Print("[SUCCESS] Solar tracker started.");
        }

        private static double GetHeadingError(double initial, double final)
        {
            var diff = final - initial;
            var absDiff = Math.Abs(diff);

            if (absDiff <= 180) return absDiff == 180 ? absDiff : diff;
            if (final > initial) return absDiff - 360;
            return 360 - absDiff;
        }

        

        private void PointEast()
        {
            _currentHeading = _bno.read_vector(SerialBno.Bno055VectorType.VectorEuler);
            _degreesToMove = GetHeadingError(_zeroPointHeading.X, _currentHeading.X);
            _panServo.Recover(_degreesToMove);

           

        }
        
    }
}