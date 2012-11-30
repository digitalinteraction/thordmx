using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPS.Communication;

namespace IPS.Communication.Plugins
{
    public class VirtualDMX:IDmxOutput
    {
        byte[] data = new byte[513];
        public void UpdateChannel(int channel, int value)
        {
            data[channel] = (byte)value;
        }

        public void UpdateChannels(byte[] channels)
        {
            data = channels;
        }

        public void BlackOut()
        {
            data = new byte[513];
        }

        public byte[] GetChannelData()
        {
            return data;
        }

        public void Start()
        {
            
        }

        public void Stop()
        {
            
        }


        public string Name
        {
            get { return "Virtual DMX"; }
        }
    }
}
