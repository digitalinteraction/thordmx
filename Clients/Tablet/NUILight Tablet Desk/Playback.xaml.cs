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

namespace IPS.TabletDesk
{
    /// <summary>
    /// Interaction logic for Playback.xaml
    /// </summary>
    public partial class Playback : UserControl
    {
        public MainWindow window;

        public Playback()
        {
            InitializeComponent();
        }

        public ProgressBar CueProgress
        {
            get { return progressBar1; }
        }

        public void GO()
        {
            surfaceButton1_Click(null, null);
        }

        private void surfaceButton1_Click(object sender, RoutedEventArgs e)
        {
            window.NextCue();

            if (window.NextCueToFire != null)
            {
                incoming.Content = window.NextCueToFireNum + 1 + " " + window.NextCueToFire.name;
                outgoing.Content = window.CurrentCueOnFireNum + 1 + " " + window.CurrentCueOnFire.name;
            }
        }

        private void surfaceButton4_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void surfaceButton2_Click(object sender, RoutedEventArgs e)
        {
            window.dmxcontroller.DoBlackout();
            if (window.dmxcontroller.Blackout)
                surfaceButton2.Background = Brushes.Orange;
            else
                surfaceButton2.Background = Brushes.Silver;
        }

        private void chkLockLive_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
        	window.LockLiveOutput = false;
        }

        private void chkLockLive_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
        	window.LockLiveOutput = true;
        }

        private void surfaceButton3_Click(object sender, RoutedEventArgs e)
        {
            window.Save();
        }

        private void surfaceButton5_Click(object sender, RoutedEventArgs e)
        {
            window.LoadCurrentCue();
            if (window.NextCueToFire != null)
                incoming.Content = window.NextCueToFireNum+1 + " " +window.NextCueToFire.name;
            if (window.CurrentCueOnFire!=null)
                outgoing.Content = window.CurrentCueOnFireNum+1 + " " +window.CurrentCueOnFire.name;
        }

        private void chkLockLive1_TapGesture(object sender, TouchEventArgs e)
        {
            chkLockLive1.IsChecked = !chkLockLive1.IsChecked;
        }
    }
}
