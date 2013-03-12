using System;
using Microsoft.SPOT;
using Gadgeteer.Modules.DigitalInteractionGroup;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Microsoft.SPOT.Hardware;

namespace Gadgeteer.Modules.DigitalInteractionGroup
{
    /// <summary>
    /// Module to connect to a ThorDMX lighting control server and control lighting. This module can act as either a controller or a device in the lighting system.
    /// 
    /// TODO:
    /// - Register event for specific channel update.
    /// - Fading Controls
    /// </summary>
    public class LightingControl
    {
        OSC osc;
        /// <summary>
        /// Name of this device to display on the Server
        /// </summary>
        public string DeviceName {get;set;}
        private int transmitport = 12345;
        private int receiveport = 8888;
        private System.Net.Sockets.Socket receiver;

        public class RgbLamp
        {
            public RgbLamp(int chan, LightingControl lc)
            {
                Channel = chan;
                this.lc = lc;
            }

            private LightingControl lc;

            public int Channel { get; private set; }
            public void SetColor(Color color)
            {
                lc.UpdateChannel(Channel, color.R);
                lc.UpdateChannel(Channel, color.G);
                lc.UpdateChannel(Channel, color.B);
            }
        }

        public RgbLamp RegisterRgbLamp(int startchan)
        {
            return new RgbLamp(startchan,this);
        }

        public LightingControl()
        {
            osc = new OSC();
            DeviceName = "<gadgeteer>";
        }

        /// <summary>
        /// Connect to an ThorDMX Lighting Server
        /// </summary>
        /// <param name="serverip">IP address of the local server</param>
        /// <param name="receive">Set True to receive channel updates from the server.</param>
        public void Connect(string serverip, bool receive = false)
        {
            //start transmitter:
            osc.StartTransmitter(serverip,transmitport);
            //receive is on UDP broadcast 8888
            if (receive)
            {
                Thread t = new Thread(new ThreadStart(() =>
                {
                    int[] vals = new int[513];
                    receiver = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    IPEndPoint iep = new IPEndPoint(IPAddress.Any, receiveport);
                    receiver.Bind(iep);
                    EndPoint ep = (EndPoint)iep;
                    try
                    {
                        while (true)
                        {
                            byte[] data = new byte[513];
                            int length = receiver.ReceiveFrom(data, ref ep);
                            for (int i = 0; i < 513; i++)
                            {
                                vals[i] = data[i];
                            }
                            DispatchSingle(this,vals);
                            Thread.Sleep(50);
                        }
                    }
                    finally
                    {
                        receiver.Close();
                    }
                }));
                t.Start();
            }
        }

        public delegate void LightingEventHandler(LightingControl sender, int[] channels);
        private LightingEventHandler onReceive;
        /// <summary>
        /// Event that is fired when a channel update is received from the Server.
        /// </summary>
        public event LightingEventHandler OnUpdate;

        private void DispatchSingle(LightingControl sender, int[] channels)
        {
            if (this.onReceive == null)
            {
                this.onReceive = new LightingEventHandler(this.DispatchSingle);
            }

            //for every packet
            if (Program.CheckAndInvoke(OnUpdate, this.onReceive, sender, channels))
            {
                this.OnUpdate(sender, channels);
            }
        }

        /// <summary>
        /// Update a single lighting channel.
        /// </summary>
        /// <param name="channel">Channel number (1-512)</param>
        /// <param name="value">Channel value (0-255)</param>
        public void UpdateChannel(int channel, int value)
        {
            if (value < 0 || value > 255)
            {
                throw new Exception("Value must be 0-255");
            }

            if (channel < 1 || channel > 512)
            {
                throw new Exception("Channel must be 0-512");
            }
            osc.SendMessage("/dmx/updatechannel", channel, value);
        }

        /// <summary>
        /// Updates all 512 channels at once.
        /// </summary>
        /// <param name="vals">Array of values to set channels (must be 512 items)</param>
        public void UpdateAllChannels(int[] vals)
        {
            if (vals.Length != 512)
            {
                throw new Exception("There must be 512 channels when updating all channels at once.");
            }
            osc.SendMessage("/dmx/frameupdate", DeviceName, 0, 0, vals);
        }

        /// <summary>
        /// Send Blackout command to the server.
        /// </summary>
        public void Blackout()
        {
            osc.SendMessage("/dmx/blackout", DeviceName);
        }
    }
}
