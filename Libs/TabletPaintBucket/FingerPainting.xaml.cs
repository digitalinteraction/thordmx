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
using System.Windows.Media.Animation;
using System.Timers;

namespace PaintBucket
{
    /// <summary>
    /// Interaction logic for FingerPainting.xaml
    /// </summary>
    public partial class FingerPainting : UserControl
    {
        List<PaintCanvas> pages = new List<PaintCanvas>();

        private int currentpage = 0;
        public void changePage(int pagenum)
        {
            //drop to bottom the last page
            Canvas.SetZIndex(pages[currentpage], -1);
            //pages[currentpage].BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1, 0, new Duration(new TimeSpan(0, 0, 1))));

            //wrap...
            if (pagenum > (pages.Count - 1))
            {
                pagenum = 0;
            }

            if (pagenum < 0)
            {
                pagenum = pages.Count - 1;
            }

            currentpage = pagenum;
            //pages[currentpage].BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, 1, new Duration(new TimeSpan(0, 0, 1))));

            //bring to the top the next page
            Canvas.SetZIndex(pages[currentpage], 1);
            //w_SizeChanged(null, null);
        }

        public void AddSeed(PaintBucket.PaintCanvas.Seed s)
        {
            pages[currentpage].AddSeed(s);
        }

        private bool random = false;
        private Timer randomtimer = new Timer();
        public bool Random
        {
            get { return random; }
            set
            {
                //if its random, start timer
                if (value)
                {
                    randomtimer.Start();
                }
                else
                {
                    randomtimer.Stop();
                }
                random = value;
            }
        }

        public void ResetPaint()
        {
            if (random)
            {
                pages[currentpage].ClearPaint();
            }
            else
            {
                pages[currentpage].ResetPaint();
            }
        }

        public PaintCanvas CurrentCanvas
        {
            get { return pages[currentpage]; }
        }

        public void NextPage()
        {
            changePage(currentpage + 1);
        }

        public void PreviousPage()
        {
            changePage(currentpage - 1);
        }

        public FingerPainting()
        {
            InitializeComponent();

            //this.Background = Brushes.Blue;

            //this.Width = 200;
            //this.Height = 200;
            //this.maincanvas.Width = 200;
            //this.maincanvas.Height = 200;

            //FINGER PAINTING
            //for (int i = 0; i < 4; i++)
            //{
            //    PaintCanvas c = new PaintCanvas();
            //    pages.Add(c);
            //    maincanvas.Children.Add(c);
            //    Canvas.SetZIndex(c, -1);
            //    c.changeBackground(@"canvas.jpg", true, false);
            //    if (i == 0)
            //    {
            //        c.changeForeground(@"sailboat.png");
            //    }
            //    else if (i == 1)
            //    {
            //        //c.changeForeground(@"mountain.png");
            //        c.changeForeground(@"kite.png");
            //    }
            //    else if (i == 2)
            //    {
            //        //c.changeForeground(@"mountain.png");
            //        c.changeForeground(@"grid_image.png");
            //    }
            //    else if (i == 3)
            //    {
            //        c.changeForeground(@"mountain.png");
            //    }
            //    //c.changeForeground(@"foreground.png");
            //    this.SizeChanged += new SizeChangedEventHandler(FingerPainting_SizeChanged);
            //}           

            PaintCanvas c = new PaintCanvas();
            pages.Add(c);
            maincanvas.Children.Add(c);
            this.SizeChanged += new SizeChangedEventHandler(FingerPainting_SizeChanged);

            // assign a randome image to start with
            Random random = new Random();
            changePage(0);
        }



        void FingerPainting_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.maincanvas.Width = this.ActualWidth;
            this.maincanvas.Height = this.ActualHeight;
            pages[currentpage].Width = this.ActualWidth;
            pages[currentpage].Height = this.ActualHeight;
        }

        public new void MouseDown(TouchEventArgs arg)
        {
            pages[currentpage].PaintCanvas_fingerDown(this, arg);
        }

        public new void MouseUp(TouchEventArgs arg)
        {
            pages[currentpage].PaintCanvas_fingerUp(this, arg);
        }

        public new void MouseMove(TouchEventArgs arg)
        {
            pages[currentpage].PaintCanvas_fingerUpdate(this, arg);
        }

        internal Color GetMixedColor()
        {
            return pages[currentpage].GetMixedColor();
        }
    }
}
