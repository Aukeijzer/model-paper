using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Paper_Model
{
    public partial class Form1 : Form
    {
        private int X;
        private int Y;
        private int distance;
        public Form1()
        {
            InitializeComponent();
            textBox1.ScrollBars = ScrollBars.Vertical;
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Node[] nodes = Node.createNodeGrid(X, Y, distance);
            Graph graph2 = new Graph(nodes);
            WorldNode[] worldNodes = new WorldNode[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
                worldNodes[i] = new WorldNode(nodes[i]);
            worldNodes[0].addFamily(10, 1, 7);
            World world = new World(worldNodes);
            for (int i = 0; i < 10; i++)
            {
                var logs = world.Tick();
                for (int j = 0; j < logs.Count; j++)
                {
                    textBox1.AppendText(logs[j].ToString() + Environment.NewLine);
                }
                textBox1.AppendText("---" + Environment.NewLine);
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
    }
}