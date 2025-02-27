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
            this.comboUsbDevice = new System.Windows.Forms.ComboBox();
            this.lblUsbIP = new System.Windows.Forms.Label();
            this.btnInstallDriver = new System.Windows.Forms.Button();
            this.btnRefreshUSB = new System.Windows.Forms.Button();
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
            this.textIpAddr = new System.Windows.Forms.TextBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.textPort = new System.Windows.Forms.TextBox();
            this.statusStrip.SuspendLayout();
            this.groupCommand.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboUsbDevice
            // 
            this.comboUsbDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboUsbDevice.FormattingEnabled = true;
            this.comboUsbDevice.Location = new System.Drawing.Point(66, 25);
            this.comboUsbDevice.Name = "comboUsbDevice";
            this.comboUsbDevice.Size = new System.Drawing.Size(142, 21);
            this.comboUsbDevice.TabIndex = 0;
            // 
            // lblUsbIP
            // 
            this.lblUsbIP.AutoSize = true;
            this.lblUsbIP.Location = new System.Drawing.Point(65, 9);
            this.lblUsbIP.Name = "lblUsbIP";
            this.lblUsbIP.Size = new System.Drawing.Size(100, 13);
            this.lblUsbIP.TabIndex = 1;
            this.lblUsbIP.Text = "TMC USB Devices:";
            // 
            // btnInstallDriver
            // 
            this.btnInstallDriver.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInstallDriver.ForeColor = System.Drawing.Color.Black;
            this.btnInstallDriver.Location = new System.Drawing.Point(429, 24);
            this.btnInstallDriver.Name = "btnInstallDriver";
            this.btnInstallDriver.Size = new System.Drawing.Size(125, 23);
            this.btnInstallDriver.TabIndex = 2;
            this.btnInstallDriver.Text = "Install Driver (64 bit)";
            this.btnInstallDriver.UseVisualStyleBackColor = true;
            this.btnInstallDriver.Click += new System.EventHandler(this.btnInstallDriver_Click);
            // 
            // btnRefreshUSB
            // 
            this.btnRefreshUSB.ForeColor = System.Drawing.Color.Black;
            this.btnRefreshUSB.Location = new System.Drawing.Point(214, 24);
            this.btnRefreshUSB.Name = "btnRefreshUSB";
            this.btnRefreshUSB.Size = new System.Drawing.Size(65, 23);
            this.btnRefreshUSB.TabIndex = 3;
            this.btnRefreshUSB.Text = "Refresh";
            this.btnRefreshUSB.UseVisualStyleBackColor = true;
            this.btnRefreshUSB.Click += new System.EventHandler(this.btnRefreshUSB_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.ForeColor = System.Drawing.Color.Black;
            this.btnOpen.Location = new System.Drawing.Point(284, 24);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(52, 23);
            this.btnOpen.TabIndex = 4;
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
            this.label5.Location = new System.Drawing.Point(430, 9);
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
            this.groupCommand.TabIndex = 8;
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
            this.linkHelp.Location = new System.Drawing.Point(339, 29);
            this.linkHelp.Name = "linkHelp";
            this.linkHelp.Size = new System.Drawing.Size(59, 13);
            this.linkHelp.TabIndex = 9;
            this.linkHelp.TabStop = true;
            this.linkHelp.Text = "Show Help";
            this.linkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkHelp_LinkClicked);
            // 
            // radioUSB
            // 
            this.radioUSB.AutoSize = true;
            this.radioUSB.Checked = true;
            this.radioUSB.Location = new System.Drawing.Point(14, 13);
            this.radioUSB.Name = "radioUSB";
            this.radioUSB.Size = new System.Drawing.Size(47, 17);
            this.radioUSB.TabIndex = 10;
            this.radioUSB.TabStop = true;
            this.radioUSB.Text = "USB";
            this.radioUSB.UseVisualStyleBackColor = true;
            this.radioUSB.CheckedChanged += new System.EventHandler(this.radioUSB_CheckedChanged);
            // 
            // radioTCP
            // 
            this.radioTCP.AutoSize = true;
            this.radioTCP.Location = new System.Drawing.Point(14, 30);
            this.radioTCP.Name = "radioTCP";
            this.radioTCP.Size = new System.Drawing.Size(46, 17);
            this.radioTCP.TabIndex = 11;
            this.radioTCP.Text = "TCP";
            this.radioTCP.UseVisualStyleBackColor = true;
            // 
            // textIpAddr
            // 
            this.textIpAddr.Location = new System.Drawing.Point(67, 25);
            this.textIpAddr.Name = "textIpAddr";
            this.textIpAddr.Size = new System.Drawing.Size(25, 20);
            this.textIpAddr.TabIndex = 12;
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(214, 9);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(29, 13);
            this.lblPort.TabIndex = 13;
            this.lblPort.Text = "Port:";
            // 
            // textPort
            // 
            this.textPort.Location = new System.Drawing.Point(216, 25);
            this.textPort.Name = "textPort";
            this.textPort.Size = new System.Drawing.Size(25, 20);
            this.textPort.TabIndex = 14;
            // 
            // FormTransfer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(566, 206);
            this.Controls.Add(this.textPort);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.radioTCP);
            this.Controls.Add(this.radioUSB);
            this.Controls.Add(this.textIpAddr);
            this.Controls.Add(this.linkHelp);
            this.Controls.Add(this.groupCommand);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.btnRefreshUSB);
            this.Controls.Add(this.btnInstallDriver);
            this.Controls.Add(this.lblUsbIP);
            this.Controls.Add(this.comboUsbDevice);
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

        private System.Windows.Forms.ComboBox comboUsbDevice;
        private System.Windows.Forms.Label lblUsbIP;
        private System.Windows.Forms.Button btnInstallDriver;
        private System.Windows.Forms.Button btnRefreshUSB;
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
        private System.Windows.Forms.TextBox textIpAddr;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.TextBox textPort;

    }
}