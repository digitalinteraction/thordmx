using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IPS.Server
{
    public partial class Level : UserControl
    {
        public Level()
        {
            InitializeComponent();
        }

        public int Channel { get; set; }

        private int val;
        public int Value { 
            get { return val; } 
            set { 
                val = Math.Min(255,value); 
                Invalidate();
            } 
        }

        Font font = new Font(FontFamily.GenericSansSerif, 8);
        Font fontsmall = new Font(FontFamily.GenericSansSerif, 7);

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.FillRectangle(Brushes.GreenYellow, 1, this.Height - ((this.Height-13) * (val / 255.0f)), this.Width-2,(this.Height-13) * (val / 255.0f));
            e.Graphics.DrawString(val + "", fontsmall, Brushes.Silver, 2, 24);
            e.Graphics.DrawString(Channel+"", font, Brushes.Black, 1, 0);
            e.Graphics.DrawLine(Pens.Silver, new Point(0, this.Height-1), new Point(this.Width, this.Height-1));
            e.Graphics.DrawLine(Pens.Silver, new Point(0, 13), new Point(this.Width, 13));
        }
    }
}
