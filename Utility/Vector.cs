using System.Collections;

namespace DemoSatSpring2017Netduino_OnboardSD.Utility {
    public struct Vector {

        public float[] InnerArray { get; private set; }
        public float X { get { return InnerArray[0]; } set { InnerArray[0] = value; } } 
        public float Y { get { return InnerArray[1]; } set { InnerArray[1] = value; } } 
        public float Z { get { return InnerArray[2]; } set { InnerArray[2] = value; } } 

        public Vector(float x, float y, float z) {
            InnerArray = new float[3];
            X = x;
            Y = y;
            Z = z;
        }
    }
}