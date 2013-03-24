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
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation;
using System.Windows.Media.Effects;
using ScottLogic.Shapes;

namespace IPS.TabletPainting
{
    /// <summary>
    /// Interaction logic for FingerMenu.xaml
    /// </summary>
    public partial class FingerMenu : UserControl
    {
        public FingerMenu()
        {
            InitializeComponent();
        }

        private PiePiece GenPie(int angle, Color c)
        {
            PiePiece pie = new PiePiece();
            pie.WedgeAngle = 20;
            pie.CentreX = 150 / 2;
            pie.CentreY = 150 / 2;
            pie.RotationAngle = angle;
            pie.Radius = 150 / 2 - 25;
            pie.InnerRadius = 75;

            //grid.Children.Add(pie);
            pie.Fill = new SolidColorBrush(c);
            return pie;
        }

        DispatcherTimer timer = new DispatcherTimer();
        public FingerMenu(RigView rig)
        {
            InitializeComponent();
            
            //CircularMenu menu = new CircularMenu();
            //menu.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            //menu.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            //menu.Visibility = Visibility.Hidden;
            //canvas.Children.Add(menu);
            //Canvas.SetLeft(menu, menu.ActualWidth / 2);
            //Canvas.SetTop(menu, menu.ActualHeight / 2);


            PiePiece pie = new PiePiece();
            pie.CentreX = this.Width / 2;
            pie.CentreY = this.Height / 2;
            pie.Radius = this.Height / 2 - 5;
            pie.InnerRadius = this.Height/2 - 10;
            pie.Fill = Brushes.Aqua;
            canvas2.Children.Add(pie);

            DispatcherTimer main = new DispatcherTimer();
            main.Interval = TimeSpan.FromMilliseconds(300);
            main.Tick+=new EventHandler((oxx,exx)=>{
                main.Stop();
                DispatcherTimer t = new DispatcherTimer();
                t.Tick += new EventHandler((o, e) =>
                {
                    pie.WedgeAngle = Math.Min(pie.WedgeAngle + 6, 359);

                });
                t.Interval = TimeSpan.FromMilliseconds(10);
                t.Start();

                timer.Interval = TimeSpan.FromMilliseconds(920);
                timer.Tick += new EventHandler((o, e) =>
                {
                    //dispaly the menu...
                    PaintSelector selector = new PaintSelector(rig);
                    ScatterViewItem it = new ScatterViewItem();
                    it.BorderThickness = new Thickness(0);
                    //it.ShowsActivationEffects = false;
                    it.Center = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
                    it.CanScale = false;
                    it.ClipToBounds = false;
                    it.CanRotate = false;
                    it.Width = 227;
                    it.Height = 151;
                    it.ContainerManipulationDelta += new ContainerManipulationDeltaEventHandler(it_ContainerManipulationDelta);
                    it.Deceleration = 9999999;
                    //it.Background = Brushes.Transparent;
                    it.Content = selector;
                    rig.scatterview.Items.Add(it);
                    timer.Stop();
                    rig.grid.Children.Remove(this);

                    it.ApplyTemplate();
                    it.Background = null;
                    it.ShowsActivationEffects = false;
                    //Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome ssc;
                    //ssc = it.Template.FindName("shadow", it) as Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome;
                    //ssc.Visibility = Visibility.Hidden;
                    Vector dist = it.Center - new Point(512, 300);

                    double angle = Math.Atan2(300 - it.Center.Y, 512 - it.Center.X);
                    double deg = (angle / Math.PI) * 180;

                    //if distance was small, then reverse...

                    //Console.WriteLine(dist.Length);

                    if (dist.Length < 170)
                        it.Orientation = deg + 180;
                    else
                        it.Orientation = deg;
                });
                timer.Start();               
});
            main.Start();

            this.Unloaded += new RoutedEventHandler((o, e) =>
            {
                main.Stop();
                if (timer != null)
                    timer.Stop();
            });
        }

        void it_ContainerManipulationDelta(object sender, ContainerManipulationDeltaEventArgs e)
        {
            ScatterViewItem it = sender as ScatterViewItem;
            Vector dist = it.Center - new Point(512, 300);

            double angle = Math.Atan2(300-it.Center.Y,512- it.Center.X);
            double deg = (angle / Math.PI) * 180;

            //if distance was small, then reverse...

            //Console.WriteLine(dist.Length);

            if (dist.Length < 170)
                it.Orientation = deg + 180;
            else
                it.Orientation = deg ;
        }

        void it_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            //scatter view item moved...
            //ScatterViewItem it = sender as ScatterViewItem;
            //it.Orientation++;
        }
    }
}
