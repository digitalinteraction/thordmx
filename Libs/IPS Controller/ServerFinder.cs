using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using ZeroconfService;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;

namespace IPS.Controller
{
    public class ServerFinder
    {
        public List<string> Servers {get;set;}
        NetServiceBrowser nsBrowser;
        public ServerFinder()
        {
            try
            {
                nsBrowser = new NetServiceBrowser();
            }
            catch
            {
                MessageBox.Show("Problem starting ServerFinder - Bonjour is probably not installed");
                Environment.Exit(1);
            }
            Label l = new Label();

            Servers = new List<string>();
            //Servers = new ObservableCollection<string> ();
            //BonjourServiceResolver bsr = new BonjourServiceResolver();
            //bsr.ServiceFound += new Network.ZeroConf.ObjectEvent<Network.ZeroConf.IService>(bsr_ServiceFound);
            //bsr.Resolve("_dmx._udp");
            
            nsBrowser.AllowMultithreadedCallbacks = true;
            nsBrowser.AllowApplicationForms = true;

            //nsBrowser.InvokeableObject = l;
            nsBrowser.DidFindService += new NetServiceBrowser.ServiceFound(nsBrowser_DidFindService);
            nsBrowser.DidRemoveService += new NetServiceBrowser.ServiceRemoved(nsBrowser_DidRemoveService);
            nsBrowser.SearchForService("_dmx._udp", "");
        }

        public void Stop()
        {
            nsBrowser.Stop();
        }

        void nsBrowser_DidRemoveService(NetServiceBrowser browser, NetService service, bool moreComing)
        {
            Servers.RemoveAll((o) => { return o == service.HostName; });
        }
        void nsBrowser_DidFindService(NetServiceBrowser browser, NetService service, bool moreComing)
        {

            service.DidResolveService += new NetService.ServiceResolved(service_DidResolveService);
            service.ResolveWithTimeout(200);
        }

        public event Action<string,string> OnServerFound;

        void service_DidResolveService(NetService service)
        {
            if (!Servers.Contains(service.HostName))
            {
                Servers.Add(service.HostName);
                if (OnServerFound != null)
                    OnServerFound(service.HostName,(service.Addresses[0] as IPEndPoint).Address.ToString());
            }
        }
    }
}
