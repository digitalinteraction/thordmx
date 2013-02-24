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
using Alchemy.Classes;
using System.Web;


namespace IPS.Communication.Plugins
{
    public class WebSocketsEventClient:IEventClient,IServerService
    {
        Dictionary<string, Dictionary<string, DmxEventHandler>> handlers = new Dictionary<string, Dictionary<string, DmxEventHandler>>();

        Alchemy.WebSocketServer sse;

        public int Port { get; private set; }

        public void Connect()
        {
            Port = 8283;
            try
            {
                //start server socket, for fast comms
                sse = new Alchemy.WebSocketServer(Port, IPAddress.Any);
                sse.FlashAccessPolicyEnabled = false;
                sse.OnConnected = OnConnected;
                sse.OnDisconnect = OnDisconnect;
                sse.Start();
            }
            catch 
            { 
            
            }
            
        }

        List<UserContext> Clients = new List<UserContext>();

        void OnConnected(UserContext context)
        {
            Clients.Add(context);
            context.SetOnReceive(new Alchemy.OnEventDelegate((o) =>
            {
                //process received frame...
                //parse data: o.Data

                //Uri u = new Uri("http://localhost/" + o.DataFrame);

                var vals = HttpUtility.ParseQueryString(o.DataFrame.ToString());
                string device = o.ClientAddress.ToString();
                string address;
                switch (vals[0])
                {
                    case "update":
                        int channel = Convert.ToInt32(vals["channel"]);
                        int value = Convert.ToInt32(vals["value"]);
                        address = "/dmx/updatechannel";

                        if (handlers.ContainsKey("*"))
                        {
                            if (handlers["*"].ContainsKey(address))
                                handlers["*"][address].Invoke(device, address, new object[] { channel, value});
                        }

                        if (handlers.ContainsKey(device))
                        {
                            if (handlers[device].ContainsKey(address))
                                handlers[device][address].Invoke(device, address, new object[] { channel, value });
                        }
                    break;

                    case "blackout":
                        address = "/dmx/blackout";

                        if (handlers.ContainsKey("*"))
                        {
                            if (handlers["*"].ContainsKey(address))
                                handlers["*"][address].Invoke(device, address,null);
                        }

                        if (handlers.ContainsKey(device))
                        {
                            if (handlers[device].ContainsKey(address))
                                handlers[device][address].Invoke(device, address, null);
                        }

                    break;
                }
            }));
        }

        private class SocketCommand
        {
            public string command = "";
            public string[] cues;
        }

        void OnDisconnect(UserContext context)
        {
            Clients.Remove(context);
        }

        object[] oldvals = new object[512];

        byte?[] temp = new byte?[512];

        public void Disconnect()
        {
            sse.Stop();
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

        public WebSocketsEventClient()
        {

        }


        public string Name
        {
            get { return "Websockets Control"; }
        }

        ~WebSocketsEventClient()
        {
            //if (server != null)
            //    server.Close();
        }


        [Browsable(false)]
        public Dictionary<int, string> Services
        {
            get
            {
                var d = new Dictionary<int, string>();
                d.Add(Port, "_wsk._tcp");
                return d;
            }
        }
    }
}
