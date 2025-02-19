namespace Operations
{
    partial class DecodeUART
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DecodeUART));
            this.label1 = new System.Windows.Forms.Label();
            this.comboBaudrate = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.comboStartbit = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.comboDatabits = new System.Windows.Forms.ComboBox();
            this.btnDecode = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.comboParity = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.comboStopbits = new System.Windows.Forms.ComboBox();
            this.lblChannelA = new System.Windows.Forms.Label();
            this.lblNameA = new System.Windows.Forms.Label();
            this.checkHalfduplex = new System.Windows.Forms.CheckBox();
            this.lblChannelB = new System.Windows.Forms.Label();
            this.comboChannelB = new System.Windows.Forms.ComboBox();
            this.groupChannels = new System.Windows.Forms.GroupBox();
            this.groupSettings = new System.Windows.Forms.GroupBox();
            this.lblChip = new System.Windows.Forms.Label();
            this.comboChip = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.linkHelp = new System.Windows.Forms.LinkLabel();
            this.groupChannels.SuspendLayout();
            this.groupSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(31, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 13);
            this.label1.TabIndex = 0;
            // 
            // comboBaudrate
            // 
            this.comboBaudrate.FormattingEnabled = true;
            this.comboBaudrate.Items.AddRange(new object[] {
            "75",
            "110",
            "134",
            "150",
            "300",
            "600",
            "1200",
            "1800",
            "2400",
            "4800",
            "7200",
            "9600",
            "10400",
            "14400",
            "19200",
            "38400",
            "57600",
            "115200",
            "128000",
            "196000",
            "230400",
            "256000",
            "460800",
            "921600"});
            this.comboBaudrate.Location = new System.Drawing.Point(77, 17);
            this.comboBaudrate.MaxDropDownItems = 30;
            this.comboBaudrate.Name = "comboBaudrate";
            this.comboBaudrate.Size = new System.Drawing.Size(117, 21);
            this.comboBaudrate.TabIndex = 20;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Baudrate:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 49);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Startbit:";
            // 
            // comboStartbit
            // 
            this.comboStartbit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboStartbit.FormattingEnabled = true;
            this.comboStartbit.Location = new System.Drawing.Point(76, 46);
            this.comboStartbit.Name = "comboStartbit";
            this.comboStartbit.Size = new System.Drawing.Size(118, 21);
            this.comboStartbit.TabIndex = 30;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 77);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(49, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Databits:";
            // 
            // comboDatabits
            // 
            this.comboDatabits.FormattingEnabled = true;
            this.comboDatabits.Items.AddRange(new object[] {
            "5",
            "6",
            "7",
            "8",
            "9"});
            this.comboDatabits.Location = new System.Drawing.Point(77, 74);
            this.comboDatabits.MaxDropDownItems = 30;
            this.comboDatabits.Name = "comboDatabits";
            this.comboDatabits.Size = new System.Drawing.Size(117, 21);
            this.comboDatabits.TabIndex = 40;
            // 
            // btnDecode
            // 
            this.btnDecode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDecode.ForeColor = System.Drawing.Color.Black;
            this.btnDecode.Location = new System.Drawing.Point(49, 339);
            this.btnDecode.Name = "btnDecode";
            this.btnDecode.Size = new System.Drawing.Size(75, 23);
            this.btnDecode.TabIndex = 100;
            this.btnDecode.Text = "Decode";
            this.btnDecode.UseVisualStyleBackColor = true;
            this.btnDecode.Click += new System.EventHandler(this.btnDecode_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 107);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(36, 13);
            this.label6.TabIndex = 101;
            this.label6.Text = "Parity:";
            // 
            // comboParity
            // 
            this.comboParity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboParity.FormattingEnabled = true;
            this.comboParity.Location = new System.Drawing.Point(76, 104);
            this.comboParity.Name = "comboParity";
            this.comboParity.Size = new System.Drawing.Size(118, 21);
            this.comboParity.TabIndex = 102;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 135);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(51, 13);
            this.label7.TabIndex = 103;
            this.label7.Text = "Stoptbits:";
            // 
            // comboStopbits
            // 
            this.comboStopbits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboStopbits.FormattingEnabled = true;
            this.comboStopbits.Location = new System.Drawing.Point(76, 132);
            this.comboStopbits.Name = "comboStopbits";
            this.comboStopbits.Size = new System.Drawing.Size(118, 21);
            this.comboStopbits.TabIndex = 104;
            // 
            // lblChannelA
            // 
            this.lblChannelA.AutoSize = true;
            this.lblChannelA.Location = new System.Drawing.Point(10, 18);
            this.lblChannelA.Name = "lblChannelA";
            this.lblChannelA.Size = new System.Drawing.Size(49, 13);
            this.lblChannelA.TabIndex = 105;
            this.lblChannelA.Text = "Channel:";
            // 
            // lblNameA
            // 
            this.lblNameA.AutoSize = true;
            this.lblNameA.Location = new System.Drawing.Point(77, 18);
            this.lblNameA.Name = "lblNameA";
            this.lblNameA.Size = new System.Drawing.Size(35, 13);
            this.lblNameA.TabIndex = 106;
            this.lblNameA.Text = "Name";
            // 
            // checkHalfduplex
            // 
            this.checkHalfduplex.AutoSize = true;
            this.checkHalfduplex.Location = new System.Drawing.Point(11, 38);
            this.checkHalfduplex.Name = "checkHalfduplex";
            this.checkHalfduplex.Size = new System.Drawing.Size(181, 17);
            this.checkHalfduplex.TabIndex = 107;
            this.checkHalfduplex.Text = "Decode two halfduplex channels";
            this.checkHalfduplex.UseVisualStyleBackColor = true;
            this.checkHalfduplex.CheckedChanged += new System.EventHandler(this.checkHalfduplex_CheckedChanged);
            // 
            // lblChannelB
            // 
            this.lblChannelB.AutoSize = true;
            this.lblChannelB.Location = new System.Drawing.Point(10, 62);
            this.lblChannelB.Name = "lblChannelB";
            this.lblChannelB.Size = new System.Drawing.Size(49, 13);
            this.lblChannelB.TabIndex = 108;
            this.lblChannelB.Text = "Channel:";
            // 
            // comboChannelB
            // 
            this.comboChannelB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboChannelB.FormattingEnabled = true;
            this.comboChannelB.Location = new System.Drawing.Point(76, 59);
            this.comboChannelB.Name = "comboChannelB";
            this.comboChannelB.Size = new System.Drawing.Size(116, 21);
            this.comboChannelB.TabIndex = 109;
            this.comboChannelB.SelectedIndexChanged += new System.EventHandler(this.comboChannelB_SelectedIndexChanged);
            // 
            // groupChannels
            // 
            this.groupChannels.Controls.Add(this.checkHalfduplex);
            this.groupChannels.Controls.Add(this.comboChannelB);
            this.groupChannels.Controls.Add(this.lblChannelA);
            this.groupChannels.Controls.Add(this.lblChannelB);
            this.groupChannels.Controls.Add(this.lblNameA);
            this.groupChannels.ForeColor = System.Drawing.Color.White;
            this.groupChannels.Location = new System.Drawing.Point(14, 6);
            this.groupChannels.Name = "groupChannels";
            this.groupChannels.Size = new System.Drawing.Size(210, 93);
            this.groupChannels.TabIndex = 110;
            this.groupChannels.TabStop = false;
            // 
            // groupSettings
            // 
            this.groupSettings.Controls.Add(this.comboDatabits);
            this.groupSettings.Controls.Add(this.label1);
            this.groupSettings.Controls.Add(this.label7);
            this.groupSettings.Controls.Add(this.comboBaudrate);
            this.groupSettings.Controls.Add(this.comboStopbits);
            this.groupSettings.Controls.Add(this.label2);
            this.groupSettings.Controls.Add(this.label6);
            this.groupSettings.Controls.Add(this.comboStartbit);
            this.groupSettings.Controls.Add(this.comboParity);
            this.groupSettings.Controls.Add(this.label4);
            this.groupSettings.Controls.Add(this.label5);
            this.groupSettings.Location = new System.Drawing.Point(12, 105);
            this.groupSettings.Name = "groupSettings";
            this.groupSettings.Size = new System.Drawing.Size(212, 163);
            this.groupSettings.TabIndex = 111;
            this.groupSettings.TabStop = false;
            // 
            // lblChip
            // 
            this.lblChip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblChip.ForeColor = System.Drawing.Color.Yellow;
            this.lblChip.Location = new System.Drawing.Point(7, 304);
            this.lblChip.Name = "lblChip";
            this.lblChip.Size = new System.Drawing.Size(218, 32);
            this.lblChip.TabIndex = 126;
            this.lblChip.Text = "Description";
            // 
            // comboChip
            // 
            this.comboChip.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboChip.FormattingEnabled = true;
            this.comboChip.Location = new System.Drawing.Point(89, 276);
            this.comboChip.MaxDropDownItems = 30;
            this.comboChip.Name = "comboChip";
            this.comboChip.Size = new System.Drawing.Size(117, 21);
            this.comboChip.TabIndex = 124;
            this.comboChip.SelectedIndexChanged += new System.EventHandler(this.comboChip_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 279);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 13);
            this.label3.TabIndex = 125;
            this.label3.Text = "Post Decoder:";
            // 
            // linkHelp
            // 
            this.linkHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkHelp.AutoSize = true;
            this.linkHelp.LinkColor = System.Drawing.Color.Lime;
            this.linkHelp.Location = new System.Drawing.Point(134, 343);
            this.linkHelp.Name = "linkHelp";
            this.linkHelp.Size = new System.Drawing.Size(59, 13);
            this.linkHelp.TabIndex = 127;
            this.linkHelp.TabStop = true;
            this.linkHelp.Text = "Show Help";
            this.linkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkHelp_LinkClicked);
            // 
            // DecodeUART
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(239, 374);
            this.Controls.Add(this.linkHelp);
            this.Controls.Add(this.lblChip);
            this.Controls.Add(this.comboChip);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.groupSettings);
            this.Controls.Add(this.groupChannels);
            this.Controls.Add(this.btnDecode);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DecodeUART";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Decode Asynchronous";
            this.groupChannels.ResumeLayout(false);
            this.groupChannels.PerformLayout();
            this.groupSettings.ResumeLayout(false);
            this.groupSettings.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBaudrate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboStartbit;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboDatabits;
        private System.Windows.Forms.Button btnDecode;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboParity;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboStopbits;
        private System.Windows.Forms.Label lblChannelA;
        private System.Windows.Forms.Label lblNameA;
        private System.Windows.Forms.CheckBox checkHalfduplex;
        private System.Windows.Forms.Label lblChannelB;
        private System.Windows.Forms.ComboBox comboChannelB;
        private System.Windows.Forms.GroupBox groupChannels;
        private System.Windows.Forms.GroupBox groupSettings;
        private System.Windows.Forms.Label lblChip;
        private System.Windows.Forms.ComboBox comboChip;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.LinkLabel linkHelp;
    }
}