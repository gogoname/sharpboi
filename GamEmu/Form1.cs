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
using System.Threading;

namespace SharpBoi
{
    public partial class Form1 : Form
    {
        RAM ram;
        CPU cpu;
        public Form1()
        {
            InitializeComponent();
            this.Text = "GamEmu";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            buttonStep.Hide();
            buttonRun.Hide();
            numericUpDownRun.Hide();
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                dialog.Filter = "Gameboy files (*.gb)|*.gb";
                dialog.RestoreDirectory = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = dialog.FileName;
                    ram = new RAM();
                    byte[] instructions = ParseFile(filePath);
                    cpu = new CPU(ram, instructions);
                    MessageBox.Show("File has been loaded successfully, use the STEP or PLAY button in order to run it.", "Success", MessageBoxButtons.OK);
                    buttonStep.Show();
                    buttonRun.Show();
                    numericUpDownRun.Show();
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

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void buttonStep_Click(object sender, EventArgs e)
        {
            if (cpu is null)
            {
                MessageBox.Show("You must first load a file", "ERROR", MessageBoxButtons.OK);
            }
            else
            {
                cpu.Step();
                labelPC.Text = "PC: " + cpu.PC.value.ToString("X");
                labelAF.Text = "AF: " + cpu.AF.value.ToString("X");
                labelBC.Text = "BC: " + cpu.BC.value.ToString("X");
                labelDE.Text = "DE: " + cpu.DE.value.ToString("X");
                labelHL.Text = "HL: " + cpu.HL.value.ToString("X");
                labelA.Text = "A: " + cpu.AF.GetHigh().value.ToString("X");
                labelF.Text = "F: " + cpu.AF.GetLow().value.ToString("X");
                labelB.Text = "B: " + cpu.BC.GetHigh().value.ToString("X");
                labelC.Text = "C: " + cpu.BC.GetLow().value.ToString("X");
                labelD.Text = "D: " + cpu.DE.GetHigh().value.ToString("X");
                labelE.Text = "E: " + cpu.DE.GetLow().value.ToString("X");
                labelH.Text = "H: " + cpu.HL.GetHigh().value.ToString("X");
                labelL.Text = "L: " + cpu.HL.GetLow().value.ToString("X");
            }
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            if (cpu is null)
            {
                MessageBox.Show("You must first load a file", "ERROR", MessageBoxButtons.OK);
            }
            else
            {
                if (numericUpDownRun.Value == 0)
                {
                    MessageBox.Show("The number of steps must be higher than 0", "ERROR", MessageBoxButtons.OK);
                }
                else
                {
                    for (int i = 0; i < numericUpDownRun.Value; i++)
                    {
                        cpu.Step();
                    }
                    labelPC.Text = "PC: " + cpu.PC.value.ToString("X");
                    labelAF.Text = "AF: " + cpu.AF.value.ToString("X");
                    labelBC.Text = "BC: " + cpu.BC.value.ToString("X");
                    labelDE.Text = "DE: " + cpu.DE.value.ToString("X");
                    labelHL.Text = "HL: " + cpu.HL.value.ToString("X");
                    labelA.Text = "A: " + cpu.AF.GetHigh().value.ToString("X");
                    labelF.Text = "F: " + cpu.AF.GetLow().value.ToString("X");
                    labelB.Text = "B: " + cpu.BC.GetHigh().value.ToString("X");
                    labelC.Text = "C: " + cpu.BC.GetLow().value.ToString("X");
                    labelD.Text = "D: " + cpu.DE.GetHigh().value.ToString("X");
                    labelE.Text = "E: " + cpu.DE.GetLow().value.ToString("X");
                    labelH.Text = "H: " + cpu.HL.GetHigh().value.ToString("X");
                    labelL.Text = "L: " + cpu.HL.GetLow().value.ToString("X");
                }
            }
        }
    }
}
