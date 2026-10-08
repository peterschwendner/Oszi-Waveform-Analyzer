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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Fourier           = Operations.Fourier;
using SpectrumFFT       = Operations.SpectrumFFT;
using Capture           = OsziWaveformAnalyzer.Utils.Capture;
using Channel           = OsziWaveformAnalyzer.Utils.Channel;
using Utils             = OsziWaveformAnalyzer.Utils;
using PlatformManager   = Platform.PlatformManager;

namespace Transfer
{
    /// <summary>
    /// The source of the Live X/Y display: an audio input or a capture that is played (Operations.CaptureStream)
    /// </summary>
    public interface IXYStream
    {
        String  Name             { get; } // window title
        int     SampleRate       { get; }
        double  SamplesPerSecond { get; } // SampleRate * playback speed
        long    SkippedSamples   { get; }
        String  StatusInfo       { get; } // e.g. the position in a file, or null
        bool    CanSnapshot      { get; } // the last second can be stored in the main window

        // Additional settings in the toolbar (e.g. channels and speed of a capture)
        void    AddControls(FlowLayoutPanel i_Bar);
        // Throws
        void    StartStream();
        // Copies all new samples since the last call (X and Y). Skips the oldest if they do not fit. Throws.
        int     ReadStream(float[] f_X, float[] f_Y, out bool b_Skipped);
        void    StopStream();
        Capture GetLastSamples(int s32_Count);
    }

    /// <summary>
    /// The audio input as source of the Live X/Y display: Left = X, Right = Y
    /// </summary>
    public class AudioXYStream : IXYStream
    {
        AudioInput mi_Audio = new AudioInput();
        String     ms_Device;
        int        ms32_Rate;
        bool       mb_Volt;

        public AudioXYStream(String s_Device, int s32_Rate, bool b_Volt)
        {
            ms_Device = s_Device;
            ms32_Rate = s32_Rate;
            mb_Volt   = b_Volt;
        }

        public String Name             { get { return "Audio: " + ms_Device + (mb_Volt ? "  (calibrated)" : ""); } }
        public int    SampleRate       { get { return ms32_Rate; } }
        public double SamplesPerSecond { get { return ms32_Rate; } }
        public long   SkippedSamples   { get { return mi_Audio.SkippedSamples; } }
        public String StatusInfo       { get { return null; } }
        public bool   CanSnapshot      { get { return true; } }

        public void    AddControls(FlowLayoutPanel i_Bar) {}
        public void    StartStream() { mi_Audio.StartStream(ms_Device, ms32_Rate); }
        public int     ReadStream(float[] f_X, float[] f_Y, out bool b_Skipped) { return mi_Audio.ReadStream(f_X, f_Y, out b_Skipped); }
        public void    StopStream() { mi_Audio.Stop(); }
        public Capture GetLastSamples(int s32_Count) { return mi_Audio.GetLastSamples(s32_Count); }
    }

    /// <summary>
    /// Live X/Y display of an audio input like an analog oscilloscope in XY mode: Left = X, Right = Y.
    /// The beam leaves a trace on the phosphor which fades with the selected persistence.
    /// Typical use: oscilloscope music, Lissajous figures, phase of stereo signals.
    /// </summary>
    public class FormLiveXY : Form
    {
        static readonly String[] PERSISTENCE = { "10 ms", "30 ms", "100 ms", "300 ms", "1 s", "3 s" };
        static readonly String[] BRIGHTNESS  = { "0.1", "0.2", "0.5", "1", "2", "5", "10", "20", "50" };
        static readonly String[] ZOOM        = { "Auto", "1 x", "2 x", "5 x", "10 x", "20 x", "50 x", "100 x", "200 x", "500 x", "1000 x" };

        IXYStream   mi_Stream;
        int         ms32_Rate;
        double[]    md_Factor;   // full scale --> Volt (1.0 if not calibrated)
        bool        mb_Volt;
        bool        mb_Running;
        Timer       mi_Timer;
        Stopwatch   mi_Watch = new Stopwatch();
        double      md_LastTime;
        double      md_AutoPeak;
        float[]     mf_Left, mf_Right, mf_X, mf_Y;

        // statistics for the status bar
        Stopwatch   mi_InfoWatch = new Stopwatch();
        int         ms32_Frames;
        float       mf_PeakX, mf_PeakY;

        Button      mi_BtnStart;
        ComboBox    mi_ComboZoom;
        ComboBox    mi_ComboPersist;
        ComboBox    mi_ComboBright;
        ComboBox    mi_ComboMode;
        CheckBox    mi_CheckSwap;
        Label       mi_LblInfo;
        XYLiveView  mi_View;

        /// <summary>
        /// b_Volt = the samples are in Volt after multiplication with the factors
        /// d_FactorLeft/Right = Volt per full scale (calibrated audio input) or 1.0
        /// </summary>
        public FormLiveXY(IXYStream i_Stream, bool b_Volt, double d_FactorLeft, double d_FactorRight)
        {
            mi_Stream   = i_Stream;
            ms32_Rate   = i_Stream.SampleRate;
            mb_Volt     = b_Volt;
            md_Factor   = new double[] { d_FactorLeft, d_FactorRight };

            // up to 0.5 seconds per frame (at most 1 M samples), more is skipped
            int s32_Buffer = Math.Max(1024, Math.Min(ms32_Rate / 2, 1 << 20));
            mf_Left  = new float[s32_Buffer];
            mf_Right = new float[s32_Buffer];
            mf_X     = new float[s32_Buffer];
            mf_Y     = new float[s32_Buffer];

            Text          = "Live X/Y  —  " + i_Stream.Name;
            Icon          = Utils.FormMain != null ? Utils.FormMain.Icon : null;
            BackColor     = Color.DimGray;
            ForeColor     = Color.White;
            ClientSize    = new Size(760, 820);
            MinimumSize   = new Size(500, 500);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;

            FlowLayoutPanel i_Bar = new FlowLayoutPanel();
            i_Bar.Dock         = DockStyle.Top;
            i_Bar.AutoSize     = true;
            i_Bar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            i_Bar.Padding      = new Padding(4, 4, 4, 0);

            mi_BtnStart = AddButton(i_Bar, "Stop", 4);
            mi_BtnStart.Width     = 60;
            mi_BtnStart.AutoSize  = false;
            mi_BtnStart.BackColor = Color.Salmon;
            mi_BtnStart.Click    += delegate { if (mb_Running) StopStream(); else StartStream(); };

            mi_ComboZoom    = AddCombo(i_Bar, "Zoom:",        65, ZOOM,        0);
            mi_ComboPersist = AddCombo(i_Bar, "Persistence:", 65, PERSISTENCE, 2);
            mi_ComboBright  = AddCombo(i_Bar, "Brightness:",  50, BRIGHTNESS,  3);
            mi_ComboMode    = AddCombo(i_Bar, "Beam:",        60, new String[] { "Lines", "Dots" }, 0);

            mi_CheckSwap = new CheckBox();
            mi_CheckSwap.Text     = "Swap X/Y";
            mi_CheckSwap.AutoSize = true;
            mi_CheckSwap.Margin   = new Padding(10, 4, 0, 0);
            i_Bar.Controls.Add(mi_CheckSwap);

            i_Stream.AddControls(i_Bar);

            if (i_Stream.CanSnapshot)
                AddButton(i_Bar, "Snapshot 1 s", 12).Click += new EventHandler(OnSnapshot);
            AddButton(i_Bar, "Export Image", 4).Click  += new EventHandler(OnExportImage);

            LinkLabel i_Help = new LinkLabel();
            i_Help.Text      = "Show Help";
            i_Help.LinkColor = Color.Lime;
            i_Help.AutoSize  = true;
            i_Help.Margin    = new Padding(12, 5, 0, 0);
            i_Help.LinkClicked += delegate { PlatformManager.Instance.ShowHelp(this, "LiveXY"); };
            i_Bar.Controls.Add(i_Help);

            mi_LblInfo = new Label();
            mi_LblInfo.Dock      = DockStyle.Bottom;
            mi_LblInfo.Height    = 22;
            mi_LblInfo.TextAlign = ContentAlignment.MiddleLeft;
            mi_LblInfo.Padding   = new Padding(4, 0, 0, 0);

            mi_View = new XYLiveView();
            mi_View.Dock = DockStyle.Fill;
            mi_View.Unit = b_Volt ? "V" : "FS";

            Controls.Add(mi_View); // Fill must be added first
            Controls.Add(mi_LblInfo);
            Controls.Add(i_Bar);

            mi_ComboZoom.SelectedIndexChanged += delegate { md_AutoPeak = 0; mi_View.Clear(); };
            mi_ComboMode.SelectedIndexChanged += delegate { mi_View.Lines = mi_ComboMode.SelectedIndex == 0; };
            mi_CheckSwap.CheckedChanged       += delegate { mi_View.Clear(); };

            mi_Timer = new Timer();
            mi_Timer.Interval = 15; // approx 60 frames per second
            mi_Timer.Tick    += new EventHandler(OnTimer);
        }

        static ComboBox AddCombo(FlowLayoutPanel i_Bar, String s_Label, int s32_Width, String[] s_Items, int s32_Default)
        {
            Label i_Label = new Label();
            i_Label.Text     = s_Label;
            i_Label.AutoSize = true;
            i_Label.Margin   = new Padding(8, 5, 0, 0);
            i_Bar.Controls.Add(i_Label);

            ComboBox i_Combo = new ComboBox();
            i_Combo.DropDownStyle = ComboBoxStyle.DropDownList;
            i_Combo.Width         = s32_Width;
            i_Combo.Items.AddRange(s_Items);
            i_Combo.SelectedIndex = s32_Default;
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

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            StartStream();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopStream();
            base.OnFormClosing(e);
        }

        void PrintInfo(String s_Text, Color c_Color)
        {
            mi_LblInfo.Text      = s_Text;
            mi_LblInfo.ForeColor = c_Color;
        }

        // ============================================================================================

        void StartStream()
        {
            try
            {
                mi_Stream.StartStream();
            }
            catch (Exception Ex)
            {
                PrintInfo("Error: " + Ex.Message.Replace("\n", " "), Color.FromArgb(0xFF, 0xA0, 0x80));
                return;
            }
            mb_Running = true;
            mi_BtnStart.Text      = "Stop";
            mi_BtnStart.BackColor = Color.Salmon;
            mi_View.BreakLine();
            mi_Watch.Reset();
            mi_Watch.Start();
            md_LastTime = 0;
            mi_InfoWatch.Reset();
            mi_InfoWatch.Start();
            ms32_Frames = 0;
            mi_Timer.Start();
        }

        /// <summary>
        /// The image stays visible (Freeze). The recorded samples remain available for the Snapshot.
        /// </summary>
        void StopStream()
        {
            mi_Timer.Stop();
            mi_Stream.StopStream();
            mb_Running = false;
            mi_BtnStart.Text      = "Start";
            mi_BtnStart.BackColor = Color.PaleGreen;
        }

        double PersistenceSec
        {
            get
            {
                String[] s_Parts = mi_ComboPersist.Text.Split(' ');
                double d_Value = double.Parse(s_Parts[0], CultureInfo.InvariantCulture);
                return s_Parts[1] == "ms" ? d_Value / 1000 : d_Value;
            }
        }

        /// <summary>
        /// One frame: read the new samples, let the old trace fade, draw the new trace.
        /// </summary>
        void OnTimer(object sender, EventArgs e)
        {
            double d_Now = mi_Watch.Elapsed.TotalSeconds;
            double d_Dt  = d_Now - md_LastTime;
            md_LastTime  = d_Now;

            int  s32_Count;
            bool b_Skipped;
            try
            {
                s32_Count = mi_Stream.ReadStream(mf_Left, mf_Right, out b_Skipped);
            }
            catch (Exception Ex)
            {
                StopStream();
                PrintInfo("Error: " + Ex.Message.Replace("\n", " "), Color.FromArgb(0xFF, 0xA0, 0x80));
                return;
            }
            if (b_Skipped)
                mi_View.BreakLine(); // do not connect samples across a gap

            // Left = X, Right = Y (or swapped), converted into Volt if calibrated
            float[] f_SrcX = mi_CheckSwap.Checked ? mf_Right : mf_Left;
            float[] f_SrcY = mi_CheckSwap.Checked ? mf_Left  : mf_Right;
            float   f_FacX = (float)md_Factor[mi_CheckSwap.Checked ? 1 : 0];
            float   f_FacY = (float)md_Factor[mi_CheckSwap.Checked ? 0 : 1];
            float   f_FramePeak = 0;
            for (int S=0; S<s32_Count; S++)
            {
                mf_X[S] = f_SrcX[S] * f_FacX;
                mf_Y[S] = f_SrcY[S] * f_FacY;
                mf_PeakX    = Math.Max(mf_PeakX, Math.Abs(mf_X[S]));
                mf_PeakY    = Math.Max(mf_PeakY, Math.Abs(mf_Y[S]));
                f_FramePeak = Math.Max(f_FramePeak, Math.Max(Math.Abs(mf_X[S]), Math.Abs(mf_Y[S])));
            }

            UpdateRange(f_FramePeak, d_Dt);

            double d_Tau = PersistenceSec;
            double d_Bright = double.Parse(mi_ComboBright.Text, CultureInfo.InvariantCulture);
            mi_View.Decay(Math.Exp(-d_Dt / d_Tau));
            mi_View.AddSamples(mf_X, mf_Y, s32_Count, d_Bright / (mi_Stream.SamplesPerSecond * d_Tau));
            mi_View.Render();

            ms32_Frames ++;
            if (mi_InfoWatch.ElapsedMilliseconds >= 500)
                UpdateInfo();
        }

        /// <summary>
        /// The value at the edge of the graticule (4 divisions from the center).
        /// Auto: follows the peak of the signal (falls slowly within approx 2 seconds), rounded to 1-2-5 steps.
        /// </summary>
        void UpdateRange(float f_FramePeak, double d_Dt)
        {
            double d_FullScale = Math.Max(md_Factor[0], md_Factor[1]);
            double d_Range;
            if (mi_ComboZoom.SelectedIndex == 0)
            {
                md_AutoPeak = Math.Max(f_FramePeak, md_AutoPeak * Math.Exp(-d_Dt / 2));
                d_Range = NiceCeiling(Math.Max(md_AutoPeak * 1.05, d_FullScale * 1e-5));
            }
            else
            {
                double d_Zoom = double.Parse(mi_ComboZoom.Text.Split(' ')[0], CultureInfo.InvariantCulture);
                d_Range = d_FullScale / d_Zoom;
            }

            if (d_Range != mi_View.Range)
            {
                mi_View.Range = d_Range;
                mi_View.Clear(); // the old trace has another scale
            }
        }

        /// <summary>
        /// 0.0123 --> 0.02, 0.37 --> 0.5
        /// </summary>
        static double NiceCeiling(double d_Value)
        {
            double d_Pow = Math.Pow(10, Math.Floor(Math.Log10(d_Value)));
            double d_Norm = d_Value / d_Pow;
            if (d_Norm <= 1) return d_Pow;
            if (d_Norm <= 2) return d_Pow * 2;
            if (d_Norm <= 5) return d_Pow * 5;
            return d_Pow * 10;
        }

        void UpdateInfo()
        {
            double d_Sec = mi_InfoWatch.Elapsed.TotalSeconds;
            String s_PeakX, s_PeakY;
            if (mb_Volt)
            {
                s_PeakX = SpectrumFFT.FormatVolt(mf_PeakX) + "p";
                s_PeakY = SpectrumFFT.FormatVolt(mf_PeakY) + "p";
            }
            else
            {
                s_PeakX = Fourier.ToDb(mf_PeakX, Fourier.eUnit.FullScale).ToString("0.0", CultureInfo.InvariantCulture) + " dBFS";
                s_PeakY = Fourier.ToDb(mf_PeakY, Fourier.eUnit.FullScale).ToString("0.0", CultureInfo.InvariantCulture) + " dBFS";
            }

            String s_Info = String.Format(CultureInfo.InvariantCulture, "{0} Hz   {1:0} fps   Peak X: {2}   Peak Y: {3}   1 div = {4}",
                                          ms32_Rate, ms32_Frames / d_Sec, s_PeakX, s_PeakY, mi_View.FormatDiv());
            if (mi_Stream.SkippedSamples > 0)
                s_Info += String.Format(CultureInfo.InvariantCulture, "   Skipped: {0:0.00} s", (double)mi_Stream.SkippedSamples / ms32_Rate);
            if (mi_Stream.StatusInfo != null)
                s_Info += "   " + mi_Stream.StatusInfo;

            bool b_Clip = !mb_Volt && (mf_PeakX >= 0.999f || mf_PeakY >= 0.999f);
            if (b_Clip)
                s_Info += "   CLIPPED --> reduce the gain!";
            PrintInfo(s_Info, b_Clip ? Color.FromArgb(0xFF, 0xA0, 0x80) : Color.White);

            mi_InfoWatch.Reset();
            mi_InfoWatch.Start();
            ms32_Frames = 0;
            mf_PeakX    = 0;
            mf_PeakY    = 0;
        }

        // ============================================================================================

        /// <summary>
        /// The last second of both channels into the main window (for the X/Y Plot, FFT or saving)
        /// </summary>
        void OnSnapshot(object sender, EventArgs e)
        {
            if (Utils.FormMain.HasUnsavedChanges())
                return;

            Capture i_Capture = mi_Stream.GetLastSamples(ms32_Rate);
            if (i_Capture == null)
            {
                PrintInfo("Nothing has been recorded yet.", Color.FromArgb(0xFF, 0xA0, 0x80));
                return;
            }

            if (mb_Volt)
            {
                for (int C=0; C<2; C++)
                {
                    float f_Factor = (float)md_Factor[C];
                    float[] f_Data = i_Capture.mi_Channels[C].mf_Analog;
                    for (int S=0; S<f_Data.Length; S++)
                        f_Data[S] *= f_Factor;
                }
            }

            Utils.FormMain.StoreNewCapture(i_Capture);
            PrintInfo(String.Format("Stored the last {0:N0} samples in the main window (right-click on a channel --> X/Y Plot).", i_Capture.ms32_Samples), Color.LightGreen);
        }

        void OnExportImage(object sender, EventArgs e)
        {
            SaveFileDialog i_Dlg = new SaveFileDialog();
            i_Dlg.Filter           = "PNG image (*.png)|*.png";
            i_Dlg.FileName         = "LiveXY " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".png";
            i_Dlg.InitialDirectory = Utils.SampleDir;
            if (i_Dlg.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                using (Bitmap i_Bmp = new Bitmap(mi_View.Width, mi_View.Height))
                {
                    mi_View.DrawToBitmap(i_Bmp, new Rectangle(0, 0, mi_View.Width, mi_View.Height));
                    i_Bmp.Save(i_Dlg.FileName, ImageFormat.Png);
                }
                PrintInfo("Saved image " + i_Dlg.FileName, Color.LightGreen);
            }
            catch (Exception Ex)
            {
                Utils.ShowExceptionBox(this, Ex);
            }
        }
    }

    // ==================================================================================================================

    /// <summary>
    /// Simulates the phosphor of an analog oscilloscope.
    /// Each sample deposits energy along the line from the previous sample: a fast moving beam is dimmer than a slow one.
    /// The energy is distributed to the 4 neighbour pixels (anti-aliasing) and blurred for display (width of the beam).
    /// The energy decays exponentially (persistence). The brightness is 1 - exp(-energy), so the phosphor saturates softly.
    /// </summary>
    public class XYLiveView : Panel
    {
        const int MARGIN = 40;
        const int DIVS   = 8;      // divisions of the graticule (4 from the center to the edge)
        const int LUT_SIZE  = 2048;
        const float LUT_MAX = 8;   // energy at the end of the LUT

        public double Range  = 1;      // value at the edge of the graticule
        public bool   Lines  = true;   // false = dots only
        public String Unit   = "FS";

        float[] mf_Energy;
        float[] mf_Blur;     // horizontally blurred energy
        int[]   ms32_Pixels;
        Bitmap  mi_Bitmap;
        int     ms32_Size;
        float   mf_LastX = float.NaN;
        float   mf_LastY;
        static int[] ms32_Lut = CreateLut();

        public XYLiveView()
        {
            DoubleBuffered = true;
            BackColor      = Color.Black;
        }

        /// <summary>
        /// P31 phosphor: black --> green --> light green (saturated)
        /// </summary>
        static int[] CreateLut()
        {
            int[] s32_Lut = new int[LUT_SIZE];
            for (int i=0; i<LUT_SIZE; i++)
            {
                double d_Energy = (double)i / LUT_SIZE * LUT_MAX;
                double d_Bright = 1 - Math.Exp(-d_Energy);
                double d_White  = Math.Max(0, (d_Bright - 0.75) / 0.25); // the core of the beam becomes whitish
                int R = (int)(255 * (0.15 * d_Bright + 0.6 * d_White));
                int G = (int)(255 * d_Bright);
                int B = (int)(255 * (0.35 * d_Bright + 0.5 * d_White));
                s32_Lut[i] = Color.FromArgb(0xFF, Math.Min(255, R), Math.Min(255, G), Math.Min(255, B)).ToArgb();
            }
            return s32_Lut;
        }

        Rectangle PlotRect
        {
            get
            {
                int s32_Size = Math.Max(50, Math.Min(ClientSize.Width, ClientSize.Height) - 2 * MARGIN);
                return new Rectangle((ClientSize.Width - s32_Size) / 2, (ClientSize.Height - s32_Size) / 2, s32_Size, s32_Size);
            }
        }

        void EnsureBuffers()
        {
            int s32_Size = PlotRect.Width;
            if (s32_Size == ms32_Size && mi_Bitmap != null)
                return;

            ms32_Size   = s32_Size;
            mf_Energy   = new float[s32_Size * s32_Size];
            mf_Blur     = new float[s32_Size * s32_Size];
            ms32_Pixels = new int  [s32_Size * s32_Size];
            if (mi_Bitmap != null)
                mi_Bitmap.Dispose();
            mi_Bitmap = new Bitmap(s32_Size, s32_Size, PixelFormat.Format32bppArgb);
            mf_LastX  = float.NaN;
        }

        public void Clear()
        {
            if (mf_Energy != null)
                Array.Clear(mf_Energy, 0, mf_Energy.Length);
            mf_LastX = float.NaN;
        }

        public void BreakLine()
        {
            mf_LastX = float.NaN;
        }

        public void Decay(double d_Factor)
        {
            EnsureBuffers();
            float f_Factor = (float)d_Factor;
            for (int i=0; i<mf_Energy.Length; i++)
                mf_Energy[i] *= f_Factor;
        }

        /// <summary>
        /// d_Energy = energy of one sample. It is scaled with the size of the plot,
        /// so a figure has the same brightness independent of the window size.
        /// </summary>
        public void AddSamples(float[] f_X, float[] f_Y, int s32_Count, double d_Energy)
        {
            EnsureBuffers();
            float f_Half   = ms32_Size / 2f;
            float f_Scale  = (float)(f_Half / Range);
            float f_Energy = (float)(d_Energy * ms32_Size * 3);

            for (int S=0; S<s32_Count; S++)
            {
                float X = f_Half + f_X[S] * f_Scale;
                float Y = f_Half - f_Y[S] * f_Scale;

                if (Lines && !float.IsNaN(mf_LastX))
                    DrawSegment(mf_LastX, mf_LastY, X, Y, f_Energy);
                else
                    Deposit(X, Y, f_Energy);

                mf_LastX = X;
                mf_LastY = Y;
            }
        }

        /// <summary>
        /// The energy of the sample is distributed along the line (not including the start point which belongs to the previous sample)
        /// </summary>
        void DrawSegment(float X0, float Y0, float X1, float Y1, float f_Energy)
        {
            float f_DX = X1 - X0;
            float f_DY = Y1 - Y0;
            int s32_Steps = (int)Math.Ceiling(Math.Max(Math.Abs(f_DX), Math.Abs(f_DY)));
            if (s32_Steps <= 1)
            {
                Deposit(X1, Y1, f_Energy);
                return;
            }
            // Far outside of the screen (clipped signal with high zoom): do not waste time
            if (s32_Steps > 4 * ms32_Size)
                return;

            float f_Part = f_Energy / s32_Steps;
            for (int k=1; k<=s32_Steps; k++)
            {
                float f_T = (float)k / s32_Steps;
                Deposit(X0 + f_DX * f_T, Y0 + f_DY * f_T, f_Part);
            }
        }

        /// <summary>
        /// Bilinear: the energy is distributed to the 4 pixels around the exact position
        /// </summary>
        void Deposit(float X, float Y, float f_Energy)
        {
            X -= 0.5f; // pixel centers
            Y -= 0.5f;
            if (X < 0 || Y < 0)
                return;
            int s32_X = (int)X;
            int s32_Y = (int)Y;
            if (s32_X >= ms32_Size - 1 || s32_Y >= ms32_Size - 1)
                return;

            float f_FX = X - s32_X;
            float f_FY = Y - s32_Y;
            int   s32_Pos = s32_Y * ms32_Size + s32_X;
            mf_Energy[s32_Pos]                 += f_Energy * (1 - f_FX) * (1 - f_FY);
            mf_Energy[s32_Pos + 1]             += f_Energy * f_FX       * (1 - f_FY);
            mf_Energy[s32_Pos + ms32_Size]     += f_Energy * (1 - f_FX) * f_FY;
            mf_Energy[s32_Pos + ms32_Size + 1] += f_Energy * f_FX       * f_FY;
        }

        /// <summary>
        /// Converts the energy into colors and displays them
        /// </summary>
        public void Render()
        {
            EnsureBuffers();
            int N = ms32_Size;

            // The beam has a width of approx 3 pixels: blur with the kernel 1-2-1 horizontally and vertically.
            // The blur reduces the peak energy of a thin line to approx 1/3, which is compensated by GAIN.
            const float GAIN = 3f;
            for (int Y=0; Y<N; Y++)
            {
                int s32_Row = Y * N;
                mf_Blur[s32_Row] = mf_Energy[s32_Row];
                for (int X=1; X<N-1; X++)
                    mf_Blur[s32_Row + X] = (mf_Energy[s32_Row + X - 1] + 2 * mf_Energy[s32_Row + X] + mf_Energy[s32_Row + X + 1]) * 0.25f;
                mf_Blur[s32_Row + N - 1] = mf_Energy[s32_Row + N - 1];
            }

            float f_ToLut = LUT_SIZE / LUT_MAX * GAIN * 0.25f;
            for (int i=0; i<mf_Blur.Length; i++)
            {
                float f_Up   = (i >= N)                ? mf_Blur[i - N] : 0;
                float f_Down = (i < mf_Blur.Length - N) ? mf_Blur[i + N] : 0;
                int s32_Idx = (int)((f_Up + 2 * mf_Blur[i] + f_Down) * f_ToLut);
                ms32_Pixels[i] = ms32_Lut[s32_Idx < LUT_SIZE ? s32_Idx : LUT_SIZE - 1];
            }

            BitmapData i_Data = mi_Bitmap.LockBits(new Rectangle(0, 0, ms32_Size, ms32_Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            for (int Y=0; Y<ms32_Size; Y++)
                Marshal.Copy(ms32_Pixels, Y * ms32_Size, new IntPtr(i_Data.Scan0.ToInt64() + (long)Y * i_Data.Stride), ms32_Size);
            mi_Bitmap.UnlockBits(i_Data);
            Invalidate();
        }

        /// <summary>
        /// "0.25 FS" or "500 mV"
        /// </summary>
        public String FormatDiv()
        {
            double d_Div = Range / (DIVS / 2);
            if (Unit == "V")
                return SpectrumFFT.FormatVolt(d_Div);
            return d_Div.ToString("0.#####", CultureInfo.InvariantCulture) + " FS";
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics  g      = e.Graphics;
            Rectangle r_Plot = PlotRect;

            if (mi_Bitmap != null && mi_Bitmap.Width == r_Plot.Width)
                g.DrawImageUnscaled(mi_Bitmap, r_Plot.Left, r_Plot.Top);

            // Graticule like an oscilloscope screen
            using (Pen   i_Grid  = new Pen(Color.FromArgb(0x60, 0x80, 0x80, 0x80)))
            using (Pen   i_Axis  = new Pen(Color.FromArgb(0xA0, 0x90, 0x90, 0x90)))
            using (Brush i_Text  = new SolidBrush(Color.FromArgb(0xC0, 0xC0, 0xC0)))
            using (Font  i_Font  = new Font("Segoe UI", 8))
            {
                for (int D=0; D<=DIVS; D++)
                {
                    float f_Pos = (float)D / DIVS;
                    Pen i_Pen = (D == 0 || D == DIVS || D == DIVS / 2) ? i_Axis : i_Grid;
                    g.DrawLine(i_Pen, r_Plot.Left + f_Pos * r_Plot.Width, r_Plot.Top, r_Plot.Left + f_Pos * r_Plot.Width, r_Plot.Bottom);
                    g.DrawLine(i_Pen, r_Plot.Left, r_Plot.Top + f_Pos * r_Plot.Height, r_Plot.Right, r_Plot.Top + f_Pos * r_Plot.Height);
                }

                String s_Div = "1 div = " + FormatDiv();
                g.DrawString(s_Div, i_Font, i_Text, r_Plot.Left, r_Plot.Bottom + 4);
                g.DrawString("X", i_Font, i_Text, r_Plot.Right + 4, r_Plot.Top + r_Plot.Height / 2 - 7);
                g.DrawString("Y", i_Font, i_Text, r_Plot.Left + r_Plot.Width / 2 - 4, r_Plot.Top - 16);
            }
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
