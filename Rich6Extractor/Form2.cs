using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rich6Extractor
{
    public partial class Form2 : Form
    {
        Rich6Viewer v1;
        TreeView t1;
        public enum result_code { NotFound=-1,Cancel=-2};
        public int result;
        public Form2(Rich6Viewer v,TreeView t)
        {
            v1 = v;
            t1 = t;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var key = textBox1.Text;
            result = (int)result_code.NotFound;
            for (int i = t1.SelectedNode.Index; i < t1.Nodes.Count; i++)
            {
                if (v1.fileName[i].Contains(key))
                {
                    result = i;
                    break;
                }
            }
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            result = (int)result_code.Cancel;
            this.Close();
        }

        private void Form2_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        private void Form2_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    button1.PerformClick();
                    break;
                default: return;
            }
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
            }
        }
    }
}
