using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using System.Net;
using System.IO;
using JsonExSerializer;
using System.Drawing;
using System.Threading;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Windows.Media.Animation;
using System.Collections;
using System.Net.Sockets;
using IPS.SharedObjects;
using IPS.Communication;
using IPS.Communication.Plugins;
using IPS.Controller;

namespace IPS.TabletDesk
{
	/// <summary>
	/// Interaction logic for SurfaceWindow1.xaml
	/// </summary>
	public partial class MainWindow : SurfaceWindow
	{
		
		private int CurrentCue = 0;
		List<Cue> cueslist = new List<Cue>();

		public Cue outgoing;
		public Cue incoming;
		public int outgoingnum;
		public int incomingnum;

		DispatcherTimer fader = new DispatcherTimer();

		public void Save()
		{
			TextWriter textWriter = new StreamWriter(@"save.xml");
			XmlSerializer x = new XmlSerializer(cueslist.GetType());
			x.Serialize(textWriter, cueslist);
			textWriter.Flush();
			textWriter.Close();
		}

		private void Load()
		{
			if (File.Exists(@"save.xml"))
			{
				TextReader textReader = new StreamReader(@"save.xml");
				XmlSerializer x = new XmlSerializer(cueslist.GetType());
				cueslist = (List<Cue>)x.Deserialize(textReader);
				textReader.Close();
			}
			cuedisplay_OnRefresh();
		}

		Cue jumpedcue = null;
		internal void LoadCurrentCue()
		{
			//jumped = true;
			//jumpedcue = ActualSelectedCue;

			NextCueToFire = ActualSelectedCue;
			//set text to be correct...
			int i=0;
			int final=0;
			foreach (Cue c in cueslist)
				{
					if (c == NextCueToFire)
						final = i;
					i++;
				}
			NextCueToFireNum = final;
		}

		//bool jumped = false;

		//USED FOR ACTUAL FADING, SHOULD NOT BE CAHNGED AFTER TIMER IS FIRED
		Cue fadedowncue;
		Cue fadeupcue;

		//NEW IMPLEMENTATION
		public Cue NextCueToFire;
		public int NextCueToFireNum = 0;
		public Cue CurrentCueOnFire;
		public int CurrentCueOnFireNum = -1;

		byte[] fadedownvals = new byte[512];

		private void FireCue()
		{
			if (CurrentCueOnFire == null)
				CurrentCueOnFire = new Cue();
			fadedowncue = CurrentCueOnFire;
			dmxcontroller.LiveValues.CopyTo(fadedownvals, 0);
			//fadedownvals = livevalues;
			fadeupcue = NextCueToFire;
			if (NextCueToFire != null)
			{
				//set the text here...
				fader.Start();
				starttime = DateTime.Now.Ticks;
				cuedisplay_OnRefresh();
				playback.CueProgress.Value = 0;
				////go to the cue in the list so you can see it.
				//foreach (CueListItem it in surfaceListBox1.Items)
				//{
				//    if (it.Cue == CurrentCueOnFire)
				//        surfaceListBox1.ScrollIntoView((SurfaceListBoxItem)it);
				//}
				//surfaceListBox1.ScrollIntoView(item);
			}
		}



		public void NextCue()//GO BUTTON
		{
			//fire the current next cue...
			FireCue();

			if (NextCueToFire != null)
			{
				//check to see if there are any more cues to be had
				//setup the next cue to fire...
				//set old ones
				CurrentCueOnFireNum = NextCueToFireNum;
				CurrentCueOnFire = NextCueToFire;
				cuedisplay_OnRefresh();

				if (cueslist.Count > CurrentCueOnFireNum + 1)
				{
					//set new ones...
					NextCueToFireNum = CurrentCueOnFireNum + 1;
					NextCueToFire = cueslist[NextCueToFireNum];
				}
			}
			else
			{
				//Microsoft.Surface.UserNotifications.RequestNotification("Cue Error", "No Cue Loaded!");
			}
		}

		long starttime = 0;

		// There are 10,000 ticks in a millisecond. fadeup and down time is in seconds
		void fader_Tick(object sender, EventArgs e)
		{
			//Debug.Print(".");
			long now = DateTime.Now.Ticks;
			//Debug.Print("Begin: " + now);
			long diff = (now - starttime) / 1000;

			////stops timer if things are out of time range
			if (diff > (fadedowncue.fadedown * 10000) && diff > (fadeupcue.fadeup * 10000))
			{
				fader.Stop();
			}

			////calculate current lighting values...
			int[] vals = new int[512];

			double inc = diff/(fadeupcue.fadeup*10000);
			if (inc > 1)
				inc = 1;
			if (inc < 0)
				inc = 0;
			//Debug.Print("Inc MX: " + inc);
			double outg = 1 - (diff/(fadedowncue.fadedown*10000));
			if (outg > 1)
				outg = 1;
			if (outg < 0)
				outg = 0;

			for (int i = 0; i < 512; i++)
			{
				vals[i] = (int)(((fadedownvals[i] * outg) + (fadeupcue.channels[i] * inc)) / (inc + outg));
				//vals[i] = (inc * (incoming.channels[i] - outgoing.channels[i])) + (outg * (incoming.channels[i] - outgoing.channels[i]));
			}
			dmxcontroller.UpdateValues(vals);
			//update screen
			if (fadeupcue.fadeup > fadedowncue.fadedown)
				playback.CueProgress.Value = (diff/(fadeupcue.fadeup)) * 100;
			else
				playback.CueProgress.Value = (diff / (fadedowncue.fadedown*10000)) * 100;
		}

		private string devicename = "";        

		private delegate void StringMethodInvoker(string s, string t);

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

        ServerFinder finder = new ServerFinder();

		/// <summary>
		/// Default constructor.
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();

			// Add handlers for Application activation events
			//AddActivationHandlers();
			 //surfaceListBox1.ItemsSource = cueslist;
			 cuedisplay.OnRefresh += new CueDisplay.RefreshMethod(cuedisplay_OnRefresh);
			 
			 playback.window = this;
			 cuedisplay.window = this;
			 fader.Interval = TimeSpan.FromTicks(100);//10 times a second
			 fader.Tick += new EventHandler(fader_Tick);

			 Load();

             List<string> knowndevices = new List<string>();
            
             //ListCollectionView v = new ListCollectionView(finder.Servers);
             finder.OnServerFound += new Action<string,string>(finder_OnServerFound);

			 //List<string> knowndevices = new List<string>();

			 gone.Interval = TimeSpan.FromSeconds(3);
			 gone.Tick += new EventHandler(gone_Tick);
			 playback.outgoing.Content = "--";
			 playback.incoming.Content = "--";

			 cuedisplay.PropertiesScatter.Visibility = Visibility.Hidden;
			 playbackscatter.Visibility = Visibility.Hidden;
			 c2 = new Cue();
			 c2.name = "---";
			 cuedisplay.SetCue(c2);
			 DispatcherTimer savetimer = new DispatcherTimer();
			 savetimer.Interval = TimeSpan.FromMinutes(3);
			 savetimer.Tick += new EventHandler((o, e) =>
			 {
				 Save();
			 });
			 savetimer.Start();

             Timer t = new Timer(new TimerCallback((o) => {
                 if (isLoaded)
                 {
                     feedback_OnUpdate(lastupdatevalues);
                 }
             }), null, 0, 300);
		}

		private bool connected;

		void gone_Tick(object sender, EventArgs e)
		{
			if (!vis)
			{
				DoubleAnimation ani = new DoubleAnimation(cuelistscatter.Opacity, 0.5, new Duration(TimeSpan.FromMilliseconds(200)));
				ani.FillBehavior = FillBehavior.HoldEnd;
				cuelistscatter.BeginAnimation(ScatterViewItem.OpacityProperty, ani);
			}
		}

		Cue c;
		Cue c1;
		public Cue c2;//the live cue

		Cue ActualSelectedCue = null;
		int AcutalSelectedIndex = -1;
		void cuedisplay_OnRefresh()
		{
			//surfaceListBox1.ItemsSource = null;
			//surfaceListBox1.ItemsSource = cueslist;
			//surfaceListBox1.SelectedItem = SelectedCue;

			surfaceListBox1.Items.Clear();
			ActualSelectedCue = null;
			AcutalSelectedIndex = -1;

			int i = 0;
			foreach (Cue c in cueslist)
			{
				//ListBoxItem li = new ListBoxItem();

				CueListItem cl = new CueListItem(c,i+1);
				//li.Content = cl;

				if ((object)c == (object)SelectedCue)
				{
					cl.Select(true);
					ActualSelectedCue = SelectedCue;
					AcutalSelectedIndex = i;
					//surfaceListBox1.SelectedItem = cl;
				}
				if (i == CurrentCueOnFireNum)
				{
					cl.Playing(true);
				}
				i++;
				surfaceListBox1.Items.Add(cl);
			}
		}


		
		#region Buttons

		//add
		private void surfaceButton2_Click(object sender, RoutedEventArgs e)
		{
			cueslist.Add(new Cue());
			cuedisplay_OnRefresh();
		}


		//remove
		private void surfaceButton3_Click(object sender, RoutedEventArgs e)
		{
			if (ActualSelectedCue != null)
			{
				cueslist.Remove(ActualSelectedCue);
				cuedisplay_OnRefresh();
			}
		}

		//copy?
		private void surfaceButton4_Click(object sender, RoutedEventArgs e)
		{
			if (ActualSelectedCue != null)
			{
				cueslist.Insert(AcutalSelectedIndex+1,new Cue(ActualSelectedCue));
				cuedisplay_OnRefresh();
			}
		}

		//up
		private void surfaceButton5_Click(object sender, RoutedEventArgs e)
		{
			if (ActualSelectedCue != null && (AcutalSelectedIndex-1>=0))
			{
				Cue temp = ActualSelectedCue;
				cueslist.Remove(temp);
				cueslist.Insert(AcutalSelectedIndex-1, temp);
				cuedisplay_OnRefresh();
			}
		}

		//down
		private void surfaceButton6_Click(object sender, RoutedEventArgs e)
		{
			if (ActualSelectedCue != null && AcutalSelectedIndex < cueslist.Count-1)
			{
				Cue temp = ActualSelectedCue;
				cueslist.Remove(temp);
				cueslist.Insert(AcutalSelectedIndex+1, temp);
				cuedisplay_OnRefresh();
			}
		}
#endregion

		Cue SelectedCue;

		public bool LockLiveOutput
		{
			get;
			set;
		}

		private void surfaceListBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (surfaceListBox1.SelectedIndex != -1)
			{
				Cue c = ((CueListItem)surfaceListBox1.SelectedItem).Cue;
				SelectedCue = c;
				cuedisplay.SetCue(c);
				if (!LockLiveOutput)
				{
					dmxcontroller.UpdateValues(c.channels);
				}
				dirtyflag.Visibility = Visibility.Hidden;
				cuedisplay_OnRefresh();
			}
		}

		private Rig temprig;
		private Uri targetip = null;

		private bool isLoaded = false;
		private string connectionip = "";

		public IController dmxcontroller;

        IFeedback feedback;

		#region Initial Login
		private void surfaceButton10_Click(object sender, RoutedEventArgs e)
		{
            finder.Stop();
            if (connectionip == "")
            {
                UserNotifications.RequestNotification("Error", "Please select a server");
                return;
            }
			isLoaded = true;
			if (address.Text != "")
				connectionip = address.Text;

            //IPHostEntry ip = Dns.GetHostEntry(connectionip);
            //IPHostEntry ipHostEntry = Dns.GetHostEntry(connectionip);
            //IPAddress[] ipv4Addresses = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList,a => a.AddressFamily == AddressFamily.InterNetwork);
            Uri uri = new Uri("http://" + connectionip);
            connectionip = uri.Host;
            //progress.Visibility = Visibility.Visible;
			if (connectionip!="")
			{
		   
				try
				{
					devicename = myname.Text;
					//targetip = new Uri("http://" + connectionip + ":1235/venue.thor");
					
                    //sets up transmitter
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
							cuedisplay.setRig(temprig, image);
						}
						progress.Visibility = Visibility.Hidden;
                        faders.Start(dmxcontroller, temprig);
						//setup receive loop
					});

					worker.DoWork += new DoWorkEventHandler((o,a) =>
					{
						try
						{
                            temprig = Rig.LoadRigFromServer(connectionip);
						}
						catch (Exception ex)
						{
							//fail...
							//Microsoft.Surface.UserNotifications.RequestNotification("Connection Error", ex.Message);
                            UserNotifications.RequestNotification("Connection Error", ex.Message);
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
		#endregion


		

		private void surfaceButton1_Click(object sender, RoutedEventArgs e)
		{
			if (playback.Visibility == Visibility.Hidden)
			{
				playback.Visibility = Visibility.Visible;
				cuedisplay.Visibility = Visibility.Hidden;
			}
			else
			{
				playback.Visibility = Visibility.Hidden;
				cuedisplay.Visibility = Visibility.Visible;
			}
		}

		

		private void SurfaceButton_Click(object sender, RoutedEventArgs e)
		{
			if (cuedisplay.PropertiesScatter.Visibility == Visibility.Hidden)
			{
				cuedisplay.PropertiesScatter.Visibility = Visibility.Visible;
			}
			else
			{
				cuedisplay.PropertiesScatter.Visibility = Visibility.Hidden;
			}
			cuedisplay.PropertiesScatter.Center = new System.Windows.Point(600, 400);
			cuedisplay.PropertiesScatter.Orientation = 0;
		}

		private bool vis;
		private void SurfaceButton_Click_1(object sender, RoutedEventArgs e)
		{
			if (vis)
			{
				cuelistscatter.BeginAnimation(ScatterViewItem.OpacityProperty, null);
				cuelistscatter.Opacity = 0.5;
			}
			else
			{
				cuelistscatter.BeginAnimation(ScatterViewItem.OpacityProperty, null);
				cuelistscatter.Opacity = 1;
			}
			vis = !vis;
		}

		private void SurfaceButton_Click_2(object sender, RoutedEventArgs e)
		{
			if (playbackscatter.Visibility == Visibility.Hidden)
			{
				playbackscatter.Visibility = Visibility.Visible;
			}
			else
			{
				playbackscatter.Visibility = Visibility.Hidden;
			}
			playbackscatter.Orientation = 0;
			playbackscatter.Center = new System.Windows.Point(200, 400);
		}

		private double angle = 0;

		private void SurfaceButton_Click_3(object sender, RoutedEventArgs e)
		{

			angle += 90;
			cuedisplay.canvas.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
			cuedisplay.canvas.RenderTransform = new RotateTransform(angle);
		}

		DispatcherTimer gone = new DispatcherTimer();
		private void cuelistscatter_PreviewContactDown(object sender, TouchEventArgs e)
		{
			DoubleAnimation ani = new DoubleAnimation(cuelistscatter.Opacity, 1, new Duration(TimeSpan.FromMilliseconds(200)));
			ani.FillBehavior = FillBehavior.HoldEnd;
			cuelistscatter.BeginAnimation(ScatterViewItem.OpacityProperty, ani);
			gone.Stop();
		}

		private void cuelistscatter_PreviewContactUp(object sender, TouchEventArgs e)
		{

            //if (Contacts.GetContactsOver(cuelistscatter).Count > 0)
			{
				if (gone.IsEnabled)
					gone.Stop();
				gone.Start();
			}
		}

		private void SurfaceButton_Click_4(object sender, RoutedEventArgs e)
		{
			cuedisplay.ResetView();
		}

		private void SurfaceWindow_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Middle)
			{
				playback.GO();
			}
		}

		private void SurfaceWindow_Closed(object sender, EventArgs e)
		{
			Environment.Exit(0);
		}

		private void servers_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			connectionip = (string)((SurfaceListBoxItem)servers.SelectedItem).Tag;
		}

        delegate void IntUpdate(LightControl l, int[] i);
        DateTime lastupdate = DateTime.Now;
        byte[] lastupdatevalues = new byte[512];
        void feedback_OnUpdate(byte[] vals)
        {
            lastupdatevalues = vals;
            //if (loadedrig)
            {
                if (lastupdate + TimeSpan.FromMilliseconds(200) < DateTime.Now)
                {
                    this.Dispatcher.BeginInvoke(new Action<byte[]>((o) =>
                    {
                        faders.Update(o);
                    }), lastupdatevalues);


                    foreach (LightControl c in cuedisplay.Lights)
                    {
                        // if (c.Channel == i)
                        //{
                        int[] cop = new int[c.TheLight.UsedChannels];
                        Array.Copy(lastupdatevalues, c.Channel, cop, 0, c.TheLight.UsedChannels);


                        this.Dispatcher.BeginInvoke(new IntUpdate((q, o) =>
                        {
                            q.UpdateLiveChannel(cop);
                        }), new object[] { c, cop });

                        // }
                    }
                    //for (int i = 1; i < vals.Count(); i++)
                    //{

                    //}
                    lastupdate = DateTime.Now;
                }
            }

            //for (int i = 1; i < vals.Count(); i++)
            //{
            //    foreach (LightControl c in cuedisplay.Lights)
            //    {
            //        int[] cop = new int[c.TheLight.UsedChannels];
            //        Array.Copy(vals, c.Channel, cop, 0, c.TheLight.UsedChannels);

            //        //if (c.Channel == i)
            //        {
            //            this.Dispatcher.BeginInvoke(new IntUpdate((q, o) =>
            //            {
            //                //q.UpdateLiveChannel(o);
            //            }), new object[] { c, cop });
            //        }
            //    }
            //}
        }

        private void surfaceButton22_Copy_Click(object sender, RoutedEventArgs e)
        {
            faders.Visibility = Visibility.Visible;
        }

        private void TabItem_ContactDown(object sender, TouchEventArgs e)
        {

            tabs.SelectedItem = (sender as TabItem);
        }
    }
}