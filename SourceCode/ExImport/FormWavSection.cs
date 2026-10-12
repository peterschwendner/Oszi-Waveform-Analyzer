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

using Utils             = OsziWaveformAnalyzer.Utils;

namespace ExImport
{
    /// <summary>
    /// A WAV file is longer than WavFile.MAX_SAMPLES: the user selects the section to be loaded (start and duration).
    /// </summary>
    public class FormWavSection : Form
    {
        long     ms64_Frames;     // total samples per channel in the file
        int      ms32_Rate;
        double   md_MaxSeconds;   // the longest section that can be loaded
        TextBox  mi_TextStart;
        TextBox  mi_TextDuration;
        Label    mi_LblSection;
        Button   mi_BtnLoad;

        /// <summary>
        /// The result: first sample and count of samples of the section
        /// </summary>
        public long StartFrame { get; private set; }
        public int  FrameCount { get; private set; }

        public FormWavSection(String s_FileName, long s64_Frames, int s32_Rate, int s32_Channels)
        {
            ms64_Frames   = s64_Frames;
            ms32_Rate     = s32_Rate;
            md_MaxSeconds = (double)Math.Min(WavFile.MAX_SAMPLES, s64_Frames) / s32_Rate;

            Text            = "WAV Import  —  Select a Section";
            Icon            = Utils.FormMain != null ? Utils.FormMain.Icon : null;
            BackColor       = Color.DimGray;
            ForeColor       = Color.White;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            ShowInTaskbar   = false;
            StartPosition   = FormStartPosition.CenterParent;
            ClientSize      = new Size(520, 215);

            Label i_Info = new Label();
            i_Info.Location = new Point(12, 10);
            i_Info.Size     = new Size(496, 62);
            i_Info.Text     = String.Format(CultureInfo.InvariantCulture,
                                            "{0}\nLength: {1}   ({2:N0} samples per channel, {3} Hz, {4} channels)\n"
                                          + "The file is too long to be loaded entirely. At most {5} can be loaded.\nSelect the section:",
                                            s_FileName, WavFile.FormatTime((double)s64_Frames / s32_Rate), s64_Frames, s32_Rate, s32_Channels,
                                            WavFile.FormatTime(md_MaxSeconds));
            Controls.Add(i_Info);

            mi_TextStart    = AddField("Start:",    82,  "0:00.0",                       "m:ss or seconds");
            mi_TextDuration = AddField("Duration:", 112, WavFile.FormatTime(md_MaxSeconds), "max. " + WavFile.FormatTime(md_MaxSeconds));

            // Quick selection of the start
            AddQuickButton("Beginning", 290, 0.0);
            AddQuickButton("Middle",    365, 0.5);
            AddQuickButton("End",       440, 1.0);

            mi_LblSection = new Label();
            mi_LblSection.Location = new Point(12, 145);
            mi_LblSection.Size     = new Size(496, 20);
            Controls.Add(mi_LblSection);

            mi_BtnLoad = new Button();
            mi_BtnLoad.Text      = "Load";
            mi_BtnLoad.ForeColor = Color.Black;
            mi_BtnLoad.BackColor = Color.PaleGreen;
            mi_BtnLoad.Location  = new Point(320, 177);
            mi_BtnLoad.Size      = new Size(90, 26);
            mi_BtnLoad.Click    += new EventHandler(OnLoadClick);
            Controls.Add(mi_BtnLoad);
            AcceptButton = mi_BtnLoad;

            Button i_Cancel = new Button();
            i_Cancel.Text         = "Cancel";
            i_Cancel.ForeColor    = Color.Black;
            i_Cancel.BackColor    = SystemColors.Control;
            i_Cancel.Location     = new Point(418, 177);
            i_Cancel.Size         = new Size(90, 26);
            i_Cancel.DialogResult = DialogResult.Cancel;
            Controls.Add(i_Cancel);
            CancelButton = i_Cancel;

            mi_TextStart   .TextChanged += delegate { UpdateSection(); };
            mi_TextDuration.TextChanged += delegate { UpdateSection(); };
            UpdateSection();
        }

        TextBox AddField(String s_Label, int Y, String s_Value, String s_Hint)
        {
            Label i_Label = new Label();
            i_Label.Text     = s_Label;
            i_Label.AutoSize = true;
            i_Label.Location = new Point(12, Y + 3);
            Controls.Add(i_Label);

            TextBox i_Text = new TextBox();
            i_Text.Location = new Point(80, Y);
            i_Text.Width    = 80;
            i_Text.Text     = s_Value;
            Controls.Add(i_Text);

            Label i_Hint = new Label();
            i_Hint.Text      = s_Hint;
            i_Hint.AutoSize  = true;
            i_Hint.ForeColor = Color.Silver;
            i_Hint.Location  = new Point(166, Y + 3);
            Controls.Add(i_Hint);
            return i_Text;
        }

        /// <summary>
        /// d_Position = 0 --> the section starts at the beginning, 0.5 --> in the middle, 1 --> ends at the end of the file
        /// </summary>
        void AddQuickButton(String s_Text, int X, double d_Position)
        {
            Button i_Button = new Button();
            i_Button.Text      = s_Text;
            i_Button.ForeColor = Color.Black;
            i_Button.BackColor = SystemColors.Control;
            i_Button.Location  = new Point(X, 80);
            i_Button.Size      = new Size(70, 24);
            i_Button.Click    += delegate
            {
                double d_Total    = (double)ms64_Frames / ms32_Rate;
                double d_Duration = WavFile.ParseTime(mi_TextDuration.Text);
                if (d_Duration <= 0 || d_Duration > md_MaxSeconds)
                    d_Duration = md_MaxSeconds;
                double d_Start = Math.Max(0, (d_Total - d_Duration) * d_Position);
                mi_TextStart.Text = WavFile.FormatTime(d_Start);
            };
            Controls.Add(i_Button);
        }

        /// <summary>
        /// Calculates the section from the input. returns null if valid, otherwise the error.
        /// </summary>
        String CalcSection(out long s64_Start, out int s32_Count)
        {
            s64_Start = 0;
            s32_Count = 0;
            double d_Start    = WavFile.ParseTime(mi_TextStart.Text);
            double d_Duration = WavFile.ParseTime(mi_TextDuration.Text);
            if (d_Start < 0)
                return "Invalid start time";
            if (d_Duration <= 0)
                return "Invalid duration";

            s64_Start = (long)Math.Round(d_Start * ms32_Rate);
            if (s64_Start >= ms64_Frames - Utils.MIN_VALID_SAMPLES)
                return "The start is behind the end of the file";

            long s64_Count = (long)Math.Round(d_Duration * ms32_Rate);
            s64_Count = Math.Min(s64_Count, ms64_Frames - s64_Start); // until the end of the file
            if (s64_Count > WavFile.MAX_SAMPLES)
                return "The duration is longer than " + WavFile.FormatTime(md_MaxSeconds);
            if (s64_Count < Utils.MIN_VALID_SAMPLES)
                return "The section is too short";

            s32_Count = (int)s64_Count;
            return null;
        }

        void UpdateSection()
        {
            long s64_Start;
            int  s32_Count;
            String s_Error = CalcSection(out s64_Start, out s32_Count);
            if (s_Error != null)
            {
                mi_LblSection.Text      = s_Error;
                mi_LblSection.ForeColor = Color.FromArgb(0xFF, 0xA0, 0x80);
                mi_BtnLoad.Enabled      = false;
                return;
            }
            mi_LblSection.Text = String.Format(CultureInfo.InvariantCulture, "Section: {0} ... {1}   ({2:N0} samples per channel)",
                                               WavFile.FormatTime((double)s64_Start / ms32_Rate),
                                               WavFile.FormatTime((double)(s64_Start + s32_Count) / ms32_Rate), s32_Count);
            mi_LblSection.ForeColor = Color.LightGreen;
            mi_BtnLoad.Enabled      = true;
        }

        void OnLoadClick(object sender, EventArgs e)
        {
            long s64_Start;
            int  s32_Count;
            if (CalcSection(out s64_Start, out s32_Count) != null)
                return;

            StartFrame   = s64_Start;
            FrameCount   = s32_Count;
            DialogResult = DialogResult.OK;
        }
    }
}
