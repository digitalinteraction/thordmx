using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPS.Communication;
using Alchemy.Classes;
using System.Net;
using System.ComponentModel;

namespace IPS.Plugins.WebSockets
{
    public class WebSocketsOutput:IDmxOutput,IServerService
    {
        byte[] data = new byte[512];
        public int Port { get; private set; }
        List<UserContext> Clients = new List<UserContext>();
        DateTime last;

        public WebSocketsOutput()
        {
            Port = 8284;
        }

        public void UpdateChannel(int channel, int value)
        {
            data[channel] = (byte)value;
            Push();
        }

        public void UpdateChannels(byte[] chans)
        {
            data = chans;
        }

        private void Push()
        {
            if (DateTime.Now - last > TimeSpan.FromMilliseconds(100))
            {


                string[] ints = data.Select(x => ((byte)x).ToString()).ToArray();

                string idata = "{\"command\":\"update\",\"channels\":[" + ints.Aggregate((o, e) => o + "," + e) + "]}";
                Clients.ForEach(new Action<UserContext>((ox) => { ox.Send(idata); }));
                last = DateTime.Now;
            }
        }


        Alchemy.WebSocketServer sse;
        public void BlackOut()
        {
            data = new byte[512];
        }

        public byte[] GetChannelData()
        {
            return data;
        }

        void OnConnected(UserContext context)
        {
            Clients.Add(context);
        }

        void OnDisconnect(UserContext context)
        {
            Clients.Remove(context);
        }

        public void Start()
        {
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

        public void Stop()
        {
            sse.Stop();
        }

        public string Name
        {
            get { return "WebSockets Output"; }
        }

        [Browsable(false)]
        public Dictionary<int, string> Services
        {
            get
            {
                var d = new Dictionary<int, string>();
                d.Add(Port, "_wsko._tcp");
                return d;
            }
        }
    }
}
