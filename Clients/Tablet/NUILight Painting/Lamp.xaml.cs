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
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Timers;
using IPS.SharedObjects;
using IPS.TabletPainting;

namespace IPS.TabletPainting
{
    /// <summary>
    /// Interaction logic for Lamp.xaml
    /// </summary>
    public partial class Lamp : UserControl
    {
        public Lamp()
        {
            InitializeComponent();
        }
        RigView rig;
        public Lamp(RigView r)
        {
            InitializeComponent();
            this.rig = r;
            Level = 0.0;
        }


        public Light Light { get; set; }

        private double level;
        public double Level { get { return level; } 
            set { 
                this.Opacity = 0.8*value;
                if (this.Opacity == 0) { 
                    this.Visibility = System.Windows.Visibility.Hidden;
                } else { 
                    this.Visibility = System.Windows.Visibility.Visible;
                } 
                level = value; 
            } 
        }
        public Color Color { set { ellipse.Fill = new SolidColorBrush(value); } get { return ((SolidColorBrush)ellipse.Fill).Color; } }
        public int Channel { get; set; }

        private void UserControl_ContactDown(object sender, TouchEventArgs e)
        {
            last = e.GetTouchPoint(this).Position;
        }

        Point last = new Point();
        double totaldistance = 0;

        private void UserControl_ContactChanged(object sender, TouchEventArgs e)
        {
            totaldistance += e.GetTouchPoint(this).Position.Distance(last);

            last = e.GetTouchPoint(this).Position;

            //Debug.Print("td:"+totaldistance);
            if (totaldistance > 100)
            {
                int time = 0;
                double val = Level;
                Timer t = new Timer();
                t.Interval = 1;
                t.Elapsed += new ElapsedEventHandler((ox, ex) =>
                {
                    val = Math.Max(0, val - 0.01);
                    rig.Window.dmxcontroller.UpdateValue(Channel, (byte)Math.Max(0, ((val) * 255)));
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        //HACK
                        //Level = val;
                    }));

                    if (time > 5)
                    {
                        time = 0;
                        t.Stop();
                    }
                    time++;
                });
                t.Start();
                //Level = Level - 0.2;
                //this is slow...
                //rig.Window.dmxcontroller.UpdateValue(Channel, (byte)Math.Max(0,((Level-0.2)*255)));
                //Console.WriteLine("LEVEL:" + ((Level - 0.2) * 255));
                totaldistance = 0;
            }
            Canvas.SetZIndex(this, 1);
        }

        private void UserControl_ContactLeave(object sender, TouchEventArgs e)
        {
            totaldistance = 0;
            Canvas.SetZIndex(this, 0);
        }

        private void UserControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            last = e.GetPosition(this);
        }

        private void UserControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                totaldistance += e.GetPosition(this).Distance(last);

                last = e.GetPosition(this);

                //Debug.Print("td:"+totaldistance);
                if (totaldistance > 100)
                {
                    int time = 0;
                    double val = Level;
                    Timer t = new Timer();
                    t.Interval = 1;
                    t.Elapsed += new ElapsedEventHandler((ox, ex) =>
                    {
                        val = Math.Max(0, val - 0.01);
                        rig.Window.dmxcontroller.UpdateValue(Channel, (byte)Math.Max(0, ((val) * 255)));
                        Dispatcher.BeginInvoke(new PaintSelector.ActionI((lm,o) =>
                        {
                            //HACK
                            //lm.Level = o;
                        }),new object[] {this,val});
                        if (time > 5)
                        {
                            time = 0;
                            t.Stop();
                        }
                        time++;
                    });
                    t.Start();
                    //Level = Level - 0.2;
                    //this is slow...
                    //rig.Window.dmxcontroller.UpdateValue(Channel, (byte)Math.Max(0,((Level-0.2)*255)));
                    //Console.WriteLine("LEVEL:" + ((Level - 0.2) * 255));
                    totaldistance = 0;
                }
                Canvas.SetZIndex(this, 1);
            }
        }

        private void UserControl_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            totaldistance = 0;
            Canvas.SetZIndex(this, 0);
        }
    }
}
