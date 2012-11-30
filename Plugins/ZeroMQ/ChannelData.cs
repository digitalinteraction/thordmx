using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroMQ
{
    [Serializable]
    public class ChannelData
    {
        public enum MessageType {BLACKOUT,UPDATEONE,UPDATEALL}
        public int[] Data;
        public MessageType Action;
        public string Device;
        public int Channel;
        public byte Value;
    }
}
