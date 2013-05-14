using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.ComponentModel;
using Apache.NMS.Stomp;
using Apache.NMS.Stomp.Commands;
using Apache.NMS;


namespace IPS.Communication.Plugins
{
    public class StompEventClient : IEventClient
    {

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

        IConnection connection;
        ISession session;
        Apache.NMS.Stomp.MessageConsumer consumer;

        public StompEventClient()
        {
            //load settings
            this.Username = Stomp.Properties.Settings.Default.username;
            this.Password = Stomp.Properties.Settings.Default.password;
            this.Port = Stomp.Properties.Settings.Default.port;
            this.ServerUri = Stomp.Properties.Settings.Default.server;
            this.Topic = Stomp.Properties.Settings.Default.topic;
        }

        //private Socket statussocket;
        public void Connect()
        {
            //save settings...
            Stomp.Properties.Settings.Default.username = this.Username;
            Stomp.Properties.Settings.Default.password = this.Password;
            Stomp.Properties.Settings.Default.port = this.Port;
            Stomp.Properties.Settings.Default.server = this.ServerUri;
            Stomp.Properties.Settings.Default.topic = this.Topic;
            Stomp.Properties.Settings.Default.Save();

            IConnectionFactory factory = new NMSConnectionFactory(new Uri("stomp:tcp://"+ServerUri+":"+Port));

            connection = factory.CreateConnection(Username, Password);
            ISession session = connection.CreateSession();

           // session = connection.CreateSession() as Session;
            consumer = session.CreateConsumer(session.GetDestination("topic://"+Topic)) as MessageConsumer;
            consumer.Listener += new Apache.NMS.MessageListener(consumer_Listener);

        }

        void consumer_Listener(Apache.NMS.IMessage msgg)
        {
            var msg = msgg as TextMessage;
            //process message
            if (msg.Text != null && msg.Text != "")
            {
                string[] message = msg.Text.Split(',');
                if (message.Length == 1 && message[0] == "BLACKOUT")
                {
                    processMessage("/dmx/blackout", new ArrayList(), msg.Destination.PhysicalName);
                }
                if (message.Length == 2 && message[0] == "UPDATEONE")
                {
                    processMessage("/dmx/updatechannel", new ArrayList() { msg.Destination.PhysicalName, message[1], message[2] }, msg.Destination.PhysicalName);
                }
                if (message.Length == 513 && message[0] == "UPDATEALL")
                {
                    var l = message.Skip(1).ToList();
                    l.Insert(0, msg.Destination.PhysicalName);
                    processMessage("/dmx/frameupdate", new ArrayList(l), msg.Destination.PhysicalName);
                }
            }
        }

        public void Disconnect()
        {
            connected = false;
            consumer.Close();
            session.Close();
            connection.Close();
        }

        int[] oldvals = new int[512];

        byte?[] temp = new byte?[512];

        //messages are in the format /type/message/ device, v1, v2, v3, v4
        private void processMessage(string address, ArrayList args, string device)
        {
            //ArrayList args = vals;

            if (address == "/dmx/frameupdate")
            {
                int[] vals = args.Cast<int>().ToArray();

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

        public string ServerUri { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public string Topic {get;set;}

        public string Name
        {
            get { return "STOMP"; }
        }

        [Browsable(false)]
        public Dictionary<int, string> Services
        {
            get
            {
                return null;
            }
        }
    }
}
