using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.ComponentModel;


namespace IPS.Communication.Plugins
{
    public class UDPBroadcast : IDmxOutput,IServerService
    {

        public UDPBroadcast()
        {
            BroadcastPort = 8888;
        }

        Socket statussocket;

        public int BroadcastPort { get; private set; }

        private IPEndPoint broadcastendpoint;

        byte[] data = new byte[513];

        public string Name
        {
            get { return "UDP Broadcast"; }
        }

        [Browsable(false)]
        public Dictionary<int, string> Services
        {
            get
            {
                var d = new Dictionary<int, string>();
                d.Add(8888, "_dmx._udp");
                return d;
            }
        }

        private void send()
        {
            try
            {
                statussocket.SendTo(data, broadcastendpoint);
            }
            catch { }
        }

        public void UpdateChannel(int channel, int value)
        {
            data[channel] = (byte)value;
            send();
        }

        public void UpdateChannels(byte[] channels)
        {
            data = channels;
            send();
        }

        public void BlackOut()
        {
            data = new byte[513];
            send();
        }

        public byte[] GetChannelData()
        {
            return data;
        }

        public void Start()
        {
            statussocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            statussocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            broadcastendpoint = new IPEndPoint(IPAddress.Broadcast, BroadcastPort);
        }

        public void Stop()
        {
            try
            {
                statussocket.Close();
            }
            catch { }
        }
    }
}
