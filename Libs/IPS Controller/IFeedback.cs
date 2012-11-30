using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPS.Controller
{
    public interface IFeedback
    {
        event Action<byte[]> OnUpdate;
    }
}
