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
using CommandLine;
using System.IO.Ports;

namespace IPS.Communication.Plugins
{
    public class EnttecDMXEventClient:IEventClient,IServerService
    {
        string FIRMWARE_FILENAME = "main.bin";
        string VERSION_STRING = "1.44";

        int FIRMWARE_FLASH_PAGES = 96;
        int FLASH_PAGE_SIZE = 64; //in bytes

        int F1_KEY_VALUE = 59;
        int F2_KEY_VALUE = 60;
        int F4_KEY_VALUE = 62;
        int F8_KEY_VALUE = 66;

        byte SOM_VALUE = 0x7E;
        byte EOM_VALUE = 0xE7;

        byte REPROGRAM_FIRMWARE_LABEL = 1;
        byte PROGRAM_FLASH_PAGE_LABEL = 2;
        byte RECEIVED_DMX_LABEL = 5;
        byte OUTPUT_ONLY_SEND_DMX_LABEL = 6;
        byte RDM_SEND_DMX_LABEL = 7;
        byte INVALID_LABEL = 0xFF;

        private SerialPort ser;

        Dictionary<string, Dictionary<string, DmxEventHandler>> handlers = new Dictionary<string, Dictionary<string, DmxEventHandler>>();

        private string port="COM1";

        [Option("i", "input port (serial input)", Required = false, HelpText = "EnttecDMX Serial Port (For Input)")]
        [TypeConverter(typeof(RuleConverter))]
        public string SerialPort
        {
            get
            {
                string S = "";
                if (port != null)
                {
                    S = port;
                }
                else
                {
                    if (ListAvailablePorts().Length > 0)
                    {
                        //Sort the list before displaying it
                        Array.Sort(ListAvailablePorts());
                        S = ListAvailablePorts()[0];
                    }
                }
                return S;
            }
            set { port = value; }
        }

        

        public void Connect()
        {
            // Main procedure
            Debug.Print("\nDMX USB Pro Widget Input, version " + (VERSION_STRING));
            // Initialize serial interface
            this.portnum = SerialPort;
            open_serial_port(portnum);
            start_dmx_receive();
            try
            {
                Thread t = new Thread(new ThreadStart(() => {
                    while (true)
                    {
                        //get bytes...
                        byte b = (byte)ser.ReadByte();
                        //if its the start of a packet...7E
                        if (b == SOM_VALUE)
                        {
                            //get rest of packet...
                            b = (byte)ser.ReadByte();
                            if (b == RECEIVED_DMX_LABEL)
                            {
                                int g = ser.ReadByte();
                                int length = g + (ser.ReadByte() << 8);
                                byte[] data = new byte[length];
                                for (int i = 0; i < length; i++)
                                {
                                    data[i] = (byte)ser.ReadByte();
                                }
                                b = (byte)ser.ReadByte();
                                if (b == EOM_VALUE)
                                {
                                    Array.Copy(data,1, tempvals,0,length-1);
                                    ProcessMessage(tempvals);
                                }
                            }
                        }

                    }
                }));
                t.Start();
            }
            catch 
            { 
            
            }

        }

        byte[] tempvals = new byte[512];

        public void start_dmx_receive()
        {
	        //# Change DMX direction to input
	        //int_data = [0] + range(0,256) + range(0,256);
	        //byte[] msg_data = [chr(int_data[j]) for j in range(len(int_data))];
            //send 512 bytes of 0's
	        flush_rx();
	        transmit_to_widget(RDM_SEND_DMX_LABEL, new byte[512]);
        }

        byte[] oldvals = new byte[512];

        //MessagePackSerializer<Dictionary<string, object>> serializer = MessagePackSerializer.Create<Dictionary<string, object>>();

        byte?[] temp = new byte?[512];

        private void ProcessMessage(byte[] vals)
        {
            //string device = c.Device;
            string address;
            string device = "<dmx input>";

            //Console.WriteLine(vals[0]);

            //decide to process the message if its not exactly the same message...
            if (Enumerable.SequenceEqual(vals,oldvals))
            {
                return;
            }

            temp = vals.Cast<byte?>().ToArray();

            //work out which ones have changed and set the rest to null
            for (int i = 0; i < vals.Length; i++)
            {
                if ((byte)vals[i] == (byte)oldvals[i])
                    temp[i] = null;
            }

            Array.Copy(vals,oldvals,512);

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


        public string Name
        {
            get { return "EnttecDMX Input"; }
        }

        [Browsable(false)]
        public Dictionary<int, string> Services
        {
            get
            {
                return new Dictionary<int,string>();
            }
        }

        public static string[] ListAvailablePorts()
        {
	          return System.IO.Ports.SerialPort.GetPortNames();
        }

        private void open_serial_port(string port_num)
        {
	        //// Open serial port with receive timeout
	        ser = new SerialPort(port_num, 57600);
	        ser.Open();
            start_dmx_receive();
        }

        private void close_serial_port()
        {
	        ser.Close();
        }

        private void flush_rx()
        {
	        // Discard any queued receive data
	        byte[] tmp = new byte[600];
	        ser.Read(tmp,0,600);
        }

        private void transmit_to_widget(byte label, byte[] data)
        {
	        byte[] transmit = new byte[data.Length + 5];

	        transmit[0] = SOM_VALUE;
	        transmit[1] = label;
	        transmit[2] = (byte)(data.Length & 0xFF);
	        transmit[3] = (byte)(((data.Length >> 8)) & 0xFF);
	        for (int i = 0; i < data.Length; i++)
	        {
		        transmit[i + 4] = data[i];
	        }
	        transmit[data.Length + 4] = EOM_VALUE;

	        if (ser.IsOpen)
	        {
		        ser.Write(transmit, 0, transmit.Length);
	        }
        }

        public EnttecDMXEventClient()
        {
            ICommandLineParser parser = new CommandLineParser();
            if (parser.ParseArguments(Environment.GetCommandLineArgs(), this))
            {
                // consume Options type properties
            }
        }
        private string portnum = "COM1";

        public void Disconnect()
        {
            close_serial_port();
        }
    }
}
