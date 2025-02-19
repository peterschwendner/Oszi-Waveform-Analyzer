namespace Operations
{
    partial class Mathematical
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Mathematical));
            this.btnExecute = new System.Windows.Forms.Button();
            this.comboMath = new System.Windows.Forms.ComboBox();
            this.lblChannelA = new System.Windows.Forms.Label();
            this.lblChannelB = new System.Windows.Forms.Label();
            this.lblNameA = new System.Windows.Forms.Label();
            this.comboChannelB = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textResult = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.lblMathMode = new System.Windows.Forms.Label();
            this.btnExecuteClose = new System.Windows.Forms.Button();
            this.linkHelp = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // btnExecute
            // 
            this.btnExecute.ForeColor = System.Drawing.Color.Black;
            this.btnExecute.Location = new System.Drawing.Point(133, 143);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(61, 23);
            this.btnExecute.TabIndex = 5;
            this.btnExecute.Text = "Execute";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // comboMath
            // 
            this.comboMath.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboMath.FormattingEnabled = true;
            this.comboMath.Location = new System.Drawing.Point(103, 81);
            this.comboMath.Name = "comboMath";
            this.comboMath.Size = new System.Drawing.Size(163, 21);
            this.comboMath.TabIndex = 6;
            // 
            // lblChannelA
            // 
            this.lblChannelA.AutoSize = true;
            this.lblChannelA.Location = new System.Drawing.Point(12, 11);
            this.lblChannelA.Name = "lblChannelA";
            this.lblChannelA.Size = new System.Drawing.Size(86, 13);
            this.lblChannelA.TabIndex = 7;
            this.lblChannelA.Text = "Input Channel A:";
            // 
            // lblChannelB
            // 
            this.lblChannelB.AutoSize = true;
            this.lblChannelB.Location = new System.Drawing.Point(12, 33);
            this.lblChannelB.Name = "lblChannelB";
            this.lblChannelB.Size = new System.Drawing.Size(86, 13);
            this.lblChannelB.TabIndex = 8;
            this.lblChannelB.Text = "Input Channel B:";
            // 
            // lblNameA
            // 
            this.lblNameA.AutoSize = true;
            this.lblNameA.Location = new System.Drawing.Point(103, 11);
            this.lblNameA.Name = "lblNameA";
            this.lblNameA.Size = new System.Drawing.Size(77, 13);
            this.lblNameA.TabIndex = 9;
            this.lblNameA.Text = "Channel Name";
            // 
            // comboChannelB
            // 
            this.comboChannelB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboChannelB.FormattingEnabled = true;
            this.comboChannelB.Location = new System.Drawing.Point(103, 30);
            this.comboChannelB.Name = "comboChannelB";
            this.comboChannelB.Size = new System.Drawing.Size(163, 21);
            this.comboChannelB.TabIndex = 10;
            this.comboChannelB.SelectedIndexChanged += new System.EventHandler(this.comboChannelB_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Math Operation:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 114);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(82, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Result Channel:";
            // 
            // textResult
            // 
            this.textResult.Location = new System.Drawing.Point(103, 111);
            this.textResult.Name = "textResult";
            this.textResult.Size = new System.Drawing.Size(162, 20);
            this.textResult.TabIndex = 13;
            this.textResult.Text = "Math Result";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 58);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(64, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Math Mode:";
            // 
            // lblMathMode
            // 
            this.lblMathMode.AutoSize = true;
            this.lblMathMode.Location = new System.Drawing.Point(103, 58);
            this.lblMathMode.Name = "lblMathMode";
            this.lblMathMode.Size = new System.Drawing.Size(40, 13);
            this.lblMathMode.TabIndex = 15;
            this.lblMathMode.Text = "Analog";
            // 
            // btnExecuteClose
            // 
            this.btnExecuteClose.ForeColor = System.Drawing.Color.Black;
            this.btnExecuteClose.Location = new System.Drawing.Point(13, 143);
            this.btnExecuteClose.Name = "btnExecuteClose";
            this.btnExecuteClose.Size = new System.Drawing.Size(109, 23);
            this.btnExecuteClose.TabIndex = 16;
            this.btnExecuteClose.Text = "Execute and Close";
            this.btnExecuteClose.UseVisualStyleBackColor = true;
            this.btnExecuteClose.Click += new System.EventHandler(this.btnExecuteClose_Click);
            // 
            // linkHelp
            // 
            this.linkHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkHelp.AutoSize = true;
            this.linkHelp.LinkColor = System.Drawing.Color.Lime;
            this.linkHelp.Location = new System.Drawing.Point(204, 146);
            this.linkHelp.Name = "linkHelp";
            this.linkHelp.Size = new System.Drawing.Size(59, 13);
            this.linkHelp.TabIndex = 116;
            this.linkHelp.TabStop = true;
            this.linkHelp.Text = "Show Help";
            this.linkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkHelp_LinkClicked);
            // 
            // Mathematical
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(283, 178);
            this.Controls.Add(this.linkHelp);
            this.Controls.Add(this.btnExecuteClose);
            this.Controls.Add(this.lblMathMode);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textResult);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboChannelB);
            this.Controls.Add(this.lblNameA);
            this.Controls.Add(this.lblChannelB);
            this.Controls.Add(this.lblChannelA);
            this.Controls.Add(this.comboMath);
            this.Controls.Add(this.btnExecute);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Mathematical";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Math Operations";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.ComboBox comboMath;
        private System.Windows.Forms.Label lblChannelA;
        private System.Windows.Forms.Label lblChannelB;
        private System.Windows.Forms.Label lblNameA;
        private System.Windows.Forms.ComboBox comboChannelB;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textResult;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblMathMode;
        private System.Windows.Forms.Button btnExecuteClose;
        private System.Windows.Forms.LinkLabel linkHelp;
    }
}