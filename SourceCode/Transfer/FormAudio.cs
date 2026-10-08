/*
------------------------------------------------------------
Oscilloscope Waveform Analyzer by ElmüSoft (www.netcult.ch/elmue)
This code is released under the terms of the GNU General Public License.
Modified/added by Peter Schwendner, 2026. See Git history for detailed authorship.
------------------------------------------------------------

NAMING CONVENTIONS which allow to see the type of a variable immediately without having to jump to the variable declaration:
 
     cName  for class    definitions
     tName  for type     definitions
     eName  for enum     definitions
     kName  for "konstruct" (struct) definitions (letter 's' already used for string)
   delName  for delegate definitions

    b_Name  for bool
    c_Name  for Char, also Color
    d_Name  for double
    e_Name  for enum variables
    f_Name  for function delegates, also float
    i_Name  for instances of classes
    k_Name  for "konstructs" (struct) (letter 's' already used for string)
	r_Name  for Rectangle
    s_Name  for strings
    o_Name  for objects
 
   s8_Name  for   signed  8 Bit (sbyte)
  s16_Name  for   signed 16 Bit (short)
  s32_Name  for   signed 32 Bit (int)
  s64_Name  for   signed 64 Bit (long)
   u8_Name  for unsigned  8 Bit (byte)
  u16_Name  for unsigned 16 bit (ushort)
  u32_Name  for unsigned 32 Bit (uint)
  u64_Name  for unsigned 64 Bit (ulong)

  An additional "m" is prefixed for all member variables (e.g. ms_String)
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

using WaterfallFFT      = Operations.WaterfallFFT;
using Fourier           = Operations.Fourier;
using SpectrumFFT       = Operations.SpectrumFFT;
using Capture           = OsziWaveformAnalyzer.Utils.Capture;
using Channel           = OsziWaveformAnalyzer.Utils.Channel;
using eRegKey           = OsziWaveformAnalyzer.Utils.eRegKey;
using Utils             = OsziWaveformAnalyzer.Utils;
using PlatformManager   = Platform.PlatformManager;

namespace Transfer
{
    /// <summary>
    /// Records a stereo signal from an audio interface or sound card into the main window,
    /// or opens the live Waterfall FFT with the audio input.
    /// With a calibration (FormCalibrate) the samples are converted from full scale into Volt.
    /// </summary>
    public class FormAudio : Form
    {
        static readonly String[] DURATIONS = { "0.1", "0.2", "0.5", "1", "2", "5", "10", "20", "30", "60" }; // seconds

        ComboBox    mi_ComboDevice;
        ComboBox    mi_ComboRate;
        ComboBox    mi_ComboDuration;
        CheckBox    mi_CheckVolt;
        Label       mi_LblCalib;
        Button      mi_BtnRecord;
        Button      mi_BtnWaterfall;
        Button      mi_BtnCalibrate;
        Button      mi_BtnLiveXY;
        ProgressBar mi_Progress;
        Label       mi_LblStatus;
        bool        mb_Recording;
        bool        mb_Abort;
        bool        mb_CloseAfterRecord;

        public FormAudio()
        {
            Text            = "Audio Input  —  Audio Interface / Sound Card";
            Icon            = Utils.FormMain != null ? Utils.FormMain.Icon : null;
            BackColor       = Color.DimGray;
            ForeColor       = Color.White;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            ShowInTaskbar   = false;
            StartPosition   = FormStartPosition.CenterParent;
            ClientSize      = new Size(520, 257);

            mi_ComboDevice   = AddCombo(this, "Device:",   12, 300);
            mi_ComboRate     = AddCombo(this, "Rate:",     44, 100);
            mi_ComboDuration = AddCombo(this, "Duration:", 76, 100);

            mi_ComboDevice.Items.Add(AudioInput.DEFAULT_DEVICE);
            foreach (String s_Name in AudioInput.EnumerateDevices())
                mi_ComboDevice.Items.Add(s_Name);
            Utils.ComboAdjustDropDownWidth(mi_ComboDevice);

            foreach (int s32_Rate in AudioInput.SAMPLE_RATES)
                mi_ComboRate.Items.Add(s32_Rate.ToString());

            foreach (String s_Sec in DURATIONS)
                mi_ComboDuration.Items.Add(s_Sec + " s");

            Label i_Hint = new Label();
            i_Hint.Text      = "Records Left and Right (stereo).";
            i_Hint.AutoSize  = true;
            i_Hint.Location  = new Point(195, 47);
            Controls.Add(i_Hint);

            mi_CheckVolt = new CheckBox();
            mi_CheckVolt.Text     = "Volt (calibrated)";
            mi_CheckVolt.AutoSize = true;
            mi_CheckVolt.Location = new Point(14, 142);
            Controls.Add(mi_CheckVolt);

            mi_LblCalib = new Label();
            mi_LblCalib.Location = new Point(135, 136);
            mi_LblCalib.Size     = new Size(250, 30);
            Controls.Add(mi_LblCalib);

            mi_BtnRecord = AddButton(this, "Record", 390, 40, 115);
            mi_BtnRecord.BackColor = Color.Salmon;
            mi_BtnRecord.Click    += new EventHandler(OnRecordClick);

            mi_BtnWaterfall = AddButton(this, "Waterfall FFT...", 390, 72, 115);
            mi_BtnWaterfall.BackColor = Color.Plum;
            mi_BtnWaterfall.Click    += new EventHandler(OnWaterfallClick);

            mi_BtnLiveXY = AddButton(this, "Live X/Y...", 390, 104, 115);
            mi_BtnLiveXY.BackColor = Color.PaleGreen;
            mi_BtnLiveXY.Click    += new EventHandler(OnLiveXYClick);

            mi_BtnCalibrate = AddButton(this, "Calibrate...", 390, 136, 115);
            mi_BtnCalibrate.BackColor = Color.BlanchedAlmond;
            mi_BtnCalibrate.Click    += new EventHandler(OnCalibrateClick);

            LinkLabel i_Help = new LinkLabel();
            i_Help.Text      = "Show Help";
            i_Help.LinkColor = Color.Lime;
            i_Help.AutoSize  = true;
            i_Help.Location  = new Point(420, 14);
            i_Help.LinkClicked += delegate { PlatformManager.Instance.ShowHelp(this, "AudioInput"); };
            Controls.Add(i_Help);

            mi_Progress = new ProgressBar();
            mi_Progress.Location = new Point(12, 177);
            mi_Progress.Size     = new Size(493, 16);
            Controls.Add(mi_Progress);

            mi_LblStatus = new Label();
            mi_LblStatus.Location  = new Point(12, 200);
            mi_LblStatus.Size      = new Size(493, 50);
            mi_LblStatus.Text      = "Records both channels into the main window, where you can use FFT Spectrum, X/Y Plot and save the capture.";
            Controls.Add(mi_LblStatus);

            // "Device name|Rate|Samples" is shared with the Waterfall FFT
            String[] s_Settg = Utils.RegReadString(eRegKey.AudioInput, "Windows default|48000|8192").Split('|');
            mi_ComboDevice.Text = s_Settg[0];
            if (mi_ComboDevice.SelectedIndex < 0) mi_ComboDevice.SelectedIndex = 0;
            mi_ComboRate.Text = s_Settg.Length > 1 ? s_Settg[1] : "48000";
            if (mi_ComboRate.SelectedIndex < 0) mi_ComboRate.SelectedIndex = 1;
            mi_ComboDuration.Text = Utils.RegReadString(eRegKey.AudioRecord, "1 s");
            if (mi_ComboDuration.SelectedIndex < 0) mi_ComboDuration.SelectedIndex = 3;
            mi_CheckVolt.Checked = Utils.RegReadBool(eRegKey.AudioUseCalib);

            mi_ComboDevice.SelectedIndexChanged += delegate { UpdateCalibration(); };
            UpdateCalibration();
        }

        internal static ComboBox AddCombo(Control i_Parent, String s_Label, int Y, int s32_Width)
        {
            Label i_Label = new Label();
            i_Label.Text     = s_Label;
            i_Label.AutoSize = true;
            i_Label.Location = new Point(12, Y + 3);
            i_Parent.Controls.Add(i_Label);

            ComboBox i_Combo = new ComboBox();
            i_Combo.DropDownStyle = ComboBoxStyle.DropDownList;
            i_Combo.Location      = new Point(80, Y);
            i_Combo.Width         = s32_Width;
            i_Parent.Controls.Add(i_Combo);
            return i_Combo;
        }

        internal static Button AddButton(Control i_Parent, String s_Text, int X, int Y, int s32_Width)
        {
            Button i_Button = new Button();
            i_Button.Text      = s_Text;
            i_Button.ForeColor = Color.Black;
            i_Button.Location  = new Point(X, Y);
            i_Button.Size      = new Size(s32_Width, 25);
            i_Parent.Controls.Add(i_Button);
            return i_Button;
        }

        /// <summary>
        /// Shows the calibration of the selected device. "Volt" can only be checked if at least one channel is calibrated.
        /// </summary>
        void UpdateCalibration()
        {
            double d_Left  = AudioInput.GetCalibration(mi_ComboDevice.Text, 1);
            double d_Right = AudioInput.GetCalibration(mi_ComboDevice.Text, 2);

            mi_LblCalib.Text = "Left:  " + AudioInput.FormatCalibration(d_Left) + "\nRight: " + AudioInput.FormatCalibration(d_Right);
            mi_CheckVolt.Enabled = !double.IsNaN(d_Left) || !double.IsNaN(d_Right);
            mi_LblCalib.ForeColor = mi_CheckVolt.Enabled ? Color.Cyan : Color.Silver;
        }

        bool UseVolt
        {
            get { return mi_CheckVolt.Enabled && mi_CheckVolt.Checked; }
        }

        void SaveSettings()
        {
            // Keep the samples per spectrum of the Waterfall FFT
            String[] s_Old = Utils.RegReadString(eRegKey.AudioInput, "Windows default|48000|8192").Split('|');
            String s_Samples = s_Old.Length > 2 ? s_Old[2] : "8192";
            Utils.RegWriteString(eRegKey.AudioInput,  mi_ComboDevice.Text + "|" + mi_ComboRate.Text + "|" + s_Samples);
            Utils.RegWriteString(eRegKey.AudioRecord, mi_ComboDuration.Text);
            Utils.RegWriteBool  (eRegKey.AudioUseCalib, mi_CheckVolt.Checked);
        }

        void PrintStatus(String s_Text, Color c_Color)
        {
            mi_LblStatus.Text      = s_Text;
            mi_LblStatus.ForeColor = c_Color;
        }

        void EnableControls(bool b_Enable)
        {
            mi_BtnWaterfall.Enabled  = b_Enable;
            mi_BtnCalibrate.Enabled  = b_Enable;
            mi_BtnLiveXY.Enabled     = b_Enable;
            mi_ComboDevice.Enabled   = b_Enable;
            mi_ComboRate.Enabled     = b_Enable;
            mi_ComboDuration.Enabled = b_Enable;
            mi_CheckVolt.Enabled     = b_Enable;
            if (b_Enable)
                UpdateCalibration();
        }

        // ===============================================================================

        void OnRecordClick(object sender, EventArgs e)
        {
            if (mb_Recording)
            {
                mb_Abort = true;
                return;
            }

            if (Utils.FormMain != null && Utils.FormMain.HasUnsavedChanges())
                return;

            bool b_Volt = UseVolt;
            SaveSettings();

            int    s32_Rate    = int.Parse(mi_ComboRate.Text);
            double d_Seconds   = double.Parse(mi_ComboDuration.Text.Split(' ')[0], CultureInfo.InvariantCulture);
            int    s32_Samples = (int)Math.Round(d_Seconds * s32_Rate);

            mb_Recording = true;
            mb_Abort     = false;
            mi_BtnRecord.Text = "Cancel";
            EnableControls(false);
            mi_Progress.Value = 0;
            PrintStatus("Recording " + mi_ComboDuration.Text + " ...", Color.White);

            Capture i_Capture = null;
            String  s_Error   = null;
            try
            {
                i_Capture = new AudioInput().Record(mi_ComboDevice.Text, s32_Rate, s32_Samples, OnProgress);
            }
            catch (Exception Ex)
            {
                s_Error = Ex.Message;
            }

            mb_Recording = false;
            mi_BtnRecord.Text = "Record";
            EnableControls(true);

            if (s_Error != null)
            {
                mi_Progress.Value = 0;
                PrintStatus("Error: " + s_Error.Replace("\n", " "), Color.FromArgb(0xFF, 0xA0, 0x80));
            }
            else if (i_Capture == null)
            {
                mi_Progress.Value = 0;
                PrintStatus("Aborted", Color.FromArgb(0xFF, 0xA0, 0x80));
            }
            else
            {
                String s_Levels = GetLevels(i_Capture, b_Volt);
                Utils.FormMain.StoreNewCapture(i_Capture);
                PrintStatus(s_Levels, s_Levels.Contains("CLIPPED") ? Color.FromArgb(0xFF, 0xA0, 0x80) : Color.LightGreen);
            }

            if (mb_CloseAfterRecord)
                Close();
        }

        bool OnProgress(int s32_Recorded, int s32_Total)
        {
            mi_Progress.Value = (int)(100L * s32_Recorded / Math.Max(1, s32_Total));
            Application.DoEvents(); // allow the user to click "Cancel"
            return mb_Abort;
        }

        /// <summary>
        /// Measures the peak level of both channels, so the user can adjust the gain of the interface.
        /// b_Volt = true --> converts the calibrated channels into Volt. Uncalibrated channels stay in full scale.
        /// </summary>
        String GetLevels(Capture i_Capture, bool b_Volt)
        {
            String s_Levels = "";
            bool   b_Clip   = false;
            for (int C=0; C<i_Capture.mi_Channels.Count; C++)
            {
                Channel i_Channel = i_Capture.mi_Channels[C];
                float f_Peak = 0;
                int s32_Clipped = 0;
                foreach (float f_Value in i_Channel.mf_Analog)
                {
                    float f_Abs = Math.Abs(f_Value);
                    f_Peak = Math.Max(f_Peak, f_Abs);
                    if (f_Abs >= 0.999f)
                        s32_Clipped ++;
                }
                s_Levels += String.Format(CultureInfo.InvariantCulture, "   {0}: {1:0.0} dBFS", i_Channel.ms_Name, Fourier.ToDb(f_Peak, Fourier.eUnit.FullScale));

                double d_Cal = AudioInput.GetCalibration(mi_ComboDevice.Text, C + 1);
                if (b_Volt && !double.IsNaN(d_Cal))
                {
                    float f_Factor = (float)AudioInput.FullScaleVolt(d_Cal);
                    for (int S=0; S<i_Channel.mf_Analog.Length; S++)
                        i_Channel.mf_Analog[S] *= f_Factor;

                    s_Levels += " = " + SpectrumFFT.FormatVolt(f_Peak * f_Factor) + "p";
                    i_Capture.mb_FullScale = false;
                }
                else if (b_Volt)
                {
                    i_Channel.ms_Name += " (not calibrated)";
                }

                if (s32_Clipped > 0)
                {
                    s_Levels += " (CLIPPED)";
                    b_Clip = true;
                }
            }

            String s_Text = String.Format(CultureInfo.InvariantCulture, "Recorded {0:N0} samples at {1} Hz into the main window{2}.\nPeak level:{3}",
                                          i_Capture.ms32_Samples, mi_ComboRate.Text, b_Volt ? " in Volt" : " (full scale)", s_Levels);
            if (b_Clip)
                s_Text += "   --> reduce the gain!";
            return s_Text;
        }

        void OnWaterfallClick(object sender, EventArgs e)
        {
            SaveSettings();
            using (WaterfallFFT i_Waterfall = new WaterfallFFT(new AudioInput(UseVolt)))
            {
                i_Waterfall.ShowDialog(this);
            }
        }

        /// <summary>
        /// Volt is only used if both channels are calibrated (X and Y must have the same unit)
        /// </summary>
        void OnLiveXYClick(object sender, EventArgs e)
        {
            SaveSettings();
            double d_Left  = AudioInput.GetCalibration(mi_ComboDevice.Text, 1);
            double d_Right = AudioInput.GetCalibration(mi_ComboDevice.Text, 2);
            bool   b_Volt  = UseVolt && !double.IsNaN(d_Left) && !double.IsNaN(d_Right);
            if (UseVolt && !b_Volt)
                PrintStatus("Live X/Y uses full scale, because Volt requires the calibration of both channels.", Color.FromArgb(0xFF, 0xD0, 0x80));

            AudioXYStream i_Stream = new AudioXYStream(mi_ComboDevice.Text, int.Parse(mi_ComboRate.Text), b_Volt);
            using (FormLiveXY i_LiveXY = new FormLiveXY(i_Stream, b_Volt,
                                                        b_Volt ? AudioInput.FullScaleVolt(d_Left)  : 1.0,
                                                        b_Volt ? AudioInput.FullScaleVolt(d_Right) : 1.0))
            {
                i_LiveXY.ShowDialog(this);
            }
        }

        void OnCalibrateClick(object sender, EventArgs e)
        {
            SaveSettings();
            using (FormCalibrate i_Calib = new FormCalibrate(mi_ComboDevice.Text, int.Parse(mi_ComboRate.Text)))
            {
                i_Calib.ShowDialog(this);
            }
            UpdateCalibration();
            if (mi_CheckVolt.Enabled)
                mi_CheckVolt.Checked = true;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // The recording must finish first
            if (mb_Recording)
            {
                e.Cancel = true;
                mb_CloseAfterRecord = true;
                mb_Abort = true;
                return;
            }
            base.OnFormClosing(e);
        }
    }

    // ==================================================================================================================

    /// <summary>
    /// Calibration of an audio input: a sine wave with known voltage is connected and measured.
    /// The result is the level in dBV of a full scale sine wave. It can also be entered manually.
    /// </summary>
    public class FormCalibrate : Form
    {
        String   ms_Device;
        int      ms32_Rate;
        TextBox  mi_TextKnown;
        TextBox[] mi_TextCal = new TextBox[2];
        Button[] mi_BtnMeasure = new Button[2];
        Label    mi_LblStatus;
        bool     mb_Measuring;

        public FormCalibrate(String s_Device, int s32_Rate)
        {
            ms_Device   = s_Device;
            ms32_Rate   = s32_Rate;

            Text            = "Calibrate  —  " + s_Device;
            Icon            = Utils.FormMain != null ? Utils.FormMain.Icon : null;
            BackColor       = Color.DimGray;
            ForeColor       = Color.White;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            ShowInTaskbar   = false;
            StartPosition   = FormStartPosition.CenterParent;
            ClientSize      = new Size(520, 300);

            Label i_Info = new Label();
            i_Info.Location = new Point(12, 10);
            i_Info.Size     = new Size(496, 92);
            i_Info.Text     = "1. Connect a sine wave (approx 50 Hz ... 10 kHz) with known voltage, e.g. from a generator,\n"
                            + "    measured with a multimeter or an oscilloscope.\n"
                            + "2. Set the gain of the interface as you will use it. The calibration is only valid for this gain!\n"
                            + "3. Enter the voltage and click 'Measure' for the channel.\n\n"
                            + "The result is the level of a full scale sine wave (0 dBFS) in dBV. It can also be entered directly.";
            Controls.Add(i_Info);

            Label i_LblKnown = new Label();
            i_LblKnown.Text     = "Known signal:";
            i_LblKnown.AutoSize = true;
            i_LblKnown.Location = new Point(12, 113);
            Controls.Add(i_LblKnown);

            mi_TextKnown = new TextBox();
            mi_TextKnown.Location = new Point(110, 110);
            mi_TextKnown.Width    = 70;
            mi_TextKnown.Text     = "1.000";
            Controls.Add(mi_TextKnown);

            Label i_LblUnit = new Label();
            i_LblUnit.Text     = "Volt RMS  (Vpeak = Vrms × 1.414)";
            i_LblUnit.AutoSize = true;
            i_LblUnit.Location = new Point(186, 113);
            Controls.Add(i_LblUnit);

            String[] s_Names = { "Left:", "Right:" };
            for (int C=0; C<2; C++)
            {
                int Y = 145 + C * 32;
                Label i_Lbl = new Label();
                i_Lbl.Text     = s_Names[C] + "   0 dBFS =";
                i_Lbl.AutoSize = true;
                i_Lbl.Location = new Point(12, Y + 3);
                Controls.Add(i_Lbl);

                mi_TextCal[C] = new TextBox();
                mi_TextCal[C].Location = new Point(110, Y);
                mi_TextCal[C].Width    = 70;
                double d_Cal = AudioInput.GetCalibration(s_Device, C + 1);
                mi_TextCal[C].Text = double.IsNaN(d_Cal) ? "" : d_Cal.ToString("0.00", CultureInfo.InvariantCulture);
                Controls.Add(mi_TextCal[C]);

                Label i_Db = new Label();
                i_Db.Text     = "dBV   (empty = not calibrated)";
                i_Db.AutoSize = true;
                i_Db.Location = new Point(186, Y + 3);
                Controls.Add(i_Db);

                int s32_Chan = C + 1;
                mi_BtnMeasure[C] = FormAudio.AddButton(this, "Measure " + s_Names[C].TrimEnd(':'), 390, Y - 2, 115);
                mi_BtnMeasure[C].BackColor = Color.LightSkyBlue;
                mi_BtnMeasure[C].Click += delegate { Measure(s32_Chan); };
            }

            mi_LblStatus = new Label();
            mi_LblStatus.Location = new Point(12, 210);
            mi_LblStatus.Size     = new Size(496, 45);
            Controls.Add(mi_LblStatus);

            Button i_OK = FormAudio.AddButton(this, "Save", 300, 262, 100);
            i_OK.BackColor = Color.PaleGreen;
            i_OK.Click += new EventHandler(OnSaveClick);

            Button i_Cancel = FormAudio.AddButton(this, "Cancel", 405, 262, 100);
            i_Cancel.DialogResult = DialogResult.Cancel;
            CancelButton = i_Cancel;
        }

        void PrintStatus(String s_Text, Color c_Color)
        {
            mi_LblStatus.Text      = s_Text;
            mi_LblStatus.ForeColor = c_Color;
        }

        /// <summary>
        /// Records 1 second and measures the amplitude of the strongest sine wave with the flat top window.
        /// The flat top window measures the amplitude exactly, independent of the frequency.
        /// Hum and noise do not falsify the result as they would with an RMS measurement.
        /// </summary>
        void Measure(int s32_Chan)
        {
            if (mb_Measuring)
                return;

            double d_Vrms;
            if (!double.TryParse(mi_TextKnown.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out d_Vrms) || d_Vrms <= 0)
            {
                PrintStatus("Enter the known voltage in Volt RMS, e.g. 1.000", Color.FromArgb(0xFF, 0xA0, 0x80));
                return;
            }

            mb_Measuring = true;
            Cursor = Cursors.WaitCursor;
            PrintStatus("Measuring ...", Color.White);
            try
            {
                Capture i_Capture = new AudioInput().Record(ms_Device, ms32_Rate, ms32_Rate, delegate { Application.DoEvents(); return false; });
                float[] f_Samples = i_Capture.mi_Channels[s32_Chan - 1].mf_Analog;

                double d_Freq;
                double d_DbFS = MeasureSine(f_Samples, ms32_Rate, out d_Freq);

                // dBV of the known signal - dBFS of the measured signal = dBV of a full scale sine wave
                double d_Cal = 20 * Math.Log10(d_Vrms) - d_DbFS;
                mi_TextCal[s32_Chan - 1].Text = d_Cal.ToString("0.00", CultureInfo.InvariantCulture);

                PrintStatus(String.Format(CultureInfo.InvariantCulture, "Measured {0}: {1:0.00} dBFS at {2}  -->  0 dBFS = {3:+0.00;-0.00} dBV\nClick 'Save' to store the calibration.",
                                          s32_Chan == 1 ? "Left" : "Right", d_DbFS, SpectrumFFT.FormatFreq(d_Freq), d_Cal), Color.LightGreen);
            }
            catch (Exception Ex)
            {
                PrintStatus("Error: " + Ex.Message.Replace("\n", " "), Color.FromArgb(0xFF, 0xA0, 0x80));
            }
            mb_Measuring = false;
            Cursor = Cursors.Default;
        }

        /// <summary>
        /// Returns the level of the strongest sine wave in dBFS. Throws if the signal is not usable for a calibration.
        /// </summary>
        public static double MeasureSine(float[] f_Samples, int s32_Rate, out double d_Freq)
        {
            foreach (float f_Value in f_Samples)
            {
                if (Math.Abs(f_Value) >= 0.999f)
                    throw new Exception("The signal is clipped. Reduce the gain or the voltage.");
            }

            int s32_FftSize;
            double[] d_Amp    = Fourier.AmplitudeSpectrum(f_Samples, 0, f_Samples.Length, Fourier.eWindow.FlatTop,        true, out s32_FftSize);
            double[] d_Refine = Fourier.AmplitudeSpectrum(f_Samples, 0, f_Samples.Length, Fourier.eWindow.BlackmanHarris, true, out s32_FftSize);
            double d_BinWidth = (double)s32_Rate / s32_FftSize;
            List<Fourier.Peak> i_Peaks = Fourier.FindPeaks(d_Amp, d_Refine, d_BinWidth, 1, 3);
            if (i_Peaks.Count == 0)
                throw new Exception("No sine wave found.");

            Fourier.Peak i_Peak = i_Peaks[0];
            d_Freq = i_Peak.md_Frequency;
            double d_DbFS = Fourier.ToDb(i_Peak.md_Amplitude, Fourier.eUnit.FullScale);
            if (d_DbFS < -60)
                throw new Exception(String.Format(CultureInfo.InvariantCulture, "The signal is too weak ({0:0.0} dBFS). Increase the gain or the voltage.", d_DbFS));
            if (d_Freq < 20 || d_Freq > 0.4 * s32_Rate)
                throw new Exception("The strongest frequency is " + SpectrumFFT.FormatFreq(d_Freq) + ". Use a sine wave between 50 Hz and 10 kHz.");
            return d_DbFS;
        }

        void OnSaveClick(object sender, EventArgs e)
        {
            double[] d_Cal = new double[2];
            for (int C=0; C<2; C++)
            {
                String s_Text = mi_TextCal[C].Text.Trim().Replace(',', '.');
                if (s_Text.Length == 0)
                {
                    d_Cal[C] = double.NaN;
                }
                else if (!double.TryParse(s_Text, NumberStyles.Float, CultureInfo.InvariantCulture, out d_Cal[C]) || Math.Abs(d_Cal[C]) > 100)
                {
                    PrintStatus("Invalid value for " + (C == 0 ? "Left" : "Right") + ": " + mi_TextCal[C].Text, Color.FromArgb(0xFF, 0xA0, 0x80));
                    return;
                }
            }
            for (int C=0; C<2; C++)
                AudioInput.SetCalibration(ms_Device, C + 1, d_Cal[C]);

            DialogResult = DialogResult.OK;
        }
    }
}
