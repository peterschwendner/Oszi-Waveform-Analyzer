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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
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
    /// Right-click on an analog channel --> "X/Y Plot" opens a window which displays channel X versus channel Y (Lissajous figure).
    /// This replaces the XY mode of the oscilloscope: capture both channels in Yt mode and display them here as X/Y.
    /// The window is not modal.
    /// </summary>
    public class XYPlot : Form, IOperation
    {
        const String RANGE_ALL     = "Entire capture";
        const String RANGE_VISIBLE = "Visible screen";

        int        ms32_VisStart;
        int        ms32_VisEnd;
        int        ms32_Samples;
        ComboBox   mi_ComboX;
        ComboBox   mi_ComboY;
        ComboBox   mi_ComboRange;
        CheckBox   mi_CheckLines;
        CheckBox   mi_CheckEqual;
        Label      mi_LblInfo;
        XYView     mi_View;

        /// <summary>
        /// Implementation of interface IOperation
        /// </summary>
        public void GetMenuItems(Channel i_Channel, bool b_Analog, List<GraphMenuItem> i_Items)
        {
            if (i_Channel == null || !b_Analog || i_Channel.mf_Analog == null)
                return; // the user did not click on an analog channel

            if (OsziPanel.CurCapture.ms32_AnalogCount < 2)
                return; // X/Y requires 2 analog channels

            GraphMenuItem i_Item = new GraphMenuItem();
            i_Item.ms_MenuText  = "X/Y Plot";
            i_Item.ms_ImageFile = "ArrowUpDown.ico";
            i_Items.Add(i_Item);
        }

        /// <summary>
        /// Implementation of interface IOperation
        /// </summary>
        public String Execute(Channel i_Channel, int s32_Sample, bool b_Analog, Object o_Tag)
        {
            Capture i_Capture = OsziPanel.CurCapture;
            ms32_Samples  = i_Capture.ms32_Samples;
            ms32_VisStart = Math.Max(0, Utils.OsziPanel.DispStart);
            ms32_VisEnd   = Math.Min(ms32_Samples - 1, Utils.OsziPanel.DispEnd);

            CreateControls();

            // X = the channel that the user has clicked, Y = the next analog channel
            List<Channel> i_Analog = new List<Channel>();
            foreach (Channel i_Chan in i_Capture.mi_Channels)
            {
                if (i_Chan.mf_Analog != null)
                    i_Analog.Add(i_Chan);
            }
            foreach (Channel i_Chan in i_Analog)
            {
                mi_ComboX.Items.Add(i_Chan);
                mi_ComboY.Items.Add(i_Chan);
            }

            int s32_X = Math.Max(0, i_Analog.IndexOf(i_Channel));
            mi_ComboX.SelectedIndex = s32_X;
            mi_ComboY.SelectedIndex = (s32_X + 1) % i_Analog.Count;

            mi_ComboX    .SelectedIndexChanged += new EventHandler(OnSettingChanged);
            mi_ComboY    .SelectedIndexChanged += new EventHandler(OnSettingChanged);
            mi_ComboRange.SelectedIndexChanged += new EventHandler(OnSettingChanged);
            mi_CheckLines.CheckedChanged       += new EventHandler(OnSettingChanged);
            mi_CheckEqual.CheckedChanged       += new EventHandler(OnSettingChanged);

            Show(Utils.FormMain); // not modal
            Calculate();
            return null;
        }

        void CreateControls()
        {
            Text          = "X/Y Plot";
            Icon          = Utils.FormMain.Icon;
            BackColor     = Color.DimGray;
            ForeColor     = Color.White;
            ClientSize    = new Size(700, 640);
            MinimumSize   = new Size(450, 400);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;

            FlowLayoutPanel i_Bar = new FlowLayoutPanel();
            i_Bar.Dock    = DockStyle.Top;
            i_Bar.Height  = 32;
            i_Bar.Padding = new Padding(4, 4, 4, 0);

            mi_ComboX     = AddCombo(i_Bar, "X:", 100);
            mi_ComboY     = AddCombo(i_Bar, "Y:", 100);
            mi_ComboRange = AddCombo(i_Bar, "Range:", 110);
            mi_ComboRange.Items.AddRange(new Object[] { RANGE_ALL, RANGE_VISIBLE });
            mi_ComboRange.SelectedIndex = 0;

            mi_CheckLines = AddCheck(i_Bar, "Connect samples", true);
            mi_CheckEqual = AddCheck(i_Bar, "Same Volt/Div",   true);

            mi_LblInfo = new Label();
            mi_LblInfo.Dock      = DockStyle.Bottom;
            mi_LblInfo.Height    = 22;
            mi_LblInfo.TextAlign = ContentAlignment.MiddleLeft;
            mi_LblInfo.Padding   = new Padding(4, 0, 0, 0);

            mi_View = new XYView();
            mi_View.Dock = DockStyle.Fill;

            Controls.Add(mi_View); // Fill must be added first
            Controls.Add(mi_LblInfo);
            Controls.Add(i_Bar);
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

        void Calculate()
        {
            Channel i_ChanX = (Channel)mi_ComboX.SelectedItem;
            Channel i_ChanY = (Channel)mi_ComboY.SelectedItem;
            if (i_ChanX == null || i_ChanY == null)
                return;

            bool b_Visible = mi_ComboRange.Text == RANGE_VISIBLE;
            int  s32_Start = b_Visible ? ms32_VisStart : 0;
            int  s32_End   = b_Visible ? ms32_VisEnd   : ms32_Samples - 1;

            Text = "X/Y Plot  —  X: " + i_ChanX.ms_Name + "   Y: " + i_ChanY.ms_Name;
            mi_View.SetData(i_ChanX.mf_Analog, i_ChanY.mf_Analog, s32_Start, s32_End,
                            mi_CheckLines.Checked, mi_CheckEqual.Checked, OsziPanel.GetChannelColor(i_ChanY));

            double d_Corr = Correlation(i_ChanX.mf_Analog, i_ChanY.mf_Analog, s32_Start, s32_End);
            String s_Phase = double.IsNaN(d_Corr) ? "?" : (Math.Acos(d_Corr) * 180 / Math.PI).ToString("0.0", CultureInfo.InvariantCulture) + "°";

            mi_LblInfo.Text = String.Format("Samples: {0:N0}   X: {1} pp   Y: {2} pp   Phase: {3}  (valid only if X and Y have the same frequency, 0...180°)",
                                            s32_End - s32_Start + 1,
                                            SpectrumFFT.FormatVolt(mi_View.RangeX), SpectrumFFT.FormatVolt(mi_View.RangeY), s_Phase);
        }

        /// <summary>
        /// Correlation coefficient of X and Y. For two sine waves of the same frequency this is the cosine of the phase shift.
        /// Returns NaN if one signal is constant.
        /// </summary>
        public static double Correlation(float[] f_X, float[] f_Y, int s32_Start, int s32_End)
        {
            int    s32_Count = s32_End - s32_Start + 1;
            double d_MeanX = 0, d_MeanY = 0;
            for (int S=s32_Start; S<=s32_End; S++)
            {
                d_MeanX += f_X[S];
                d_MeanY += f_Y[S];
            }
            d_MeanX /= s32_Count;
            d_MeanY /= s32_Count;

            double d_XY = 0, d_XX = 0, d_YY = 0;
            for (int S=s32_Start; S<=s32_End; S++)
            {
                double dX = f_X[S] - d_MeanX;
                double dY = f_Y[S] - d_MeanY;
                d_XY += dX * dY;
                d_XX += dX * dX;
                d_YY += dY * dY;
            }
            if (d_XX <= 0 || d_YY <= 0)
                return double.NaN;

            return Math.Max(-1, Math.Min(1, d_XY / Math.Sqrt(d_XX * d_YY)));
        }
    }

    // ==================================================================================================================

    /// <summary>
    /// Draws the X/Y figure like the phosphor of an analog oscilloscope:
    /// The brightness of each pixel depends on how often the trace passes through it.
    /// This is fast even with millions of samples because no GDI line is drawn per sample.
    /// </summary>
    public class XYView : Panel
    {
        const int MARGIN = 55;

        float[] mf_X;
        float[] mf_Y;
        int     ms32_Start;
        int     ms32_End;
        bool    mb_Lines;
        bool    mb_Equal;
        Color   mc_Color;
        double  md_MinX, md_MaxX, md_MinY, md_MaxY; // data range
        double  md_ViewX0, md_ViewY0, md_ScaleX, md_ScaleY; // Volt at the plot's left/bottom, Volt per pixel
        Bitmap  mi_Bitmap;
        Point   mk_Mouse = new Point(-1, -1);

        public double RangeX { get { return md_MaxX - md_MinX; } }
        public double RangeY { get { return md_MaxY - md_MinY; } }

        public XYView()
        {
            DoubleBuffered = true;
            BackColor      = Color.Black;
        }

        public void SetData(float[] f_X, float[] f_Y, int s32_Start, int s32_End, bool b_Lines, bool b_Equal, Color c_Color)
        {
            mf_X       = f_X;
            mf_Y       = f_Y;
            ms32_Start = s32_Start;
            ms32_End   = s32_End;
            mb_Lines   = b_Lines;
            mb_Equal   = b_Equal;
            mc_Color   = c_Color;

            md_MinX = md_MinY = double.MaxValue;
            md_MaxX = md_MaxY = double.MinValue;
            for (int S=s32_Start; S<=s32_End; S++)
            {
                md_MinX = Math.Min(md_MinX, f_X[S]);  md_MaxX = Math.Max(md_MaxX, f_X[S]);
                md_MinY = Math.Min(md_MinY, f_Y[S]);  md_MaxY = Math.Max(md_MaxY, f_Y[S]);
            }
            Render();
        }

        Rectangle PlotRect
        {
            get { return new Rectangle(MARGIN, 10, Math.Max(10, Width - MARGIN - 15), Math.Max(10, Height - 10 - 35)); }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Render();
        }

        /// <summary>
        /// Calculates the scaling and renders the intensity bitmap
        /// </summary>
        void Render()
        {
            if (mf_X == null || Width < 50 || Height < 50)
                return;

            Rectangle r_Plot = PlotRect;
            int W = r_Plot.Width;
            int H = r_Plot.Height;

            // 5% margin, avoid zero range for constant signals
            double d_SpanX = Math.Max(md_MaxX - md_MinX, 1e-6) * 1.1;
            double d_SpanY = Math.Max(md_MaxY - md_MinY, 1e-6) * 1.1;
            md_ScaleX = d_SpanX / W;
            md_ScaleY = d_SpanY / H;
            if (mb_Equal) // circles stay circles
                md_ScaleX = md_ScaleY = Math.Max(md_ScaleX, md_ScaleY);

            md_ViewX0 = (md_MinX + md_MaxX) / 2 - md_ScaleX * W / 2;
            md_ViewY0 = (md_MinY + md_MaxY) / 2 - md_ScaleY * H / 2;

            // Count how often the trace passes through each pixel
            int[] s32_Hits = new int[W * H];
            int s32_PrevX = 0, s32_PrevY = 0;
            for (int S=ms32_Start; S<=ms32_End; S++)
            {
                int X = (int)((mf_X[S] - md_ViewX0) / md_ScaleX);
                int Y = H - 1 - (int)((mf_Y[S] - md_ViewY0) / md_ScaleY);

                if (mb_Lines && S > ms32_Start)
                    AddLine(s32_Hits, W, H, s32_PrevX, s32_PrevY, X, Y);
                else
                    AddPixel(s32_Hits, W, H, X, Y);

                s32_PrevX = X;
                s32_PrevY = Y;
            }

            int s32_MaxHits = 1;
            foreach (int s32_Count in s32_Hits)
                s32_MaxHits = Math.Max(s32_MaxHits, s32_Count);

            // Logarithmic brightness, but single hits must still be clearly visible
            int[] s32_Pixels = new int[W * H];
            double d_LogMax = Math.Log(1 + s32_MaxHits);
            for (int i=0; i<s32_Pixels.Length; i++)
            {
                if (s32_Hits[i] == 0)
                    continue;
                double d_Bright = 0.35 + 0.65 * Math.Log(1 + s32_Hits[i]) / d_LogMax;
                s32_Pixels[i] = Color.FromArgb(0xFF, (int)(mc_Color.R * d_Bright), (int)(mc_Color.G * d_Bright), (int)(mc_Color.B * d_Bright)).ToArgb();
            }

            if (mi_Bitmap != null)
                mi_Bitmap.Dispose();

            mi_Bitmap = new Bitmap(W, H, PixelFormat.Format32bppArgb);
            BitmapData i_Data = mi_Bitmap.LockBits(new Rectangle(0, 0, W, H), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            for (int Y=0; Y<H; Y++)
            {
                Marshal.Copy(s32_Pixels, Y * W, new IntPtr(i_Data.Scan0.ToInt64() + (long)Y * i_Data.Stride), W);
            }
            mi_Bitmap.UnlockBits(i_Data);
            Invalidate();
        }

        static void AddPixel(int[] s32_Hits, int W, int H, int X, int Y)
        {
            if (X >= 0 && X < W && Y >= 0 && Y < H)
                s32_Hits[Y * W + X] ++;
        }

        /// <summary>
        /// DDA line. The end point is not added, because it is the start point of the next line.
        /// </summary>
        static void AddLine(int[] s32_Hits, int W, int H, int X0, int Y0, int X1, int Y1)
        {
            int s32_Steps = Math.Max(Math.Abs(X1 - X0), Math.Abs(Y1 - Y0));
            if (s32_Steps == 0)
            {
                AddPixel(s32_Hits, W, H, X0, Y0);
                return;
            }
            s32_Steps = Math.Min(s32_Steps, 4 * (W + H)); // protection against extreme values
            double dX = (double)(X1 - X0) / s32_Steps;
            double dY = (double)(Y1 - Y0) / s32_Steps;
            for (int i=0; i<s32_Steps; i++)
            {
                AddPixel(s32_Hits, W, H, (int)Math.Round(X0 + i * dX), (int)Math.Round(Y0 + i * dY));
            }
        }

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
            if (mi_Bitmap == null)
                return;

            Graphics  g      = e.Graphics;
            Rectangle r_Plot = PlotRect;
            int W = r_Plot.Width;
            int H = r_Plot.Height;

            using (Pen   i_Grid  = new Pen(Color.FromArgb(0x50, 0x50, 0x50)))
            using (Pen   i_Zero  = new Pen(Color.FromArgb(0x90, 0x90, 0x90)))
            using (Brush i_Text  = new SolidBrush(Color.FromArgb(0xC0, 0xC0, 0xC0)))
            using (Font  i_Font  = new Font("Segoe UI", 8))
            {
                // Vertical grid lines with X voltage labels
                double d_StepX = NiceStep(md_ScaleX * W, 8);
                for (double V = Math.Ceiling(md_ViewX0 / d_StepX) * d_StepX; V <= md_ViewX0 + md_ScaleX * W; V += d_StepX)
                {
                    float X = (float)(r_Plot.Left + (V - md_ViewX0) / md_ScaleX);
                    g.DrawLine(Math.Abs(V) < d_StepX * 1e-6 ? i_Zero : i_Grid, X, r_Plot.Top, X, r_Plot.Bottom);
                    String s_Label = SpectrumFFT.FormatVolt(Math.Abs(V) < d_StepX * 1e-6 ? 0 : V);
                    SizeF  k_Size  = g.MeasureString(s_Label, i_Font);
                    g.DrawString(s_Label, i_Font, i_Text, X - k_Size.Width / 2, r_Plot.Bottom + 3);
                }

                // Horizontal grid lines with Y voltage labels
                double d_StepY = NiceStep(md_ScaleY * H, 8);
                for (double V = Math.Ceiling(md_ViewY0 / d_StepY) * d_StepY; V <= md_ViewY0 + md_ScaleY * H; V += d_StepY)
                {
                    float Y = (float)(r_Plot.Bottom - (V - md_ViewY0) / md_ScaleY);
                    g.DrawLine(Math.Abs(V) < d_StepY * 1e-6 ? i_Zero : i_Grid, r_Plot.Left, Y, r_Plot.Right, Y);
                    String s_Label = SpectrumFFT.FormatVolt(Math.Abs(V) < d_StepY * 1e-6 ? 0 : V);
                    SizeF  k_Size  = g.MeasureString(s_Label, i_Font);
                    g.DrawString(s_Label, i_Font, i_Text, r_Plot.Left - k_Size.Width - 3, Y - k_Size.Height / 2);
                }

                g.DrawImageUnscaled(mi_Bitmap, r_Plot.Left, r_Plot.Top);

                using (Pen i_Frame = new Pen(Color.FromArgb(0x90, 0x90, 0x90)))
                    g.DrawRectangle(i_Frame, r_Plot);

                g.DrawString("X", i_Font, i_Text, r_Plot.Right - 10, r_Plot.Bottom + 16);
                g.DrawString("Y", i_Font, i_Text, 4, r_Plot.Top);

                if (r_Plot.Contains(mk_Mouse))
                {
                    double d_X = md_ViewX0 + (mk_Mouse.X - r_Plot.Left)   * md_ScaleX;
                    double d_Y = md_ViewY0 + (r_Plot.Bottom - mk_Mouse.Y) * md_ScaleY;
                    g.DrawString("X: " + SpectrumFFT.FormatVolt(d_X) + "    Y: " + SpectrumFFT.FormatVolt(d_Y),
                                 i_Font, Brushes.White, r_Plot.Left + 4, r_Plot.Top + 4);
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
