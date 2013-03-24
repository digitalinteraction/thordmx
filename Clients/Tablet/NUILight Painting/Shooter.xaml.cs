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

namespace IPS.TabletPainting
{
    /// <summary>
    /// Interaction logic for Shooter.xaml
    /// </summary>
    public partial class Shooter : UserControl
    {
        public Shooter()
        {
            InitializeComponent();
        }

        public Color Color { set { this.filler.Fill = new SolidColorBrush(value); } } 
    }
}
