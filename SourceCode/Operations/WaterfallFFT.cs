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
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using Utils             = OsziWaveformAnalyzer.Utils;
using Capture           = OsziWaveformAnalyzer.Utils.Capture;
using PlatformManager   = Platform.PlatformManager;

namespace Operations
{
    /// <summary>
    /// Waterfall FFT: an automatic loop acquires one channel from the oscilloscope, calculates its spectrum
    /// and adds it as a new row to the waterfall, until the user clicks "Cancel".
    /// The newest spectrum is at the top. The color shows the level in dBV.
    /// The waterfall can be exported as PNG image and the collected spectra as CSV matrix.
    ///
    /// This window does not know the oscilloscope. The caller passes a function that acquires one channel.
    /// </summary>
    public class WaterfallFFT : Form
    {
        /// <summary>
        /// Acquires one channel from the oscilloscope. Returns null if the user has aborted. Throws on error.
        /// </summary>
        public delegate Capture delAcquire(int s32_Channel);

        class Row
        {
            public DateTime mi_Time;
            public float[]  mf_Amp;  // Volt peak per frequency bin
        }

        const String MAX_FREQ_AUTO = "Nyquist";
        const String SCALE_DB     = "dBV (RMS)";
        const String SCALE_LINEAR = "Volt (peak)";

        delAcquire   mf_Acquire;
        Action       mf_Abort;
        String       ms_Source;
        List<Row>    mi_Rows = new List<Row>(); // newest first
        double       md_Rate;
        int          ms32_FftSize;
        int          ms32_Samples;
        bool         mb_Running;
        bool         mb_Cancel;
        bool         mb_CloseAfterLoop;

        ComboBox      mi_ComboChannel;
        ComboBox      mi_ComboWindow;
        ComboBox      mi_ComboScale;
        ComboBox      mi_ComboRange;
        ComboBox      mi_ComboRows;
        ComboBox      mi_ComboMaxFreq;
        Button        mi_BtnStart;
        Label         mi_LblInfo;
        SplitContainer mi_Split;
        SpectrumView  mi_Spectrum;
        WaterfallView mi_Waterfall;

        /// <summary>
        /// s_Source = name of the oscilloscope for the window title and the CSV header
        /// f_Abort  = aborts a running transfer (called when the user clicks "Cancel")
        /// </summary>
        public WaterfallFFT(String s_Source, delAcquire f_Acquire, Action f_Abort)
        {
            ms_Source  = s_Source;
            mf_Acquire = f_Acquire;
            mf_Abort   = f_Abort;
            CreateControls();
        }

        void CreateControls()
        {
            Text          = "Waterfall FFT  —  " + ms_Source;
            Icon          = Utils.FormMain != null ? Utils.FormMain.Icon : null;
            BackColor     = Color.DimGray;
            ForeColor     = Color.White;
            ClientSize    = new Size(1000, 720);
            MinimumSize   = new Size(700, 500);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;

            FlowLayoutPanel i_Bar = new FlowLayoutPanel();
            i_Bar.Dock         = DockStyle.Top;
            i_Bar.AutoSize     = true;
            i_Bar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            i_Bar.MinimumSize  = new Size(0, 32);
            i_Bar.Padding      = new Padding(4, 4, 4, 0);

            mi_ComboChannel = AddCombo(i_Bar, "Channel:", 55);
            mi_ComboChannel.Items.AddRange(new Object[] { "CH1", "CH2" });
            mi_ComboChannel.SelectedIndex = 0;

            mi_ComboWindow = AddCombo(i_Bar, "Window:", 140);
            foreach (Fourier.eWindow e_Win in Enum.GetValues(typeof(Fourier.eWindow)))
                mi_ComboWindow.Items.Add(Utils.GetDescriptionAttribute(e_Win));
            mi_ComboWindow.SelectedIndex = 0;

            // Audio: show only the low frequencies. (The highest frequency that can be measured is half the sample rate.)
            mi_ComboMaxFreq = AddCombo(i_Bar, "Max. freq:", 75);
            mi_ComboMaxFreq.Items.AddRange(new Object[] { MAX_FREQ_AUTO, "500 Hz", "1 kHz", "2 kHz", "5 kHz", "10 kHz", "20 kHz", "50 kHz", "100 kHz", "200 kHz", "500 kHz", "1 MHz" });
            mi_ComboMaxFreq.SelectedIndex = 0;

            mi_ComboScale = AddCombo(i_Bar, "Scale:", 90);
            mi_ComboScale.Items.AddRange(new Object[] { SCALE_DB, SCALE_LINEAR });
            mi_ComboScale.SelectedIndex = 0;

            mi_ComboRange = AddCombo(i_Bar, "Colors:", 70);
            mi_ComboRange.Items.AddRange(new Object[] { "40 dB", "60 dB", "80 dB", "100 dB", "120 dB" });
            mi_ComboRange.SelectedIndex = 2;

            mi_ComboRows = AddCombo(i_Bar, "History:", 70);
            mi_ComboRows.Items.AddRange(new Object[] { "50", "100", "300", "1000" });
            mi_ComboRows.SelectedIndex = 2;

            mi_BtnStart = AddButton(i_Bar, "Start", 12);
            mi_BtnStart.BackColor = Color.PaleGreen;
            mi_BtnStart.Width     = 70;
            mi_BtnStart.AutoSize  = false;
            mi_BtnStart.Click    += new EventHandler(OnStartCancel);

            AddButton(i_Bar, "Export Image", 12).Click += new EventHandler(OnExportImage);
            AddButton(i_Bar, "Export Matrix", 4).Click += new EventHandler(OnExportMatrix);
            AddButton(i_Bar, "Clear", 4).Click += delegate { ClearRows("Cleared"); };

            LinkLabel i_Help = new LinkLabel();
            i_Help.Text      = "Show Help";
            i_Help.LinkColor = Color.Lime;
            i_Help.AutoSize  = true;
            i_Help.Margin    = new Padding(12, 5, 0, 0);
            i_Help.LinkClicked += delegate { PlatformManager.Instance.ShowHelp(this, "Waterfall"); };
            i_Bar.Controls.Add(i_Help);

            mi_LblInfo = new Label();
            mi_LblInfo.Dock      = DockStyle.Bottom;
            mi_LblInfo.Height    = 22;
            mi_LblInfo.TextAlign = ContentAlignment.MiddleLeft;
            mi_LblInfo.Padding   = new Padding(4, 0, 0, 0);
            mi_LblInfo.Text      = "Click 'Start' to acquire the selected channel repeatedly.";

            mi_Spectrum = new SpectrumView();
            mi_Spectrum.Dock       = DockStyle.Fill;
            mi_Spectrum.TraceColor = Color.Yellow;

            mi_Waterfall = new WaterfallView();
            mi_Waterfall.Dock = DockStyle.Fill;

            mi_Split = new SplitContainer();
            mi_Split.Dock          = DockStyle.Fill;
            mi_Split.Orientation   = Orientation.Horizontal;
            mi_Split.BackColor     = Color.DimGray;
            mi_Split.Panel1.Controls.Add(mi_Spectrum);
            mi_Split.Panel2.Controls.Add(mi_Waterfall);

            Controls.Add(mi_Split); // Fill must be added first
            Controls.Add(mi_LblInfo);
            Controls.Add(i_Bar);

            // Both views show the same frequency range
            mi_Spectrum.ZoomChanged  += delegate { mi_Waterfall.SetFrequencyRange(mi_Spectrum.FreqMin, mi_Spectrum.FreqMax); };
            mi_Waterfall.ZoomChanged += delegate { mi_Spectrum.SetFrequencyRange(mi_Waterfall.FreqMin, mi_Waterfall.FreqMax); };

            mi_ComboScale.SelectedIndexChanged += delegate { mi_Spectrum.ShowDb = mi_ComboScale.Text == SCALE_DB; mi_Spectrum.Invalidate(); };
            mi_ComboRange.SelectedIndexChanged += delegate { UpdateWaterfall(); };
            mi_ComboMaxFreq.SelectedIndexChanged += delegate { ApplyMaxFrequency(); };
            mi_ComboRows .SelectedIndexChanged += delegate { TrimRows(); UpdateWaterfall(); };
            mi_ComboWindow .SelectedIndexChanged += delegate { ClearRows("Window function changed"); };
            mi_ComboChannel.SelectedIndexChanged += delegate { ClearRows("Channel changed"); };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            mi_Split.SplitterDistance = ClientSize.Height / 3;
        }

        static ComboBox AddCombo(FlowLayoutPanel i_Bar, String s_Label, int s32_Width)
        {
            Label i_Label = new Label();
            i_Label.Text     = s_Label;
            i_Label.AutoSize = true;
            i_Label.Margin   = new Padding(6, 5, 0, 0);
            i_Bar.Controls.Add(i_Label);

            ComboBox i_Combo = new ComboBox();
            i_Combo.DropDownStyle = ComboBoxStyle.DropDownList;
            i_Combo.Width         = s32_Width;
            i_Bar.Controls.Add(i_Combo);
            return i_Combo;
        }

        static Button AddButton(FlowLayoutPanel i_Bar, String s_Text, int s32_MarginLeft)
        {
            Button i_Button = new Button();
            i_Button.Text      = s_Text;
            i_Button.ForeColor = Color.Black;
            i_Button.BackColor = SystemColors.Control;
            i_Button.AutoSize  = true;
            i_Button.Margin    = new Padding(s32_MarginLeft, 0, 0, 0);
            i_Bar.Controls.Add(i_Button);
            return i_Button;
        }

        /// <summary>
        /// "20 kHz" --> 20000, "Nyquist" --> 0
        /// </summary>
        double SelectedMaxFrequency
        {
            get
            {
                String[] s_Parts = mi_ComboMaxFreq.Text.Split(' ');
                if (s_Parts.Length != 2)
                    return 0;
                double d_Value = double.Parse(s_Parts[0], CultureInfo.InvariantCulture);
                switch (s_Parts[1])
                {
                    case "kHz": return d_Value * 1e3;
                    case "MHz": return d_Value * 1e6;
                    default:    return d_Value;
                }
            }
        }

        /// <summary>
        /// Displays the frequencies from 0 to the selected maximum in both views (this is a zoom, the FFT is not changed)
        /// </summary>
        void ApplyMaxFrequency()
        {
            double d_Max = SelectedMaxFrequency;
            mi_Spectrum .MaxFrequency = d_Max;
            mi_Waterfall.MaxFrequency = d_Max;
            if (md_Rate <= 0)
                return;

            double d_Top = (d_Max > 0) ? Math.Min(d_Max, md_Rate / 2) : md_Rate / 2;
            mi_Spectrum .SetFrequencyRange(0, d_Top);
            mi_Waterfall.SetFrequencyRange(0, d_Top);
        }

        int MaxRows
        {
            get { return int.Parse(mi_ComboRows.Text); }
        }

        // =====================================================================================================
        //                                          Loop
        // =====================================================================================================

        void OnStartCancel(object sender, EventArgs e)
        {
            if (mb_Running)
            {
                mb_Cancel = true;
                mi_BtnStart.Enabled = false; // until the loop has finished
                mf_Abort();                  // abort a running transfer
                return;
            }
            RunLoop();
        }

        /// <summary>
        /// Acquire --> FFT --> add row --> repeat until "Cancel".
        /// The acquisition runs in the GUI thread and calls Application.DoEvents(), so the GUI stays responsive.
        /// </summary>
        void RunLoop()
        {
            mb_Running = true;
            mb_Cancel  = false;
            mi_BtnStart.Text       = "Cancel";
            mi_BtnStart.BackColor  = Color.Salmon;
            mi_ComboChannel.Enabled = false;

            int s32_Channel  = mi_ComboChannel.SelectedIndex + 1;
            int s32_Steps    = 0;
            int s32_Timeouts = 0; // consecutive
            int s32_Retries  = 0; // total
            String s_Error   = null;
            try
            {
                while (!mb_Cancel)
                {
                    DateTime t_Start = DateTime.Now;
                    PrintInfo("Acquiring CH" + s32_Channel + " ...", Color.White);

                    Capture i_Capture;
                    try
                    {
                        i_Capture = mf_Acquire(s32_Channel);
                    }
                    catch (TimeoutException)
                    {
                        // A single lost response must not stop a loop that runs for minutes. Repeat the step.
                        // (Before each command the SCPI class discards a late response.)
                        if (++s32_Timeouts >= 3)
                            throw;
                        s32_Retries ++;
                        continue;
                    }
                    if (i_Capture == null || mb_Cancel)
                        break; // aborted

                    s32_Timeouts = 0;
                    AddCapture(i_Capture);
                    s32_Steps ++;

                    String s_Info = String.Format("Step {0}   {1}   Duration: {2:F1} s   Rows: {3}   Samples: {4:N0}   FFT size: {5:N0}   Sample rate: {6}   Nyquist: {7}   Resolution: {8}",
                                                  s32_Steps, DateTime.Now.ToString("HH:mm:ss"), (DateTime.Now - t_Start).TotalSeconds,
                                                  mi_Rows.Count, ms32_Samples, ms32_FftSize, SpectrumFFT.FormatFreq(md_Rate),
                                                  SpectrumFFT.FormatFreq(md_Rate / 2), SpectrumFFT.FormatFreq(md_Rate / ms32_Samples));
                    if (s32_Retries > 0)
                        s_Info += "   Repeated after timeout: " + s32_Retries;
                    PrintInfo(s_Info, Color.White);
                    Application.DoEvents();
                }
            }
            catch (Exception Ex)
            {
                s_Error = Ex.Message;
            }

            mb_Running = false;
            mi_BtnStart.Text        = "Start";
            mi_BtnStart.BackColor   = Color.PaleGreen;
            mi_BtnStart.Enabled     = true;
            mi_ComboChannel.Enabled = true;

            if (s_Error != null)
            {
                PrintInfo("Error: " + s_Error.Replace("\n", " "), Color.FromArgb(0xFF, 0xA0, 0x80));
                if (!mb_CloseAfterLoop)
                    MessageBox.Show(this, s_Error, "Waterfall FFT", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (mb_Cancel)
            {
                PrintInfo("Stopped after " + s32_Steps + " steps. " + mi_Rows.Count + " spectra can be exported.", Color.White);
            }

            if (mb_CloseAfterLoop)
                Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // The loop must finish first, otherwise the transfer would continue with a closed window
            if (mb_Running)
            {
                e.Cancel = true;
                mb_CloseAfterLoop = true;
                mb_Cancel = true;
                mf_Abort();
                return;
            }
            base.OnFormClosing(e);
        }

        void PrintInfo(String s_Text, Color c_Color)
        {
            mi_LblInfo.Text      = s_Text;
            mi_LblInfo.ForeColor = c_Color;
        }

        // =====================================================================================================
        //                                         Spectra
        // =====================================================================================================

        /// <summary>
        /// Calculates the spectrum of the first analog channel and adds it as the newest row.
        /// If the sample rate or the count of samples has changed (other timebase) the waterfall is restarted,
        /// because all rows must have the same frequency axis.
        /// </summary>
        public void AddCapture(Capture i_Capture)
        {
            float[] f_Samples = null;
            foreach (Utils.Channel i_Chan in i_Capture.mi_Channels)
            {
                if (i_Chan.mf_Analog != null)
                {
                    f_Samples = i_Chan.mf_Analog;
                    break;
                }
            }
            if (f_Samples == null)
                throw new Exception("The oscilloscope has not sent analog data.");

            int    s32_Count = Math.Min(i_Capture.ms32_Samples, Fourier.MAX_FFT_SIZE);
            double d_Rate    = (double)(Utils.PICOS_PER_SECOND / i_Capture.ms64_SampleDist);

            if (mi_Rows.Count > 0 && (d_Rate != md_Rate || s32_Count != ms32_Samples))
                ClearRows("The timebase of the oscilloscope has changed --> the waterfall has been restarted");

            Fourier.eWindow e_Window = (Fourier.eWindow)mi_ComboWindow.SelectedIndex;
            int s32_FftSize;
            double[] d_Amp = Fourier.AmplitudeSpectrum(f_Samples, 0, s32_Count, e_Window, true, out s32_FftSize);

            md_Rate      = d_Rate;
            ms32_Samples = s32_Count;
            ms32_FftSize = s32_FftSize;

            Row i_Row = new Row();
            i_Row.mi_Time = DateTime.Now;
            i_Row.mf_Amp  = new float[d_Amp.Length];
            for (int k=0; k<d_Amp.Length; k++)
                i_Row.mf_Amp[k] = (float)d_Amp[k];

            mi_Rows.Insert(0, i_Row);
            TrimRows();

            // The newest spectrum with its peaks in the upper view
            double d_BinWidth = d_Rate / s32_FftSize;
            int s32_MinDist = Math.Max(3, (int)(3.0 * s32_FftSize / s32_Count));
            double[] d_Refine = null;
            if (e_Window != Fourier.eWindow.BlackmanHarris)
            {
                int s32_Dummy;
                d_Refine = Fourier.AmplitudeSpectrum(f_Samples, 0, s32_Count, Fourier.eWindow.BlackmanHarris, true, out s32_Dummy);
            }
            mi_Spectrum.ShowDb = mi_ComboScale.Text == SCALE_DB;
            mi_Spectrum.Peaks  = FindPeaksBelow(Fourier.FindPeaks(d_Amp, d_Refine, d_BinWidth, 20, s32_MinDist), 5);
            mi_Spectrum.SetSpectrum(d_Amp, d_BinWidth, d_Rate);

            UpdateWaterfall();
            mi_Waterfall.SetFrequencyRange(mi_Spectrum.FreqMin, mi_Spectrum.FreqMax);
        }

        /// <summary>
        /// Only the peaks in the displayed frequency range (Max. freq) are of interest
        /// </summary>
        List<Fourier.Peak> FindPeaksBelow(List<Fourier.Peak> i_All, int s32_Max)
        {
            double d_Max = SelectedMaxFrequency;
            List<Fourier.Peak> i_Peaks = new List<Fourier.Peak>();
            foreach (Fourier.Peak i_Peak in i_All)
            {
                if (i_Peaks.Count < s32_Max && (d_Max <= 0 || i_Peak.md_Frequency <= d_Max))
                    i_Peaks.Add(i_Peak);
            }
            return i_Peaks;
        }

        void TrimRows()
        {
            while (mi_Rows.Count > MaxRows)
                mi_Rows.RemoveAt(mi_Rows.Count - 1);
        }

        void ClearRows(String s_Reason)
        {
            mi_Rows.Clear();
            UpdateWaterfall();
            if (s_Reason != null)
                PrintInfo(s_Reason, Color.FromArgb(0xFF, 0xD0, 0x80));
        }

        void UpdateWaterfall()
        {
            List<float[]>  i_Amps  = new List<float[]>();
            List<DateTime> i_Times = new List<DateTime>();
            foreach (Row i_Row in mi_Rows)
            {
                i_Amps .Add(i_Row.mf_Amp);
                i_Times.Add(i_Row.mi_Time);
            }
            double d_BinWidth = ms32_FftSize > 0 ? md_Rate / ms32_FftSize : 0;
            mi_Waterfall.SetData(i_Amps, i_Times, d_BinWidth, md_Rate, MaxRows, int.Parse(mi_ComboRange.Text.Split(' ')[0]));
        }

        // =====================================================================================================
        //                                          Export
        // =====================================================================================================

        /// <summary>
        /// Saves both views (newest spectrum and waterfall) as PNG image
        /// </summary>
        void OnExportImage(object sender, EventArgs e)
        {
            if (mi_Rows.Count == 0)
            {
                MessageBox.Show(this, "There are no spectra yet. Click 'Start' first.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog i_Dlg = new SaveFileDialog();
            i_Dlg.Filter           = "PNG image (*.png)|*.png";
            i_Dlg.FileName         = "Waterfall " + DateTime.Now.ToString("yyyy-MM-dd HH-mm") + ".png";
            i_Dlg.InitialDirectory = Utils.SampleDir;
            if (i_Dlg.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                using (Bitmap i_Bmp = new Bitmap(mi_Split.Width, mi_Split.Height))
                {
                    mi_Split.DrawToBitmap(i_Bmp, new Rectangle(0, 0, mi_Split.Width, mi_Split.Height));
                    i_Bmp.Save(i_Dlg.FileName, ImageFormat.Png);
                }
                PrintInfo("Saved image " + i_Dlg.FileName, Color.LightGreen);
            }
            catch (Exception Ex)
            {
                Utils.ShowExceptionBox(this, Ex);
            }
        }

        /// <summary>
        /// Saves the collected spectra as CSV matrix: one row per spectrum (oldest first), one column per frequency.
        /// The unit depends on the Scale (dBV RMS or Volt peak).
        /// </summary>
        void OnExportMatrix(object sender, EventArgs e)
        {
            if (mi_Rows.Count == 0)
            {
                MessageBox.Show(this, "There are no spectra yet. Click 'Start' first.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog i_Dlg = new SaveFileDialog();
            i_Dlg.Filter           = "CSV file (*.csv)|*.csv";
            i_Dlg.FileName         = "Waterfall " + DateTime.Now.ToString("yyyy-MM-dd HH-mm") + ".csv";
            i_Dlg.InitialDirectory = Utils.SampleDir;
            if (i_Dlg.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                int s32_Rows = ExportMatrix(i_Dlg.FileName);
                PrintInfo(String.Format("Saved matrix with {0} spectra x {1} frequencies to {2}", s32_Rows, mi_Rows[0].mf_Amp.Length, i_Dlg.FileName), Color.LightGreen);
            }
            catch (Exception Ex)
            {
                Utils.ShowExceptionBox(this, Ex);
            }
        }

        /// <summary>
        /// returns the count of spectra written
        /// </summary>
        public int ExportMatrix(String s_Path)
        {
            bool   b_Db       = mi_ComboScale.Text == SCALE_DB;
            double d_BinWidth = md_Rate / ms32_FftSize;
            int    s32_Bins   = mi_Rows[0].mf_Amp.Length;
            CultureInfo i_Inv = CultureInfo.InvariantCulture;

            using (StreamWriter i_Writer = new StreamWriter(s_Path, false, Encoding.UTF8))
            {
                i_Writer.WriteLine("# Waterfall FFT,{0},CH{1}", ms_Source.Replace(',', ' '), mi_ComboChannel.SelectedIndex + 1);
                i_Writer.WriteLine("# Window,{0}", mi_ComboWindow.Text);
                i_Writer.WriteLine(String.Format(i_Inv, "# Sample rate [Hz],{0:G10},Samples,{1},FFT size,{2},Bin width [Hz],{3:G10}",
                                                 md_Rate, ms32_Samples, ms32_FftSize, d_BinWidth));
                i_Writer.WriteLine("# Unit,{0}", b_Db ? "dBV (RMS)" : "Volt (peak)");
                i_Writer.WriteLine("# Rows = spectra (oldest first), columns = frequencies [Hz]");

                StringBuilder i_Line = new StringBuilder("Time,Elapsed [s]");
                for (int k=0; k<s32_Bins; k++)
                    i_Line.Append(',').Append((k * d_BinWidth).ToString("G10", i_Inv));
                i_Writer.WriteLine(i_Line.ToString());

                DateTime t_First = mi_Rows[mi_Rows.Count - 1].mi_Time;
                for (int R=mi_Rows.Count - 1; R>=0; R--) // oldest first
                {
                    Row i_Row = mi_Rows[R];
                    i_Line.Length = 0;
                    i_Line.Append(i_Row.mi_Time.ToString("yyyy-MM-dd HH:mm:ss.fff", i_Inv));
                    i_Line.Append(',').Append((i_Row.mi_Time - t_First).TotalSeconds.ToString("F3", i_Inv));
                    for (int k=0; k<s32_Bins; k++)
                    {
                        i_Line.Append(',');
                        if (b_Db) i_Line.Append(Fourier.ToDbV(i_Row.mf_Amp[k]).ToString("F2", i_Inv));
                        else      i_Line.Append(i_Row.mf_Amp[k].ToString("G6", i_Inv));
                    }
                    i_Writer.WriteLine(i_Line.ToString());
                }
            }
            return mi_Rows.Count;
        }
    }

    // ==================================================================================================================

    /// <summary>
    /// Draws the waterfall: frequency horizontal, time vertical (newest at the top), level as color.
    /// Drag with the left mouse button to zoom the frequency range, double-click to reset.
    /// </summary>
    public class WaterfallView : Panel
    {
        const int MARGIN_LEFT   = 70;
        const int MARGIN_RIGHT  = 70; // color bar
        const int MARGIN_TOP    = 8;
        const int MARGIN_BOTTOM = 28;

        List<float[]>  mi_Amps  = new List<float[]>();
        List<DateTime> mi_Times = new List<DateTime>();
        double  md_BinWidth;
        double  md_Rate;
        double  md_FreqMin, md_FreqMax;
        int     ms32_MaxRows = 300;
        double  md_DbRange   = 80;
        double  md_DbTop;
        Bitmap  mi_Bitmap;
        Point   mk_Mouse   = new Point(-1, -1);
        int     ms32_DragX = -1;
        static  int[] ms32_ColorMap = CreateColorMap();

        public event EventHandler ZoomChanged;
        public double MaxFrequency = 0; // displayed without zoom, 0 = Nyquist
        public double FreqMin { get { return md_FreqMin; } }
        public double FreqMax { get { return md_FreqMax; } }

        public WaterfallView()
        {
            DoubleBuffered = true;
            BackColor      = Color.Black;
        }

        /// <summary>
        /// i_Amps = spectra in Volt peak, newest first
        /// </summary>
        public void SetData(List<float[]> i_Amps, List<DateTime> i_Times, double d_BinWidth, double d_Rate, int s32_MaxRows, double d_DbRange)
        {
            bool b_KeepZoom = md_Rate == d_Rate && md_FreqMax > md_FreqMin;
            mi_Amps      = i_Amps;
            mi_Times     = i_Times;
            md_BinWidth  = d_BinWidth;
            md_Rate      = d_Rate;
            ms32_MaxRows = s32_MaxRows;
            md_DbRange   = d_DbRange;
            if (!b_KeepZoom)
            {
                md_FreqMin = 0;
                md_FreqMax = FullRangeMax(d_Rate);
            }
            Render();
        }

        double FullRangeMax(double d_Rate)
        {
            return (MaxFrequency > 0) ? Math.Min(MaxFrequency, d_Rate / 2) : d_Rate / 2;
        }

        public void SetFrequencyRange(double d_Min, double d_Max)
        {
            if (d_Max <= d_Min || (d_Min == md_FreqMin && d_Max == md_FreqMax))
                return;
            md_FreqMin = d_Min;
            md_FreqMax = d_Max;
            Render();
        }

        Rectangle PlotRect
        {
            get { return new Rectangle(MARGIN_LEFT, MARGIN_TOP, Math.Max(10, Width - MARGIN_LEFT - MARGIN_RIGHT), Math.Max(10, Height - MARGIN_TOP - MARGIN_BOTTOM)); }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Render();
        }

        /// <summary>
        /// Black - blue - magenta - red - orange - yellow - white (like a thermal camera)
        /// </summary>
        static int[] CreateColorMap()
        {
            Color[] c_Steps = { Color.FromArgb(0, 0, 0), Color.FromArgb(0, 0, 140), Color.FromArgb(150, 0, 170),
                                Color.FromArgb(230, 30, 40), Color.FromArgb(255, 140, 0), Color.FromArgb(255, 240, 0), Color.FromArgb(255, 255, 255) };
            int[] s32_Map = new int[256];
            for (int i=0; i<256; i++)
            {
                double d_Pos = i / 255.0 * (c_Steps.Length - 1);
                int    s32_A = Math.Min((int)d_Pos, c_Steps.Length - 2);
                double d_F   = d_Pos - s32_A;
                Color  c1 = c_Steps[s32_A], c2 = c_Steps[s32_A + 1];
                s32_Map[i] = Color.FromArgb(0xFF, (int)(c1.R + (c2.R - c1.R) * d_F), (int)(c1.G + (c2.G - c1.G) * d_F), (int)(c1.B + (c2.B - c1.B) * d_F)).ToArgb();
            }
            return s32_Map;
        }

        double XToFreq(int X, Rectangle r_Plot)
        {
            return md_FreqMin + (double)(X - r_Plot.Left) / r_Plot.Width * (md_FreqMax - md_FreqMin);
        }

        /// <summary>
        /// One bitmap row per spectrum, the maximum of all bins in each pixel column. It is stretched to the plot height.
        /// </summary>
        void Render()
        {
            if (mi_Bitmap != null)
            {
                mi_Bitmap.Dispose();
                mi_Bitmap = null;
            }

            Rectangle r_Plot = PlotRect;
            if (mi_Amps.Count == 0 || md_BinWidth <= 0 || r_Plot.Width < 10)
            {
                Invalidate();
                return;
            }

            // The top of the color scale follows the strongest signal of all rows (rounded up to 10 dB)
            double d_Max = 1e-12;
            foreach (float[] f_Amp in mi_Amps)
                foreach (float f in f_Amp)
                    d_Max = Math.Max(d_Max, f);
            md_DbTop = Math.Ceiling(Fourier.ToDbV(d_Max) / 10) * 10;

            int W = r_Plot.Width;
            int H = ms32_MaxRows;
            int[] s32_Pixels = new int[W * H]; // rows without data stay transparent

            // Bin range for each pixel column
            int[] s32_Bin0 = new int[W];
            int[] s32_Bin1 = new int[W];
            int s32_Bins = mi_Amps[0].Length;
            for (int X=0; X<W; X++)
            {
                double d_F0 = md_FreqMin + (double)X       / W * (md_FreqMax - md_FreqMin);
                double d_F1 = md_FreqMin + (double)(X + 1) / W * (md_FreqMax - md_FreqMin);
                s32_Bin0[X] = Math.Max(0, Math.Min(s32_Bins - 1, (int)Math.Floor(d_F0 / md_BinWidth)));
                s32_Bin1[X] = Math.Max(s32_Bin0[X], Math.Min(s32_Bins - 1, (int)Math.Ceiling(d_F1 / md_BinWidth) - 1));
            }

            for (int R=0; R<mi_Amps.Count && R<H; R++)
            {
                float[] f_Amp = mi_Amps[R];
                for (int X=0; X<W; X++)
                {
                    float f_Max = 0;
                    for (int k=s32_Bin0[X]; k<=s32_Bin1[X]; k++)
                        f_Max = Math.Max(f_Max, f_Amp[k]);

                    double d_Rel = (Fourier.ToDbV(f_Max) - (md_DbTop - md_DbRange)) / md_DbRange;
                    int s32_Idx = (int)(Math.Max(0, Math.Min(1, d_Rel)) * 255);
                    s32_Pixels[R * W + X] = ms32_ColorMap[s32_Idx];
                }
            }

            mi_Bitmap = new Bitmap(W, H, PixelFormat.Format32bppArgb);
            BitmapData i_Data = mi_Bitmap.LockBits(new Rectangle(0, 0, W, H), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            for (int Y=0; Y<H; Y++)
                Marshal.Copy(s32_Pixels, Y * W, new IntPtr(i_Data.Scan0.ToInt64() + (long)Y * i_Data.Stride), W);
            mi_Bitmap.UnlockBits(i_Data);
            Invalidate();
        }

        static double NiceStep(double d_Range, int s32_Count)
        {
            double d_Raw  = d_Range / Math.Max(1, s32_Count);
            double d_Pow  = Math.Pow(10, Math.Floor(Math.Log10(d_Raw)));
            double d_Norm = d_Raw / d_Pow;
            if (d_Norm < 1.5) return d_Pow;
            if (d_Norm < 3.5) return d_Pow * 2;
            if (d_Norm < 7.5) return d_Pow * 5;
            return d_Pow * 10;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics  g      = e.Graphics;
            Rectangle r_Plot = PlotRect;

            using (Pen   i_Frame = new Pen(Color.FromArgb(0x90, 0x90, 0x90)))
            using (Brush i_Text  = new SolidBrush(Color.FromArgb(0xC0, 0xC0, 0xC0)))
            using (Font  i_Font  = new Font("Segoe UI", 8))
            {
                if (mi_Bitmap == null)
                {
                    g.DrawRectangle(i_Frame, r_Plot);
                    g.DrawString("No spectra yet", i_Font, i_Text, r_Plot.Left + 6, r_Plot.Top + 6);
                    return;
                }

                // ---- waterfall: each spectrum is one band of the plot height ----
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode   = PixelOffsetMode.Half;
                g.DrawImage(mi_Bitmap, r_Plot);
                g.PixelOffsetMode   = PixelOffsetMode.Default;
                g.DrawRectangle(i_Frame, r_Plot);

                // ---- frequency axis ----
                double d_StepX = NiceStep(md_FreqMax - md_FreqMin, Math.Max(2, r_Plot.Width / 90));
                for (long n = (long)Math.Ceiling(md_FreqMin / d_StepX); n * d_StepX <= md_FreqMax; n++)
                {
                    double d_F = n * d_StepX;
                    float X = (float)(r_Plot.Left + (d_F - md_FreqMin) / (md_FreqMax - md_FreqMin) * r_Plot.Width);
                    g.DrawLine(i_Frame, X, r_Plot.Bottom, X, r_Plot.Bottom + 4);
                    String s_Label = SpectrumFFT.FormatFreq(d_F);
                    SizeF  k_Size  = g.MeasureString(s_Label, i_Font);
                    float  f_Left  = Math.Min(X - k_Size.Width / 2, r_Plot.Right - k_Size.Width + 4); // do not overlap "dBV"
                    g.DrawString(s_Label, i_Font, i_Text, f_Left, r_Plot.Bottom + 5);
                }

                // ---- time axis: age of the rows relative to the newest ----
                float f_RowH = (float)r_Plot.Height / ms32_MaxRows;
                int s32_LabelEvery = Math.Max(1, (int)Math.Ceiling(40 / f_RowH)); // a label every 40 pixels
                for (int R=0; R<mi_Times.Count; R+=s32_LabelEvery)
                {
                    float Y = r_Plot.Top + R * f_RowH;
                    double d_Age = (mi_Times[R] - mi_Times[0]).TotalSeconds;
                    String s_Label = d_Age.ToString("0.0", CultureInfo.InvariantCulture) + " s";
                    SizeF  k_Size  = g.MeasureString(s_Label, i_Font);
                    g.DrawLine(i_Frame, r_Plot.Left - 4, Y, r_Plot.Left, Y);
                    g.DrawString(s_Label, i_Font, i_Text, r_Plot.Left - k_Size.Width - 5, Y - k_Size.Height / 2);
                }

                // ---- color bar ----
                Rectangle r_Bar = new Rectangle(r_Plot.Right + 12, r_Plot.Top, 14, r_Plot.Height);
                for (int Y=0; Y<r_Bar.Height; Y++)
                {
                    int s32_Idx = 255 - Y * 255 / Math.Max(1, r_Bar.Height - 1);
                    using (Pen i_Pen = new Pen(Color.FromArgb(ms32_ColorMap[s32_Idx])))
                        g.DrawLine(i_Pen, r_Bar.Left, r_Bar.Top + Y, r_Bar.Right, r_Bar.Top + Y);
                }
                g.DrawRectangle(i_Frame, r_Bar);
                for (int i=0; i<=4; i++)
                {
                    double d_Db = md_DbTop - md_DbRange * i / 4;
                    float  Y    = r_Bar.Top + (float)r_Bar.Height * i / 4;
                    String s_Label = d_Db.ToString("0", CultureInfo.InvariantCulture);
                    g.DrawString(s_Label, i_Font, i_Text, r_Bar.Right + 3, Y - 7);
                }
                g.DrawString("dBV", i_Font, i_Text, r_Bar.Left - 2, r_Bar.Bottom + 5);

                // ---- zoom rectangle and mouse readout ----
                if (ms32_DragX >= 0 && mk_Mouse.X >= 0)
                {
                    using (Brush i_Zoom = new SolidBrush(Color.FromArgb(0x40, 0x80, 0x80, 0xFF)))
                        g.FillRectangle(i_Zoom, Math.Min(ms32_DragX, mk_Mouse.X), r_Plot.Top, Math.Abs(mk_Mouse.X - ms32_DragX), r_Plot.Height);
                }
                else if (r_Plot.Contains(mk_Mouse))
                {
                    int R = (int)((mk_Mouse.Y - r_Plot.Top) / f_RowH);
                    if (R < mi_Amps.Count)
                    {
                        double d_F = XToFreq(mk_Mouse.X, r_Plot);
                        int    k   = Math.Max(0, Math.Min(mi_Amps[R].Length - 1, (int)Math.Round(d_F / md_BinWidth)));
                        String s_Read = String.Format(CultureInfo.InvariantCulture, "{0}   {1}   {2:0.0} dBV",
                                                      SpectrumFFT.FormatFreq(k * md_BinWidth), mi_Times[R].ToString("HH:mm:ss"),
                                                      Fourier.ToDbV(mi_Amps[R][k]));
                        SizeF k_Size = g.MeasureString(s_Read, i_Font);
                        using (Brush i_Back = new SolidBrush(Color.FromArgb(0xC0, 0, 0, 0)))
                            g.FillRectangle(i_Back, r_Plot.Left + 3, r_Plot.Top + 3, k_Size.Width + 4, k_Size.Height);
                        g.DrawString(s_Read, i_Font, Brushes.White, r_Plot.Left + 5, r_Plot.Top + 3);
                    }
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            mk_Mouse = e.Location;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            mk_Mouse = new Point(-1, -1);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && PlotRect.Contains(e.Location))
                ms32_DragX = e.X;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (ms32_DragX >= 0 && Math.Abs(e.X - ms32_DragX) > 5 && md_BinWidth > 0)
            {
                Rectangle r_Plot = PlotRect;
                double d_F1 = XToFreq(Math.Max(r_Plot.Left,  Math.Min(ms32_DragX, e.X)), r_Plot);
                double d_F2 = XToFreq(Math.Min(r_Plot.Right, Math.Max(ms32_DragX, e.X)), r_Plot);
                if (d_F2 - d_F1 > md_BinWidth * 4)
                {
                    md_FreqMin = d_F1;
                    md_FreqMax = d_F2;
                    Render();
                    if (ZoomChanged != null)
                        ZoomChanged(this, EventArgs.Empty);
                }
            }
            ms32_DragX = -1;
            Invalidate();
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            md_FreqMin = 0;
            md_FreqMax = FullRangeMax(md_Rate);
            Render();
            if (ZoomChanged != null)
                ZoomChanged(this, EventArgs.Empty);
        }

        protected override void Dispose(bool b_Disposing)
        {
            if (b_Disposing && mi_Bitmap != null)
            {
                mi_Bitmap.Dispose();
                mi_Bitmap = null;
            }
            base.Dispose(b_Disposing);
        }
    }
}
