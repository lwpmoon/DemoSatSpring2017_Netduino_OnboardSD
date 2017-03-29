using System;
using DemoSatSpring2017Netduino_OnboardSD.Flight_Computer;
using DemoSatSpring2017Netduino_OnboardSD.Work_Items;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using PWMChannels = SecretLabs.NETMF.Hardware.NetduinoPlus.PWMChannels;

namespace DemoSatSpring2017Netduino_OnboardSD.Drivers
{
    class HeaterControler
    {
        private readonly PWM _heaterControlPin;
       
        private readonly AnalogInput _tempSensor;
        private readonly AnalogInput _heaterSensor;
        
        public HeaterControler()
        {
            _tempSensor = new AnalogInput(Cpu.AnalogChannel.ANALOG_0);
            _heaterSensor = new AnalogInput(Cpu.AnalogChannel.ANALOG_1);
            _heaterControlPin = new PWM(PWMChannels.PWM_PIN_D10,1000, 0, false);
            _heaterControlPin.Start();
        }

        public void Update()
        {
            var heaterTemp = GetHeaterTemp();
            var temperature = GetTemp();

            SetHeater(temperature, heaterTemp);

            Debug.Print("Heater heaterTemp: " + heaterTemp);
            Debug.Print("Internal Temp: " + temperature);
        }


        private double GetTemp()
        {
            var i = 0;
            double readings = 0;
            while (i < 20)
            {
                readings += _tempSensor.Read();
                i++;
            }
            var voltage = (readings / 20) * 3.3;
            var tempC = (voltage - 0.5) * 100;
            var tempF = (tempC * 1.8) + 32;

            return tempF;
        }

        private double GetHeaterTemp()
        {
            var i = 0;
            double readings = 0;
            while (i < 20)
            {
                readings += _heaterSensor.Read();
                i++;
            }
            var voltage = (readings / 20) * 3.3;
            var tempC = (voltage - 0.5) * 100;
            var tempF = (tempC * 1.8) + 32;

            return tempF;
        }

        

        private void SetHeater(double temperature, double heaterTemp)
        {
            double power = 0;

            if (heaterTemp <= 100)
            {
                if (temperature >= 78) power = 0;
                if (temperature < 78 && temperature >= 76) power = 0.2;
                if (temperature < 76 && temperature >= 74) power = 0.4;
                if (temperature < 74 && temperature >= 72) power = 0.6;
                if (temperature < 72 && temperature >= 70) power = 0.8;
                if (temperature < 70 && temperature >= 0) power = 1; 
            }

            _heaterControlPin.DutyCycle = power;
        }

    }
}
