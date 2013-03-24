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
using Gadgeteer.Modules.GHIElectronics;
using GHI.Premium.Net;

namespace ColorScanner
{
    public partial class Program
    {
        GTM.DigitalInteractionGroup.LightingControl dmx = new GTM.DigitalInteractionGroup.LightingControl();
        GTM.DigitalInteractionGroup.LightingControl.RgbLamp lamp;

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            //ethernet_J11D.Interface.Open();
            //NetworkInterfaceExtension.AssignNetworkingStackTo(ethernet_J11D.Interface);
            //ethernet_J11D.Interface.NetworkInterface.EnableDhcp();
            ethernet_J11D.Interface.Open();
            NetworkInterfaceExtension.AssignNetworkingStackTo(ethernet_J11D.Interface);
            ethernet_J11D.Interface.NetworkInterface.EnableDhcp();
            ethernet_J11D.Interface.NetworkAddressChanged += new GHI.Premium.Net.NetworkInterfaceExtension.NetworkAddressChangedEventHandler(Interface_NetworkAddressChanged);

            button1.ButtonPressed += new Button.ButtonEventHandler(button1_ButtonPressed);
            camera.PictureCaptured += new Camera.PictureCapturedEventHandler(camera_PictureCaptured);
            camera.CurrentPictureResolution = Camera.PictureResolution.Resolution160x120;
            Debug.Print("Program Started");
        }

        void camera_PictureCaptured(Camera sender, GT.Picture picture)
        {
            
            //average of color:
            Bitmap bmp = picture.MakeBitmap();
            int r=0, g=0, b=0;

            for (int i = 60; i < 100; i++)
            {
                for (int j = 40; j < 80; j++)
                {
                    r += (int)ColorUtility.GetRValue(bmp.GetPixel(i, j));
                    g += (int)ColorUtility.GetGValue(bmp.GetPixel(i, j));
                    b += (int)ColorUtility.GetBValue(bmp.GetPixel(i, j));
                }
            }
            int total = 40 * 40;
            r = r / total;
            g = g / total;
            b = b / total;
            button1.TurnLEDOff();
            lamp.SetColor(GT.Color.FromRGB((byte)r, (byte)g, (byte)b));
        }

        void button1_ButtonPressed(Button sender, Button.ButtonState state)
        {
            camera.TakePicture();
            button1.TurnLEDOn();
            dmx.FadeDown(lamp.Channel,1);
            dmx.FadeDown(lamp.Channel+1, 1);
            dmx.FadeDown(lamp.Channel+2, 1);
        }

        void Interface_NetworkAddressChanged(object sender, EventArgs e)
        {
            Debug.Print("Ethernet UPx");
            dmx.Connect("192.168.1.110");
            //dmx.Blackout();
            //dmx.UpdateAllChannels(new int[512]);
            //dmx.FadeUp(5);
            lamp = dmx.RegisterRgbLamp(1);
            //rgb.SetColor(GT.Color.Purple);
            //dmx.UpdateChannel(2, 255);
        }
    }
}
