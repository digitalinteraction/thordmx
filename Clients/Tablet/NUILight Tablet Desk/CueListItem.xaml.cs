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
using IPS.TabletDesk;

namespace IPS.TabletDesk
{
    /// <summary>
    /// Interaction logic for CueListItem.xaml
    /// </summary>
    public partial class CueListItem : SurfaceListBoxItem
    {
        public CueListItem()
        {
            InitializeComponent();
        }

        public Cue Cue
        {
            get;
            set;
        }

        public void Select(bool yes)
        {
            if (yes)
            {
                image.Visibility = System.Windows.Visibility.Visible;
                //image.Source = new BitmapImage(new Uri("equalizer.png", UriKind.Relative));
            }
        }

        public void Playing(bool yes)
        {
            if (yes)
            {
                Background = Brushes.Black;
                n.Foreground = Brushes.DarkKhaki;
                fade.Foreground = Brushes.DarkKhaki;
            }
        }

        public CueListItem(Cue c,int num)
        {
            InitializeComponent();
            this.n.Content = num + " " + c.name;
            fade.Content = ""+Math.Round(c.fadeup,1);
            Cue = c;
        }
    }
}
