using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Reflection;
using ZeroconfService;
using System.Collections;

namespace IPS.Communication
{
    public class DmxManager
    {
        List<IEventClient> eventsources;
        List<IDmxOutput> dmxoutputs;
        
        public List<string> ValidDevices
        {
            get;
            set;
        }

        //private byte[] virt = new byte[513];

        public delegate void DmxStatus(byte[] chans);
        public delegate void DevicesStatus(List<string> devices);

        public Dictionary<string,DateTime> LiveDevices
        {
            get;
            set;
        }

        public event Action<string> OnLogEvent;

        public List<IDmxOutput> Outputs { get { return dmxoutputs; } }
        public List<IEventClient> Clients { get { return eventsources; } }

        public event DmxStatus OnReceive;
        public event DevicesStatus OnDeviceUpdate;

        List<NetService> Services = new List<NetService>();

        private void SetupServerBroadcast()
        {
            //ZEROCONF
            foreach (var o in Outputs)
            {
                if (o is IServerService)
                {
                    foreach (var s in (o as IServerService).Services)
                    {
                        var sr = new NetService("", s.Value, "NUILightServer Out - " + o.Name, s.Key);
                        sr.Publish();
                        Services.Add(sr);
                    }
                }
            }

            foreach (var o in Clients)
            {
                if (o is IServerService)
                {
                    foreach (var s in (o as IServerService).Services)
                    {
                        var sr = new NetService("", s.Value, "NUILightServer In - " + o.Name, s.Key);
                        sr.Publish();
                        Services.Add(sr);
                    }
                }
            }

            var ss = new NetService("", "_http._tcp", "NUILightServer - Venue Server", 1235);
            ss.Publish();
            Services.Add(ss);

            var s1 = new NetService("", "_http._tcp", "NUILightServer - Admin HTML5 App", 80);
            s1.Publish();
            Services.Add(s1);

            var s2 = new NetService("", "_http._tcp", "NUILightServer - WebSockets Server Control", 8282);
            s2.Publish();
            Services.Add(s2);
        }

        public void StopServerBroadcast()
        {
            Services.ForEach((o) => { o.Stop(); });
            Services.Clear();
        }

        public bool IsStarted {get;set;}

        private void ProcessLog(string log)
        {
            if (OnLogEvent != null)
            {
                OnLogEvent(log);
            }
        }

        public void Start()
        {
            SetupServerBroadcast();
            dmxoutputs.ForEach(new Action<IDmxOutput>((o) =>
            {
                try
                {
                    (o as ILoggable).OnLogEvent += ProcessLog;
                    o.Start();
                }
                catch { }
            }));

            eventsources.ForEach(new Action<IEventClient>((o) =>
            {
                try
                {
                    (o as ILoggable).OnLogEvent += ProcessLog;
                    o.Connect();
                }
                catch { }
            }));
            IsStarted = true;
        }

        public void Stop()
        {
            IsStarted = false;
            //stop all...
            dmxoutputs.ForEach(new Action<IDmxOutput>((o) =>
            {
                (o as ILoggable).OnLogEvent -= ProcessLog;
                o.Stop();

            }));

            //stop Bonjour broadcast
            StopServerBroadcast();
            eventsources.ForEach((o) => { 
                o.Disconnect();
                (o as ILoggable).OnLogEvent -= ProcessLog;
            });
        }

        public DmxManager()
        {
            eventsources = new List<IEventClient>();
            dmxoutputs = new List<IDmxOutput>();
            //auto load plugins...
            if (Directory.Exists(Directory.GetCurrentDirectory() + "\\Plugins"))
            {
                //for each dll
                foreach (string s in Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Plugins","*.dll"))
                {
                    try
                    {
                        Assembly am = Assembly.LoadFile(s);
                        var types = am.GetTypes();
                        foreach (Type t in types)
                        {
                            if (typeof(IEventClient).IsAssignableFrom(t))
                            {
                                var ttx = Activator.CreateInstance(t) as IEventClient;
                                //link up any events...
                                eventsources.Add(ttx);
                            }

                            if (typeof(IDmxOutput).IsAssignableFrom(t))
                            {
                                var ttx = Activator.CreateInstance(t) as IDmxOutput;
                                //link up any events...
                                dmxoutputs.Add(ttx);
                            }
                        }
                    }
                    catch { }

                }
            }
            else
            {
                throw new Exception();
            }

           
            
            ValidDevices = new List<string>();
            LiveDevices = new Dictionary<string, DateTime>();
            

            //setup timer to purge connected devices...
            Thread tt = new Thread(new ThreadStart(() => {

                while (true)
                {
                    Dictionary<string, DateTime> Temp = new Dictionary<string, DateTime>();

                    Temp = LiveDevices;

                    List<string> remove = new List<string>();

                    foreach (KeyValuePair<string,DateTime> kp in Temp)
                    {    
                        if (kp.Value < DateTime.Now.Subtract(TimeSpan.FromSeconds(10)))
                        {
                            remove.Add(kp.Key);
                        }

                    }
                    foreach (string s in remove)
                        LiveDevices.Remove(s);

                    if (OnDeviceUpdate != null)
                        OnDeviceUpdate(LiveDevices.Keys.ToList());

                    Thread.Sleep(1000);
                }
            }));
            tt.Priority = ThreadPriority.Lowest;
            tt.Start();

            Thread ttt = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    fireStatus();
                    KeepAlive();
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }));
            ttt.Priority = ThreadPriority.Lowest;
            ttt.Start();

            

            #region handlers
            foreach (IEventClient osc in eventsources)
            {
                

                //handlers
                osc.RegisterHandler((dev, type, variables) =>
                {
                    //update dmx data...
                    if (ValidDevices.Contains("*") || ValidDevices.Contains(dev))
                    {
                        
                        //for each 
                        dmxoutputs.ForEach(new Action<IDmxOutput>((o) =>
                        {
                            o.UpdateChannel((int)variables[1], (int)variables[2]);
                        }));
                        
                        //    virt[(int)variables[1]] = (byte)((int)variables[2]);

                        if (!LiveDevices.ContainsKey(dev))
                        {
                            LiveDevices.Add(dev, DateTime.Now);
                        }
                        else
                        {
                            LiveDevices[dev] = DateTime.Now;
                        }
                    }
                    fireStatus();
                }
                , "dmx", "updatechannel", "*");

                osc.RegisterHandler((dev, type, variables) =>
                {
                    //update dmx data...
                    if (ValidDevices.Contains("*") || ValidDevices.Contains(dev))
                    {
                        //byte[] vars = new byte[513];
                        if (variables.Count() == 512)
                        {

                            //for (int i = 1; i <= 512; i++)
                            //{
                            //    vars[i - 1] = (byte)(variables[i]);
                            //    //dmx.UpdateChannel(i, (int)variables[i]);
                            //    //dmx.UpdateChannels(
                            //    //Debug.Print(""+i);
                            //}

                            temp = variables.Cast<byte?>().ToArray();
                            //temp = variables.Select(x => (byte?)((int)x)).ToArray();

                            for (int o = 0; o < temp.Length; o++)
                            {
                                if (!temp[o].HasValue)//replace with live value
                                {
                                    temp[o] = currentoutput[o];
                                }
                            }

                            dmxoutputs.ForEach(new Action<IDmxOutput>((o) =>
                            {
                                o.UpdateChannels(temp.Cast<byte>().ToArray());
                            }));

                            //dmxoutputs.ForEach(new Action<IDmxOutput>((o) =>
                            //{
                                
                            //    o.UpdateChannels(variables.Cast<byte>().ToArray());
                            //}));
                        }
                    }

                    fireStatus();

                    if (!LiveDevices.ContainsKey(dev))
                    {
                        LiveDevices.Add(dev, DateTime.Now);
                    }
                    else
                    {
                        LiveDevices[dev] = DateTime.Now;
                    }

                }, "dmx", "frameupdate", "*");

                osc.RegisterHandler((dev, type, variables) =>
                {
                    //update dmx data...
                    if (ValidDevices.Contains("*") || ValidDevices.Contains(dev))
                    {
                        dmxoutputs.ForEach(new Action<IDmxOutput>((o) =>
                        {
                            o.UpdateChannels(new byte[513]);
                            o.UpdateChannel((int)variables[1], (int)variables[2]);
                        }));
                    }
                    fireStatus();
                    if (!LiveDevices.ContainsKey(dev))
                    {
                        LiveDevices.Add(dev, DateTime.Now);
                    }
                    else
                    {
                        LiveDevices[dev] = DateTime.Now;
                    }

                }, "dmx", "replacechannel", "*");

                osc.RegisterHandler((dev, type, variables) =>
                {
                    //blackout data
                    if (ValidDevices.Contains("*") || ValidDevices.Contains(dev))
                    {
                        dmxoutputs.ForEach(new Action<IDmxOutput>((o) =>
                        {
                            o.BlackOut();
                        }));
                    }
                    fireStatus();
                    if (!LiveDevices.ContainsKey(dev))
                    {
                        LiveDevices.Add(dev, DateTime.Now);
                    }
                    else
                    {
                        LiveDevices[dev] = DateTime.Now;
                    }

                }, "dmx", "blackout", "*");
                //osc.Connect();
            }

            #endregion        
        }

        public void UpdateChannel(int chan, int val)
        {
            dmxoutputs.ForEach(new Action<IDmxOutput>((o) =>
           {
               o.UpdateChannel(chan, val);
           }));
        }

        byte?[] temp = new byte?[512];

        public void UpdateChannels(byte[] chans) //replaces the whole frame
        {
            dmxoutputs.ForEach(new Action<IDmxOutput>((o) =>
            {
                o.UpdateChannels(chans);
            }));
        }

        //only replaces the ones that have changed...
        public void UpdateChannels(byte?[] chans)
        {
            temp = chans;
            for (int o = 0; o < temp.Length; o++)
            {
                if (!temp[o].HasValue)//replace with live value
                {
                    temp[o] = currentoutput[o];
                }
            }

            dmxoutputs.ForEach(new Action<IDmxOutput>((o) =>
            {
                o.UpdateChannels(temp.Cast<byte>().ToArray());
            }));
        }

        byte[] currentoutput = new byte[512];

        private void KeepAlive()
        {
            if (IsStarted)
            {
                dmxoutputs.ForEach(new Action<IDmxOutput>((o) =>
                {
                    o.UpdateChannels(currentoutput);
                }));
            }
        }

        private void fireStatus()
        {
            if (dmxoutputs.Count > 0)
            {
                if (OnReceive != null)
                {
                    OnReceive(dmxoutputs.First().GetChannelData());//to gui / webui
                }
                currentoutput = dmxoutputs.First().GetChannelData();
            }
        }

        public void Close()
        {
            try
            {
                dmxoutputs.ForEach(new Action<IDmxOutput>((o) =>
                {
                    o.Stop();
                }));
            }
            catch
            {

            }
            try
            {
                eventsources.ForEach(new Action<IEventClient>((o) =>
                {
                    o.Disconnect();
                }));
                
            }
            catch
            {

            }            
        }

        ~DmxManager()
        {
            IsStarted = false;
            //stop all...
            dmxoutputs.ForEach(new Action<IDmxOutput>((o) =>
            {
                try
                {
                    o.Stop();
                }
                catch { }
            }));

            //stop Bonjour broadcast
            StopServerBroadcast();
            eventsources.ForEach((o) => {
                try
                {
                    o.Disconnect();
                }
                catch { }
            });
        }

        public bool DebugMode
        {
            set
            {
                dmxoutputs.ForEach(new Action<IDmxOutput>((o) =>
                {
                    (o as ILoggable).DebugMode = value;
                }));
                eventsources.ForEach((o) =>
                {
                    (o as ILoggable).DebugMode = value;
                });
            }
        }
    }
}
