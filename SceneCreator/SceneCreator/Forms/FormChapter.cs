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
    public partial class FormChapter : Form
    {
        Dictionary<string, Proto.ProtoChapter.protoRow> chaptersLink;

        public string Result { get; set; }
        public FormChapter(Dictionary<string, Proto.ProtoChapter.protoRow> ChaptersLink)
        {
            InitializeComponent();
            chaptersLink = ChaptersLink;
            Result = null;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Result = null;
            Close();
        }

        private void button1_Click(object sender, EventArgs e){Save();}

        private void textBox1_KeyDown(object sender, KeyEventArgs e){if (e.KeyCode == Keys.Enter) { Save(); } if (e.KeyCode == Keys.Escape) { button2_Click(sender, e); } }


        private void Save()
        {
            if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Имя главы не может быть пустым!", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Focus();
                return;
            }
            if (chaptersLink.ContainsKey(textBox1.Text))
            {
                MessageBox.Show("Уже есть глава с таким именем!" + Environment.NewLine + "Имена глав должны быть уникальны, назовите главу по другому.", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Focus();
                return;
            }
            Result = textBox1.Text;
            Close();
        }
    }
}
