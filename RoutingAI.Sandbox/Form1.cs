using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using libWyvernzora;
using RoutingAI.Algorithms.Clustering;
using RoutingAI.API.OSRM;
using System.Diagnostics;
using System.IO;
using RoutingAI.Utilities;

namespace RoutingAI.Sandbox
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region Clustering Demo

        const Int32 radius = 4;
        Coordinate[] coordinates = new Coordinate[0];
        IClusteringAlgorithm<Coordinate> processor = null;
        Color[] colors = new Color[]
        {
            Color.Black,
            Color.White,
            Color.Red,
            Color.Green,
            Color.Blue,
            Color.Yellow,
            Color.Green,
            Color.Purple,
            Color.DarkGoldenrod,
            Color.DarkOliveGreen,
            Color.Cyan,
            Color.Magenta,
            Color.Lime,
            Color.HotPink,
            Color.Coral,
            Color.Aqua,
            Color.Turquoise,
            Color.Tomato,
            Color.SaddleBrown,
            Color.Silver
        };

        private Point C2P(Coordinate c)
        {
            // convert x
            Int32 x = (Int32)(((c.lon + 100) / 200) * panel1.Width);
            Int32 y = (Int32)(((c.lat + 100) / 200) * panel1.Height);

            return new Point(x, y);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            processor = null;

            Int32 count = (int)numericUpDown1.Value;
            coordinates = new Coordinate[count];
            Random r = new Random();
            for (int i = 0; i < coordinates.Length; i++)
            {
                coordinates[i] = new Coordinate(r.Next(-10000, 10000) / 100.0f, r.Next(-10000, 10000) / 100.0f);
            }

            panel1.Invalidate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Random r = new Random();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            //processor = new KMedoidsProcessor<Coordinate>((int)numericUpDown2.Value, coordinates, new RoutingAI.Algorithms.StraightDistanceAlgorithm());
            processor = new PAMClusteringAlgorithm<Coordinate>(coordinates, (int)numericUpDown2.Value, new RoutingAI.Algorithms.StraightDistanceAlgorithm());
            processor.Run();
            sw.Stop();
            lblTime.Text = String.Format("Time Elapsed: {0}ms", sw.ElapsedMilliseconds);
            panel1.Invalidate();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            Random rand = new Random();

            Pen border = new Pen(new SolidBrush(Color.Black), 1);
            Pen cborder = new Pen(new SolidBrush(Color.Black), 2);

            foreach (Coordinate c in coordinates)
            {
                Point p = C2P(c);
                if (processor != null)
                {
                    Int32 assignment = processor.GetClusterIndex(c);
                    if (assignment > 0)
                        g.FillEllipse(new SolidBrush(colors[assignment % colors.Length]), p.X - radius, p.Y - radius, radius * 2, radius * 2);
                }
                g.DrawEllipse(border, p.X - radius, p.Y - radius, radius * 2, radius * 2);
            }

            if (processor != null)
            {
                foreach (Coordinate c in processor.Centroids)
                {
                    Point p = C2P(c);
                    int r = 8;
                    g.FillRectangle(new SolidBrush(Color.OrangeRed), p.X - r, p.Y - r, 2 * r, 2 * r);
                    g.DrawRectangle(cborder, p.X - r, p.Y - r, r * 2, r * 2);
                }
            }
        }
  
        #endregion

        private void button3_Click(object sender, EventArgs e)
        {
            Int32 nodeIncr = (int)numericUpDown3.Value;
            Int32 maxNodex = (int)numericUpDown4.Value;
            Int32 timeout = (int)numericUpDown5.Value;

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (Object o, DoWorkEventArgs a) =>
            {
                StreamWriter result = new StreamWriter(String.Format("StressTest-{0}.csv", DateTime.Now.Ticks));
                RoutingAI.Algorithms.IDistanceAlgorithm<Coordinate> distancealg = new RoutingAI.Algorithms.StraightDistanceAlgorithm();
                Random r = new Random();
                Coordinate[] data;

                Int32 currentNodes = 100;
                while (currentNodes < maxNodex)
                {
                    data = new Coordinate[currentNodes];
                    for (int i = 0; i < data.Length; i++)
                        data[i] = new Coordinate(r.Next(-10000, 10000) / 100.0f, r.Next(-10000, 10000) / 100.0f);

                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    //KMedoidsProcessor<Coordinate> proc = new KMedoidsProcessor<Coordinate>((int)Math.Sqrt(currentNodes), data, distancealg);
                    IClusteringAlgorithm<Coordinate> proc = new PAMClusteringAlgorithm<Coordinate>(data, (int)Math.Sqrt(currentNodes), distancealg);
                    proc.Run();
                    sw.Stop();
                    result.WriteLine("{0},{1}", currentNodes, sw.ElapsedMilliseconds);
                    result.Flush();

                    this.BeginInvoke(new Action(() =>
                    {
                        //coordinates = data;
                        //processor = proc;
                        //panel1.Invalidate();
                        lblCurrentResult.Text = String.Format("{0},{1}", currentNodes, sw.ElapsedMilliseconds);
                    }));

                    currentNodes += nodeIncr;
                }

            };
            bw.RunWorkerAsync();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Random r = new Random();

            // grab random sample
            Int32 sampleSize = (Int32)Math.Sqrt(coordinates.Length);
            if (sampleSize < numericUpDown2.Value) sampleSize = (int)numericUpDown2.Value;

            //Coordinate[] sample = coordinates.OrderBy((Coordinate c) => { return r.Next(); }).Take(sampleSize).ToArray();
            //Coordinate[] sample = coordinates.GetRandomSample(sampleSize);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            //processor = new KMedoidsProcessor<Coordinate>((int)numericUpDown2.Value, sample, new RoutingAI.Algorithms.StraightDistanceAlgorithm());
            //processor = new KMedoidsSampledProcessor<Coordinate>((int)numericUpDown2.Value, coordinates, sampleSize, 100, new RoutingAI.Algorithms.StraightDistanceAlgorithm(), 2);
            processor = new CLARAClusteringAlgorithm<Coordinate>(coordinates, (int)numericUpDown2.Value, (int)numericUpDown6.Value, new RoutingAI.Algorithms.StraightDistanceAlgorithm());
            processor.Run();
            sw.Stop();
            lblTime.Text = String.Format("Time Elapsed: {0}ms", sw.ElapsedMilliseconds);
            panel1.Invalidate();

        }
  }
}
