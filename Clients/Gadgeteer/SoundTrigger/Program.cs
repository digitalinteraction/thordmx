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
            ethernet.Interface.Open();
            NetworkInterfaceExtension.AssignNetworkingStackTo(ethernet.Interface);
            ethernet.Interface.NetworkInterface.EnableDhcp();
            ethernet.Interface.NetworkAddressChanged += new GHI.Premium.Net.NetworkInterfaceExtension.NetworkAddressChangedEventHandler(Interface_NetworkAddressChanged);
            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
            //music.StopPlaying();
            music.SetVolume(255);
            music.Play(Resources.GetBytes(Resources.BinaryResources.police_s_Wiretrip_7787_hifi));//plays track
            //music.SineTest();
            music.musicFinished += new Music.MusicFinishedPlayingEventHandler(music_musicFinished);
        }

        void music_musicFinished(Music sender)
        {
            playing = false;
        }

        void Interface_NetworkAddressChanged(object sender, EventArgs e)
        {
            music.Play(Resources.GetBytes(Resources.BinaryResources._27881_stickinthemud_bike_horn_2));//plays track
            playing = true;
            dmx.Connect("192.168.1.100",true);
            dmx.OnUpdate += new GTM.DigitalInteractionGroup.LightingControl.LightingEventHandler(dmx_OnUpdate);
        }

        int last = 0;
        bool playing = false;

        void dmx_OnUpdate(GTM.DigitalInteractionGroup.LightingControl sender, int[] channels)
        {
            //Debug.Print("16:" + channels[16]);
            if (last != channels[16])
            {
                last = channels[16];
                if (!playing)
                {
                    playing = true;
                    music.SetVolume((byte)channels[16]);//sets volume
                    music.Play(Resources.GetBytes(Resources.BinaryResources._27881_stickinthemud_bike_horn_2));//plays track
                }
            }
        }
    }
}
