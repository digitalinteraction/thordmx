using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Timers;
using System.Diagnostics;
using System.Windows;
using System.IO;


namespace PaintBucket
{
    public class PaintCanvas : UserControl
    {
        private Rectangle rec = new Rectangle();
        private BitmapSource bitmap;
        private ImageBrush brush = new ImageBrush();
        private Canvas c;
        private string background;
        private string foregroundpath;
        private Rectangle foreground;
        private List<Seed> seeds = new List<Seed>();
        private List<Seed> availableseeds = new List<Seed>();
        private Seed lastActiveSeed;
        
        public struct Seed
        {
            public int posx;
            public int posy;
            public int radius;
            public float c;
            public float m;
            public float y;
        }

        public List<Seed> Seeds
        {
            get { return seeds; }
        }

        private Dictionary<int, Finger> fingers = new Dictionary<int, Finger>();

        private Layer wetlayer = new Layer(120, 120);
        //private Layer drylayer = new Layer(640, 480);


        private Timer t = new Timer();

        private byte[] rawImage;

        int width = 120;
        int height = 120;
        int rawStride;
        PixelFormat pf = PixelFormats.Bgra32;

        public void LoadSeeds()
        {
            //loads seeds relative to a file
            //seeds.Clear();
            //seeds.Add(s);                    
            //availableseeds.Add(s);
        }


        public void AddSeed(Seed s)
        {
            wetlayer.AddPaint(s.posx, s.posy, s.radius, s.c, s.m, s.y);
            UpdatePaint();
        }

        private delegate void MethodDelegate();

        
        public void ClearPaint()
        {
            wetlayer.Clear();
            UpdatePaint();
        }

        public void ResetPaint()
        {
            wetlayer.Clear();
            UpdatePaint();
        }

        public void changeBackground(string imagefile,bool tile,bool resetpaint)
        {
            this.background = imagefile;
            ImageBrush cv = new ImageBrush();
            if (tile)
            {
                cv.Stretch = Stretch.None;
                cv.TileMode = TileMode.Tile;
                cv.Viewport = new Rect(0, 0, 0.05, 0.05);
            }
            else
            {
                cv.Stretch = Stretch.UniformToFill;
            }

            cv.ImageSource = new BitmapImage(new Uri(imagefile, UriKind.Relative));
            c.Background = cv;

            if (resetpaint)
                wetlayer.Clear();
        }

        public void changeForeground(string imagefile)
        {
            ImageBrush cv = new ImageBrush();
            
            cv.Stretch = Stretch.UniformToFill;
            foregroundpath = imagefile;
            cv.ImageSource = new BitmapImage(new Uri(imagefile, UriKind.Relative));
            foreground.Fill = cv;            
        }

        public PaintCanvas()
        {
            //this.fingerDown += new fingerDownHandler(PaintCanvas_fingerDown);
            //this.fingerUp += new fingerUpHandler(PaintCanvas_fingerUp);
            //this.fingerUpdate += new fingerUpdateHandler(PaintCanvas_fingerUpdate);

            //t.Interval = 100;
            //t.Elapsed+=new ElapsedEventHandler(do_timeradd);
            //t.Start();

            // Define parameters used to create the BitmapSource.
            
            rawStride = (width * pf.BitsPerPixel + 7) / 8;

            rawImage = new byte[rawStride * height];

            // Initialize the image with data.
            Random value = new Random();
            value.NextBytes(rawImage);

            foreground = new Rectangle();
            
            c = new Canvas();

            this.SizeChanged += new SizeChangedEventHandler(PaintCanvas_SizeChanged);

            c.Children.Add(rec);
            c.Children.Add(foreground);            

            //this.Children.Add(c);
            this.AddChild(c);
            //rec.Margin = new Thickness(20);
            //wetlayer.AddPaint();
            UpdatePaint();
            //drying paint...
        }

        void PaintCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            rec.Width = this.ActualWidth;
            rec.Height = this.ActualHeight;
            foreground.Width = this.ActualWidth;
            foreground.Height = this.ActualHeight;
        }

        private void UpdatePaint()
        {
            //if (this.Parent!=null)
                //Debug.Print(((Window)this.Parent).Width+"");
            rawImage = wetlayer.Render();
            
            //if (bitmap == null)
                bitmap = BitmapSource.Create(width, height, 96, 96, pf, null, rawImage, rawStride);
           
            
            //else
            //    bitmap.CopyPixels(rawImage, rawStride, 0);
            
            //brush = new ImageBrush(bitmap);
            brush.ImageSource = bitmap;
           
            rec.Fill = brush;
            brush.Stretch = Stretch.Fill;
            //this.Dispatcher.Invoke(null, System.Windows.Threading.DispatcherPriority.Render, null);
        }

        private int sincelastupdate = 0;
        private int sincelastmove= 0;

        /*private void dotimer()
        {
            sincelastupdate += 100;
            sincelastmove += 100;
            status.updatetext.Content = sincelastupdate + " ms since update";
            status.statustext.Content = sincelastmove + " ms since move";
        }*/

        private delegate void TimerInvoker();

        /*void do_timeradd(object sender, ElapsedEventArgs e)
        {
            //pull into correct thread...
            if (App.Current.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new TimerInvoker(dotimer));
                return;
            }
            //wetlayer.DryToLayer(0.2);
            //UpdatePaint();

        }*/

        //TODO these are important
        public void PaintCanvas_fingerUpdate(object sender, TouchEventArgs e)
        {
            //redo the position for window resize:
            //Debug.Print(e.data.X+"");

            float fx = (float)(e.data.X * bitmap.Width / ActualWidth) - (e.data.width / 2);
            float fy = (float)(e.data.Y * bitmap.Height / ActualHeight) - (e.data.height / 2);

            bool newFinger = false;
            if (!fingers.ContainsKey(e.data.ID))
            {
                Finger f = new Finger();
                f.Data = e.data;
                f.Layer = new Layer((int)e.data.width, (int)e.data.height);
                fingers.Add(e.data.ID, f);
                fingers[e.data.ID].Data.X = fx;
                fingers[e.data.ID].Data.Y = fy;
                newFinger = true;
            }

            if (fingers.ContainsKey(e.data.ID))
            {
                //Debug.Print("finger update:" + e.data.ID);

                float x1 = fingers[e.data.ID].Data.X, y1 = fingers[e.data.ID].Data.Y;
                float x2 = fx, y2 = fy;
                int n;  
// Make based on length
//n = (int)(Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)) / 1.0f);
                n = 5;
                for (int i = 0; i < n; i++)
                {
                    if (!newFinger)
                    {
                        //pick up paint
                        wetlayer.TransferToLayer(fingers[e.data.ID].Layer, 0.98f, fingers[e.data.ID].Data.X, fingers[e.data.ID].Data.Y, false);
                    }

                    fingers[e.data.ID].Data = e.data;
                    fingers[e.data.ID].Data.X = x1 + (x2 - x1) * i / n;
                    fingers[e.data.ID].Data.Y = y1 + (y2 - y1) * i / n;

                    if (!newFinger)
                    {
                        //deposit paint
                        wetlayer.TransferToLayer(fingers[e.data.ID].Layer, 1, fingers[e.data.ID].Data.X, fingers[e.data.ID].Data.Y, true);
                    }

                    newFinger = false;
                }
               
                //UpdatePaint();
                this.InvalidateVisual();
            }
        }

        public void PaintCanvas_fingerUp(object sender, TouchEventArgs e)
        {
            if (fingers.ContainsKey(e.data.ID))
            {
                //transfer paint on the finger back to the table
                //wetlayer.TransferToLayer(fingers[e.data.ID].Layer, 1, e.data.X, e.data.Y, true);
                fingers.Remove(e.data.ID);
            }
            //UpdatePaint();
            this.InvalidateVisual();
        }

        public void PaintCanvas_fingerDown(object sender, TouchEventArgs e)
        {
            
            
            /*
            Finger f = new Finger();
            f.Data = e.data;
            f.Layer = new Layer((int)e.data.width,(int)e.data.height);
            if (!fingers.ContainsKey(e.data.ID))
                fingers.Add(e.data.ID,f);
            */
        }


        protected override void OnRender(DrawingContext dc)
        {
            UpdatePaint();
            //dc.DrawImage(bitmap,new Rect(Canvas.GetLeft(rec),Canvas.GetTop(rec),rec.ActualWidth,rec.ActualWidth));
            base.OnRender(dc);
        }

        public double getPaintedAmt()
        {
            return wetlayer.getPaintedAmt();
        }

        internal Color GetMixedColor()
        {

            float c = 0;
            float m = 0;
            float y = 0;
            float a = 0;

            //average c,m,y in an area
            for (int i = height / 2 - 10; i < width / 2 + 10; i++)
            {
                for (int j = height / 2 - 10; j < width / 2 + 10; j++)
                {
                    float[] temp = wetlayer.GetColorAt(j, i);                    
                    c += temp[0];
                    m += temp[1];
                    y += temp[2];
                    a += temp[3];
                }
            }

            c = c / (20*20);
            m = m / (20*20);
            y = y / (20*20);
            a = a / (20*20);

            //byte[] vals = new byte[] {(byte)((1-c)*255),(byte)((1-m)*255),(byte)((1-y)*255)};

            //return Colors.Green;

            return Color.FromArgb(255,(byte)c,(byte)m,(byte)y);
            //            R = ( 1 - C ) * 255
            //G = ( 1 - M ) * 255
            //B = ( 1 - Y ) * 255
        }
    }
}
