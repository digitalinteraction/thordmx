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
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;

namespace PaintBucket
{
    /// <summary>
    /// Interaction logic for PaintBucketControl.xaml
    /// </summary>
    public partial class PaintBucketControl : SurfaceUserControl
    {
        public static float ConC(float c, float k)
        {
            return ((c * (1 - k)) + k);
        }

        public static float ConM(float m, float k)
        {
            return ((m * (1 - k)) + k);
        }
        public static float ConY(float y, float k)
        {
            return ((y * (1 - k)) + k);
        }

        //C = 1 - ( R / 255 )
        //M = 1 - ( G / 255 )
        //Y = 1 - ( B / 255 )

        public PaintBucketControl()
        {
            InitializeComponent();

            PaintBucket.PaintCanvas.Seed s = new PaintCanvas.Seed();
            s.c = 1f - (219f / 255f);
            s.m = 1f - (0f / 255f);
            s.y = 1f - (219f / 255f);

            s.radius = 100;
            s.posx = 100;
            s.posy = 400;
            finger.AddSeed(s);

            PaintBucket.PaintCanvas.Seed s1 = new PaintCanvas.Seed();
            s1.c = 1f - (255 / 255f);
            s1.m = 1f - (200f / 255f);
            s1.y = 1f - (0f / 255f);

            s1.posx = 100;
            s1.posy = 100;
            s1.radius = 100;

            finger.AddSeed(s1);

            PaintBucket.PaintCanvas.Seed s2 = new PaintCanvas.Seed();
            s2.c = 1f - (0f / 255f);
            s2.m = 1f - (255f / 255f);
            s2.y = 1f - (217f / 255f);

            s2.posx = 400;
            s2.posy = 400;
            s2.radius = 100;
            finger.AddSeed(s2);

            PaintBucket.PaintCanvas.Seed s3 = new PaintCanvas.Seed();
            s3.c = 1f - (255 / 255f);
            s3.m = 1f - (255 / 255f);
            s3.y = 1f - (255 / 255f);

            s3.posx = 400;
            s3.posy = 100;
            s3.radius = 100;
            finger.AddSeed(s3);
        }

        public Color GetMixedColor()
        {

            return finger.GetMixedColor();
        }

        private void FingerPainting_ContactDown(object sender, ContactEventArgs e)
        {
            finger.MouseDown(new TouchEventArgs(new TouchData(null, e.Contact.Id, e.Contact.Id, (float)e.GetPosition(this).X, (float)e.GetPosition(this).Y, 0, 0, 100, 100, 0, 0)));
            e.Handled = true;
            //finger.MouseDown(new TouchEventArgs());
        }

        private void FingerPainting_ContactChanged(object sender, ContactEventArgs e)
        {
            finger.MouseMove(new TouchEventArgs(new TouchData(null, e.Contact.Id, e.Contact.Id, (float)e.GetPosition(this).X, (float)e.GetPosition(this).Y, 0, 0, 100, 100, 0, 0)));
            //finger.MouseMove(new TouchEventArgs());
            e.Handled = true;
        }

        private void FingerPainting_ContactUp(object sender, ContactEventArgs e)
        {
            
            finger.MouseUp(new TouchEventArgs(new TouchData(null, e.Contact.Id, e.Contact.Id, (float)e.GetPosition(this).X, (float)e.GetPosition(this).Y, 0, 0, 100, 100, 0, 0)));
            e.Handled = true;
            //finger.MouseUp(new TouchEventArgs());
        }
    }
}
