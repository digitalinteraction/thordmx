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
using IPS.SharedObjects;
using IPS.Controller;

namespace IPS.TabletDesk
{
    /// <summary>
    /// Interaction logic for Faders.xaml
    /// </summary>
    public partial class Faders : UserControl
    {
        public Faders()
        {
            InitializeComponent();
        }

        IController dmx;

        public void Start(IController dmx,Rig rig)
        {
            //the controller
            this.dmx = dmx;
            for (int i = 1; i < 51; i++)
            {
                
                Fader f = new Fader(dmx,i);

                var l = (from n in rig.Lights where n.Channel == i select n);
                if (l.Count() > 0)
                {
                    f.SetLamp(l.First());
                }
                stack.Children.Add(f);
            }
            stack.Width = 100 * 50;
        }

        private void SurfaceButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	this.Visibility = Visibility.Hidden;
        }

        internal void Update(byte[] o)
        {
            if (this.Visibility == System.Windows.Visibility.Hidden)
            {
                for (int i = 0; i < 50; i++)
                {
                    (stack.Children[i] as Fader).Update(o[i+1]);
                }
            }
        }
    }
}
