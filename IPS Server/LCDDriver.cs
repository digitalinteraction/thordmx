using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Diagnostics;
using System.Threading;

namespace IPS.Server
{
    public class LCDDriver
    {
        MainForm window;

        int barstartval = 1;

        SerialPort comport;
        //192x64
        //254 103 [refID] [type] [x1] [y1] [x2] [y2]
        private void CreateBarChart()
        {
            
            for (int i=0;i<16;i++)
            {
                byte[] clear_screen = new byte[8];
                clear_screen[0] = 254;
                clear_screen[1] = 103;
                clear_screen[2] = (byte)i;//ref
                clear_screen[3] = 0;//type
                clear_screen[4] = (byte)((i * 12));//left
                clear_screen[5] = 22;//top
                clear_screen[6] = (byte)((i * 12) + 10);
                clear_screen[7] = 54;
                //Send clear screen command to display

                comport.Write(clear_screen, 0, clear_screen.Length);
                Thread.Sleep(100);
            }
            UpdateNums();
        }

        private string genspaces(string s,int ir)
        {
            string r = "";
            for (int i = 0; i < (ir - s.Length); i++)
            {
                r += " ";
            }
            return r;
        }

        private void UpdateNums()
        {
            CLS();
            Home();
            //Thread.Sleep(100);
            SetTextPosPix(0, 56);
            //Thread.Sleep(100);
            TextNoLF(1, (barstartval).ToString().PadRight(4) + "   Current Output  " + (barstartval + 15).ToString().PadLeft(4));
            Thread.Sleep(100);
            ClearBarValues();
            DefaultText();
        }

        private void ClearBarValues()
        {
            for (int i = 0; i < 16; i++)
            {
                byte[] clear_screen = new byte[4];
                clear_screen[0] = 254;
                clear_screen[1] = 105;
                clear_screen[2] = (byte)(i);
                clear_screen[3] = 0;
                //Send clear screen command to display
                comport.Write(clear_screen, 0, clear_screen.Length);
            }
        }

        //254 105 [ref] [value]
        private void SetBarValues(int[] values)
        {
            Random r = new Random();
            for (int i = 0; i < values.Length; i++)
            {
                
                byte[] clear_screen = new byte[4];
                clear_screen[0] = 254;
                clear_screen[1] = 105;
                clear_screen[2] = (byte)i;
                byte x = (byte)((values[i] / 256.0) * 32);
                //x = (byte)r.Next(31);
                clear_screen[3] = x > 31 ? (byte)31 : x;
                //Send clear screen command to display
                comport.Write(clear_screen, 0, clear_screen.Length);
                //Thread.Sleep(10);
            }
        }

        private void CLS()
        {
            byte[] clear_screen = new byte[2];
            clear_screen[0] = 254;
            clear_screen[1] = 88;
            //Send clear screen command to display
            comport.Write(clear_screen, 0, clear_screen.Length);
        }

        private void Home()
        {
            byte[] clear_screen = new byte[2];
            clear_screen[0] = 254;
            clear_screen[1] = 72;
            //Send clear screen command to display
            comport.Write(clear_screen, 0, clear_screen.Length);
        }

        private void SetTextPos(int x, int y)
        {
            byte[] clear_screen = new byte[4];
            clear_screen[0] = 254;
            clear_screen[1] = 71;
            clear_screen[2] = (byte)x;
            clear_screen[3] = (byte)y;
            //Send clear screen command to display
            comport.Write(clear_screen, 0, clear_screen.Length);
        }

        private void SetTextPosPix(int x, int y)
        {
            byte[] clear_screen = new byte[4];
            clear_screen[0] = 254;
            clear_screen[1] = 121;
            clear_screen[2] = (byte)x;
            clear_screen[3] = (byte)y;
            //Send clear screen command to display
            comport.Write(clear_screen, 0, clear_screen.Length);
        }

        private void SetFont(int id)
        {
            byte[] clear_screen = new byte[3];
            clear_screen[0] = 254;
            clear_screen[1] = 49;
            clear_screen[2] = (byte)id;
            //Send clear screen command to display
            comport.Write(clear_screen, 0, clear_screen.Length);
        }

        private void SetStartupState(int led, int state)
        {
            byte[] clear_screen = new byte[4];
            clear_screen[0] = 254;
            clear_screen[1] = 195;
            clear_screen[2] = (byte)led;
            clear_screen[3] = (byte)state;
            //Send clear screen command to display
            comport.Write(clear_screen, 0, clear_screen.Length);
        }

        private void SetBaud()
        {
            byte[] clear_screen = new byte[3];
            clear_screen[0] = 254;
            clear_screen[1] = 57;
            clear_screen[2] = 0x10;
            //Send clear screen command to display
            comport.Write(clear_screen, 0, clear_screen.Length);
        }

        private void GPIOOff(int gp)
        {
                byte[] clear_screen = new byte[3];
                clear_screen[0] = 254;
                clear_screen[1] = 87;
                clear_screen[2] = (byte)gp;
                //Send clear screen command to display
                comport.Write(clear_screen, 0, clear_screen.Length);
        }

        private void SetLED(int led, LEDColor color)
        {
            if (led == 1)
            {
                switch (color)
                {
                    case LEDColor.RED:
                        byte[] clear_screen;
                        clear_screen = new byte[3];
                        clear_screen[0] = 254;
                        clear_screen[1] = 86;
                        clear_screen[2] = 1;
                        comport.Write(clear_screen, 0, clear_screen.Length);
                        break;
                    case LEDColor.ORANGE:
                        //Send clear screen command to display
                        clear_screen = new byte[3];
                        clear_screen[0] = 254;
                        clear_screen[1] = 86;
                        clear_screen[2] = 1;
                        //Send clear screen command to display
                        comport.Write(clear_screen, 0, clear_screen.Length);
                        clear_screen = new byte[3];
                        clear_screen[0] = 254;
                        clear_screen[1] = 86;
                        clear_screen[2] = 2;
                        //Send clear screen command to display
                        comport.Write(clear_screen, 0, clear_screen.Length);
                        break;
                    case LEDColor.GREEN:
                        clear_screen = new byte[3];
                        clear_screen[0] = 254;
                        clear_screen[1] = 86;
                        clear_screen[2] = 2;
                        //Send clear screen command to display
                        comport.Write(clear_screen, 0, clear_screen.Length);
                        break;
                }

                
            }

            if (led == 2)
            {
                switch (color)
                {
                    case LEDColor.RED:
                        byte[] clear_screen;
                        clear_screen = new byte[3];
                        clear_screen[0] = 254;
                        clear_screen[1] = 86;
                        clear_screen[2] = 3;
                        comport.Write(clear_screen, 0, clear_screen.Length);
                        break;
                    case LEDColor.ORANGE:
                        //Send clear screen command to display
                        clear_screen = new byte[3];
                        clear_screen[0] = 254;
                        clear_screen[1] = 86;
                        clear_screen[2] = 3;
                        //Send clear screen command to display
                        comport.Write(clear_screen, 0, clear_screen.Length);
                        clear_screen = new byte[3];
                        clear_screen[0] = 254;
                        clear_screen[1] = 86;
                        clear_screen[2] = 4;
                        //Send clear screen command to display
                        comport.Write(clear_screen, 0, clear_screen.Length);
                        break;
                    case LEDColor.GREEN:
                        clear_screen = new byte[3];
                        clear_screen[0] = 254;
                        clear_screen[1] = 86;
                        clear_screen[2] = 4;
                        //Send clear screen command to display
                        comport.Write(clear_screen, 0, clear_screen.Length);
                        break;
                }


            }

            if (led == 3)
            {
                switch (color)
                {
                    case LEDColor.RED:
                        byte[] clear_screen;
                        clear_screen = new byte[3];
                        clear_screen[0] = 254;
                        clear_screen[1] = 86;
                        clear_screen[2] = 5;
                        comport.Write(clear_screen, 0, clear_screen.Length);
                        break;
                    case LEDColor.ORANGE:
                        //Send clear screen command to display
                        clear_screen = new byte[3];
                        clear_screen[0] = 254;
                        clear_screen[1] = 86;
                        clear_screen[2] = 5;
                        //Send clear screen command to display
                        comport.Write(clear_screen, 0, clear_screen.Length);
                        clear_screen = new byte[3];
                        clear_screen[0] = 254;
                        clear_screen[1] = 86;
                        clear_screen[2] = 6;
                        //Send clear screen command to display
                        comport.Write(clear_screen, 0, clear_screen.Length);
                        break;
                    case LEDColor.GREEN:
                        clear_screen = new byte[3];
                        clear_screen[0] = 254;
                        clear_screen[1] = 86;
                        clear_screen[2] = 6;
                        //Send clear screen command to display
                        comport.Write(clear_screen, 0, clear_screen.Length);
                        break;
                }


            }

            //turns off GPIO
            
        }

        private void Text(int font,string text)
        {
            SetFont(font);
            System.Text.Encoding ascii = System.Text.Encoding.ASCII;
            Byte[] encodedBytes = ascii.GetBytes(text+"\n");
            comport.Write(encodedBytes,0,encodedBytes.Length);
        }

        private void TextNoLF(int font, string text)
        {
            SetFont(font);
            System.Text.Encoding ascii = System.Text.Encoding.ASCII;
            Byte[] encodedBytes = ascii.GetBytes(text);
            comport.Write(encodedBytes, 0, encodedBytes.Length);
        }



        public LCDDriver(string port,MainForm window)
        {
            this.window = window;
            comport = new SerialPort();
            comport.PortName = port; //19200    //Provide the name of the port to which the display is attached
            comport.BaudRate = 115200;      //Set the baud rate to display default, 19200
            comport.DataBits = 8;      //Display uses 8N1 serial protocol
            comport.Parity = Parity.None;
            comport.StopBits = StopBits.One;
            comport.DataReceived += new SerialDataReceivedEventHandler(comport_DataReceived);
            comport.Open();

            //SetBaud();            

            System.Windows.Forms.Timer timer2 = new System.Windows.Forms.Timer();
            timer2.Interval = 500;
            timer2.Tick += new EventHandler(timer2_Tick);
            timer2.Start();

            CLS();
            Text(2,"\n      System Loaded");
            GPIOOff(1);
            GPIOOff(2);
            GPIOOff(3);
            GPIOOff(4);
            GPIOOff(5);
            GPIOOff(6);
            SetStartupState(1, 1);
            SetStartupState(2, 1);
            SetStartupState(3, 1);
            SetStartupState(4, 1);
            SetStartupState(5, 1);
            SetStartupState(6, 1);

            SetLED(1, LEDColor.GREEN);
            Thread.Sleep(100);

            CLS();
            CreateBarChart();

        }
        bool led1;

        void timer2_Tick(object sender, EventArgs e)
        {
            led1 = !led1;
            if (led1)
            {
                GPIOOff(1);
                GPIOOff(2);
                SetLED(1, LEDColor.GREEN);
                Random r = new Random(DateTime.Now.Millisecond);

                //SetBarValues(new int[] { r.Next(255), r.Next(255), r.Next(255), r.Next(255), r.Next(255), r.Next(255), r.Next(255), r.Next(255), r.Next(255), r.Next(255), r.Next(255), r.Next(255), r.Next(255), r.Next(255), r.Next(255), r.Next(255) });

            }
            else
            {
                GPIOOff(1);
                GPIOOff(2);
                SetLED(1, LEDColor.ORANGE);
            }
       }

        void comport_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string temp = comport.ReadExisting();
            char[] c = temp.ToCharArray();
            switch ((KeyType)c[0])
            {
                case KeyType.LEFT:
                    if (barstartval > 16)
                        barstartval -= 16;
                    UpdateNums();
                    break;
                case KeyType.RIGHT:
                    if (barstartval < 512-16)
                        barstartval += 16;
                    UpdateNums();
                    break;
                default:

                    break;
            }
        }

        void  ok1_MenuAction()
        {
 	        throw new NotImplementedException();
        }

        private enum KeyType:byte
        {
            UP=66,
            DOWN=72,
            LEFT=68,
            RIGHT=67,
            MIDDLE=69,
            B1=65,
            B2=71
        }
        private enum LEDColor
        {
            RED,
            GREEN,
            ORANGE
        }

        public void ErrorScreen()
        {
            //do erro screen
            CLS();
            SetLED(0, LEDColor.RED);
            SetLED(1, LEDColor.RED);
            SetLED(2, LEDColor.RED);
            Text(1, "ERROR, PLEASE RESTART!");
        }

        private void UpdateValue()
        {


        }

        public void DefaultText()
        {
            Home();
            SetTextPosPix(0, 0);
            Thread.Sleep(100);
            Text(1, "DMX Server ON " + window.SystemName + "\nVenue: " + window.Venues.CurrentVenue.Name);
            Thread.Sleep(100);
        }



        int[] last = new int[512];
        int[] cp = new int[16];

        DateTime lastupdate = DateTime.Now;

        internal void UpdateValues(int[] p)
        {
            bool flag = false;
            for (int i=barstartval;i<barstartval+16;i++)
            {
                if (last[barstartval]!=p[barstartval])
                    flag = true;
            }

            //if (flag)
            {
                Array.Copy(p, barstartval, cp, 0, 16);
                //UpdateValues(cp);
                if (lastupdate < DateTime.Now.Subtract(TimeSpan.FromMilliseconds(300)))
                {
                    SetBarValues(cp);
                    lastupdate = DateTime.Now;
                    last = (int[])p.Clone();
                }
            }
        }

        internal void CopyMode()
        {
            CLS();
            SetTextPosPix(20, 20);
            Text(2, "UPDATING VENUE FILE");
            Thread.Sleep(500);
        }

        internal void NormalMode()
        {
            CLS();
            CreateBarChart();
        }
    }
}
