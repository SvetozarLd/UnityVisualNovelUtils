using NAudio.Wave;
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

        #region variables data
        public Dictionary<int, byte[]> CPBacks = new Dictionary<int, byte[]>(); // backgrounds
        public Dictionary<int, byte[]> CPLayers = new Dictionary<int, byte[]>(); // persons
        public Dictionary<int, byte[]> CPMusics = new Dictionary<int, byte[]>(); // musics
        public Dictionary<string, Proto.ProtoChapter.protoRow> CPChapters = new Dictionary<string, Proto.ProtoChapter.protoRow>(); // book

        private DataTable dt; // link of ChaptersDataTable datatable - for scenes datagridview
        private DataTable MusicTable;// DataTable for dataGridView_Musics;
        private bool UpdateChapterFieldLocker = false; // flag for denied update scenes info
        private TabPage tab; // hidding tab with scenes 
        private string CurrentPath = string.Empty; //corrent book path
        private int CurrentSongKey = 0;
        #region for sound
        //private WaveOutEvent outputDevice = new WaveOutEvent();
        //private AudioFileReader audioFile;
        public IWavePlayer mWaveOutDevice;
        public WaveStream mMainOutputStream;
        public WaveChannel32 mVolumeStream;
        #endregion
        #endregion

        #region Constructor + Necessaries for start
        public FormMain()
        {
            InitializeComponent();
        }

        private void SetStartVariables()
        {
            ComboBox_ListView.Items.Add("LargeIcon");
            ComboBox_ListView.Items.Add("SmallIcon");
            ComboBox_ListView.Items.Add("List");
            ComboBox_ListView.Items.Add("Tile");
            ComboBox_ListView.SelectedIndex = 2;
            panel3.Width = 200;

            MusicTable = new DataTable();
            MusicTable.Columns.Add("uid", typeof(int));
            MusicTable.Columns.Add("text", typeof(string));
            dataGridView_Musics.AutoGenerateColumns = false;
            dataGridView_Musics.DataSource = MusicTable;
            dataGridView_ScenesCheckpoints.AutoGenerateColumns = false;
            dataGridView_Scenes.AutoGenerateColumns = false;
            dataGridViewScenes_BindingSource();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            tab = tabControl1.TabPages[1];
            Properties.Settings.Default.Reload();
            CheckRecent();
            tabControl1.TabPages.Remove(tab);
            Utils.ChaptersDataTable.Initialization();
            SetStartVariables();
        }
        #endregion

        #region HotKeys handlers
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.S)) { SaveBook(CurrentPath); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion

        #region Selected Background/Characters/Musics
        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            //ListViewHitTestInfo info = lst.HitTest(e.X, e.Y);
            if (e.Button == MouseButtons.Right)
            {
                ListView lst = (ListView)sender;
                toolStripMenuItem_Edit.Visible = true;
                contextMenuStrip_AddEditRemove.Tag = lst.Name;
                contextMenuStrip_AddEditRemove.Show(lst.PointToScreen(e.Location));
            }
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView lst = (ListView)sender;
            int i = 0;
            if (lst.SelectedItems.Count > 0) { i = (int)lst.SelectedItems[0].Tag; }
            if (lst.Name.Equals(listView_Backgrounds.Name))
            {
                Image tmp = pictureBox2.Image;
                if (i > 0) { pictureBox1.Image = Utils.ImagesWorks.ByteToImage(CPBacks[i]); toolStripMenuItem_Add.Enabled = toolStripMenuItem_Edit.Enabled = toolStripMenuItem_Delete.Enabled = true; }
                else { pictureBox1.Image = Properties.Resources.Backgraund0; toolStripMenuItem_Add.Enabled = true; toolStripMenuItem_Edit.Enabled = toolStripMenuItem_Delete.Enabled = false; }
                if (tmp != null) { tmp.Dispose(); }

            }
            if (lst.Name.Equals(listView_Characters.Name))
            {
                Image tmp = pictureBox2.Image;
                if (i > 0) { pictureBox2.Image = Utils.ImagesWorks.ByteToImage(CPLayers[i]); toolStripMenuItem_Add.Enabled = toolStripMenuItem_Edit.Enabled = toolStripMenuItem_Delete.Enabled = true; }
                else { pictureBox2.Image = pictureBox1.Image = Properties.Resources.Character0; toolStripMenuItem_Add.Enabled = true; toolStripMenuItem_Edit.Enabled = toolStripMenuItem_Delete.Enabled = false; }
                if (tmp != null) { tmp.Dispose(); }
            }
        }


        private void ComboBox_ListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (ComboBox_ListView.SelectedIndex)
            {
                case 0:
                    listView_Backgrounds.View = View.LargeIcon;
                    listView_Characters.View = View.LargeIcon;
                    break;
                case 1:
                    listView_Backgrounds.View = View.SmallIcon;
                    listView_Characters.View = View.SmallIcon;
                    break;
                case 2:
                    listView_Backgrounds.View = View.List;
                    listView_Characters.View = View.List;
                    break;
                case 3:
                    listView_Backgrounds.View = View.Tile;
                    listView_Characters.View = View.Tile;
                    break;
            }
        }
        #endregion



        #region Тут море всякого пиздеца - разобрать и привести в порядок по мере реализации
        private void dataGridViewScenes_BindingSource()
        {

            Utils.ChaptersDataTable.result res = Utils.ChaptersDataTable.ConvertFromProto(new Dictionary<int, Proto.ProtoScene.protoRow>());
            if (res.dt != null)
            {
                dt?.Dispose();
                dt = res.dt;
                dataGridView_Scenes.DataSource = dt;
            }
            else
            {
                MessageBox.Show(res.ex.Message);
            }
        }

        private void dataGridView3_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView_Scenes.SelectedRows != null && dataGridView_Scenes.SelectedRows.Count > 0)
            {
                fillTemplateByScene((int)dataGridView_Scenes.SelectedRows[0].Cells["SceneUid"].Value);
            }
        }


        private void fillTemplateByScene(int uid)
        {
            UpdateChapterFieldLocker = false;
            Image tmpimgback = pictureBox_Backs.BackgroundImage;
            Image tmpimglayer = pictureBox_Backs.Image;
            if (CPChapters[toolStripComboBox_Chapters.Text].Scenes.ContainsKey(uid))
            {
                textBox1.Text = CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Name;
                textBox2.Text = CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Message;
                groupBox_SceneMain.Text = "Сцена № " + uid.ToString();
                groupBox_SceneMain.Tag = uid;
                checkBox_Autotrans.Checked = CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].AutoJump;
                numericUpDown_Sound.Value = CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Sound;
                numericUpDown_AutoTransTiming.Value = Convert.ToDecimal(CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].AutoJumpTimer);
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
            if (tmpimgback != null) { tmpimgback.Dispose(); tmpimgback = null; }
            if (tmpimglayer != null) { tmpimglayer.Dispose(); tmpimglayer = null; }
            UpdateChapterFieldLocker = true;
        }


        private void showButtonChoice(int uid)
        {
            if (CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].ButtonChoice == null)
            { CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].ButtonChoice = new List<Proto.ProtoScene.protoСhoice>(); }
            dataGridView_ScenesCheckpoints.DataSource = null;
            dataGridView_ScenesCheckpoints.DataSource = CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].ButtonChoice.ToList();
            dataGridView_ScenesCheckpoints.Refresh();
        }



        private void dataGridView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                DataGridView dataGridView = (DataGridView)sender;
                contextMenuStrip_AddEditRemove.Tag = dataGridView.Name;
                dataGridView.ClearSelection();
                int i = dataGridView.HitTest(e.X, e.Y).RowIndex;
                if (i > -1) { dataGridView.Rows[i].Selected = true; toolStripMenuItem_Add.Enabled = toolStripMenuItem_Edit.Enabled = toolStripMenuItem_Delete.Enabled = true; } else { dataGridView.ClearSelection(); toolStripMenuItem_Add.Enabled = true; toolStripMenuItem_Edit.Enabled = toolStripMenuItem_Delete.Enabled = false; }
                if (dataGridView.Name.Equals(dataGridView_Scenes.Name)) { toolStripMenuItem_Edit.Visible = false; } else { toolStripMenuItem_Edit.Visible = true; }
                contextMenuStrip_AddEditRemove.Show(dataGridView.PointToScreen(e.Location));
                contextMenuStrip_AddEditRemove.Show();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (((Control)sender).Name.Equals(buttonSelectSceneBackground.Name))
            {
                if (listView_Backgrounds.Items.Count > 0)
                {
                    using (FormSelectImages frm = new FormSelectImages((from ListViewItem item in listView_Backgrounds.Items select (ListViewItem)item.Clone()).ToArray(), imageList_BackBig, imageList_BackSmall, listView_Backgrounds.View)) //listView1.Items.OfType<ListViewItem>().ToArray()
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
                if (listView_Characters.Items.Count > 0)
                {
                    using (FormSelectImages frm = new FormSelectImages((from ListViewItem item in listView_Characters.Items select (ListViewItem)item.Clone()).ToArray(), imageList_LayerBig, imageList_LayerSmall, listView_Characters.View)) //listView1.Items.OfType<ListViewItem>().ToArray()
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
            Image tmp = null;
            if (uid > 0)
            {
                int backuid = (int)numericUpDown_Background.Value;
                if (CPBacks.ContainsKey(backuid))
                {
                    tmp = pictureBox_Backs.BackgroundImage;
                    pictureBox_Backs.BackgroundImage = Utils.ImagesWorks.ByteToImage(CPBacks[backuid]);
                    CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Background = backuid;
                }
                else
                {
                    if (backuid > 0)
                    {
                        DialogResult result = MessageBox.Show("Фонового изображения с таким номером не существует!" + Environment.NewLine + "Отменить изменение?",
                                            "Внимание!",
                                            MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Question,
                                            MessageBoxDefaultButton.Button1);
                        if (result == DialogResult.Yes) { numericUpDown_Background.Value = CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Background; }
                        else { tmp = pictureBox_Backs.BackgroundImage; pictureBox_Backs.BackgroundImage = Properties.Resources.Backgraund0; CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Background = backuid; }
                    }
                    else
                    {
                        tmp = pictureBox_Backs.BackgroundImage;
                        pictureBox_Backs.BackgroundImage = Properties.Resources.Backgraund0;
                        CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Background = 0;
                    }
                }
            }
            else { tmp = pictureBox_Backs.BackgroundImage; pictureBox_Backs.BackgroundImage = Properties.Resources.Backgraund0; }
            if (tmp != null) { tmp.Dispose(); tmp = null; }
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
            Image tmp = null;
            if (uid > 0)
            {
                int backuid = (int)numericUpDown_Layer.Value;
                if (CPLayers.ContainsKey(backuid))
                {
                    tmp = pictureBox_Backs.Image;
                    pictureBox_Backs.Image = Utils.ImagesWorks.ByteToImage(CPLayers[backuid]);
                    CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Layer = backuid;
                }
                else
                {
                    if (backuid > 0)
                    {
                        DialogResult result = MessageBox.Show("Изображения порсонажа с таким номером не существует!" + Environment.NewLine + "Отменить изменение?",
                        "Внимание!",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                        if (result == DialogResult.Yes)
                        { numericUpDown_Layer.Value = CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Layer; }
                        else { tmp = pictureBox_Backs.Image; pictureBox_Backs.Image = Properties.Resources.Character0; CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Layer = backuid; }
                    }
                    else
                    {
                        CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Layer = 0;
                        tmp = pictureBox_Backs.Image; pictureBox_Backs.Image = Properties.Resources.Character0;
                    }
                }

            }
            if (tmp != null) { tmp.Dispose(); tmp = null; }
        }

        private void numericUpDown_Sound_ValueChanged(object sender, EventArgs e)
        {
            int uid = checkScenesDic();
            if (uid > 0)
            {
                int backuid = (int)numericUpDown_Sound.Value;
                if (CPMusics.ContainsKey(backuid))
                {
                    CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Sound = backuid;
                }
                else
                {
                    if (backuid > 0)
                    {
                        DialogResult result = MessageBox.Show("Мзыкальной дорожки с таким номером не существует!" + Environment.NewLine + "Отменить изменение?",
                        "Внимание!",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                        if (result == DialogResult.Yes) { numericUpDown_Sound.Value = CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Sound; } else { CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Sound = backuid; }
                    }
                    else
                    {
                        CPChapters[toolStripComboBox_Chapters.Text].Scenes[uid].Sound = 0;
                    }
                }
            }
        }

        #endregion

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 2 || tabControl1.SelectedIndex == 3) { ComboBox_ListView.Visible = true; }
            else { ComboBox_ListView.Visible = false; }
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



        private void toolStripComboBox_Chapters_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (toolStripComboBox_Chapters.SelectedIndex < 0)
            {
                if (tabControl1.TabPages.Count == 4) { tabControl1.TabPages.Remove(tab); }
            }
            else
            {
                if (tabControl1.TabPages.Count == 3) { tabControl1.TabPages.Insert(1, tab); }
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
                Text = directory.FullName;
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
                Text = path;
                return true;
            }
            catch (Exception ex) { ShowExeption(ex); return false; }
        }

        private void LoadBackground(string path)
        {
            if (!string.IsNullOrEmpty(path) || !string.IsNullOrWhiteSpace(path))
            {

                if (CPBacks != null) { CPBacks.Clear(); } else { CPBacks = new Dictionary<int, byte[]>(); }
                listView_Backgrounds.Items.Clear(); pictureBox1.Image = null;
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
                        listView_Backgrounds.Items.Add(item);
                    }
                }
            }
        }

        private void LoadLayers(string path)
        {
            if (CPLayers != null) { CPLayers.Clear(); } else { CPLayers = new Dictionary<int, byte[]>(); }
            listView_Characters.Items.Clear(); pictureBox2.Image = null;
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
                    listView_Characters.Items.Add(item);
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
                foreach (KeyValuePair<int, byte[]> i in CPMusics)
                {
                    DataRow dr = MusicTable.NewRow();
                    dr["uid"] = i.Key;
                    dr["text"] = Utils.FilesWorks.GetStringOfFileSize(i.Value.Length);
                    MusicTable.Rows.Add(dr);
                }
                dataGridView_Musics.Sort(dataGridView_Musics.Columns[0], System.ComponentModel.ListSortDirection.Ascending);
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
            Text = path;
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

        #region экспорт в юнити
        private void toolStripMenuItem_ScenesExport_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog browserDialog = new FolderBrowserDialog
            {
                Description = "Выберите папку проекта",
                SelectedPath = Properties.Settings.Default.path
            };
            string path = SelectFolder(browserDialog);
            if (path == null) { return; }
            Utils.FilesWorks.result result = Utils.FilesWorks.Save(CPChapters[toolStripComboBox_Chapters.Text].Scenes, path + @"\" + toolStripComboBox_Chapters.SelectedIndex.ToString("0000") + "_scenes.bytes");
            if (result != null && result.Ex != null) { ShowExeption(result.Ex); }

            //Dictionary<int, Proto.ProtoScene.protoRow> dicsave = new Dictionary<int, Proto.ProtoScene.protoRow>();



            //for (int i = 0; i < listBox_Chapters.Items.Count; i++)
            //{
            //    if (CPChapters[(string)listBox_Chapters.Items[i]].Scenes == null)
            //    {
            //        CPChapters[(string)listBox_Chapters.Items[i]].Scenes = new Dictionary<int, Proto.ProtoScene.protoRow>();
            //    }
            //    dicsave.Add(i, CPChapters[(string)listBox_Chapters.Items[i]]);
            //}
            //Utils.FilesWorks.result result = Utils.FilesWorks.Save(dicsave, path + @"\book.dat");
            //if (result != null && result.Ex != null) { ShowExeption(result.Ex); }
        }
        #endregion


        /// <summary>
        /// опять быстрый костыль, handler of toolStripMenuItem##
        /// </summary>
        #region Context Menu handlers: Add/Remove/Edit(images/music/checkpoints scenes)

        #region ContextMenu: Add (images/music/checkpoints scenes)
        private void ContextMenu_Add_Click(object sender, EventArgs e)
        {

            int i = 0; //temp int
            #region Backgrounds
            if (contextMenuStrip_AddEditRemove.Tag.ToString().Equals(listView_Backgrounds.Name))
            {
                if (CPBacks.Count > 0) { i = CPBacks.Keys.Max() + 1; } else { i = 1; }
                Utils.ImageUI result = Utils.ImagesWorks.GetFilesByteArray("Все файлы| *.*", i);
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
                    listView_Backgrounds.Items.Add(item);
                }
                return;
            }
            #endregion
            #region Characters
            if (contextMenuStrip_AddEditRemove.Tag.ToString().Equals(listView_Characters.Name))
            {
                if (CPLayers.Count > 0) { i = CPLayers.Keys.Max() + 1; } else { i = 1; }
                Utils.ImageUI result = Utils.ImagesWorks.GetFilesByteArray("Все файлы| *.*", i);
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
                    listView_Characters.Items.Add(item);
                }
                return;
            }
            #endregion
            #region Scene transitions (edit scene list for jump to ****)
            if (contextMenuStrip_AddEditRemove.Tag.ToString().Equals(dataGridView_ScenesCheckpoints.Name))
            {
                i = (int)groupBox_SceneMain.Tag;
                if (CPChapters[toolStripComboBox_Chapters.Text].Scenes[i].ButtonChoice == null) { CPChapters[toolStripComboBox_Chapters.Text].Scenes[i].ButtonChoice = new List<Proto.ProtoScene.protoСhoice>(); }
                using (FormButtonChoice frm = new FormButtonChoice(null))
                {
                    frm.ShowInTaskbar = false;
                    frm.StartPosition = FormStartPosition.Manual;
                    frm.Location = dataGridView_ScenesCheckpoints.PointToScreen(Point.Empty); //Cursor.Position
                    frm.ShowDialog();
                    if (frm.Result != null)
                    {
                        CPChapters[toolStripComboBox_Chapters.Text].Scenes[i].ButtonChoice.Add(frm.Result);
                        showButtonChoice(i);
                        Utils.ChaptersDataTable.UpdateRow(new KeyValuePair<int, Proto.ProtoScene.protoRow>(i, CPChapters[toolStripComboBox_Chapters.Text].Scenes[i]));
                    }
                }
                return;
            }
            #endregion
            #region Scenes
            if (contextMenuStrip_AddEditRemove.Tag.ToString().Equals(dataGridView_Scenes.Name))
            {
                if (CPChapters[toolStripComboBox_Chapters.Text].Scenes == null) { CPChapters[toolStripComboBox_Chapters.Text].Scenes = new Dictionary<int, Proto.ProtoScene.protoRow>(); }
                if (CPChapters[toolStripComboBox_Chapters.Text].Scenes.Count > 0) { i = CPChapters[toolStripComboBox_Chapters.Text].Scenes.Keys.Max() + 1; } else { i = 1; }
                Proto.ProtoScene.protoRow tmp = new Proto.ProtoScene.protoRow
                {
                    Message = string.Empty,
                    Name = string.Empty,
                    ButtonChoice = new List<Proto.ProtoScene.protoСhoice>()
                };
                Proto.ProtoScene.protoСhoice itempc = new Proto.ProtoScene.protoСhoice()
                {
                    Price = 0,
                    ButtonText = string.Empty,
                    NextScene = i + 1
                };
                if (CPChapters[toolStripComboBox_Chapters.Text].Scenes.ContainsKey(i - 1))
                {
                    tmp.Background = CPChapters[toolStripComboBox_Chapters.Text].Scenes[i - 1].Background;
                    tmp.Layer = CPChapters[toolStripComboBox_Chapters.Text].Scenes[i - 1].Layer;
                    tmp.Sound = CPChapters[toolStripComboBox_Chapters.Text].Scenes[i - 1].Sound;
                }
                tmp.ButtonChoice.Add(itempc);
                CPChapters[toolStripComboBox_Chapters.Text].Scenes.Add(i, tmp);
                Utils.ChaptersDataTable.result result = Utils.ChaptersDataTable.AddNewRow(new KeyValuePair<int, Proto.ProtoScene.protoRow>(i, tmp));
                if (result.ex != null) { ShowExeption(result.ex); }
                return;
            }
            #endregion
            #region ScenesButtonImage
            if (contextMenuStrip_AddEditRemove.Tag.ToString().Equals(pictureBox_Chapter.Name))
            {
                Utils.ImageUI result = Utils.ImagesWorks.GetFilesByteArray("Все файлы| *.*", i);
                if (result != null && result.Image != null)
                {
                    Proto.ProtoChapter.protoRow row = new Proto.ProtoChapter.protoRow();
                    if (CPChapters.TryGetValue((string)textBox_ChapterName.Tag, out row))
                    {
                        row.Preview = null;
                        row.Preview = result.Image;
                        listBox_Chapters_SelectedIndexChanged(null, null);
                    }
                }
                return;
            }
            #endregion
            #region Musics
            if (contextMenuStrip_AddEditRemove.Tag.ToString().Equals(dataGridView_Musics.Name))
            {
                if (CPMusics.Count > 0) { i = CPMusics.Keys.Max() + 1; } else { i = 1; }
                OpenFileDialog openFileDialog1 = new OpenFileDialog
                {
                    InitialDirectory = "c:\\",
                    Filter = "All files (*.*)|*.*",
                    FilterIndex = 2,
                    RestoreDirectory = true
                };
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    Utils.FilesWorks.result result = Utils.FilesWorks.Load(openFileDialog1.FileName, Utils.FilesWorks.Type.File);
                    if (result.Ex != null) { return; }
                    if (LoadAudioFromData(i, (byte[])result.Value))
                    {
                        CPMusics.Add(i, (byte[])result.Value);
                        DataRow dr = MusicTable.NewRow();
                        dr["uid"] = i;
                        dr["text"] = Utils.FilesWorks.GetStringOfFileSize(CPMusics[i].Length);
                        MusicTable.Rows.Add(dr);
                    }
                }
                return;
            }
            #endregion
        }
        #endregion

        #region ContextMenu: Delete (images/music/checkpoints scenes)
        private void toolStripMenuItem_Delete_Click(object sender, EventArgs e)
        {

            int i = 0; //temp int
            #region Backgrounds
            if (contextMenuStrip_AddEditRemove.Tag.ToString().Equals(listView_Backgrounds.Name))
            {
                if (listView_Backgrounds.SelectedItems.Count > 0)
                {
                    i = (int)listView_Backgrounds.SelectedItems[0].Tag;
                    listView_Backgrounds.Items.Remove(listView_Backgrounds.SelectedItems[0]);
                    CPBacks.Remove(i);
                }
                return;
            }
            #endregion
            #region Characters
            if (contextMenuStrip_AddEditRemove.Tag.ToString().Equals(listView_Characters.Name))
            {
                if (listView_Characters.SelectedItems.Count > 0)
                {
                    i = (int)listView_Characters.SelectedItems[0].Tag;
                    listView_Characters.Items.Remove(listView_Characters.SelectedItems[0]);
                    CPLayers.Remove(i);
                }
                return;
            }
            #endregion
            #region Scene transitions (edit scene list for jump to ****)
            if (contextMenuStrip_AddEditRemove.Tag.ToString().Equals(dataGridView_ScenesCheckpoints.Name))
            {
                if (dataGridView_Scenes.SelectedRows != null && dataGridView_Scenes.SelectedRows.Count > 0)
                {
                    i = (int)groupBox_SceneMain.Tag;
                    CPChapters[toolStripComboBox_Chapters.Text].Scenes[i].ButtonChoice.RemoveAt(dataGridView_ScenesCheckpoints.SelectedRows[0].Index);
                    showButtonChoice(i);
                }
                return;
            }
            #endregion
            #region Scenes
            if (contextMenuStrip_AddEditRemove.Tag.ToString().Equals(dataGridView_Scenes.Name))
            {
                if (dataGridView_Scenes.SelectedRows != null && dataGridView_Scenes.SelectedRows.Count > 0)
                {
                    i = (int)dataGridView_Scenes.SelectedRows[0].Cells["SceneUid"].Value;
                    CPChapters[toolStripComboBox_Chapters.Text].Scenes.Remove(i);
                    Utils.ChaptersDataTable.DeleteRow(i);
                }
                return;
            }
            #endregion
            #region ScenesButtonImage
            if (contextMenuStrip_AddEditRemove.Tag.ToString().Equals(pictureBox_Chapter.Name))
            {
                Proto.ProtoChapter.protoRow row = new Proto.ProtoChapter.protoRow();
                if (CPChapters.TryGetValue((string)textBox_ChapterName.Tag, out row))
                {
                    row.Preview = null;
                    listBox_Chapters_SelectedIndexChanged(null, null);
                }
                return;
            }
            #endregion
            #region music
            if (contextMenuStrip_AddEditRemove.Tag.ToString().Equals(dataGridView_Musics.Name))
            {
                if (dataGridView_Musics.SelectedRows != null && dataGridView_Musics.SelectedRows.Count > 0)
                {
                    i = (int)dataGridView_Musics.SelectedRows[0].Cells[0].Value;
                    if (CurrentSongKey == i) { AudioUnload(); }
                    CPMusics.Remove(i);
                    DataRow row = MusicTable.Select("uid =" + i).Single();
                    row.Delete();
                }
                return;
            }
            #endregion
        }
        #endregion

        #region ContextMenu: Edit (images/music/checkpoints scenes)
        private void toolStripMenuItem_Edit_Click(object sender, EventArgs e)
        {
            int i = 0; // temp int
            #region Backgrounds
            if (contextMenuStrip_AddEditRemove.Tag.ToString().Equals(listView_Backgrounds.Name))
            {
                if (listView_Backgrounds.SelectedItems.Count > 0)
                {
                    i = (int)listView_Backgrounds.SelectedItems[0].Tag;
                    Utils.ImageUI result = Utils.ImagesWorks.GetFilesByteArray("Все файлы| *.*", i);
                    if (result != null && result.Image != null && result.PreviewSmall != null)
                    {
                        CPBacks[i] = null;
                        CPBacks[i] = result.Image;
                        string resultname = i.ToString();

                        Image imageBig = imageList_BackBig.Images[resultname];
                        Image imageSmall = imageList_BackSmall.Images[resultname];
                        imageList_BackBig.Images.RemoveByKey(resultname);
                        imageList_BackSmall.Images.RemoveByKey(resultname);
                        imageList_BackBig.Images.Add(resultname, Utils.ImagesWorks.ByteToImage(result.PreviewBig));
                        imageList_BackSmall.Images.Add(resultname, Utils.ImagesWorks.ByteToImage(result.PreviewSmall));
                        listView_Backgrounds.SelectedItems[0].ImageIndex = imageList_BackBig.Images.IndexOfKey(resultname);
                        listView_Backgrounds.Refresh();
                        if (imageBig != null) { imageBig.Dispose(); }
                        if (imageSmall != null) { imageSmall.Dispose(); }
                    }
                }
                return;
            }
            #endregion
            #region Characters
            if (contextMenuStrip_AddEditRemove.Tag.ToString().Equals(listView_Characters.Name))
            {
                if (listView_Characters.SelectedItems.Count > 0)
                {
                    i = (int)listView_Characters.SelectedItems[0].Tag;
                    Utils.ImageUI result = Utils.ImagesWorks.GetFilesByteArray("Все файлы| *.*", i);
                    if (result != null && result.Image != null && result.PreviewSmall != null)
                    {
                        CPLayers[i] = null;
                        CPLayers[i] = result.Image;
                        string resultname = i.ToString();

                        Image imageBig = imageList_LayerBig.Images[resultname];
                        Image imageSmall = imageList_LayerSmall.Images[resultname];
                        imageList_LayerBig.Images.RemoveByKey(resultname);
                        imageList_LayerSmall.Images.RemoveByKey(resultname);
                        imageList_LayerBig.Images.Add(resultname, Utils.ImagesWorks.ByteToImage(result.PreviewBig));
                        imageList_LayerSmall.Images.Add(resultname, Utils.ImagesWorks.ByteToImage(result.PreviewSmall));
                        listView_Characters.SelectedItems[0].ImageIndex = imageList_LayerBig.Images.IndexOfKey(resultname);
                        listView_Characters.Refresh();
                        if (imageBig != null) { imageBig.Dispose(); }
                        if (imageSmall != null) { imageSmall.Dispose(); }
                    }
                }
                return;
            }
            #endregion
            #region Scene transitions (edit scene list for jump to ****)
            if (contextMenuStrip_AddEditRemove.Tag.ToString().Equals(dataGridView_ScenesCheckpoints.Name))
            {
                i = (int)groupBox_SceneMain.Tag;
                using (FormButtonChoice frm = new FormButtonChoice(CPChapters[toolStripComboBox_Chapters.Text].Scenes[i].ButtonChoice[dataGridView_ScenesCheckpoints.SelectedRows[0].Index]))
                {
                    frm.ShowInTaskbar = false;
                    frm.StartPosition = FormStartPosition.Manual;
                    frm.Location = dataGridView_ScenesCheckpoints.PointToScreen(Point.Empty);
                    frm.ShowDialog();
                    if (frm.Result != null)
                    {
                        CPChapters[toolStripComboBox_Chapters.Text].Scenes[i].ButtonChoice[dataGridView_ScenesCheckpoints.SelectedRows[0].Index] = frm.Result;
                        showButtonChoice(i);
                        Utils.ChaptersDataTable.UpdateRow(new KeyValuePair<int, Proto.ProtoScene.protoRow>(i, CPChapters[toolStripComboBox_Chapters.Text].Scenes[i]));
                    }
                }
                return;
            }
            #endregion
            #region ScenesButtonImage
            if (contextMenuStrip_AddEditRemove.Tag.ToString().Equals(pictureBox_Chapter.Name))
            {
                Utils.ImageUI result = Utils.ImagesWorks.GetFilesByteArray("Все файлы| *.*", i);
                if (result != null && result.Image != null)
                {
                    Proto.ProtoChapter.protoRow row = new Proto.ProtoChapter.protoRow();
                    if (CPChapters.TryGetValue((string)textBox_ChapterName.Tag, out row))
                    {
                        row.Preview = null;
                        row.Preview = result.Image;
                        listBox_Chapters_SelectedIndexChanged(null, null);
                    }
                }
                return;
            }
            #endregion
            #region Music
            if (contextMenuStrip_AddEditRemove.Tag.ToString().Equals(dataGridView_Musics.Name))
            {
                if (dataGridView_Musics.SelectedRows != null && dataGridView_Musics.SelectedRows.Count > 0)
                {
                    i = (int)dataGridView_Musics.SelectedRows[0].Cells[0].Value;
                    if (CurrentSongKey == i) { AudioUnload(); }
                    OpenFileDialog openFileDialog1 = new OpenFileDialog
                    {
                        InitialDirectory = "c:\\",
                        Filter = "All files (*.*)|*.*",
                        FilterIndex = 2,
                        RestoreDirectory = true
                    };
                    if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        Utils.FilesWorks.result result = Utils.FilesWorks.Load(openFileDialog1.FileName, Utils.FilesWorks.Type.File);
                        if (result.Ex != null) { return; }
                        if (LoadAudioFromData(i, (byte[])result.Value))
                        {
                            CPMusics[i] = null;
                            CPMusics[i] = (byte[])result.Value;
                            DataRow row = MusicTable.Select("uid =" + i).Single();
                            row["uid"] = i;
                            row["text"] = Utils.FilesWorks.GetStringOfFileSize(CPMusics[i].Length);
                        }
                    }
                }
                return;
            }
            #endregion
        }
        private void dataGridView_MouseDoubleClick(object sender, MouseEventArgs e) { contextMenuStrip_AddEditRemove.Tag = ((DataGridView)sender).Name; toolStripMenuItem_Edit_Click(null, null); }
        private void dataGridView_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) { contextMenuStrip_AddEditRemove.Tag = ((DataGridView)sender).Name; toolStripMenuItem_Edit_Click(null, null); } }

        #endregion

        #endregion

        #region Musics

        public bool LoadAudioFromData(int SongKey, byte[] data)
        {
            try
            {
                AudioUnload();
                MemoryStream tmpStr = new MemoryStream(data);
                mMainOutputStream = new Mp3FileReader(tmpStr);
                mVolumeStream = new WaveChannel32(mMainOutputStream);
                mWaveOutDevice = new WaveOut();
                mWaveOutDevice.Init(mVolumeStream);
                mWaveOutDevice.Volume = trackBar1.Value / 100f;
                mWaveOutDevice.Play();
                CurrentSongKey = SongKey;
                return true;
            }
            catch (Exception ex) { ShowExeption(ex); return false; }
        }

        public void AudioUnload()
        {
            if (mWaveOutDevice != null) { mWaveOutDevice.Stop(); }
            if (mMainOutputStream != null)
            {
                mVolumeStream.Close();
                mVolumeStream = null;
                mMainOutputStream.Close();
                mMainOutputStream = null;
            }
            if (mWaveOutDevice != null) { mWaveOutDevice.Dispose(); mWaveOutDevice = null; }
        }

        private void AudioVolume_Click(object sender, EventArgs e)
        {
            if (mWaveOutDevice != null) { mWaveOutDevice.Volume = trackBar1.Value / 100f; }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            AudioUnload();
        }

        private void dataGridView_Musics_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) { return; }
            if (e.ColumnIndex == 2)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);

                int w = Properties.Resources.Play_16x16.Width;
                int h = Properties.Resources.Play_16x16.Height;
                int x = e.CellBounds.Left + (e.CellBounds.Width - w) / 2;
                int y = e.CellBounds.Top + (e.CellBounds.Height - h) / 2;

                e.Graphics.DrawImage(Properties.Resources.Play_16x16, new Rectangle(x, y, w, h));
                e.Handled = true;
            }
        }

        private void dataGridView_Musics_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) { return; }
            if (e.ColumnIndex == 2)
            {
                int i = (int)dataGridView_Musics.Rows[e.RowIndex].Cells[0].Value;
                if (CPMusics.ContainsKey(i)) { LoadAudioFromData(i, CPMusics[i]); }
            }
        }
        #endregion

        #region Reordering, get chapters info  in/from chapters Listbox
        #region Chapters operations (Add/Move/Remove)
        private void ChaptersRearrange_Click(object sender, EventArgs e)
        {
            if (listBox_Chapters.SelectedIndex < 0) { return; }
            ToolStripMenuItem control = (ToolStripMenuItem)sender;
            object item = listBox_Chapters.SelectedItem;
            int itemindex = listBox_Chapters.SelectedIndex;
            listBox_Chapters.Items.RemoveAt(listBox_Chapters.SelectedIndex);
            switch (control.Name)
            {
                case "contextMenuStrip_ChapterStart":
                    listBox_Chapters.Items.Insert(0, item);
                    break;
                case "contextMenuStrip_ChapterEnd":
                    listBox_Chapters.Items.Add(item);
                    break;
                case "contextMenuStrip_ChapterUp":
                    itemindex--;
                    if (itemindex < 0) { itemindex = 0; }
                    listBox_Chapters.Items.Insert(itemindex, item);
                    break;
                case "contextMenuStrip_ChapterDown":
                    itemindex++;
                    if (itemindex > listBox_Chapters.Items.Count) { listBox_Chapters.Items.Add(item); }
                    else { listBox_Chapters.Items.Insert(itemindex, item); }
                    break;
            }

        }

        private void contextMenuStrip_ChapterDelete_Click(object sender, EventArgs e)
        {
            if (listBox_Chapters.SelectedIndex < 0) { return; }
            if (CPChapters.ContainsKey((string)listBox_Chapters.SelectedItem)) { CPChapters.Remove((string)listBox_Chapters.SelectedItem); }
            toolStripComboBox_Chapters.Items.Remove(listBox_Chapters.SelectedItem);
            if (toolStripComboBox_Chapters.Items.Count > 0) { toolStripComboBox_Chapters.SelectedIndex = 0; }
            toolStripComboBox_Chapters_SelectedIndexChanged(null, null);
            listBox_Chapters.Items.RemoveAt(listBox_Chapters.SelectedIndex);
        }

        private void context_ChapterNew_Click(object sender, EventArgs e)
        {
            using (FormChapter frm = new FormChapter(CPChapters))
            {
                frm.ShowInTaskbar = false;
                frm.StartPosition = FormStartPosition.Manual;
                frm.Location = contextMenuStrip_Chapters.PointToScreen(Point.Empty);
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
        #endregion
        #region Chapter Info (Show\Rename\Image)
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

        private void listBox_Chapters_MouseDown(object sender, MouseEventArgs e)
        {
            listBox_Chapters.SelectedIndex = listBox_Chapters.IndexFromPoint(e.X, e.Y);
            switch (e.Button)
            {
                case MouseButtons.Right:
                    contextMenuStrip_ChapterUp.Enabled = false;
                    contextMenuStrip_ChapterDown.Enabled = false;
                    contextMenuStrip_ChapterStart.Enabled = false;
                    contextMenuStrip_ChapterEnd.Enabled = false;
                    contextMenuStrip_ChapterDelete.Enabled = false;
                    contextMenuStrip_Chapters.Tag = null;
                    if (listBox_Chapters.SelectedItem != null)
                    {
                        contextMenuStrip_Chapters.Tag = listBox_Chapters.SelectedItem;
                        if (listBox_Chapters.SelectedIndex > 0) { contextMenuStrip_ChapterUp.Enabled = true; contextMenuStrip_ChapterStart.Enabled = true; }
                        if (listBox_Chapters.SelectedIndex < listBox_Chapters.Items.Count - 1) { contextMenuStrip_ChapterDown.Enabled = true; contextMenuStrip_ChapterEnd.Enabled = true; }
                        contextMenuStrip_ChapterDelete.Enabled = true;
                    }
                    contextMenuStrip_Chapters.Show(listBox_Chapters.PointToScreen(e.Location));
                    contextMenuStrip_Chapters.Show();
                    break;
                case MouseButtons.Left:
                    if (listBox_Chapters.SelectedItem == null) { return; }
                    listBox_Chapters_SelectedIndexChanged(sender, e);
                    listBox_Chapters.DoDragDrop(listBox_Chapters.SelectedItem, DragDropEffects.Move);
                    break;
            }
        }

        private void listBox_Chapters_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_Chapters.SelectedIndex < 0)
            {
                textBox_ChapterName.Tag = string.Empty;
                Image tmp = pictureBox_Chapter.Image;
                pictureBox_Chapter.Image = Properties.Resources.chapter0;
                if (tmp != null) { tmp.Dispose(); }
                return;
            }


            Proto.ProtoChapter.protoRow row = new Proto.ProtoChapter.protoRow();

            if (CPChapters.TryGetValue((string)listBox_Chapters.SelectedItem, out row))
            {
                textBox_ChapterName.Text = row.Name;
                textBox_ChapterName.Tag = row.Name;
                Image tmp = pictureBox_Chapter.Image;
                pictureBox_Chapter.Image = Utils.ImagesWorks.ByteToImage(row.Preview);
                if (tmp != null) { tmp.Dispose(); }
            }
        }

        private void fillingComboBox_Chapters()
        {
            toolStripComboBox_Chapters.Items.Clear();
            toolStripComboBox_Chapters.Items.AddRange(listBox_Chapters.Items.OfType<string>().ToArray());
            if (toolStripComboBox_Chapters.SelectedIndex == -1 && toolStripComboBox_Chapters.Items.Count > 0) { toolStripComboBox_Chapters.SelectedIndex = 0; }
        }

        private void ChapterPicture_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                PictureBox control = (PictureBox)sender;
                contextMenuStrip_AddEditRemove.Tag = control.Name;
                if (control.Image == null)
                {
                    toolStripMenuItem_Add.Enabled = true;
                    toolStripMenuItem_Edit.Visible = true;
                    toolStripMenuItem_Edit.Enabled = toolStripMenuItem_Delete.Enabled = false;
                }
                else
                {
                    toolStripMenuItem_Add.Enabled = false;
                    toolStripMenuItem_Edit.Visible = true;
                    toolStripMenuItem_Edit.Enabled = toolStripMenuItem_Delete.Enabled = true;
                }
                if (!control.Name.Equals(pictureBox_Chapter.Name) || listBox_Chapters.SelectedItem != null) { contextMenuStrip_AddEditRemove.Show(control.PointToScreen(e.Location)); }
                else { MessageBox.Show("Для установки изображения главы, вы сначала должны выбрать эту главу..", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); }
            }
        }

        #endregion

        #endregion

        private void buttonSelectSceneSound_Click(object sender, EventArgs e)
        {
            using (FormSelectMusic musicselectform = new FormSelectMusic(CPMusics, this))
            {
                musicselectform.ShowDialog();
                if (musicselectform.result > 0) { numericUpDown_Sound.Value = musicselectform.result; }
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            using (var Frm = new FormGoogleDrive())
            {
                Frm.ShowDialog();
            }
        }
    }
}
