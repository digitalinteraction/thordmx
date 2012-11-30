using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPS.Communication
{
    public interface IDmxOutput
    {
       void UpdateChannel(int channel, int value);
       void UpdateChannels(byte[] channels);
       void BlackOut();
       byte[] GetChannelData();
       void Start();
       void Stop();
       string Name { get; }
    }
}
