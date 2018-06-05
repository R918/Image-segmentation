using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image_segmentation
{
    class Max_min
    {
        public static int Start(List<Cluster> clusarr, byte[,,] res, bool Initialize_Random_Maxmin)
        {
            int Height = res.GetUpperBound(1) + 1;
            int Width = res.GetUpperBound(2) + 1;
            double average = 0;
            double Max = 0;
            if (Initialize_Random_Maxmin)
            {
                Random rand = new Random();
                int X = rand.Next(0, Height);
                int Y = rand.Next(0, Width);
                clusarr[0].current_pixel = new Img_pixel(X, Y, res[0, X, Y], res[1, X, Y], res[2, X, Y]);
            }
            else
                clusarr[0].current_pixel = new Img_pixel(0, 0, res[0, 0, 0], res[1, 0, 0], res[2, 0, 0]);
            Img_pixel pixel = new Img_pixel();
            double first_max = 0;
            Img_pixel farest_pixel = new Img_pixel();
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    double max = Math.Abs(clusarr[0].current_pixel.R - res[0, i, j])
                        + Math.Abs(clusarr[0].current_pixel.G - res[1, i, j])
                        + Math.Abs(clusarr[0].current_pixel.B - res[2, i, j]);
                    if (max > first_max)
                    {
                        first_max = max;
                        farest_pixel = new Img_pixel(i, j, res[0, i, j], res[1, i, j], res[2, i, j]);
                    }
                }
            }
            clusarr.Add(new Cluster(farest_pixel));
            do
            {
                Max = 0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        double min = Math.Abs(clusarr[0].current_pixel.R - res[0, i, j])
                            + Math.Abs(clusarr[0].current_pixel.G - res[1, i, j])
                            + Math.Abs(clusarr[0].current_pixel.B - res[2, i, j]); // начальное минимальное расстояние
                        for (int n = 0; n < clusarr.Count; n++)
                        {
                            double tmp = Math.Abs(clusarr[n].current_pixel.R - res[0, i, j])
                            + Math.Abs(clusarr[n].current_pixel.G - res[1, i, j])
                            + Math.Abs(clusarr[n].current_pixel.B - res[2, i, j]);
                            if (min > tmp)
                            {
                                min = tmp;
                            }
                        }
                        if (min > Max)
                        {
                            Max = min;
                            pixel = new Img_pixel(i, j, res[0, i, j], res[1, i, j], res[2, i, j]);
                        }
                    }
                }
                average = 0;
                for (int n = 1; n < clusarr.Count; n++)
                {
                    average += Math.Abs(clusarr[0].current_pixel.R - clusarr[n].current_pixel.R)
                           + Math.Abs(clusarr[0].current_pixel.G - clusarr[n].current_pixel.G)
                           + Math.Abs(clusarr[0].current_pixel.B - clusarr[n].current_pixel.B);
                }
                average /= clusarr.Count;
                if (Max > average / 2)
                    clusarr.Add(new Cluster(pixel));
            } while (Max > average/2);
            Cluster cl = new Cluster();
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
                    cl.scores.Add(pixel);
                }
            }
            
            return clusarr.Count;
        }
    }
}
