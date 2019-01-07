using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Rich6Extractor
{
    public partial class Form1 : Form
    {
        private bool saved;
        public string path;
        Rich6Viewer viewer;
        public Form1()
        {
            InitializeComponent();
            saved = true;
        }

        private void ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();
            var f = new OpenFileDialog();
            f.Filter = ".pck文件|*.pck";
            f.ShowDialog();
            if (f.FileName.Length > 0)
            {
                path = f.FileName;
                int v;
                byte[] header = new byte[8];
                using (var tempF=File.Open(path, FileMode.Open)){
                    tempF.Read(header, 0, 8);
                    if (header[4] == 0x1) v = 6;
                    else v = 7;
                }
                viewer = new Rich6Viewer(path, v);
                viewer.ReadAll();
                for (int i = 0; i < viewer.fileName.Count; i++)
                {
                    TreeNode t = new TreeNode(viewer.fileName[i]);
                    treeView1.Nodes.Add(t);
                }
                treeView1.SelectedNode = treeView1.Nodes[0];
                treeView1.Update();
            }
        }

        

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeView1.SelectedNode = e.Node;
                contextMenuStrip1.Show(new Point(MousePosition.X, MousePosition.Y));
            }
            int index = e.Node.Index;
            textBox2.Text = ("Size:" + viewer.fileSize[index].ToString("X8") + "  " + 
                "Offset:" + viewer.fileOffset[index].ToString("X8") + " " +
                " meta:"+(0x9+0x108*(index-1)).ToString("X8"));
            var encoding = Encoding.GetEncoding(936);
            textBox1.Text = encoding.GetString(viewer.ReadNode(index));
        }

        private void TreeVToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string path = "";
            SaveFileDialog s = new SaveFileDialog();
            s.FileName = treeView1.SelectedNode.Text.Split('/').Last();
            if (s.ShowDialog()== DialogResult.OK)
            {
                path = s.FileName;
                var buffer = viewer.ReadNode(treeView1.SelectedNode.Index);
                File.WriteAllBytes(path, buffer);
            }
        }

        private void TreeVToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            string path = "";
            OpenFileDialog o = new OpenFileDialog();          
            if (o.ShowDialog() == DialogResult.OK)
            {
                path = o.FileName;
                MessageBox.Show(viewer.InputNode(path, treeView1.SelectedNode.Index));
                saved = false;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            /*
            if (!saved)
            {
                if(MessageBox.Show("是否保存修改?",this.Name,MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    
                    //File.WriteAllBytes(path, viewer.data);
                }
            }
            */
        }

        private void TreeVToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Form2 f2 = new Form2(viewer, treeView1);
            f2.StartPosition = FormStartPosition.CenterParent;
            f2.ShowDialog();
            if (f2.result < 0)
            {
                if (f2.result == -1)
                {
                    MessageBox.Show("Not Found.");
                }
            }
            else
            {
                treeView1.SelectedNode = treeView1.Nodes[f2.result];
            }
        }

        private void treeView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (treeView1.Nodes.Count > 0 && e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.F:
                        TreeVToolStripMenuItem3_Click(sender, new EventArgs());
                        break;
                    case Keys.S:
                        TreeVToolStripMenuItem1_Click(sender, new EventArgs());
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
