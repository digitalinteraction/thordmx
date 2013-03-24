using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using JsonExSerializer;
using System.Net;
using System.IO;

namespace IPS.SharedObjects
{
    public class Rig
    {
        public string Name
        { get; set; }
        public List<Light> Lights
        { get; set; }
        public string Filename { get; set; }

        public static Rig LoadRigFromServer(string serverip)
        {
            WebClient Client = new WebClient();

			//Uri uri = new Uri("http://ncl.ac.uk");
			Client.DownloadFile("http://"+serverip+":1235/venue.thor", "venue.thor");
			//load venue information...

			ZipFile zip = new ZipFile("venue.thor");
			ZipEntry im = zip.GetEntry("venue.jpg");
			ZipEntry js = zip.GetEntry("rig.json");

			Serializer s = new Serializer(typeof(Rig));
			Rig rig = (Rig)s.Deserialize(zip.GetInputStream(js));

            if (rig == null)
            {
                throw new Exception("Rig not Found or Loaded");
            }

			BufferedStream stream = new BufferedStream(zip.GetInputStream(im));

			//FileStream fs = new FileStream("venue.jpg", FileMode.Create);
			//byte[] arr=new byte[stream.Length];

			using (FileStream streamWriter = File.Create("venue.jpg"))
			{
				int size = 2048;
				byte[] data = new byte[2048];
				while (true)
				{
					size = stream.Read(data, 0, data.Length);
					if (size > 0)
						streamWriter.Write(data, 0, size);
					else
						break;
				}
				streamWriter.Close();
			}
            return rig;
        }
    }
}
