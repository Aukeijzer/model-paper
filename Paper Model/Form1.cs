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
        private int distance = 2;
        private List<int> legsUsage = new List<int>();
        private List<int> bikeUsage = new List<int>();
        private List<int> carUsage = new List<int>();
        private List<int> busUsage = new List<int>();
        private Series series1;
        private Series series2;
        public Form1()
        {
            InitializeComponent();
            textBox1.ScrollBars = ScrollBars.Vertical;
            chart1.Titles.Add("Vehicle usage");
            series1 = chart1.Series.Add("Vehicle usage");
            series1.ChartType = SeriesChartType.Pie;
            chart2.Titles.Add("Total CO2 emission in grams");
            series2 = chart2.Series.FindByName("CO2 emission");
            
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
                worldNodes[i].addFamily(5, 1, 5);
            }
            List<int> bus = new List<int>() { 0, 1, 2 };
            List<List<int>> buses = new List<List<int>>() { bus };
            World world = new World(worldNodes, buses);
            for (int i = 0; i < 24; i++)
            {
                var logs = world.Tick();
                bool auke = false;
                if (auke)
                {
                    for (int j = 0; j < logs.Count; j++)
                    {
                        textBox1.AppendText(logs[j].ToString() + Environment.NewLine);
                    }
                    textBox1.AppendText("---" + Environment.NewLine);
                }
                textBox1.AppendText(i+" ");
                legsUsage.Add(World.legsUsage);
                bikeUsage.Add(World.bikeUsage);
                carUsage.Add(World.carUsage);
                busUsage.Add(World.busUsage);
                series2.Points.AddXY(i, World.totalCarEmissions);
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
            series1.Points.Clear();
            if (numericUpDown4.Value > 0)
            {
                series1.Points.AddXY("Walking", legsUsage[(int)numericUpDown4.Value-1]);
                series1.Points.AddXY("Cycling", bikeUsage[(int)numericUpDown4.Value-1]);
                series1.Points.AddXY("Driving", carUsage[(int)numericUpDown4.Value-1]);
                series1.Points.AddXY("Bus", busUsage[(int)numericUpDown4.Value-1]);
            }
        }
    }
}