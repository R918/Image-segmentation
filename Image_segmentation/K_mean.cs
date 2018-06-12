using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Image_segmentation
{
    class K_mean : Cluster
    {
        public static void InitializeCentroidsRGB(List<K_mean> clusarr, byte[,,] res, bool Initialize_random)//  инициализация центройдов для цветного изображения
        {
            int H = res.GetUpperBound(1) + 1;
            int W = res.GetUpperBound(2) + 1;
            int stepX;
            int stepY;
            if (Initialize_random)
            {
                stepX = 0;
                stepY = 0;
                Random rand = new Random();

                for (int i = 0; i < clusarr.Count; i++)
                {
                    stepX = rand.Next(0, H);
                    stepY = rand.Next(0, W);
                    clusarr[i].current_pixel.R = res[0, stepX, stepY];
                    clusarr[i].current_pixel.G = res[1, stepX, stepY];
                    clusarr[i].current_pixel.B = res[2, stepX, stepY];
                    //clusarr[i].current_pixel.X = stepX;
                    //clusarr[i].current_pixel.Y = stepY;
                }
            }
            else
            {
                stepX = 0;
                stepY = 0;
                int sX = H / clusarr.Count;
                int sY = W / clusarr.Count;
                for (int i = 0; i < clusarr.Count; i++)
                {
                    clusarr[i].current_pixel.R = res[0, stepX, stepY];
                    clusarr[i].current_pixel.G = res[1, stepX, stepY];
                    clusarr[i].current_pixel.B = res[2, stepX, stepY];
                    //clusarr[i].current_pixel.X = stepX;
                    //clusarr[i].current_pixel.Y = stepY;
                    stepX += sX;
                    stepY += sY;
                }
            }
        }
        public static void SetCenterRGB(List<K_mean> clusarr)// вычисление центра кластера
        {
            for (int j = 0; j < clusarr.Count; j++)
            {
                int sumX = 0, sumY = 0, sumR = 0, sumG = 0, sumB = 0;
                if (clusarr[j].scores.Count == 0)
                    return;
                int size = clusarr[j].scores.Count;
                for (int i = 0; i < size; i++)
                {
                    //sumX += clusarr[j].scores[i].X;//the centers of mass by x
                    //sumY += clusarr[j].scores[i].Y;//the centers of mass by y
                    sumR += clusarr[j].scores[i].R;//the centers of mass by red
                    sumG += clusarr[j].scores[i].G;//the centers of mass by green
                    sumB += clusarr[j].scores[i].B;//the centers of mass by blue
                }
                //clusarr[j].last_pixel.X = clusarr[j].current_pixel.X;
                //clusarr[j].last_pixel.Y = clusarr[j].current_pixel.Y;
                clusarr[j].last_pixel.R = clusarr[j].current_pixel.R;
                clusarr[j].last_pixel.G = clusarr[j].current_pixel.G;
                clusarr[j].last_pixel.B = clusarr[j].current_pixel.B;
                //clusarr[j].current_pixel.X = (sumX / size);
                //clusarr[j].current_pixel.Y = (sumY / size);
                clusarr[j].current_pixel.R = (byte)(sumR / size);
                clusarr[j].current_pixel.G = (byte)(sumG / size);
                clusarr[j].current_pixel.B = (byte)(sumB / size);
            }
        }
        public static void BindRGB(List<K_mean> clusarr, byte[,,] res)//связывание пикселей с кластерами по значениям RGB
        {
            int k = clusarr.Count;
            int H = res.GetUpperBound(1) + 1;
            int W = res.GetUpperBound(2) + 1;
            K_mean cl = new K_mean();
            for (int j = 0; j < k; j++)// очищаем привязанные к кластеру пиксели
                clusarr[j].scores.Clear();
            for (int i = 0; i < H; i++) // строки
            {
                for (int j = 0; j < W; j++) // столбцы
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
                    Img_pixel pt = new Img_pixel(i, j, res[0, i, j], res[1, i, j], res[2, i, j]);
                    cl.scores.Add(pt); // привязываем пиксель к кластеру к наиблежайшему кластеру
                }
            }
        }
        public static void printRGB(List<K_mean> clusarr, byte[,,] res, bool Marker_color)// Марикровка по среднему значению цвета кластера 
        {
            int k = clusarr.Count;
            int xre = 0;
            int yre = 0;
            if (Marker_color)
            {
                //Img_pixel[] Colors = new Img_pixel[10];
                //for (int i = 0; i < Colors.Length; i++)
                //    Colors[i] = new Img_pixel();
                //Colors[0].R = 42; Colors[0].G = 82; Colors[0].B = 190; // красный
                //Colors[1].R = 196; Colors[1].G = 30; Colors[1].B = 58; // синий
                //Colors[2].R = 255; Colors[2].G = 186; Colors[2].B = 0; // желтый
                //Colors[3].R = 3; Colors[3].G = 192; Colors[3].B = 60; // зеленый
                //Colors[4].R = 0; Colors[4].G = 191; Colors[4].B = 255;// голубой
                //Colors[5].R = 128; Colors[5].G = 0; Colors[5].B = 128; // фиолетовый
                //Colors[6].R = 255; Colors[6].G = 55; Colors[6].B = 105; // розовый
                //Colors[7].R = 172; Colors[7].G = 183; Colors[7].B = 142;// салатовый
                //Colors[8].R = 115; Colors[8].G = 74; Colors[8].B = 18;// коричневый
                //Colors[9].R = 0; Colors[9].G = 0; Colors[9].B = 0;// черный
                //Colors[10].R = 255; Colors[10].G = 255; Colors[10].B = 255;// белый
                //Colors[11].R = 217; Colors[11].G = 247; Colors[11].B = 255;  // светло-голубой
                int[] cols = new int[]
                    {
                            0xff, 0xff8080, 0xffff, 0xff00, 0xffff80, 0x80ff, 0xff80ff, 0xc0c0c0, 0xffff00, 0xff0000, 0xff00ff, 0x808080, 0xc0, 0x40c0, 0xc0c0,
                            0xffffff, 0xc0c0ff, 0xc0e0ff, 0xc0ffff, 0xc0ffc0, 0xffffc0, 0xffc0c0, 0xffc0ff, 0xe0e0e0, 0x8080ff, 0x80c0ff, 0x80ffff, 0x80ff80,
                            0xc000, 0xc0c000, 0xc00000, 0xc000c0, 0x404040, 0x80, 0x4080, 0x8080, 0x8000, 0x808000, 0x800000, 0x800080, 0,
                            0x40, 0x404080, 0x4040, 0x4000, 0x404000, 0x400000, 0x400040
                    };
                Color[] colors = new Color[0x30];
                for (int i = 0; i < cols.Length; i++)
                {
                    colors[i] = ColorTranslator.FromOle(cols[i]);
                }

                for (int p = 0; p < k; p++)
                {
                    for (int t = 0; t < clusarr[p].scores.Count; t++)
                    {
                        xre = clusarr[p].scores[t].X;
                        yre = clusarr[p].scores[t].Y;
                        res[0, xre, yre] = colors[p].R;
                        res[1, xre, yre] = colors[p].G;
                        res[2, xre, yre] = colors[p].B;
                    }
                    clusarr[p].current_pixel.R = colors[p].R;
                    clusarr[p].current_pixel.G = colors[p].G;
                    clusarr[p].current_pixel.B = colors[p].B;
                }
                
            }
            else
            {
                for (int p = 0; p < k; p++)
                {
                    for (int t = 0; t < clusarr[p].scores.Count; t++)
                    {
                        xre = clusarr[p].scores[t].X;
                        yre = clusarr[p].scores[t].Y;
                        res[0, xre, yre] = clusarr[p].current_pixel.R;
                        res[1, xre, yre] = clusarr[p].current_pixel.G;
                        res[2, xre, yre] = clusarr[p].current_pixel.B;
                    }
                }
            }
        }
        public static void StartRGB(List<K_mean> clusarr, byte[,,] res, bool Initialize_random, bool Marker_color)// начать кластеризацию для цветного изображения 
        {
            int k = clusarr.Count;
            int H = res.GetUpperBound(1) + 1;
            int W = res.GetUpperBound(2) + 1;
            InitializeCentroidsRGB(clusarr, res, Initialize_random);

            int chk = 0;

            while (chk < k)
            {
                chk = 0;
                BindRGB(clusarr, res);
                SetCenterRGB(clusarr);//Высчитываем новые координаты центроидов 

                for (int p = 0; p < k; p++)//Проверяем не совпадают ли они с предыдущими цент-ми
                    if (Math.Abs(clusarr[p].current_pixel.R - clusarr[p].last_pixel.R) < 5 &&
                        Math.Abs(clusarr[p].current_pixel.G - clusarr[p].last_pixel.G) < 5 &&
                        Math.Abs(clusarr[p].current_pixel.B - clusarr[p].last_pixel.B) < 5)
                    {
                        chk++;
                    }

            }
            printRGB(clusarr, res, Marker_color);
        }
    }
}
