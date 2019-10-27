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
    public partial class FormSelectMusic : Form
    {
        public int result { get; set; }
        Dictionary<int, byte[]> musics;
        FormMain main = null;
        public FormSelectMusic(Dictionary<int, byte[]> CPMusics, FormMain form)
        {
            InitializeComponent();
            musics = CPMusics;
            main = form;
        }

        private void FormSelectMusic_Load(object sender, EventArgs e)
        {
            if (musics == null) { return; }
            foreach (var item in musics)
            {
                listBox1.Items.Add(item.Key);
            }
            if (listBox1.Items.Count > 0) { listBox1.SelectedIndex = 0; }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (musics == null || main == null || listBox1.SelectedIndex <0) { return; }
            int i = (int)listBox1.SelectedItem;
            if (musics.ContainsKey(i)){main.LoadAudioFromData(i, musics[i]);}
        }

        private void button2_Click(object sender, EventArgs e)
        {
            result = 0;
            main.AudioUnload();
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            result = (int)listBox1.SelectedItem;
            main.AudioUnload();
            Close();
        }
    }
}
