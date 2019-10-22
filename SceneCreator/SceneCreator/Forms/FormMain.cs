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
    public partial class FormMain : Form
    {
        DataTable scenesDt = new DataTable();
        DataTable backsDt = new DataTable();
        DataTable layersDt = new DataTable();



        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {

        }


        private void LoadScenes(string path)
        {

        }

        private void SaveScenes(string path)
        {

        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Right:

                    break;
            }
            
        }

        
    }
}
