using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.ComponentModel;
using agsXMPP.protocol.x.muc;
using agsXMPP;


namespace IPS.Communication.Plugins
{
    public class XmppEventClient : IEventClient, ILoggable
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

        agsXMPP.XmppClientConnection client = new agsXMPP.XmppClientConnection();

        public XmppEventClient()
        {
            //load settings
            this.Username = XMPP.Properties.Settings.Default.username;
            this.Password = XMPP.Properties.Settings.Default.password;
            this.Port = XMPP.Properties.Settings.Default.port;
            this.ServerUri = XMPP.Properties.Settings.Default.server;
            this.JoinGroup = XMPP.Properties.Settings.Default.room;
        }

        //private Socket statussocket;
        public void Connect()
        {
            //save settings...
            XMPP.Properties.Settings.Default.username = this.Username;
            XMPP.Properties.Settings.Default.password = this.Password;
            XMPP.Properties.Settings.Default.port = this.Port;
            XMPP.Properties.Settings.Default.server = this.ServerUri;
            XMPP.Properties.Settings.Default.room = this.JoinGroup;
            XMPP.Properties.Settings.Default.Save();

            client.Username = Username;
            client.Password = Password;
            client.Port = Port;
            client.Server = ServerUri;
            client.OnMessage += new agsXMPP.protocol.client.MessageHandler(client_OnMessage);
            client.OnLogin += new agsXMPP.ObjectHandler(client_OnLogin);
            client.OnError += new ErrorHandler(client_OnError);
            client.Open();
        }

        void client_OnError(object sender, Exception ex)
        {
            client.Close();
            client.Open();
        }

        void client_OnLogin(object sender)
        {
            connected = true;
            client.Show = agsXMPP.protocol.client.ShowType.chat;
            client.SendMyPresence();
            if (JoinGroup != "" && JoinGroup != null)
            {
                MucManager mucManager = new MucManager(client);
                Jid Room = new Jid(JoinGroup);
                mucManager.AcceptDefaultConfiguration(Room);
                mucManager.JoinRoom(Room, client.Username);
            }
        }

        void client_OnMessage(object sender, agsXMPP.protocol.client.Message msg)
        {
            //process message
            if (msg.Body != null)
            {
                string[] message = msg.Body.Split(',');
                if (message.Length == 1 && message[0] == "BLACKOUT")
                {
                    processMessage("/dmx/blackout", new ArrayList(), msg.From);
                }
                if (message.Length == 3 && message[0] == "UPDATEONE")
                {
                    processMessage("/dmx/updatechannel", new ArrayList() { msg.From, int.Parse(message[1]), int.Parse(message[2]) }, msg.From);
                }
                if (message.Length == 514 && message[0] == "UPDATEALL")
                {
                    var l = message.Skip(1).ToList();
                    l.Insert(0, msg.From);
                    processMessage("/dmx/frameupdate", new ArrayList(l), msg.From);
                }
            }
        }

        public void Disconnect()
        {
            connected = false;
            client.Close();
        }

        int[] oldvals = new int[512];

        byte?[] temp = new byte?[512];

        //messages are in the format /type/message/ device, v1, v2, v3, v4
        private void processMessage(string address,ArrayList args, string device)
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
        public string JoinGroup { get; set; }
        public int Port { get; set; }

        public string Name
        {
            get { return "XMPP"; }
        }

        [Browsable(false)]
        public Dictionary<int, string> Services
        {
            get {
                return null;
            }
        }

        public event Action<string> OnLogEvent;
        private bool debug = false;
        public bool DebugMode
        {
            set { debug = true; }
        }
    }
}
