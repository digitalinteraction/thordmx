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
using IPS.SharedObjects;
using DRAWING = System.Drawing;
using System.IO;
using IPS.SurfaceDesk;
using System.Windows.Input;

namespace IPS.TabletDesk
{
    /// <summary>
    /// Interaction logic for LightControl.xaml
    /// </summary>
    public partial class LightControl : UserControl
    {
        //private Manipulation manipulationProcessor;

        public LightControl()
        {
            InitializeComponent();
            IsManipulationEnabled = true;
        }

        private bool selected;
        public bool Selected
        {
            get { return selected; }
            set { 
                selected = value;
                if (value)
                    selel.Visibility = Visibility.Visible;
                else
                    selel.Visibility = Visibility.Hidden;
            }
        }

        public delegate void LightUpdated(int channel, int value);
        public event LightUpdated OnLightUpdate;

        DRAWING.Bitmap Wheel = new DRAWING.Bitmap(Directory.GetCurrentDirectory() + @"\ColorWheel.png");

        public int Channel{get;set;}
        MainWindow window;
        Light thelight;
        public Light TheLight
        {
            get { return thelight; }
        }
        public LightControl(Light light,MainWindow window)
        {
            InitializeComponent();
            IsManipulationEnabled = true;
            ManipulationDelta += new EventHandler<System.Windows.Input.ManipulationDeltaEventArgs>(LightControl_ManipulationDelta);
            thelight = light;
            this.window = window;
            Channel = light.Channel;
            chan.Content = light.Channel;
           // manipulationProcessor = new Affine2DManipulationProcessor(Affine2DManipulations.TranslateY, this);
            
            //manipulationProcessor.Affine2DManipulationDelta += OnManipulationDelta;
            progress.Height = 0;
                if (LightMap.Mapping.ContainsKey(light.LampType))
                    image.Source = LightMap.Mapping[light.LampType];

                if (light.Color != "")
                {
                    ColorConverter cc = new ColorConverter();
                    try
                    {
                        Color c = (Color)cc.ConvertFrom(light.Color);
                        color.Fill = new SolidColorBrush(c);
                    }
                    catch (Exception e)
                    {

                    }
                }
                else
                {
                    color.Fill = Brushes.Transparent;
                }

                if (thelight.UsedChannels == 0)
                    thelight.UsedChannels = 1;

                ChannelValue = new int[thelight.UsedChannels];
        }

        void LightControl_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            if (TheLight.UsedChannels != 3)
            {
                if (!window.dmxcontroller.Blackout)
                {
                    int val = ChannelValue[0] + ((int)e.DeltaManipulation.Translation.Y * -2);

                    if (val < 0)
                        val = 0;
                    if (val > 255)
                        val = 255;

                    ChannelValue[0] = val;
                    if (!window.LockLiveOutput)
                    {
                        OnLightUpdate(Channel, this.ChannelValue[0]);
                    }

                    progress.Height = (val / 255.0f) * 30;
                    perc.Content = (int)((val / 255.0f) * 100) + "%";
                }
            }
            else
            {
                //the color wheel
                Point pp = e.ManipulationOrigin;

                var c = this.TouchesCaptured;


                //get the point relative to gettouchpoint


                Point p = c.First().GetTouchPoint(colorimg).Position;
                //Point p = window.TranslatePoint(pp, colorimg);

                //get the point rel

                //Point p = pp;
                //Debug.Print(p.ToString());
                int x = (int)Math.Min(149, Math.Max(0, p.X));
                int y = (int)Math.Min(149, Math.Max(0, p.Y));


                int r = Wheel.GetPixel(x, y).R;
                int g = Wheel.GetPixel(x, y).G;
                int b = Wheel.GetPixel(x, y).B;

                ChannelValue[0] = r;
                ChannelValue[1] = g;
                ChannelValue[2] = b;

                //Debug.Print("Red: " + Wheel.GetPixel(x,y).R);
                // Debug.Print("Green: " + Wheel.GetPixel(x, y).G);
                //Debug.Print("Blue: " + Wheel.GetPixel(x, y).B);
                if (!window.LockLiveOutput) // update live...
                {
                    OnLightUpdate(this.Channel, r);
                    OnLightUpdate(this.Channel + 1, g);
                    OnLightUpdate(this.Channel + 2, b);
                }

                finalcolor.Stroke = new SolidColorBrush(Color.FromRgb((byte)r, (byte)g, (byte)b));
                color.Fill = finalcolor.Stroke;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    UpdateChannel(ChannelValue);
                }));
            }
        }


        public int[] ChannelValue { get; set; }

        public delegate void LightPreviewUpdate(LightControl me, int channel, int[] val);
        public event LightPreviewUpdate OnLightPreviewUpdate;

        public void UpdateChannel(int[] val)
        {
            //ChannelValue = val;
            //ChannelValue = val;
            double agg = 0;
            foreach (int i in val)
            {
                agg += i;
            }

            agg = agg / val.Count();

            progress.Height = (agg / 255.0f) * 32;
            perc.Content = (int)((agg / 255.0f) * 100) + "%";

            //update all the other ones too...
            if (OnLightPreviewUpdate != null)
                OnLightPreviewUpdate(this, Channel, ChannelValue);

            //progress.Height = (val / 255.0f) * 32;
            //perc.Content = (int)((val / 255.0f) * 100) + "%";

            ////update all the other ones too...
            //if (OnLightPreviewUpdate != null)
            //    OnLightPreviewUpdate(this, Channel, ChannelValue);
        }

        public void UpdateChannelNoLoop(int[] val)
        {
            double agg = 0;
            foreach (int i in val)
            {
                agg += i;
            }

            agg = agg / val.Count();

            //ChannelValue = val;
            progress.Height = (agg / 255.0f) * 32;
            perc.Content = (int)((agg / 255.0f) * 100) + "%";
        }

        public void UpdateLiveChannel(int[] val)
        {
            //live.Height = (val / 255.0f) * 32;
            double agg = 0;
            foreach (int i in val)
            {
                agg += i;
            }
            agg = agg / val.Count();

            live.Height = (agg / 255.0f) * 32;
            if (TheLight.UsedChannels == 3)
            {
                color.Fill = new SolidColorBrush(Color.FromRgb((byte)val[0], (byte)val[1], (byte)val[2]));
            }
        }

        private void SurfaceUserControl_PreviewContactDown(object sender, TouchEventArgs e)
        {
            //e.Handled = true;
        }

        protected override void OnLostTouchCapture(TouchEventArgs e)
        {
            base.OnLostTouchCapture(e);

            if (e.OriginalSource == this)
            {
                //#warning TODO: Pass the proper UIElement.
                Manipulation.RemoveManipulator(this, e.TouchDevice);
                Manipulation.CompleteManipulation(this);
            }
        }


        private void SurfaceUserControl_ContactDown(object sender, TouchEventArgs e)
        {
            base.OnTouchDown(e);
            // Capture this contact
            if (!window.cuedisplay.SelectPressed)
            {
                //e.Contact.Capture(this);
                if (e.TouchDevice.Capture(this))
                {
                    //EnsureManipulationProcessor();
                    Manipulation.AddManipulator(this, e.TouchDevice);
                }

                window.dirtyflag.Visibility = Visibility.Visible;
                // Mark this event as handled
                e.Handled = true;
            }
            else
            {
                Selected = !Selected;
                if (window.cuedisplay.SelectedLights.Count > 0)
                {
                    window.cuedisplay.ShowFader();
                }
                else
                {
                    window.cuedisplay.fader.Visibility = Visibility.Hidden;
                }
            }

            if (window.cuedisplay.FlashPressed)
            {
                int val = 255;

                if (val < 0)
                    val = 0;
                if (val > 255)
                    val = 255;

                ChannelValue[0] = val;
                if (!window.LockLiveOutput)
                {
                    OnLightUpdate(Channel, val);
                }
                progress.Height = (val / 255.0f) * 30;
                perc.Content = (int)((val / 255.0f) * 100) + "%";
            }

            if (TheLight.UsedChannels == 3)
                colorwheel.Visibility = System.Windows.Visibility.Visible;

        }

        private void SurfaceUserControl_ContactChanged(object sender, TouchEventArgs e)
        {
            //e.Handled = true;
        }

        private void SurfaceUserControl_PreviewContactChanged(object sender, TouchEventArgs e)
        {
            //e.Handled = true;
        }

        private void SurfaceUserControl_ContactUp(object sender, TouchEventArgs e)
        {
            if (window.cuedisplay.FlashPressed)
            {
                int val = 0;

                if (val < 0)
                    val = 0;
                if (val > 255)
                    val = 255;

                for (int i=0;i<ChannelValue.Count();i++)
                    ChannelValue[i] = val;

                if (!window.LockLiveOutput)
                {
                    OnLightUpdate(Channel, val);
                }
                progress.Height = (val / 255.0f) * 30;
                perc.Content = (int)((val / 255.0f) * 100) + "%";
            }

            e.Handled = true;
            //Contacts.ReleaseContactCapture(e.Contact);

            colorwheel.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
