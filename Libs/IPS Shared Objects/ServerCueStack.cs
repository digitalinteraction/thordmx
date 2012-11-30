using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections;

namespace IPS.SharedObjects
{


    public class ServerCueStack
    {
        //list of cues
        public ObservableCollection<ServerCue> Cues { get; set; }

        public ServerCueStack()
        {
            //try and deserialize the list:
            try
            {
                JsonExSerializer.Serializer ser = new JsonExSerializer.Serializer(typeof(ObservableCollection<ServerCue>));
                if (File.Exists(Directory.GetCurrentDirectory() + "\\servercues.json"))
                {
                    var f = File.OpenText(Directory.GetCurrentDirectory() + "\\servercues.json");
                    Cues = ser.Deserialize(f.ReadToEnd()) as ObservableCollection<ServerCue>;
                    f.Close();
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                Cues = new ObservableCollection<ServerCue>();
                //for testing...
         //       Cues.Add(new ServerCue() {Name="Test Cue"  });
           //     Cues[0].Channels[2] = 255;
            }

            Cues.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Cues_CollectionChanged);
        }

        void Cues_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //save new version of it...
            JsonExSerializer.Serializer ser = new JsonExSerializer.Serializer(typeof(ObservableCollection<ServerCue>));
            string s = ser.Serialize(Cues);
            var f = File.CreateText(Directory.GetCurrentDirectory() + "\\servercues.json");
            f.Write(s);
            f.Close();            
        }

        //new cue

        //get cue values

        

        //get cue values given existing dmx values
        public byte[] GetCombinedCue(ServerCue c,byte[] currentstatus)
        {
            //outs = currentstatus.Join(c.Channels,

            byte[] outp = new byte[513];
            //var outp = currentstatus.Combine(c.Channels,(o1,o2)=> (int)o1==-1 ? o2 : o1);

            for (int i = 0; i < 513; i++)
            {
                outp[i] = (byte)(c.Channels[i] == -1 ? currentstatus[i] : (byte)c.Channels[i]);
            }

            //combine the two arrays replacing only the ones which are not -1 in the server array
            return outp;
            //return c.Channels.();
        }
    }
}
