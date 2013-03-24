using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace IPS.SurfaceDesk
{
    public static class LightMap
    {
        static LightMap()
        {
            //setup the light things...
            Mapping = new Dictionary<int, BitmapImage>();
            Mapping.Add(0, new BitmapImage(new Uri(@"\Resources\11.png", UriKind.Relative)));
            Mapping.Add(1, new BitmapImage(new Uri(@"\Resources\7.png", UriKind.Relative)));
            Mapping.Add(2, new BitmapImage(new Uri(@"\Resources\9.png", UriKind.Relative)));
            Mapping.Add(3, new BitmapImage(new Uri(@"\Resources\1.png", UriKind.Relative)));
            Mapping.Add(4, new BitmapImage(new Uri(@"\Resources\3.png", UriKind.Relative)));
            Mapping.Add(5, new BitmapImage(new Uri(@"\Resources\2.png", UriKind.Relative)));
            Mapping.Add(6, new BitmapImage(new Uri(@"\Resources\6.png", UriKind.Relative)));
            Mapping.Add(7, new BitmapImage(new Uri(@"\Resources\5.png", UriKind.Relative)));
            Mapping.Add(8, new BitmapImage(new Uri(@"\Resources\3.png", UriKind.Relative)));
        }

        //lamptype,filename
        public static Dictionary<int, BitmapImage> Mapping
        {
            get;
            set;
        }
    }
}
