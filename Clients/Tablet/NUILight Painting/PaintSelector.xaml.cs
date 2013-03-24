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
using Microsoft.Surface.Presentation.Controls;
using ScottLogic.Shapes;
using System.Timers;
using System.Windows.Threading;

namespace IPS.TabletPainting
{
    /// <summary>
    /// Interaction logic for PaintSelector.xaml
    /// </summary>
    public partial class PaintSelector : UserControl
    {
        public PaintSelector()
        {
            InitializeComponent();
        }

        RigView r;
        public PaintSelector(RigView r)
        {
            InitializeComponent();
            this.r = r;

        }

        private void Image_ContactTapGesture(object sender, TouchEventArgs e)
        {
            
        }

        private void Ellipse_PreviewContactDown(object sender, TouchEventArgs e)
        {
            DispatcherTimer tt = new DispatcherTimer();
            button.Stroke = Brushes.Aquamarine;
            tt.Interval = TimeSpan.FromMilliseconds(300);
            tt.Tick+=new EventHandler((o,ex)=>{
                button.Stroke = Brushes.Transparent;
                tt.Stop();
            });
            tt.Start();
            

            Color c = paintbucket.GetMixedColor();

            //will need to translate this point...
            //Point point = FindScatterParent(this).Center;

            Point point = this.TranslatePoint(new Point(0, this.ActualHeight / 2), r);

            Point seqpt = this.TranslatePoint(new Point(0, this.ActualHeight / 2), r.sequencer);

            Sequencer.SequenceEvent fr = r.sequencer.HitTestSequence(seqpt);
            if (fr != null)
            {
                fr.Color = c;
                ((PiePiece)fr.Element).Fill = new SolidColorBrush(c);
                ((Shooter)fr.Shooter).Color = c;
                r.sequencer.ResetSeq(fr);
                
            }

            List<Lamp> lm = r.GetLampsFromPosition(point);
            if (lm.Count>0)
            {
                RgbHsl.HSL h2 = RgbHsl.RGB_to_HSL(c);
                //Console.WriteLine("Main H: " + h2.H);
                foreach (Lamp l in lm)
                {
                    RgbHsl.HSL h1 = RgbHsl.RGB_to_HSL(l.Color);
                    
                    
                    //Console.WriteLine("Chan:" + l.Channel);
                    int chan = l.Channel;
                    double lev = l.Level;
                    //Console.WriteLine("Channel: " + chan + " H:" + lev);
                    if (Math.Abs(h1.H - h2.H) < 0.15)
                    {
                        int time = 0;
                        double val = lev;
                        Timer t = new Timer();
                        t.Interval = 1;
                        t.Elapsed += new ElapsedEventHandler((ox, ex) =>
                        {
                            val = Math.Min(1, val + 0.01);
                            
                            r.Window.dmxcontroller.UpdateValue(chan, (byte)Math.Min(255, ((val) * 255)));
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                l.Level = val;
                            }));
                            if (time > 5)
                            {
                                time = 0;
                                t.Stop();
                            }
                            time++;
                        });
                        t.Start();
                    }
                }
            }
        }

        public double ColorSimilarity(Color a, Color b)
        {
            double d = Math.Sqrt(Math.Pow(a.R - b.A, 2) + Math.Pow(a.B - b.B, 2) + Math.Pow(a.G - b.G, 2));
            return d;
        }

        public static ScatterViewItem FindScatterParent(FrameworkElement u)
        {
            FrameworkElement returns = u;
            while (!(returns is ScatterViewItem) && returns!=null)
            {
                returns = (FrameworkElement)returns.Parent;
            }
            return (ScatterViewItem)returns;
        }

        private void small_PreviewContactTapGesture(object sender, TouchEventArgs e)
        {
            Ellipse_PreviewContactDown(sender, e);
        }

        private void Ellipse_ContactDown(object sender, TouchEventArgs e)
        {
            r.scatterview.Items.Remove(FindScatterParent(this));
        }

        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            r.scatterview.Items.Remove(FindScatterParent(this));
        }

        public delegate void ActionI(Lamp l,double o);
        private void button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            DispatcherTimer tt = new DispatcherTimer();
            button.Stroke = Brushes.Aquamarine;
            tt.Interval = TimeSpan.FromMilliseconds(300);
            tt.Tick += new EventHandler((o, ex) =>
            {
                button.Stroke = Brushes.Transparent;
                tt.Stop();
            });
            tt.Start();


            Color c = paintbucket.GetMixedColor();

            //will need to translate this point...
            //Point point = FindScatterParent(this).Center;

            Point point = this.TranslatePoint(new Point(0, this.ActualHeight / 2), r);

            Point seqpt = this.TranslatePoint(new Point(0, this.ActualHeight / 2), r.sequencer);

            Sequencer.SequenceEvent fr = r.sequencer.HitTestSequence(seqpt);
            if (fr != null)
            {
                fr.Color = c;
                ((PiePiece)fr.Element).Fill = new SolidColorBrush(c);
                ((Shooter)fr.Shooter).Color = c;
                r.sequencer.ResetSeq(fr);

            }

            List<Lamp> lm = r.GetLampsFromPosition(point);
            if (lm.Count > 0)
            {
                RgbHsl.HSL h2 = RgbHsl.RGB_to_HSL(c);
                //Console.WriteLine("Main H: " + h2.H);
                foreach (Lamp l in lm)
                {
                    RgbHsl.HSL h1 = RgbHsl.RGB_to_HSL(l.Color);


                    //Console.WriteLine("Chan:" + l.Channel);
                    int chan = l.Channel;
                    double lev = l.Level;
                    //Console.WriteLine("Channel: " + chan + " H:" + lev);
                    if (Math.Abs(h1.H - h2.H) < 0.25 || 1 - Math.Abs(h1.H - h2.H) < 0.25)
                    {
                        int time = 0;
                        double val = lev;
                        Timer t = new Timer();
                        t.Interval = 1;
                        t.Elapsed += new ElapsedEventHandler((ox, ex) =>
                        {
                            val = Math.Min(1, val + 0.01);

                            r.Window.dmxcontroller.UpdateValue(chan, (byte)Math.Min(255, ((val) * 255)));
                            Dispatcher.BeginInvoke(new ActionI((lmm,o) =>
                            {
                                //HACK
                                //lmm.Level = o;
                            }),new object[]{l,val});
                            if (time > 5)
                            {
                                time = 0;
                                t.Stop();
                            }
                            time++;
                        });
                        t.Start();
                    }
                }
            }
        }
    }
}
