using System;
using System.Windows.Forms;

namespace SceneCreator.Forms
{
    public partial class FormButtonChoice : Form
    {
        public Proto.ProtoScene.protoСhoice result { get; set; }

        public FormButtonChoice(Proto.ProtoScene.protoСhoice item)
        {
            InitializeComponent();
            result = item;
            if (item!= null)
            {
                textBox1.Text = result.ButtonText;
                numericUpDown1.Value = result.Price;
                numericUpDown2.Value = result.NextScene;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            result = null;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            result = new Proto.ProtoScene.protoСhoice
            {
                NextScene = (int)numericUpDown2.Value,
                Price = (int)numericUpDown1.Value,
                ButtonText = textBox1.Text
            };
            Close();
        }

        private void numericUpDown2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) { button1_Click(null, null); }
            if (e.KeyCode == Keys.Escape) { button2_Click(null, null); }
        }
    }
}
