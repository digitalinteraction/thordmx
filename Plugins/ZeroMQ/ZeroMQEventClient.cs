using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Diagnostics;
using ZeroMQ;

namespace IPS.Communication.Plugins
{
    public class ZeroMQEventClient:IEventClient,IServerService
    {
        Dictionary<string, Dictionary<string, DmxEventHandler>> handlers = new Dictionary<string, Dictionary<string, DmxEventHandler>>();

        ZmqContext context = ZmqContext.Create();
        ZmqSocket server;
        public void Connect()
        {
            try
            {
                server = context.CreateSocket(SocketType.PULL);
                server.Bind("tcp://*:8887");
                server.ReceiveHighWatermark = 1;
                //thread..

                Thread t = new Thread(new ThreadStart(() => {
                    while (true)
                    {
                        var msg = server.ReceiveFrame();
                        try
                        {
                            if (msg.Buffer.Length == 514) //if its from the python implementation...
                            {
                                var b = msg.Buffer.Skip(1).Take(512).Cast<object>().ToArray();
                                ProcessMessage(b, "<zeromq>", ChannelData.MessageType.UPDATEALL);
                            } else if (msg.Buffer.Length >= 512)//if its from the c# implementation
                            {
                                var b = msg.Buffer.Take(512).Cast<object>().ToArray();
                                ProcessMessage(b, "<zeromq>", ChannelData.MessageType.UPDATEALL);
                            }                            
                        }
                        catch (Exception e) {
                            
                        }
                    }
                }));
                t.Start();
            }
            catch 
            { 
            
            }

        }

        object[] oldvals = new object[512];

        //MessagePackSerializer<Dictionary<string, object>> serializer = MessagePackSerializer.Create<Dictionary<string, object>>();

        byte?[] temp = new byte?[512];

        private void ProcessMessage(object[] vals, string device,ZeroMQ.ChannelData.MessageType msgt)
        {
            //string device = c.Device;
            string address;
            
            //decide to process the message if its not exactly the same message...
            if (Enumerable.SequenceEqual(vals,oldvals))
            {
                return;
            }

            if (oldvals[0] == null)
            {
                oldvals = vals;
            }

            temp = vals.Cast<byte?>().ToArray();

            //work out which ones have changed and set the rest to null
            for (int i = 0; i < vals.Length; i++)
            {
                if ((byte)vals[i] == (byte)oldvals[i])
                    temp[i] = null;
            }

            oldvals = vals;

            //decide what to do...
            switch (msgt)
            {
                case ChannelData.MessageType.BLACKOUT:
                    address = "/dmx/blackout";

                    if (handlers.ContainsKey("*"))
                    {
                        if (handlers["*"].ContainsKey(address))
                            handlers["*"][address].Invoke(device, address, null);
                    }

                    if (handlers.ContainsKey(device))
                    {
                        if (handlers[device].ContainsKey(address))
                            handlers[device][address].Invoke(device, address, null);
                    }
                    break;

                case ChannelData.MessageType.UPDATEALL:
                    address = "/dmx/frameupdate";

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
                    break;

                //case ChannelData.MessageType.UPDATEONE:
                //    address = "/dmx/updatechannel";

                //    if (handlers.ContainsKey("*"))
                //    {
                //        //if (handlers["*"].ContainsKey(address))
                //            //handlers["*"][address].Invoke(device, address, new object[] {c.Channel,c.Value});
                //    }

                //    if (handlers.ContainsKey(device))
                //    {
                //        //if (handlers[device].ContainsKey(address))
                //         //   handlers[device][address].Invoke(device, address, new object[] { c.Channel, c.Value });
                //    }
                //    break;
            }            
        }

        public void Disconnect()
        {
            //server.Unbind("tcp://*:8887");
            server.Close();
        }

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

        private void SendMessage(string device, string address, string[] message)
        {

            if (handlers.ContainsKey("*"))
            {
                if (handlers["*"].ContainsKey(address))
                    handlers["*"][address].Invoke(device, address, message);
            }

            if (handlers.ContainsKey(device))
            {
                if (handlers[device].ContainsKey(address))
                    handlers[device][address].Invoke(device, address, message);
            }
        }

        public ZeroMQEventClient()
        {

        }


        public string Name
        {
            get { return "ZeroMQ"; }
        }

        ~ZeroMQEventClient()
        {
            if (server != null)
                server.Close();
        }


        [Browsable(false)]
        public Dictionary<int, string> Services
        {
            get
            {
                var d = new Dictionary<int, string>();
                d.Add(8887, "_zmq._tcp");
                return d;
            }
        }
    }
}
