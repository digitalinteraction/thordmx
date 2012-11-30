using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using ZeroconfService;
using System.ComponentModel;
using System.Windows.Forms;

namespace IPS.Controller
{
    public class ServerFinder
    {
        public List<string> Servers {get;set;}

        public ServerFinder()
        {

            Label l = new Label();

            Servers = new List<string>();
            //Servers = new ObservableCollection<string> ();
            //BonjourServiceResolver bsr = new BonjourServiceResolver();
            //bsr.ServiceFound += new Network.ZeroConf.ObjectEvent<Network.ZeroConf.IService>(bsr_ServiceFound);
            //bsr.Resolve("_dmx._udp");
            NetServiceBrowser nsBrowser = new NetServiceBrowser();
            nsBrowser.AllowMultithreadedCallbacks = true;
            nsBrowser.AllowApplicationForms = true;

            //nsBrowser.InvokeableObject = l;
            nsBrowser.DidFindService += new NetServiceBrowser.ServiceFound(nsBrowser_DidFindService);
            nsBrowser.DidRemoveService += new NetServiceBrowser.ServiceRemoved(nsBrowser_DidRemoveService);
            nsBrowser.SearchForService("_dmx._udp", "");
        }

        void nsBrowser_DidRemoveService(NetServiceBrowser browser, NetService service, bool moreComing)
        {
            Servers.Remove(service.HostName);
        }

        void nsBrowser_DidFindService(NetServiceBrowser browser, NetService service, bool moreComing)
        {

            service.DidResolveService += new NetService.ServiceResolved(service_DidResolveService);
            service.ResolveWithTimeout(100);
        }

        public event Action<string> OnServerFound;

        void service_DidResolveService(NetService service)
        {
           Servers.Add(service.HostName);
           if (OnServerFound != null)
               OnServerFound(service.HostName);
        }
    }
}
