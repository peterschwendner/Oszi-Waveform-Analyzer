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
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

using WaterfallFFT      = Operations.WaterfallFFT;
using Fourier           = Operations.Fourier;
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
    /// </summary>
    public class FormAudio : Form
    {
        static readonly String[] DURATIONS = { "0.1", "0.2", "0.5", "1", "2", "5", "10", "20", "30", "60" }; // seconds

        ComboBox    mi_ComboDevice;
        ComboBox    mi_ComboRate;
        ComboBox    mi_ComboDuration;
        Button      mi_BtnRecord;
        Button      mi_BtnWaterfall;
        ProgressBar mi_Progress;
        Label       mi_LblStatus;
        AudioInput  mi_Audio;
        bool        mb_Recording;
        bool        mb_Abort;

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
            ClientSize      = new Size(520, 190);

            mi_ComboDevice   = AddCombo("Device:",   12, 300);
            mi_ComboRate     = AddCombo("Rate:",     44, 100);
            mi_ComboDuration = AddCombo("Duration:", 76, 100);

            mi_ComboDevice.Items.Add("Windows default");
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

            mi_BtnRecord = AddButton("Record", 390, 40, 115);
            mi_BtnRecord.BackColor = Color.Salmon;
            mi_BtnRecord.Click    += new EventHandler(OnRecordClick);

            mi_BtnWaterfall = AddButton("Waterfall FFT...", 390, 72, 115);
            mi_BtnWaterfall.BackColor = Color.Plum;
            mi_BtnWaterfall.Click    += new EventHandler(OnWaterfallClick);

            LinkLabel i_Help = new LinkLabel();
            i_Help.Text      = "Show Help";
            i_Help.LinkColor = Color.Lime;
            i_Help.AutoSize  = true;
            i_Help.Location  = new Point(420, 14);
            i_Help.LinkClicked += delegate { PlatformManager.Instance.ShowHelp(this, "AudioInput"); };
            Controls.Add(i_Help);

            mi_Progress = new ProgressBar();
            mi_Progress.Location = new Point(12, 118);
            mi_Progress.Size     = new Size(493, 16);
            Controls.Add(mi_Progress);

            mi_LblStatus = new Label();
            mi_LblStatus.Location  = new Point(12, 142);
            mi_LblStatus.Size      = new Size(493, 40);
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
        }

        ComboBox AddCombo(String s_Label, int Y, int s32_Width)
        {
            Label i_Label = new Label();
            i_Label.Text     = s_Label;
            i_Label.AutoSize = true;
            i_Label.Location = new Point(12, Y + 3);
            Controls.Add(i_Label);

            ComboBox i_Combo = new ComboBox();
            i_Combo.DropDownStyle = ComboBoxStyle.DropDownList;
            i_Combo.Location      = new Point(80, Y);
            i_Combo.Width         = s32_Width;
            Controls.Add(i_Combo);
            return i_Combo;
        }

        Button AddButton(String s_Text, int X, int Y, int s32_Width)
        {
            Button i_Button = new Button();
            i_Button.Text      = s_Text;
            i_Button.ForeColor = Color.Black;
            i_Button.Location  = new Point(X, Y);
            i_Button.Size      = new Size(s32_Width, 25);
            Controls.Add(i_Button);
            return i_Button;
        }

        void SaveSettings()
        {
            // Keep the samples per spectrum of the Waterfall FFT
            String[] s_Old = Utils.RegReadString(eRegKey.AudioInput, "Windows default|48000|8192").Split('|');
            String s_Samples = s_Old.Length > 2 ? s_Old[2] : "8192";
            Utils.RegWriteString(eRegKey.AudioInput,  mi_ComboDevice.Text + "|" + mi_ComboRate.Text + "|" + s_Samples);
            Utils.RegWriteString(eRegKey.AudioRecord, mi_ComboDuration.Text);
        }

        void PrintStatus(String s_Text, Color c_Color)
        {
            mi_LblStatus.Text      = s_Text;
            mi_LblStatus.ForeColor = c_Color;
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

            SaveSettings();

            int    s32_Rate    = int.Parse(mi_ComboRate.Text);
            double d_Seconds   = double.Parse(mi_ComboDuration.Text.Split(' ')[0], CultureInfo.InvariantCulture);
            int    s32_Samples = (int)Math.Round(d_Seconds * s32_Rate);

            mb_Recording = true;
            mb_Abort     = false;
            mi_BtnRecord.Text          = "Cancel";
            mi_BtnWaterfall.Enabled    = false;
            mi_ComboDevice.Enabled     = false;
            mi_ComboRate.Enabled       = false;
            mi_ComboDuration.Enabled   = false;
            mi_Progress.Value          = 0;
            PrintStatus("Recording " + mi_ComboDuration.Text + " ...", Color.White);

            Capture i_Capture = null;
            String  s_Error   = null;
            try
            {
                mi_Audio  = new AudioInput();
                i_Capture = mi_Audio.Record(mi_ComboDevice.SelectedIndex - 1, mi_ComboDevice.Text, s32_Rate, s32_Samples, OnProgress);
            }
            catch (Exception Ex)
            {
                s_Error = Ex.Message;
            }

            mb_Recording = false;
            mi_BtnRecord.Text        = "Record";
            mi_BtnWaterfall.Enabled  = true;
            mi_ComboDevice.Enabled   = true;
            mi_ComboRate.Enabled     = true;
            mi_ComboDuration.Enabled = true;

            if (s_Error != null)
            {
                mi_Progress.Value = 0;
                PrintStatus("Error: " + s_Error.Replace("\n", " "), Color.FromArgb(0xFF, 0xA0, 0x80));
                return;
            }
            if (i_Capture == null)
            {
                mi_Progress.Value = 0;
                PrintStatus("Aborted", Color.FromArgb(0xFF, 0xA0, 0x80));
                if (mb_CloseAfterRecord) Close();
                return;
            }

            Utils.FormMain.StoreNewCapture(i_Capture);
            PrintLevels(i_Capture);
            if (mb_CloseAfterRecord) Close();
        }

        bool mb_CloseAfterRecord;

        bool OnProgress(int s32_Recorded, int s32_Total)
        {
            mi_Progress.Value = (int)(100L * s32_Recorded / Math.Max(1, s32_Total));
            Application.DoEvents(); // allow the user to click "Cancel"
            return mb_Abort;
        }

        /// <summary>
        /// Shows the peak level of both channels, so the user can adjust the gain of the interface.
        /// </summary>
        void PrintLevels(Capture i_Capture)
        {
            String s_Levels = "";
            bool   b_Clip   = false;
            foreach (Channel i_Channel in i_Capture.mi_Channels)
            {
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
                if (s32_Clipped > 0)
                {
                    s_Levels += " (CLIPPED)";
                    b_Clip = true;
                }
            }

            String s_Text = String.Format(CultureInfo.InvariantCulture, "Recorded {0:N0} samples at {1} Hz into the main window.\nPeak level:{2}",
                                          i_Capture.ms32_Samples, mi_ComboRate.Text, s_Levels);
            if (b_Clip)
                s_Text += "   --> reduce the gain!";
            PrintStatus(s_Text, b_Clip ? Color.FromArgb(0xFF, 0xA0, 0x80) : Color.LightGreen);
        }

        void OnWaterfallClick(object sender, EventArgs e)
        {
            SaveSettings();
            using (WaterfallFFT i_Waterfall = new WaterfallFFT(new AudioInput()))
            {
                i_Waterfall.ShowDialog(this);
            }
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
}
