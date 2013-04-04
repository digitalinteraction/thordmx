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

namespace SoundTrigger
{
    public partial class Program : Gadgeteer.Program
    {
        // GTM.Module definitions
        Gadgeteer.Modules.GHIElectronics.Music music;
        Gadgeteer.Modules.GHIElectronics.Ethernet_J11D ethernet;
        Gadgeteer.Modules.GHIElectronics.UsbClientSP usbClientSP;

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
		
            ethernet = new GTM.GHIElectronics.Ethernet_J11D(7);
		
            music = new GTM.GHIElectronics.Music(9);

        }
    }
}
