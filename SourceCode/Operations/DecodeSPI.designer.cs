namespace Operations
{
    partial class DecodeSPI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DecodeSPI));
            this.btnDecode = new System.Windows.Forms.Button();
            this.comboCLK = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboClkEdge = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboMOSI = new System.Windows.Forms.ComboBox();
            this.radioHalfDuplex = new System.Windows.Forms.RadioButton();
            this.radioFullDuplex = new System.Windows.Forms.RadioButton();
            this.comboMISO = new System.Windows.Forms.ComboBox();
            this.lblMISO = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.comboDataBits = new System.Windows.Forms.ComboBox();
            this.comboBitOrder = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.comboCSEL = new System.Windows.Forms.ComboBox();
            this.lblChipSel = new System.Windows.Forms.Label();
            this.checkChipSelect = new System.Windows.Forms.CheckBox();
            this.comboPolarity = new System.Windows.Forms.ComboBox();
            this.lblPolarity = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.lblChip = new System.Windows.Forms.Label();
            this.comboChip = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.linkHelp = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // btnDecode
            // 
            this.btnDecode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDecode.ForeColor = System.Drawing.Color.Black;
            this.btnDecode.Location = new System.Drawing.Point(191, 248);
            this.btnDecode.Name = "btnDecode";
            this.btnDecode.Size = new System.Drawing.Size(75, 23);
            this.btnDecode.TabIndex = 100;
            this.btnDecode.Text = "Decode";
            this.btnDecode.UseVisualStyleBackColor = true;
            this.btnDecode.Click += new System.EventHandler(this.btnDecode_Click);
            // 
            // comboCLK
            // 
            this.comboCLK.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboCLK.FormattingEnabled = true;
            this.comboCLK.Location = new System.Drawing.Point(96, 31);
            this.comboCLK.MaxDropDownItems = 30;
            this.comboCLK.Name = "comboCLK";
            this.comboCLK.Size = new System.Drawing.Size(131, 21);
            this.comboCLK.Sorted = true;
            this.comboCLK.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 102;
            this.label1.Text = "Clock:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(232, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 103;
            this.label2.Text = "Clock Edge:";
            // 
            // comboClkEdge
            // 
            this.comboClkEdge.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboClkEdge.FormattingEnabled = true;
            this.comboClkEdge.Items.AddRange(new object[] {
            "Rising Edge",
            "Falling Edge"});
            this.comboClkEdge.Location = new System.Drawing.Point(301, 31);
            this.comboClkEdge.MaxDropDownItems = 30;
            this.comboClkEdge.Name = "comboClkEdge";
            this.comboClkEdge.Size = new System.Drawing.Size(131, 21);
            this.comboClkEdge.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 88);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 105;
            this.label3.Text = "Data MOSI:";
            // 
            // comboMOSI
            // 
            this.comboMOSI.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboMOSI.FormattingEnabled = true;
            this.comboMOSI.Location = new System.Drawing.Point(96, 85);
            this.comboMOSI.MaxDropDownItems = 30;
            this.comboMOSI.Name = "comboMOSI";
            this.comboMOSI.Size = new System.Drawing.Size(131, 21);
            this.comboMOSI.Sorted = true;
            this.comboMOSI.TabIndex = 20;
            // 
            // radioHalfDuplex
            // 
            this.radioHalfDuplex.AutoSize = true;
            this.radioHalfDuplex.Location = new System.Drawing.Point(95, 63);
            this.radioHalfDuplex.Name = "radioHalfDuplex";
            this.radioHalfDuplex.Size = new System.Drawing.Size(80, 17);
            this.radioHalfDuplex.TabIndex = 12;
            this.radioHalfDuplex.Text = "Half Duplex";
            this.radioHalfDuplex.UseVisualStyleBackColor = true;
            // 
            // radioFullDuplex
            // 
            this.radioFullDuplex.AutoSize = true;
            this.radioFullDuplex.Checked = true;
            this.radioFullDuplex.Location = new System.Drawing.Point(15, 63);
            this.radioFullDuplex.Name = "radioFullDuplex";
            this.radioFullDuplex.Size = new System.Drawing.Size(77, 17);
            this.radioFullDuplex.TabIndex = 11;
            this.radioFullDuplex.TabStop = true;
            this.radioFullDuplex.Text = "Full Duplex";
            this.radioFullDuplex.UseVisualStyleBackColor = true;
            this.radioFullDuplex.CheckedChanged += new System.EventHandler(this.radioFullDuplex_CheckedChanged);
            // 
            // comboMISO
            // 
            this.comboMISO.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboMISO.FormattingEnabled = true;
            this.comboMISO.Location = new System.Drawing.Point(301, 85);
            this.comboMISO.MaxDropDownItems = 30;
            this.comboMISO.Name = "comboMISO";
            this.comboMISO.Size = new System.Drawing.Size(131, 21);
            this.comboMISO.Sorted = true;
            this.comboMISO.TabIndex = 30;
            // 
            // lblMISO
            // 
            this.lblMISO.AutoSize = true;
            this.lblMISO.Location = new System.Drawing.Point(233, 88);
            this.lblMISO.Name = "lblMISO";
            this.lblMISO.Size = new System.Drawing.Size(63, 13);
            this.lblMISO.TabIndex = 109;
            this.lblMISO.Text = "Data MISO:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 114);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 13);
            this.label5.TabIndex = 111;
            this.label5.Text = "Data Bits:";
            // 
            // comboDataBits
            // 
            this.comboDataBits.FormattingEnabled = true;
            this.comboDataBits.Items.AddRange(new object[] {
            "8",
            "16",
            "24",
            "32",
            "48",
            "56",
            "64"});
            this.comboDataBits.Location = new System.Drawing.Point(96, 111);
            this.comboDataBits.MaxDropDownItems = 30;
            this.comboDataBits.Name = "comboDataBits";
            this.comboDataBits.Size = new System.Drawing.Size(131, 21);
            this.comboDataBits.TabIndex = 40;
            // 
            // comboBitOrder
            // 
            this.comboBitOrder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBitOrder.FormattingEnabled = true;
            this.comboBitOrder.Items.AddRange(new object[] {
            "MSB first",
            "LSB first"});
            this.comboBitOrder.Location = new System.Drawing.Point(301, 111);
            this.comboBitOrder.MaxDropDownItems = 30;
            this.comboBitOrder.Name = "comboBitOrder";
            this.comboBitOrder.Size = new System.Drawing.Size(131, 21);
            this.comboBitOrder.TabIndex = 50;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(233, 114);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(51, 13);
            this.label6.TabIndex = 113;
            this.label6.Text = "Bit Order:";
            // 
            // comboCSEL
            // 
            this.comboCSEL.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboCSEL.FormattingEnabled = true;
            this.comboCSEL.Location = new System.Drawing.Point(96, 160);
            this.comboCSEL.MaxDropDownItems = 30;
            this.comboCSEL.Name = "comboCSEL";
            this.comboCSEL.Size = new System.Drawing.Size(131, 21);
            this.comboCSEL.Sorted = true;
            this.comboCSEL.TabIndex = 70;
            // 
            // lblChipSel
            // 
            this.lblChipSel.AutoSize = true;
            this.lblChipSel.Location = new System.Drawing.Point(14, 163);
            this.lblChipSel.Name = "lblChipSel";
            this.lblChipSel.Size = new System.Drawing.Size(64, 13);
            this.lblChipSel.TabIndex = 115;
            this.lblChipSel.Text = "Chip Select:";
            // 
            // checkChipSelect
            // 
            this.checkChipSelect.AutoSize = true;
            this.checkChipSelect.Location = new System.Drawing.Point(16, 141);
            this.checkChipSelect.Name = "checkChipSelect";
            this.checkChipSelect.Size = new System.Drawing.Size(102, 17);
            this.checkChipSelect.TabIndex = 60;
            this.checkChipSelect.Text = "Use Chip Select";
            this.checkChipSelect.UseVisualStyleBackColor = true;
            this.checkChipSelect.CheckedChanged += new System.EventHandler(this.checkChipSelect_CheckedChanged);
            // 
            // comboPolarity
            // 
            this.comboPolarity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboPolarity.FormattingEnabled = true;
            this.comboPolarity.Items.AddRange(new object[] {
            "Low = Chip Selected",
            "High = Chip Selected"});
            this.comboPolarity.Location = new System.Drawing.Point(301, 160);
            this.comboPolarity.MaxDropDownItems = 30;
            this.comboPolarity.Name = "comboPolarity";
            this.comboPolarity.Size = new System.Drawing.Size(131, 21);
            this.comboPolarity.TabIndex = 80;
            // 
            // lblPolarity
            // 
            this.lblPolarity.AutoSize = true;
            this.lblPolarity.Location = new System.Drawing.Point(234, 162);
            this.lblPolarity.Name = "lblPolarity";
            this.lblPolarity.Size = new System.Drawing.Size(61, 13);
            this.lblPolarity.TabIndex = 118;
            this.lblPolarity.Text = "CS Polarity:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.ForeColor = System.Drawing.Color.Yellow;
            this.label9.Location = new System.Drawing.Point(12, 9);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(415, 13);
            this.label9.TabIndex = 120;
            this.label9.Text = "If channel names are CLK, MISO, MOSI, SEL combo boxes are selected automatically." +
                "";
            // 
            // lblChip
            // 
            this.lblChip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblChip.ForeColor = System.Drawing.Color.Yellow;
            this.lblChip.Location = new System.Drawing.Point(13, 226);
            this.lblChip.Name = "lblChip";
            this.lblChip.Size = new System.Drawing.Size(415, 19);
            this.lblChip.TabIndex = 123;
            this.lblChip.Text = "Description";
            // 
            // comboChip
            // 
            this.comboChip.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboChip.FormattingEnabled = true;
            this.comboChip.Location = new System.Drawing.Point(96, 198);
            this.comboChip.MaxDropDownItems = 30;
            this.comboChip.Name = "comboChip";
            this.comboChip.Size = new System.Drawing.Size(131, 21);
            this.comboChip.TabIndex = 90;
            this.comboChip.SelectedIndexChanged += new System.EventHandler(this.comboChip_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 201);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 13);
            this.label4.TabIndex = 121;
            this.label4.Text = "Post Decoder:";
            // 
            // linkHelp
            // 
            this.linkHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkHelp.AutoSize = true;
            this.linkHelp.LinkColor = System.Drawing.Color.Lime;
            this.linkHelp.Location = new System.Drawing.Point(282, 252);
            this.linkHelp.Name = "linkHelp";
            this.linkHelp.Size = new System.Drawing.Size(59, 13);
            this.linkHelp.TabIndex = 124;
            this.linkHelp.TabStop = true;
            this.linkHelp.Text = "Show Help";
            this.linkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkHelp_LinkClicked);
            // 
            // DecodeSPI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(450, 282);
            this.Controls.Add(this.linkHelp);
            this.Controls.Add(this.lblChip);
            this.Controls.Add(this.comboChip);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.comboPolarity);
            this.Controls.Add(this.lblPolarity);
            this.Controls.Add(this.checkChipSelect);
            this.Controls.Add(this.comboCSEL);
            this.Controls.Add(this.lblChipSel);
            this.Controls.Add(this.comboBitOrder);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.comboDataBits);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.comboMISO);
            this.Controls.Add(this.lblMISO);
            this.Controls.Add(this.radioFullDuplex);
            this.Controls.Add(this.radioHalfDuplex);
            this.Controls.Add(this.comboMOSI);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboClkEdge);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboCLK);
            this.Controls.Add(this.btnDecode);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DecodeSPI";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Decode Synchronous";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnDecode;
        private System.Windows.Forms.ComboBox comboCLK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboClkEdge;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboMOSI;
        private System.Windows.Forms.RadioButton radioHalfDuplex;
        private System.Windows.Forms.RadioButton radioFullDuplex;
        private System.Windows.Forms.ComboBox comboMISO;
        private System.Windows.Forms.Label lblMISO;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboDataBits;
        private System.Windows.Forms.ComboBox comboBitOrder;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboCSEL;
        private System.Windows.Forms.Label lblChipSel;
        private System.Windows.Forms.CheckBox checkChipSelect;
        private System.Windows.Forms.ComboBox comboPolarity;
        private System.Windows.Forms.Label lblPolarity;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label lblChip;
        private System.Windows.Forms.ComboBox comboChip;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.LinkLabel linkHelp;
    }
}