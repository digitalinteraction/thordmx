using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace PaintBucket
{
    public class Layer
    {
        private int width;
        private int height;
        private float[,,] pigmentmap;
        private float[,] heightmap;
        private byte[] render;
        PixelFormat pf = PixelFormats.Bgra32;
        int rawStride;
        int painted;

        public int GetWidth()
        {
            return width;
        }

        public int GetHeight()
        {
            return height;
        }

        public float[] GetColorAt(int x,int y)
        {
            float r = pigmentmap[y, x, 0];
            float g = pigmentmap[y, x, 1];
            float b = pigmentmap[y, x, 2];
            float a = Math.Min(1.0f, 1.0f * heightmap[y, x]);
            float[] temp = new float[4];
            temp[0] = (byte)(255.0f * (1.0f - a * Math.Min(1.0f, r)));
            temp[1] = (byte)(255.0f * (1.0f - a * Math.Min(1.0f, g)));
            temp[2] = (byte)(255.0f * (1.0f - a * Math.Min(1.0f, b)));
            //render[q++] = (byte)(255);
            temp[3] = (byte)((255.0f * a));
            return temp;
        }

        public float GetHeightAt(int x, int y)
        {
            return heightmap[x, y];
        }

        public Layer(int width, int height)
        {
            this.width = width;
            this.height = height;
            pigmentmap = new float[width,height,3];
            heightmap = new float[width,height];

            rawStride = (width * pf.BitsPerPixel + 7) / 8;
            render = new byte[rawStride * height];
            painted = 0;
        }

        public void AddImage(string src)
        {            
            BitmapImage i = new BitmapImage(new Uri(src));            
        }

        //TODO squidge function

        //TODO texture map for finger

        //TODO Tools for manipulating paint

        //TODO drying function

        //TODO speed!

        public void AddPaint(float cx, float cy, float cr, float pc, float pm, float py)
        {
            for (int y = 0; y < pigmentmap.GetLength(1); y++)
            {
                for (int x = 0; x < pigmentmap.GetLength(0); x++)
                {
                    float v = (float)Math.Sqrt(Math.Pow(cx - x, 2) + Math.Pow(cy - y, 2)) / cr;
                    if (v < 1.0f)
                    {
                        v = Math.Max(v * 5.0f, 1.0f);
                        pigmentmap[x, y, 0] = pc;
                        pigmentmap[x, y, 1] = pm;
                        pigmentmap[x, y, 2] = py;
                        //heightmap[x, y] = 10;
                        heightmap[x, y] = 5 * v;
                        //heightmap[x, y] = 0;
                        painted = painted + 1;
                    }
                }
            }
        }

        public double getPaintedAmt()
        {
            double len1, len0, res;
            len1 = pigmentmap.GetLength(1);
            len0 = pigmentmap.GetLength(0);
            res = painted / (len1 * len0);
            return res;//painted / (pigmentmap.GetLength(1) * pigmentmap.GetLength(0)); 
        }

        public void AddPaint()
        {
            int width = pigmentmap.GetLength(0);
            int height = pigmentmap.GetLength(1);
            float cr = 50.0f;
            int n = 8;
            for (int i = 0; i < n; i++)
            {
                float cx = (width  / 2) + (width  / 4) * (float)Math.Cos(2 * Math.PI * i / n);
                float cy = (height / 2) + (height / 4) * (float)Math.Sin(2 * Math.PI * i / n);
                float pc = 3.0f * ((i >> 0) & 1);
                float pm = 3.0f * ((i >> 1) & 1);
                float py = 3.0f * ((i >> 2) & 1);
                //float pt = pc + pm + py;
                //if (pt == 0.0f) { pt = 1.0f; }   // Avoid /0
                //AddPaint(cx, cy, cr, pc / pt, pm / pt, py / pt);
                AddPaint(cx, cy, cr, pc, pm, py);
            }
//AddPaint(width / 2.0f, height / 2.0f, 50.0f, 20 * 255.0f, 0.2f, 0.2f);
        }

        //called every certain amount of time
        public void DryToLayer(double amount)
        {

            for (int x = 0; x < this.width; x++)
            {
                for (int y = 0; y<this.height; y++)
                {
                    if (this.heightmap[x, y] > 0)
                    {
                        this.heightmap[x, y] = this.heightmap[x, y] - (float)amount;
                    }
                }
            }
        }

        //clears canvas
        public void Clear()
        {
            pigmentmap = new float[width,height,3];
            heightmap = new float[width,height];
            painted = 0;
        }


        //for trasferring between layers
        public void TransferToLayer(Layer l, float amount,float xi,float yi,bool direction)
        {
            int xmin = (int)xi;
            int xmax = (int)xi + l.width;
            int ymin = (int)yi;
            int ymax = (int)yi + l.height;

            if (xmin < 0) xmin = 0;
            if (ymin < 0) ymin = 0;
            if (xmax > this.width) xmax = this.width;
            if (ymax > this.height) ymax = this.height;

            //Debug.Print("height:" + l.heightmap[0, 0]);

            if (direction) // putting back down on canvas...
            {
                //for each pixel in l, add r,g,b values to same place in this layer and add a amount*height of finger to this layer
                
                //pixels on the bigger canvas
                for (int x = xmin; x < xmax; x++)
                {
                    for (int y = ymin; y < ymax; y++)
                    {
                        // on canvas
                        float cr = this.pigmentmap[x,y,0];
                        float cg = this.pigmentmap[x,y,1];
                        float cb = this.pigmentmap[x, y, 2];
                        float ch = this.heightmap[x, y];

                        // on brush
                        float br = l.pigmentmap[x - xmin, y - ymin, 0];
                        float bg = l.pigmentmap[x - xmin, y - ymin, 1];
                        float bb = l.pigmentmap[x - xmin, y - ymin, 2];
                        float bh = l.heightmap[x - xmin, y - ymin];

                        // calculate new values
                        float h = ch + bh;
                        float r, g, b;
                        if (h == 0.0f) { r = g = b = 0.0f; }
                        else
                        {
                            r = cr * (ch / h) + br * (bh / h);
                            g = cg * (ch / h) + bg * (bh / h);
                            b = cb * (ch / h) + bb * (bh / h);
                        }

                        //if (l.heightmap[x-xmin, y-ymin] > 0)

                        // Set canvas
                        this.pigmentmap[x, y, 0] = r;
                        this.pigmentmap[x, y, 1] = g;
                        this.pigmentmap[x, y, 2] = b;
                        this.heightmap[x, y] = h;

                        //remove hight from finger
                        //l.heightmap[x-xmin, y-ymin] = l.heightmap[x-xmin, y-ymin] - 1;

                        //remove and add height map
                        //this.heightmap[x*y] = 

                    }
                }
            }
            else //picking up from canvas...
            {
                //pixels on the bigger canvas
                for (int x = xmin; x < xmax; x++)
                {
                    for (int y = ymin; y < ymax; y++)
                    {
                        float r = this.pigmentmap[x, y, 0];
                        float g = this.pigmentmap[x, y, 1];
                        float b = this.pigmentmap[x, y, 2];

                        float a = amount * Math.Max(0.0f, (float)(1.0f - Math.Sqrt(Math.Pow(x - (float)(xmin + xmax) / 2, 2) + Math.Pow(y - (float)(ymin + ymax) / 2, 2)) / ((float)Math.Min(xmax - xmin, ymax - ymin) / 2)));
                        a = Math.Min(a, 1.0f);

                        /*
                        if (r < 64.0f) { r = 0.0f; }
                        if (g < 64.0f) { g = 0.0f; }
                        if (b < 64.0f) { b = 0.0f; }

                        r *= a;
                        g *= a;
                        b *= a;

                        float max = 0;
                        max = Math.Max(max, r);
                        max = Math.Max(max, g);
                        max = Math.Max(max, b);
                        max = Math.Max(255.0f, max);
                        float scale = 255.0f / max;

                        r *= scale;
                        g *= scale;
                        b *= scale;

                        this.pigmentmap[x, y, 0] -= r;
                        this.pigmentmap[x, y, 1] -= g;
                        this.pigmentmap[x, y, 2] -= b;

                        //check to see if there is enough paint...
                        //if (this.heightmap[x, y] > 0)
                        {
                            l.pigmentmap[x - xmin, y - ymin, 0] = r;
                            l.pigmentmap[x - xmin, y - ymin, 1] = g;
                            l.pigmentmap[x - xmin, y - ymin, 2] = b;
                            l.heightmap[x - xmin, y - ymin] = l.heightmap[x - xmin, y - ymin] + 1;
                            this.heightmap[x, y] = this.heightmap[x, y] - 1;
                        }
                        */

                        float h = this.heightmap[x, y];

                        h *= a;

                        l.pigmentmap[x - xmin, y - ymin, 0] = r;
                        l.pigmentmap[x - xmin, y - ymin, 1] = g;
                        l.pigmentmap[x - xmin, y - ymin, 2] = b;
                        l.heightmap[x - xmin, y - ymin] = h;
                        this.heightmap[x, y] -= h;

                    }
                }
            }
        }
        
        //to get image values
        public byte[] Render()
        {
            int q = 0;
            int width = pigmentmap.GetLength(0);
            int height = pigmentmap.GetLength(1);
            painted = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    //float tp = pigmentmap[j, i, 0] + pigmentmap[j, i, 1] + pigmentmap[j, i, 2];
                    //if (tp == 0.0f) { tp = 1.0f; } // Avoid /0

/*
if (pigmentmap[j, i, 0] + pigmentmap[j, i, 1] + pigmentmap[j, i, 2] <= 0.02f)
{
    if ((((i >> 6) & 1) ^ ((j >> 6) & 1)) != 0)
    {
        render[q++] = 0x00; render[q++] = 0x00; render[q++] = 0x00; render[q++] = 0x00;
    }
    else
    {
        render[q++] = 0x33; render[q++] = 0x33; render[q++] = 0x33; render[q++] = 0x00;
    }
}
else
{
*/

                    float r = pigmentmap[j, i, 0];
                    float g = pigmentmap[j, i, 1];
                    float b = pigmentmap[j, i, 2];

                    // update the painted amount
                    if (r>0 || g > 0 || b >0)
                        painted = painted + 1;
                    

                    /*
                    float h0 = heightmap[j, i];
                    float h1 = (i + 1 < height && j + 1 < width) ? heightmap[j + 1, i + 1] : h0;

                    float d = h1 - h0;
                    r += 0.2f * d; g += 0.2f * d; b += 0.2f * d; 
                    r = Math.Max(0, r); g = Math.Max(0, g); b = Math.Max(0, b);
                    */
                    
                    //RGBA

                    float a = Math.Min(1.0f, 1.0f * heightmap[j, i]);

                    render[q++] = (byte)(255.0f * (1.0f - a * Math.Min(1.0f, b)));
                    render[q++] = (byte)(255.0f * (1.0f - a * Math.Min(1.0f, g)));
                    render[q++] = (byte)(255.0f * (1.0f - a * Math.Min(1.0f, r)));
                    //render[q++] = (byte)(255);
                    render[q++] = (byte)((255.0f * a));

//}


                    /*
                    float max = 0;
                    max = Math.Max(max, pigmentmap[j, i, 0]);
                    max = Math.Max(max, pigmentmap[j, i, 1]);
                    max = Math.Max(max, pigmentmap[j, i, 2]);
                    max = Math.Max(255.0f, max);
                    float scale = 255.0f / max;

                    render[q] = (byte)(pigmentmap[j, i, 0] * scale);
                    q++;
                    render[q] = (byte)(pigmentmap[j, i, 1] * scale);
                    q++;
                    render[q] = (byte)(pigmentmap[j, i, 2] * scale);
                    q++;
                    render[q] = (byte)0;
                    q++;
                    */


                }
            }
            return render;
        }
    }
}
