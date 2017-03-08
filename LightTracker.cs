using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using DemoSatSpring2017Netduino_OnboardSD;
using DemoSatSpring2017Netduino_OnboardSD.Flight_Computer;
using DemoSatSpring2017Netduino_OnboardSD.Work_Items;

namespace DemoSatSpring2017Netduino_OnboardSD
{
    public class LightTracker
    {

        private const double Tolerance = 0.005;

        // private TestServo servo = null;
        private readonly WorkItem _trackAction;

        private readonly ContServo _panServo;
        //private readonly Servo _tiltServo;

        private readonly AnalogInput photoCellL;
        private readonly AnalogInput photoCellR;
        //private readonly AnalogInput photoCellBL;
        //private readonly AnalogInput photoCellBR;

        private double _left = 0;
        private double _right = 0;
        //private double _bottomRight = 0;
        //private double _bottomLeft = 0;

        private double _maxBrightness;
        private double _brightestX = 0;
        //private double _brightestY = 0;

        private double _lastX = 0;
        //private double _lastY = 0;

        private bool _foundX = false;
        //private bool _foundY = false;
        private double _brightestXTick;

        //private enum Direction { up, down, left, right, none, error }
        private enum Direction { Left, Right, None, Error }

        public LightTracker(Cpu.PWMChannel panPin, Cpu.AnalogChannel topLeftPhotocell, Cpu.AnalogChannel topRightPhotocell)
        {


            _panServo = new ContServo(panPin);
            _panServo.reset_ticks();


            //_tiltServo = new Servo(tiltPin, 150) { Degree = 150 };
            //_lastY = _tiltServo.Degree;


            photoCellL = new AnalogInput(topLeftPhotocell);
            photoCellR = new AnalogInput(topRightPhotocell);
            //photoCellBL = new AnalogInput(bottomLeftPhotocell);
            //photoCellBR = new AnalogInput(bottomRightPhotocell);


            _trackAction = new WorkItem(SearchForLight, true);

        }

        public void Start()
        {
            FlightComputer.Instance.Execute(_trackAction);
        }

        public void StopDemo()
        {
            //TODO: SetRepeat function does not currently exists
            //_trackAction.SetRepeat(false);
        }

        private void UpdateSensors()
        {
            _left = photoCellL.Read();
            _right = photoCellR.Read();
            //_bottomLeft = photoCellBL.Read();
            //_bottomRight = photoCellBR.Read();

            Debug.Print("TL: " + _left);
            Debug.Print("TR: " + _right);
            //Debug.Print("BL: " + _bottomLeft);
            //Debug.Print("TR: " + _bottomRight);
        }
        private double GetCurrentBrightness(bool update)
        {

            if (update) UpdateSensors();
            return _left + _right;
        }

        //private Direction getYDirection(bool update)
        //{
        //    if (update) UpdateSensors();

        //    var topAvg = (_right + _left) / 2.0;
        //    var botAvg = (_bottomRight + _bottomLeft) / 2.0;

        //    if (System.Math.Abs(topAvg - botAvg) < Tolerance) return Direction.none;

        //    _foundY = false;
        //    if (topAvg > botAvg) return Direction.up;
        //    if (topAvg < botAvg) return Direction.down;

        //    return Direction.error;
        //}

        private Direction GetXDirection(bool update)
        {
            if (update) UpdateSensors();

            //var leftAvg = (_left + _bottomLeft) / 2.0;
            //var rightAvg = (_bottomRight + _right) / 2.0;

            //if (System.Math.Abs(leftAvg - rightAvg) < Tolerance) return Direction.none;
            if (System.Math.Abs(_left - _right) < Tolerance) return Direction.None;

            _foundX = false;
            //if (leftAvg > rightAvg) return Direction.left;
            //if (leftAvg < rightAvg) return Direction.right;
            if (_left > _right) return Direction.Left;
            if (_left < _right) return Direction.Right;

            return Direction.Error;
        }

        //private void Tilt()
        //{
        //    var yDirection = getYDirection(false);

        //    switch (yDirection)
        //    {
        //        case Direction.up:
        //            _lastY--;
        //            break;
        //        case Direction.down:
        //            _lastY++;
        //            break;
        //        case Direction.none:
        //            _foundY = true;
        //            return;
        //        case Direction.error:
        //            Debug.Print("Something went wrong in the pan logic - returned Direction.Error");
        //            break;

        //    }
        //    if (_lastY > 160) _lastY = 90; //if we went too far, Start over
        //    if (_lastY < 90) _lastY = 160;

        //    _tiltServo.Degree = _lastY;
        //    Thread.Sleep(5);
        //    _tiltServo.disengage();
        //}

        private void Pan()
        {

            switch (GetXDirection(false))
            {
                case Direction.Left:
                    // _lastX--;
                    _panServo.go_counterclockwise_one_tick();
                    break;
                case Direction.Right:
                    _panServo.go_clockwise_one_tick();
                    //  _lastX++;
                    break;
                case Direction.None:
                    _foundX = true;
                    return;
                case Direction.Error:
                    Debug.Print("Something went wrong in the tilt logic - returned Direction.Error");
                    break;
            }
            int ticks = _panServo.get_ticks();
            Debug.Print("GOT HERE!!!!");
            Debug.Print("Ticks: " + ticks);
            if (System.Math.Abs(ticks) > 80)
            {
                _panServo.go_to_zero();
                for (int i = 0; i < 80; i++)
                {
                    _panServo.go_counterclockwise_one_tick();
                }
            }
            //if (_lastX > 360) _lastX = 0;
            //if (_lastX < 0) _lastX = 360;

            //_panServo.Degree = _lastX;
            //Thread.Sleep(5);
            //_panServo.disengage();

        }


        private void SearchForLight()
        {

            UpdateSensors(); //updates all sensors
            CheckBrightness(); //compares current brightness with max we've found and updates it
            Pan();  //gets the direction we're supposed to pan (if any) and pans. If we supposedly found the right spot, make note for confirmation later.
            //Tilt(); // gets the direction we're supposed to tilt (if any) and tilts. If we supposedly found the right spot, make note for confirmation later.
            ConfirmLocation(); //if we found the right pan and tilt spot, check it against our brightest spot we found during searching.. 
                               //If the spot is considerably brighter than our current spot, hang out there for 5 seconds for picture taking, and mark that we haven't found the right spot yet.

        }

        private void ConfirmLocation()
        {

            //this method only runs if we've supposedly found a bright location.
            //if (!_foundX || !_foundY) return;
            if (!_foundX) return;

            // if our current brightness (new update) is lower than some other brightness we found during searching, 
            //    move to that brightness and wait 5 seconds (allows time for picture taking) before continuing
            if (GetCurrentBrightness(true) < _maxBrightness - .5)
            { //.5 is a tolerance - may not need...

                _lastX = _brightestX;
                //_lastY = _brightestY;

                _panServo.go_to_zero();
                if (_brightestXTick > 0)
                {
                    for (int i = 0; i < _brightestXTick; i++)
                    {
                        _panServo.go_clockwise_one_tick();
                    }
                }
                else if (_brightestXTick < 0)
                {
                    for (int i = 0; i < -_brightestXTick; i++)
                    {
                        _panServo.go_counterclockwise_one_tick();
                    }
                }
                //_panServo.Degree = _lastX;
                //Thread.Sleep(5);
                //_panServo.disengage();

                //_tiltServo.Degree = _lastY;
                //Thread.Sleep(5);
                //_tiltServo.disengage();

                Thread.Sleep(5000);

                _foundX = false;
                //_foundY = false;
            }
            else Debug.Print("We found the brightest location.");
        }

        private void CheckBrightness()
        {
            var currentBrightness = GetCurrentBrightness(false);
            if (currentBrightness > _maxBrightness)
            {
                _brightestXTick = _panServo.get_ticks();
                _brightestX = _lastX;
                //_brightestY = _lastY;
                _maxBrightness = currentBrightness;
            }

            Debug.Print("Current Brightest location:" + _lastX);
            Debug.Print("Current Max Brightness:" + _maxBrightness);

        }
    }
}