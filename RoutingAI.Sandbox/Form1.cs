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
using RoutingAI.Algorithms.KMedoids;
using RoutingAI.API.OSRM;

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
        KMedoidsProcessor<Coordinate> processor = null;
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

            processor = new KMedoidsProcessor<Coordinate>((int)numericUpDown2.Value, coordinates, new RoutingAI.Algorithms.StraightDistanceAlgorithm());
            panel1.Invalidate();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);

            Pen border = new Pen(new SolidBrush(Color.Black), 1);
            Pen cborder = new Pen(new SolidBrush(Color.Black), 2);

            foreach (Coordinate c in coordinates)
            {
                Point p = C2P(c);
                if (processor != null)
                    g.FillEllipse(new SolidBrush(colors[processor.GetClusterIndex(c)]), p.X - radius, p.Y - radius, radius * 2, radius * 2);
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
  }
}
