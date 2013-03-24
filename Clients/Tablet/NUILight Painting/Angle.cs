using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DrawRect
{
    public class Angle
    {
        private static double _GradToRad = 180d / Math.PI;

        int _degrees;
        double _rad;

        public double Radians
        {
            get { return _rad; }            
        }

        public int Degrees
        {
            get { return _degrees; }            
        }

        public Angle(int degrees)
        {
            _degrees = degrees;
            _rad = (degrees*1.0) / _GradToRad;
        }

    }
}
