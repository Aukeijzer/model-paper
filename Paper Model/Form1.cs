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
        private List<int> busUsage = new List<int>();
        private Series series1;
        private Series series2;
        List<float[]> results = new List<float[]>();

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
            float gasPrice;
            for (int m = 0; m < 31; m++)
            {
                gasPrice = 0.05f+m*0.01f;
                textBox3.AppendText(gasPrice + Environment.NewLine);
                for (int k = 0; k < 10; k++)
                {
                    Node[] nodes = Node.createNodeGrid(X, Y, distance);
                    Graph graph2 = new Graph(nodes);
                    
                    WorldNode[] worldNodes = new WorldNode[nodes.Length];
                    for (int i = 0; i < nodes.Length; i++)
                        worldNodes[i] = new WorldNode(nodes[i]);
                    for (int i = 0; i < X; i++)
                        for (int j = 0; j < Y; j++)
                            worldNodes[i * Y + j].carPark = (i % 2 == 0) && (j % 2 == 0); //change 1 to 2 to have a park node once every four nodes or 3 for once every 9.
                    for (int i = 0; i < nodes.Length / 3; i++)
                    {
                        worldNodes[i].addFamily(6, 2, 5);
                    }
                    List<int> bus1 = new List<int>() { 40, 41, 42, 43, 44, 45 ,46 ,47 ,48, 49,
                                                59, 58, 57, 56, 55, 54, 53, 52, 51, 50, 40};
                    List<int> bus2 = new List<int>() { 4, 14, 24, 34, 44, 54 ,64 ,74 ,84, 94,
                                                65, 85, 75, 65, 55, 45, 35, 25, 15, 5, 4};
                    List<List<int>> buses = new List<List<int>>() { bus1, bus2 };
                    List<List<int>> buses2 = new List<List<int>>() { };
                    World world = new World(worldNodes, buses2, gasPrice);
                    float[] runResults = new float[24];
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
                        //textBox1.AppendText(i + " ");
                        //legsUsage.Add(World.legsUsage);
                        //bikeUsage.Add(World.bikeUsage);
                        //carUsage.Add(World.carUsage);
                        //busUsage.Add(World.busUsage);
                        runResults[i] = World.totalCarKM;
                        //series2.Points.AddXY(i, World.totalCarKM);
                    }
                    results.Add(runResults);
                }
                button2_Click(new object(), e);
                results.Clear();
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

        private void button2_Click(object sender, EventArgs e)
        {
            float[] average = new float[24];
            for (int i = 0; i < 24; i++)
            {
                average[i] = 0;
                for (int j = 0; j < results.Count; j++)
                    average[i] += results[j][i];
                average[i] = average[i] / 10f;
                //textBox1.AppendText(average[i].ToString() + Environment.NewLine);
            }
            //textBox1.AppendText("\n Standard deviations: \n");

            float[] sd = new float[24];
            for (int i = 0; i < 24; i++)
            {
                sd[i] = 0;
                for (int j = 0; j < results.Count; j++)
                    sd[i] += Math.Abs(average[i] - results[j][i]);
                sd[i] = sd[i] / 10f;
                //textBox1.AppendText(sd[i].ToString() + Environment.NewLine);
            }
            //textBox1.AppendText("\n --- \n ");
            float trueAverage = 0;
            for (int i = 0; i < 24; i++)
                trueAverage += average[i];
            trueAverage = trueAverage / 24f;
            float trueSD = 0;
            for (int i = 0; i < 24; i++)
                trueSD += sd[i];
            trueSD = trueSD / 24f;
            textBox1.AppendText(trueAverage.ToString() + Environment.NewLine);
            textBox2.AppendText(trueSD.ToString() + Environment.NewLine);
        }
    }
}