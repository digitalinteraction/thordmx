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

namespace Sequencer
{
    public partial class Program
    {

        ArrayList buttons = new ArrayList();
        ArrayList lights = new ArrayList();
        int[] colors = new int[4];

        int speed = 5;
        GTM.DigitalInteractionGroup.LightingControl dmx = new GTM.DigitalInteractionGroup.LightingControl();
        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            ethernet_J11D.Interface.Open();
            NetworkInterfaceExtension.AssignNetworkingStackTo(ethernet_J11D.Interface);
            ethernet_J11D.Interface.NetworkInterface.EnableDhcp();
            ethernet_J11D.Interface.NetworkAddressChanged += new GHI.Premium.Net.NetworkInterfaceExtension.NetworkAddressChangedEventHandler(Interface_NetworkAddressChanged);

            Debug.Print("Program Started");

            buttons.Add(button);
            buttons.Add(button1);
            buttons.Add(button2);
            buttons.Add(button3);

            foreach (Button b in buttons)
            {
                b.ButtonPressed+=new Button.ButtonEventHandler(b_ButtonPressed);
            }

            for (int i = 0; i < 4; i++)
            {
                lights.Add(dmx.RegisterRgbLamp((i * 3) + 1));
            }

            multicolorLed.GreenBlueSwapped = true;
            multicolorLed1.GreenBlueSwapped = true;
            multicolorLed2.GreenBlueSwapped = true;
            multicolorLed3.GreenBlueSwapped = true;


            multicolorLed1.FadeRepeatedly(GT.Color.Red);
            multicolorLed2.FadeRepeatedly(GT.Color.Red);
            multicolorLed3.FadeRepeatedly(GT.Color.Red);
            multicolorLed.FadeRepeatedly(GT.Color.Red);
        }

        void SetColor(MulticolorLed led, int i)
        {
            colors[i]++;
            if (colors[i] > 8)
            {
                colors[i] = 1;
            }
            switch (colors[i])
            {
                case 1:
                    led.TurnColor(GT.Color.Red);
                    break;
                case 2:
                    led.TurnColor(GT.Color.Green);
                    break;
                case 3:
                    led.TurnColor(GT.Color.Blue);
                    break;
                case 4:
                    led.TurnColor(GT.Color.Orange);
                    break;
                case 5:
                    led.TurnColor(GT.Color.Purple);
                    break;
                case 6:
                    led.TurnColor(GT.Color.Cyan);
                    break;
                case 7:
                    led.TurnColor(GT.Color.White);
                    break;
                case 8:
                    led.TurnColor(GT.Color.Black);
                    break;
            }
        }

        GT.Color GetColor(int i)
        {
            switch (colors[i])
            {
                case 1:
                    return GT.Color.Red;
                case 2:
                    return GT.Color.FromRGB(0, 255, 0);
                case 3:
                    return GT.Color.Blue;
                case 4:
                    return GT.Color.Orange;
                case 5:
                    return GT.Color.Purple;
                case 6:
                    return GT.Color.Cyan;
                case 7:
                    return GT.Color.White;
                case 8:
                    return GT.Color.Black;
            }
            return Color.Black;
        }

        void b_ButtonPressed(Button sender, Button.ButtonState state)
        {
            if (sender == button)
            {
                SetColor(multicolorLed, 0);
            }
            if (sender == button1)
            {
                SetColor(multicolorLed1, 1);
            }
            if (sender == button2)
            {
                SetColor(multicolorLed2, 2);
            }
            if (sender == button3)
            {
                SetColor(multicolorLed3, 3);
            }
        }

        int currentlamp = 0;
        int intermediatecounter = 0;

        void timer_Tick(GT.Timer timer)
        {
            intermediatecounter++;
            if (intermediatecounter > speed)
            {
                intermediatecounter = 0;
                //next in the sequence...
                currentlamp++;
                if (currentlamp > 3)
                    currentlamp = 0;

                int i = 0;
                foreach (Button b in buttons)
                {
                    b.TurnLEDOff();
                    (lights[i] as Gadgeteer.Modules.DigitalInteractionGroup.LightingControl.RgbLamp).SetColor(GT.Color.Black);
                    i++;
                }

                (buttons[currentlamp] as Button).TurnLEDOn();
                //set dmx colors:
                (lights[currentlamp] as Gadgeteer.Modules.DigitalInteractionGroup.LightingControl.RgbLamp).SetColor(GetColor(currentlamp));
                
            }

            double pot = potentiometer.ReadPotentiometerPercentage();
            speed = (int)(5 + ((pot - 0.5) * 10));
        }

        void Interface_NetworkAddressChanged(object sender, EventArgs e)
        {
            dmx.Connect("192.168.1.102");
            multicolorLed1.TurnOff();
            multicolorLed2.TurnOff();
            multicolorLed3.TurnOff();
            multicolorLed.TurnOff();
            //start loop...
            GT.Timer timer = new GT.Timer(150);
            timer.Tick += new GT.Timer.TickEventHandler(timer_Tick);
            timer.Start();
        }
    }
}
