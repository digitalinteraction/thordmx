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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using JsonExSerializer;
using Microsoft.Win32;
using System.Drawing;
using IPS.VenueBuilder;
using System.Drawing.Imaging;
using System.Net.Cache;
using System.Net;
using System.Collections.Specialized;
using Alchemy.Classes;
using System.ComponentModel;
using MahApps.Metro.Controls;
using IPS.SharedObjects;
using IPS.Controller;


namespace IPS.VenueBuilder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            therig.Lights = new List<Light>();
            LampTypes = new Dictionary<int, string>();
            //add lamp types here
            LampTypes.Add(0, "Generic");
            LampTypes.Add(1, "PAR 64");
            LampTypes.Add(2, "Fresnel");
            LampTypes.Add(3, "Flood");
            LampTypes.Add(4, "Source4 Zoom");
            LampTypes.Add(5, "Source4 2550");
            LampTypes.Add(6, "RGB Mixer");

            foreach (KeyValuePair<int, string> v in LampTypes)
            {
                lamptypes.Items.Add(v);
            }
            colorpicker.SelectedColorChanged += new RoutedPropertyChangedEventHandler<System.Windows.Media.Color>(colorpicker_SelectedColorChanged);
            channel.ValueChanged += new RoutedPropertyChangedEventHandler<object>(channel_ValueChanged);
            channel.Minimum = 1;
            channel.Maximum = 512;

            for (int i=1;i<=512;i++)
            {
                Label l = new Label();
                l.Padding = new Thickness(3);
                l.Content = i;
                l.FontSize = 9;
                l.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                dmxs.Add(i, l);
                dmx.Children.Add(l);
            }
            ServerFinder finder = new ServerFinder();
            finder.OnServerFound += new Action<string>(finder_OnServerFound);
        }

        Alchemy.WebSocketClient ws;

        void finder_OnServerFound(string obj)
        {
            ws = new Alchemy.WebSocketClient("ws://" + obj + ":8282/events");
            Dispatcher.BeginInvoke(new Action(() => {
                servern.Content = obj;
                upload.IsEnabled = true;
                serverload.IsEnabled = true;
                //load server venues
                
                ws.OnReceive = OnReceive;
                ws.Connect();
                ws.Send("?venues");
                
            }));
        }
        void OnReceive(UserContext context)
        {
            string data = context.DataFrame.ToString();
            Serializer jx = new Serializer(typeof(Dictionary<string, string>));
            Dictionary<string, string> t = jx.Deserialize(data) as Dictionary<string,string>;

            this.Dispatcher.BeginInvoke(new Action<Dictionary<string, string>>((s) =>
            {

                foreach (KeyValuePair<string, string> y in s)
                {
                    ComboBoxItem cb = new ComboBoxItem();
                    cb.Content = y.Value;
                    cb.Tag = y.Key;
                    servervenues.Items.Add(cb);
                }
            }),new object[] {t});
            ws.Disconnect();
        }

        Dictionary<int, Label> dmxs = new Dictionary<int, Label>();

        void Refresh()
        {
            foreach (Label l in dmxs.Values)
            {
                l.Background = System.Windows.Media.Brushes.Transparent;
                l.BorderBrush = System.Windows.Media.Brushes.Transparent;
                l.BorderThickness = new Thickness(1);
            }
            foreach (Light l in lamps.Values)
            {
                dmxs[l.Channel].Background = System.Windows.Media.Brushes.YellowGreen;
                for (int i = l.Channel; i < l.Channel + l.UsedChannels; i++)
                {
                    dmxs[i].BorderBrush = System.Windows.Media.Brushes.YellowGreen;
                }
            }


        }

        void channel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (currentlamp != null)
            {
                int old = currentlamp.Channel;
                //dmxs[old].Background = System.Windows.Media.Brushes.Transparent;

                currentlamp.Channel = (int)e.NewValue;
                foreach (KeyValuePair<Lamp, Light> v in lamps)
                {
                    if (v.Value == currentlamp)
                    {
                        v.Key.label1.Content = "" + currentlamp.Channel;
                    }
                    //if (v.Value.Channel == old)
                    //{
                    //    v.Value.Channel = (int)(double)e.NewValue;
                    //    v.Key.label1.Content = "" + currentlamp.Channel;
                    //}
                }
                //dmxs[currentlamp.Channel].Background = System.Windows.Media.Brushes.YellowGreen;
                saved = false;
            }
            Refresh();
        }

        void colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color> e)
        {
            if (currentlamp != null)
            {
                currentlamp.Color = string.Format("#{0:X2}{1:X2}{2:X2}", e.NewValue.R, e.NewValue.G, e.NewValue.B);
                foreach (KeyValuePair<Lamp, Light> v in lamps)
                {
                    if (v.Value == currentlamp)
                    {
                        v.Key.ellipse.Fill = new SolidColorBrush(e.NewValue);
                    }
                }
                saved = false;
            }
        }

        private Rig therig = new Rig();
        System.Drawing.Image image;

        public Dictionary<int, string> LampTypes
        {
            get;
            set;
        }

        private void Save(string filename)
        {
            try
            {
                if (image != null)
                {
                    image.Save("venue.jpg",ImageFormat.Jpeg);
                    //set all light positions...
                    foreach (KeyValuePair<Lamp,Light> ll in lamps)
                    {
                        ll.Value.Position = new PointF((float)(Canvas.GetLeft(ll.Key) + (ll.Key.Width / 2)) / image.Width, (float)(Canvas.GetTop(ll.Key) + (ll.Key.Height / 2)) / image.Height);
                    }

                    if (therig.Name == null)
                        therig.Name = "Un-Named";

                    //data
                    Serializer serializer = new Serializer(typeof(Rig));
                    string jsonText = serializer.Serialize(therig);
                    TextWriter tw = new StreamWriter(Directory.GetCurrentDirectory()+"\\rig.json");
                    tw.Write(jsonText);
                    tw.Flush();
                    tw.Close();

                    using (ZipOutputStream s = new ZipOutputStream(File.Create(filename)))
                    {
                        s.SetLevel(8); // 0 - store only to 9 - means best compression
                        byte[] buffer = new byte[4096];
                        // Using GetFileName makes the result compatible with XP
                        // as the resulting path is not absolute.
                        ZipEntry dat = new ZipEntry("rig.json");
                        ZipEntry img = new ZipEntry("venue.jpg");

                        // Could also use the last write time or similar for the file.
                        dat.DateTime = DateTime.Now;
                        s.PutNextEntry(dat);

                        using (FileStream fs = File.OpenRead(Directory.GetCurrentDirectory()+"\\rig.json"))
                        {
                            // Using a fixed size buffer here makes no noticeable difference for output
                            // but keeps a lid on memory usage.
                            int sourceBytes;
                            do
                            {
                                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                s.Write(buffer, 0, sourceBytes);
                            } 
                            while (sourceBytes > 0);
                        }

                        img.DateTime = DateTime.Now;
                        s.PutNextEntry(img);

                        using (FileStream fs = File.OpenRead(Directory.GetCurrentDirectory() + "\\venue.jpg"))
                        {

                            // Using a fixed size buffer here makes no noticeable difference for output
                            // but keeps a lid on memory usage.
                            int sourceBytes;
                            do
                            {
                                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                s.Write(buffer, 0, sourceBytes);
                            } while (sourceBytes > 0);
                        }
                       
                        s.Finish();
                        s.Close();
                    }
                    saved = true;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error saving file - "+e.Message);
            }
        }

        bool saved = false;

        private void LoadFile(string filename)
        {
            progress.Visibility = System.Windows.Visibility.Visible;
            try
            {
                ZipFile zipfile = new ZipFile(filename);

                //image
                ZipEntry img = zipfile.GetEntry("venue.jpg");
                Stream s = zipfile.GetInputStream(img);

                image = new Bitmap(s);

                s.Close();
                s = zipfile.GetInputStream(img);
                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = s;
                bmp.EndInit();
                background.Source = bmp;

                s.Close();
                ZipEntry dat = zipfile.GetEntry("rig.json");
                Stream t = zipfile.GetInputStream(dat);
                TextReader reader = new StreamReader(t);
                string json = reader.ReadToEnd();
                reader.Close();


                //TODO - this is not working!!!
                Serializer serializer = new Serializer(typeof(Rig));
                therig = (Rig)serializer.Deserialize(json);
                venuename.Text = therig.Name;
                BuildLights();
                addlampbtn.IsEnabled = true;
                zipfile.Close();
            }
            catch
            {
                MessageBox.Show("Error loading file!");
            }
            progress.Visibility = System.Windows.Visibility.Hidden;
        }

        Dictionary<Lamp, Light> lamps = new Dictionary<Lamp, Light>();

        private void BuildLights()
        {
            lamps.Clear();
            dragcanvas.Children.Clear();
            foreach (Light l in therig.Lights)
            {
                Lamp lm = new Lamp(l, this);
                lm.Opacity = 0.6;
                dragcanvas.Children.Add(lm);
                lamps.Add(lm, l);
                Canvas.SetLeft(lm,(int)(l.Position.X * image.Width) - lm.Width/2);
                Canvas.SetTop(lm, (int)(l.Position.Y * image.Height) - lm.Height/2);
                dmxs[l.Channel].Background = System.Windows.Media.Brushes.YellowGreen;
            }
            
        }

        private void AddLight()
        {
            
            Light light = new Light();
            light.UsedChannels = 1;
            Lamp lm = new Lamp(light,this);
            lm.Opacity = 0.6;
            dragcanvas.Children.Add(lm);
            //lm.Left = -panel1.AutoScrollPosition.X;
            //lm.Top = -panel1.AutoScrollPosition.Y;
            lamps.Add(lm, light);
            therig.Lights.Add(light);
            SetLamp(light);
        }

        internal void DeleteLight(Lamp lamp)
        {
            dragcanvas.Children.Remove(lamp);
            therig.Lights.Remove(lamps[lamp]);
            lamps.Remove(lamp);

        }

        private Light currentlamp;

        public void SetLamp(Light l)
        {
            if (currentlamp!=null)
            {
                foreach (KeyValuePair<Lamp,Light> ll in lamps)
                {
                    if (ll.Value == currentlamp)
                        ll.Key.Opacity = 0.6;
                    
                }
            }

            foreach (KeyValuePair<Lamp, Light> ll in lamps)
            {
                if (ll.Value == l)
                    ll.Key.Opacity = 1;

            }

            currentlamp = l;
            lampinfo.IsEnabled = true;
            channel.Text = ""+l.Channel;
            used.Value = 1;
            checkBox1.IsChecked = l.Hidden;
            if (l.UsedChannels == 0)
                l.UsedChannels = 1;
            used.Value = l.UsedChannels;
            foreach (KeyValuePair<int, string> v in LampTypes)
            {
                if (v.Key == l.LampType)
                {
                    lamptypes.SelectedItem = v;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "Jpeg|*.jpg"; // Filter files by extension
            if (dlg.ShowDialog() == true && dlg.FileName != "")
            {
                BitmapImage im = new BitmapImage(new Uri(dlg.FileName));
                im.CacheOption = BitmapCacheOption.OnLoad;
                
                background.Source = im;

                image = new Bitmap(dlg.FileName);
                //pictureBox1.Image = image;
            }
            saved = false;
            addlampbtn.IsEnabled = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            AddLight();
            saved = false;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.DefaultExt = ".thor";
            dlg.Filter = "Thor DMX Venue|*.thor"; // Filter files by extension
            if (dlg.ShowDialog() == true && dlg.FileName != "")
             {
                 LoadFile(dlg.FileName);
             }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.AddExtension = true;
            dlg.DefaultExt = ".thor";
            dlg.Filter = "Thor DMX Venue|*.thor"; // Filter files by extension
            if (dlg.ShowDialog() == true && dlg.FileName != "")
            {
                Save(dlg.FileName);
            }
        }

        private void lamptypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (currentlamp != null)
            {
                currentlamp.LampType = ((KeyValuePair<int, string>)lamptypes.SelectedItem).Key;
                currentlamp.LampTypeName = ((KeyValuePair<int, string>)lamptypes.SelectedItem).Value;
                saved = false;
            }
        }

        private void venuename_TextChanged(object sender, TextChangedEventArgs e)
        {
            therig.Name = venuename.Text;
            saved = false;
        }

        private void channel_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (currentlamp != null)
            {
                Lamp temp = null;
                foreach (KeyValuePair<Lamp, Light> v in lamps)
                {
                    if (v.Value == currentlamp)
                    {
                        temp = v.Key;
                        lampinfo.IsEnabled = false;
                    }
                }
                DeleteLight(temp);
                saved = false;
            }
        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            currentlamp.Hidden = (bool)checkBox1.IsChecked;
            saved = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!saved)
            {
                if (MessageBox.Show("This rig is not saved, really close?", "Warning", MessageBoxButton.YesNo)==MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        private void used_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (currentlamp != null)
            {
                
               
                currentlamp.UsedChannels = (int)e.NewValue;

                //dmxs[currentlamp.Channel].Background = System.Windows.Media.Brushes.YellowGreen;
                saved = false;
            }
            Refresh();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            progress.Visibility = System.Windows.Visibility.Visible;
            Save(Directory.GetCurrentDirectory() + @"\temp.thor");
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.RunWorkerAsync(servern.Content);
            //upload to server...
            

            //<form action="/?venue" method="POST" enctype="multipart/form-data">
			//<input type="file" name="myfile" />
			//<input type="submit" value="Upload" />
			//</form> 
            
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progress.Visibility = System.Windows.Visibility.Hidden;
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            

            CookieContainer cookies = new CookieContainer();
            //add or use cookies
            NameValueCollection querystring = new NameValueCollection();
            querystring.Add("venue", "");
            string uploadfile;// set to file to upload
            uploadfile = Directory.GetCurrentDirectory() + @"\temp.thor";

            //everything except upload file and url can be left blank if needed
            string outdata = UploadFileEx.Class1.UploadFileEx(uploadfile, "http://" + e.Argument + "/?venue", "myfile", "application/zip", null, cookies);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            //load from server...



            var y = (servervenues.SelectedItem as ComboBoxItem).Tag as string;


            progress.Visibility = System.Windows.Visibility.Visible;
            //get venue from server
            WebClient w = new WebClient();
            w.DownloadFileCompleted += new AsyncCompletedEventHandler(w_DownloadFileCompleted);
            w.DownloadFileAsync(new Uri("http://" + servern.Content + ":1235/" + y),Directory.GetCurrentDirectory() + @"\temp.thor",null);
            //load rig...
            

        }

        void w_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            LoadFile(Directory.GetCurrentDirectory() + @"\temp.thor");
            progress.Visibility = System.Windows.Visibility.Hidden;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //chage scale
            //scaler.CenterX = background.ActualWidth / 2;
            //scaler.CenterY = background.ActualHeight / 2;
            //scroll to the center ??? 
            scroller.ScrollToHorizontalOffset(scroller.ScrollableWidth / 2);
            scroller.ScrollToVerticalOffset(scroller.ScrollableHeight / 2);
            scaler.ScaleX = e.NewValue / 10;
            scaler.ScaleY = e.NewValue /10;
        }

        
        //private void colorPanel1_ColorChanged(object sender, PJLControls.ColorChangedEventArgs e)
        //{
        //    if (currentlamp != null)
        //    {
        //        currentlamp.Color = System.Drawing.ColorTranslator.ToHtml(e.Color);
        //        foreach (KeyValuePair<Lamp, Light> v in lamps)
        //        {
        //            if (v.Value == currentlamp)
        //            {
        //                v.Key.BackColor = e.Color;
        //            }
        //        }
        //    }
        //}

    }
}
