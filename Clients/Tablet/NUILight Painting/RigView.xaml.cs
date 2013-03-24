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
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;
using System.Diagnostics;
using IPS.TabletPainting;
using IPS.SharedObjects;

namespace IPS.TabletPainting
{
    /// <summary>
    /// Interaction logic for RigView.xaml
    /// </summary>
    public partial class RigView : UserControl
    {
        public RigView()
        {
            InitializeComponent();
            //xproc = new Affine2DManipulationProcessor(Affine2DManipulations.TranslateX, grid);
            //yproc = new Affine2DManipulationProcessor(Affine2DManipulations.TranslateY, grid);


            //xproc.Affine2DManipulationCompleted += new EventHandler<Affine2DOperationCompletedEventArgs>();
            //yproc.Affine2DManipulationCompleted += new EventHandler<Affine2DOperationCompletedEventArgs>(yproc_Affine2DManipulationCompleted);
            this.Loaded += new RoutedEventHandler(RigView_Loaded);
        }

        void RigView_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
        private SurfaceWindow1 win;
        public SurfaceWindow1 Window { get { return win; } set { win = value; sequencer.Window = value; } }

        public Rig therig {get;set;}

        //Affine2DManipulationProcessor xproc;
        //Affine2DManipulationProcessor yproc;

        public void SetRig(Rig r, BitmapImage o)
        {
            this.therig = r;
            this.Background = new ImageBrush(o);

            //load rig onto the page...
            foreach (Light l in r.Lights)
            {
                if (!l.Hidden)
                {
                    Lamp lmp = new Lamp(this);
                    try
                    {
                        lmp.Color = (Color)ColorConverter.ConvertFromString(l.Color);
                    }
                    catch
                    {

                    }
                    lights.Children.Add(lmp);
                    lmp.Channel = l.Channel;

                    Canvas.SetLeft(lmp, l.Position.X * lights.ActualWidth - lmp.Width / 2);
                    Canvas.SetTop(lmp, l.Position.Y * lights.ActualHeight - lmp.Height / 2);
                }
            }
        }

        public List<Lamp> GetLampsFromPosition(Point p)
        {
            //Button b = new Button();
            //grid.Children.Add(b);
            //Canvas.SetLeft(b, p.X);
            //Canvas.SetTop(b, p.Y);

            List<Lamp> found = new List<Lamp>();
            foreach (Lamp l in lights.Children)
            {
                Point c = new Point(Canvas.GetLeft(l) + l.ActualWidth/2,Canvas.GetTop(l)+l.ActualHeight/2);
                if (distance(c, p) < l.ActualWidth/2)
                {
                    found.Add(l);
                }
            }

            return found;
        }

        private Lamp GetLampFromChild(DependencyObject fr)
        {
            FrameworkElement returns = fr as FrameworkElement;
            while (!(returns is Lamp) && returns != null)
            {
                returns = (FrameworkElement)returns.Parent;
            }
            return (Lamp)returns;
        }

        private void Grid_ContactDown(object sender, TouchEventArgs e)
        {
            ////e.Handled = false;
            ////after 100 milis...

            ////xproc.BeginTrack(e.Contact);
            ////yproc.BeginTrack(e.Contact);

            ////contacts.Add(e.Contact.Id, e.Contact);

            //FingerMenu m = new FingerMenu(this);
            //grid.Children.Add(m);
            ////m.Margin = new Thickness(e.Contact.GetPosition(this).X, e.Contact.GetPosition(this).Y, 0, 0);
            //Canvas.SetTop(m, e.GetTouchPoint(grid).Position.Y - 25);
            //Canvas.SetLeft(m, e.GetTouchPoint(grid).Position.X - 25);
            ////m.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            ////m.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            //menus.Remove(e.TouchDevice.Id);
            //menus.Add(e.TouchDevice.Id, m);
            //origins.Remove(e.TouchDevice.Id);
            //origins.Add(e.TouchDevice.Id, e.GetTouchPoint(grid).Position);
            //Totals.Remove(e.TouchDevice.Id);
            //Totals.Add(e.TouchDevice.Id, 0);

        }

        Dictionary<int, TouchPoint> contacts = new Dictionary<int, TouchPoint>();
        Dictionary<int, FingerMenu> menus = new Dictionary<int, FingerMenu>();
        Dictionary<int, Point> origins = new Dictionary<int, Point>();

        private void Grid_ContactUp(object sender, TouchEventArgs e)
        {
            ////e.Handled = false;
            //if (menus.ContainsKey(e.TouchDevice.Id))
            //{

            //    grid.Children.Remove(menus[e.TouchDevice.Id]);
            //    menus.Remove(e.TouchDevice.Id);
            //}

            //if (origins.ContainsKey(e.TouchDevice.Id))
            //{

            //    //double length = Math.Sqrt(Math.Pow(origins[e.Contact.Id].X - e.GetPosition(grid).X, 2) + Math.Pow(origins[e.Contact.Id].Y - e.GetPosition(grid).Y, 2));
            //    double length = Totals[e.TouchDevice.Id];
            //    //Console.WriteLine("length: " + length);

            //    if (length > 100 && length < 400)
            //    {
            //        //do soemthing with end point...

            //        Line l = new Line();
            //        l.X1 = origins[e.TouchDevice.Id].X;
            //        l.Y1 = origins[e.TouchDevice.Id].Y;
            //        l.X2 = e.GetTouchPoint(grid).Position.X;
            //        l.Y2 = e.GetTouchPoint(grid).Position.Y;

            //        //work out the angle...
            //        double angle = (Math.Atan2(l.Y1 - l.Y2, l.X1 - l.X2) / Math.PI) * 180;

            //        sequencer.StartSeq((int)angle - 90, Colors.Black, (int)length*15, 1000);

            //        //get distance from edge of screen, use line length over this as a proportion of time light should be on.
            //    }
            //    origins.Remove(e.TouchDevice.Id);

            //}
            //origins.Remove(e.TouchDevice.Id);
            //Totals.Remove(e.TouchDevice.Id);
            //menus.Remove(e.TouchDevice.Id);

        }
        Dictionary<int, Point> lastpos = new Dictionary<int, Point>();

        Dictionary<int, double> Totals = new Dictionary<int, double>();

        private void Grid_ContactChanged(object sender, TouchEventArgs e)
        {
            ////e.Handled = false;
            ////cancel the waiting...
            ////contacts.Remove(e.Contact.Id);
            //if (lastpos.ContainsKey(e.TouchDevice.Id))
            //{
            //    if (menus.ContainsKey(e.TouchDevice.Id) && point_diff(lastpos[e.TouchDevice.Id], e.GetTouchPoint(grid).Position))
            //    {
            //        grid.Children.Remove(menus[e.TouchDevice.Id]);
            //        menus.Remove(e.TouchDevice.Id);
            //    }
            //    if (Totals.ContainsKey(e.TouchDevice.Id))
            //        Totals[e.TouchDevice.Id] = Totals[e.TouchDevice.Id] + lastpos[e.TouchDevice.Id].Distance(e.GetTouchPoint(grid).Position);
            //}


            //lastpos[e.TouchDevice.Id] = e.GetTouchPoint(grid).Position;
            ////incremental distance...
            
        }

        private bool point_diff(Point point, Point point_2)
        {
            double xdiff = point.X - point_2.X;
            double ydiff = point.Y - point_2.Y;

            return !(Math.Abs(xdiff - ydiff) < 5);
            //return diff.
        }

        private double distance(Point point, Point point_2)
        {
            double xdiff = point.X - point_2.X;
            double ydiff = point.Y - point_2.Y;

            return Math.Sqrt(xdiff * xdiff + ydiff * ydiff);
            //return diff.
        }

        private void sequencer_ContactDown(object sender, TouchEventArgs e)
        {

        }

        private void grid_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if (!menus.ContainsKey(999))
            {
                FingerMenu m = new FingerMenu(this);
                grid.Children.Add(m);
                //m.Margin = new Thickness(e.Contact.GetPosition(this).X, e.Contact.GetPosition(this).Y, 0, 0);
                Canvas.SetTop(m, (e.GetPosition(grid).Y * 1.25) - 25);
                Canvas.SetLeft(m, (e.GetPosition(grid).X * 1.25) - 25);
                //m.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                //m.VerticalAlignment = System.Windows.VerticalAlignment.Top;

                menus.Add(999, m);
                if (!origins.ContainsKey(999))
                    origins.Add(999, e.GetPosition(grid));
                if (!Totals.ContainsKey(999))
                    Totals.Add(999, 0);
            }
        }

        private void grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //if its a right click


                //if its a left click


                e.Handled = false;
                if (menus.ContainsKey(999))
                {

                    grid.Children.Remove(menus[999]);
                    menus.Remove(999);
                }

                if (origins.ContainsKey(999))
                {

                    //double length = Math.Sqrt(Math.Pow(origins[e.Contact.Id].X - e.GetPosition(grid).X, 2) + Math.Pow(origins[e.Contact.Id].Y - e.GetPosition(grid).Y, 2));
                    double length = Totals[999];
                    //Console.WriteLine("length: " + length);

                    if (length > 60 && length < 400)
                    {
                        //do soemthing with end point...

                        Line l = new Line();
                        l.X1 = origins[999].X;
                        l.Y1 = origins[999].Y;
                        l.X2 = e.GetPosition(grid).X;
                        l.Y2 = e.GetPosition(grid).Y;

                        //work out the angle...
                        double angle = (Math.Atan2(l.Y1 - l.Y2, l.X1 - l.X2) / Math.PI) * 180;


                        bool startseq = false;
                        //if its in the right quadrant...
                        //1
                        if (l.X1 <= 512 && l.Y1 <= 300 && angle < 90 && angle > 0)
                        {
                            startseq = true;
                        }

                        if (l.X1 >= 512 && l.Y1 <= 300 && angle > 90 && angle < 180)
                        {
                            startseq = true;
                        }


                        if (l.X1 < 512 && l.Y1 > 300 && angle > -90 && angle < 0)
                        {
                            startseq = true;
                        }


                        if (l.X1 > 512 && l.Y1 > 300 && angle < -90 && angle > -180)
                        {
                            startseq = true;
                        }

                        if (startseq)
                            sequencer.StartSeq((int)angle - 90, Colors.Black, (int)length * 15, 1000);

                        //get distance from edge of screen, use line length over this as a proportion of time light should be on.
                    }



                }
                origins.Remove(999);
                Totals.Remove(999);
                menus.Remove(999);
        }

        

        private void grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (lastpos.ContainsKey(999))
            {
                if (menus.ContainsKey(999) && point_diff(lastpos[999], e.GetPosition(grid)))
                {
                    grid.Children.Remove(menus[999]);
                    menus.Remove(999);
                }
                if (Totals.ContainsKey(999))
                {
                    Totals[999] = Totals[999] + lastpos[999].Distance(e.GetPosition(grid));
                }                
            }

            //update lines...
            lastpos[999] = e.GetPosition(grid);
            
        }
    }
}

