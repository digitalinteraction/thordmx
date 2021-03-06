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

namespace ColorScanner
{
    public partial class Program : Gadgeteer.Program
    {
        // GTM.Module definitions
        Gadgeteer.Modules.GHIElectronics.UsbClientSP usbClientSP;
        Gadgeteer.Modules.GHIElectronics.Ethernet_J11D ethernet_J11D;
        Gadgeteer.Modules.GHIElectronics.Camera camera;
        Gadgeteer.Modules.GHIElectronics.Button button1;

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
		
            camera = new GTM.GHIElectronics.Camera(3);
		
            button1 = new GTM.GHIElectronics.Button(5);
		
            ethernet_J11D = new GTM.GHIElectronics.Ethernet_J11D(7);

        }
    }
}
