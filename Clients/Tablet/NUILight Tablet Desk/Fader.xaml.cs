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
using System.Windows.Navigation;
using System.Windows.Shapes;
using IPS.SharedObjects;
using Microsoft.Surface.Presentation.Controls;
using IPS.Controller;

namespace IPS.TabletDesk
{
	/// <summary>
	/// Interaction logic for Fader.xaml
	/// </summary>
	public partial class Fader : UserControl
	{
		public Fader()
		{
			InitializeComponent();
		}

        int channel = 0;
        IController dmx;

        public Fader(IController dmx,int channel)
        {
            InitializeComponent();
            this.dmx = dmx;
            this.channel = channel;
            chan.Content = channel;
        }

        public void UpdateValue(int val)
        {
            
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!updating)
            {
                dmx.UpdateValue(channel, (int)((e.NewValue / 100.0) * 255.0));
            }
            value.Content = (int)e.NewValue + "%";
        }

        internal void SetLamp(Light l)
        {
            try
            {
                chan.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(l.Color));
            }
            catch {}
        }
        bool updating = false;
        internal void Update(int p)
        {
            updating = true;
            value.Content = (int)((p / 255)*100) + "%";
            slider.Value = (p / 255.0) * 100.0;
            updating = false;
        }
    }
}