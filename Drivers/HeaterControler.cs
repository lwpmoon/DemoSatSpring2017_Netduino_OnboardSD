using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace DemoSatSpring2017Netduino_OnboardSD.Drivers
{
    class HeaterControler
    {
        private readonly PWM _heaterControlPin;
       
        private readonly AnalogInput _tempSensor;
        private readonly AnalogInput _heaterSensor;

        public HeaterControler()
        {
            _tempSensor = new AnalogInput(Cpu.AnalogChannel.ANALOG_5);
            _heaterSensor = new AnalogInput(Cpu.AnalogChannel.ANALOG_3);
            _heaterControlPin = new PWM(PWMChannels.PWM_PIN_D10, 1000, 0, false);
            _heaterControlPin.Start();
        }

        /// <summary>
        /// Returns the current tempurature of the payload
        /// Use to set the required heater power
        /// </summary>
        /// <returns></returns>
        public short GetTemp()
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
            //var tempF = (tempC * 1.8) + 32;

            return (short)tempC;
        }

        /// <summary>
        /// Returns the current tempurature of the heating element
        /// Use to prevent over heating the payload
        /// </summary>
        /// <returns></returns>
        public short GetHeaterTemp()
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
            //var tempF = (tempC * 1.8) + 32;

            return (short)tempC;
        }

        
        /// <summary>
        /// Sets the heater power level based on payload and heater temperature
        /// </summary>
        /// <param name="temperature"></param>
        /// <param name="heaterTemp"></param>
        public void SetHeater(short temperature, short heaterTemp)
        {
            double power = 0;

            if (heaterTemp <= 120)
            {
                if (temperature >= 25) power = 0;
                if (temperature < 25 && temperature >= 23) power = 0.2;
                if (temperature < 23 && temperature >= 20) power = 0.4;
                if (temperature < 20 && temperature >= 18) power = 0.6;
                if (temperature < 18 && temperature >= 15) power = 0.8;
                if (temperature < 15 && temperature >= 0) power = 1; 
            }

            _heaterControlPin.DutyCycle = power;
        }

    }
}
