/*
------------------------------------------------------------
Oscilloscope Waveform Analyzer by ElmüSoft (www.netcult.ch/elmue)
This code is released under the terms of the GNU General Public License.
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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

using IOperation        = Operations.OperationManager.IOperation;
using GraphMenuItem     = Operations.OperationManager.GraphMenuItem;
using Utils             = OsziWaveformAnalyzer.Utils;
using OsziPanel         = OsziWaveformAnalyzer.OsziPanel;
using Capture           = OsziWaveformAnalyzer.Utils.Capture;
using Channel           = OsziWaveformAnalyzer.Utils.Channel;

namespace Operations
{
    /// <summary>
    /// Fast Fourier Transform without any external dependencies.
    /// </summary>
    public static class Fourier
    {
        public enum eWindow
        {
            [Description("Hann")]                  Hann,
            [Description("Hamming")]               Hamming,
            [Description("Blackman-Harris")]       BlackmanHarris,
            [Description("Flat Top (amplitude)")]  FlatTop,
            [Description("Rectangular (none)")]    Rectangular,
        }

        // Limit memory usage: 4 M samples need 2 x 32 MB for the complex FFT buffers
        public const int MAX_FFT_SIZE = 1 << 22;

        public static int NextPowerOf2(int s32_Value)
        {
            int s32_Pow = 1;
            while (s32_Pow < s32_Value)
                s32_Pow <<= 1;
            return s32_Pow;
        }

        /// <summary>
        /// Symmetric window of length s32_Count
        /// </summary>
        public static double[] CreateWindow(eWindow e_Window, int s32_Count)
        {
            double[] d_Win = new double[s32_Count];
            double   d_Div = Math.Max(1, s32_Count - 1);
            for (int n=0; n<s32_Count; n++)
            {
                double x = 2.0 * Math.PI * n / d_Div;
                switch (e_Window)
                {
                    case eWindow.Hann:           d_Win[n] = 0.5  - 0.5  * Math.Cos(x); break;
                    case eWindow.Hamming:        d_Win[n] = 0.54 - 0.46 * Math.Cos(x); break;
                    case eWindow.BlackmanHarris: d_Win[n] = 0.35875 - 0.48829 * Math.Cos(x) + 0.14128 * Math.Cos(2 * x) - 0.01168 * Math.Cos(3 * x); break;
                    case eWindow.FlatTop:        d_Win[n] = 0.21557895 - 0.41663158 * Math.Cos(x) + 0.277263158 * Math.Cos(2 * x)
                                                          - 0.083578947 * Math.Cos(3 * x) + 0.006947368 * Math.Cos(4 * x); break;
                    default:                     d_Win[n] = 1.0; break;
                }
            }
            return d_Win;
        }

        /// <summary>
        /// In-place iterative radix-2 FFT. The length must be a power of 2.
        /// </summary>
        public static void Transform(double[] d_Re, double[] d_Im)
        {
            int N = d_Re.Length;

            // Bit reversal permutation
            for (int i=1, j=0; i<N; i++)
            {
                int s32_Bit = N >> 1;
                for (; (j & s32_Bit) != 0; s32_Bit >>= 1)
                    j ^= s32_Bit;
                j ^= s32_Bit;

                if (i < j)
                {
                    double d_Tmp = d_Re[i]; d_Re[i] = d_Re[j]; d_Re[j] = d_Tmp;
                           d_Tmp = d_Im[i]; d_Im[i] = d_Im[j]; d_Im[j] = d_Tmp;
                }
            }

            for (int s32_Len=2; s32_Len<=N; s32_Len<<=1)
            {
                int    s32_Half = s32_Len >> 1;
                double d_Angle  = -2.0 * Math.PI / s32_Len;

                // Twiddle factors of this stage (calculated directly to avoid accumulating rounding errors)
                double[] d_Cos = new double[s32_Half];
                double[] d_Sin = new double[s32_Half];
                for (int k=0; k<s32_Half; k++)
                {
                    d_Cos[k] = Math.Cos(d_Angle * k);
                    d_Sin[k] = Math.Sin(d_Angle * k);
                }

                for (int s32_Start=0; s32_Start<N; s32_Start+=s32_Len)
                {
                    for (int k=0; k<s32_Half; k++)
                    {
                        int a = s32_Start + k;
                        int b = a + s32_Half;
                        double d_Re2 = d_Re[b] * d_Cos[k] - d_Im[b] * d_Sin[k];
                        double d_Im2 = d_Re[b] * d_Sin[k] + d_Im[b] * d_Cos[k];
                        d_Re[b] = d_Re[a] - d_Re2;
                        d_Im[b] = d_Im[a] - d_Im2;
                        d_Re[a] += d_Re2;
                        d_Im[a] += d_Im2;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the one-sided amplitude spectrum (bins 0 ... FFT size / 2) in Volt peak.
        /// A sine wave of amplitude A produces a peak of A (exactly A with the flat top window).
        /// The samples are zero-padded to the next power of 2.
        /// </summary>
        public static double[] AmplitudeSpectrum(float[] f_Samples, int s32_Start, int s32_Count, eWindow e_Window,
                                                 bool b_RemoveDC, out int s32_FftSize)
        {
            s32_FftSize = NextPowerOf2(s32_Count);

            double d_Mean = 0;
            if (b_RemoveDC)
            {
                for (int i=0; i<s32_Count; i++)
                    d_Mean += f_Samples[s32_Start + i];
                d_Mean /= s32_Count;
            }

            double[] d_Win = CreateWindow(e_Window, s32_Count);
            double   d_Sum = 0;
            double[] d_Re  = new double[s32_FftSize];
            double[] d_Im  = new double[s32_FftSize];
            for (int i=0; i<s32_Count; i++)
            {
                d_Re[i] = (f_Samples[s32_Start + i] - d_Mean) * d_Win[i];
                d_Sum  += d_Win[i];
            }

            Transform(d_Re, d_Im);

            int      s32_Bins = s32_FftSize / 2 + 1;
            double[] d_Amp    = new double[s32_Bins];
            double   d_Scale  = 2.0 / d_Sum;
            for (int k=0; k<s32_Bins; k++)
            {
                d_Amp[k] = Math.Sqrt(d_Re[k] * d_Re[k] + d_Im[k] * d_Im[k]) * d_Scale;
            }
            // DC and Nyquist exist only once
            d_Amp[0] /= 2;
            d_Amp[s32_Bins - 1] /= 2;
            return d_Amp;
        }

        /// <summary>
        /// Converts Volt peak into dBV (RMS) as displayed by most oscilloscopes. 1 Vrms = 0 dBV
        /// </summary>
        public static double ToDbV(double d_VoltPeak)
        {
            return 20.0 * Math.Log10(Math.Max(d_VoltPeak / Math.Sqrt(2), 1e-12));
        }

        public class Peak
        {
            public double md_Frequency; // Hz, interpolated between bins
            public double md_Amplitude; // Volt peak
        }

        /// <summary>
        /// Finds the strongest local maxima in d_Amp.
        /// The frequency is refined by parabolic interpolation of the logarithmic magnitude in d_Refine.
        /// This is very accurate with the Blackman-Harris window (error < 1% of a bin), but not with the flat top window,
        /// whose flat main lobe does not have a parabolic shape. Therefore d_Refine should be a Blackman-Harris spectrum
        /// of the same samples, while d_Amp may use any window. d_Refine = null --> refine with d_Amp.
        /// s32_MinDist = minimum distance between two peaks in bins
        /// </summary>
        public static List<Peak> FindPeaks(double[] d_Amp, double[] d_Refine, double d_BinWidth, int s32_MaxPeaks, int s32_MinDist)
        {
            if (d_Refine == null)
                d_Refine = d_Amp;

            List<int> i_Candidates = new List<int>();
            for (int k=2; k<d_Amp.Length - 1; k++) // skip DC
            {
                if (d_Amp[k] > d_Amp[k - 1] && d_Amp[k] >= d_Amp[k + 1])
                    i_Candidates.Add(k);
            }
            i_Candidates.Sort(delegate(int a, int b) { return d_Amp[b].CompareTo(d_Amp[a]); });

            List<int>  i_Taken = new List<int>();
            List<Peak> i_Peaks = new List<Peak>();
            double d_Strongest = i_Candidates.Count > 0 ? d_Amp[i_Candidates[0]] : 0;
            foreach (int k in i_Candidates)
            {
                if (i_Peaks.Count >= s32_MaxPeaks || d_Amp[k] < d_Strongest * 1e-4) // ignore peaks 80 dB below the strongest
                    break;

                bool b_TooClose = false;
                foreach (int s32_Taken in i_Taken)
                {
                    if (Math.Abs(s32_Taken - k) < s32_MinDist)
                        b_TooClose = true;
                }
                if (b_TooClose)
                    continue;

                // The maximum of the refine spectrum near this peak
                int s32_Max = k;
                for (int i=Math.Max(1, k - s32_MinDist / 2); i<=Math.Min(d_Refine.Length - 2, k + s32_MinDist / 2); i++)
                {
                    if (d_Refine[i] > d_Refine[s32_Max])
                        s32_Max = i;
                }

                double a = Math.Log(Math.Max(d_Refine[s32_Max - 1], 1e-15));
                double b = Math.Log(Math.Max(d_Refine[s32_Max],     1e-15));
                double c = Math.Log(Math.Max(d_Refine[s32_Max + 1], 1e-15));
                double d_Denom = a - 2 * b + c;
                double d_Delta = (d_Denom < 0) ? 0.5 * (a - c) / d_Denom : 0;

                Peak i_Peak = new Peak();
                i_Peak.md_Frequency = (s32_Max + d_Delta) * d_BinWidth;
                i_Peak.md_Amplitude = d_Amp[k];
                i_Peaks.Add(i_Peak);
                i_Taken.Add(k);
            }
            return i_Peaks;
        }
    }

    // ==================================================================================================================

    /// <summary>
    /// Right-click on an analog channel --> "FFT Spectrum" opens a window with the frequency spectrum.
    /// The window is not modal, so multiple spectra can be compared while working with the main window.
    /// </summary>
    public class SpectrumFFT : Form, IOperation
    {
        const String RANGE_ALL     = "Entire capture";
        const String RANGE_VISIBLE = "Visible screen";
        const String SCALE_DB      = "dBV (RMS)";
        const String SCALE_LINEAR  = "Volt (peak)";

        float[]      mf_Samples;
        decimal      md_SampleRate;  // Hz
        int          ms32_AllStart;
        int          ms32_AllCount;
        int          ms32_VisStart;
        int          ms32_VisCount;

        ComboBox     mi_ComboWindow;
        ComboBox     mi_ComboScale;
        ComboBox     mi_ComboRange;
        CheckBox     mi_CheckLogF;
        CheckBox     mi_CheckDC;
        Label        mi_LblInfo;
        SpectrumView mi_View;

        /// <summary>
        /// Implementation of interface IOperation
        /// </summary>
        public void GetMenuItems(Channel i_Channel, bool b_Analog, List<GraphMenuItem> i_Items)
        {
            if (i_Channel == null || !b_Analog || i_Channel.mf_Analog == null)
                return; // the user did not click on an analog channel

            // When all analog channels are drawn on top of each other it is impossible to know which channel the user wants.
            if (Utils.OsziPanel.CommonAnalogDrawing)
                return;

            GraphMenuItem i_Item = new GraphMenuItem();
            i_Item.ms_MenuText  = "FFT Spectrum";
            i_Item.ms_ImageFile = "Filter.ico";
            i_Items.Add(i_Item);
        }

        /// <summary>
        /// Implementation of interface IOperation
        /// </summary>
        public String Execute(Channel i_Channel, int s32_Sample, bool b_Analog, Object o_Tag)
        {
            Capture i_Capture = OsziPanel.CurCapture;
            if (i_Capture.ms64_SampleDist <= 0)
                return "Error: The capture has no valid sample rate.";

            mf_Samples    = i_Channel.mf_Analog; // the array is not modified by this class
            md_SampleRate = Utils.PICOS_PER_SECOND / i_Capture.ms64_SampleDist;
            ms32_AllStart = 0;
            ms32_AllCount = i_Capture.ms32_Samples;
            ms32_VisStart = Math.Max(0, Utils.OsziPanel.DispStart);
            ms32_VisCount = Math.Min(i_Capture.ms32_Samples, Utils.OsziPanel.DispEnd + 1) - ms32_VisStart;

            CreateControls(i_Channel);
            Show(Utils.FormMain); // not modal
            Calculate();
            return null;
        }

        void CreateControls(Channel i_Channel)
        {
            Text          = "FFT Spectrum  —  " + i_Channel.ms_Name;
            Icon          = Utils.FormMain.Icon;
            BackColor     = Color.DimGray;
            ForeColor     = Color.White;
            ClientSize    = new Size(900, 520);
            MinimumSize   = new Size(600, 350);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;

            FlowLayoutPanel i_Bar = new FlowLayoutPanel();
            i_Bar.Dock    = DockStyle.Top;
            i_Bar.Height  = 32;
            i_Bar.Padding = new Padding(4, 4, 4, 0);

            mi_ComboWindow = AddCombo(i_Bar, "Window:", 140);
            foreach (Fourier.eWindow e_Win in Enum.GetValues(typeof(Fourier.eWindow)))
                mi_ComboWindow.Items.Add(Utils.GetDescriptionAttribute(e_Win));
            mi_ComboWindow.SelectedIndex = 0;

            mi_ComboScale = AddCombo(i_Bar, "Scale:", 90);
            mi_ComboScale.Items.AddRange(new Object[] { SCALE_DB, SCALE_LINEAR });
            mi_ComboScale.SelectedIndex = 0;

            mi_ComboRange = AddCombo(i_Bar, "Range:", 110);
            mi_ComboRange.Items.AddRange(new Object[] { RANGE_ALL, RANGE_VISIBLE });
            mi_ComboRange.SelectedIndex = 0;

            mi_CheckLogF = AddCheck(i_Bar, "Log frequency", false);
            mi_CheckDC   = AddCheck(i_Bar, "Remove DC",     true);

            Button i_Export = new Button();
            i_Export.Text      = "Export CSV";
            i_Export.ForeColor = Color.Black;
            i_Export.BackColor = SystemColors.Control;
            i_Export.AutoSize  = true;
            i_Export.Margin    = new Padding(10, 0, 0, 0);
            i_Export.Click    += new EventHandler(OnExportClick);
            i_Bar.Controls.Add(i_Export);

            mi_LblInfo = new Label();
            mi_LblInfo.Dock      = DockStyle.Bottom;
            mi_LblInfo.Height    = 22;
            mi_LblInfo.TextAlign = ContentAlignment.MiddleLeft;
            mi_LblInfo.Padding   = new Padding(4, 0, 0, 0);

            mi_View = new SpectrumView();
            mi_View.Dock       = DockStyle.Fill;
            mi_View.TraceColor = OsziPanel.GetChannelColor(i_Channel);

            Controls.Add(mi_View);    // Fill must be added first
            Controls.Add(mi_LblInfo);
            Controls.Add(i_Bar);

            mi_ComboWindow.SelectedIndexChanged += new EventHandler(OnSettingChanged);
            mi_ComboRange .SelectedIndexChanged += new EventHandler(OnSettingChanged);
            mi_CheckDC    .CheckedChanged       += new EventHandler(OnSettingChanged);
            mi_ComboScale .SelectedIndexChanged += new EventHandler(OnDisplayChanged);
            mi_CheckLogF  .CheckedChanged       += new EventHandler(OnDisplayChanged);
        }

        static ComboBox AddCombo(FlowLayoutPanel i_Bar, String s_Label, int s32_Width)
        {
            Label i_Label = new Label();
            i_Label.Text      = s_Label;
            i_Label.AutoSize  = true;
            i_Label.Margin    = new Padding(6, 5, 0, 0);
            i_Bar.Controls.Add(i_Label);

            ComboBox i_Combo = new ComboBox();
            i_Combo.DropDownStyle = ComboBoxStyle.DropDownList;
            i_Combo.Width         = s32_Width;
            i_Bar.Controls.Add(i_Combo);
            return i_Combo;
        }

        static CheckBox AddCheck(FlowLayoutPanel i_Bar, String s_Text, bool b_Checked)
        {
            CheckBox i_Check = new CheckBox();
            i_Check.Text     = s_Text;
            i_Check.Checked  = b_Checked;
            i_Check.AutoSize = true;
            i_Check.Margin   = new Padding(10, 4, 0, 0);
            i_Bar.Controls.Add(i_Check);
            return i_Check;
        }

        void OnSettingChanged(object sender, EventArgs e)
        {
            Calculate();
        }

        void OnDisplayChanged(object sender, EventArgs e)
        {
            mi_View.ShowDb   = mi_ComboScale.Text == SCALE_DB;
            mi_View.LogFreq  = mi_CheckLogF.Checked;
            mi_View.Invalidate();
        }

        void Calculate()
        {
            bool b_Visible  = mi_ComboRange.Text == RANGE_VISIBLE;
            int  s32_Start  = b_Visible ? ms32_VisStart : ms32_AllStart;
            int  s32_Count  = b_Visible ? ms32_VisCount : ms32_AllCount;
            bool b_Truncated = false;

            if (s32_Count > Fourier.MAX_FFT_SIZE)
            {
                s32_Count   = Fourier.MAX_FFT_SIZE;
                b_Truncated = true;
            }

            if (s32_Count < 16)
            {
                mi_LblInfo.Text = "Not enough samples.";
                mi_View.SetSpectrum(null, 0, 0);
                return;
            }

            Cursor = Cursors.WaitCursor;
            Fourier.eWindow e_Window = (Fourier.eWindow)mi_ComboWindow.SelectedIndex;

            int s32_FftSize;
            double[] d_Amp = Fourier.AmplitudeSpectrum(mf_Samples, s32_Start, s32_Count, e_Window, mi_CheckDC.Checked, out s32_FftSize);

            double d_Rate     = (double)md_SampleRate;
            double d_BinWidth = d_Rate / s32_FftSize;
            mi_View.ShowDb    = mi_ComboScale.Text == SCALE_DB;
            mi_View.LogFreq   = mi_CheckLogF.Checked;

            // The main lobe of the window is several bins wide. Zero padding makes it wider in bins.
            int s32_MinDist = Math.Max(3, (int)(6.0 * s32_FftSize / s32_Count));

            // The peak frequencies are always measured in a Blackman-Harris spectrum (see FindPeaks())
            double[] d_Refine = null;
            if (e_Window != Fourier.eWindow.BlackmanHarris)
            {
                int s32_Dummy;
                d_Refine = Fourier.AmplitudeSpectrum(mf_Samples, s32_Start, s32_Count, Fourier.eWindow.BlackmanHarris, mi_CheckDC.Checked, out s32_Dummy);
            }
            mi_View.Peaks = Fourier.FindPeaks(d_Amp, d_Refine, d_BinWidth, 5, s32_MinDist);
            mi_View.SetSpectrum(d_Amp, d_BinWidth, d_Rate);

            String s_Info = String.Format("Samples: {0:N0}   FFT size: {1:N0}   Sample rate: {2}   Resolution: {3}   Nyquist: {4}",
                                          s32_Count, s32_FftSize,
                                          FormatFreq(d_Rate), FormatFreq(d_Rate / s32_Count), FormatFreq(d_Rate / 2));
            if (b_Truncated)
                s_Info += "   (only the first 4 M samples: zoom in and use 'Visible screen')";

            mi_LblInfo.Text      = s_Info;
            mi_LblInfo.ForeColor = b_Truncated ? Color.FromArgb(0xFF, 0xA0, 0x80) : Color.White;
            Cursor = Cursors.Default;
        }

        void OnExportClick(object sender, EventArgs e)
        {
            double[] d_Amp = mi_View.Spectrum;
            if (d_Amp == null)
                return;

            SaveFileDialog i_Dlg = new SaveFileDialog();
            i_Dlg.Filter   = "CSV file (*.csv)|*.csv";
            i_Dlg.FileName = "Spectrum.csv";
            if (i_Dlg.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                StringBuilder i_Csv = new StringBuilder();
                i_Csv.AppendLine("Frequency [Hz],Amplitude [V peak],Amplitude [dBV RMS]");
                for (int k=0; k<d_Amp.Length; k++)
                {
                    i_Csv.AppendLine(String.Format(CultureInfo.InvariantCulture, "{0:G10},{1:G6},{2:F2}",
                                                   k * mi_View.BinWidth, d_Amp[k], Fourier.ToDbV(d_Amp[k])));
                }
                File.WriteAllText(i_Dlg.FileName, i_Csv.ToString());
            }
            catch (Exception Ex)
            {
                Utils.ShowExceptionBox(this, Ex);
            }
        }

        public static String FormatFreq(double d_Freq)
        {
            if (double.IsNaN(d_Freq) || double.IsInfinity(d_Freq) || d_Freq > 1e18)
                return "?";
            return Utils.FormatFrequency((decimal)Math.Max(0, d_Freq));
        }

        public static String FormatVolt(double d_Volt)
        {
            double d_Abs = Math.Abs(d_Volt);
            if (d_Abs == 0)    return "0 V";
            if (d_Abs >= 1)    return (d_Volt).ToString("0.###", CultureInfo.InvariantCulture)       + " V";
            if (d_Abs >= 1e-3) return (d_Volt * 1e3).ToString("0.###", CultureInfo.InvariantCulture) + " mV";
            return                    (d_Volt * 1e6).ToString("0.###", CultureInfo.InvariantCulture) + " µV";
        }
    }

    // ==================================================================================================================

    /// <summary>
    /// Draws the spectrum. Drag with the left mouse button to zoom the frequency range, double-click to reset.
    /// </summary>
    public class SpectrumView : Panel
    {
        const int MARGIN_LEFT   = 70;
        const int MARGIN_RIGHT  = 20;
        const int MARGIN_TOP    = 12;
        const int MARGIN_BOTTOM = 28;
        const double DB_RANGE   = 100; // dynamic range displayed

        double[] md_Amp;
        double   md_BinWidth;
        double   md_Rate;
        double   md_FreqMin;  // zoom
        double   md_FreqMax;
        int      ms32_MouseX = -1;
        int      ms32_DragX  = -1;

        public Color              TraceColor = Color.Yellow;
        public bool               ShowDb     = true;
        public bool               LogFreq    = false;
        public List<Fourier.Peak> Peaks      = new List<Fourier.Peak>();

        public double[] Spectrum  { get { return md_Amp;      } }
        public double   BinWidth  { get { return md_BinWidth; } }

        public SpectrumView()
        {
            DoubleBuffered = true;
            BackColor      = Color.Black;
            ResizeRedraw   = true;
        }

        public void SetSpectrum(double[] d_Amp, double d_BinWidth, double d_Rate)
        {
            bool b_KeepZoom = md_Amp != null && md_Rate == d_Rate && md_FreqMax > md_FreqMin;
            md_Amp      = d_Amp;
            md_BinWidth = d_BinWidth;
            md_Rate     = d_Rate;
            if (!b_KeepZoom)
            {
                md_FreqMin = 0;
                md_FreqMax = d_Rate / 2;
            }
            Invalidate();
        }

        Rectangle PlotRect
        {
            get
            {
                return new Rectangle(MARGIN_LEFT, MARGIN_TOP,
                                     Math.Max(10, Width  - MARGIN_LEFT - MARGIN_RIGHT),
                                     Math.Max(10, Height - MARGIN_TOP  - MARGIN_BOTTOM));
            }
        }

        // In log mode the lowest frequency is the first bin
        double MinFreqDisplayed
        {
            get { return LogFreq ? Math.Max(md_FreqMin, md_BinWidth) : md_FreqMin; }
        }

        float FreqToX(double d_Freq, Rectangle r_Plot)
        {
            double d_Min = MinFreqDisplayed;
            double d_Rel = LogFreq ? (Math.Log10(Math.Max(d_Freq, d_Min)) - Math.Log10(d_Min)) / (Math.Log10(md_FreqMax) - Math.Log10(d_Min))
                                   : (d_Freq - d_Min) / (md_FreqMax - d_Min);
            return (float)(r_Plot.Left + d_Rel * r_Plot.Width);
        }

        double XToFreq(int X, Rectangle r_Plot)
        {
            double d_Min = MinFreqDisplayed;
            double d_Rel = (double)(X - r_Plot.Left) / r_Plot.Width;
            if (LogFreq)
                return Math.Pow(10, Math.Log10(d_Min) + d_Rel * (Math.Log10(md_FreqMax) - Math.Log10(d_Min)));
            return d_Min + d_Rel * (md_FreqMax - d_Min);
        }

        void GetYRange(out double d_Bottom, out double d_Top)
        {
            double d_Max = 0;
            foreach (double d_Val in md_Amp)
                d_Max = Math.Max(d_Max, d_Val);

            if (ShowDb)
            {
                d_Top    = Math.Ceiling((Fourier.ToDbV(d_Max) + 5) / 10) * 10;
                d_Bottom = d_Top - DB_RANGE;
            }
            else
            {
                d_Top    = d_Max > 0 ? d_Max * 1.1 : 1;
                d_Bottom = 0;
            }
        }

        double ValueOf(double d_Amp)
        {
            return ShowDb ? Fourier.ToDbV(d_Amp) : d_Amp;
        }

        /// <summary>
        /// 1-2-5 step for approximately s32_Count grid lines
        /// </summary>
        static double NiceStep(double d_Range, int s32_Count)
        {
            double d_Raw  = d_Range / s32_Count;
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
            Graphics g = e.Graphics;
            Rectangle r_Plot = PlotRect;

            using (Pen   i_Grid  = new Pen(Color.FromArgb(0x50, 0x50, 0x50)))
            using (Pen   i_Frame = new Pen(Color.FromArgb(0x90, 0x90, 0x90)))
            using (Brush i_Text  = new SolidBrush(Color.FromArgb(0xC0, 0xC0, 0xC0)))
            using (Font  i_Font  = new Font("Segoe UI", 8))
            {
                g.DrawRectangle(i_Frame, r_Plot);
                if (md_Amp == null || md_FreqMax <= md_FreqMin)
                    return;

                double d_Bottom, d_Top;
                GetYRange(out d_Bottom, out d_Top);

                // ------------ horizontal grid + Y labels ------------
                double d_StepY = ShowDb ? 10 : NiceStep(d_Top - d_Bottom, 8);
                for (double d_Y = Math.Ceiling(d_Bottom / d_StepY) * d_StepY; d_Y <= d_Top + d_StepY * 1e-6; d_Y += d_StepY)
                {
                    float Y = (float)(r_Plot.Bottom - (d_Y - d_Bottom) / (d_Top - d_Bottom) * r_Plot.Height);
                    g.DrawLine(i_Grid, r_Plot.Left, Y, r_Plot.Right, Y);
                    String s_Label = ShowDb ? d_Y.ToString("0", CultureInfo.InvariantCulture) + " dBV" : SpectrumFFT.FormatVolt(d_Y);
                    SizeF k_Size = g.MeasureString(s_Label, i_Font);
                    g.DrawString(s_Label, i_Font, i_Text, r_Plot.Left - k_Size.Width - 3, Y - k_Size.Height / 2);
                }

                // ------------ vertical grid + frequency labels ------------
                if (LogFreq)
                {
                    double d_Min = MinFreqDisplayed;
                    for (double d_Dec = Math.Pow(10, Math.Floor(Math.Log10(d_Min))); d_Dec <= md_FreqMax; d_Dec *= 10)
                    {
                        for (int m=1; m<=9; m++)
                        {
                            double d_F = d_Dec * m;
                            if (d_F < d_Min || d_F > md_FreqMax)
                                continue;
                            float X = FreqToX(d_F, r_Plot);
                            g.DrawLine(i_Grid, X, r_Plot.Top, X, r_Plot.Bottom);
                            if (m == 1)
                                DrawFreqLabel(g, i_Font, i_Text, d_F, X, r_Plot);
                        }
                    }
                }
                else
                {
                    double d_StepX = NiceStep(md_FreqMax - md_FreqMin, 10);
                    for (double d_F = Math.Ceiling(md_FreqMin / d_StepX) * d_StepX; d_F <= md_FreqMax + d_StepX * 1e-6; d_F += d_StepX)
                    {
                        float X = FreqToX(d_F, r_Plot);
                        g.DrawLine(i_Grid, X, r_Plot.Top, X, r_Plot.Bottom);
                        DrawFreqLabel(g, i_Font, i_Text, d_F, X, r_Plot);
                    }
                }

                // ------------ trace: maximum of all bins in each pixel column ------------
                g.SetClip(r_Plot);
                List<PointF> i_Points = new List<PointF>();
                int s32_LastBin = -1;
                for (int X=r_Plot.Left; X<=r_Plot.Right; X++)
                {
                    int s32_Bin0 = (int)Math.Floor  (XToFreq(X,     r_Plot) / md_BinWidth);
                    int s32_Bin1 = (int)Math.Ceiling(XToFreq(X + 1, r_Plot) / md_BinWidth);
                    s32_Bin0 = Math.Max(0, Math.Max(s32_Bin0, s32_LastBin + 1));
                    s32_Bin1 = Math.Min(md_Amp.Length - 1, s32_Bin1);

                    if (s32_Bin1 <= s32_Bin0) // less than one bin per pixel: draw the bins themselves
                    {
                        if (s32_Bin0 < md_Amp.Length && s32_Bin0 > s32_LastBin)
                        {
                            i_Points.Add(new PointF(FreqToX(s32_Bin0 * md_BinWidth, r_Plot), ValueToY(md_Amp[s32_Bin0], d_Bottom, d_Top, r_Plot)));
                            s32_LastBin = s32_Bin0;
                        }
                        continue;
                    }

                    double d_Max = 0;
                    for (int k=s32_Bin0; k<s32_Bin1; k++)
                        d_Max = Math.Max(d_Max, md_Amp[k]);

                    i_Points.Add(new PointF(X, ValueToY(d_Max, d_Bottom, d_Top, r_Plot)));
                    s32_LastBin = s32_Bin1 - 1;
                }
                if (i_Points.Count > 1)
                {
                    using (Pen i_Trace = new Pen(TraceColor, 1))
                        g.DrawLines(i_Trace, i_Points.ToArray());
                }

                // ------------ peaks ------------
                int s32_Nr = 1;
                foreach (Fourier.Peak i_Peak in Peaks)
                {
                    if (i_Peak.md_Frequency < MinFreqDisplayed || i_Peak.md_Frequency > md_FreqMax)
                        continue;

                    float X = FreqToX(i_Peak.md_Frequency, r_Plot);
                    float Y = ValueToY(i_Peak.md_Amplitude, d_Bottom, d_Top, r_Plot);
                    g.FillPolygon(Brushes.White, new PointF[] { new PointF(X, Y - 2), new PointF(X - 4, Y - 9), new PointF(X + 4, Y - 9) });
                    g.DrawString(s32_Nr.ToString(), i_Font, Brushes.White, X - 4, Y - 23);
                    s32_Nr ++;
                }

                g.ResetClip();

                // ------------ peak table top right ------------
                StringBuilder i_Table = new StringBuilder();
                s32_Nr = 1;
                foreach (Fourier.Peak i_Peak in Peaks)
                {
                    i_Table.AppendFormat("{0}: {1}   {2}\n", s32_Nr++, SpectrumFFT.FormatFreq(i_Peak.md_Frequency), FormatValue(i_Peak.md_Amplitude));
                }
                if (i_Table.Length > 0)
                {
                    SizeF k_Size = g.MeasureString(i_Table.ToString(), i_Font);
                    RectangleF r_Box = new RectangleF(r_Plot.Right - k_Size.Width - 8, r_Plot.Top + 4, k_Size.Width + 4, k_Size.Height);
                    using (Brush i_Back = new SolidBrush(Color.FromArgb(0xC0, 0, 0, 0)))
                        g.FillRectangle(i_Back, r_Box);
                    g.DrawString(i_Table.ToString(), i_Font, i_Text, r_Box.Left + 2, r_Box.Top);
                }

                // ------------ zoom rectangle and mouse cursor ------------
                if (ms32_DragX >= 0 && ms32_MouseX >= 0)
                {
                    using (Brush i_Zoom = new SolidBrush(Color.FromArgb(0x40, 0x80, 0x80, 0xFF)))
                        g.FillRectangle(i_Zoom, Math.Min(ms32_DragX, ms32_MouseX), r_Plot.Top, Math.Abs(ms32_MouseX - ms32_DragX), r_Plot.Height);
                }
                else if (ms32_MouseX >= r_Plot.Left && ms32_MouseX <= r_Plot.Right)
                {
                    using (Pen i_Cursor = new Pen(Color.FromArgb(0xA0, 0xA0, 0xA0)) { DashStyle = DashStyle.Dot })
                        g.DrawLine(i_Cursor, ms32_MouseX, r_Plot.Top, ms32_MouseX, r_Plot.Bottom);

                    // The strongest bin within +/- 2 pixels
                    int s32_Bin0 = Math.Max(0,                 (int)Math.Floor  (XToFreq(ms32_MouseX - 2, r_Plot) / md_BinWidth));
                    int s32_Bin1 = Math.Min(md_Amp.Length - 1, (int)Math.Ceiling(XToFreq(ms32_MouseX + 2, r_Plot) / md_BinWidth));
                    int s32_Best = s32_Bin0;
                    for (int k=s32_Bin0; k<=s32_Bin1; k++)
                    {
                        if (md_Amp[k] > md_Amp[s32_Best])
                            s32_Best = k;
                    }
                    String s_Read = SpectrumFFT.FormatFreq(s32_Best * md_BinWidth) + "   " + FormatValue(md_Amp[s32_Best]);
                    g.DrawString(s_Read, i_Font, Brushes.White, r_Plot.Left + 4, r_Plot.Top + 4);
                }
            }
        }

        String FormatValue(double d_Amp)
        {
            return ShowDb ? Fourier.ToDbV(d_Amp).ToString("0.0", CultureInfo.InvariantCulture) + " dBV"
                          : SpectrumFFT.FormatVolt(d_Amp);
        }

        float ValueToY(double d_Amp, double d_Bottom, double d_Top, Rectangle r_Plot)
        {
            double d_Rel = (ValueOf(d_Amp) - d_Bottom) / (d_Top - d_Bottom);
            d_Rel = Math.Max(-0.01, Math.Min(1.01, d_Rel));
            return (float)(r_Plot.Bottom - d_Rel * r_Plot.Height);
        }

        void DrawFreqLabel(Graphics g, Font i_Font, Brush i_Brush, double d_Freq, float X, Rectangle r_Plot)
        {
            String s_Label = SpectrumFFT.FormatFreq(d_Freq);
            SizeF  k_Size  = g.MeasureString(s_Label, i_Font);
            g.DrawString(s_Label, i_Font, i_Brush, X - k_Size.Width / 2, r_Plot.Bottom + 4);
        }

        // ------------ mouse: drag to zoom, double click to reset ------------

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            ms32_MouseX = e.X;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            ms32_MouseX = -1;
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
            if (ms32_DragX >= 0 && Math.Abs(e.X - ms32_DragX) > 5 && md_Amp != null)
            {
                Rectangle r_Plot = PlotRect;
                double d_F1 = XToFreq(Math.Max(r_Plot.Left,  Math.Min(ms32_DragX, e.X)), r_Plot);
                double d_F2 = XToFreq(Math.Min(r_Plot.Right, Math.Max(ms32_DragX, e.X)), r_Plot);
                if (d_F2 - d_F1 > md_BinWidth * 4)
                {
                    md_FreqMin = d_F1;
                    md_FreqMax = d_F2;
                }
            }
            ms32_DragX = -1;
            Invalidate();
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            md_FreqMin = 0;
            md_FreqMax = md_Rate / 2;
            Invalidate();
        }
    }
}
