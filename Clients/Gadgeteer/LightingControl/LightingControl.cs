using System;
using Microsoft.SPOT;
using Gadgeteer.Modules.DigitalInteractionGroup;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Microsoft.SPOT.Hardware;
using System.Collections;

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

        /// <summary>
        /// Describes a RGB mixer lamp (using 3 channels) as a quick way to set colors.
        /// </summary>
        public class RgbLamp
        {
            public RgbLamp(int chan, LightingControl lc)
            {
                Channel = chan;
                this.lc = lc;
            }

            private LightingControl lc;

            public int Channel { get; private set; }


            /// <summary>
            /// Sets the color of the lamp.
            /// </summary>
            /// <param name="color">GT.Color that the lamp should change to</param>
            public void SetColor(Color color)
            {
                lc.UpdateChannel(Channel, color.R);
                lc.UpdateChannel(Channel+1, color.G);
                lc.UpdateChannel(Channel+2, color.B);
            }
        }

        byte[] live = new byte[513];

        ArrayList faders = new ArrayList();

        private double DEFAULT_FADE = 3;

        public void FadeUp(int channel)
        {
            faders.Add(new FaderItem() { channel=channel,timeleft = DEFAULT_FADE,target=255});
            if (!timerstarted)
                StartTimer();
        }

        public void FadeUp(int channel, double time)
        {
            faders.Add(new FaderItem() { channel=channel,timeleft = time, target = 255});
            if (!timerstarted)
                StartTimer();
        }

        private class FaderItem
        {
            public int channel;
            public double timeleft;
            public double change;
            public int target;
        }

        public void FadeDown(int channel)
        {
            faders.Add(new FaderItem() { channel = channel, timeleft = DEFAULT_FADE, target = 0 });
            if (!timerstarted)
                StartTimer();
        }

        public void FadeDown(int channel,double time)
        {
            faders.Add(new FaderItem() { channel=channel,timeleft = time, target = 0});
            if (!timerstarted)
                StartTimer();
        }

        bool timerstarted = false;

        private double FADE_WAIT = 0.1;

        private void StartTimer()
        {
            Thread t = new Thread(new ThreadStart(() => {
                timerstarted = true;
                while (timerstarted)
                {
                    bool change = false;
                    for (int i = 0; i < faders.Count;i++)
                    {
                        FaderItem v = faders[i] as FaderItem;
                        if (v.change == 0)
                        {
                            v.change = ((double)(v.target - live[v.channel]) / (double)(v.timeleft / FADE_WAIT));
                            v.timeleft += FADE_WAIT;
                        }
                        if (v.timeleft > 0)
                        {
                            if ((v.change < 0 && live[v.channel] >= v.target) || (v.change > 0 && live[v.channel] <= v.target))
                            {
                                UpdateChannel(v.channel, System.Math.Min(System.Math.Max(0, ((int)live[v.channel] + (int)v.change)), 255));
                                v.timeleft -= FADE_WAIT;
                                change = true;
                            }
                        }

                    }

                    Thread.Sleep((int)(FADE_WAIT * 1000));

                    //if no changes needed:
                    if (!change)
                    {
                        timerstarted = false;
                        return;
                    }
                }
            }));
            t.Start();
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

        DateTime lastdispatched = DateTime.Now;

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
                        byte[] data = new byte[513];
                        while (true)
                        {  
                            int length = receiver.ReceiveFrom(data, ref ep);
                            if (lastdispatched + TimeSpan.FromTicks(TimeSpan.TicksPerSecond) < DateTime.Now)
                            {
                                //Debug.Print(data[16] + "");
                                for (int i = 0; i < 513; i++)
                                {
                                    vals[i] = data[i];
                                }
                                DispatchSingle(this, vals);
                                lastdispatched = DateTime.Now;
                            }
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
            osc.SendMessage("/dmx/updatechannel", DeviceName, channel,value);
            live[channel] = (byte)value;
        }

        int[] output = new int[514];

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
            Array.Copy(vals,0,output,2,512);
            osc.SendMessage("/dmx/frameupdate", DeviceName,output);
            for (int i = 0; i < 512; i++)
                live[i] = (byte)vals[i];
        }

        /// <summary>
        /// Send Blackout command to the server.
        /// </summary>
        public void Blackout()
        {
            osc.SendMessage("/dmx/blackout", DeviceName);
            live = new byte[512];
        }
    }
}
