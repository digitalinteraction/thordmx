using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPS.SharedObjects
{
    [Serializable]
    public class ServerCue
    {

        public ServerCue()
        {
            Channels = new int[513];
        }

        public ServerCue(int[] p, string p_2)
        {
            Channels = p.Select((b)=> (b == 0 ? -1 : (int)b)).ToArray();
            Name = p_2;
        }

        public int[] Channels { get; set; }
        public string Name { get; set; }
    }
}
