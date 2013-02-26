using System;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;
using Microsoft.SPOT.Hardware;
using GHI.Premium.System;
using Microsoft.SPOT;

namespace Gadgeteer.Modules.DigitalInteractionGroup
{
    /// <summary>
    /// Open Sound Control modue for .NET Gadgeteer. Allows transmission and receiving of OSC 1.0 UDP packets.
    /// <br></br><br></br>
    /// Created from the specification at http://http://opensoundcontrol.org/spec-1_0.
    /// <br></br>
    /// Receiving OSC bundles is supported, but without posponed processing i.e. The timestamp field is ignored.
    /// <br></br>
    /// Only wildcards '*' are supported in message addresses.
    /// </summary>
    public class OSC
    {
        private Hashtable handlers = new Hashtable();
        private readonly static string BUNDLESTRING = "#bundle";
        private readonly static char[] INVALID_ADDRESS_CHARS = {' ','#','*',',','?','[',']','{','}'};
        private readonly static byte COMMA = (byte)',';
        private System.Net.Sockets.Socket receiver;
        private System.Net.Sockets.Socket transmitter;
        private OSCPacketEventHandler onReceive;

        /// <summary>
        /// Method definition for receiving an OSC message.
        /// </summary>
        /// <param name="sender">OSC module that received the message</param>
        /// <param name="address">OSC address of the packet</param>
        /// <param name="data">List of items in the received message</param>
        public delegate void OSCPacketEventHandler(OSC sender, string address, ArrayList data);
        /// <summary>
        /// UDP port that the receiver is listening to.
        /// </summary>
        public int ReceiverPort { get; private set; }
        /// <summary>
        /// UDP port that the transmitter is sending to.
        /// </summary>
        private int TransmitterPort { get; set; }
        /// <summary>
        /// True if the OSC transmitter has been started.
        /// </summary>
        public bool IsTransmitterStarted { get; private set; }
        /// <summary>
        /// True if the OSC receiver has been started.
        /// </summary>
        public bool IsReceiverStarted { get; private set; }
        /// <summary>
        /// Destination IP of transmitter (if started).
        /// </summary>
        public EndPoint DestinationIP { get; private set; }
        /// <summary>
        /// If true, an event is fired when ANY valid OSC message is received. Turn this off to avoid slow running applications. (Default = True).
        /// </summary>
        public bool ReportAllPackets { get; set; }
        /// <summary>
        /// Fired when a message any valid OSC message is received. Only fired when <see cref="ReportAllPackets"/> is True.
        /// </summary>
        public event OSCPacketEventHandler PacketReceived;


        /// <summary>
        /// Represents a specific function performed on the server when receiving messages.
        /// </summary>
        internal class OSCReceivedHandler
        {
            private OSCPacketEventHandler onReceive;
            /// <summary>
            /// Address pattern on the server which can be matched. Currently only wildcard address are supported within incoming packets e.g. '/*'
            /// </summary>
            public string Pattern { get; private set; }

            /// <summary>
            /// Creates a handler which fires an event when matching OSC messages are received.
            /// </summary>
            /// <param name="address">Address for messages to match and fire this event e.g. '/guitar/1'</param>
            public OSCReceivedHandler(string address)
            {
                if (address.IndexOfAny(INVALID_ADDRESS_CHARS) != -1)
                    throw new Exception("Only text characters are valid for OSC functions.");
                Pattern = address;
            }

            /// <summary>
            /// Event fired when a message matching the address is received.
            /// </summary>
            public event OSCPacketEventHandler PacketReceived;

            internal void Invoke(OSC sender, string address, ArrayList items)
            {
                //invoke this event...
                if (this.onReceive == null)
                {
                    this.onReceive = new OSCPacketEventHandler(this.Invoke);
                }

                if (Program.CheckAndInvoke(PacketReceived, this.onReceive, sender, address, items))
                {
                    this.PacketReceived(sender, address, items);
                }
            }
        }

        public OSC()
        {
            receiver = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            transmitter = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ReportAllPackets = true;
        }

        /// <summary>
        /// Starts an OSC transmitter to a specific location and port.
        /// </summary>
        /// <param name="destinationip">Destination IP address e.g. 192.168.1.1</param>
        /// <param name="port">Destination UDP port e.g. 8000</param>
        public void StartTransmitter(string destinationip,int port)
        {
            TransmitterPort = port;
            IsTransmitterStarted = true;
            DestinationIP = new IPEndPoint(IPAddress.Parse(destinationip), port);
        }

        /// <summary>
        /// Starts an OSC receiver listening on the specified UDP port.
        /// </summary>
        /// <param name="port">UDP port to listen on e.g. 8001</param>
        public void StartReceiver(int port)
        {
            IsReceiverStarted = true;
            ReceiverPort = port;
            receiver.Bind(new IPEndPoint(IPAddress.Any, port));
            receiver.Listen(1);

            Thread rthread = new Thread(new ThreadStart(() => {
                try
                {
                    EndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    while (true)
                    {
                        if (receiver.Poll(-1, SelectMode.SelectRead))
                        {
                            byte[] buffer = new byte[receiver.Available];
                            int bytesRead = receiver.ReceiveFrom(buffer, ref RemoteIpEndPoint);

                            //do somthing with the packet...
                            if (buffer.Length > 0)
                                ProcessMessage(buffer);
                        }
                        else
                        {
                            Thread.Sleep(10);
                        }
                    }
                }
                finally
                {
                    IsReceiverStarted = false;
                }
            }));
            rthread.Start();
        }

        private void ProcessMessage(byte[] bytesRead)
        {
            try
            {
                //if its a bundle
                if (new string(Encoding.UTF8.GetChars(Utility.ExtractRangeFromArray(bytesRead, 0, 7))) == BUNDLESTRING)
                {
                    //next 8 bytes is timetag...
                    byte[] time = Utility.ExtractRangeFromArray(bytesRead, 8, 8);

                    int pointer = 16;
                    while (pointer < bytesRead.Length)
                    {
                        //next 4 bytes from pointer is length of packet...
                        int length = BytesToInt(Utility.ExtractRangeFromArray(bytesRead, pointer, 4));
                        //get this amount of bytes for the packet...
                        ProcessMessage(Utility.ExtractRangeFromArray(bytesRead, pointer + 4, length));
                        pointer = pointer + 4 + length;
                    }
                }
                else
                {
                    ProcessPacket(bytesRead);
                }
            }
            catch
            { 
            //do nothing and return to thread.
            }
        }

        private void ProcessPacket(byte[] data)
        {
            string address = "/";
            ArrayList items = new ArrayList();
            //do the packet processing...
            
            //get the address string...
            //loop through until you reach a ','.
            int i = 0;
            for (i = 0; i < data.Length; i++)
                if (data[i] == COMMA)
                    break;
            address = new string(Encoding.UTF8.GetChars(Utility.ExtractRangeFromArray(data, 0, i - 1)));

            for (i=i+1; i < data.Length; i++)
            {
                if (data[i]=='i')
                {
                    items.Add(typeof(int));
                    continue;
                }
                else if (data[i]=='f')
                {
                    items.Add(typeof(float));
                    continue;
                }
                else if (data[i]=='s')
                {
                    items.Add(typeof(string));
                    continue;
                }
                else if (data[i]=='b')
                {
                    items.Add(typeof(byte[]));
                    continue;
                }
                else if (data[i] != 0x0)
                {
                    return;//this is an invalid packet (type not recognised).
                }
                else
                {
                    break;//end of string
                }
            }

            int counter = i + (4-(i%4));
            for (i = 0; i < items.Count; i++)
            {
                if (items[i] == typeof(int))
                {
                    items[i] = (int)BytesToInt(Utility.ExtractRangeFromArray(data, counter, 4));
                    counter += 4;
                    continue;
                } else if (items[i] == typeof(float))
                {
                    float o = 0;
                    Util.ExtractValueFromArray(out o,swapEndian(Utility.ExtractRangeFromArray(data, counter, 4)),0);
                    items[i] = o;
                    counter += 4;
                    continue;
                }
                else if (items[i] == typeof(string))
                {
                    //loop through from the current location until its a null (0x0), then work out padding... character...
                    string s = "";
                    while (data[counter] != 0x0)
                    {
                        s += (char)data[counter];
                        counter++;
                    }
                    items[i] = s;
                    //if counter not multiple of 4, add the rest to it...
                    counter = counter + (4 - (counter % 4));
                    continue;
                }
                else if (items[i] == typeof(byte[]))
                {
                    int length = (int)Utility.ExtractValueFromArray(data, counter, 4);
                    items[i] = Utility.ExtractRangeFromArray(data, counter + 4, length);
                    counter = counter + 4 + length;
                    continue;
                }
            }

            DispatchPacket(this,address,items);
        }

        private void DispatchSingle(OSC sender, string address, ArrayList items)
        {
            if (this.onReceive == null)
            {
                this.onReceive = new OSCPacketEventHandler(this.DispatchSingle);
            }

            //for every packet
            if (ReportAllPackets && Program.CheckAndInvoke(PacketReceived, this.onReceive, sender, address, items))
            {
                this.PacketReceived(sender, address, items);
            }
        }

        private void DispatchPacket(OSC sender,string address, ArrayList items)
        {
            DispatchSingle(sender,address,items);

            //match address in the handler array and dispatch
            string[] add = address.Split('/');
            foreach (string h in handlers.Keys)
            {
                if (MatchHandler(add,h))
                {
                    (handlers[h] as OSCReceivedHandler).Invoke(this,address,items);
                }
            }
        }

        private bool MatchHandler(string[] address, string handler)
        {
            string[] hand = handler.Split('/');
            if (hand.Equals(address))
                return true;
            if (hand.Length != address.Length)
                return false;

            //for each segment...
            for (int i = 0; i < address.Length; i++)
            {
                if (address[i] != "*" && address[i] != hand[i])
                {
                    return false;
                }
            }
            //all correct
            return true;
        }

        /// <summary>
        /// Adds a handler for a server method to be called when a message with a specific address is received.
        /// </summary>
        /// <param name="pattern">OSC address to match incoming messages against.</param>
        /// <returns>OSCReceivedHandler containing the event.</returns>
        internal OSCReceivedHandler AddHandler(string pattern)
        {
            if (pattern.IndexOfAny(INVALID_ADDRESS_CHARS) != -1)
                throw new Exception("Invalid character in OSC address");
            var o = new OSCReceivedHandler(pattern);
            handlers.Add(pattern, o);
            return o;
        }

        private byte[] IntToBytes(int i)
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte)(i >> 24);
            bytes[1] = (byte)(i >> 16);
            bytes[2] = (byte)(i >> 8);
            bytes[3] = (byte)i;
            return bytes;
        }

        private int BytesToInt(byte[] b)
        {
            return (b[0] << 24) + (b[1] << 16) + (b[2] << 8) + b[3];
        }

        private byte[] GetTypeStringandData(object[] l)
        {
            byte[] data = new byte[0];
            string r = ",";
            foreach (var i in l)
            {
                if (i is byte[])
                {
                    r += "b";
                    byte[] b = new byte[4];
                    data = Utility.CombineArrays(data, PadToFour(IntToBytes((int)(i as byte[]).Length)));
                    data = Utility.CombineArrays(data, PadToFour(i as byte[]));
                } else if (i is Int32)
                {
                    r += "i";
                    data = Utility.CombineArrays(data, PadToFour(IntToBytes((int)i)));
                }
                else if (i is float)
                {
                    r += "f";
                    byte[] d = new byte[4];
                    Util.InsertValueIntoArray((float)i, d, 0);
                    
                    data = Utility.CombineArrays(data, swapEndian(d));
                }
                else if (i is String)
                {
                    r += "s";
                    data = Utility.CombineArrays(data, PadToFour(Utility.CombineArrays(Encoding.UTF8.GetBytes(i as string),new byte[]{0x0})));
                }
                else
                {
                    throw new Exception("Invalid data type in OSC Message: " + i.GetType().ToString());
                }
            }

            byte[] types = PadToFour(Utility.CombineArrays(Encoding.UTF8.GetBytes(r),new byte[]{0x0}));
            
            return Utility.CombineArrays(types,data);
        }

        private byte[] swapEndian(byte[] data)
        {
            byte[] swapped = new byte[data.Length];
            for (int i = data.Length - 1, j = 0; i >= 0; i--, j++)
            {
                swapped[j] = data[i];
            }
            return swapped;
        }

        private byte[] PadToFour(byte[] data)
        {
            int missing = (4 - data.Length % 4);
            if (missing == 4)
            {
                return data;
            }
            else
            {
                //add extra blanks to make it up to multiple of 4.
                byte[] result = new byte[data.Length + missing];
                data.CopyTo(result, 0);
                return result;
            }
        }

        /// <summary>
        /// Transmit an OSC message to the desitination.
        /// </summary>
        /// <param name="address">Valid OSC address pattern.</param>
        /// <param name="items">List of values to be transmitted (int, float, string or byte[]).</param>
        public void SendMessage(string address, params object[] items)
        {
            if (IsTransmitterStarted)
            {
                //check begins with /
                if (address[0] != '/')
                    throw new Exception("Invalid OSC Address");

                byte[] add = PadToFour(Utility.CombineArrays(Encoding.UTF8.GetBytes(address),new byte[] {0x0}));
                byte[] tps = GetTypeStringandData(items);
                transmitter.SendTo(Utility.CombineArrays(add,tps), DestinationIP);
            }
            else
            {
                throw new Exception("Transmitter not Started");
            }
        }
    }
}
