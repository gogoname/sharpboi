namespace SharpBoi
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonStep = new System.Windows.Forms.Button();
            this.labelPC = new System.Windows.Forms.Label();
            this.labelA = new System.Windows.Forms.Label();
            this.labelB = new System.Windows.Forms.Label();
            this.labelC = new System.Windows.Forms.Label();
            this.labelD = new System.Windows.Forms.Label();
            this.labelE = new System.Windows.Forms.Label();
            this.labelH = new System.Windows.Forms.Label();
            this.labelL = new System.Windows.Forms.Label();
            this.labelF = new System.Windows.Forms.Label();
            this.labelAF = new System.Windows.Forms.Label();
            this.labelBC = new System.Windows.Forms.Label();
            this.labelDE = new System.Windows.Forms.Label();
            this.labelHL = new System.Windows.Forms.Label();
            this.buttonRun = new System.Windows.Forms.Button();
            this.numericUpDownRun = new System.Windows.Forms.NumericUpDown();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRun)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.White;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.openToolStripMenuItem.Text = "Open...";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // buttonStep
            // 
            this.buttonStep.Location = new System.Drawing.Point(247, 74);
            this.buttonStep.Name = "buttonStep";
            this.buttonStep.Size = new System.Drawing.Size(59, 59);
            this.buttonStep.TabIndex = 1;
            this.buttonStep.Text = "STEP";
            this.buttonStep.UseVisualStyleBackColor = true;
            this.buttonStep.Click += new System.EventHandler(this.buttonStep_Click);
            // 
            // labelPC
            // 
            this.labelPC.AutoSize = true;
            this.labelPC.Location = new System.Drawing.Point(470, 296);
            this.labelPC.Name = "labelPC";
            this.labelPC.Size = new System.Drawing.Size(21, 13);
            this.labelPC.TabIndex = 3;
            this.labelPC.Text = "PC";
            // 
            // labelA
            // 
            this.labelA.AutoSize = true;
            this.labelA.Location = new System.Drawing.Point(244, 189);
            this.labelA.Name = "labelA";
            this.labelA.Size = new System.Drawing.Size(14, 13);
            this.labelA.TabIndex = 4;
            this.labelA.Text = "A";
            // 
            // labelB
            // 
            this.labelB.AutoSize = true;
            this.labelB.Location = new System.Drawing.Point(244, 216);
            this.labelB.Name = "labelB";
            this.labelB.Size = new System.Drawing.Size(14, 13);
            this.labelB.TabIndex = 5;
            this.labelB.Text = "B";
            // 
            // labelC
            // 
            this.labelC.AutoSize = true;
            this.labelC.Location = new System.Drawing.Point(244, 242);
            this.labelC.Name = "labelC";
            this.labelC.Size = new System.Drawing.Size(14, 13);
            this.labelC.TabIndex = 6;
            this.labelC.Text = "C";
            // 
            // labelD
            // 
            this.labelD.AutoSize = true;
            this.labelD.Location = new System.Drawing.Point(244, 269);
            this.labelD.Name = "labelD";
            this.labelD.Size = new System.Drawing.Size(15, 13);
            this.labelD.TabIndex = 7;
            this.labelD.Text = "D";
            // 
            // labelE
            // 
            this.labelE.AutoSize = true;
            this.labelE.Location = new System.Drawing.Point(244, 296);
            this.labelE.Name = "labelE";
            this.labelE.Size = new System.Drawing.Size(14, 13);
            this.labelE.TabIndex = 8;
            this.labelE.Text = "E";
            // 
            // labelH
            // 
            this.labelH.AutoSize = true;
            this.labelH.Location = new System.Drawing.Point(244, 357);
            this.labelH.Name = "labelH";
            this.labelH.Size = new System.Drawing.Size(15, 13);
            this.labelH.TabIndex = 9;
            this.labelH.Text = "H";
            // 
            // labelL
            // 
            this.labelL.AutoSize = true;
            this.labelL.Location = new System.Drawing.Point(245, 392);
            this.labelL.Name = "labelL";
            this.labelL.Size = new System.Drawing.Size(13, 13);
            this.labelL.TabIndex = 10;
            this.labelL.Text = "L";
            // 
            // labelF
            // 
            this.labelF.AutoSize = true;
            this.labelF.Location = new System.Drawing.Point(245, 326);
            this.labelF.Name = "labelF";
            this.labelF.Size = new System.Drawing.Size(13, 13);
            this.labelF.TabIndex = 11;
            this.labelF.Text = "F";
            // 
            // labelAF
            // 
            this.labelAF.AutoSize = true;
            this.labelAF.Location = new System.Drawing.Point(470, 189);
            this.labelAF.Name = "labelAF";
            this.labelAF.Size = new System.Drawing.Size(20, 13);
            this.labelAF.TabIndex = 12;
            this.labelAF.Text = "AF";
            // 
            // labelBC
            // 
            this.labelBC.AutoSize = true;
            this.labelBC.Location = new System.Drawing.Point(469, 216);
            this.labelBC.Name = "labelBC";
            this.labelBC.Size = new System.Drawing.Size(21, 13);
            this.labelBC.TabIndex = 13;
            this.labelBC.Text = "BC";
            // 
            // labelDE
            // 
            this.labelDE.AutoSize = true;
            this.labelDE.Location = new System.Drawing.Point(470, 242);
            this.labelDE.Name = "labelDE";
            this.labelDE.Size = new System.Drawing.Size(22, 13);
            this.labelDE.TabIndex = 14;
            this.labelDE.Text = "DE";
            // 
            // labelHL
            // 
            this.labelHL.AutoSize = true;
            this.labelHL.Location = new System.Drawing.Point(470, 269);
            this.labelHL.Name = "labelHL";
            this.labelHL.Size = new System.Drawing.Size(21, 13);
            this.labelHL.TabIndex = 15;
            this.labelHL.Text = "HL";
            // 
            // buttonRun
            // 
            this.buttonRun.Location = new System.Drawing.Point(438, 74);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(100, 33);
            this.buttonRun.TabIndex = 17;
            this.buttonRun.Text = "Run steps:";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // numericUpDownRun
            // 
            this.numericUpDownRun.Location = new System.Drawing.Point(438, 113);
            this.numericUpDownRun.Name = "numericUpDownRun";
            this.numericUpDownRun.Size = new System.Drawing.Size(100, 20);
            this.numericUpDownRun.TabIndex = 18;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.numericUpDownRun);
            this.Controls.Add(this.buttonRun);
            this.Controls.Add(this.labelHL);
            this.Controls.Add(this.labelDE);
            this.Controls.Add(this.labelBC);
            this.Controls.Add(this.labelAF);
            this.Controls.Add(this.labelF);
            this.Controls.Add(this.labelL);
            this.Controls.Add(this.labelH);
            this.Controls.Add(this.labelE);
            this.Controls.Add(this.labelD);
            this.Controls.Add(this.labelC);
            this.Controls.Add(this.labelB);
            this.Controls.Add(this.labelA);
            this.Controls.Add(this.labelPC);
            this.Controls.Add(this.buttonStep);
            this.Controls.Add(this.menuStrip1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRun)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.Button buttonStep;
        private System.Windows.Forms.Label labelPC;
        private System.Windows.Forms.Label labelA;
        private System.Windows.Forms.Label labelB;
        private System.Windows.Forms.Label labelC;
        private System.Windows.Forms.Label labelD;
        private System.Windows.Forms.Label labelE;
        private System.Windows.Forms.Label labelH;
        private System.Windows.Forms.Label labelL;
        private System.Windows.Forms.Label labelF;
        private System.Windows.Forms.Label labelAF;
        private System.Windows.Forms.Label labelBC;
        private System.Windows.Forms.Label labelDE;
        private System.Windows.Forms.Label labelHL;
        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.NumericUpDown numericUpDownRun;
    }
}

