using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
namespace SceneCreator.Forms
{
    public partial class FormMain : Form
    {
        //DataTable scenesDt = new DataTable();
        //DataTable backsDt = new DataTable();
        //DataTable layersDt = new DataTable();

        public Dictionary<int, byte[]> CPBacks = new Dictionary<int, byte[]>(); // backgrounds
        public Dictionary<int, byte[]> CPLayers = new Dictionary<int, byte[]>(); // persons
        public Dictionary<int, Proto.ProtoChapters.protoRow> protoChapters = new Dictionary<int, Proto.ProtoChapters.protoRow>(); // chapters
        private DataTable dt;
        public FormMain()
        {
            InitializeComponent();
        }


        private void SetStartVariables()
        {
            toolStripComboBox1.Items.Add("LargeIcon");
            toolStripComboBox1.Items.Add("SmallIcon");
            toolStripComboBox1.Items.Add("List");
            toolStripComboBox1.Items.Add("Tile");
            toolStripComboBox1.SelectedIndex = 2;
            panel3.Width = 200;
        }




        private void FormMain_Load(object sender, EventArgs e)
        {
            SetStartVariables();
            LoadScenes("");
            dataGridView1.AutoGenerateColumns = false;
            dataGridView3.AutoGenerateColumns = false;
        }





        private void LoadScenes(string path)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
            {
                Proto.ProtoChapters.protoRow protoRow = new Proto.ProtoChapters.protoRow
                {
                    Name = "Ктулху",
                    Message = "Всех захаваю внатуре нах!"
                };
                protoChapters.Add(1, protoRow);
                Proto.ProtoChapters.protoRow protoRow1 = new Proto.ProtoChapters.protoRow
                {
                    Name = "Фтагн",
                    Message = "Хуй тебе!"
                };
                protoChapters.Add(2, protoRow1);
            }

            dataGridViewScenes_BindingSource();
        }

        private void SaveScenes(string path)
        {

        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            ListView lst = (ListView)sender;
            switch (e.Button)
            {
                case MouseButtons.Right:
                    contextMenuStrip1.Tag = lst.Name;
                    contextMenuStrip1.Show(lst.PointToScreen(e.Location));
                    contextMenuStrip1.Show();
                    break;
                case MouseButtons.Left:
                    ListViewHitTestInfo info = lst.HitTest(e.X, e.Y);
                    if (info.Item != null)
                    {
                        if (lst.Name.Equals(listView1.Name))
                        {
                            pictureBox1.Image = null;
                            pictureBox1.Image = Utils.FilesWorks.ByteToImage(CPBacks[(int)info.Item.Tag]);
                            return;
                        }
                        if (lst.Name.Equals(listView2.Name))
                        {
                            pictureBox2.Image = null;
                            pictureBox2.Image = Utils.FilesWorks.ByteToImage(CPLayers[(int)info.Item.Tag]);
                            return;
                        }
                    }
                    break;
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //var ww = listView1.SelectedItems
        }


        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            #region __________________________
            //ListView lst = null;
            //switch (tabControl1.SelectedIndex)
            //{
            //    case 1: lst = listView1; break;
            //    case 2: lst = listView2; break;
            //}
            //if (lst != null)
            //{
            //    switch (toolStripComboBox1.SelectedIndex)
            //    {
            //        case 0:
            //            lst.View = View.LargeIcon;
            //            break;
            //        case 1:
            //            lst.View = View.SmallIcon;
            //            break;
            //        case 2:
            //            lst.View = View.List;
            //            break;
            //        case 3:
            //            lst.View = View.Tile;
            //            break;
            //    }
            //}
            //ListView lst = null;
            //switch (tabControl1.SelectedIndex)
            //{
            //    case 1: lst = listView1; break;
            //    case 2: lst = listView2; break;
            //}
            #endregion
            switch (toolStripComboBox1.SelectedIndex)
            {
                case 0:
                    listView1.View = View.LargeIcon;
                    listView2.View = View.LargeIcon;
                    break;
                case 1:
                    listView1.View = View.SmallIcon;
                    listView2.View = View.SmallIcon;
                    break;
                case 2:
                    listView1.View = View.List;
                    listView2.View = View.List;
                    break;
                case 3:
                    listView1.View = View.Tile;
                    listView2.View = View.Tile;
                    break;
            }
        }

        #region Gui: Image Panels uniform width
        private void panel3_SizeChanged(object sender, EventArgs e)
        {
            if (((Control)sender).Name.Equals(panel3.Name)) { panel1.Width = panel3.Width; } else { panel3.Width = panel1.Width; }
        }
        #endregion








        #region Add BinaryFile (Image/Sound)
        private void добавитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int i = 0;
            if (contextMenuStrip1.Tag.ToString().Equals(listView1.Name))
            {
                if (CPBacks.Count > 0) { i = CPBacks.Keys.Max() + 1; } else { i = 1; }
                Utils.ImageUI result = Utils.FilesWorks.GetFilesByteArray("Изображения PNG24| *.png", i);
                if (result != null && result.Image != null && result.PreviewSmall != null)
                {
                    CPBacks.Add(i, result.Image);
                    string resultname = i.ToString();
                    imageList_BackBig.Images.Add(resultname, Utils.FilesWorks.ByteToImage(result.PreviewBig));
                    imageList_BackSmall.Images.Add(resultname, Utils.FilesWorks.ByteToImage(result.PreviewSmall));
                    ListViewItem item = new ListViewItem
                    {
                        ImageKey = resultname,
                        Text = resultname,
                        Tag = i
                    };
                    listView1.Items.Add(item);
                }
                return;
            }
            if (contextMenuStrip1.Tag.ToString().Equals(listView2.Name))
            {
                if (CPLayers.Count > 0) { i = CPLayers.Keys.Max() + 1; } else { i = 1; }
                Utils.ImageUI result = Utils.FilesWorks.GetFilesByteArray("Изображения PNG24| *.png", i);
                if (result != null && result.Image != null && result.PreviewSmall != null)
                {
                    CPLayers.Add(i, result.Image);
                    string resultname = i.ToString();
                    imageList_LayerBig.Images.Add(resultname, Utils.FilesWorks.ByteToImage(result.PreviewBig));
                    imageList_LayerSmall.Images.Add(resultname, Utils.FilesWorks.ByteToImage(result.PreviewSmall));
                    ListViewItem item = new ListViewItem
                    {
                        ImageKey = resultname,
                        Text = resultname,
                        Tag = i
                    };
                    listView2.Items.Add(item);
                }
                return;
            }

            if (contextMenuStrip1.Tag.ToString().Equals(dataGridView1.Name))
            {
                using (FormButtonChoice frm = new FormButtonChoice(null))
                {
                    frm.ShowInTaskbar = false;
                    frm.StartPosition = FormStartPosition.Manual;
                    frm.Location = contextMenuStrip1.PointToScreen(Point.Empty);
                    frm.ShowDialog();
                    if (frm.result != null)
                    {
                        int uid = (int)groupBox_SceneMain.Tag;
                        if (protoChapters[uid].ButtonChoice == null)
                        {
                            protoChapters[uid].ButtonChoice = new List<Proto.ProtoChapters.protoСhoice>();

                        }
                        protoChapters[uid].ButtonChoice.Add(frm.result);
                        showButtonChoice(uid);
                    }
                }
                return;
            }
            if (contextMenuStrip1.Tag.ToString().Equals(dataGridView3.Name))
            {
                if (protoChapters.Count > 0) { i = protoChapters.Keys.Max() + 1; } else { i = 1; }
                Proto.ProtoChapters.protoRow tmp = new Proto.ProtoChapters.protoRow
                {
                    Message = string.Empty,
                    Name = string.Empty,
                    ButtonChoice = new List<Proto.ProtoChapters.protoСhoice>()
                };
                Utils.ChaptersDataTable.result result = Utils.ChaptersDataTable.AddNewRow(new KeyValuePair<int, Proto.ProtoChapters.protoRow>(i, tmp));
                if (result.ex != null) { MessageBox.Show(result.ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error); } else { protoChapters.Add(i, tmp); }
            }
        }
        #endregion

        private void dataGridViewScenes_BindingSource()
        {
            Utils.ChaptersDataTable.result res = Utils.ChaptersDataTable.ConvertFromProto(protoChapters);
            if (res.dt != null)
            {
                dt = res.dt;
                dataGridView3.DataSource = dt;
            }
            else
            {
                MessageBox.Show(res.ex.Message);
            }
        }

        private void dataGridView3_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView3.SelectedRows != null && dataGridView3.SelectedRows.Count > 0)
            {
                fillTemplateByScene((int)dataGridView3.SelectedRows[0].Cells["SceneUid"].Value);
            }
        }


        private void fillTemplateByScene(int uid)
        {

            textBox1.Text = protoChapters[uid].Name;
            textBox2.Text = protoChapters[uid].Message;
            groupBox_SceneMain.Text = "Сцена № " + uid.ToString();
            groupBox_SceneMain.Tag = uid;
            checkBox1.Checked = protoChapters[uid].AutoJump;
            numericUpDown3.Value = Convert.ToDecimal(protoChapters[uid].AutoJumpTimer);
            if (protoChapters[uid].Background == 0 && protoChapters.ContainsKey(uid - 1)) { protoChapters[uid].Background = protoChapters[uid - 1].Background; }
            if (protoChapters[uid].Layer == 0 && protoChapters.ContainsKey(uid - 1)) { protoChapters[uid].Layer = protoChapters[uid - 1].Layer; }
            showButtonChoice(uid);
        }


        private void showButtonChoice(int uid)
        {
            dataGridView1.DataSource = null;
            if (protoChapters[uid].ButtonChoice != null)
            {
                dataGridView1.DataSource = protoChapters[uid].ButtonChoice;
            }
            dataGridView1.Refresh();
        }



        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                DataGridView dataGridView = (DataGridView)sender;
                contextMenuStrip1.Tag = dataGridView.Name;
                contextMenuStrip1.Show(dataGridView.PointToScreen(e.Location));
                contextMenuStrip1.Show();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (((Control)sender).Name.Equals(button2.Name))
            {
                if (listView1.Items.Count > 0)
                {
                    using (FormSelectImages frm = new FormSelectImages((from ListViewItem item in listView1.Items select (ListViewItem)item.Clone()).ToArray(), imageList_BackBig, imageList_BackSmall, listView1.View)) //listView1.Items.OfType<ListViewItem>().ToArray()
                    {
                        frm.ShowDialog();
                        numericUpDown1.Value = frm.result;
                    }
                }
                else
                {
                    DialogResult result = MessageBox.Show("Вы не добавили в том фоновые изображения!" + Environment.NewLine + "Перейти в раздел фоновых изображений для добавления?",
                                                            "Внимание!",
                                                            MessageBoxButtons.YesNo,
                                                            MessageBoxIcon.Question,
                                                            MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Yes) { tabControl1.SelectedIndex = 1; }
                }
            }
            else
            {
                if (listView2.Items.Count > 0)
                {
                    using (FormSelectImages frm = new FormSelectImages((from ListViewItem item in listView2.Items select (ListViewItem)item.Clone()).ToArray(), imageList_LayerBig, imageList_LayerSmall, listView2.View)) //listView1.Items.OfType<ListViewItem>().ToArray()
                    {
                        frm.ShowDialog();
                        numericUpDown2.Value = frm.result;
                    }
                }
                else
                {
                    DialogResult result = MessageBox.Show("Вы не добавили в том изображения персонажей!" + Environment.NewLine + "Перейти в раздел изображений персонажей для добавления?",
                                                            "Внимание!",
                                                            MessageBoxButtons.YesNo,
                                                            MessageBoxIcon.Question,
                                                            MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Yes) { tabControl1.SelectedIndex = 2; }
                }
            }

        }

        private void aboutBookToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormBookInfo frm = new FormBookInfo())
            {
                frm.ShowDialog();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //int uid = (int)groupBox_SceneMain.Tag;
            //if (protoChapters.ContainsKey(uid))
            //{
            //    protoChapters[uid].AutoJump = checkBox1.Checked;
            //}
            numericUpDown3.Enabled = checkBox1.Checked;
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            //int uid = (int)groupBox_SceneMain.Tag;
            //if (protoChapters.ContainsKey(uid))
            //{
            //    protoChapters[uid].AutoJumpTimer = (float)numericUpDown3.Value;
            //}
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //int uid = (int)groupBox_SceneMain.Tag;
            //if (protoChapters.ContainsKey(uid))
            //{
            //    protoChapters[uid].Name = textBox1.Text;                
            //}
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            //int uid = (int)groupBox_SceneMain.Tag;
            //if (protoChapters.ContainsKey(uid))
            //{
            //    protoChapters[uid].Message = textBox2.Text;
            //}
        }

        //private void button4_Click(object sender, EventArgs e)
        //{
        //    int uid = (int)groupBox_SceneMain.Tag;
        //    if (protoChapters.ContainsKey(uid))
        //    {
        //        protoChapters[uid].Name = textBox1.Text;
        //        //protoChapters[uid].AutoJump = textBox1.Text;
        //        //protoChapters[uid].AutoJumpTimer = textBox1.Text;
        //        protoChapters[uid].Background = (int)numericUpDown1.Value;
        //        //protoChapters[uid].ButtonChoice = textBox1.Text;
        //        protoChapters[uid].Layer = (int)numericUpDown2.Value;
        //        protoChapters[uid].Message = textBox2.Text;
        //        protoChapters[uid].Sound = 0;
        //    }
        //    else
        //    {
        //        MessageBox.Show("Сцена с номером " + uid.ToString() + " не найдена! О_о", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}
    }
}
