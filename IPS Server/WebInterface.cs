using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using MiniHttpd;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Alchemy.Classes;
using System.Web;
using IPS.SharedObjects;
using IPS.Communication;

namespace IPS.Server
{
    public class WebInterface
    {
        private class CacheManifest : IFile
        {
            public CacheManifest(IDirectory parent)
            {
                this.Parent = parent;
            }

            public string ContentType
            {
                get { return "text/cache-manifest"; }
            }

            public void OnFileRequested(MiniHttpd.HttpRequest request, IDirectory directory)
            {

                FileInfo index = new FileInfo(Directory.GetCurrentDirectory() + "//www//admin//template//cache.manifest.file");
                //insert stuff
                string content = new StreamReader(index.OpenRead()).ReadToEnd();
                request.Response.ResponseContent = new MemoryStream();
                StreamWriter w = new StreamWriter(request.Response.ResponseContent);
                request.Response.ContentType = "text/cache-manifest";
                w.Write(content);
                w.Flush();
            }

            public string Name
            {
                get { return "cache.manifest"; }
            }

            public IDirectory Parent
            {
                get;
                private set;
            }

            public void Dispose()
            {
             
            }
        }

        public class MyIndex : IFile
        {
            MainForm window;
            public MyIndex(MainForm window)
            {
                this.window = window;  
            }

            #region IFile Members

            public void OnFileRequested(MiniHttpd.HttpRequest request, IDirectory directory)
            {                
                //open file
                FileInfo index = new FileInfo(Directory.GetCurrentDirectory() + "//www//admin//index.html");
                //insert stuff
                string content = new StreamReader(index.OpenRead()).ReadToEnd();

                content = content.Replace("%accesslist%", window.AllowedDevices);
                content = content.Replace("%systemname%", window.SystemName);
                if (window.IsVirtual)
                    content = content.Replace("%virtchecked%", "checked");

                //output
                request.Response.ResponseContent = new MemoryStream();
                StreamWriter w = new StreamWriter(request.Response.ResponseContent);
                w.Write(content);
                w.Flush();
            }

            public string ContentType
            {
                get { return "text/html"; }
            }

            #endregion

            #region IResource Members

            public string Name
            {
                get { return "index.html"; }
            }

            public IDirectory Parent
            {
                get { return new VirtualDirectory(); }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                //do nothing
            }

            #endregion
        }

        MainForm window;
        HttpWebServer server;
        BasicAuthenticator auth;
        Alchemy.WebSocketServer sse;
        public WebInterface(MainForm window)
        {
            this.window = window;
            
            window.OnChannelUpdate += new DmxManager.DmxStatus(DmxController_OnReceive);

            server = new HttpWebServer(80);
            
            VirtualDirectory root = new VirtualDirectory();
            DriveDirectory template = new DriveDirectory("template",Directory.GetCurrentDirectory() + @"\www\admin\template",root);
            DriveDirectory apps = new DriveDirectory("apps", Directory.GetCurrentDirectory() + @"\www\admin\apps", root);

            server.Root = root;
            var cm = new CacheManifest(root);
           
            root.AddDirectory(template);
            root.AddDirectory(apps);

            server.IndexPage = new MyIndex(window);

            root.AddFile(cm);

            //auth = new BasicAuthenticator();
            //auth.AddUser("admin", Properties.Settings.Default.password);

            //server.RequireAuthentication = true;
            //server.AuthenticateRealm = "DMX Controller Login";
            //server.Authenticator = auth;

            server.ValidRequestReceived += new HttpServer.RequestEventHandler(server_ValidRequestReceived);
            server.InvalidRequestReceived += new HttpServer.RequestEventHandler(server_InvalidRequestReceived);

            server.Start();

            try
            {
                ////start server socket, for feedback + fast comms
                sse = new Alchemy.WebSocketServer(8282, IPAddress.Any);
                sse.OnConnected = OnConnected;
                sse.OnDisconnect = OnDisconnect;
                sse.Start();
            }
            catch (Exception e)
            {
               
            }

            //setup hooks for changing cuelist...
            window.CueStack.Cues.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Cues_CollectionChanged);
        }

        void Cues_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
           //transmit new things...
            JsonExSerializer.Serializer ser = new JsonExSerializer.Serializer(typeof(SocketCommand));
            SocketCommand s = new SocketCommand() { command = "cues", cues = window.CueStack.Cues.Select((o1) => o1.Name).ToArray() };
            ser.Config.IsCompact = true;
            ser.Config.OutputTypeInformation = false;
            ser.Config.OutputTypeComment = false;
            string dat = ser.Serialize(s);
            //string data2 = "{\"command\":\"cues\",\"cues\"" + dat + "\"}";
            Clients.ForEach(new Action<UserContext>((ox) => { ox.Send(dat); }));
        }

        DateTime last = new DateTime();

        void DmxController_OnReceive(byte[] chans)
        {
            //if (DateTime.Now - last > TimeSpan.FromMilliseconds(50))
            //{
            //    string[] ints = chans.Select(x => ((int)x).ToString()).ToArray();

            //    string data = "{\"command\":\"update\",\"channels\":[" + ints.Aggregate((o, e) => o + "," + e) + "]}";
            //    Clients.ForEach(new Action<UserContext>((ox) => { ox.Send(data); }));
            //    last = DateTime.Now;
            //}
        }

        List<UserContext> Clients = new List<UserContext>();

        void OnConnected(UserContext context)
        {
            Clients.Add(context);
            context.SetOnReceive(new Alchemy.OnEventDelegate((o) => { 
                //process received frame...
                //parse data: o.Data

                //Uri u = new Uri("http://localhost/" + o.DataFrame);

                var vals = HttpUtility.ParseQueryString(o.DataFrame.ToString());

                switch (vals[0])
                {
                    //case "update":
                    //    int channel = Convert.ToInt32(vals["channel"]);
                    //    int value = Convert.ToInt32(vals["value"]);
                    //    if (window.DmxController != null)
                    //    {
                    //        window.DmxController.UpdateChannel(channel, value);
                    //    }
                    //    break;

                    case "status":
                        string html = "Status at " + DateTime.Now.ToUniversalTime() + "<br>" + "Server Running: <b>" + window.IsRunning + "</b><br>" + "Connected Clients: <b>" + window.ConnectedClients + "</b><br>";
                        string data = "{\"command\":\"status\",\"html\":\""+ html + "\"}";
                        context.Send(data);
                        break;

                    case "cues":
                        //json
                        JsonExSerializer.Serializer ser = new JsonExSerializer.Serializer(typeof(SocketCommand));
                        ser.Config.IsCompact = true;
                        ser.Config.OutputTypeInformation = false;
                        ser.Config.OutputTypeComment = false;
                        SocketCommand s = new SocketCommand() {command="cues",cues=window.CueStack.Cues.Select((o1)=>o1.Name).ToArray()};
                        string dat = ser.Serialize(s);
                        //string data2 = "{\"command\":\"cues\",\"cues\"" + dat + "\"}";
                        context.Send(dat);
                        break;

                    case "venues":
                        //json
                        JsonExSerializer.Serializer serx = new JsonExSerializer.Serializer(typeof(Dictionary<string,string>));
                        serx.Config.IsCompact = true;
                        serx.Config.OutputTypeInformation = false;
                        serx.Config.OutputTypeComment = false;
                        Dictionary<string, string> rigs = new Dictionary<string, string>();
                        foreach (Rig r in window.Venues.Layouts)
                        {
                            rigs.Add(new FileInfo(r.Filename).Name,r.Name);
                        }
                        string datx = serx.Serialize(rigs);
                        //string data2 = "{\"command\":\"cues\",\"cues\"" + dat + "\"}";
                        context.Send(datx);
                        break;

                    case "newcue"://create cue
                        //adds new cue
                        window.NewCue(vals["name"]);
                        break;

                    case "cue"://play cue
                        window.PlayCue(Int32.Parse(vals["index"]));
                        break;
                }

                //Console.WriteLine("Data received:" + vals.Keys[0] + ":" + vals[0]);
            }));
        }

        private class SocketCommand
        {
            public string command = "";
            public string[] cues;
        }

        void OnDisconnect(UserContext context)
        {
            Clients.Remove(context);
        }

        void server_InvalidRequestReceived(object sender, RequestEventArgs e)
        {
            //redirect back to index...
            //e.Request.Response.ResponseContent = new MemoryStream();
            //StreamWriter w = new StreamWriter(e.Request.Response.ResponseContent);
            e.Request.Response.ResponseCode = "301";
            e.Request.Response.SetHeader("Location","/");
            
        }

        /**
        * Knuth-Morris-Pratt Algorithm for Pattern Matching
        */
       
            /**
            * Finds the first occurrence of the pattern in the text.
            */
            public int indexOf(byte[] data, byte[] pattern)
            {
                int[] failure = computeFailure(pattern);

                int j = 0;
                if (data.Length == 0) return -1;

                for (int i = 0; i < data.Length; i++)
                {
                    while (j > 0 && pattern[j] != data[i])
                    {
                        j = failure[j - 1];
                    }
                    if (pattern[j] == data[i]) { j++; }
                    if (j == pattern.Length)
                    {
                        return i - pattern.Length + 1;
                    }
                }
                return -1;
            }

            /**
            * Computes the failure function using a boot-strapping process,
            * where the pattern is matched against itself.
            */
            private int[] computeFailure(byte[] pattern)
            {
                int[] failure = new int[pattern.Length];

                int j = 0;
                for (int i = 1; i < pattern.Length; i++)
                {
                    while (j > 0 && pattern[j] != pattern[i])
                    {
                        j = failure[j - 1];
                    }
                    if (pattern[j] == pattern[i])
                    {
                        j++;
                    }
                    failure[i] = j;
                }

                return failure;
            }

        void server_ValidRequestReceived(object sender, RequestEventArgs e)
        {
            //Console.WriteLine("Good: " + e.Request.ToString());
            if (e.Request.Method == "POST")
            {
                switch (e.Request.Uri.Query)
                {
                    case "?venue":

                        //do something with the venue file...

                        string[] split = e.Request.Headers["Content-type"].Split('=');
                        string boundrycode = split[1];

                        string filename = Directory.GetCurrentDirectory() + "//www/layout_upload_" + DateTime.Now.Ticks + ".thor";
                        FileInfo file = new FileInfo(filename);
                        FileStream fs = file.Open(FileMode.Create);

                        BinaryReader br = new BinaryReader(e.Request.PostData);
                        byte[] bytes = new byte[e.Request.ContentLength];
                        br.Read(bytes, 0, bytes.Length);

                        //Debug.Print(""+bytes);

                        //need to remove the right bytes...
                        int len = bytes.Length - (boundrycode.Length + 6);

                        byte[] newarr = new byte[len];
                        Array.Copy(bytes, newarr, len);                        

                        int position = indexOf(newarr, new byte[]{13,10,13,10});//boundry at start

                        int len2 = newarr.Length-(position);

                        byte[] evennewer = new byte[len2];

                        Array.Copy(newarr,position+4, evennewer,0, len2-4);
                        fs.Write(evennewer, 0, evennewer.Length);
                        fs.Close();
                        window.Venues.AddVenue(filename);
                        //update window...
                        break;

                    case "?restart":
                        window.RestartServer();
                        break;

                    default:
                        //send back default file
                        server.NavigateToUrl("/");
                        break;
                }
            }
            else
            {
                switch (e.Request.Uri.Query)
                {
                    case "?password":
                        if (e.Request.Query["password"] != "")
                        {
                            auth.RemoveUser("admin");
                            auth.AddUser("admin", e.Request.Query["password"]);
                            Properties.Settings.Default.password = e.Request.Query["password"];
                            Properties.Settings.Default.Save();

                            e.Request.Response.ResponseContent = new MemoryStream();
                            StreamWriter writer2 = new StreamWriter(e.Request.Response.ResponseContent);
                            writer2.Flush();
                        }
                        break;
                }
                
                if (e.Request.Uri.Query.Contains("dmx"))
                {

                    window.IsVirtual = false;
                    if (e.Request.Query.AllKeys.Contains("virtual"))
                        window.IsVirtual = true;

                    window.RestartServer();
                    e.Request.Response.ResponseContent = new MemoryStream();
                    StreamWriter writer = new StreamWriter(e.Request.Response.ResponseContent);
                    writer.Flush();
                }
                if (e.Request.Uri.Query.Contains("access"))
                {
                    window.AllowedDevices = e.Request.Query["accesslist"];
                    window.RestartServer();
                    e.Request.Response.ResponseContent = new MemoryStream();
                    StreamWriter writer = new StreamWriter(e.Request.Response.ResponseContent);
                    writer.Flush();
                }
            }
        }
    }
}
