using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IPS.Controller;
using System.Net;
using IPS.SharedObjects;
using System.IO;
using System.Threading;

namespace ExampleClient
{
    public partial class Form1 : Form
    {
        IController controller;
        IFeedback feedback;
        //create venue parser
        ServerVenues venues = new ServerVenues();
        //create venue finder
        ServerFinder finder = new ServerFinder();
        List<Label> labels = new List<Label>();

        public Form1()
        {
            InitializeComponent();
            //find server
            finder.OnServerFound += new Action<string,string>(finder_OnServerFound);
        }

        bool done = false;

        void finder_OnServerFound(string obj,string ip)
        {
                //start client
                controller = new IPS.Communication.Plugins.OSCController(ip);
                controller.Start();

                feedback = new IPS.Communication.Plugins.UDPBroadcastFeedback();
                feedback.OnUpdate += new Action<byte[]>(feedback_OnUpdate);

                //get venue
                WebClient web = new WebClient();
                web.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(web_DownloadFileCompleted);
                web.DownloadFileAsync(new Uri("http://" + obj + ":1235/venue.thor"), Directory.GetCurrentDirectory() + "\\temp.thor", null);
        }

        void feedback_OnUpdate(byte[] obj)
        {
            labels.ForEach((o) => { o.Text = obj[(int)o.Tag].ToString(); });
        }

        void web_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Thread.Sleep(1000);
            //load venue
            Rig r = venues.LoadFromFile(Directory.GetCurrentDirectory() + "\\temp.thor");
            //create controls
            this.BeginInvoke(new Action<Rig>(CreateControls),r);
        }


        void CreateControls(Rig r)
        {
            int i =0;
            foreach (var l in r.Lights)
            {
                TrackBar t = new TrackBar();
                t.Orientation = Orientation.Vertical;
                t.Maximum = 255;
                t.Minimum = 0;
                t.Height = 200;
                t.Tag = l.Channel;
                t.ValueChanged+=new EventHandler(t_ValueChanged);
                t.Width = 20;
                t.TickStyle = TickStyle.None;
                t.Location = new Point(i * 40, 20);
                this.Controls.Add(t);
                i++;
                Label la = new Label();
                la.Tag = l.Channel;
                la.Width = 20;
                la.Location = new Point(i * 40, 0);
                labels.Add(la);
                this.Controls.Add(la);
            }
        }

        void t_ValueChanged(object sender, EventArgs e)
        {
            controller.UpdateValue((int)(sender as TrackBar).Tag, (sender as TrackBar).Value);
        }
    }
}
