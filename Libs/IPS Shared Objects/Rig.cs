using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPS.SharedObjects
{
    public class Rig
    {
        public string Name
        { get; set; }
        public List<Light> Lights
        { get; set; }
        public string Filename { get; set; }
    }
}
