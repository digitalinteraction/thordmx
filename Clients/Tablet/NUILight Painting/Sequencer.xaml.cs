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
using DrawRect;
using System.Windows.Threading;
using ScottLogic.Shapes;
using System.Windows.Media.Animation;
using Microsoft.Surface.Presentation;
using System.Timers;
using IPS.TabletPainting;

namespace IPS.TabletPainting
{

    

    /// <summary>
    /// Interaction logic for Sequencer.xaml
    /// </summary>
    public partial class Sequencer : UserControl
    {

        List<SequenceEvent> Sequences = new List<SequenceEvent>();

        public Sequencer()
        {
            InitializeComponent();
            i.Source = img;
            i.Stretch = Stretch.None;
            grid.Children.Add(i);
            i.IsHitTestVisible = false;


            //GenPie(46, Colors.Green);
            

            //StartSeq(120, Colors.Green, 3000, 1000);
        }

        public SurfaceWindow1 Window {get;set;}

        private UIElement GenPie(int angle, Color c)
        {
            PiePiece pie = new PiePiece();
            pie.WedgeAngle = 20;
            pie.CentreX = 150 / 2;
            pie.CentreY = 150 / 2;
            pie.RotationAngle = angle - 10;
            pie.Radius = 150 / 2 - 25;
            pie.InnerRadius = 75;

            //grid.Children.Add(pie);
            pie.Fill = new SolidColorBrush(c);
            return pie;
        }

        public SequenceEvent HitTestSequence(Point p)
        {
            //Point pt = new Point(p.X - Canvas.GetLeft(this), p.Y - Canvas.GetTop(this));

            HitTestResult ht = VisualTreeHelper.HitTest(grid, p);

            //FrameworkElement ht = this.InputHitTest(p) as FrameworkElement;
            if (ht != null)
            {
                return FindFromElement(ht.VisualHit);
            }
            else
            {
                return null;
            }
        }

        private SequenceEvent FindFromElement(DependencyObject el)
        {
            foreach (SequenceEvent e in Sequences)
            {
                if (e.Element == el)
                    return e;
            }
            return null;
        }

        public void StartSeq(int dir, Color c,int timeon, int timeoff)
        {
            Line l = new Line();

            SequenceEvent s = new SequenceEvent();
            s.Color = c;
            s.Angle = dir;
            s.Element = GenPie(dir, c);
            s.Element.IsHitTestVisible = true;

            s.Element.MouseDown += new MouseButtonEventHandler((o, e) =>
            {
                Sequences.Remove(s);
                grid.Children.Remove(s.Element);
                grid.Children.Remove(s.Shooter);
                grid.Children.Remove(l);
                s.DTimer.Stop();
                s.Timer.Stop();
                s.offsetimer.Stop();
            });

            s.Element.TouchDown += new EventHandler<TouchEventArgs>((o, e) => {
                Sequences.Remove(s);
                grid.Children.Remove(s.Element);
                grid.Children.Remove(s.Shooter);
                grid.Children.Remove(l);
                s.DTimer.Stop();
                s.Timer.Stop();
                s.offsetimer.Stop();
            });

            s.TimeOn = timeon;
            s.TimeOff = timeoff;
            grid.Children.Add(s.Element);
            
            //draw a line in this direction...
            l.X1 = 150 / 2;
            l.Y1 = 150 / 2;

            //final dest
            dir = dir - 90;
            double x = Math.Cos((dir / 360f) * Math.PI * 2);
            double y = Math.Sin((dir / 360f) * Math.PI * 2);


            //TODO - MAKE THIS LINE ONLY THE LENGTH UNTIL IT HITS THE WINDOW >>>

            l.X2 = x * 700 + l.X1;
            l.Y2 = y * 700 + l.Y1;
            l.StrokeThickness = 5;
            l.Stroke = Brushes.Silver;
            l.Visibility = System.Windows.Visibility.Hidden;
            grid.Children.Add(l);
            s.Line = l;
            
            ResetSeq(s);
            
            Sequences.Add(s);
            //Console.WriteLine(lamps.Count);
        }

        public void ResetSeq(SequenceEvent seq)
        {
            int timeon = seq.TimeOn;

            if (seq.Shooter != null)
            {
                seq.Shooter.BeginAnimation(Canvas.LeftProperty, null);
                seq.Shooter.BeginAnimation(Canvas.RightProperty, null);
                grid.Children.Remove(seq.Shooter);
            }

            Shooter sh = new Shooter();
            seq.Shooter = sh;
            sh.RenderTransform = new RotateTransform(seq.Angle-90);

            DoubleAnimation ax = new DoubleAnimation();
            DoubleAnimation ay = new DoubleAnimation();

            ax.From = seq.Line.X1;
            ax.To = seq.Line.X2;
            ay.From = seq.Line.Y1;
            ay.To = seq.Line.Y2;
            ax.RepeatBehavior = RepeatBehavior.Forever;
            ay.RepeatBehavior = RepeatBehavior.Forever;

            ax.Duration = TimeSpan.FromMilliseconds(timeon);
            ay.Duration = TimeSpan.FromMilliseconds(timeon);

            
            grid.Children.Insert(0, sh);
            sh.Color = seq.Color;

            if (seq.Timer!=null)
             seq.Timer.Stop();
            List<Lamp> lamps = new List<Lamp>();
            //see what it intersects...
            foreach (Lamp lm in Window.rigview.lights.Children)
            {
                if (CircleIntersection(seq.Line, lm))
                {
                    RgbHsl.HSL h1 = RgbHsl.RGB_to_HSL(lm.Color);
                    RgbHsl.HSL h2 = RgbHsl.RGB_to_HSL(seq.Color);

                    //Console.WriteLine("C:" + Math.Abs(h1.H - h2.H));
                    if (Math.Abs(h1.H - h2.H) < 0.15 && seq.Color != Colors.Black && seq.Color != Colors.White)
                    {
                        lamps.Add(lm);
                    }
                }
            }

            seq.Lamps = lamps;
            
            int val = 0;
            int time = 0;

            System.Timers.Timer t = new Timer();
            DispatcherTimer tim = new DispatcherTimer();
            tim.Interval = TimeSpan.FromMilliseconds(timeon);
            tim.Tick+=new EventHandler((o,e)=>{
                t.Start();
                //Console.WriteLine("NOW");
            });

            DispatcherTimer offset = new DispatcherTimer();
            offset.Interval = TimeSpan.FromMilliseconds(1000);
            offset.Tick+=new EventHandler((o,e)=>{
                tim.Start();
                offset.Stop();
});
            offset.Start();



            sh.BeginAnimation(Canvas.LeftProperty, ax);
            sh.BeginAnimation(Canvas.TopProperty, ay);
            
                t.Interval = 10;
                t.Elapsed += new ElapsedEventHandler((ox, ex) =>
                {
                    time++;
                    if (time < 70)
                    {
                        val = Math.Min(val + 3, 255);//fade up
                    }

                    if (time > 70 && time < 140)
                    {
                        val = Math.Max(0, val - 3);//fade down
                    }

                    foreach (Lamp l in lamps)
                    {
                        Window.dmxcontroller.UpdateValue(l.Channel, val);

                        Dispatcher.BeginInvoke(new PaintSelector.ActionI((lm,o) =>
                        {
                            //HACK
                            //lm.Level = o;
                        }),new object[] {l,val/255.0});
                    }
                    //Console.WriteLine("on: "+val);

                    //if (time > timeon / 10)
                    if (time > 140)
                    {
                        time = 0;
                        t.Stop();
                    }
                });
                seq.Timer = t;
                t.Start();
               seq.DTimer = tim;
               seq.offsetimer = offset;
        }

        private bool CircleIntersection(Line l,Lamp lm)
        {
           
            //circle center point..
            Point cp = new Point(Canvas.GetLeft(lm)+ lm.ActualWidth/2,Canvas.GetTop(lm)+ lm.ActualHeight/2);
            //d = end - start
            //f = start-center

            //THIS IS AN IMPORTANT VALUE!!!
            double r = lm.ActualWidth / 4;

            //Ellipse e = new Ellipse();
            //e.Stroke = Brushes.LightSeaGreen;
            //e.StrokeThickness = 3;

            //e.Width = r*2;
            //e.Height = r*2;
            //Canvas.SetLeft(e, cp.X - r);
            //Canvas.SetTop(e, cp.Y - r);
            //Window.rigview.grid.Children.Add(e);
           

            //line is relative to the sequencer, 

            Point linestart = TranslatePoint(new Point(l.X1, l.Y1), Window.rigview.grid);
            Point lineend = TranslatePoint(new Point(l.X2, l.Y2), Window.rigview.grid);

            //Line linenew = new Line();
            //linenew.X1 = linestart.X;
            //linenew.X2 = lineend.X;
            //linenew.Y1 = linestart.Y;
            //linenew.Y2 = lineend.Y;
            //linenew.Stroke = Brushes.GreenYellow;
            //linenew.StrokeThickness = 3;
            //Window.rigview.grid.Children.Add(linenew);

            Point d = new Point(lineend.X - linestart.X, lineend.Y - linestart.Y);
            Point f = new Point(linestart.X - cp.X, linestart.Y - cp.Y);
            

            //d = this.TranslatePoint(d, Window.rigview.grid);
            //d = this.TranslatePoint(d, Window.rigview.grid);

            double a = d.Dot(d);
            double b = 2 * f.Dot(d);
            double c = f.Dot(f) - r * r;

            double discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
            {
                // no intersection
                return false;
            }
            else
            {
                // ray didn't totally miss sphere,
                // so there is a solution to
                // the equation.


                discriminant = Math.Sqrt(discriminant);
                double t1 = (-b + discriminant) / (2 * a);
                double t2 = (-b - discriminant) / (2 * a);

                if (t1 >= 0 && t1 <= 1)
                {
                    // solution on is ON THE RAY.
                    return true;
                }
                else
                {
                    // solution "out of range" of ray
                    return false;
                }

                // use t2 for second point
            }

        }

        public static GeometryDrawing GetArc(Rect rect, Angle start, Angle sweep)
        {
            GeometryDrawing ret = new GeometryDrawing();
            StreamGeometry geo = new StreamGeometry();

            //Set correct parameters
            SweepDirection sweepDir = sweep.Degrees < 0 ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;
            bool isLargeArc = Math.Abs(sweep.Degrees) > 180;

            double cx = rect.Width / 2;
            double cy = rect.Height / 2;
            //Calculate start point
            double x1 = rect.X + cx + (Math.Cos(start.Radians) * cx);
            double y1 = rect.Y + cy + (Math.Sin(start.Radians) * cy);
            //Calculate end point
            double x2 = rect.X + cx + (Math.Cos(start.Radians + sweep.Radians) * cx);
            double y2 = rect.Y + cy + (Math.Sin(start.Radians + sweep.Radians) * cy);

            using (StreamGeometryContext ctx = geo.Open())
            {
                ctx.BeginFigure(new Point(x1, y1), false, false);
                ctx.ArcTo(new Point(x2, y2), new Size(cx, cy), 0, isLargeArc, sweepDir, true, false);
            }

            ret.Geometry = geo;
            return ret;
        }

        GeometryDrawing arc = new GeometryDrawing();
        Rect r = new Rect(0, 0, 130, 130);
        Image i = new Image();
        DrawingImage img = new DrawingImage();

        private void DrawArc(double start, double val,Color c)
        {
                if (val < 90)
                {
                    Canvas.SetBottom(i, (65 - (65 * Math.Sin((val / 360) * Math.PI * 2))));
                    Canvas.SetRight(i, (65 - (65 * Math.Cos((val / 360) * Math.PI * 2))));
                }
                else
                {
                    Canvas.SetBottom(i, 0);
                    Canvas.SetRight(i, 0);
                }

                DrawRect.Angle a = new Angle(20);
                DrawRect.Angle b = new Angle((int)val);
                GeometryDrawing arc = GetArc(r, a, b);
                arc.Pen = new Pen(new SolidColorBrush(c), 20);
                //arc.Brush = Brushes.GreenYellow;         
                Canvas.SetRight(i, 0);
                img.Drawing = arc;

                //RotateTransform rt = new RotateTransform(start);
                //i.Width = 130;
                //i.Height = 130;
                //i.RenderTransformOrigin = new Point(0.5,0.5);
                //i.RenderTransform = rt;
        }

        public class SequenceEvent
        {
            public int TimeOn { get; set; }
            public int TimeOff { get; set; }
            public Color Color { get; set; }
            public int Angle { get; set; }
            public UIElement Element { get; set; }
            public Timer Timer { get; set; }
            public DispatcherTimer DTimer { get; set; }
            public UIElement Shooter { get; set; }
            public List<Lamp> Lamps { get; set; }
            public Line Line { get; set; }

            public DispatcherTimer offsetimer { get; set; }
        }

        private void SurfaceButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        //each sequence needs a dispatcher timer, which fires a visual representation of the thing, it also does the fade up / down...
    }
}
