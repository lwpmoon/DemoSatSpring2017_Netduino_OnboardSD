using System;
using DemoSatSpring2017Netduino_OnboardSD.Flight_Computer;
using DemoSatSpring2017Netduino_OnboardSD.Work_Items;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using PWMChannels = SecretLabs.NETMF.Hardware.NetduinoPlus.PWMChannels;

namespace DemoSatSpring2017Netduino_OnboardSD.Drivers
{
    class HeaterControler
    {
        private readonly WorkItem _heaterAction;
        private readonly PWM _heaterControlPin;
        //Bug: BMP280 will no longer be heated. Reference different temperature sensor
        private readonly Bmp280 _tempSensor;
        
        private double _temperature;
        
        public HeaterControler(Bmp280 tempSensor)
        {
            _tempSensor = tempSensor;
            _heaterControlPin= new PWM(PWMChannels.PWM_PIN_D10,1000, 0, false);
            _heaterControlPin.Start();

        }

        public void UpdateTemp()
        {
            _temperature = _tempSensor.GetTemperature();
        }

        public void SetHeater(double level)
        {
            _heaterControlPin.DutyCycle = level;
        }

        public void Start()
        {
            FlightComputer.Instance.Execute(_heaterAction);
        }
    }
}
