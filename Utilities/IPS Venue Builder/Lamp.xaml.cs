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
using IPS.SharedObjects;

namespace IPS.VenueBuilder
{
    /// <summary>
    /// Interaction logic for Lamp.xaml
    /// </summary>
    public partial class Lamp : UserControl
    {
        Light l;
        MainWindow form;

        public Lamp()
        {
            InitializeComponent();
        }

        public Lamp(Light l, MainWindow form)
        {
            InitializeComponent();
            this.l = l;
            this.form = form;
            try
            {
                ellipse.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(l.Color));
            }
            catch (Exception e)
            {
                //do nothing...
            }
            if (l.Channel == 0)
                l.Channel = 1;

            label1.Content = "" + l.Channel;
        }

        //private void Lamp_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Left)
        //    {
        //        if (form.pictureBox1.PointToClient(this.PointToScreen(new Point(e.X, e.Y))).X - this.Width / 2 < form.pictureBox1.Width - this.Width && form.pictureBox1.PointToClient(this.PointToScreen(new Point(e.X, e.Y))).X - this.Width / 2 > 0)
        //        {
        //            this.Left = form.pictureBox1.PointToClient(this.PointToScreen(new Point(e.X, e.Y))).X - this.Width / 2;
        //        }

        //        if (form.pictureBox1.PointToClient(this.PointToScreen(new Point(e.X, e.Y))).Y - this.Height / 2 < form.pictureBox1.Height - this.Height && form.pictureBox1.PointToClient(this.PointToScreen(new Point(e.X, e.Y))).Y - this.Height / 2 > 0)
        //        {
        //            this.Top = form.pictureBox1.PointToClient(this.PointToScreen(new Point(e.X, e.Y))).Y - this.Height / 2;
        //        }


        //        l.Position = form.GetRelativePosition(this);
        //        //Debug.Print(""+l.Position);
        //        form.Invalidate();
        //    }
        //}

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            form.SetLamp(l);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            //Lamp_MouseMove(sender, e);
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            //Lamp_MouseMove(sender, e);
        }

        private void UserControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            form.SetLamp(l);
        }
    }
}
