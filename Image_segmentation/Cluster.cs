using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image_segmentation
{
    class Cluster
    {
        public List<Img_pixel> scores = new List<Img_pixel>();// массив координат пикселей
        public Img_pixel current_pixel;//координаты текущего центроида
        public Img_pixel last_pixel;//координаты предыдущего центоида
        public Cluster()
        {

        }
        public Cluster(Img_pixel pixel)
        {
            current_pixel = pixel;
        }
    }
}
