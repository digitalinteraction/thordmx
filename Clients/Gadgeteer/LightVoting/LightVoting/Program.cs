using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GHI.Premium.Net;
using Gadgeteer.Modules.GHIElectronics;

namespace LightVoting
{
    public partial class Program
    {
        GTM.DigitalInteractionGroup.LightingControl dmx = new GTM.DigitalInteractionGroup.LightingControl();
         
        void ProgramStarted()
        {
            
            ethernet_J11D.Interface.Open();
            NetworkInterfaceExtension.AssignNetworkingStackTo(ethernet_J11D.Interface);
            ethernet_J11D.Interface.NetworkInterface.EnableDhcp();
            ethernet_J11D.Interface.NetworkAddressChanged += new GHI.Premium.Net.NetworkInterfaceExtension.NetworkAddressChangedEventHandler(Interface_NetworkAddressChanged);
            Debug.Print("Program Started");
        }

        int votesfor = 0;
        int votesagainst = 0;

        void button_ButtonPressed(Button sender, Button.ButtonState state)
        {
            votesfor++;
            CalcLamps();
        }

        void button1_ButtonPressed(Button sender, Button.ButtonState state)
        {
            votesagainst++;
            CalcLamps();
        }

        void CalcLamps()
        {
            dmx.FadeUp(4,2);
            //dmx.FadeUp(5,2);
           // dmx.FadeUp(6,2);
           // dmx.FadeUp(7,2);
            dmx.FadeUp(8,2);
            //dmx.FadeUp(9,2);
            GT.Timer timer = new GT.Timer(3000);
            timer.Tick+=new GT.Timer.TickEventHandler((o)=>{
                o.Stop();
                int v1 = (int)((votesfor / (double)(votesfor + votesagainst)) * 255);
                int v2 = (int)((votesagainst / (double)(votesfor + votesagainst)) * 255);
                dmx.UpdateChannel(4, v2);
                dmx.UpdateChannel(8, v1);
                //dmx.FadeDown(5, 2);
                //dmx.FadeDown(6, 2);
                //dmx.FadeDown(7, 2);
                //dmx.FadeDown(9, 2);
            });
            timer.Start();
        }

        void FirstScreen()
        {
            display_T35.SimpleGraphics.DisplayImage(Resources.GetBitmap(Resources.BitmapResources.g3362), 40, 10);
            display_T35.SimpleGraphics.DisplayText("What would you call yourself?", Resources.GetFont(Resources.FontResources.chi), GT.Color.Orange, 3, 80);

            display_T35.SimpleGraphics.DisplayText("Artist", Resources.GetFont(Resources.FontResources.chi), GT.Color.Orange, 35, 130);

            display_T35.SimpleGraphics.DisplayText("Scientist", Resources.GetFont(Resources.FontResources.chi), GT.Color.Orange, 205, 130);

            display_T35.SimpleGraphics.DisplayImage(Resources.GetBitmap(Resources.BitmapResources.g3377), 40, 182);

            display_T35.SimpleGraphics.DisplayImage(Resources.GetBitmap(Resources.BitmapResources.g3377), 230, 182);
        }

        void Interface_NetworkAddressChanged(object sender, EventArgs e)
        {
            dmx.Connect("192.168.1.100");
            FirstScreen();
            button1.ButtonPressed += new Button.ButtonEventHandler(button1_ButtonPressed);
            button.ButtonPressed += new Button.ButtonEventHandler(button_ButtonPressed);
        }

        //display current score

        //on button press -- flash to register vote, then add to total and control color.
    }
}
