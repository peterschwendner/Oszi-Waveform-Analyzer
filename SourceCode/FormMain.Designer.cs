namespace OsziWaveformAnalyzer
{
    partial class FormMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnSave = new System.Windows.Forms.Button();
            this.lblInfo = new System.Windows.Forms.Label();
            this.checkSepChannels = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnCapture = new System.Windows.Forms.Button();
            this.comboOsziModel = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.comboInput = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lblDispSamples = new System.Windows.Forms.Label();
            this.lblFactorHint = new System.Windows.Forms.Label();
            this.comboFactor = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.linkSave = new System.Windows.Forms.LinkLabel();
            this.checkSaveFactor = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.comboSaveAs = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textFileName = new System.Windows.Forms.TextBox();
            this.checkLegend = new System.Windows.Forms.CheckBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.linkUpdate = new System.Windows.Forms.LinkLabel();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabOszi = new System.Windows.Forms.TabPage();
            this.osziPanel = new OsziWaveformAnalyzer.OsziPanel();
            this.tabDecoder = new System.Windows.Forms.TabPage();
            this.rtfViewer = new OsziWaveformAnalyzer.RtfViewer();
            this.panelHint = new System.Windows.Forms.Panel();
            this.pictureLogo = new System.Windows.Forms.PictureBox();
            this.trackAnalogHeight = new System.Windows.Forms.TrackBar();
            this.lblAnalogHeight = new System.Windows.Forms.Label();
            this.trackDigitalHeight = new System.Windows.Forms.TrackBar();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblDigitalHeight = new System.Windows.Forms.Label();
            this.statusStrip.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabOszi.SuspendLayout();
            this.tabDecoder.SuspendLayout();
            this.panelHint.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackAnalogHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackDigitalHeight)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.BackColor = System.Drawing.SystemColors.Control;
            this.statusStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 494);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.ShowItemToolTips = true;
            this.statusStrip.Size = new System.Drawing.Size(881, 22);
            this.statusStrip.TabIndex = 4;
            this.statusStrip.Text = "statusStrip";
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = false;
            this.statusLabel.BackColor = System.Drawing.SystemColors.Control;
            this.statusLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusLabel.ForeColor = System.Drawing.Color.Black;
            this.statusLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(400, 17);
            this.statusLabel.Text = "Status";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.ForeColor = System.Drawing.Color.Black;
            this.btnSave.Location = new System.Drawing.Point(189, 25);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(48, 23);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // lblInfo
            // 
            this.lblInfo.AutoEllipsis = true;
            this.lblInfo.ForeColor = System.Drawing.Color.Cyan;
            this.lblInfo.Location = new System.Drawing.Point(0, 5);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(427, 13);
            this.lblInfo.TabIndex = 100;
            this.lblInfo.Text = "Total Samples:  0";
            // 
            // checkSepChannels
            // 
            this.checkSepChannels.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkSepChannels.AutoSize = true;
            this.checkSepChannels.Location = new System.Drawing.Point(520, 4);
            this.checkSepChannels.Name = "checkSepChannels";
            this.checkSepChannels.Size = new System.Drawing.Size(152, 17);
            this.checkSepChannels.TabIndex = 31;
            this.checkSepChannels.Text = "Separate Analog Channels";
            this.toolTip.SetToolTip(this.checkSepChannels, "Draw the channels separately rather than on top of each other");
            this.checkSepChannels.UseVisualStyleBackColor = true;
            this.checkSepChannels.CheckedChanged += new System.EventHandler(this.checkSepChannels_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnCapture);
            this.groupBox1.Controls.Add(this.comboOsziModel);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.btnRefresh);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.comboInput);
            this.groupBox1.Location = new System.Drawing.Point(123, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(357, 102);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // btnCapture
            // 
            this.btnCapture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCapture.ForeColor = System.Drawing.Color.Black;
            this.btnCapture.Location = new System.Drawing.Point(291, 26);
            this.btnCapture.Name = "btnCapture";
            this.btnCapture.Size = new System.Drawing.Size(57, 23);
            this.btnCapture.TabIndex = 2;
            this.btnCapture.Text = "Capture";
            this.btnCapture.UseVisualStyleBackColor = true;
            this.btnCapture.Click += new System.EventHandler(this.btnCapture_Click);
            // 
            // comboOsziModel
            // 
            this.comboOsziModel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboOsziModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboOsziModel.FormattingEnabled = true;
            this.comboOsziModel.Location = new System.Drawing.Point(8, 27);
            this.comboOsziModel.MaxDropDownItems = 30;
            this.comboOsziModel.Name = "comboOsziModel";
            this.comboOsziModel.Size = new System.Drawing.Size(277, 21);
            this.comboOsziModel.Sorted = true;
            this.comboOsziModel.TabIndex = 1;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(8, 12);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(62, 13);
            this.label9.TabIndex = 6;
            this.label9.Text = "Oszi Model:";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.ForeColor = System.Drawing.Color.Black;
            this.btnRefresh.Location = new System.Drawing.Point(291, 66);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(57, 23);
            this.btnRefresh.TabIndex = 11;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Input File:";
            // 
            // comboInput
            // 
            this.comboInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboInput.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboInput.FormattingEnabled = true;
            this.comboInput.Location = new System.Drawing.Point(8, 67);
            this.comboInput.MaxDropDownItems = 30;
            this.comboInput.Name = "comboInput";
            this.comboInput.Size = new System.Drawing.Size(277, 21);
            this.comboInput.Sorted = true;
            this.comboInput.TabIndex = 10;
            this.comboInput.SelectedIndexChanged += new System.EventHandler(this.comboInput_SelectedIndexChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lblDispSamples);
            this.groupBox3.Controls.Add(this.lblFactorHint);
            this.groupBox3.Controls.Add(this.comboFactor);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Location = new System.Drawing.Point(486, 2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(121, 102);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            // 
            // lblDispSamples
            // 
            this.lblDispSamples.AutoSize = true;
            this.lblDispSamples.Location = new System.Drawing.Point(7, 50);
            this.lblDispSamples.Name = "lblDispSamples";
            this.lblDispSamples.Size = new System.Drawing.Size(50, 13);
            this.lblDispSamples.TabIndex = 24;
            this.lblDispSamples.Text = "Samples:";
            // 
            // lblFactorHint
            // 
            this.lblFactorHint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFactorHint.ForeColor = System.Drawing.Color.PeachPuff;
            this.lblFactorHint.Location = new System.Drawing.Point(7, 68);
            this.lblFactorHint.Name = "lblFactorHint";
            this.lblFactorHint.Size = new System.Drawing.Size(108, 30);
            this.lblFactorHint.TabIndex = 23;
            this.lblFactorHint.Text = "Press ALT + F\r\nand arrow up/down\r\n";
            this.lblFactorHint.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // comboFactor
            // 
            this.comboFactor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboFactor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboFactor.FormattingEnabled = true;
            this.comboFactor.Location = new System.Drawing.Point(7, 27);
            this.comboFactor.MaxDropDownItems = 30;
            this.comboFactor.Name = "comboFactor";
            this.comboFactor.Size = new System.Drawing.Size(108, 21);
            this.comboFactor.TabIndex = 20;
            this.comboFactor.SelectedIndexChanged += new System.EventHandler(this.comboFactor_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Display &Factor:";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.linkSave);
            this.groupBox4.Controls.Add(this.checkSaveFactor);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.comboSaveAs);
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.textFileName);
            this.groupBox4.Controls.Add(this.btnSave);
            this.groupBox4.Location = new System.Drawing.Point(613, 2);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(246, 102);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            // 
            // linkSave
            // 
            this.linkSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkSave.AutoSize = true;
            this.linkSave.LinkColor = System.Drawing.Color.Lime;
            this.linkSave.Location = new System.Drawing.Point(181, 50);
            this.linkSave.Name = "linkSave";
            this.linkSave.Size = new System.Drawing.Size(59, 13);
            this.linkSave.TabIndex = 102;
            this.linkSave.TabStop = true;
            this.linkSave.Text = "Show Help";
            this.toolTip.SetToolTip(this.linkSave, "Show help about the Save otions and the Factor");
            this.linkSave.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkSave_LinkClicked);
            // 
            // checkSaveFactor
            // 
            this.checkSaveFactor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkSaveFactor.AutoSize = true;
            this.checkSaveFactor.Location = new System.Drawing.Point(101, 29);
            this.checkSaveFactor.Name = "checkSaveFactor";
            this.checkSaveFactor.Size = new System.Drawing.Size(85, 17);
            this.checkSaveFactor.TabIndex = 34;
            this.checkSaveFactor.Text = "Apply Factor";
            this.toolTip.SetToolTip(this.checkSaveFactor, resources.GetString("checkSaveFactor.ToolTip"));
            this.checkSaveFactor.UseVisualStyleBackColor = true;
            this.checkSaveFactor.CheckedChanged += new System.EventHandler(this.checkSaveFactor_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 12);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(49, 13);
            this.label6.TabIndex = 33;
            this.label6.Text = "Save as:";
            // 
            // comboSaveAs
            // 
            this.comboSaveAs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboSaveAs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSaveAs.FormattingEnabled = true;
            this.comboSaveAs.Location = new System.Drawing.Point(8, 27);
            this.comboSaveAs.MaxDropDownItems = 30;
            this.comboSaveAs.Name = "comboSaveAs";
            this.comboSaveAs.Size = new System.Drawing.Size(86, 21);
            this.comboSaveAs.TabIndex = 1;
            this.comboSaveAs.SelectedIndexChanged += new System.EventHandler(this.comboSaveAs_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 52);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(52, 13);
            this.label5.TabIndex = 32;
            this.label5.Text = "Filename:";
            // 
            // textFileName
            // 
            this.textFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textFileName.Location = new System.Drawing.Point(8, 68);
            this.textFileName.Name = "textFileName";
            this.textFileName.Size = new System.Drawing.Size(229, 20);
            this.textFileName.TabIndex = 2;
            // 
            // checkLegend
            // 
            this.checkLegend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkLegend.AutoSize = true;
            this.checkLegend.Location = new System.Drawing.Point(678, 4);
            this.checkLegend.Name = "checkLegend";
            this.checkLegend.Size = new System.Drawing.Size(62, 17);
            this.checkLegend.TabIndex = 33;
            this.checkLegend.Text = "Legend";
            this.toolTip.SetToolTip(this.checkLegend, "Show the legends with channel name and Min/Max voltages");
            this.checkLegend.UseVisualStyleBackColor = true;
            this.checkLegend.CheckedChanged += new System.EventHandler(this.checkLegend_CheckedChanged);
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 32000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            // 
            // linkUpdate
            // 
            this.linkUpdate.AutoSize = true;
            this.linkUpdate.LinkColor = System.Drawing.Color.Lime;
            this.linkUpdate.Location = new System.Drawing.Point(4, 1);
            this.linkUpdate.Name = "linkUpdate";
            this.linkUpdate.Size = new System.Drawing.Size(113, 13);
            this.linkUpdate.TabIndex = 103;
            this.linkUpdate.TabStop = true;
            this.linkUpdate.Text = "Check for new version";
            this.toolTip.SetToolTip(this.linkUpdate, "Check on the ElmüSoft homepage if there is a new version");
            this.linkUpdate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkUpdate_LinkClicked);
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabOszi);
            this.tabControl.Controls.Add(this.tabDecoder);
            this.tabControl.Location = new System.Drawing.Point(15, 110);
            this.tabControl.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(854, 380);
            this.tabControl.TabIndex = 100;
            // 
            // tabOszi
            // 
            this.tabOszi.Controls.Add(this.osziPanel);
            this.tabOszi.Location = new System.Drawing.Point(4, 22);
            this.tabOszi.Name = "tabOszi";
            this.tabOszi.Size = new System.Drawing.Size(846, 354);
            this.tabOszi.TabIndex = 0;
            this.tabOszi.Text = "Oszi";
            this.tabOszi.UseVisualStyleBackColor = true;
            // 
            // osziPanel
            // 
            this.osziPanel.AutoScroll = true;
            this.osziPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.osziPanel.BackColor = System.Drawing.Color.Transparent;
            this.osziPanel.DispSteps = 0;
            this.osziPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.osziPanel.Location = new System.Drawing.Point(0, 0);
            this.osziPanel.Name = "osziPanel";
            this.osziPanel.SeparateChannels = false;
            this.osziPanel.Size = new System.Drawing.Size(846, 354);
            this.osziPanel.TabIndex = 0;
            this.osziPanel.TabStop = true;
            this.osziPanel.Zoom = 0;
            // 
            // tabDecoder
            // 
            this.tabDecoder.Controls.Add(this.rtfViewer);
            this.tabDecoder.Location = new System.Drawing.Point(4, 22);
            this.tabDecoder.Name = "tabDecoder";
            this.tabDecoder.Size = new System.Drawing.Size(838, 363);
            this.tabDecoder.TabIndex = 1;
            this.tabDecoder.Text = "Decoder";
            this.tabDecoder.UseVisualStyleBackColor = true;
            // 
            // rtfViewer
            // 
            this.rtfViewer.BackColor = System.Drawing.Color.Black;
            this.rtfViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtfViewer.ForeColor = System.Drawing.Color.White;
            this.rtfViewer.Location = new System.Drawing.Point(0, 0);
            this.rtfViewer.Name = "rtfViewer";
            this.rtfViewer.Size = new System.Drawing.Size(838, 363);
            this.rtfViewer.TabIndex = 1;
            this.rtfViewer.Text = "";
            // 
            // panelHint
            // 
            this.panelHint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panelHint.Controls.Add(this.lblInfo);
            this.panelHint.Controls.Add(this.checkSepChannels);
            this.panelHint.Controls.Add(this.checkLegend);
            this.panelHint.Location = new System.Drawing.Point(131, 108);
            this.panelHint.Name = "panelHint";
            this.panelHint.Size = new System.Drawing.Size(738, 22);
            this.panelHint.TabIndex = 0;
            // 
            // pictureLogo
            // 
            this.pictureLogo.Location = new System.Drawing.Point(17, 8);
            this.pictureLogo.Name = "pictureLogo";
            this.pictureLogo.Size = new System.Drawing.Size(96, 96);
            this.pictureLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureLogo.TabIndex = 101;
            this.pictureLogo.TabStop = false;
            // 
            // trackAnalogHeight
            // 
            this.trackAnalogHeight.BackColor = System.Drawing.Color.DimGray;
            this.trackAnalogHeight.Location = new System.Drawing.Point(861, 8);
            this.trackAnalogHeight.Name = "trackAnalogHeight";
            this.trackAnalogHeight.Size = new System.Drawing.Size(214, 45);
            this.trackAnalogHeight.TabIndex = 102;
            this.trackAnalogHeight.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackAnalogHeight.Scroll += new System.EventHandler(this.trackAnalogHeight_Scroll);
            // 
            // lblAnalogHeight
            // 
            this.lblAnalogHeight.AutoSize = true;
            this.lblAnalogHeight.Location = new System.Drawing.Point(4, 1);
            this.lblAnalogHeight.Name = "lblAnalogHeight";
            this.lblAnalogHeight.Size = new System.Drawing.Size(77, 13);
            this.lblAnalogHeight.TabIndex = 35;
            this.lblAnalogHeight.Text = "Analog Height:";
            // 
            // trackDigitalHeight
            // 
            this.trackDigitalHeight.BackColor = System.Drawing.Color.DimGray;
            this.trackDigitalHeight.Location = new System.Drawing.Point(861, 39);
            this.trackDigitalHeight.Name = "trackDigitalHeight";
            this.trackDigitalHeight.Size = new System.Drawing.Size(214, 45);
            this.trackDigitalHeight.TabIndex = 103;
            this.trackDigitalHeight.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackDigitalHeight.Scroll += new System.EventHandler(this.trackDigital_Scroll);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.linkUpdate);
            this.panel1.Controls.Add(this.lblDigitalHeight);
            this.panel1.Controls.Add(this.lblAnalogHeight);
            this.panel1.Location = new System.Drawing.Point(861, 66);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(211, 37);
            this.panel1.TabIndex = 104;
            // 
            // lblDigitalHeight
            // 
            this.lblDigitalHeight.AutoSize = true;
            this.lblDigitalHeight.Location = new System.Drawing.Point(4, 19);
            this.lblDigitalHeight.Name = "lblDigitalHeight";
            this.lblDigitalHeight.Size = new System.Drawing.Size(76, 13);
            this.lblDigitalHeight.TabIndex = 36;
            this.lblDigitalHeight.Text = "Digital  Height:";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(881, 516);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.trackDigitalHeight);
            this.Controls.Add(this.trackAnalogHeight);
            this.Controls.Add(this.pictureLogo);
            this.Controls.Add(this.panelHint);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.statusStrip);
            this.ForeColor = System.Drawing.Color.White;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(700, 474);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Oszi";
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tabOszi.ResumeLayout(false);
            this.tabDecoder.ResumeLayout(false);
            this.panelHint.ResumeLayout(false);
            this.panelHint.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackAnalogHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackDigitalHeight)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private OsziPanel osziPanel;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.CheckBox checkSepChannels;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboInput;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox comboFactor;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboSaveAs;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textFileName;
        private System.Windows.Forms.CheckBox checkLegend;
        private System.Windows.Forms.Button btnCapture;
        private System.Windows.Forms.ComboBox comboOsziModel;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label lblFactorHint;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.CheckBox checkSaveFactor;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabOszi;
        private System.Windows.Forms.TabPage tabDecoder;
        private System.Windows.Forms.Panel panelHint;
        private System.Windows.Forms.Label lblDispSamples;
        private System.Windows.Forms.PictureBox pictureLogo;
        private System.Windows.Forms.TrackBar trackAnalogHeight;
        private System.Windows.Forms.Label lblAnalogHeight;
        private System.Windows.Forms.TrackBar trackDigitalHeight;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblDigitalHeight;
        private RtfViewer rtfViewer;
        private System.Windows.Forms.LinkLabel linkSave;
        private System.Windows.Forms.LinkLabel linkUpdate;
    }
}

