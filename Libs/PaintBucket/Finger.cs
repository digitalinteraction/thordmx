using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PaintBucket
{
    public class Finger
    {
        private TouchData data;
        public TouchData Data
        {
            get { return data; }
            set { data = value; }
        }

        private Layer layer;
        public Layer Layer
        {
            get { return layer; }
            set { layer = value; }
        }

    }
}
