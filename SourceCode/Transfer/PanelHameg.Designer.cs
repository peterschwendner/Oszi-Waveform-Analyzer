namespace Transfer
{
    partial class PanelHameg
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupTransfer = new System.Windows.Forms.GroupBox();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnAuto = new System.Windows.Forms.Button();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnSingle = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lblSerie = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblBrand = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblModel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblSerial = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblFirmware = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblTimeBase = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblSampleRate = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lblAcqState = new System.Windows.Forms.Label();
            this.radioScreen = new System.Windows.Forms.RadioButton();
            this.radioMemory = new System.Windows.Forms.RadioButton();
            this.btnTransfer = new System.Windows.Forms.Button();
            this.groupTransfer.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupTransfer
            // 
            this.groupTransfer.Controls.Add(this.btnTransfer);
            this.groupTransfer.Controls.Add(this.radioMemory);
            this.groupTransfer.Controls.Add(this.radioScreen);
            this.groupTransfer.Controls.Add(this.lblAcqState);
            this.groupTransfer.Controls.Add(this.label8);
            this.groupTransfer.Controls.Add(this.lblSampleRate);
            this.groupTransfer.Controls.Add(this.label7);
            this.groupTransfer.Controls.Add(this.lblTimeBase);
            this.groupTransfer.Controls.Add(this.label6);
            this.groupTransfer.Controls.Add(this.lblFirmware);
            this.groupTransfer.Controls.Add(this.label5);
            this.groupTransfer.Controls.Add(this.lblSerial);
            this.groupTransfer.Controls.Add(this.label4);
            this.groupTransfer.Controls.Add(this.lblModel);
            this.groupTransfer.Controls.Add(this.label3);
            this.groupTransfer.Controls.Add(this.lblBrand);
            this.groupTransfer.Controls.Add(this.label2);
            this.groupTransfer.Controls.Add(this.lblSerie);
            this.groupTransfer.Controls.Add(this.label1);
            this.groupTransfer.Controls.Add(this.btnSingle);
            this.groupTransfer.Controls.Add(this.btnStop);
            this.groupTransfer.Controls.Add(this.btnRun);
            this.groupTransfer.Controls.Add(this.btnAuto);
            this.groupTransfer.Controls.Add(this.btnReset);
            this.groupTransfer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.groupTransfer.Location = new System.Drawing.Point(5, 2);
            this.groupTransfer.Name = "groupTransfer";
            this.groupTransfer.Size = new System.Drawing.Size(540, 155);
            this.groupTransfer.TabIndex = 0;
            this.groupTransfer.TabStop = false;
            this.groupTransfer.Text = "  Hameg Waveform Transfer  ";
            // 
            // btnReset
            // 
            this.btnReset.ForeColor = System.Drawing.Color.Black;
            this.btnReset.Location = new System.Drawing.Point(16, 19);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(60, 23);
            this.btnReset.TabIndex = 1;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnAuto
            // 
            this.btnAuto.ForeColor = System.Drawing.Color.Black;
            this.btnAuto.Location = new System.Drawing.Point(86, 19);
            this.btnAuto.Name = "btnAuto";
            this.btnAuto.Size = new System.Drawing.Size(60, 23);
            this.btnAuto.TabIndex = 2;
            this.btnAuto.Text = "Auto";
            this.btnAuto.UseVisualStyleBackColor = true;
            this.btnAuto.Click += new System.EventHandler(this.btnAuto_Click);
            // 
            // btnRun
            // 
            this.btnRun.ForeColor = System.Drawing.Color.Black;
            this.btnRun.Location = new System.Drawing.Point(156, 19);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(60, 23);
            this.btnRun.TabIndex = 3;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnStop
            // 
            this.btnStop.ForeColor = System.Drawing.Color.Black;
            this.btnStop.Location = new System.Drawing.Point(226, 19);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(60, 23);
            this.btnStop.TabIndex = 4;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnSingle
            // 
            this.btnSingle.ForeColor = System.Drawing.Color.Black;
            this.btnSingle.Location = new System.Drawing.Point(296, 19);
            this.btnSingle.Name = "btnSingle";
            this.btnSingle.Size = new System.Drawing.Size(60, 23);
            this.btnSingle.TabIndex = 5;
            this.btnSingle.Text = "Single";
            this.btnSingle.UseVisualStyleBackColor = true;
            this.btnSingle.Click += new System.EventHandler(this.btnSingle_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(13, 50);
            this.label1.Name = "label1";
            this.label1.TabIndex = 6;
            this.label1.Text = "Serie:";
            // 
            // lblSerie
            // 
            this.lblSerie.AutoSize = true;
            this.lblSerie.ForeColor = System.Drawing.Color.Cyan;
            this.lblSerie.Location = new System.Drawing.Point(75, 50);
            this.lblSerie.Name = "lblSerie";
            this.lblSerie.TabIndex = 7;
            this.lblSerie.Text = "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(13, 66);
            this.label2.Name = "label2";
            this.label2.TabIndex = 8;
            this.label2.Text = "Brand:";
            // 
            // lblBrand
            // 
            this.lblBrand.AutoSize = true;
            this.lblBrand.ForeColor = System.Drawing.Color.Cyan;
            this.lblBrand.Location = new System.Drawing.Point(75, 66);
            this.lblBrand.Name = "lblBrand";
            this.lblBrand.TabIndex = 9;
            this.lblBrand.Text = "";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(13, 82);
            this.label3.Name = "label3";
            this.label3.TabIndex = 10;
            this.label3.Text = "Model:";
            // 
            // lblModel
            // 
            this.lblModel.AutoSize = true;
            this.lblModel.ForeColor = System.Drawing.Color.Cyan;
            this.lblModel.Location = new System.Drawing.Point(75, 82);
            this.lblModel.Name = "lblModel";
            this.lblModel.TabIndex = 11;
            this.lblModel.Text = "";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(13, 98);
            this.label4.Name = "label4";
            this.label4.TabIndex = 12;
            this.label4.Text = "Serial:";
            // 
            // lblSerial
            // 
            this.lblSerial.AutoSize = true;
            this.lblSerial.ForeColor = System.Drawing.Color.Cyan;
            this.lblSerial.Location = new System.Drawing.Point(75, 98);
            this.lblSerial.Name = "lblSerial";
            this.lblSerial.TabIndex = 13;
            this.lblSerial.Text = "";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(13, 114);
            this.label5.Name = "label5";
            this.label5.TabIndex = 14;
            this.label5.Text = "Firmware:";
            // 
            // lblFirmware
            // 
            this.lblFirmware.AutoSize = true;
            this.lblFirmware.ForeColor = System.Drawing.Color.Cyan;
            this.lblFirmware.Location = new System.Drawing.Point(75, 114);
            this.lblFirmware.Name = "lblFirmware";
            this.lblFirmware.TabIndex = 15;
            this.lblFirmware.Text = "";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(335, 50);
            this.label6.Name = "label6";
            this.label6.TabIndex = 16;
            this.label6.Text = "Timebase:";
            // 
            // lblTimeBase
            // 
            this.lblTimeBase.AutoSize = true;
            this.lblTimeBase.ForeColor = System.Drawing.Color.Cyan;
            this.lblTimeBase.Location = new System.Drawing.Point(411, 50);
            this.lblTimeBase.Name = "lblTimeBase";
            this.lblTimeBase.TabIndex = 17;
            this.lblTimeBase.Text = "";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.ForeColor = System.Drawing.Color.White;
            this.label7.Location = new System.Drawing.Point(335, 66);
            this.label7.Name = "label7";
            this.label7.TabIndex = 18;
            this.label7.Text = "Samplerate:";
            // 
            // lblSampleRate
            // 
            this.lblSampleRate.AutoSize = true;
            this.lblSampleRate.ForeColor = System.Drawing.Color.Cyan;
            this.lblSampleRate.Location = new System.Drawing.Point(411, 66);
            this.lblSampleRate.Name = "lblSampleRate";
            this.lblSampleRate.TabIndex = 19;
            this.lblSampleRate.Text = "";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.ForeColor = System.Drawing.Color.White;
            this.label8.Location = new System.Drawing.Point(335, 82);
            this.label8.Name = "label8";
            this.label8.TabIndex = 20;
            this.label8.Text = "Acquisition:";
            // 
            // lblAcqState
            // 
            this.lblAcqState.AutoSize = true;
            this.lblAcqState.ForeColor = System.Drawing.Color.Cyan;
            this.lblAcqState.Location = new System.Drawing.Point(411, 82);
            this.lblAcqState.Name = "lblAcqState";
            this.lblAcqState.TabIndex = 21;
            this.lblAcqState.Text = "";
            // 
            // radioScreen
            // 
            this.radioScreen.AutoSize = true;
            this.radioScreen.Checked = true;
            this.radioScreen.ForeColor = System.Drawing.Color.White;
            this.radioScreen.Location = new System.Drawing.Point(298, 108);
            this.radioScreen.Name = "radioScreen";
            this.radioScreen.TabIndex = 22;
            this.radioScreen.TabStop = true;
            this.radioScreen.Text = "Transfer Visible Screen";
            this.radioScreen.UseVisualStyleBackColor = true;
            // 
            // radioMemory
            // 
            this.radioMemory.AutoSize = true;
            this.radioMemory.ForeColor = System.Drawing.Color.White;
            this.radioMemory.Location = new System.Drawing.Point(298, 126);
            this.radioMemory.Name = "radioMemory";
            this.radioMemory.TabIndex = 23;
            this.radioMemory.Text = "Transfer Entire Memory";
            this.radioMemory.UseVisualStyleBackColor = true;
            // 
            // btnTransfer
            // 
            this.btnTransfer.ForeColor = System.Drawing.Color.Black;
            this.btnTransfer.Location = new System.Drawing.Point(443, 114);
            this.btnTransfer.Name = "btnTransfer";
            this.btnTransfer.Size = new System.Drawing.Size(85, 23);
            this.btnTransfer.TabIndex = 24;
            this.btnTransfer.Text = "Transfer";
            this.btnTransfer.UseVisualStyleBackColor = true;
            this.btnTransfer.Click += new System.EventHandler(this.btnTransfer_Click);
            // 
            // PanelHameg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.Controls.Add(this.groupTransfer);
            this.Name = "PanelHameg";
            this.Size = new System.Drawing.Size(549, 162);
            this.groupTransfer.ResumeLayout(false);
            this.groupTransfer.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupTransfer;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnAuto;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnSingle;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblSerie;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblBrand;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblModel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblSerial;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblFirmware;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblTimeBase;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblSampleRate;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lblAcqState;
        private System.Windows.Forms.RadioButton radioScreen;
        private System.Windows.Forms.RadioButton radioMemory;
        private System.Windows.Forms.Button btnTransfer;
    }
}
