using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image_segmentation
{
    class Img_pixel
    {
        public int X, Y;
        public byte R, G, B;

        public Img_pixel()
        {
            X = 0;
            Y = 0;
            R = 0;
            G = 0;
            B = 0;
        }
        public Img_pixel(int x, int y, byte r, byte g, byte b) 
        {
            X = x;
            Y = y;
            R = r;
            G = g;
            B = b;
        }
    }
}
