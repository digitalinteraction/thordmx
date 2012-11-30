using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using JsonExSerializer;

namespace IPS.SharedObjects
{
    public class ServerVenues
    {
        public ObservableCollection<Rig> Layouts { get; set; }

        public ServerVenues()
        {
            Layouts = new ObservableCollection<Rig>();
            //load in all venues from files found in the dir...
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\www", "*.thor");
            foreach (string f in files)
            {
                if (!f.EndsWith("\\venue.thor") && !f.Contains("layout_layout"))
                {
                    //load and parse...
                    try
                    {
                        Layouts.Add(LoadFromFile(f));
                    }
                    catch
                    {
                    }
                }
            }
            if (File.Exists(Directory.GetCurrentDirectory() + "\\www\\venue.thor"))
            {

                CurrentVenue = LoadFromFile(Directory.GetCurrentDirectory() + "\\www\\venue.thor");
            }
            else
            {
                if (Layouts.Count > 0 && CurrentVenue == null)
                {
                    MakeCurrent(Layouts.First());
                }
            }
        }

        public Rig LoadFromFile(string filename)
        {
            ZipFile zip = new ZipFile(filename);
            ZipEntry js = zip.GetEntry("rig.json");
            Serializer s = new Serializer(typeof(Rig));
            Rig rig = (Rig)s.Deserialize(zip.GetInputStream(js));
            rig.Filename = filename;
            zip.Close();
            return rig;
        }

        public void AddVenue(string f)
        {
            FileInfo fi = new FileInfo(f);
            File.Copy(f,Directory.GetCurrentDirectory() + "\\www\\layout_" + fi.Name,true);
            var r = LoadFromFile(Directory.GetCurrentDirectory() + "\\www\\layout_" + fi.Name);
            Layouts.Add(r);
            //make current
            MakeCurrent(r);
        }

        public Rig CurrentVenue { get; set; }

        public void RemoveVenue(Rig rig)
        {
            //if its not the default
            if (CurrentVenue != rig)
            {
                //remove file
                Layouts.Remove(rig);
                try
                {
                    File.Delete(rig.Filename);
                }
                catch
                {
                }
            }
        }

        public event Action<Rig> OnVenueChange;

        public void MakeCurrent(Rig rig)
        {
            File.Copy(rig.Filename, Directory.GetCurrentDirectory() + "\\www\\venue.thor",true);
            CurrentVenue = rig;
            if (OnVenueChange != null)
            {
                OnVenueChange(rig);
            }
        }
    }
}
