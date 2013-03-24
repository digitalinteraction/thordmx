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
using System.Windows.Threading;
using System.Windows.Media.Animation;

namespace IPS.TabletPainting
{
    /// <summary>
    /// Interaction logic for Hits.xaml
    /// </summary>
    public partial class Hits : UserControl
    {
        RotateTransform rt = new RotateTransform();
        public Hits()
        {
            InitializeComponent();

            this.RenderTransformOrigin = new Point(0.5, 0.5);
            this.RenderTransform = rt;
            
            RunTimer();

            Hints.Add("Hold down to find the color mixer...");
            Hints.Add("Try rubbing lights to make them get darker...");
            Hints.Add("Drag your finger towards a light to start flashing it...");
            Hints.Add("Did you know you can drop colors straight onto the lights?");
            Hints.Add("You can change to color of a flasher by dropping a color onto it...");
        }

        private List<string> Hints = new List<string>();

        private void RunTimer()
        {
            Random rand = new Random(DateTime.Now.Millisecond);
            DispatcherTimer t = new DispatcherTimer();
            t.Interval = TimeSpan.FromSeconds(rand.Next(10, 30));
            t.Tick += new EventHandler((o, e) =>
            {               
                //DoubleAnimation da = new DoubleAnimation();
                //da.AccelerationRatio = 0.5;
                //da.DecelerationRatio = 0.5;
                //da.Duration = TimeSpan.FromSeconds(3);
                //da.From = rt.Angle;
                //da.To = rand.Next(0, 360);
                //rt.BeginAnimation(RotateTransform.AngleProperty, da);
                text.Text = Hints[rand.Next(0, Hints.Count-1)];
                //RunTimer();
                //t.Stop();
            });
            t.Start();
        }
    }

    
}
