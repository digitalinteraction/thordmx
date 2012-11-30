using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace IPS.Communication
{
    public interface IServerService
    {
        Dictionary<int, string> Services {get;}
    }
}
