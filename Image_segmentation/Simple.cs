using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Image_segmentation
{
    class Simple: Cluster
    {
        public static int Start(List<Cluster> clusarr,byte[,,] res, int T, bool MarkUp)
        {
            int Height = res.GetUpperBound(1) + 1;
            int Width = res.GetUpperBound(2) + 1;
            Cluster cl = new Cluster();
            Img_pixel pixel = new Img_pixel();
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    double min = Math.Abs(clusarr[0].current_pixel.R - res[0, i, j])
                        + Math.Abs(clusarr[0].current_pixel.G - res[1, i, j])
                        + Math.Abs(clusarr[0].current_pixel.B - res[2, i, j]); // начальное минимальное расстояние
                    cl = clusarr[0];
                    for (int n = 0; n < clusarr.Count; n++)
                    {
                        double tmp = Math.Abs(clusarr[n].current_pixel.R - res[0, i, j])
                        + Math.Abs(clusarr[n].current_pixel.G - res[1, i, j])
                        + Math.Abs(clusarr[n].current_pixel.B - res[2, i, j]);
                        if (min > tmp)
                        {
                            min = tmp;
                            cl = clusarr[n];
                        }
                    }
                    pixel = new Img_pixel(i, j, res[0, i, j], res[1, i, j], res[2, i, j]);
                    if (min <= 3 * T)
                        cl.scores.Add(pixel);
                    else
                    {
                        clusarr.Add(new Cluster());
                        clusarr[clusarr.Count - 1].current_pixel = new Img_pixel(i, j, res[0, i, j], res[1, i, j], res[2, i, j]);
                        clusarr[clusarr.Count - 1].scores.Add(pixel);
                    }
                }
            }
            return clusarr.Count;
        }
    }
}
