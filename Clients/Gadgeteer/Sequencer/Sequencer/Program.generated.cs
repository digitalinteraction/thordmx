﻿
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Gadgeteer Designer.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Gadgeteer;
using GTM = Gadgeteer.Modules;

namespace Sequencer
{
    public partial class Program : Gadgeteer.Program
    {
        // GTM.Module definitions
        Gadgeteer.Modules.GHIElectronics.Ethernet_J11D ethernet_J11D;
        Gadgeteer.Modules.GHIElectronics.UsbClientSP usbClientSP;
        Gadgeteer.Modules.GHIElectronics.Button button;
        Gadgeteer.Modules.GHIElectronics.Button button1;
        Gadgeteer.Modules.GHIElectronics.Button button2;
        Gadgeteer.Modules.GHIElectronics.Button button3;
        Gadgeteer.Modules.GHIElectronics.MulticolorLed multicolorLed1;
        Gadgeteer.Modules.GHIElectronics.MulticolorLed multicolorLed2;
        Gadgeteer.Modules.GHIElectronics.MulticolorLed multicolorLed3;
        Gadgeteer.Modules.GHIElectronics.MulticolorLed multicolorLed;
        Gadgeteer.Modules.GHIElectronics.Potentiometer potentiometer;

        public static void Main()
        {
            //Important to initialize the Mainboard first
            Mainboard = new GHIElectronics.Gadgeteer.FEZSpider();			

            Program program = new Program();
            program.InitializeModules();
            program.ProgramStarted();
            program.Run(); // Starts Dispatcher
        }

        private void InitializeModules()
        {   
            // Initialize GTM.Modules and event handlers here.		
            usbClientSP = new GTM.GHIElectronics.UsbClientSP(1);
		
            ethernet_J11D = new GTM.GHIElectronics.Ethernet_J11D(7);
		
            multicolorLed = new GTM.GHIElectronics.MulticolorLed(8);
		
            multicolorLed1 = new GTM.GHIElectronics.MulticolorLed(multicolorLed.DaisyLinkSocketNumber);
		
            multicolorLed2 = new GTM.GHIElectronics.MulticolorLed(multicolorLed1.DaisyLinkSocketNumber);
		
            multicolorLed3 = new GTM.GHIElectronics.MulticolorLed(multicolorLed2.DaisyLinkSocketNumber);
		
            potentiometer = new GTM.GHIElectronics.Potentiometer(9);
		
            button3 = new GTM.GHIElectronics.Button(10);
		
            button2 = new GTM.GHIElectronics.Button(11);
		
            button1 = new GTM.GHIElectronics.Button(12);
		
            button = new GTM.GHIElectronics.Button(14);

        }
    }
}
