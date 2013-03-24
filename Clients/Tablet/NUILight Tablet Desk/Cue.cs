using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPS.TabletDesk
{
    public class Cue
    {
        public int[] channels;
        public string name = "Cue";
        public double fadeup = 3;
        public double fadedown = 3;
        
        public Cue()
        {
            channels = new int[512];
        }

        public Cue(Cue c)
        {
            int[] n = new int[512];
            c.channels.CopyTo(n, 0);
            this.channels = n;
            this.name = c.name;
            this.fadeup = c.fadeup;
            this.fadedown = c.fadedown;
        }

        public override string ToString()
        {
            if (name.Equals(""))
                return "Cue";
            else
                return name;
        }
    }
}
