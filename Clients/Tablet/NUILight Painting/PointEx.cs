using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace IPS.TabletPainting
{
    public static class PointEx
    {
        public static double Dot(this Point myInterface, Point p)
        {
            return (myInterface.X * p.X + myInterface.Y * p.Y);
        }

        public static double Distance(this Point myInterface, Point p)
        {
            return Math.Sqrt(Math.Pow(myInterface.X - p.X, 2) + Math.Pow(myInterface.Y - p.Y, 2));
        }
    }
}
