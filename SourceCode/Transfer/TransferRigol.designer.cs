namespace Transfer
{
    partial class TransferRigol
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TransferRigol));
            this.comboUsbDevice = new System.Windows.Forms.ComboBox();
            this.lblUsbDevices = new System.Windows.Forms.Label();
            this.btnInstallDriver = new System.Windows.Forms.Button();
            this.btnRefreshUSB = new System.Windows.Forms.Button();
            this.btnOpen = new System.Windows.Forms.Button();
            this.groupCapture = new System.Windows.Forms.GroupBox();
            this.lblTotDuration = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.lblSampleRate = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lblSamplePoints = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.btnTransfer = new System.Windows.Forms.Button();
            this.radioMemory = new System.Windows.Forms.RadioButton();
            this.radioScreen = new System.Windows.Forms.RadioButton();
            this.lblSerie = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnForceTrig = new System.Windows.Forms.Button();
            this.btnSingle = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnAuto = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.lblFirmware = new System.Windows.Forms.Label();
            this.lblSerial = new System.Windows.Forms.Label();
            this.lblModel = new System.Windows.Forms.Label();
            this.lblBrand = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
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
            this.groupCapture.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.groupCommand.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboUsbDevice
            // 
            this.comboUsbDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboUsbDevice.FormattingEnabled = true;
            this.comboUsbDevice.Location = new System.Drawing.Point(13, 25);
            this.comboUsbDevice.Name = "comboUsbDevice";
            this.comboUsbDevice.Size = new System.Drawing.Size(152, 21);
            this.comboUsbDevice.TabIndex = 0;
            // 
            // lblUsbDevices
            // 
            this.lblUsbDevices.AutoSize = true;
            this.lblUsbDevices.Location = new System.Drawing.Point(12, 9);
            this.lblUsbDevices.Name = "lblUsbDevices";
            this.lblUsbDevices.Size = new System.Drawing.Size(121, 13);
            this.lblUsbDevices.TabIndex = 1;
            this.lblUsbDevices.Text = "Detected USB Devices:";
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
            this.btnRefreshUSB.Location = new System.Drawing.Point(173, 24);
            this.btnRefreshUSB.Name = "btnRefreshUSB";
            this.btnRefreshUSB.Size = new System.Drawing.Size(63, 23);
            this.btnRefreshUSB.TabIndex = 3;
            this.btnRefreshUSB.Text = "Refresh";
            this.btnRefreshUSB.UseVisualStyleBackColor = true;
            this.btnRefreshUSB.Click += new System.EventHandler(this.btnRefreshUSB_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.ForeColor = System.Drawing.Color.Black;
            this.btnOpen.Location = new System.Drawing.Point(243, 24);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(63, 23);
            this.btnOpen.TabIndex = 4;
            this.btnOpen.Text = "Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // groupCapture
            // 
            this.groupCapture.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupCapture.Controls.Add(this.lblTotDuration);
            this.groupCapture.Controls.Add(this.label12);
            this.groupCapture.Controls.Add(this.lblSampleRate);
            this.groupCapture.Controls.Add(this.label8);
            this.groupCapture.Controls.Add(this.lblSamplePoints);
            this.groupCapture.Controls.Add(this.label10);
            this.groupCapture.Controls.Add(this.btnTransfer);
            this.groupCapture.Controls.Add(this.radioMemory);
            this.groupCapture.Controls.Add(this.radioScreen);
            this.groupCapture.Controls.Add(this.lblSerie);
            this.groupCapture.Controls.Add(this.label6);
            this.groupCapture.Controls.Add(this.btnReset);
            this.groupCapture.Controls.Add(this.btnForceTrig);
            this.groupCapture.Controls.Add(this.btnSingle);
            this.groupCapture.Controls.Add(this.btnStop);
            this.groupCapture.Controls.Add(this.btnRun);
            this.groupCapture.Controls.Add(this.btnAuto);
            this.groupCapture.Controls.Add(this.btnClear);
            this.groupCapture.Controls.Add(this.lblFirmware);
            this.groupCapture.Controls.Add(this.lblSerial);
            this.groupCapture.Controls.Add(this.lblModel);
            this.groupCapture.Controls.Add(this.lblBrand);
            this.groupCapture.Controls.Add(this.label4);
            this.groupCapture.Controls.Add(this.label3);
            this.groupCapture.Controls.Add(this.label2);
            this.groupCapture.Controls.Add(this.label1);
            this.groupCapture.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.groupCapture.Location = new System.Drawing.Point(12, 53);
            this.groupCapture.Name = "groupCapture";
            this.groupCapture.Size = new System.Drawing.Size(542, 161);
            this.groupCapture.TabIndex = 5;
            this.groupCapture.TabStop = false;
            this.groupCapture.Text = "  Rigol  Only  ";
            // 
            // lblTotDuration
            // 
            this.lblTotDuration.AutoSize = true;
            this.lblTotDuration.ForeColor = System.Drawing.Color.Cyan;
            this.lblTotDuration.Location = new System.Drawing.Point(373, 87);
            this.lblTotDuration.Name = "lblTotDuration";
            this.lblTotDuration.Size = new System.Drawing.Size(41, 13);
            this.lblTotDuration.TabIndex = 25;
            this.lblTotDuration.Text = "240 ms";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.ForeColor = System.Drawing.Color.White;
            this.label12.Location = new System.Drawing.Point(297, 87);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(77, 13);
            this.label12.TabIndex = 24;
            this.label12.Text = "Total Duration:";
            // 
            // lblSampleRate
            // 
            this.lblSampleRate.AutoSize = true;
            this.lblSampleRate.ForeColor = System.Drawing.Color.Cyan;
            this.lblSampleRate.Location = new System.Drawing.Point(373, 54);
            this.lblSampleRate.Name = "lblSampleRate";
            this.lblSampleRate.Size = new System.Drawing.Size(37, 13);
            this.lblSampleRate.TabIndex = 23;
            this.lblSampleRate.Text = "1 GHz";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.ForeColor = System.Drawing.Color.White;
            this.label8.Location = new System.Drawing.Point(297, 54);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(63, 13);
            this.label8.TabIndex = 22;
            this.label8.Text = "Samlperate:";
            // 
            // lblSamplePoints
            // 
            this.lblSamplePoints.AutoSize = true;
            this.lblSamplePoints.ForeColor = System.Drawing.Color.Cyan;
            this.lblSamplePoints.Location = new System.Drawing.Point(373, 70);
            this.lblSamplePoints.Name = "lblSamplePoints";
            this.lblSamplePoints.Size = new System.Drawing.Size(61, 13);
            this.lblSamplePoints.TabIndex = 21;
            this.lblSamplePoints.Text = "24.000.000";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.ForeColor = System.Drawing.Color.White;
            this.label10.Location = new System.Drawing.Point(297, 70);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(73, 13);
            this.label10.TabIndex = 20;
            this.label10.Text = "Samplepoints:";
            // 
            // btnTransfer
            // 
            this.btnTransfer.ForeColor = System.Drawing.Color.Black;
            this.btnTransfer.Location = new System.Drawing.Point(465, 114);
            this.btnTransfer.Name = "btnTransfer";
            this.btnTransfer.Size = new System.Drawing.Size(65, 23);
            this.btnTransfer.TabIndex = 19;
            this.btnTransfer.Text = "Transfer";
            this.toolTip.SetToolTip(this.btnTransfer, "Transfer the currently displayed signal(s) from the oscilloscope to the computer." +
                    "");
            this.btnTransfer.UseVisualStyleBackColor = true;
            this.btnTransfer.Click += new System.EventHandler(this.btnTransfer_Click);
            // 
            // radioMemory
            // 
            this.radioMemory.AutoSize = true;
            this.radioMemory.Checked = true;
            this.radioMemory.ForeColor = System.Drawing.Color.White;
            this.radioMemory.Location = new System.Drawing.Point(300, 126);
            this.radioMemory.Name = "radioMemory";
            this.radioMemory.Size = new System.Drawing.Size(132, 17);
            this.radioMemory.TabIndex = 18;
            this.radioMemory.TabStop = true;
            this.radioMemory.Text = "Capture Entire Memory";
            this.radioMemory.UseVisualStyleBackColor = true;
            // 
            // radioScreen
            // 
            this.radioScreen.AutoSize = true;
            this.radioScreen.ForeColor = System.Drawing.Color.White;
            this.radioScreen.Location = new System.Drawing.Point(300, 108);
            this.radioScreen.Name = "radioScreen";
            this.radioScreen.Size = new System.Drawing.Size(132, 17);
            this.radioScreen.TabIndex = 17;
            this.radioScreen.Text = "Capture Visible Screen";
            this.radioScreen.UseVisualStyleBackColor = true;
            // 
            // lblSerie
            // 
            this.lblSerie.AutoSize = true;
            this.lblSerie.ForeColor = System.Drawing.Color.White;
            this.lblSerie.Location = new System.Drawing.Point(68, 54);
            this.lblSerie.Name = "lblSerie";
            this.lblSerie.Size = new System.Drawing.Size(53, 13);
            this.lblSerie.TabIndex = 16;
            this.lblSerie.Text = "DS1000Z";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(15, 54);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(34, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "Serie:";
            // 
            // btnReset
            // 
            this.btnReset.ForeColor = System.Drawing.Color.Black;
            this.btnReset.Location = new System.Drawing.Point(18, 19);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(55, 23);
            this.btnReset.TabIndex = 14;
            this.btnReset.Text = "Reset";
            this.toolTip.SetToolTip(this.btnReset, "Factory Reset");
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnForceTrig
            // 
            this.btnForceTrig.ForeColor = System.Drawing.Color.Black;
            this.btnForceTrig.Location = new System.Drawing.Point(445, 19);
            this.btnForceTrig.Name = "btnForceTrig";
            this.btnForceTrig.Size = new System.Drawing.Size(84, 23);
            this.btnForceTrig.TabIndex = 13;
            this.btnForceTrig.Text = "Force Trigger";
            this.toolTip.SetToolTip(this.btnForceTrig, "For triggering once ins STOP + SINGLE mode.");
            this.btnForceTrig.UseVisualStyleBackColor = true;
            this.btnForceTrig.Click += new System.EventHandler(this.btnForceTrig_Click);
            // 
            // btnSingle
            // 
            this.btnSingle.ForeColor = System.Drawing.Color.Black;
            this.btnSingle.Location = new System.Drawing.Point(375, 19);
            this.btnSingle.Name = "btnSingle";
            this.btnSingle.Size = new System.Drawing.Size(55, 23);
            this.btnSingle.TabIndex = 12;
            this.btnSingle.Text = "Single";
            this.toolTip.SetToolTip(this.btnSingle, "Enable single (one shot) trigger mode. ");
            this.btnSingle.UseVisualStyleBackColor = true;
            this.btnSingle.Click += new System.EventHandler(this.btnSingle_Click);
            // 
            // btnStop
            // 
            this.btnStop.ForeColor = System.Drawing.Color.Black;
            this.btnStop.Location = new System.Drawing.Point(302, 19);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(55, 23);
            this.btnStop.TabIndex = 11;
            this.btnStop.Text = "Stop";
            this.toolTip.SetToolTip(this.btnStop, "Show the signal stored in memory.");
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnRun
            // 
            this.btnRun.ForeColor = System.Drawing.Color.Black;
            this.btnRun.Location = new System.Drawing.Point(229, 19);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(55, 23);
            this.btnRun.TabIndex = 10;
            this.btnRun.Text = "Run";
            this.toolTip.SetToolTip(this.btnRun, "Show the live input signal");
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnAuto
            // 
            this.btnAuto.ForeColor = System.Drawing.Color.Black;
            this.btnAuto.Location = new System.Drawing.Point(158, 19);
            this.btnAuto.Name = "btnAuto";
            this.btnAuto.Size = new System.Drawing.Size(55, 23);
            this.btnAuto.TabIndex = 9;
            this.btnAuto.Text = "Auto";
            this.toolTip.SetToolTip(this.btnAuto, "Automatically adjust the vertical scale, horizontal time base and trigger mode ac" +
                    "cording to the input signal to realize optimum waveform display. ");
            this.btnAuto.UseVisualStyleBackColor = true;
            this.btnAuto.Click += new System.EventHandler(this.btnAuto_Click);
            // 
            // btnClear
            // 
            this.btnClear.ForeColor = System.Drawing.Color.Black;
            this.btnClear.Location = new System.Drawing.Point(88, 19);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(55, 23);
            this.btnClear.TabIndex = 8;
            this.btnClear.Text = "Clear";
            this.toolTip.SetToolTip(this.btnClear, "Clears the screen in STOP mode");
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // lblFirmware
            // 
            this.lblFirmware.AutoSize = true;
            this.lblFirmware.ForeColor = System.Drawing.Color.Cyan;
            this.lblFirmware.Location = new System.Drawing.Point(68, 121);
            this.lblFirmware.Name = "lblFirmware";
            this.lblFirmware.Size = new System.Drawing.Size(72, 13);
            this.lblFirmware.TabIndex = 7;
            this.lblFirmware.Text = "00.04.04.SP4";
            // 
            // lblSerial
            // 
            this.lblSerial.AutoSize = true;
            this.lblSerial.ForeColor = System.Drawing.Color.Cyan;
            this.lblSerial.Location = new System.Drawing.Point(68, 104);
            this.lblSerial.Name = "lblSerial";
            this.lblSerial.Size = new System.Drawing.Size(96, 13);
            this.lblSerial.TabIndex = 6;
            this.lblSerial.Text = "DS1ZC204807063";
            // 
            // lblModel
            // 
            this.lblModel.AutoSize = true;
            this.lblModel.ForeColor = System.Drawing.Color.Cyan;
            this.lblModel.Location = new System.Drawing.Point(68, 87);
            this.lblModel.Name = "lblModel";
            this.lblModel.Size = new System.Drawing.Size(76, 13);
            this.lblModel.TabIndex = 5;
            this.lblModel.Text = "DS1074Z Plus";
            // 
            // lblBrand
            // 
            this.lblBrand.AutoSize = true;
            this.lblBrand.ForeColor = System.Drawing.Color.Cyan;
            this.lblBrand.Location = new System.Drawing.Point(68, 70);
            this.lblBrand.Name = "lblBrand";
            this.lblBrand.Size = new System.Drawing.Size(98, 13);
            this.lblBrand.TabIndex = 4;
            this.lblBrand.Text = "Rigol Technologies";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(15, 121);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Firmware:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(15, 104);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Serial Nº:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(15, 87);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Model:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(15, 70);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Brand:";
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 348);
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
            this.groupCommand.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupCommand.Controls.Add(this.label9);
            this.groupCommand.Controls.Add(this.textResponse);
            this.groupCommand.Controls.Add(this.btnSend);
            this.groupCommand.Controls.Add(this.label7);
            this.groupCommand.Controls.Add(this.textCommand);
            this.groupCommand.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.groupCommand.Location = new System.Drawing.Point(13, 220);
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
            this.textResponse.Location = new System.Drawing.Point(17, 79);
            this.textResponse.Name = "textResponse";
            this.textResponse.Size = new System.Drawing.Size(511, 20);
            this.textResponse.TabIndex = 3;
            // 
            // btnSend
            // 
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
            this.linkHelp.Location = new System.Drawing.Point(331, 29);
            this.linkHelp.Name = "linkHelp";
            this.linkHelp.Size = new System.Drawing.Size(59, 13);
            this.linkHelp.TabIndex = 9;
            this.linkHelp.TabStop = true;
            this.linkHelp.Text = "Show Help";
            this.linkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkHelp_LinkClicked);
            // 
            // TransferRigol
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(566, 370);
            this.Controls.Add(this.linkHelp);
            this.Controls.Add(this.groupCommand);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.groupCapture);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.btnRefreshUSB);
            this.Controls.Add(this.btnInstallDriver);
            this.Controls.Add(this.lblUsbDevices);
            this.Controls.Add(this.comboUsbDevice);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TransferRigol";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Transfer Rigol";
            this.groupCapture.ResumeLayout(false);
            this.groupCapture.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.groupCommand.ResumeLayout(false);
            this.groupCommand.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboUsbDevice;
        private System.Windows.Forms.Label lblUsbDevices;
        private System.Windows.Forms.Button btnInstallDriver;
        private System.Windows.Forms.Button btnRefreshUSB;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.GroupBox groupCapture;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.Label lblSerial;
        private System.Windows.Forms.Label lblModel;
        private System.Windows.Forms.Label lblBrand;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblFirmware;
        private System.Windows.Forms.Button btnSingle;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnAuto;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnForceTrig;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Label lblSerie;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnTransfer;
        private System.Windows.Forms.RadioButton radioMemory;
        private System.Windows.Forms.RadioButton radioScreen;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblSampleRate;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lblSamplePoints;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.GroupBox groupCommand;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textCommand;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textResponse;
        private System.Windows.Forms.Label lblTotDuration;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.LinkLabel linkHelp;

    }
}