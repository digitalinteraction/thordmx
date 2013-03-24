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
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using System.Diagnostics;
using System.Windows.Threading;
using System.Threading;
using IPS.SharedObjects;

namespace IPS.TabletDesk
{
    /// <summary>
    /// Interaction logic for CueDisplay.xaml
    /// </summary>
    public partial class CueDisplay : UserControl
    {
        public delegate void RefreshMethod();

        public event RefreshMethod OnRefresh;

        public MainWindow window;


        public CueDisplay()
        {
            InitializeComponent();
            canvas.Children.Add(box);
            
            Canvas.SetZIndex(box, -1);
            box.Opacity = 0;
            box.KeyDown += new KeyEventHandler(box_KeyDown);
        }

        LightControl numsel = null;
        void box_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                
                //select and find the light...
                if (box.Text != "")
                {
                    int chan = Int32.Parse(box.Text);
                    box.Text = "";
         
                    foreach (LightControl l in lights)
                    {
                        if (l.Channel == chan)
                        {
                            //move the screen and highlight it...
                            ResetView();
                            l.Selected = true;
                            numsel = l;
                            DispatcherTimer timer = new DispatcherTimer();
                            timer.Interval = TimeSpan.FromMilliseconds(500);
                            timer.Tick += new EventHandler((o, ex) =>
                            {
                                timer.Stop();
                                numsel.Selected = false;
                            });
                            timer.Start();
                        }
                    }
                }
            }
        }

        public void ResetView()
        {
            Point p = new Point();
            p.X = 1024 / 2;// -Canvas.GetLeft(l);
            p.Y = 768 / 2;// -Canvas.GetTop(l);
            sv1.Width = therigplan.Width;
            sv1.Height = therigplan.Height;

            sv1.Center = p;
            sv1.UpdateLayout();
            //sv1_ScatterManipulationDelta(null, null);
        }

        Cue cue = new Cue();
        public void SetCue(Cue c)
        {
            this.cue = c;
            if (c != window.c2)
            {
                //load this cue values
                foreach (LightControl l in lights)
                {
                    l.UpdateChannelNoLoop(new int[] { c.channels[l.Channel] });
                }
                surfaceTextBox1.Text = c.name;

                label1.Content = "Fade In Time: " + fadein.Value + " sec";
                label2.Content = "Fade Out Time: " + fadeout.Value + " sec";
                fadein.Value = c.fadeup;
                fadeout.Value = c.fadedown;
                fadein.IsEnabled = true;
                fadeout.IsEnabled = true;
                surfaceTextBox1.IsEnabled = true;
            }
            else
            {
                surfaceTextBox1.Text = c.name;
                label1.Content = "---";
                label2.Content = "---";
                fadein.IsEnabled = false;
                fadeout.IsEnabled = false;
                surfaceTextBox1.IsEnabled = false;
            }
        }

        public ScatterViewItem PropertiesScatter
        {
            get { return cuepropertiesscatter; }
        }

        public Canvas VenueImage
        {
            get { return canvas; }
        }

        Rig therig = null;
        List<LightControl> lights = new List<LightControl>();

        public List<LightControl> Lights
        {
            get { return lights; }
        }

        BitmapImage therigplan;
        public void setRig(Rig r,BitmapImage plan)
        {
            therig = r;
                //setup the rig here...
               // canvas.Background = new ImageBrush(plan);
                //canvas.Width = plan.Width;
                //canvas.Height = plan.Height;
            //image.BeginInit();

            //ImageBrush brush = new ImageBrush(plan);
            //brush.Stretch = Stretch.Fill;
            //image.Background = brush;
            therigplan = plan;
            canvas.Background = new ImageBrush(plan);
            sv1.Width = plan.Width;
            sv1.Height = plan.Height;

            //    image.EndInit();

                //image.Width = plan.Width;
                //image.Height = plan.Height;
                
                foreach (Light l in r.Lights)
                {
                    LightControl lc = new LightControl(l,window);
                    canvas.Children.Add(lc);
                    Canvas.SetLeft(lc, (l.Position.X * sv1.Width) + 15.0f);
                    Canvas.SetTop(lc, (l.Position.Y * sv1.Height) + 15.0f);
                    lights.Add(lc);
                    lc.OnLightUpdate += new LightControl.LightUpdated(lc_OnLightUpdate);
                    lc.OnLightPreviewUpdate += new LightControl.LightPreviewUpdate(lc_OnLightPreviewUpdate);
                }
        }

        void lc_OnLightPreviewUpdate(LightControl me, int channel, int[] val)
        {
            
            //channel is the starting channel:
            for (int i = channel; i < channel+val.Count()-1; i++)
            {
                foreach (LightControl l in lights)
                {
                    if (l.Channel == i && l.ChannelValue.Count()==1)
                    {
                        l.ChannelValue[0] = val[i - channel];
                        l.UpdateChannelNoLoop(new int[]{val[i - channel]});
                    }
                }
            }

            foreach (LightControl l in lights)
            {
                if (l.Channel == channel && l != me && l.ChannelValue.Count() == val.Count())
                {
                    for (int i = 0; i < val.Count(); i++)
                    {
                        l.ChannelValue[i] = val[i];
                    }
                    l.UpdateChannelNoLoop(val);
                }
            }
        }

        void lc_OnLightUpdate(int channel, int value)
        {
            if (!window.LockLiveOutput)
                window.dmxcontroller.UpdateValue(channel, value);
        }

        private void surfaceTextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cue != null)
            {
                this.cue.name = surfaceTextBox1.Text;
                if (OnRefresh!=null)
                OnRefresh();
            }           
        }

        private void surfaceButton1_Click(object sender, RoutedEventArgs e)
        {
            window.dirtyflag.Visibility = Visibility.Hidden;
            //save the cue...
            //foreach (LightControl l in lights)
            //{
            //    this.cue.channels[l.Channel] = l.ChannelValue;
            //}
            foreach (LightControl l in lights)
            {
                for (int i = l.Channel; i < l.TheLight.UsedChannels + l.Channel; i++)
                {
                    this.cue.channels[i] = l.ChannelValue[i - l.Channel];
                }
            }
        }

        private void fadeout_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (label2 != null)
            {
                cue.fadedown = (float)Math.Round(fadeout.Value,2);
                label2.Content = "Fade Out Time: " + Math.Round(cue.fadedown,1) + " secs";
            }
        }

        private void fadein_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (label2 != null)
            {
                cue.fadeup = (float)Math.Round(fadein.Value, 2);
                label1.Content = "Fade In Time: " + Math.Round(cue.fadeup,1) + " secs";
            }
        }

        internal void UpdateValues(int[] vals)
        {
            if (window.LockLiveOutput)
            {
                //when in live mode, feedback on what lights are on...
                //load this cue values
                foreach (LightControl l in lights)
                {
                    //l.UpdateChannel(vals[l.Channel]);
                }
            }
        }

        private void surfaceScrollViewer1_ContactDown(object sender, TouchEventArgs e)
        {
            
        }

        private void sv1_ScatterManipulationDelta(object sender,  object e)
        {

            //TO FIX
            //rearrange the lights...
            foreach (LightControl lc in lights)
            {
                Canvas.SetLeft(lc, (lc.TheLight.Position.X * sv1.ActualWidth) + (sv1.ActualWidth/1024)*15.0f);
                Canvas.SetTop(lc, (lc.TheLight.Position.Y * sv1.ActualHeight) + (sv1.ActualHeight/768)*15.0f);
                //Debug.Print(""+sv1.ActualHeight);
            }
        }

        public bool SelectPressed
        {
            get;
            set;
        }

        private void SurfaceButton_ContactDown(object sender, TouchEventArgs e)
        {
            SelectPressed = true;
        }

        private void SurfaceButton_ContactUp(object sender, TouchEventArgs e)
        {
            SelectPressed = false;
            //remove all the selection...
            fader.Visibility = Visibility.Hidden;
            foreach (LightControl l in SelectedLights)
            {
                l.Selected = false;
            }
        }

        public void ShowFader()
        {
            //slider1.Value = 0;
            fader.Visibility = Visibility.Visible;
            
        }

        public List<LightControl> SelectedLights
        {
            get {
                List<LightControl> temp = new List<LightControl>();
                foreach (LightControl l in lights)
                {
                    if (l.Selected)
                        temp.Add(l);
                }
                return temp;
            }
        }

        private void SurfaceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //Debug.Print(""+e.NewValue);
            perc.Content = Math.Round(((e.NewValue / 255)*100),0) + "%";
            foreach (LightControl l in window.cuedisplay.SelectedLights)
            {
                //l.UpdateChannel((int)e.NewValue);
                //if (!window.LockLiveOutput)
                //{
                //    window.dmxcontroller.UpdateValue(l.TheLight.Channel, (int)e.NewValue);
                //}
                l.UpdateChannel(new int[] { (int)e.NewValue });
                if (!window.LockLiveOutput)
                {
                    window.dmxcontroller.UpdateValue(l.TheLight.Channel, (int)e.NewValue);
                }
            }
        }

        TextBox box = new TextBox();
        private void SurfaceButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void SurfaceButton_ContactDown_1(object sender, TouchEventArgs e)
        {
            //Microsoft.Surface.Core.SurfaceKeyboard.Layout = Microsoft.Surface.Core.KeyboardLayout.Numeric;
            //Microsoft.Surface.Core.SurfaceKeyboard.IsVisible = true;
            box.Focus();
        }

        private void SurfaceButton_ContactUp_1(object sender, TouchEventArgs e)
        {
            //Microsoft.Surface.Core.SurfaceKeyboard.IsVisible = false;
        }

        public bool FlashPressed { get; set; }

        private void SurfaceButton_PreviewContactDown(object sender, TouchEventArgs e)
        {
            FlashPressed = true;
        }

        private void SurfaceButton_PreviewContactUp(object sender, TouchEventArgs e)
        {
            FlashPressed = false;
        }
    }
}
