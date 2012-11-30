using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using IPS.Controller;

namespace IPS.Communication.Plugins
{
    public class UDPBroadcastFeedback:IFeedback
    {
        public UDPBroadcastFeedback()
        {
            Thread t = new Thread(new ThreadStart(() =>
            {
                Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint iep = new IPEndPoint(IPAddress.Any, 8888);
                sock.Bind(iep);
                EndPoint ep = (EndPoint)iep;
                try
                {
                    while (true)
                    {
                        byte[] data = new byte[513];
                        int length = sock.ReceiveFrom(data, ref ep);
                        //LiveValues = data;
                        if (OnUpdate != null)
                        {
                            OnUpdate(data);
                        }
                    }
                }
                finally
                {
                    sock.Close();
                }
            }));
            t.Priority = ThreadPriority.Lowest;
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        public event Action<byte[]> OnUpdate;
    }
}
