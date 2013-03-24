using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using System.ComponentModel;
using System.Net;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using IPS.SharedObjects;
using IPS.Communication;
using System.Linq;
using IPS.Controller;
using IPS.Communication.Plugins;

namespace IPS.TabletPainting
{
    /// <summary>
    /// Interaction logic for SurfaceWindow1.xaml
    /// </summary>
    public partial class SurfaceWindow1 : SurfaceWindow
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// 

        private delegate void StringMethodInvoker(string s, string t);
        ServerFinder finder = new ServerFinder();
        public SurfaceWindow1()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            //TODO REMOVE THIS FOR LOGIN
            //setup.Visibility = System.Windows.Visibility.Hidden;

            
            finder.OnServerFound += new Action<string,string>(finder_OnServerFound);

            connectionip = "192.168.1.100";
        }

        void finder_OnServerFound(string obj,string ip)
        {
            this.Dispatcher.BeginInvoke(new Action<string,string>((s,i) =>
            {
                SurfaceListBoxItem it = new SurfaceListBoxItem();
                it.Content = s;
                it.Tag = i;
                //it.Tag = Dns.Resolve(obj).AddressList.First().ToString();
                servers.Items.Add(it);
            }), new object[] { obj,ip });
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(((Exception)e.ExceptionObject).StackTrace);
            MessageBox.Show(((Exception)e.ExceptionObject).Message);
            MessageBox.Show(((Exception)e.ExceptionObject).InnerException.Message);
        }

        string connectionip;
        bool isLoaded = false;
        string devicename;
        Uri targetip;
        public IController dmxcontroller {get;set;}
        IFeedback feedback;
        Rig temprig;

        #region Initial Login
        private void surfaceButton10_Click(object sender, RoutedEventArgs e)
        {
            finder.Stop();
            isLoaded = true;
            if (connectionip == "")
                return;
            if (address.Text != "")
                connectionip = address.Text;

            if (connectionip != "")
            {
                try
                {
                    devicename = "tabletpainting/" + Dns.GetHostName(); ;
                    targetip = new Uri("http://" + connectionip + ":1235/venue.thor");

                    //sets up osc transmitter
                    dmxcontroller = new OSCController(connectionip);
                    dmxcontroller.DeviceName = devicename + "/" + Dns.GetHostName();
                    dmxcontroller.Start();

                    feedback = new UDPBroadcastFeedback();
                    feedback.OnUpdate += new Action<byte[]>(feedback_OnUpdate);

                    BackgroundWorker worker = new BackgroundWorker();
                    worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((o, a) =>
                    {
                        if (temprig != null)
                        {
                            setup.Visibility = Visibility.Hidden;
                            BitmapImage image = new BitmapImage(new Uri("venue.jpg", UriKind.Relative));

                            rigview.SetRig(temprig, image);
                            rigview.Window = this;

                            //cuedisplay.setRig(temprig, image);
                        }
                        progress.Visibility = Visibility.Hidden;
                        //setup receive loop
                    });

                    worker.DoWork += new DoWorkEventHandler((o, a) =>
                    {
                        try
                        {


                            temprig = Rig.LoadRigFromServer(connectionip);
                        }
                        catch (Exception ex)
                        {
                            //fail...
                            MessageBox.Show(ex.Message);
                            //Microsoft.Surface.UserNotifications.RequestNotification("Connection Error", ex.Message);
                        }
                    });
                    progress.Visibility = Visibility.Visible;
                    worker.RunWorkerAsync();

                }
                catch (Exception ex)
                {
                    //Microsoft.Surface.UserNotifications.RequestNotification("Connection Error", ex.Message);
                }
            }
        }

        void feedback_OnUpdate(byte[] vals)
        {
            //TODO -- rate limiting from the other tablet app
            Dispatcher.BeginInvoke(new MethodInvoker(delegate()
            {
                foreach (Lamp l in rigview.lights.Children)
                {
                    l.Level = vals[l.Channel] / 255f;
                    //Console.WriteLine("c:"+l.Channel+" v: " + l.Level);
                }
            }));
        }
        #endregion

        private void servers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            connectionip = (string)((SurfaceListBoxItem)servers.SelectedItem).Tag;
            //Uri uri = new Uri("http://" + connectionip);
            //connectionip = uri.Host;
        }

        delegate void MethodInvoker();

        private void SurfaceWindow_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void surfaceButton11_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void surfaceButton1_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}