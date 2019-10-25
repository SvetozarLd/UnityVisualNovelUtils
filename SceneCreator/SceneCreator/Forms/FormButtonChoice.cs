using System;
using System.Windows.Forms;

namespace SceneCreator.Forms
{
    public partial class FormButtonChoice : Form
    {
        public Proto.ProtoScene.protoСhoice Result { get; set; }

        public FormButtonChoice(Proto.ProtoScene.protoСhoice item)
        {
            InitializeComponent();
            Result = item;
            if (item!= null)
            {
                textBox1.Text = Result.ButtonText;
                numericUpDown1.Value = Result.Price;
                numericUpDown2.Value = Result.NextScene;
            }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Result = null;
            Close();
        }

        private void Save_Click(object sender, EventArgs e)
        {
            Result = new Proto.ProtoScene.protoСhoice
            {
                NextScene = (int)numericUpDown2.Value,
                Price = (int)numericUpDown1.Value,
                ButtonText = textBox1.Text
            };
            Close();
        }

        private void ChangeCheckpoint_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) { Save_Click(null, null); }
            if (e.KeyCode == Keys.Escape) { Cancel_Click(null, null); }
        }
    }
}
