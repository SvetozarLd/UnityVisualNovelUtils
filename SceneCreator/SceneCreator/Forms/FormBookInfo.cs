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
using System.Diagnostics;

namespace SceneCreator.Forms
{
    public partial class FormBookInfo : Form
    {
        public FormBookInfo()
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(Properties.Settings.Default.path) && !string.IsNullOrWhiteSpace(Properties.Settings.Default.path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(Properties.Settings.Default.path);
                if (directoryInfo.Exists)
                {
                    //treeView1.AfterSelect += treeView1_AfterSelect;
                    BuildTree(directoryInfo, treeView1.Nodes);
                    treeView1.Enabled = true;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FormBookInfo_Load(object sender, EventArgs e)
        {
            textBox1.Text = Properties.Settings.Default.path;
        }

        private void BuildTree(DirectoryInfo directoryInfo, TreeNodeCollection addInMe)
        {
            TreeNode folderNode = new TreeNode();
            folderNode.Name = directoryInfo.FullName;
            folderNode.Text = directoryInfo.Name;
            addInMe.Add(folderNode);
            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                TreeNode fileNode = new TreeNode();
                fileNode.Name = file.FullName;
                fileNode.Text = file.Name;
                folderNode.Nodes.Add(fileNode);
            }
            foreach (DirectoryInfo subdir in directoryInfo.GetDirectories())
            {
                BuildTree(subdir, folderNode.Nodes);
            }
        }


        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var node = treeView1.SelectedNode;
            if (node!= null)
            {
                FileAttributes attr = File.GetAttributes(node.Name);
                if (!attr.HasFlag(FileAttributes.Directory))
                {
                    FileInfo fileInfo = new FileInfo(node.Name);
                    toolStripStatusLabel1.Text = fileSizeToString(fileInfo.Length);
                    return;
                }
            }
            toolStripStatusLabel1.Text = string.Empty;
        }

        private string fileSizeToString(long length)
        {
            if (length > 1048576) { return Math.Round((double)length / 1048576, 3).ToString() + " mb"; }
            if (length > 1024) { return Math.Round((double)length / 1024, 3).ToString() + " kb"; }
            return length.ToString() + " b";
        }
    }
}
