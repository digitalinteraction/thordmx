using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace IPS.Communication
{
    public delegate void DmxEventHandler(string osc_device, string osc_type, object[] variables);
    public interface IEventClient
    {
        void Connect();
        void Disconnect();
        void RegisterHandler(DmxEventHandler dothis, string osc_type, string osc_name, string osc_device);
        void UnregisterHandler(string osc_type, string osc_name, string osc_device);
        void UnregisterHandler(string osc_device);
        string Name { get; }
    }
}
