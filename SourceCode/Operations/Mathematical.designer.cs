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
            this.lblParam = new System.Windows.Forms.Label();
            this.textParameter = new System.Windows.Forms.TextBox();
            this.lblUnit = new System.Windows.Forms.Label();
            this.groupSingle = new System.Windows.Forms.GroupBox();
            this.lblChannel = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.groupDual = new System.Windows.Forms.GroupBox();
            this.groupSingle.SuspendLayout();
            this.groupDual.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnExecute
            // 
            this.btnExecute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnExecute.ForeColor = System.Drawing.Color.Black;
            this.btnExecute.Location = new System.Drawing.Point(139, 269);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(66, 23);
            this.btnExecute.TabIndex = 51;
            this.btnExecute.Text = "Execute";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // comboMath
            // 
            this.comboMath.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboMath.FormattingEnabled = true;
            this.comboMath.Location = new System.Drawing.Point(113, 11);
            this.comboMath.Name = "comboMath";
            this.comboMath.Size = new System.Drawing.Size(163, 21);
            this.comboMath.TabIndex = 20;
            this.comboMath.SelectedIndexChanged += new System.EventHandler(this.comboMath_SelectedIndexChanged);
            // 
            // lblChannelA
            // 
            this.lblChannelA.AutoSize = true;
            this.lblChannelA.ForeColor = System.Drawing.Color.White;
            this.lblChannelA.Location = new System.Drawing.Point(8, 22);
            this.lblChannelA.Name = "lblChannelA";
            this.lblChannelA.Size = new System.Drawing.Size(86, 13);
            this.lblChannelA.TabIndex = 7;
            this.lblChannelA.Text = "Input Channel A:";
            // 
            // lblChannelB
            // 
            this.lblChannelB.AutoSize = true;
            this.lblChannelB.ForeColor = System.Drawing.Color.White;
            this.lblChannelB.Location = new System.Drawing.Point(8, 50);
            this.lblChannelB.Name = "lblChannelB";
            this.lblChannelB.Size = new System.Drawing.Size(86, 13);
            this.lblChannelB.TabIndex = 8;
            this.lblChannelB.Text = "Input Channel B:";
            // 
            // lblNameA
            // 
            this.lblNameA.AutoSize = true;
            this.lblNameA.ForeColor = System.Drawing.Color.White;
            this.lblNameA.Location = new System.Drawing.Point(99, 22);
            this.lblNameA.Name = "lblNameA";
            this.lblNameA.Size = new System.Drawing.Size(77, 13);
            this.lblNameA.TabIndex = 9;
            this.lblNameA.Text = "Channel Name";
            // 
            // comboChannelB
            // 
            this.comboChannelB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboChannelB.FormattingEnabled = true;
            this.comboChannelB.Location = new System.Drawing.Point(99, 47);
            this.comboChannelB.Name = "comboChannelB";
            this.comboChannelB.Size = new System.Drawing.Size(163, 21);
            this.comboChannelB.TabIndex = 10;
            this.comboChannelB.SelectedIndexChanged += new System.EventHandler(this.comboChannelB_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Math Operation:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(8, 81);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(84, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Output Channel:";
            // 
            // textResult
            // 
            this.textResult.Location = new System.Drawing.Point(99, 78);
            this.textResult.Name = "textResult";
            this.textResult.Size = new System.Drawing.Size(162, 20);
            this.textResult.TabIndex = 40;
            this.textResult.Text = "Math Result";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(23, 40);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(64, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Math Mode:";
            // 
            // lblMathMode
            // 
            this.lblMathMode.AutoSize = true;
            this.lblMathMode.Location = new System.Drawing.Point(111, 40);
            this.lblMathMode.Name = "lblMathMode";
            this.lblMathMode.Size = new System.Drawing.Size(40, 13);
            this.lblMathMode.TabIndex = 15;
            this.lblMathMode.Text = "Analog";
            // 
            // btnExecuteClose
            // 
            this.btnExecuteClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnExecuteClose.ForeColor = System.Drawing.Color.Black;
            this.btnExecuteClose.Location = new System.Drawing.Point(12, 269);
            this.btnExecuteClose.Name = "btnExecuteClose";
            this.btnExecuteClose.Size = new System.Drawing.Size(116, 23);
            this.btnExecuteClose.TabIndex = 50;
            this.btnExecuteClose.Text = "Execute and Close";
            this.btnExecuteClose.UseVisualStyleBackColor = true;
            this.btnExecuteClose.Click += new System.EventHandler(this.btnExecuteClose_Click);
            // 
            // linkHelp
            // 
            this.linkHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkHelp.AutoSize = true;
            this.linkHelp.LinkColor = System.Drawing.Color.Lime;
            this.linkHelp.Location = new System.Drawing.Point(232, 272);
            this.linkHelp.Name = "linkHelp";
            this.linkHelp.Size = new System.Drawing.Size(59, 13);
            this.linkHelp.TabIndex = 52;
            this.linkHelp.TabStop = true;
            this.linkHelp.Text = "Show Help";
            this.linkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkHelp_LinkClicked);
            // 
            // lblParam
            // 
            this.lblParam.AutoSize = true;
            this.lblParam.ForeColor = System.Drawing.Color.White;
            this.lblParam.Location = new System.Drawing.Point(8, 46);
            this.lblParam.Name = "lblParam";
            this.lblParam.Size = new System.Drawing.Size(58, 13);
            this.lblParam.TabIndex = 117;
            this.lblParam.Text = "Parameter:";
            // 
            // textParameter
            // 
            this.textParameter.Location = new System.Drawing.Point(99, 43);
            this.textParameter.Name = "textParameter";
            this.textParameter.Size = new System.Drawing.Size(77, 20);
            this.textParameter.TabIndex = 30;
            // 
            // lblUnit
            // 
            this.lblUnit.AutoSize = true;
            this.lblUnit.ForeColor = System.Drawing.Color.White;
            this.lblUnit.Location = new System.Drawing.Point(182, 46);
            this.lblUnit.Name = "lblUnit";
            this.lblUnit.Size = new System.Drawing.Size(26, 13);
            this.lblUnit.TabIndex = 118;
            this.lblUnit.Text = "Unit";
            // 
            // groupSingle
            // 
            this.groupSingle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupSingle.Controls.Add(this.lblChannel);
            this.groupSingle.Controls.Add(this.lblName);
            this.groupSingle.Controls.Add(this.lblParam);
            this.groupSingle.Controls.Add(this.textParameter);
            this.groupSingle.Controls.Add(this.lblUnit);
            this.groupSingle.ForeColor = System.Drawing.Color.White;
            this.groupSingle.Location = new System.Drawing.Point(14, 60);
            this.groupSingle.Name = "groupSingle";
            this.groupSingle.Size = new System.Drawing.Size(276, 76);
            this.groupSingle.TabIndex = 119;
            this.groupSingle.TabStop = false;
            this.groupSingle.Text = " Single Channel ";
            // 
            // lblChannel
            // 
            this.lblChannel.AutoSize = true;
            this.lblChannel.ForeColor = System.Drawing.Color.White;
            this.lblChannel.Location = new System.Drawing.Point(8, 22);
            this.lblChannel.Name = "lblChannel";
            this.lblChannel.Size = new System.Drawing.Size(49, 13);
            this.lblChannel.TabIndex = 41;
            this.lblChannel.Text = "Channel:";
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.ForeColor = System.Drawing.Color.White;
            this.lblName.Location = new System.Drawing.Point(99, 22);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(77, 13);
            this.lblName.TabIndex = 42;
            this.lblName.Text = "Channel Name";
            // 
            // groupDual
            // 
            this.groupDual.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupDual.Controls.Add(this.label4);
            this.groupDual.Controls.Add(this.textResult);
            this.groupDual.Controls.Add(this.lblChannelB);
            this.groupDual.Controls.Add(this.lblChannelA);
            this.groupDual.Controls.Add(this.lblNameA);
            this.groupDual.Controls.Add(this.comboChannelB);
            this.groupDual.ForeColor = System.Drawing.Color.White;
            this.groupDual.Location = new System.Drawing.Point(14, 142);
            this.groupDual.Name = "groupDual";
            this.groupDual.Size = new System.Drawing.Size(276, 114);
            this.groupDual.TabIndex = 120;
            this.groupDual.TabStop = false;
            this.groupDual.Text = " Dual Channel ";
            // 
            // Mathematical
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(304, 304);
            this.Controls.Add(this.groupDual);
            this.Controls.Add(this.groupSingle);
            this.Controls.Add(this.linkHelp);
            this.Controls.Add(this.btnExecuteClose);
            this.Controls.Add(this.lblMathMode);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboMath);
            this.Controls.Add(this.label5);
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
            this.groupSingle.ResumeLayout(false);
            this.groupSingle.PerformLayout();
            this.groupDual.ResumeLayout(false);
            this.groupDual.PerformLayout();
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
        private System.Windows.Forms.Label lblParam;
        private System.Windows.Forms.TextBox textParameter;
        private System.Windows.Forms.Label lblUnit;
        private System.Windows.Forms.GroupBox groupSingle;
        private System.Windows.Forms.Label lblChannel;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.GroupBox groupDual;
    }
}