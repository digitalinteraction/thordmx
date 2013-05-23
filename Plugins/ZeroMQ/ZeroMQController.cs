using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroMQ;
using System.IO;
using System.Net;
using IPS.Controller;
using ZMQ;

namespace IPS.Communication.Plugins.ZeroMQ
{
    public class ZeroMQController : IController, ILoggable
    {
        //MessagePackSerializer<ChannelData> serializer = MessagePackSerializer.Create<ChannelData>();
        Context context = new Context();
        Socket client;
        
        //MemoryStream stream = new MemoryStream();
        string server;
        public ZeroMQController(string server)
        {
           // request_socket = new ZmqPushSocket();
            this.server = server;
            live = new byte[513];
            //stream = new MemoryStream();
        }

        byte[] live;
        bool blackout = false;
        string devicename = "<unset>";

        public bool Blackout
        {
            get
            {
                return blackout;
            }
            set
            {
                blackout = true;
                DoBlackout();
            }
        }

        public string DeviceName
        {
            get
            {
                return devicename;
            }
            set
            {
                devicename = value;
            }
        }

        public byte[] LiveValues
        {
            get
            {
                return live;
            }
            set
            {
                live = value;
            }
        }

        private void Send()
        {
            client.Send(live);
        }

        public void DoBlackout()
        {
           // ChannelData c = new ChannelData();
            //c.Device = devicename;
            //c.Action = ChannelData.MessageType.BLACKOUT;
            
            //serializer.Pack(stream, c);
            //request_socket.Send(stream.ToArray());
            live = new byte[513];
            Send();
        }

        public void UpdateValue(int chan, int val)
        {
            //ChannelData c = new ChannelData();
            //c.Device = devicename;
            //c.Action = ChannelData.MessageType.UPDATEONE;
            //c.Channel = chan;
            //c.Value = (byte)val;
            //stream.Position = 0;
            //stream.Flush();
            //serializer.Pack(stream, c);
            //request_socket.Send(stream.ToArray());

            live[chan] = (byte)val;
            Send();
        }

        public void UpdateValues(int[] vals)
        {
            //ChannelData c = new ChannelData();
            //c.Device = devicename;
            //c.Action = ChannelData.MessageType.UPDATEALL;
            //c.Data = vals.Cast<int>().ToArray();
            //stream.Flush();
            //stream.Position = 0;
            //serializer.Pack(stream, c);
            //request_socket.Send(stream.ToArray());
            live = vals.Select(b => (byte)b).ToArray();
            Send();
        }


        public void Start()
        {
            //request_socket.Bind("tcp://"+server+":8887");
            client = context.Socket(SocketType.PUSH);
            client.Connect("tcp://" + server + ":8887");
        }

        public event Action<string> OnLogEvent;
        private bool debug = false;
        public bool DebugMode
        {
            set { debug = true; }
        }
    }
}
