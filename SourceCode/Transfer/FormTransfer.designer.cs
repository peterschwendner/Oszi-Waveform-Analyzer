namespace Transfer
{
    partial class FormTransfer
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTransfer));
            this.comboDevices = new System.Windows.Forms.ComboBox();
            this.lblUsbEndp = new System.Windows.Forms.Label();
            this.btnInstallDriver = new System.Windows.Forms.Button();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnOpen = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.label5 = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.groupCommand = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.textResponse = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.textCommand = new System.Windows.Forms.TextBox();
            this.linkHelp = new System.Windows.Forms.LinkLabel();
            this.radioUSB = new System.Windows.Forms.RadioButton();
            this.radioTCP = new System.Windows.Forms.RadioButton();
            this.radioVXI = new System.Windows.Forms.RadioButton();
            this.textVxiLink = new System.Windows.Forms.TextBox();
            this.lblVxiLink = new System.Windows.Forms.Label();
            this.statusStrip.SuspendLayout();
            this.groupCommand.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboDevices
            // 
            this.comboDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboDevices.FormattingEnabled = true;
            this.comboDevices.Location = new System.Drawing.Point(62, 25);
            this.comboDevices.Name = "comboDevices";
            this.comboDevices.Size = new System.Drawing.Size(128, 21);
            this.comboDevices.TabIndex = 10;
            // 
            // lblUsbEndp
            // 
            this.lblUsbEndp.AutoSize = true;
            this.lblUsbEndp.Location = new System.Drawing.Point(61, 9);
            this.lblUsbEndp.Name = "lblUsbEndp";
            this.lblUsbEndp.Size = new System.Drawing.Size(66, 13);
            this.lblUsbEndp.TabIndex = 1;
            this.lblUsbEndp.Text = "USB Device";
            // 
            // btnInstallDriver
            // 
            this.btnInstallDriver.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInstallDriver.ForeColor = System.Drawing.Color.Black;
            this.btnInstallDriver.Location = new System.Drawing.Point(433, 24);
            this.btnInstallDriver.Name = "btnInstallDriver";
            this.btnInstallDriver.Size = new System.Drawing.Size(121, 23);
            this.btnInstallDriver.TabIndex = 60;
            this.btnInstallDriver.Text = "Install Driver (64 bit)";
            this.btnInstallDriver.UseVisualStyleBackColor = true;
            this.btnInstallDriver.Click += new System.EventHandler(this.btnInstallDriver_Click);
            // 
            // btnSearch
            // 
            this.btnSearch.ForeColor = System.Drawing.Color.Black;
            this.btnSearch.Location = new System.Drawing.Point(193, 24);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(56, 23);
            this.btnSearch.TabIndex = 20;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.ForeColor = System.Drawing.Color.Black;
            this.btnOpen.Location = new System.Drawing.Point(305, 24);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(48, 23);
            this.btnOpen.TabIndex = 40;
            this.btnOpen.Text = "Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 184);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(566, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 6;
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = false;
            this.statusLabel.BackColor = System.Drawing.SystemColors.Control;
            this.statusLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusLabel.ForeColor = System.Drawing.Color.Black;
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(520, 17);
            this.statusLabel.Text = "Status";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(434, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(105, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "IVI USB TMC Driver:";
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 32000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            // 
            // groupCommand
            // 
            this.groupCommand.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupCommand.Controls.Add(this.label9);
            this.groupCommand.Controls.Add(this.textResponse);
            this.groupCommand.Controls.Add(this.btnSend);
            this.groupCommand.Controls.Add(this.label7);
            this.groupCommand.Controls.Add(this.textCommand);
            this.groupCommand.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.groupCommand.Location = new System.Drawing.Point(13, 56);
            this.groupCommand.Name = "groupCommand";
            this.groupCommand.Size = new System.Drawing.Size(541, 114);
            this.groupCommand.TabIndex = 100;
            this.groupCommand.TabStop = false;
            this.groupCommand.Text = "  Any SCPI Device  ";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.ForeColor = System.Drawing.Color.White;
            this.label9.Location = new System.Drawing.Point(16, 63);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(58, 13);
            this.label9.TabIndex = 4;
            this.label9.Text = "Response:";
            // 
            // textResponse
            // 
            this.textResponse.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textResponse.Location = new System.Drawing.Point(17, 79);
            this.textResponse.Name = "textResponse";
            this.textResponse.Size = new System.Drawing.Size(511, 20);
            this.textResponse.TabIndex = 3;
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.ForeColor = System.Drawing.Color.Black;
            this.btnSend.Location = new System.Drawing.Point(464, 36);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(65, 23);
            this.btnSend.TabIndex = 2;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.ForeColor = System.Drawing.Color.White;
            this.label7.Location = new System.Drawing.Point(18, 21);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(84, 13);
            this.label7.TabIndex = 1;
            this.label7.Text = "SCPI Command:";
            // 
            // textCommand
            // 
            this.textCommand.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textCommand.Location = new System.Drawing.Point(17, 37);
            this.textCommand.Name = "textCommand";
            this.textCommand.Size = new System.Drawing.Size(441, 20);
            this.textCommand.TabIndex = 0;
            this.textCommand.Text = "*IDN?";
            // 
            // linkHelp
            // 
            this.linkHelp.AutoSize = true;
            this.linkHelp.LinkColor = System.Drawing.Color.Lime;
            this.linkHelp.Location = new System.Drawing.Point(362, 28);
            this.linkHelp.Name = "linkHelp";
            this.linkHelp.Size = new System.Drawing.Size(59, 13);
            this.linkHelp.TabIndex = 50;
            this.linkHelp.TabStop = true;
            this.linkHelp.Text = "Show Help";
            this.linkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkHelp_LinkClicked);
            // 
            // radioUSB
            // 
            this.radioUSB.AutoSize = true;
            this.radioUSB.Location = new System.Drawing.Point(14, 5);
            this.radioUSB.Name = "radioUSB";
            this.radioUSB.Size = new System.Drawing.Size(47, 17);
            this.radioUSB.TabIndex = 1;
            this.radioUSB.Text = "USB";
            this.radioUSB.UseVisualStyleBackColor = true;
            this.radioUSB.CheckedChanged += new System.EventHandler(this.OnRadioButton_CheckedChanged);
            // 
            // radioTCP
            // 
            this.radioTCP.AutoSize = true;
            this.radioTCP.Location = new System.Drawing.Point(14, 35);
            this.radioTCP.Name = "radioTCP";
            this.radioTCP.Size = new System.Drawing.Size(46, 17);
            this.radioTCP.TabIndex = 3;
            this.radioTCP.Text = "TCP";
            this.radioTCP.UseVisualStyleBackColor = true;
            this.radioTCP.CheckedChanged += new System.EventHandler(this.OnRadioButton_CheckedChanged);
            // 
            // radioVXI
            // 
            this.radioVXI.AutoSize = true;
            this.radioVXI.Location = new System.Drawing.Point(14, 20);
            this.radioVXI.Name = "radioVXI";
            this.radioVXI.Size = new System.Drawing.Size(42, 17);
            this.radioVXI.TabIndex = 2;
            this.radioVXI.Text = "VXI";
            this.radioVXI.UseVisualStyleBackColor = true;
            this.radioVXI.CheckedChanged += new System.EventHandler(this.OnRadioButton_CheckedChanged);
            // 
            // textVxiLink
            // 
            this.textVxiLink.Location = new System.Drawing.Point(253, 25);
            this.textVxiLink.Name = "textVxiLink";
            this.textVxiLink.Size = new System.Drawing.Size(48, 20);
            this.textVxiLink.TabIndex = 30;
            // 
            // lblVxiLink
            // 
            this.lblVxiLink.AutoSize = true;
            this.lblVxiLink.Location = new System.Drawing.Point(253, 9);
            this.lblVxiLink.Name = "lblVxiLink";
            this.lblVxiLink.Size = new System.Drawing.Size(47, 13);
            this.lblVxiLink.TabIndex = 15;
            this.lblVxiLink.Text = "VXI Link";
            // 
            // FormTransfer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(566, 206);
            this.Controls.Add(this.lblVxiLink);
            this.Controls.Add(this.textVxiLink);
            this.Controls.Add(this.radioVXI);
            this.Controls.Add(this.radioTCP);
            this.Controls.Add(this.radioUSB);
            this.Controls.Add(this.linkHelp);
            this.Controls.Add(this.groupCommand);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.btnInstallDriver);
            this.Controls.Add(this.lblUsbEndp);
            this.Controls.Add(this.comboDevices);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormTransfer";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Transfer";
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.groupCommand.ResumeLayout(false);
            this.groupCommand.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboDevices;
        private System.Windows.Forms.Label lblUsbEndp;
        private System.Windows.Forms.Button btnInstallDriver;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.GroupBox groupCommand;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textCommand;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textResponse;
        private System.Windows.Forms.LinkLabel linkHelp;
        private System.Windows.Forms.RadioButton radioUSB;
        private System.Windows.Forms.RadioButton radioTCP;
        private System.Windows.Forms.RadioButton radioVXI;
        private System.Windows.Forms.TextBox textVxiLink;
        private System.Windows.Forms.Label lblVxiLink;

    }
}