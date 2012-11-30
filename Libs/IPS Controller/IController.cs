using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPS.Controller
{
    public interface IController
    {
        bool Blackout { get; set; }
        string DeviceName { get; set; }
        byte[] LiveValues {get;set;}
        void DoBlackout();
        void UpdateValue(int chan, int val);
        void UpdateValues(int[] vals);
        void Start();
    }
}
