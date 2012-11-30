using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using OSC.NET;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.ComponentModel;


namespace IPS.Communication.Plugins
{
    public class OscEventClient:IEventClient,IServerService
    {
        OSC.NET.OSCReceiver oscreceiver = null;
        Dictionary<string, Dictionary<string, DmxEventHandler>> handlers = new Dictionary<string, Dictionary<string, DmxEventHandler>>();
        bool connected = false;

        //register message... 
        public void RegisterHandler(DmxEventHandler dothis, string osc_type, string osc_name, string osc_device)
        {
            if (!handlers.ContainsKey(osc_device))
            {
                handlers.Add(osc_device, new Dictionary<string, DmxEventHandler>());
            }

            handlers["*"].Add("/" + osc_type + "/" + osc_name, dothis);
        }

        public void UnregisterHandler(string osc_type, string osc_name, string osc_device)
        {
            if (handlers.ContainsKey(osc_device))
            {
                if (handlers[osc_device].ContainsKey("/" + osc_type + "/" + osc_name))
                    handlers[osc_device].Remove("/" + osc_type + "/" + osc_name);
            }
        }

        public void UnregisterHandler(string osc_device)
        {
            if (handlers.ContainsKey(osc_device))
            {
                handlers.Remove(osc_device);
            }
        }

        public int Port { get { return port; } }
        private bool StartedStomp = false;
        //private Socket statussocket;
        public void Connect()
        {
            //start osc listening...
            oscreceiver = new OSC.NET.OSCReceiver(port);
            oscreceiver.Connect();
            connected = true;
            Thread thread = null;
            thread = new Thread(new ThreadStart(listen));
            thread.Start();

            //broadcast dmx...
            //statussocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //statussocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
        }

        public void Disconnect()
        {
            connected = false;
            oscreceiver.Close();
        }

        private int port=12345;

        private void listen()
        {
            while (connected)
            {
                try
                {
                    OSCPacket packet = oscreceiver.Receive();
                    if (packet != null)
                    {
                        if (packet.IsBundle())
                        {
                            ArrayList messages = packet.Values;
                            for (int i = 0; i < messages.Count; i++)
                            {
                                processMessage((OSCMessage)messages[i]);
                            }
                        }
                        else processMessage((OSCMessage)packet);
                    }
                    else Console.WriteLine("null packet");
                }
                catch (Exception e) { Console.WriteLine(e.Message); }
            }
        }

        int[] oldvals = new int[512];

        byte?[] temp = new byte?[512];

        //messages are in the format /type/message/ device, v1, v2, v3, v4
        private void processMessage(OSCMessage message)
        {
            string address = message.Address;
            ArrayList args = message.Values;
            string device = (string)args[0]; 

            if (address == "/dmx/frameupdate")
            {

                int[] vals = message.Values.ToArray().Skip(2).Take(512).Cast<int>().ToArray();

                if (Enumerable.SequenceEqual(vals, oldvals))
                {
                    return;
                }

                if (oldvals[0] == null)
                {
                    oldvals = vals;
                }

                //temp = vals.Cast<int>().Cast<byte?>().ToArray();
                temp = vals.Select(x => (byte?)((int)x)).ToArray();

                //work out which ones have changed and set the rest to null
                for (int i = 0; i < vals.Length; i++)
                {
                    if ((byte)vals[i] == (byte)oldvals[i])
                        temp[i] = null;
                }
                Array.Copy(vals, oldvals, 512);
                //oldvals = vals;
                if (handlers.ContainsKey("*"))
                {
                    if (handlers["*"].ContainsKey(address))
                        handlers["*"][address].Invoke(device, address, temp.Cast<object>().ToArray());
                }

                if (handlers.ContainsKey(device))
                {
                    if (handlers[device].ContainsKey(address))
                        handlers[device][address].Invoke(device, address, temp.Cast<object>().ToArray());
                }
            }
            else
            {
                //send var[1] and var[2] as channel and value
                if (handlers.ContainsKey("*"))
                {
                    if (handlers["*"].ContainsKey(address))
                        handlers["*"][address].Invoke(device, address, args.ToArray());
                }

                if (handlers.ContainsKey(device))
                {
                    if (handlers[device].ContainsKey(address))
                        handlers[device][address].Invoke(device, address, args.ToArray());
                }
            }
            

            
        }


        private string ServerName = "192.168.1.100";

        public long SequenceNumber = 0;

        public string Name
        {
            get { return "OSC"; }
        }

        [Browsable(false)]
        public Dictionary<int, string> Services
        {
            get { 
                var d = new Dictionary<int, string>();
                d.Add(12345, "_osc._udp");
                return d;
            }
        }
    }
}
