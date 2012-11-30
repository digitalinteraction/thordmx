using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using IPS.Controller;

namespace IPS.Communication.Plugins
{
    public class OSCController:IController
    {
        public bool Blackout { get; set; }
        public string DeviceName { get; set; }


        public byte[] LiveValues {get;set;}

        int[] tempblackout = new int[512];
        private OSC.NET.OSCTransmitter transmitter;
        
        string connectionip;

        public OSCController(string connectionip)
        {
            this.connectionip = connectionip;
            DeviceName = "<osc>";
        }

        DateTime lastupdatetime = DateTime.Now;

        public void DoBlackout()
        {
            if (!this.Blackout)
            {

                OSC.NET.OSCMessage msg = new OSC.NET.OSCMessage("/dmx/blackout");
                try
                {
                    msg.Append(DeviceName);
                }
                catch
                {
                }
                transmitter.Send(msg);

                Blackout = true;
                for (int i = 0; i < 512; i++)
                    tempblackout[i] = (int)LiveValues[i];

                LiveValues = new byte[512];

            }
            else
            {
                Blackout = false;
                UpdateValues(tempblackout);
            }
        }


        public void UpdateValue(int chan, int val)
        {

                OSC.NET.OSCMessage msg = new OSC.NET.OSCMessage("/dmx/updatechannel");
                msg.Append(DeviceName);
                msg.Append(chan);
                msg.Append(val);
                transmitter.Send(msg);

                LiveValues[chan] = (byte)val;

        }

        public void UpdateValues(int[] vals)
        {
            OSC.NET.OSCMessage msg = new OSC.NET.OSCMessage("/dmx/frameupdate");
            msg.Append(DeviceName);
            msg.Append(0);
            msg.Append(0);
            for (int i = 0; i < 512; i++)
            {
                msg.Append((int)vals[i]);
            }
            transmitter.Send(msg);

            for (int i = 0; i < 512; i++)
                LiveValues[i] = (byte)vals[i];

        }


        public void Start()
        {
            LiveValues = new byte[512];
            transmitter = new OSC.NET.OSCTransmitter(connectionip, 12345);
            transmitter.Connect();
        }
    }
}
