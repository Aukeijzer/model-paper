using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Paper_Model
{
    public partial class Form1 : Form
    {
        private int X = 10;
        private int Y = 10;
        private int distance = 10;
        private List<int> legsUsage = new List<int>();
        private List<int> bikeUsage = new List<int>();
        private List<int> carUsage = new List<int>();
        private Series series;
        public Form1()
        {
            InitializeComponent();
            textBox1.ScrollBars = ScrollBars.Vertical;
            chart1.Titles.Add("Vehicle usage");
            series = chart1.Series.Add("Vehicle usage");
            series.ChartType = SeriesChartType.Pie;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Node[] nodes = Node.createNodeGrid(X, Y, distance);
            Graph graph2 = new Graph(nodes);
            WorldNode[] worldNodes = new WorldNode[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
                worldNodes[i] = new WorldNode(nodes[i]);
            for (int i = 0; i < nodes.Length / 3; i++)
            {
                worldNodes[i].addFamily(4, 1, 3);
            }
            World world = new World(worldNodes);
            for (int i = 0; i < 24; i++)
            {
                var logs = world.Tick();
                for (int j = 0; j < logs.Count; j++)
                {
                    textBox1.AppendText(logs[j].ToString() + Environment.NewLine);
                }
                textBox1.AppendText("---" + Environment.NewLine);
                legsUsage.Add(World.legsUsage);
                bikeUsage.Add(World.bikeUsage);
                carUsage.Add(World.carUsage);

            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            X = (int)numericUpDown1.Value;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            Y = (int)numericUpDown2.Value;
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            distance = (int)numericUpDown3.Value;
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            series.Points.Clear();
            if (numericUpDown4.Value > 0)
            {
                series.Points.AddXY("Walking", legsUsage[(int)numericUpDown4.Value-1]);
                series.Points.AddXY("Cycling", bikeUsage[(int)numericUpDown4.Value-1]);
                series.Points.AddXY("Driving", carUsage[(int)numericUpDown4.Value-1]);
            }
        }
    }
}