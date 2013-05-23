using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPS.Communication
{
    public interface ILoggable
    {
        event Action<string> OnLogEvent;
        bool DebugMode { set; }
    }
}
