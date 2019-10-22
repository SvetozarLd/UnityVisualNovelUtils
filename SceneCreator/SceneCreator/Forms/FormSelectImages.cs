using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SceneCreator.Forms
{
    public partial class FormSelectImages : Form
    {
        public int result { get; set; }
        public FormSelectImages(ListViewItem[] listViewItems, ImageList ImgBigLink, ImageList ImgSmallLink, View view)
        {
            InitializeComponent();
            if (ImgBigLink != null) { listView1.LargeImageList = ImgBigLink; }
            if (ImgSmallLink != null) { listView1.SmallImageList = ImgSmallLink; }
            listView1.Items.AddRange(listViewItems);
            listView1.View = view;
            toolStripDropDownButton1.Text = view.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            result = 0;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            savingResult();
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            savingResult();
        }

        private void savingResult()
        {
            if (listView1.SelectedItems.Count > 0)
            {
                result = (int)listView1.SelectedItems[0].Tag;
            }
            Close();
        }

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem toolStrip = (ToolStripMenuItem)sender;
            toolStripDropDownButton1.Text = toolStrip.Text;
            switch (toolStrip.Text)
            {
                case "LargeIcon":
                    listView1.View = View.LargeIcon;
                    break;
                case "SmallIcon":
                    listView1.View = View.SmallIcon;
                    break;
                case "List":
                    listView1.View = View.List;
                    break;
                case "Tile":
                    listView1.View = View.Tile;
                    break;
            }
        }
    }
}
