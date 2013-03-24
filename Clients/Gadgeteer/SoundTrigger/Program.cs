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

namespace SoundTrigger
{
    public partial class Program
    {
        GTM.DigitalInteractionGroup.LightingControl dmx = new GTM.DigitalInteractionGroup.LightingControl();

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            GTM.GHIElectronics.Ethernet_ENC28 ethernet = new GTM.GHIElectronics.Ethernet_ENC28(3);
            ethernet.Interface.Open();
            NetworkInterfaceExtension.AssignNetworkingStackTo(ethernet.Interface);
            ethernet.Interface.NetworkInterface.EnableDhcp();
            ethernet.Interface.NetworkAddressChanged += new GHI.Premium.Net.NetworkInterfaceExtension.NetworkAddressChangedEventHandler(Interface_NetworkAddressChanged);
            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
            music.musicFinished += new Music.MusicFinishedPlayingEventHandler(music_musicFinished);
        }

        void music_musicFinished(Music sender)
        {
            playing = false;
        }

        void Interface_NetworkAddressChanged(object sender, EventArgs e)
        {
            dmx.Connect("192.168.1.110");
            dmx.OnUpdate += new GTM.DigitalInteractionGroup.LightingControl.LightingEventHandler(dmx_OnUpdate);
        }

        int last = 0;
        bool playing = false;

        void dmx_OnUpdate(GTM.DigitalInteractionGroup.LightingControl sender, int[] channels)
        {
            if (last != channels[16])
            {
                last = channels[16];
                if (!playing)
                {
                    music.SetVolume((byte)channels[16]);//sets volume
                    music.Play(Resources.GetBytes(Resources.BinaryResources.castle_h_Roger_x_7638_hifi));//plays track
                }
            }
        }
    }
}
