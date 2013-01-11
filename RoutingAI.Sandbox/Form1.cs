using RoutingAI.Algorithms.Clustering;
using RoutingAI.API.OSRM;
using RoutingAI.DataContracts;
using RoutingAI.ServiceContracts;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.ServiceModel;
using System.Windows.Forms;

namespace RoutingAI.Sandbox
{
    public partial class Form1 : Form
    {
        private readonly OptimizationRequestGenerator _optimizationRequestGenerator = new OptimizationRequestGenerator();

        public Form1()
        {
            InitializeComponent();
            UpdateTaskCountTime();
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

        private void buttonEPAMCluster_Click(object sender, EventArgs e)
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
                    new EPAMClusteringAlgorithm<Coordinate>(data, clusterCount, new RoutingAI.Algorithms.StraightDistanceAlgorithm());
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
        private void buttonCLARACluster_Click(object sender, EventArgs e)
        {
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
        private void buttonKMeansCluster_Click(object sender, EventArgs e)
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
                    new KMeansClusteringAlgorithm<Coordinate>(data, clusterCount, new RoutingAI.Algorithms.StraightDistanceAlgorithm());
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

        #endregion

        #region Controller Demo

        #region UI

        private GenerationConfig GenerationConfig
        {
            get
            {
                int numberResources = (int)numericUpDownResourceCount.Value;
                int tasksPer = trackBarTasksPerResource.Value;

                OptimizationPeriod period = OptimizationPeriod.Day;
                if (radioButtonWeek.Checked)
                    period = OptimizationPeriod.Week;
                else if (radioButtonMonth.Checked)
                    period = OptimizationPeriod.Month;
                else if (radioButtonYear.Checked)
                    period = OptimizationPeriod.Year;

                return new GenerationConfig
                {
                    NumberResources = numberResources,
                    TasksPerResourceDay = tasksPer,
                    Period = period,
                    ResourceCostPerHour = (uint)(numericUpDownResourceCostHour.Value * 100),
                    ResourceCostPerMile = (uint)(numericUpDownResourceCostMile.Value * 100),
                    LikelihoodCompleteAllTasks = trackBarCompleteAllTasksLikelihood.Value,
                    TaskAveragePrice = (uint)(numericUpDownTaskAvgPrice.Value * 100)
                };
            }
        }

        private void UpdateTaskCountTime()
        {
            var tasksCount = OptimizationRequestGenerator.TaskCount(GenerationConfig);
            labelTaskCount.Text = "Count: " + tasksCount;

            var averageTaskTime = OptimizationRequestGenerator.AverageTaskTime(GenerationConfig);
            labelTaskAverageTimeMins.Text = "Average Time (mins): " + averageTaskTime;
        }

        //Resource Elements
        private void numericUpDownResourceCount_ValueChanged(object sender, EventArgs e)
        {
            UpdateTaskCountTime();
        }
        private void trackBarTasksPerResource_Scroll(object sender, EventArgs e)
        {
            labelTasksPerResourceDay.Text = GenerationConfig.TasksPerResourceDay.ToString();
            UpdateTaskCountTime();
        }

        private void radioButtonDay_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTaskCountTime();

            //when optimizing for a day likely none are recurring
            radioButtonTargetNone.Checked = true;
        }
        private void radioButtonWeek_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTaskCountTime();

            //when optimizing for a week likely some are recurring
            radioButtonTargetSome.Checked = true;
        }
        private void radioButtonMonth_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTaskCountTime();

            //when optimizing for a month most likely some are recurring
            radioButtonTargetSome.Checked = true;
        }
        private void radioButtonYear_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTaskCountTime();

            //when optimizing for a year most likely some are recurring
            radioButtonTargetSome.Checked = true;
        }

        //Task Elements
        private void trackBarTaskAvgPrice_Scroll(object sender, EventArgs e)
        {
            numericUpDownTaskAvgPrice.Value = (int)(trackBarTaskAvgPrice.Value / 100.0 * 500.0); //high end: $500
        }
        private void trackBarResourceCount_Scroll(object sender, EventArgs e)
        {
            numericUpDownResourceCount.Value = (int)(trackBarResourceCount.Value / 10.0 * 20.0); //high end: 1,000 tasks
        }
        private void trackBarCompleteAllTasksLikelihood_Scroll(object sender, EventArgs e)
        {
            UpdateTaskCountTime();
        }

        #endregion

        private void buttonOptimizationRequest_Click(object sender, EventArgs e)
        {
            EndpointAddress endpoint = new EndpointAddress("http://localhost:8000/RoutingAi/Controller");
            IRoutingAiService proxy = ChannelFactory<IRoutingAiService>.CreateChannel(new BasicHttpBinding(), endpoint);

            OptimizationRequest or = _optimizationRequestGenerator.Generate(GenerationConfig);
            proxy.Post(or);
        }

        #endregion
    }
}
