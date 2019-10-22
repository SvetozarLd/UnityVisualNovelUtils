using System;
using System.Windows.Forms;

namespace SceneCreator.Forms
{
    public partial class FormButtonChoice : Form
    {
        public Proto.ProtoChapters.protoСhoice result { get; set; }

        public FormButtonChoice(Proto.ProtoChapters.protoСhoice item)
        {
            InitializeComponent();
            result = item;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            result = null;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            result = new Proto.ProtoChapters.protoСhoice
            {
                nextscene = (int)numericUpDown2.Value,
                Price = (int)numericUpDown1.Value,
                Text = textBox1.Text
            };
            Close();
        }
    }
}
