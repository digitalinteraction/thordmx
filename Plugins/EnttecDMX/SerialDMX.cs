using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO.Ports;
using System.ComponentModel;
using CommandLine;

namespace IPS.Communication.Plugins
{
    public class RuleConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            //true means show a combobox
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            //true will limit to list. false will show the list, 
            //but allow free-form entry
            return true;
        }

        public override System.ComponentModel.TypeConverter.StandardValuesCollection
               GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(SerialDMX.ListAvailablePorts());
        }
    }


	public class SerialDMX:IDmxOutput
	{

string FIRMWARE_FILENAME  		= "main.bin";
string VERSION_STRING		 	= "1.44";

int FIRMWARE_FLASH_PAGES = 96;
int FLASH_PAGE_SIZE = 64; //in bytes

int F1_KEY_VALUE = 59;
int F2_KEY_VALUE = 60;
int F4_KEY_VALUE = 62;
int F8_KEY_VALUE = 66;

byte SOM_VALUE = 0x7E;
byte EOM_VALUE = 0xE7;

byte REPROGRAM_FIRMWARE_LABEL 	= 1;
byte PROGRAM_FLASH_PAGE_LABEL	= 2;
byte RECEIVED_DMX_LABEL			= 5;
byte OUTPUT_ONLY_SEND_DMX_LABEL	= 6;
byte RDM_SEND_DMX_LABEL			= 7;
byte INVALID_LABEL 				= 0xFF;
	
private SerialPort ser;
private string portnum="COM1";
private byte[] buffer = new byte[513];

public byte[] GetChannelData()
{
	return buffer;
}

private string port="COM1";

[Option("p", "port (serial)", Required = false, HelpText = "EnttecDMX Serial Port")]
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

public static string[] ListAvailablePorts()
{
	  return System.IO.Ports.SerialPort.GetPortNames();
}

private void open_serial_port(string port_num)
{
	//// Initialize serial port

	//// Open serial port with receive timeout
	ser = new SerialPort(port_num, 57600);
	ser.Open();
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


public void UpdateChannel(int channel, int value)
{
	if (channel < 1 || channel > 512)
		return;
	buffer[channel] = (byte)value;
	buffer[0]=0;
	transmit_to_widget(OUTPUT_ONLY_SEND_DMX_LABEL, buffer);
}

public void UpdateChannels(byte[] channels)
{
	buffer = channels;
	buffer[0] = 0;
	transmit_to_widget(OUTPUT_ONLY_SEND_DMX_LABEL, buffer);
}

public void BlackOut()
{
	for (int i = 0; i < buffer.Length; i++)
	{
		buffer[i] = 0;
	}
	transmit_to_widget(OUTPUT_ONLY_SEND_DMX_LABEL, buffer);
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

public SerialDMX()
{
    ICommandLineParser parser = new CommandLineParser();
    if (parser.ParseArguments(Environment.GetCommandLineArgs(), this))
    {
        // consume Options type properties
    }
}

public void Start()
{
    // Main procedure
    Debug.Print("\nDMX USB Pro Widget, version " + (VERSION_STRING));
    // Initialize serial interface
    this.portnum = SerialPort;
    open_serial_port(portnum);
}

		public void Stop()
		{
			close_serial_port();
		}


        public string Name
        {
            get { return "Enttec DMX"; }
        }
    }
}