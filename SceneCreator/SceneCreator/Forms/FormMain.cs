using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SceneCreator.Forms
{
    public partial class FormMain : Form
    {
        private enum MaterialsType
        {
            Backgrounds,
            Characters,
            Music
        }


        public Dictionary<int, byte[]> CPBacks = new Dictionary<int, byte[]>(); // backgrounds
        public Dictionary<int, byte[]> CPLayers = new Dictionary<int, byte[]>(); // persons
        public Dictionary<int, byte[]> CPMusics = new Dictionary<int, byte[]>(); // musics
        public Dictionary<string, Proto.ProtoChapter.protoRow> CPChapters = new Dictionary<string, Proto.ProtoChapter.protoRow>(); // book

        private DataTable dt; // link of ChaptersDataTable datatable - for scenes datagridview
        private bool UpdateChapterFieldLocker = false; // flag for denied update scenes info
        private TabPage tab; // hidding tab with scenes 
        private string CurrentPath = string.Empty; //corrent book path

        #region Constructor + Necessaries for start
        public FormMain()
        {
            InitializeComponent();
        }

        private void SetStartVariables()
        {
            toolStripComboBox_ListView.Items.Add("LargeIcon");
            toolStripComboBox_ListView.Items.Add("SmallIcon");
            toolStripComboBox_ListView.Items.Add("List");
            toolStripComboBox_ListView.Items.Add("Tile");
            toolStripComboBox_ListView.SelectedIndex = 2;
            panel3.Width = 200;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            tab = tabControl1.TabPages[1];
            Properties.Settings.Default.Reload();
            CheckRecent();
            tabControl1.TabPages.Remove(tab);
            Utils.ChaptersDataTable.Initialization();
            SetStartVariables();
            dataGridView1.AutoGenerateColumns = false;
            dataGridView3.AutoGenerateColumns = false;
            dataGridViewScenes_BindingSource();
        }
        #endregion

        #region HotKeys handlers
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.S)) { SaveBook(CurrentPath); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion

        #region Всё дерьмо, незабыть позже передалать и дописать
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
                            pictureBox1.Image = Utils.ImagesWorks.ByteToImage(CPBacks[(int)info.Item.Tag]);
                            return;
                        }
                        if (lst.Name.Equals(listView2.Name))
                        {
                            pictureBox2.Image = null;
                            pictureBox2.Image = Utils.ImagesWorks.ByteToImage(CPLayers[(int)info.Item.Tag]);
                            return;
                        }
                    }
                    break;
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (toolStripComboBox_ListView.SelectedIndex)
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
        #endregion

        #region Add BinaryFile (Image/Sound)
        /// <summary>
        /// опять быстрый костыль, handler of toolStripMenuItem_Add 
        /// </summary>
        private void ContextMenuAddMaterial_Click(object sender, EventArgs e)
        {

            int i = 0;
            if (contextMenuStrip1.Tag.ToString().Equals(listView1.Name))
            {
                if (CPBacks.Count > 0) { i = CPBacks.Keys.Max() + 1; } else { i = 1; }
                Utils.ImageUI result = Utils.ImagesWorks.GetFilesByteArray("Изображения PNG24| *.png", i);
                if (result != null && result.Image != null && result.PreviewSmall != null)
                {
                    CPBacks.Add(i, result.Image);
                    string resultname = i.ToString();
                    imageList_BackBig.Images.Add(resultname, Utils.ImagesWorks.ByteToImage(result.PreviewBig));
                    imageList_BackSmall.Images.Add(resultname, Utils.ImagesWorks.ByteToImage(result.PreviewSmall));
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
                Utils.ImageUI result = Utils.ImagesWorks.GetFilesByteArray("Изображения PNG24| *.png", i);
                if (result != null && result.Image != null && result.PreviewSmall != null)
                {
                    CPLayers.Add(i, result.Image);
                    string resultname = i.ToString();
                    imageList_LayerBig.Images.Add(resultname, Utils.ImagesWorks.ByteToImage(result.PreviewBig));
                    imageList_LayerSmall.Images.Add(resultname, Utils.ImagesWorks.ByteToImage(result.PreviewSmall));
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
                int uid = (int)groupBox_SceneMain.Tag;
                if (CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].ButtonChoice == null) { CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].ButtonChoice = new List<Proto.ProtoScene.protoСhoice>(); }
                using (FormButtonChoice frm = new FormButtonChoice(null))
                {
                    frm.ShowInTaskbar = false;
                    frm.StartPosition = FormStartPosition.Manual;
                    frm.Location = contextMenuStrip1.PointToScreen(Point.Empty);
                    frm.ShowDialog();
                    if (frm.result != null)
                    {
                        CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].ButtonChoice.Add(frm.result);
                        showButtonChoice(uid);
                        Utils.ChaptersDataTable.UpdateRow(new KeyValuePair<int, Proto.ProtoScene.protoRow>(uid, CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid]));
                    }
                }
                return;
            }
            if (contextMenuStrip1.Tag.ToString().Equals(dataGridView3.Name))
            {
                if (CPChapters[toolStripComboBox_Chapters.Text].Scenes == null) { CPChapters[toolStripComboBox_Chapters.Text].Scenes = new Dictionary<int, Proto.ProtoScene.protoRow>(); }
                if (CPChapters[toolStripComboBox_Chapters.Text].Scenes.Count > 0) { i = CPChapters[toolStripComboBox_Chapters.Text].Scenes.Keys.Max() + 1; } else { i = 1; }
                Proto.ProtoScene.protoRow tmp = new Proto.ProtoScene.protoRow
                {
                    Message = string.Empty,
                    Name = string.Empty,
                    ButtonChoice = new List<Proto.ProtoScene.protoСhoice>()
                };
                CPChapters[toolStripComboBox_Chapters.Text].Scenes.Add(i, tmp);
                Utils.ChaptersDataTable.result result = Utils.ChaptersDataTable.AddNewRow(new KeyValuePair<int, Proto.ProtoScene.protoRow>(i, tmp));
                if (result.ex != null) { ShowExeption(result.ex); }
            }
        }
        #endregion

        #region Тут море всякого пиздеца - разобрать и привести в порядок по мере реализации
        private void dataGridViewScenes_BindingSource()
        {

            Utils.ChaptersDataTable.result res = Utils.ChaptersDataTable.ConvertFromProto(new Dictionary<int, Proto.ProtoScene.protoRow>());
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
            UpdateChapterFieldLocker = false;
            if (CPChapters[toolStripComboBox_Chapters.Text].Scenes.ContainsKey(uid))
            {
                textBox1.Text = CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Name;
                textBox2.Text = CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Message;
                groupBox_SceneMain.Text = "Сцена № " + uid.ToString();
                groupBox_SceneMain.Tag = uid;
                checkBox_Autotrans.Checked = CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].AutoJump;
                numericUpDown_AutoTransTiming.Value = Convert.ToDecimal(CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].AutoJumpTimer);
                if (CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Background == 0 && CPChapters[toolStripComboBox_Chapters.Text].Scenes.ContainsKey(uid - 1)) { CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Background = CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid - 1].Background; }
                if (CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Layer == 0 && CPChapters[toolStripComboBox_Chapters.Text].Scenes.ContainsKey(uid - 1)) { CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Layer = CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid - 1].Layer; }
                numericUpDown_Background.Value = CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Background;
                numericUpDown_Layer.Value = CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Layer;
                if (CPBacks.ContainsKey(CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Background)) { pictureBox_Backs.BackgroundImage = Utils.ImagesWorks.ByteToImage(CPBacks[CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Background]); } else { pictureBox_Backs.BackgroundImage = Properties.Resources.Backgraund0; }
                pictureBox_Backs.Image = null;

                if (CPLayers.ContainsKey(CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Layer)) { pictureBox_Backs.Image = Utils.ImagesWorks.ByteToImage(CPLayers[CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Layer]); } else { pictureBox_Backs.Image = Properties.Resources.Character0; }

                showButtonChoice(uid);
            }
            else
            {
                textBox1.Text = string.Empty;
                textBox2.Text = string.Empty;
                groupBox_SceneMain.Text = "Сцена не существует";
                groupBox_SceneMain.Tag = 0;
                checkBox_Autotrans.Checked = false;
                numericUpDown_AutoTransTiming.Value = 0;
                pictureBox_Backs.BackgroundImage = Properties.Resources.Backgraund0;
                pictureBox_Backs.Image = null;
                pictureBox_Backs.Image = Properties.Resources.Character0;
                numericUpDown_Background.Value = 0;
                numericUpDown_Layer.Value = 0;
            }
            UpdateChapterFieldLocker = true;
        }


        private void showButtonChoice(int uid)
        {
            if (CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].ButtonChoice == null)
            { CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].ButtonChoice = new List<Proto.ProtoScene.protoСhoice>(); }
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].ButtonChoice.ToList();
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
            if (((Control)sender).Name.Equals(buttonSelectSceneBackground.Name))
            {
                if (listView1.Items.Count > 0)
                {
                    using (FormSelectImages frm = new FormSelectImages((from ListViewItem item in listView1.Items select (ListViewItem)item.Clone()).ToArray(), imageList_BackBig, imageList_BackSmall, listView1.View)) //listView1.Items.OfType<ListViewItem>().ToArray()
                    {
                        frm.ShowDialog();
                        numericUpDown_Background.Value = frm.result;
                    }
                }
                else
                {
                    DialogResult result = MessageBox.Show("Вы не добавили в том фоновые изображения!" + Environment.NewLine + "Перейти в раздел фоновых изображений для добавления?",
                                                            "Внимание!",
                                                            MessageBoxButtons.YesNo,
                                                            MessageBoxIcon.Question,
                                                            MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Yes) { tabControl1.SelectedIndex = 2; }
                }
            }
            else
            {
                if (listView2.Items.Count > 0)
                {
                    using (FormSelectImages frm = new FormSelectImages((from ListViewItem item in listView2.Items select (ListViewItem)item.Clone()).ToArray(), imageList_LayerBig, imageList_LayerSmall, listView2.View)) //listView1.Items.OfType<ListViewItem>().ToArray()
                    {
                        frm.ShowDialog();
                        numericUpDown_Layer.Value = frm.result;
                    }
                }
                else
                {
                    DialogResult result = MessageBox.Show("Вы не добавили в том изображения персонажей!" + Environment.NewLine + "Перейти в раздел изображений персонажей для добавления?",
                                                            "Внимание!",
                                                            MessageBoxButtons.YesNo,
                                                            MessageBoxIcon.Question,
                                                            MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Yes) { tabControl1.SelectedIndex = 3; }
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
            int uid = checkScenesDic();
            if (uid > 0)
            {
                CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].AutoJump = checkBox_Autotrans.Checked;
            }
            numericUpDown_AutoTransTiming.Enabled = checkBox_Autotrans.Checked;
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            int uid = checkScenesDic();
            if (uid > 0)
            {
                CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].AutoJumpTimer = (float)numericUpDown_AutoTransTiming.Value;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            int uid = checkScenesDic();
            if (uid > 0)
            {
                CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Name = textBox1.Text;
                Utils.ChaptersDataTable.UpdateRow(new KeyValuePair<int, Proto.ProtoScene.protoRow>(uid, CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid]));
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            int uid = checkScenesDic();
            if (uid > 0)
            {
                CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Message = textBox2.Text;
                Utils.ChaptersDataTable.UpdateRow(new KeyValuePair<int, Proto.ProtoScene.protoRow>(uid, CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid]));
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            int uid = checkScenesDic();
            if (uid > 0)
            {
                int backuid = (int)numericUpDown_Background.Value;
                if (backuid > 0)
                {
                    if (CPBacks.ContainsKey(backuid))
                    {
                        pictureBox_Backs.BackgroundImage = Utils.ImagesWorks.ByteToImage(CPBacks[backuid]);
                        CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Background = backuid;
                    }
                    else
                    {
                        DialogResult result = MessageBox.Show("Фонового изображения с таким номером не существует!" + Environment.NewLine + "Отменить изменение?",
                                            "Внимание!",
                                            MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Question,
                                            MessageBoxDefaultButton.Button1);
                        if (result == DialogResult.Yes) { numericUpDown_Background.Value = CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Background; }
                        else
                        {
                            pictureBox_Backs.BackgroundImage = Properties.Resources.Backgraund0;
                        }
                    }
                }
                else { pictureBox_Backs.BackgroundImage = Properties.Resources.Backgraund0; }
            }
        }

        private int checkScenesDic()
        {
            if (UpdateChapterFieldLocker)
            {
                int uid = (int)groupBox_SceneMain.Tag;
                if (CPChapters[toolStripComboBox_Chapters.Text].Scenes.ContainsKey(uid)) { return uid; }
            }
            return 0;
        }



        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            int uid = checkScenesDic();
            if (uid > 0)
            {
                int backuid = (int)numericUpDown_Layer.Value;
                if (backuid > 0)
                {
                    if (CPLayers.ContainsKey(backuid))
                    {
                        pictureBox_Backs.Image = null;
                        pictureBox_Backs.Image = Utils.ImagesWorks.ByteToImage(CPLayers[backuid]);
                        CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Layer = backuid;
                    }
                    else
                    {
                        DialogResult result = MessageBox.Show("Изображения порсонажа с таким номером не существует!" + Environment.NewLine + "Отменить изменение?",
                        "Внимание!",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                        if (result == DialogResult.Yes)
                        { numericUpDown_Layer.Value = CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Layer; }
                        else
                        {
                            pictureBox_Backs.Image = null;
                            pictureBox_Backs.Image = Properties.Resources.Character0;
                        }
                    }
                }
                else
                {
                    pictureBox_Backs.Image = null;
                    pictureBox_Backs.Image = Properties.Resources.Character0;
                }
            }
        }



        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabControl1.SelectedIndex)
            {
                case 0:

                    break;
            }
        }


        private void SaveMaterial_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem control = (ToolStripMenuItem)sender;
            if (control != null)
            {
                if (control.Name.Equals(toolStripMenuItem_SaveBacks.Name)) { SaveMaterials(MaterialsType.Backgrounds, Properties.Settings.Default.path + @"\data"); return; }
                if (control.Name.Equals(toolStripMenuItem_SaveLayers.Name)) { SaveMaterials(MaterialsType.Characters, Properties.Settings.Default.path + @"\data"); return; }
                if (control.Name.Equals(toolStripMenuItem_SaveSounds.Name)) { SaveMaterials(MaterialsType.Music, Properties.Settings.Default.path + @"\data"); }
            }
        }
        #endregion






        #region Reordering, get chapters info  in/from chapters Listbox
        private void listBox_Chapters_MouseDown(object sender, MouseEventArgs e)
        {
            listBox_Chapters.SelectedIndex = listBox_Chapters.IndexFromPoint(e.X, e.Y);
            switch (e.Button)
            {
                case MouseButtons.Right:
                    context_ChapterUp.Enabled = false;
                    context_ChapterDown.Enabled = false;
                    context_ChapterStart.Enabled = false;
                    context_ChapterEnd.Enabled = false;
                    context_ChapterDelete.Enabled = false;
                    contextMenuStrip2.Tag = null;
                    if (listBox_Chapters.SelectedItem != null)
                    {
                        contextMenuStrip2.Tag = listBox_Chapters.SelectedItem;
                        if (listBox_Chapters.SelectedIndex > 0) { context_ChapterUp.Enabled = true; context_ChapterStart.Enabled = true; }
                        if (listBox_Chapters.SelectedIndex < listBox_Chapters.Items.Count - 1) { context_ChapterDown.Enabled = true; context_ChapterEnd.Enabled = true; }
                        context_ChapterDelete.Enabled = true;
                    }
                    contextMenuStrip2.Show(listBox_Chapters.PointToScreen(e.Location));
                    contextMenuStrip2.Show();
                    break;
                case MouseButtons.Left:
                    if (listBox_Chapters.SelectedItem == null) { return; }
                    listBox_Chapters_SelectedIndexChanged(sender, e);
                    listBox_Chapters.DoDragDrop(listBox_Chapters.SelectedItem, DragDropEffects.Move);
                    break;
            }
        }

        private void listBox_Chapters_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(string))) { e.Effect = DragDropEffects.Move; } else { e.Effect = DragDropEffects.None; }
        }

        private void listBox_Chapters_DragDrop(object sender, DragEventArgs e)
        {
            Point point = listBox_Chapters.PointToClient(new Point(e.X, e.Y));
            int index = listBox_Chapters.IndexFromPoint(point);
            if (index < 0)
            {
                index = listBox_Chapters.Items.Count - 1;
            }

            object data = e.Data.GetData(typeof(string));
            listBox_Chapters.Items.Remove(data);
            listBox_Chapters.Items.Insert(index, data);
            listBox_Chapters.SelectedIndex = index;
        }

        private void listBox_Chapters_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_Chapters.SelectedItem != null)
            {
                textBox_ChapterName.Text = (string)listBox_Chapters.SelectedItem;
                textBox_ChapterName.Tag = (string)listBox_Chapters.SelectedItem;
            }
        }

        private void context_ChapterNew_Click(object sender, EventArgs e)
        {
            using (FormChapter frm = new FormChapter(CPChapters))
            {
                frm.ShowInTaskbar = false;
                frm.StartPosition = FormStartPosition.Manual;
                frm.Location = contextMenuStrip2.PointToScreen(Point.Empty);
                frm.ShowDialog();
                if (frm.Result != null)
                {
                    Proto.ProtoChapter.protoRow row = new Proto.ProtoChapter.protoRow
                    {
                        Name = frm.Result,
                        Scenes = new Dictionary<int, Proto.ProtoScene.protoRow>()
                    };
                    CPChapters.Add(frm.Result, row);
                    if (listBox_Chapters.SelectedItem != null && listBox_Chapters.SelectedIndex < listBox_Chapters.Items.Count - 1)
                    {
                        listBox_Chapters.Items.Insert(listBox_Chapters.SelectedIndex + 1, frm.Result);
                    }
                    else
                    {
                        listBox_Chapters.Items.Add(frm.Result);
                    }
                    fillingComboBox_Chapters();
                }
            }
        }
        private void ChapterRename_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox_ChapterName.Text) || string.IsNullOrWhiteSpace(textBox_ChapterName.Text))
            {
                MessageBox.Show("Имя главы не может быть пустым!", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (CPChapters.ContainsKey(textBox_ChapterName.Text))
            {
                MessageBox.Show("Уже есть глава с таким именем!" + Environment.NewLine + "Имена глав должны быть уникальны, назовите главу по другому.", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Proto.ProtoChapter.protoRow row = new Proto.ProtoChapter.protoRow();
            if (CPChapters.TryGetValue((string)textBox_ChapterName.Tag, out row))
            {
                row.Name = textBox_ChapterName.Text;
                CPChapters.Add(textBox_ChapterName.Text, row);
                CPChapters.Remove((string)textBox_ChapterName.Tag);
                toolStripComboBox_Chapters.Items[toolStripComboBox_Chapters.Items.IndexOf((string)textBox_ChapterName.Tag)] = textBox_ChapterName.Text;
                listBox_Chapters.Items[listBox_Chapters.Items.IndexOf((string)textBox_ChapterName.Tag)] = textBox_ChapterName.Text;
            }



        }

        private void fillingComboBox_Chapters()
        {
            toolStripComboBox_Chapters.Items.Clear();
            toolStripComboBox_Chapters.Items.AddRange(listBox_Chapters.Items.OfType<string>().ToArray());
            if (toolStripComboBox_Chapters.SelectedIndex == -1 && toolStripComboBox_Chapters.Items.Count > 0) { toolStripComboBox_Chapters.SelectedIndex = 0; }
        }

        #endregion

        private void toolStripComboBox_Chapters_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(toolStripComboBox_Chapters.Text) || string.IsNullOrWhiteSpace(toolStripComboBox_Chapters.Text))
            {
                if (tabControl1.TabPages.Count == 5) { tabControl1.TabPages.Remove(tab); }
            }
            else
            {
                if (tabControl1.TabPages.Count == 4) { tabControl1.TabPages.Insert(1, tab); }
                Utils.ChaptersDataTable.result ddd = Utils.ChaptersDataTable.ConvertFromProto(CPChapters[toolStripComboBox_Chapters.Text].Scenes);
            }
        }



        #region Create New book
        private void CreateNewBook()
        {
            FolderBrowserDialog browserDialog = new FolderBrowserDialog
            {
                Description = "Укажите пустую папку (или создайте новую) для нового тома",
                SelectedPath = Properties.Settings.Default.path
            };
            string path = SelectFolder(browserDialog);
            if (path != null)
            {
                DirectoryInfo directory = new DirectoryInfo(path);
                DirectoryInfo directorydata = new DirectoryInfo(directory.FullName + @"\data\");
                if (!directory.Exists) { try { Directory.CreateDirectory(directory.FullName); } catch (Exception ex) { ShowExeption(ex); return; } }

                try { if (Directory.Exists(directorydata.FullName)) { Directory.Delete(directorydata.FullName, true); } Directory.CreateDirectory(directorydata.FullName); }
                catch (Exception ex) { ShowExeption(ex); return; }

                if (CPChapters != null) { CPChapters.Clear(); } else { CPChapters = new Dictionary<string, Proto.ProtoChapter.protoRow>(); }
                SaveChapters(path);

                if (CPBacks != null) { CPBacks.Clear(); } else { CPBacks = new Dictionary<int, byte[]>(); }
                SaveMaterials(MaterialsType.Backgrounds, directorydata.FullName);
                if (CPLayers != null) { CPLayers.Clear(); } else { CPLayers = new Dictionary<int, byte[]>(); }
                SaveMaterials(MaterialsType.Characters, directorydata.FullName);
                if (CPMusics != null) { CPMusics.Clear(); } else { CPMusics = new Dictionary<int, byte[]>(); }
                SaveMaterials(MaterialsType.Music, directorydata.FullName);
                RecentBooks(path);
            }
        }
        #endregion

        #region Open Book
        private void OpenBook()
        {
            FolderBrowserDialog browserDialog = new FolderBrowserDialog
            {
                Description = "Укажите папку с данными тома",
                SelectedPath = Properties.Settings.Default.path
            };
            string tmp = SelectFolder(browserDialog);
            if (tmp != null)
            {
                DirectoryInfo directory = new DirectoryInfo(tmp);
                if (!directory.Exists)
                {
                    ShowExeption(new Exception("Папка не найдена!")); return;
                }
                LoadBook(directory.FullName);
            }
        }
        private bool LoadBook(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path)) { return false; }
                DirectoryInfo directory = new DirectoryInfo(path);
                LoadBackground(directory.FullName + @"\data");
                LoadLayers(directory.FullName + @"\data");
                LoadMusic(directory.FullName + @"\data");
                Utils.FilesWorks.result res = null;
                if (CPChapters != null) { CPChapters.Clear(); } else { CPChapters = new Dictionary<string, Proto.ProtoChapter.protoRow>(); }
                res = Utils.FilesWorks.Load(directory.FullName + @"\book.dat", Utils.FilesWorks.Type.Chapters);
                if (res.Ex != null) { ShowExeption(res.Ex); return false; }

                Dictionary<int, Proto.ProtoChapter.protoRow> dic = (Dictionary<int, Proto.ProtoChapter.protoRow>)res.Value;
                listBox_Chapters.Items.Clear();
                for (int i = 0; i < dic.Count; i++)
                {
                    CPChapters.Add(dic[i].Name, dic[i]);
                    listBox_Chapters.Items.Add(dic[i].Name);
                }
                fillingComboBox_Chapters();
                RecentBooks(directory.FullName);
                return true;
            }
            catch (Exception ex) { ShowExeption(ex); return false; }
        }

        private void LoadBackground(string path)
        {
            if (!string.IsNullOrEmpty(path) || !string.IsNullOrWhiteSpace(path))
            {

                if (CPBacks != null) { CPBacks.Clear(); } else { CPBacks = new Dictionary<int, byte[]>(); }
                listView1.Items.Clear(); pictureBox1.Image = null;
                Utils.FilesWorks.result result = Utils.FilesWorks.Load(path + @"\background.dat", Utils.FilesWorks.Type.Binary);
                if (result != null && result.Ex != null) { ShowExeption(result.Ex); }
                else
                {
                    CPBacks = (Dictionary<int, byte[]>)result.Value;
                    foreach (KeyValuePair<int, byte[]> i in CPBacks)
                    {
                        string resultname = i.Key.ToString();
                        imageList_BackBig.Images.Add(resultname, Utils.ImagesWorks.ResizeImage(Utils.ImagesWorks.ByteToImage(i.Value), 255, 255));
                        imageList_BackSmall.Images.Add(resultname, Utils.ImagesWorks.ResizeImage(Utils.ImagesWorks.ByteToImage(i.Value), 32, 32));
                        ListViewItem item = new ListViewItem
                        {
                            ImageKey = resultname,
                            Text = resultname,
                            Tag = i.Key
                        };
                        listView1.Items.Add(item);
                    }
                }
            }
        }

        private void LoadLayers(string path)
        {
            if (CPLayers != null) { CPLayers.Clear(); } else { CPLayers = new Dictionary<int, byte[]>(); }
            listView2.Items.Clear(); pictureBox2.Image = null;
            Utils.FilesWorks.result result = Utils.FilesWorks.Load(path + @"\characters.dat", Utils.FilesWorks.Type.Binary);
            if (result != null && result.Ex != null) { ShowExeption(result.Ex); }
            else
            {
                CPLayers = (Dictionary<int, byte[]>)result.Value;
                foreach (KeyValuePair<int, byte[]> i in CPLayers)
                {
                    string resultname = i.Key.ToString();
                    imageList_LayerBig.Images.Add(resultname, Utils.ImagesWorks.ResizeImage(Utils.ImagesWorks.ByteToImage(i.Value), 255, 255));
                    imageList_LayerSmall.Images.Add(resultname, Utils.ImagesWorks.ResizeImage(Utils.ImagesWorks.ByteToImage(i.Value), 32, 32));
                    ListViewItem item = new ListViewItem
                    {
                        ImageKey = resultname,
                        Text = resultname,
                        Tag = i.Key
                    };
                    listView2.Items.Add(item);
                }
            }

        }

        private void LoadMusic(string path)
        {
            if (CPMusics != null) { CPMusics.Clear(); } else { CPMusics = new Dictionary<int, byte[]>(); }
            Utils.FilesWorks.result result = Utils.FilesWorks.Load(path + @"\music.dat", Utils.FilesWorks.Type.Binary);
            if (result != null && result.Ex != null) { ShowExeption(result.Ex); }
            else
            {
                CPMusics = (Dictionary<int, byte[]>)result.Value;
            }
        }
        #endregion

        #region Saving
        private void SaveBook(string path)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
            {
                FolderBrowserDialog browserDialog = new FolderBrowserDialog
                {
                    Description = "Выберите папку проекта",
                    SelectedPath = Properties.Settings.Default.path
                };
                path = SelectFolder(browserDialog);
                if (path == null) { return; }
            }
            SaveChapters(path);
            SaveMaterials(MaterialsType.Backgrounds, path + @"\data");
            SaveMaterials(MaterialsType.Characters, path + @"\data");
            SaveMaterials(MaterialsType.Music, path + @"\data");
            RecentBooks(path);
        }

        private void SaveChapters(string path)
        {
            Dictionary<int, Proto.ProtoChapter.protoRow> dicsave = new Dictionary<int, Proto.ProtoChapter.protoRow>();
            for (int i = 0; i < listBox_Chapters.Items.Count; i++)
            {
                if (CPChapters[(string)listBox_Chapters.Items[i]].Scenes == null)
                {
                    CPChapters[(string)listBox_Chapters.Items[i]].Scenes = new Dictionary<int, Proto.ProtoScene.protoRow>();
                }
                dicsave.Add(i, CPChapters[(string)listBox_Chapters.Items[i]]);
            }
            Utils.FilesWorks.result result = Utils.FilesWorks.Save(dicsave, path + @"\book.dat");
            if (result != null && result.Ex != null) { ShowExeption(result.Ex); }
        }

        private void SaveMaterials(MaterialsType type, string path)
        {
            Utils.FilesWorks.result result = null;
            DirectoryInfo directory = new DirectoryInfo(path);
            if (!directory.Exists) { directory.Create(); }
            switch (type)
            {
                case MaterialsType.Backgrounds:
                    if (CPBacks != null)
                    {

                        result = Utils.FilesWorks.Save(CPBacks, path + @"\background.dat");
                        if (result != null && result.Ex != null) { ShowExeption(result.Ex); } else { return; }
                    }
                    break;
                case MaterialsType.Characters:
                    if (CPLayers != null)
                    {
                        result = Utils.FilesWorks.Save(CPLayers, path + @"\characters.dat");
                        if (result != null && result.Ex != null) { ShowExeption(result.Ex); } else { return; }
                    }
                    break;
                case MaterialsType.Music:
                    if (CPMusics != null)
                    {
                        result = Utils.FilesWorks.Save(CPMusics, path + @"\music.dat");
                        if (result != null && result.Ex != null) { ShowExeption(result.Ex); } else { return; }
                    }
                    break;
            }

        }
        #endregion

        #region utils&others
        private string SelectFolder(FolderBrowserDialog browserDialog)
        {
            DialogResult result = browserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                DirectoryInfo directory = new DirectoryInfo(browserDialog.SelectedPath);
                if (!directory.Exists)
                {
                    ShowExeption(new Exception("Папка не найдена!")); return null;
                }
                return directory.FullName;
            }
            return null;
        }

        private void RecentBooks(string path)
        {
            CurrentPath = path;
            if (!path.Equals(Properties.Settings.Default.path))
            {
                Properties.Settings.Default.path3 = Properties.Settings.Default.path2;
                Properties.Settings.Default.path2 = Properties.Settings.Default.path1;
                Properties.Settings.Default.path1 = Properties.Settings.Default.path;
                Properties.Settings.Default.path = path;
                Properties.Settings.Default.Save();
            }
            CheckRecent();
        }

        private void ShowExeption(Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// быстрый костыль последних открытых - видимость в меню, потом сделать нормально
        /// </summary>
        private void CheckRecent()
        {
            bool[] flg = new bool[5] { true, true, true, true, true };
            if (string.IsNullOrEmpty(Properties.Settings.Default.path) || string.IsNullOrWhiteSpace(Properties.Settings.Default.path))
            { toolStripMenuItem_Recent0.Text = string.Empty; toolStripMenuItem_Recent0.Visible = false; flg[0] = false; }
            else { toolStripMenuItem_Recent0.Text = Properties.Settings.Default.path; toolStripMenuItem_Recent0.Visible = true; }

            if (string.IsNullOrEmpty(Properties.Settings.Default.path0) || string.IsNullOrWhiteSpace(Properties.Settings.Default.path0))
            { toolStripMenuItem_Recent1.Text = string.Empty; toolStripMenuItem_Recent1.Visible = false; flg[1] = false; }
            else { toolStripMenuItem_Recent1.Text = Properties.Settings.Default.path0; toolStripMenuItem_Recent1.Visible = true; }

            if (string.IsNullOrEmpty(Properties.Settings.Default.path1) || string.IsNullOrWhiteSpace(Properties.Settings.Default.path1))
            { toolStripMenuItem_Recent2.Text = string.Empty; toolStripMenuItem_Recent2.Visible = false; flg[2] = false; }
            else { toolStripMenuItem_Recent2.Text = Properties.Settings.Default.path1; toolStripMenuItem_Recent2.Visible = true; }

            if (string.IsNullOrEmpty(Properties.Settings.Default.path2) || string.IsNullOrWhiteSpace(Properties.Settings.Default.path2))
            { toolStripMenuItem_Recent3.Text = string.Empty; toolStripMenuItem_Recent3.Visible = false; flg[3] = false; }
            else { toolStripMenuItem_Recent3.Text = Properties.Settings.Default.path2; toolStripMenuItem_Recent3.Visible = true; }

            if (string.IsNullOrEmpty(Properties.Settings.Default.path3) || string.IsNullOrWhiteSpace(Properties.Settings.Default.path3))
            { toolStripMenuItem_Recent4.Text = string.Empty; toolStripMenuItem_Recent4.Visible = false; flg[4] = false; }
            else { toolStripMenuItem_Recent4.Text = Properties.Settings.Default.path3; toolStripMenuItem_Recent4.Visible = true; }

            if (flg.All(x => x == false))
            { toolStripMenuItem_Recent.Visible = false; toolStripSeparator9.Visible = false; }
            else { toolStripMenuItem_Recent.Visible = true; toolStripSeparator9.Visible = true; }
        }

        #region Gui: Image Panels uniform width
        private void panel3_SizeChanged(object sender, EventArgs e)
        {
            if (((Control)sender).Name.Equals(panel3.Name)) { panel1.Width = panel3.Width; } else { panel3.Width = panel1.Width; }
        }
        #endregion

        #endregion

        #region Main menu buttons handlers
        private void NewBook_Click(object sender, EventArgs e) { CreateNewBook(); }
        private void OpenBook_Click(object sender, EventArgs e) { OpenBook(); }
        private void SaveBook_Click(object sender, EventArgs e) { SaveBook(CurrentPath); }
        private void SaveBookCopy_Click(object sender, EventArgs e) { SaveBook(string.Empty); }
        private void Exit_Click(object sender, EventArgs e) { Application.Exit(); }
        /// <summary>
        /// быстрый костыль последних открытых, потом сделать нормально
        /// </summary>
        private void RecentN_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            if (!LoadBook(item.Text))
            {
                if (item.Name.Equals(toolStripMenuItem_Recent0.Name)) { Properties.Settings.Default.path = string.Empty; }
                if (item.Name.Equals(toolStripMenuItem_Recent1.Name)) { Properties.Settings.Default.path0 = string.Empty; }
                if (item.Name.Equals(toolStripMenuItem_Recent2.Name)) { Properties.Settings.Default.path1 = string.Empty; }
                if (item.Name.Equals(toolStripMenuItem_Recent3.Name)) { Properties.Settings.Default.path2 = string.Empty; }
                if (item.Name.Equals(toolStripMenuItem_Recent4.Name)) { Properties.Settings.Default.path3 = string.Empty; }
                CheckRecent();
            }
        }
        #endregion


    }
}
