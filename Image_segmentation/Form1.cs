using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;


namespace Image_segmentation
{
    public partial class Form1 : Form
    {
        Image img1;
        Image img2;
        Bitmap bit1;
        static Bitmap bit2;
        Stopwatch sw = new Stopwatch();
        public bool is_Gray = false; // Gray == true, RGB = false
        public bool Initialize_Random = false;// 
        public bool Initialize_Random_Simple = false;
        public bool Initialize_Random_Maxmin = false;
        public bool Mark_color = false;// разноцветная маркировка
        public bool Mark_color_Simple = false;
        public bool Mark_color_Maxmin = false;
        public bool Otsu_Gray = true;
        static Stopwatch t1 = new Stopwatch();



        int[] cols = new int[]
        {
                            0xff, 0xff8080, 0xffff, 0xff00, 0xffff80, 0x80ff, 0xff80ff, 0xc0c0c0, 0xffff00, 0xff0000, 0xff00ff, 0x808080, 0xc0, 0x40c0, 0xc0c0,
                            0xffffff, 0xc0c0ff, 0xc0e0ff, 0xc0ffff, 0xc0ffc0, 0xffffc0, 0xffc0c0, 0xffc0ff, 0xe0e0e0, 0x8080ff, 0x80c0ff, 0x80ffff, 0x80ff80,
                            0xc000, 0xc0c000, 0xc00000, 0xc000c0, 0x404040, 0x80, 0x4080, 0x8080, 0x8000, 0x808000, 0x800000, 0x800080, 0,
                            0x40, 0x404080, 0x4040, 0x4000, 0x404000, 0x400000, 0x400040
        };
        Color[] colors = new Color[0x30];


        public Form1()
        {
            InitializeComponent();
            for (int i = 0; i < cols.Length; i++)
            {
                colors[i] = ColorTranslator.FromOle(cols[i]);
            }
        }

        private void ОткрытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog opendialog = new OpenFileDialog();
            opendialog.Filter = "Файлы изображений|*.bmp; *.png; *.jpg; *jpeg";
            if (opendialog.ShowDialog() != DialogResult.OK)
                return;
            try
            {
                sw.Reset();

                pictureBox1.Image = Image.FromFile(opendialog.FileName);

                img1 = Image.FromFile(opendialog.FileName);
                bit1 = new Bitmap(img1);

            }
            catch (OutOfMemoryException ex)
            {
                MessageBox.Show("Ошибка при загрузке изображения");
            }
            return;
        }
        private void СохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (bit2 == null)
                {
                    MessageBox.Show("Изображение не загружено");
                    return;
                }
                saveFileDialog1.InitialDirectory = "C:";

                saveFileDialog1.Filter = " bmp files (*.bmp)|*.bmp|All files|*.*";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {

                    string fileName = saveFileDialog1.FileName;
                    Bitmap bmp = bit2;
                    using (var gr = Graphics.FromImage(bmp))
                    {
                        bmp.Save(fileName, ImageFormat.Bmp);
                    }

                }
            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show("Изображение не загружено");
            }
        }
        public unsafe static Bitmap RgbToBitmap(byte[,,] rgb)
        {
            if ((rgb.GetLength(0) != 3))
            {
                throw new ArrayTypeMismatchException("Size of first dimension for passed array must be 3 (RGB components)");
            }

            int width = rgb.GetLength(2),
                height = rgb.GetLength(1);

            Bitmap result = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            BitmapData bd = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);

            try
            {
                byte* curpos;
                for (int h = 0; h < height; h++)
                {
                    curpos = ((byte*)bd.Scan0) + h * bd.Stride;
                    for (int w = 0; w < width; w++)
                    {
                        *(curpos++) = rgb[2, h, w];
                        *(curpos++) = rgb[1, h, w];
                        *(curpos++) = rgb[0, h, w];
                    }
                }
            }
            finally
            {
                result.UnlockBits(bd);
            }

            return result;
        }
        public unsafe static byte[,,] BitmapToByteRgb(Bitmap bmp)
        {
            int width = bmp.Width,
                height = bmp.Height;
            byte[,,] res = new byte[3, height, width];
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);
            try
            {
                byte* curpos;
                for (int h = 0; h < height; h++)
                {
                    curpos = ((byte*)bd.Scan0) + h * bd.Stride;
                    for (int w = 0; w < width; w++)
                    {
                        res[2, h, w] = *(curpos++);
                        res[1, h, w] = *(curpos++);
                        res[0, h, w] = *(curpos++);
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }
            return res;
        }
        public static byte[,,] ToGray(Image img)
        {
            bit2 = new Bitmap(img);
            byte a, b, c;
            a = 3;
            byte[,,] res = BitmapToByteRgb(bit2);
            for (int x = 0; x < bit2.Height; x++)
            {
                for (int y = 0; y < bit2.Width; y++)
                {
                    a = res[0, x, y];
                    b = res[1, x, y];
                    c = res[2, x, y];
                    res[0, x, y] = (byte)((a + b + c) / (byte)3);
                    res[1, x, y] = (byte)((a + b + c) / (byte)3);
                    res[2, x, y] = (byte)((a + b + c) / (byte)3);
                }
            }
            return res;
        }// перевод в серое
        public static byte[,,] ToGrayHumanSense(Image img) // перевод в серое c учетом человеческого восприятия
        {
            bit2 = new Bitmap(img);

            ////////////

            byte a, b, c;
            double d;
            a = 3;
            byte[,,] res = BitmapToByteRgb(bit2);
            for (int x = 0; x < bit2.Height; x++)
            {
                for (int y = 0; y < bit2.Width; y++)
                {
                    a = res[0, x, y];
                    b = res[1, x, y];
                    c = res[2, x, y];
                    res[0, x, y] = (byte)(a * 0.2126 + b * 0.7152 + c * 0.0722);
                    res[1, x, y] = (byte)(a * 0.2126 + b * 0.7152 + c * 0.0722);
                    res[2, x, y] = (byte)(a * 0.2126 + b * 0.7152 + c * 0.0722);
                }
            }
            return res;
        }
        private static unsafe void OtsuMethod(byte[,,] res, List<Img_pixel>[] Storage, List<int> spans, int Diff)
        {
            int bright = 0;
            int height = res.GetUpperBound(1) + 1;
            int width = res.GetUpperBound(2) + 1;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    bright = res[0, i, j];
                    Storage[bright].Add(new Img_pixel(i, j, res[0, i, j], res[1, i, j], res[2, i, j]));
                }
            }
            while (true)
            {
                for (int i = 0; i < spans.Count - 1; i++)
                {
                    if (spans[i + 1] - spans[i] <= Diff)
                        return;
                }
                for (int st = 0; st < spans.Count; st++)
                {
                    if (spans[st] == 255)
                    {
                        spans.Sort();
                        break;
                    }
                    int p1 = spans[st], p2 = spans[st + 1];
                    // Введем два вспомогательных числа:
                    int m = 0; // m - сумма высот всех бинов, домноженных на положение их середины
                    int n = 0; // n - сумма высот всех бинов
                    for (int t = p1; t <= p2; t++)
                    {
                        m += t * Storage[t].Count;
                        n += Storage[t].Count;
                    }
                    float maxSigma = -1; // Максимальное значение межклассовой дисперсии
                    int threshold = 0; // Порог, соответствующий maxSigma

                    int alpha1 = 0; // Сумма высот всех бинов для класса 1, домноженных на положение их середины
                    int beta1 = 0; // Сумма высот всех бинов для класса 1
                    // Переменная alpha2 не нужна, т.к. она равна m - alpha1
                    // Переменная beta2 не нужна, т.к. она равна n - alpha1
                    // t пробегается по всем возможным значениям порога
                    for (int t = p1; t <= p2; t++)
                    {
                        alpha1 += t * Storage[t].Count;
                        beta1 += Storage[t].Count;

                        // Считаем вероятность класса 1.
                        float w1 = (float)beta1 / n;
                        // Нетрудно догадаться, что w2 тоже не нужна, т.к. она равна 1 - w1

                        // a = a1 - a2, где a1, a2 - средние арифметические для классов 1 и 2
                        float a = (float)alpha1 / beta1 - (float)(m - alpha1) / (n - beta1);

                        // Наконец, считаем sigma
                        float sigma = w1 * (1 - w1) * a * a;

                        // Если sigma больше текущей максимальной, то обновляем maxSigma и порог
                        if (sigma > maxSigma)
                        {
                            maxSigma = sigma;
                            threshold = t;
                        }
                    }
                    spans.Add(threshold);
                }
            }
        }
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label1.Text = "Количесво кластеров: " + trackBar1.Value;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Invalidate();
            Update();
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Изображение не загружено");
                return;
            }
            if (t1.IsRunning)
            {
                MessageBox.Show("Подождите завершения обработки изображения");
                return;
            }
            
            Thread KmeanThread = new Thread(new ThreadStart(Kmean_segmentation));
            KmeanThread.Name = "Kmean_Thread";
            
            KmeanThread.Start();

        }
        public void Kmean_segmentation()
        {
            int k =0;
            this.trackBar1.BeginInvoke((MethodInvoker)(() => k = this.trackBar1.Value));
            t1.Start();
            byte[,,] res;
            if (is_Gray)
                res = ToGray(img1);
            else
            {
                bit2 = new Bitmap(img1);
                res = BitmapToByteRgb(bit2);
            }
            List<K_mean> clusarr = new List<K_mean>(k);
            for (int i = 0; i < clusarr.Capacity; i++)
            {
                clusarr.Add(new K_mean());
                clusarr[i].scores = new List<Img_pixel>();
                clusarr[i].current_pixel = new Img_pixel();
                clusarr[i].last_pixel = new Img_pixel();
            }
            //Start   
            K_mean.StartRGB(clusarr, res, Initialize_Random, Mark_color);
            t1.Stop();
            this.label3.BeginInvoke((MethodInvoker)(() => this.label3.Text = "Время выполнения: " + (Convert.ToString(Convert.ToDouble(t1.ElapsedMilliseconds) / 1000)) + " сек."));

            bit2 = RgbToBitmap(res);
            pictureBox2.Image = bit2;
            Graphics g = this.CreateGraphics();
            Font drawFont = new Font("Arial", 10);
            int x1 = 950;
            int y1 = 400;
            int space = 560;
            for (int i = 0; i < k; i++)
            {
                SolidBrush drawBrush = new SolidBrush(Color.FromArgb(clusarr[i].current_pixel.R, clusarr[i].current_pixel.G, clusarr[i].current_pixel.B));
                g.FillRectangle(drawBrush, x1, y1, space/k, 20);
                g.DrawString(Convert.ToString(i + 1), drawFont, new SolidBrush(Color.Black), x1 + space / (2*k), y1 + 23);
                x1 += space / k;
                if (x1 >= 1500)
                {
                    x1 = 950;
                    y1 += 20;
                }
            }

            t1.Reset();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Invalidate();
            Update();
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Изображение не загружено");
                return;
            }
            try
            {

                int Diff = Convert.ToInt32(numericUpDown1.Value);
                byte[,,] res;
                Stopwatch t1 = new Stopwatch();
                t1.Start();
                if (Otsu_Gray)
                    res = ToGray(img1);
                else
                    res = ToGray(img1);
                List<Img_pixel>[] Storage = new List<Img_pixel>[256];
                for (int i = 0; i < Storage.Length; i++)
                {
                    Storage[i] = new List<Img_pixel>();
                }
                List<int> spans = new List<int>();
                spans.Add(0);
                spans.Add(255);
                OtsuMethod(res, Storage, spans, Diff);
                t1.Stop();
                int tempX;
                int tempY;
                if (Otsu_Gray)
                {
                    for (int i = 0; i < spans.Count - 1; i++)
                    {
                        for (int j = spans[i]; j < spans[i + 1]; j++)
                        {
                            for (int n = 0; n < Storage[j].Count; n++)
                            {
                                tempX = Storage[j][n].X;
                                tempY = Storage[j][n].Y;
                                res[0, tempX, tempY] = (byte)(spans[i + 1]);
                                res[1, tempX, tempY] = (byte)(spans[i + 1]);
                                res[2, tempX, tempY] = (byte)(spans[i + 1]);
                            }
                        }
                    }
                }
                else
                {

                    for (int i = 0; i < spans.Count - 1; i++)
                    {
                        for (int j = spans[i]; j < spans[i + 1]; j++)
                        {
                            for (int n = 0; n < Storage[j].Count; n++)
                            {

                                tempX = Storage[j][n].X;
                                tempY = Storage[j][n].Y;
                                res[0, tempX, tempY] = colors[i].R;
                                res[1, tempX, tempY] = colors[i].G;
                                res[2, tempX, tempY] = colors[i].B;
                            }
                        }
                    }
                    Graphics g = this.CreateGraphics();
                    Font drawFont = new Font("Arial", 10);
                    int x1 = 950;
                    int y1 = 400;
                    int space = 560;
                    for (int i = 0; i < spans.Count - 1; i++)
                    {
                        SolidBrush drawBrush = new SolidBrush(colors[i]);
                        g.FillRectangle(drawBrush, x1, y1, space/ (spans.Count - 1), 20);
                        g.DrawString(Convert.ToString(i + 1), drawFont, new SolidBrush(Color.Black), x1 + (space / (spans.Count - 1))/2, y1 + 23);
                        x1 += space / (spans.Count - 1);
                        if (x1 >= 1500)
                        {
                            x1 = 950;
                            y1 += 40;
                        }
                    }
                }

                label3.Text = "Время выполнения: " + (Convert.ToString(Convert.ToDouble(t1.ElapsedMilliseconds) / 1000)) + " сек.";
                label4.Text = "Количество кластеров: " + (Convert.ToString(spans.Count - 1));
                t1.Reset();
                bit2 = RgbToBitmap(res);
                pictureBox2.Image = bit2;


            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show("Изображение не загружено");
            }
        }
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            is_Gray = true;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            is_Gray = false;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            Initialize_Random = false;
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            Initialize_Random = true;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            Mark_color = false;
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            Mark_color = true;
        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            Otsu_Gray = true;
        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            Otsu_Gray = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                Stopwatch t1 = new Stopwatch();
                t1.Start();
                bit2 = new Bitmap(img1);
                byte[,,] res = BitmapToByteRgb(bit2);
                int Height = res.GetUpperBound(1) + 1;
                int Width = res.GetUpperBound(2) + 1;
                List<Cluster> clusarr = new List<Cluster>();
                clusarr.Add(new Cluster());
                Random rand = new Random();
                if (Initialize_Random_Simple)
                {
                    int X = rand.Next(0, Height);
                    int Y = rand.Next(0, Width);
                    clusarr[0].current_pixel = new Img_pixel(X, Y, res[0, X, Y], res[1, X, Y], res[2, X, Y]);
                }
                else
                    clusarr[0].current_pixel = new Img_pixel(0, 0, res[0, 0, 0], res[1, 0, 0], res[2, 0, 0]);
                int T = Convert.ToInt32(numericUpDown2.Value);
                int k = Simple.Start(clusarr, res, T, Mark_color_Simple);
                int xre = 0;
                int yre = 0;
                Invalidate();
                Update();
                Graphics g = this.CreateGraphics();
                Font drawFont = new Font("Arial", 10);
                int x1 = 950;
                int y1 = 400;
                int space = 560;
                if (Mark_color_Simple)
                {
                    for (int p = 0; p < clusarr.Count; p++)
                    {
                        for (int t = 0; t < clusarr[p].scores.Count; t++)
                        {
                            xre = clusarr[p].scores[t].X;
                            yre = clusarr[p].scores[t].Y;
                            res[0, xre, yre] = colors[p].R;
                            res[1, xre, yre] = colors[p].G;
                            res[2, xre, yre] = colors[p].B;
                        }
                    }

                    for (int i = 0; i < k; i++)
                    {
                        SolidBrush drawBrush = new SolidBrush(colors[i]);
                        g.FillRectangle(drawBrush, x1, y1, space / k, 20);
                        g.DrawString(Convert.ToString(i + 1), drawFont, new SolidBrush(Color.Black), x1 + space / (2 * k), y1 + 23);
                        x1 += space / k;
                        if (x1 >= 1500)
                        {
                            x1 = 950;
                            y1 += 20;
                        }
                    }
                }
                else
                {
                    for (int p = 0; p < clusarr.Count; p++)
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

                    for (int i = 0; i < k; i++)
                    {
                        SolidBrush drawBrush = new SolidBrush(Color.FromArgb(clusarr[i].current_pixel.R, clusarr[i].current_pixel.G, clusarr[i].current_pixel.B));
                        g.FillRectangle(drawBrush, x1, y1, space / k, 20);
                        g.DrawString(Convert.ToString(i + 1), drawFont, new SolidBrush(Color.Black), x1 + space / (2 * k), y1 + 23);
                        x1 += space / k;
                        if (x1 >= 1500)
                        {
                            x1 = 950;
                            y1 += 20;
                        }
                    }
                }
                t1.Stop();
                label3.Text = "Время выполнения: " + (Convert.ToString(Convert.ToDouble(t1.ElapsedMilliseconds) / 1000)) + " сек.";
                label6.Text = "Количество кластеров:" + (Convert.ToString(k));
                t1.Reset();
                bit2 = RgbToBitmap(res);
                pictureBox2.Image = bit2;

            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show("Изображение не загружено");
            }
        }

        private void tabPage4_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsControl(e.KeyChar))
            {
                // <Enter>, <Backspace>, <Esc>
                if (e.KeyChar == (char)Keys.Enter)
                    // нажата клавиша <Enter>
                    // установить курсор на кнопку OK
                    button3.Focus();
                return;
            }

        }

        private void tabControl1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsControl(e.KeyChar))
            {
                // <Enter>, <Backspace>, <Esc>
                if (e.KeyChar == (char)Keys.Enter)
                    // нажата клавиша <Enter>
                    // установить курсор на кнопку OK
                    button1.Focus();
                return;
            }

        }

        private void tabPage4_Enter(object sender, EventArgs e)
        {
            button3.Focus();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Invalidate();
            Update();
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Изображение не загружено");
                return;
            }
            if (t1.IsRunning)
            {
                MessageBox.Show("Подождите завершения обработки изображения");
                return;
            }
            Thread Thread_Max_min = new Thread(Max_min_segmentation);
            Thread_Max_min.Start();
        }
        public void Max_min_segmentation()
        {
            t1.Start();
            bit2 = new Bitmap(img1);
            byte[,,] res = BitmapToByteRgb(bit2);
            List<Cluster> clusarr = new List<Cluster>();
            clusarr.Add(new Cluster());
            int k = Max_min.Start(clusarr, res, Initialize_Random_Maxmin);
            t1.Stop();
            this.label3.BeginInvoke((MethodInvoker)(() => this.label3.Text = "Время выполнения: " + (Convert.ToString(Convert.ToDouble(t1.ElapsedMilliseconds) / 1000)) + " сек."));
            this.label5.BeginInvoke((MethodInvoker)(() => this.label5.Text = "Количество кластеров:" + (Convert.ToString(k))));
            Graphics g = this.CreateGraphics();
            Font drawFont = new Font("Arial", 10);
            int x1 = 950;
            int y1 = 400;
            int space = 560;
            if (Mark_color_Maxmin)
            {
                int xre = 0;
                int yre = 0;
                for (int p = 0; p < clusarr.Count; p++)
                {
                    for (int t = 0; t < clusarr[p].scores.Count; t++)
                    {
                        xre = clusarr[p].scores[t].X;
                        yre = clusarr[p].scores[t].Y;
                        res[0, xre, yre] = colors[p].R;
                        res[1, xre, yre] = colors[p].G;
                        res[2, xre, yre] = colors[p].B;
                    }
                }
                for (int i = 0; i < k; i++)
                {
                    SolidBrush drawBrush = new SolidBrush(colors[i]);
                    g.FillRectangle(drawBrush, x1, y1, space / k, 20);
                    g.DrawString(Convert.ToString(i + 1), drawFont, new SolidBrush(Color.Black), x1 + space / (2 * k), y1 + 23);
                    x1 += space / k;
                    if (x1 >= 1500)
                    {
                        x1 = 950;
                        y1 += 20;
                    }
                }
            }
            else
            {
                int xre = 0;
                int yre = 0;
                for (int p = 0; p < clusarr.Count; p++)
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
                for (int i = 0; i < k; i++)
                {
                    SolidBrush drawBrush = new SolidBrush(Color.FromArgb(clusarr[i].current_pixel.R, clusarr[i].current_pixel.G, clusarr[i].current_pixel.B));
                    g.FillRectangle(drawBrush, x1, y1, space / k, 20);
                    g.DrawString(Convert.ToString(i + 1), drawFont, new SolidBrush(Color.Black), x1 + (space / k)/2, y1 + 23);
                    x1 += space / k;
                    if (x1 >= 1500)
                    {
                        x1 = 950;
                        y1 += 20;
                    }
                }
            }
            bit2 = RgbToBitmap(res);
            pictureBox2.Image = bit2;
            t1.Reset();
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }
        //aglomerative clustering works only on very small resolusion images ~(56x56)
        //private void button5_Click(object sender, EventArgs e)
        //{
        //    int k = Convert.ToInt32(numericUpDown3.Value);
        //    Thread aglomerative_thread = new Thread(new ParameterizedThreadStart(Aglomerative_algorithm));
        //    aglomerative_thread.Start(k);

        //}
        //public void Aglomerative_algorithm(object z)
        //{
        //    int k = (int) z;
        //    bit2 = new Bitmap(img1);
        //    byte[,,] res = BitmapToByteRgb(bit2);
        //    t1.Start();
        //    int H = res.GetUpperBound(1) + 1;
        //    int W = res.GetUpperBound(2) + 1;
        //    int x = 0, y = 0;
        //    List<Cluster> clusarr = new List<Cluster>(H * W);
        //    for (int i = 0; i < clusarr.Capacity; i++)
        //    {
        //        clusarr.Add(new Cluster());
        //        clusarr[i].scores = new List<Img_pixel>();
        //        clusarr[i].scores.Add(new Img_pixel(x, y, res[0, x, y], res[1, x, y], res[2, x, y]));
        //        y++;
        //        if (y == W)
        //        {
        //            x++; y = 0;
        //        }
        //        clusarr[i].current_pixel = new Img_pixel();
        //        clusarr[i].last_pixel = new Img_pixel();
        //    }
        //    Cluster Cluster_a, Cluster_b;
        //    while (clusarr.Count > k)
        //    {
        //        Cluster_a = null;
        //        Cluster_b = null;
        //        int min_dist = Int32.MaxValue;
        //        int dist = 0, delete = 0;
        //        for (int i = 0; i < clusarr.Count - 1; i++)
        //        {
        //            for (int j = i + 1; j < clusarr.Count; j++)
        //            {
        //                dist = Intercluster_distance(clusarr[i], clusarr[j]);
        //                if (dist < min_dist)
        //                {
        //                    min_dist = dist;
        //                    Cluster_a = clusarr[i];
        //                    Cluster_b = clusarr[j];
        //                    delete = j;
        //                }
        //                if (min_dist == 0)
        //                    break;
        //            }
        //            if (min_dist == 0)
        //                break;
        //        }
        //        Cluster_a.scores.AddRange(Cluster_b.scores);
        //        clusarr.RemoveAt(delete);
        //    }
        //    for (int j = 0; j < clusarr.Count; j++)
        //    {
        //        int sumR = 0, sumG = 0, sumB = 0;
        //        if (clusarr[j].scores.Count == 0)
        //            continue;
        //        int size = clusarr[j].scores.Count;
        //        for (int i = 0; i < size; i++)
        //        {
        //            sumR += clusarr[j].scores[i].R;//the centers of mass by red
        //            sumG += clusarr[j].scores[i].G;//the centers of mass by green
        //            sumB += clusarr[j].scores[i].B;//the centers of mass by blue
        //        }
        //        clusarr[j].current_pixel.R = (byte)(sumR / size);
        //        clusarr[j].current_pixel.G = (byte)(sumG / size);
        //        clusarr[j].current_pixel.B = (byte)(sumB / size);
        //    }
        //    int xre = 0;
        //    int yre = 0;
        //    for (int p = 0; p < k; p++)
        //    {
        //        for (int t = 0; t < clusarr[p].scores.Count; t++)
        //        {
        //            xre = clusarr[p].scores[t].X;
        //            yre = clusarr[p].scores[t].Y;
        //            res[0, xre, yre] = clusarr[p].current_pixel.R;
        //            res[1, xre, yre] = clusarr[p].current_pixel.G;
        //            res[2, xre, yre] = clusarr[p].current_pixel.B;
        //        }
        //    }
        //    t1.Stop();
        //    label3.Text = "Время выполнения: " + (Convert.ToString(Convert.ToDouble(t1.ElapsedMilliseconds) / 1000)) + " сек.";
        //    t1.Reset();
        //    bit2 = RgbToBitmap(res);
        //    pictureBox2.Image = bit2;
        //}

        //int centroids_distance(Cluster a, Cluster b)
        //{
        //    int distance = 0;
        //    int sumR = 0, sumG = 0, sumB = 0;
        //    if (a.scores.Count == 1)
        //    {
        //        a.current_pixel.R = a.scores[0].R;
        //        a.current_pixel.G = a.scores[0].G;
        //        a.current_pixel.B = a.scores[0].B;
        //    }
        //    else
        //    {
        //        for (int i = 0; i < a.scores.Count; i++)
        //        {
        //            sumR += a.scores[i].R;
        //            sumG += a.scores[i].G;
        //            sumB += a.scores[i].B;
        //        }
        //        a.current_pixel.R = (byte)(sumR / a.scores.Count);
        //        a.current_pixel.G = (byte)(sumG / a.scores.Count);
        //        a.current_pixel.B = (byte)(sumB / a.scores.Count);
        //    }
        //    if (b.scores.Count == 1)
        //    {
        //        b.current_pixel.R = b.scores[0].R;
        //        b.current_pixel.G = b.scores[0].G;
        //        b.current_pixel.B = b.scores[0].B;
        //    }
        //    else
        //    {
        //        for (int i = 0; i < b.scores.Count; i++)
        //        {
        //            sumR += b.scores[i].R;
        //            sumG += b.scores[i].G;
        //            sumB += b.scores[i].B;
        //        }
        //        b.current_pixel.R = (byte)(sumR / b.scores.Count);
        //        b.current_pixel.G = (byte)(sumG / b.scores.Count);
        //        b.current_pixel.B = (byte)(sumB / b.scores.Count);
        //    }
        //    distance = Math.Abs(a.current_pixel.R - b.current_pixel.R) + Math.Abs(a.current_pixel.G - b.current_pixel.G) + Math.Abs(a.current_pixel.B - b.current_pixel.B);

        //    return distance;
        //}

        //int Intercluster_distance(Cluster a, Cluster b)
        //{
        //    int distance = 0;
        //    int min_distance = Int32.MaxValue;
        //    for (int i = 0; i < a.scores.Count; i++)
        //    {
        //        for (int j = 0; j < b.scores.Count; j++)
        //        {
        //            distance = Math.Abs(a.scores[i].R - b.scores[j].R) + Math.Abs(a.scores[i].G - b.scores[j].G) + Math.Abs(a.scores[i].B - b.scores[j].B);
        //            if (distance < min_distance)
        //            {
        //                min_distance = distance;
        //            }
        //        }
        //    }
        //    return distance;
        //}

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Mark_color_Simple = true;
        }

        private void radioButton10_CheckedChanged(object sender, EventArgs e)
        {
            Mark_color_Simple = true;
        }

        private void radioButton9_CheckedChanged(object sender, EventArgs e)
        {
            Mark_color_Simple = false;

        }

        private void radioButton11_CheckedChanged(object sender, EventArgs e)
        {
            Initialize_Random_Simple = false;
        }

        private void radioButton12_CheckedChanged(object sender, EventArgs e)
        {
            Initialize_Random_Simple = true;
        }

        private void radioButton13_CheckedChanged(object sender, EventArgs e)
        {
            Initialize_Random_Maxmin = false;
        }

        private void radioButton14_CheckedChanged(object sender, EventArgs e)
        {
            Initialize_Random_Maxmin = true;
        }

        private void radioButton15_CheckedChanged(object sender, EventArgs e)
        {
            Mark_color_Maxmin = false;
        }

        private void radioButton16_CheckedChanged(object sender, EventArgs e)
        {
            Mark_color_Maxmin = true;
        }
    }
}
