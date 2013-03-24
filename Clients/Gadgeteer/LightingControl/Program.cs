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

namespace LightingControl
{
    public partial class Program
    {
        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
            GTM.DigitalInteractionGroup.LightingControl lighting = new GTM.DigitalInteractionGroup.LightingControl();

            lighting.Connect("192.168.0.1");
            lighting.Blackout();
            lighting.UpdateChannel(1, 512);

            var rgb = lighting.RegisterRgbLamp(1);
            rgb.SetColor(GT.Color.Purple);

            lighting.FadeDown(1);

        }
    }
}
