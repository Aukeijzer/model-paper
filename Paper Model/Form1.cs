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
        public Form1()
        {
            InitializeComponent();
            Node[] nodes = Node.createNodeGrid(10, 10, 10);
            Graph graph2 = new Graph(nodes);
            WorldNode[] worldNodes = new WorldNode[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
                worldNodes[i] = new WorldNode(nodes[i]);
            worldNodes[0].addFamily(15, 15, 7);
            World world = new World(worldNodes);
            textBox1.ScrollBars = ScrollBars.Vertical;
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
    }
}