using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace SharpBoi
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StreamReader reader;
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                dialog.Filter = "Gameboy files (*.gb)|*.gb";
                dialog.RestoreDirectory = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = dialog.FileName;
                    RAM ram = new RAM();
                    CPU cpu = new CPU(ram);
                    byte[] instructions = ParseFile(filePath);
                    cpu.Start(instructions);
                }
                else
                {
                    MessageBox.Show("There was an unknown error, you might not have access to the file.", "ERROR", MessageBoxButtons.OK);
                }
            }
        }
        private byte[] ParseFile(string filePath)
        {
            FileStream gbFile = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(gbFile);
            return br.ReadBytes((int)gbFile.Length);
        }
    }
}
