using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace IPS.SharedObjects
{
    public class Light
    {
        public int Channel { get; set; }
        public PointF Position { get; set; }
        public int LampType { get; set; }
        public string LampTypeName { get; set; }
        public string Color { get; set; }
        public string Description { get; set; }
        public bool Hidden { get; set; }
        public int UsedChannels { get; set; }
    }
}
