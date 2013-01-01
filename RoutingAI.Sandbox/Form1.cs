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

        private static Point C2P(Coordinate c, Int32 w, Int32 h)
        {
            // convert x
            Int32 x = (Int32)(((c.lon + 100) / 200) * w);
            Int32 y = (Int32)(((c.lat + 100) / 200) * h);

            return new Point(x, y);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Int32 nodeCount = (int)numericUpDown1.Value;
            Int32 clusterCount = (int)numericUpDown2.Value;
            Int32 w = panel1.Width;
            Int32 h = panel1.Height;

            panel1.Visible = false;

            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += (Object o, DoWorkEventArgs a) =>
            {
                Random rand = new Random();
                bw.ReportProgress(0, "Generating");
                // Generate Data
                Coordinate[] data = new Coordinate[nodeCount];
                for (int i = 0; i < data.Length; i++)
                    data[i] = new Coordinate(rand.Next(-10000, 10000) / 100.0f, rand.Next(-10000, 10000) / 100.0f);

                // Run Algorithm
                bw.ReportProgress(0, "Clustering");
                Stopwatch sw = new Stopwatch();
                sw.Start();
                IClusteringAlgorithm<Coordinate> processor = 
                    new PAMClusteringAlgorithm<Coordinate>(data, clusterCount, new RoutingAI.Algorithms.StraightDistanceAlgorithm());
                processor.Run();
                sw.Stop();

                this.BeginInvoke(new Action(()=>{
                    lblTime.Text = String.Format("Elapsed Time: {0}ms", sw.ElapsedMilliseconds);
                }));

                // Render
                bw.ReportProgress(0, "Rendering");
                Bitmap bmp = new Bitmap(w, h);
                Graphics g = Graphics.FromImage(bmp);

                Pen border = new Pen(new SolidBrush(Color.Black), 1);
                Pen cborder = new Pen(new SolidBrush(Color.Black), 2);

                foreach (Coordinate c in data)
                {
                    Point p = C2P(c, w, h);


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
                        Point p = C2P(c, w, h);
                        int r = 8;
                        g.FillRectangle(new SolidBrush(Color.OrangeRed), p.X - r, p.Y - r, 2 * r, 2 * r);
                        g.DrawRectangle(cborder, p.X - r, p.Y - r, r * 2, r * 2);
                    }
                }

                // result
                a.Result = bmp;
            };
            bw.ProgressChanged += (Object o, ProgressChangedEventArgs a) =>
            {
                lblPhase.Text = (string)a.UserState;
                lblProgress.Text = a.ProgressPercentage.ToString();
            };
            bw.RunWorkerCompleted += (Object o, RunWorkerCompletedEventArgs a) =>
            {
                panel1.BackgroundImage = (Bitmap)a.Result;
                panel1.Show();
            };
            bw.RunWorkerAsync();
        }
  
        #endregion

        private void button4_Click(object sender, EventArgs e)
        {
            /*
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
            */
            Int32 nodeCount = (int)numericUpDown1.Value;
            Int32 clusterCount = (int)numericUpDown2.Value;
            Int32 samplingRUns = (int)numericUpDown6.Value;
            Int32 w = panel1.Width;
            Int32 h = panel1.Height;

            panel1.Visible = false;

            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += (Object o, DoWorkEventArgs a) =>
            {
                Random rand = new Random();
                bw.ReportProgress(0, "Generating");
                // Generate Data
                Coordinate[] data = new Coordinate[nodeCount];
                for (int i = 0; i < data.Length; i++)
                    data[i] = new Coordinate(rand.Next(-10000, 10000) / 100.0f, rand.Next(-10000, 10000) / 100.0f);

                // Run Algorithm
                bw.ReportProgress(0, "Clustering");
                Stopwatch sw = new Stopwatch();
                sw.Start();
                IClusteringAlgorithm<Coordinate> processor =
                    new CLARAClusteringAlgorithm<Coordinate>(data, clusterCount, samplingRUns, new RoutingAI.Algorithms.StraightDistanceAlgorithm());
                    processor.Run();
                sw.Stop();

                this.BeginInvoke(new Action(() =>
                {
                    lblTime.Text = String.Format("Elapsed Time: {0}ms", sw.ElapsedMilliseconds);
                }));

                // Render
                bw.ReportProgress(0, "Rendering");
                Bitmap bmp = new Bitmap(w, h);
                Graphics g = Graphics.FromImage(bmp);

                Pen border = new Pen(new SolidBrush(Color.Black), 1);
                Pen cborder = new Pen(new SolidBrush(Color.Black), 2);

                foreach (Coordinate c in data)
                {
                    Point p = C2P(c, w, h);


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
                        Point p = C2P(c, w, h);
                        int r = 8;
                        g.FillRectangle(new SolidBrush(Color.OrangeRed), p.X - r, p.Y - r, 2 * r, 2 * r);
                        g.DrawRectangle(cborder, p.X - r, p.Y - r, r * 2, r * 2);
                    }
                }

                // result
                a.Result = bmp;
            };
            bw.ProgressChanged += (Object o, ProgressChangedEventArgs a) =>
            {
                lblPhase.Text = (string)a.UserState;
                lblProgress.Text = a.ProgressPercentage.ToString();
            };
            bw.RunWorkerCompleted += (Object o, RunWorkerCompletedEventArgs a) =>
            {
                panel1.BackgroundImage = (Bitmap)a.Result;
                panel1.Show();
            };
            bw.RunWorkerAsync();
        }
  
        private void RunClustering(IClusteringAlgorithm<Coordinate> alg)
        {
            
        }
    }
}
