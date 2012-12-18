using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using MiniHttpd;
using System.IO;
using System.Diagnostics;
using ICSharpCode.SharpZipLib.Zip;
using JsonExSerializer;
using Dolinay;
using IPS.Communication;
using IPS.SharedObjects;
using Messir.Windows.Forms;
using CommandLine;
using log4net.Core;
using log4net.Config;
using log4net.Appender;
using log4net;

namespace IPS.Server
{
    public partial class MainForm : Form
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Option("l", "port (lcd)", Required = false, HelpText = "LCD Serial Port")]
        public string LoadedLCDPort { get; set; }
        [Option("s", "start", Required = false, HelpText = "Start Server")]
        public bool LoadedStart { get; set; }

        public DmxManager DmxController { get { return osc; } private set { osc = value; } }

        public ServerCueStack CueStack { get; set; }
        public ServerVenues Venues { get; set; }

        DmxManager osc;
        HttpWebServer server = new HttpWebServer();

        FileStream backup;

        public int[] ChannelValues
        {
            get;
            set;
        }
        List<Level> labels = new List<Level>();
        private Thread logWatcher;
        private bool logWatching = true;
        private log4net.Appender.MemoryAppender logger;

        private void LogWatcher()
        {
            // we loop until the Form is closed  
            while (logWatching)
            {
                LoggingEvent[] events = logger.GetEvents();
                if (events != null && events.Length > 0)
                {
                    // if there are events, we clear them from the logger,  
                    // since we're done with them  
                    logger.Clear();
                    foreach (LoggingEvent ev in events)
                    {
                        string line = ev.RenderedMessage;
                        this.BeginInvoke(new Action<string>((s) =>
                        {
                            // the line we want to log  

                            // don't want to grow this log indefinetly, so limit to 100 lines  
                            if (status.Items.Count > 99)
                            {
                                status.Items.RemoveAt(status.Items.Count);
                            }
                            status.Items.Insert(0, s);
                            status.ForeColor = Color.Black;
                            if (ev.Level == log4net.Core.Level.Error)
                                status.ForeColor = Color.Red;
                            if (ev.Level == log4net.Core.Level.Info)
                                status.ForeColor = Color.Green;
                        }), line);
                    }
                }
                // nap for a while, don't need the events on the millisecond.  
                Thread.Sleep(500);
            }
        }  

        public MainForm(string[] args)
        {
            InitializeComponent();
            log4net.Config.XmlConfigurator.Configure();

            logger = new MemoryAppender();
            var repository = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();
            repository.Root.AddAppender(logger);
            //var events = memoryAppender.GetEvents();

            //logger = new log4net.Appender.MemoryAppender();
            //log4net.Config.BasicConfigurator.Configure(logger);
            

            log.Info("Starting...");

            ICommandLineParser parser = new CommandLineParser();
            if (parser.ParseArguments(args, this))
            {
                // consume Options type properties
            }

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(currentDomain_UnhandledException);

            backup = new FileStream(Directory.GetCurrentDirectory() + "\\backup.dat", FileMode.OpenOrCreate,FileAccess.ReadWrite, FileShare.None);

            CueStack = new ServerCueStack();

            Venues = new ServerVenues();
            Venues.OnVenueChange += new Action<Rig>(Venues_OnVenueChange);

            venues.DataSource = Venues.Layouts;
            venues.DisplayMember = "Name";

            if (Venues.CurrentVenue !=null)
                venue.Text = Venues.CurrentVenue.Name;

            listBox1.DataSource = CueStack.Cues;
            listBox1.DisplayMember = "Name";

            statuslight.BackColor = Color.DarkGreen;
            devices.Add("*");

            ChannelValues = new int[512];

            server.Root = new VirtualDirectory();
            server.Port = 1235;

            DriveDirectory vd = new DriveDirectory(Directory.GetCurrentDirectory() + "\\www");
            server.Root = vd;

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\www"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\www");
            }

            if (!File.Exists(Directory.GetCurrentDirectory() + "\\www\\venue.thor"))
            {
                FileStream f = File.Create(Directory.GetCurrentDirectory() + "\\www\\venue.thor");
                f.Close();
            }

            // Start the server
            server.Start();

            int num = 1;
            for (int j = 0; j < 512; j++)
            {
                //add lables
                //for (int i = 0; i < 10; i++)
                //{
                //    Label l1 = new Label();
                //    flowLayoutPanel1.Controls.Add(l1);
                //    l1.Width = 25;
                //    l1.TextAlign = ContentAlignment.TopCenter;
                //    l1.ForeColor = Color.Silver;
                //    l1.Text = "" + num;
                //    num++;
                //}

                //for (int i = 0; i < 10; i++)
                //{
                //    Label l = new Label();
                //    flowLayoutPanel1.Controls.Add(l);
                //    l.Width = 25;
                //    l.Padding = new Padding(0);

                //    l.TextAlign = ContentAlignment.TopCenter;
                //    l.Text = "0";
                //    labels.Add(l);
                //}

                //for (int i = 0; i < 10; i++)
                {
                    Level l = new Level();
                    flowLayoutPanel1.Controls.Add(l);
                    l.Width = 25;
                    l.Padding = new Padding(0);
                    l.Height = 50;
                    l.Channel = num;
                    //l.Value = num;
                    //l.TextAlign = ContentAlignment.TopCenter;
                    //l.Text = "0";
                    labels.Add(l);
                    num++;
                }
            }

            

            //start Admin Server
            try
            {
                WebInterface web = new WebInterface(this);
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                //status.Items.Insert(0, e.Message);
                //status.ForeColor = Color.Red;
            }

            if (!IsRunningOnMono())
            {
                DriveDetector usb = new DriveDetector();
                usb.DeviceArrived += new DriveDetectorEventHandler(usb_DeviceArrived);
            }

            osc = new DmxManager();

            foreach (var o in osc.Clients)
            {
                var t = new TabStripButton() { Text = o.Name };
                t.Tag = o;
                t.Click += new EventHandler((oc, e) =>
                {
                    propertygrid.SelectedObject = (oc as TabStripButton).Tag;
                });
                settings.Items.Add(t);
            }

            foreach (var o in osc.Outputs)
            {
                var t = new TabStripButton() { Text = o.Name };
                t.Tag = o;
                t.Click += new EventHandler((oc, e) =>
                {
                    propertygrid.SelectedObject = (oc as TabStripButton).Tag;
                });
                settings.Items.Add(t);
            }


            Load += new EventHandler(Form1_Load);
            
            //if args is set...


            osc.OnReceive += new DmxManager.DmxStatus(osc_OnReceive);
            osc.OnDeviceUpdate += new DmxManager.DevicesStatus(osc_OnDeviceUpdate);
        }

        void Venues_OnVenueChange(Rig obj)
        {
            this.BeginInvoke(new Action(() => {
                venue.Text = Venues.CurrentVenue.Name;               
            }));

        }

        void Form1_Load(object sender, EventArgs e)
        {
            logWatcher = new Thread(new ThreadStart(LogWatcher));
            logWatcher.Start();
            if (LoadedStart)
            {
                try
                {

                    Open();
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message);
                    //status.Items.Insert(0, ex.Message);
                    //status.ForeColor = Color.Red;
                }
            }

            if (LoadedLCDPort != null)
            {
                try
                {
                    lcd = new LCDDriver(LoadedLCDPort, this);
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message);
                    //status.Items.Insert(0, ex.Message);
                    //status.ForeColor = Color.Red;

                }
            }
        }

        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        void osc_OnDeviceUpdate(List<string> devices)
        {
            try
            {
                clients.BeginInvoke(new Action(() =>
                {

                    if (osc != null)
                    {
                        clients.Text = "";
                        connectedclients.Items.Clear();
                        foreach (string s in devices)
                        {
                            ListViewItem lv = new ListViewItem();
                            lv.Text = s;
                            if (s.Contains("Tablet"))
                                lv.ImageIndex = 0;
                            if (s.Contains("Surface"))
                                lv.ImageIndex = 1;
                            if (s.Contains("Android"))
                                lv.ImageIndex = 2;
                            if (s.Contains("Painting"))
                                lv.ImageIndex = 3;

                            clients.Text = clients.Text + ", " + s;
                            connectedclients.Items.Add(s);
                        }
                    }


                }));
            }
            catch
            { }
        }



        void usb_DeviceArrived(object sender, DriveDetectorEventArgs e)
        {
            //find venue file, and replace...
            if (File.Exists(e.Drive + "venue.thor"))
            {
                if (lcd != null)
                {
                    lcd.CopyMode();
                }
                string filename = Directory.GetCurrentDirectory() + "\\www\\layout_venue_"+DateTime.Now.Ticks + ".thor";
                File.Copy(e.Drive + "venue.thor", filename, true);
                Venues.AddVenue(filename);
                Thread.Sleep(1000);
                if (lcd != null)
                {
                    lcd.NormalMode();
                }
            }
        }

        LCDDriver lcd = null;

        void currentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (lcd != null)
                lcd.ErrorScreen();

            //StreamWriter str = new StreamWriter(Directory.GetCurrentDirectory() + "\\log.txt",true);
            //str.WriteLine(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongDateString((Exception)(e.ExceptionObject)).Source + ((Exception)(e.ExceptionObject)).StackTrace + ((Exception)(e.ExceptionObject)).Message);
            //str.Close();

            log.Fatal("Fatal Error", e.ExceptionObject as Exception);

            Environment.Exit(0);
        }

        

        int port = 1234;
        List<string> devices = new List<string>();

        private void Open()
        {       
            try
            {

                IsRunning = true;
                //osc.ValidDevices.AddRange(devices.ToList());
                osc.ValidDevices.Add("*");
                log.Info("Connected && Receiving Events");
                
                //status.ForeColor = Color.Green;
                textBox1.Enabled = false;
                //comports.Enabled = false;
                button1.Hide();
                button2.Show();

                DmxController.Start();

                if (File.Exists(Directory.GetCurrentDirectory() + "\\backup.dat"))
                {
                    byte[] chans = new byte[513];
                    backup.Read(chans,0,513);
                    osc.UpdateChannels(chans);
                }

                
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                log.Error(ex.Message);

                //status.BeginInvoke(new Action(() =>
                //{
                    //status.Items.Insert(0,ex.Message);
                    //status.ForeColor = Color.Red;
                //}));
                
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                 Open();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                //status.Items.Insert(0,ex.Message);
                //status.ForeColor = Color.Red;

                propertygrid.Enabled = false;
            }
        }


        byte[] channels = new byte[513];

        byte[] lastupdate = new byte[513];


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            lock (lastupdate)
            {
                if (lastupdate.Length!=0)
                {
                    try
                    {
                        //run this is same thread;
                        for (int i = 1; i < 512; i++)
                        {
                            if (labels.Count >= i && ChannelValues[i] != lastupdate[i])
                            {
                                labels[i - 1].Value = lastupdate[i];
                            }
                            ChannelValues[i] = lastupdate[i];
                        }

                    }
                    catch (Exception ex)
                    {

                    }
                }
            }



            statuslight.BackColor = Color.Green;
            statuslight.BackColor = Color.DarkGreen;
        }


        void osc_OnReceive(byte[] channels)
        {
            lock (lastupdate)
            {
                channels.CopyTo(lastupdate,0);
            }
            Invalidate();
            if (OnChannelUpdate != null)
                OnChannelUpdate(channels);
            if (lcd != null)
            {
                lcd.UpdateValues(ChannelValues);
            }
            //

            //if (labels[0].InvokeRequired)
            //{
            //    labels[0].BeginInvoke(new DmxManager.DmxStatus(osc_OnReceive), new object[] { channels });
            //}
            //else
            //{

                
            //}
            

            //this.channels = channels;

            ////save backup channels for restart...
            
            if (DateTime.Now - last > TimeSpan.FromMilliseconds(500))
            {
                last = DateTime.Now;
                backup.Position = 0;
                lock (lastupdate)
                {
                    backup.Write(lastupdate, 0, lastupdate.Length);
                }
                backup.Flush();
                backup.Position = 0;
            }
        }

        DateTime last = new DateTime();

        public event DmxManager.DmxStatus OnChannelUpdate;

        private void comports_SelectedIndexChanged(object sender, EventArgs e)
        {
            //comport = ((string)comports.SelectedItem);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            devices.Clear();
            if (textBox1.Text.Length == 0)
                devices.Add("*");

            try
            {
                string[] devs = textBox1.Text.Split(',');

                foreach (string d in devs)
                {
                    devices.Add(d.Trim());
                }
            }
            catch
            {
                log.Error("Invalid devices!");
                //status.Items.Insert(0,"Invalid devices!");
                //status.ForeColor = Color.Red;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            logWatching = false;
            logWatcher.Join();  
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }

        private bool closed = false;

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
            if (!closed)
            {
                notifyIcon1.ShowBalloonTip(1000, "NUILight Server", "DMX server is still running, right click on this icon to close", ToolTipIcon.Info);
                closed = true;
            }
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            if (((MouseEventArgs)e).Button == MouseButtons.Left)
                this.Show();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            server.Stop();
            Environment.Exit(0);
        }


        public string SystemName
        {
            get { return System.Environment.MachineName; }
        }

        public string AllowedDevices
        {
            get
            {
                string o = "";
                foreach (string d in devices)
                {
                    o += d + ", ";
                }
                return o;
            }
            set
            {
                devices.Clear();
                if (value.Length == 0)
                    devices.Add("*");
                try
                {
                    string[] devs = value.Split(',');

                    foreach (string d in devs)
                    {
                        devices.Add(d.Trim());
                    }
                }
                catch
                {

                }
            }
        }

        public bool IsVirtual
        {
            get;
            set;
        }

        public void RestartServer()
        {
            if (osc != null)
            {
                osc.Close();
            }
            Thread.Sleep(1000);
            Open();
            IsRunning = true;
        }

        public string ConnectedClients
        {
            get { return clients.Text; }
        }

        public bool IsRunning
        {
            get;
            set;
        }

        //OutputDevice midiout = new OutputDevice(0);
        private void button3_Click(object sender, EventArgs e)
        {

            //midiout.Send(msg.Result);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                CueStack.Cues.Remove((ServerCue)listBox1.SelectedItem);
                listBox1.DataSource = null;
                listBox1.DataSource = CueStack.Cues;
                listBox1.DisplayMember = "Name";
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            //use current values as the cue...

            string n = "New Cue";
            var result = InputBox.Show("Enter Cue Name","New Server Cue");
            if (result.ReturnCode == System.Windows.Forms.DialogResult.OK)
            {
                ServerCue cue = new ServerCue(channels.Select((o) => (int)o).ToArray(), result.Text);
                CueStack.Cues.Add(cue);
                listBox1.DataSource = null;
                listBox1.DataSource = CueStack.Cues;
                listBox1.DisplayMember = "Name";
            }
        }

        public void NewCue(string name)
        {
            ServerCue cue = new ServerCue(channels.Select((o) => (int)o).ToArray(),name);
            CueStack.Cues.Add(cue);
            listBox1.BeginInvoke(new Action(() =>
            {
                listBox1.DataSource = null;
                listBox1.DataSource = CueStack.Cues;
                listBox1.DisplayMember = "Name";
            }));
        }

        internal void PlayCue(int p)
        {
            byte[] outp = CueStack.GetCombinedCue(CueStack.Cues[p], channels);
            DmxController.UpdateChannels(outp);
        }

        ~MainForm()
        {
            backup.Close();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (venues.SelectedItem != null)
            {
                Venues.RemoveVenue(venues.SelectedItem as Rig);
                venues.DataSource = null;
                venues.DataSource = Venues.Layouts;
                venues.DisplayMember = "Name";
            }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (openFileDialog1.FileName != "")
                {
                    try
                    {
                        Venues.AddVenue(openFileDialog1.FileName);
                        venues.DataSource = null;
                        venues.DataSource = Venues.Layouts;
                        venues.DisplayMember = "Name";
                    }
                    catch
                    {
                        log.Error("Problem loading venue file!");
                        //status.Items.Insert(0, "Problem loading venue file!");
                        //status.ForeColor = Color.Red;
                    }
                }
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (venues.SelectedItem != null)
            {
                Venues.MakeCurrent(venues.SelectedItem as Rig);
                
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            //stop server
            IsRunning = false;
            //osc.ValidDevices.AddRange(devices.ToList());
            log.Info("Server Stopped");
            //status.Items.Insert(0, "Server Stopped");
            textBox1.Enabled = true;
            propertygrid.Enabled = true;
            //comports.Enabled = false;

            DmxController.Stop();
            button1.Show();
            button2.Hide();

        }
    }
}
