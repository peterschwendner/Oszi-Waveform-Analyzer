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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using IOperation        = Operations.OperationManager.IOperation;
using GraphMenuItem     = Operations.OperationManager.GraphMenuItem;
using IXYStream         = Transfer.IXYStream;
using AudioOutput       = Transfer.AudioOutput;
using FormLiveXY        = Transfer.FormLiveXY;
using Utils             = OsziWaveformAnalyzer.Utils;
using OsziPanel         = OsziWaveformAnalyzer.OsziPanel;
using Capture           = OsziWaveformAnalyzer.Utils.Capture;
using Channel           = OsziWaveformAnalyzer.Utils.Channel;

namespace Operations
{
    /// <summary>
    /// Plays a capture (e.g. an imported WAV file or an audio recording) like a live signal
    /// for the Waterfall FFT or the Live X/Y display.
    /// The playback runs in real time or with another speed. "Max" (Waterfall FFT only) calculates as fast as possible.
    ///
    /// Sound: WAV files and audio recordings are played on the audio output at speed 1 x.
    /// Then the played samples of the audio output are the clock of the display (synchronous picture and sound).
    /// Without sound the clock is a Stopwatch.
    /// </summary>
    public class CaptureStream : IWaterfallSource, IXYStream
    {
        static readonly String[] SPEEDS_XY = { "0.001 x", "0.01 x", "0.1 x", "0.25 x", "0.5 x", "1 x", "2 x", "5 x", "10 x" };
        static readonly String[] SPEEDS_WF = { "0.25 x", "0.5 x", "1 x", "2 x", "5 x", "10 x", "Max" };
        static readonly int[]    BLOCKS    = { 1024, 2048, 4096, 8192, 16384, 32768, 65536 };

        Capture       mi_Capture;
        List<Channel> mi_Analog = new List<Channel>();
        double        md_Rate;
        bool          mb_ForXY;
        int           ms32_StartX;       // XY: channel that the user has right-clicked

        ComboBox      mi_ComboSpeed;
        ComboBox      mi_ComboBlock;
        ComboBox      mi_ComboX;
        ComboBox      mi_ComboY;
        CheckBox      mi_CheckLoop;
        CheckBox      mi_CheckSound;

        AudioOutput   mi_Output = new AudioOutput();
        bool          mb_SoundOn;        // the audio output is the clock
        bool          mb_Playing;        // between Start() and Stop()
        String        ms_SoundError;
        long          ms64_OutPos;       // next sample of the audio output (background thread)
        volatile bool mb_OutLoop;
        int           ms32_OutLeft;      // index in mi_Analog
        int           ms32_OutRight;
        float         mf_SoundGain = 1;  // < 1 if the samples exceed full scale (e.g. Volt)
        String        ms_NoSound;        // the reason why the capture cannot be played, null = OK

        // The Waterfall FFT does not disable these controls while it runs
        const String KEEP_ENABLED = "KeepEnabled";

        long          ms64_Pos;          // next sample to play
        double        md_Played;         // seconds played since the start (also over loops)
        double        md_BlockTime;      // Waterfall: time of the last block
        Stopwatch     mi_Watch = new Stopwatch();
        double        md_Anchor;         // samples that should have been played at mi_Watch = 0
        long          ms64_Skipped;
        bool          mb_Abort;
        bool          mb_End;

        /// <summary>
        /// i_Clicked = the channel that the user has right-clicked (default channel)
        /// b_ForXY   = true: Live X/Y, false: Waterfall FFT
        /// </summary>
        public CaptureStream(Capture i_Capture, Channel i_Clicked, bool b_ForXY)
        {
            mi_Capture = i_Capture;
            mb_ForXY   = b_ForXY;
            md_Rate    = (double)Utils.PICOS_PER_SECOND / i_Capture.ms64_SampleDist;

            foreach (Channel i_Chan in i_Capture.mi_Channels)
            {
                if (i_Chan.mf_Analog != null)
                    mi_Analog.Add(i_Chan);
            }
            ms32_StartX = Math.Max(0, mi_Analog.IndexOf(i_Clicked));

            // Any capture with an audio sample rate can be played (a WAV file, an audio recording, also saved as OSZI file).
            // Samples above full scale (e.g. Volt) are attenuated, so that the sound is not clipped.
            if (md_Rate < 8000 || md_Rate > 192000)
            {
                ms_NoSound = "only 8 ... 192 kHz";
            }
            else
            {
                float f_Peak = 0;
                foreach (Channel i_Chan in mi_Analog)
                {
                    foreach (float f_Value in i_Chan.mf_Analog)
                        f_Peak = Math.Max(f_Peak, Math.Abs(f_Value));
                }
                if (f_Peak > 1)
                    mf_SoundGain = 1 / f_Peak;
            }
        }

        public int StartChannel
        {
            get { return ms32_StartX; }
        }

        String FileName
        {
            get { return mi_Capture.ms_Path != null ? Path.GetFileName(mi_Capture.ms_Path) : "Capture in the main window"; }
        }

        double TotalSeconds
        {
            get { return mi_Capture.ms32_Samples / md_Rate; }
        }

        /// <summary>
        /// returns 0 for "Max"
        /// </summary>
        double Speed
        {
            get
            {
                String[] s_Parts = mi_ComboSpeed.Text.Split(' ');
                return s_Parts.Length == 2 ? double.Parse(s_Parts[0], CultureInfo.InvariantCulture) : 0;
            }
        }

        public void AddControls(FlowLayoutPanel i_Bar)
        {
            if (mb_ForXY)
            {
                mi_ComboX = AddCombo(i_Bar, "X:", 80);
                mi_ComboY = AddCombo(i_Bar, "Y:", 80);
                foreach (Channel i_Chan in mi_Analog)
                {
                    mi_ComboX.Items.Add(i_Chan.ms_Name);
                    mi_ComboY.Items.Add(i_Chan.ms_Name);
                }
                mi_ComboX.SelectedIndex = ms32_StartX;
                mi_ComboY.SelectedIndex = (ms32_StartX + 1) % mi_Analog.Count;
            }
            else
            {
                mi_ComboBlock = AddCombo(i_Bar, "Samples:", 65);
                foreach (int s32_Block in BLOCKS)
                    mi_ComboBlock.Items.Add(s32_Block.ToString());
                mi_ComboBlock.Text = "8192";
            }

            mi_ComboSpeed = AddCombo(i_Bar, "Speed:", 65);
            mi_ComboSpeed.Items.AddRange(mb_ForXY ? SPEEDS_XY : SPEEDS_WF);
            mi_ComboSpeed.Text = "1 x";
            mi_ComboSpeed.SelectedIndexChanged += delegate { Restart(); };
            mi_ComboSpeed.Tag = KEEP_ENABLED;

            mi_CheckLoop = new CheckBox();
            mi_CheckLoop.Text     = "Loop";
            mi_CheckLoop.AutoSize = true;
            mi_CheckLoop.Checked  = mb_ForXY;
            mi_CheckLoop.Margin   = new Padding(8, 4, 0, 0);
            mi_CheckLoop.CheckedChanged += delegate { mb_OutLoop = mi_CheckLoop.Checked; };
            mi_CheckLoop.Tag = KEEP_ENABLED;
            i_Bar.Controls.Add(mi_CheckLoop);
            mb_OutLoop = mi_CheckLoop.Checked;

            // Only signals with an audio sample rate can be played (not an oscilloscope capture with MHz sample rate)
            mi_CheckSound = new CheckBox();
            mi_CheckSound.Text     = CanPlaySound ? "Sound" : "Sound (" + ms_NoSound + ")";
            mi_CheckSound.AutoSize = true;
            mi_CheckSound.Enabled  = CanPlaySound;
            mi_CheckSound.Checked  = CanPlaySound;
            mi_CheckSound.Tag      = CanPlaySound ? KEEP_ENABLED : null;
            mi_CheckSound.Margin   = new Padding(8, 4, 0, 0);
            mi_CheckSound.CheckedChanged += delegate { Restart(); };
            i_Bar.Controls.Add(mi_CheckSound);
        }

        bool CanPlaySound
        {
            get { return ms_NoSound == null; }
        }

        static ComboBox AddCombo(FlowLayoutPanel i_Bar, String s_Label, int s32_Width)
        {
            Label i_Label = new Label();
            i_Label.Text     = s_Label;
            i_Label.AutoSize = true;
            i_Label.Margin   = new Padding(8, 5, 0, 0);
            i_Bar.Controls.Add(i_Label);

            ComboBox i_Combo = new ComboBox();
            i_Combo.DropDownStyle = ComboBoxStyle.DropDownList;
            i_Combo.Width         = s32_Width;
            i_Bar.Controls.Add(i_Combo);
            return i_Combo;
        }

        /// <summary>
        /// The clock starts again at the current position (after Start or a change of the speed or of the sound)
        /// </summary>
        void Restart()
        {
            StopSound();
            md_Anchor = ms64_Pos;
            mi_Watch.Reset();
            mi_Watch.Start();

            // (The Waterfall FFT disables the controls while it runs: do not check mi_CheckSound.Enabled)
            if (mb_Playing && mi_CheckSound != null && mi_CheckSound.Checked && CanPlaySound && Speed == 1)
                StartSound();
        }

        /// <summary>
        /// The position that should have been reached at the current time
        /// </summary>
        double TargetPos
        {
            get
            {
                if (mb_SoundOn)
                    return md_Anchor + mi_Output.PlayedSamples;
                return md_Anchor + mi_Watch.Elapsed.TotalSeconds * md_Rate * Speed;
            }
        }

        // ===================================== Sound =====================================

        void StartSound()
        {
            // Waterfall: the first two channels (a stereo file), Live X/Y: the channels X and Y
            if (mb_ForXY)
            {
                ms32_OutLeft  = mi_ComboX.SelectedIndex;
                ms32_OutRight = mi_ComboY.SelectedIndex;
            }
            else
            {
                ms32_OutLeft  = 0;
                ms32_OutRight = Math.Min(1, mi_Analog.Count - 1); // mono --> both sides
            }
            ms64_OutPos   = ms64_Pos;
            ms_SoundError = null;
            try
            {
                mi_Output.Start(SampleRate, FillOutput);
                mb_SoundOn = true;
            }
            catch (Exception Ex)
            {
                ms_SoundError = Ex.Message;
                mb_SoundOn    = false;
            }
        }

        void StopSound()
        {
            if (mb_SoundOn)
            {
                // The clock continues with the Stopwatch at the position that has been played
                md_Anchor += mi_Output.PlayedSamples;
                mi_Output.Stop();
                mb_SoundOn = false;
            }
        }

        /// <summary>
        /// Called in the background thread of the audio output
        /// </summary>
        int FillOutput(float[] f_Left, float[] f_Right, int s32_Count)
        {
            float[] f_SrcL  = mi_Analog[ms32_OutLeft] .mf_Analog;
            float[] f_SrcR  = mi_Analog[ms32_OutRight].mf_Analog;
            int     s32_Len = mi_Capture.ms32_Samples;
            for (int S=0; S<s32_Count; S++)
            {
                if (ms64_OutPos >= s32_Len)
                {
                    if (!mb_OutLoop)
                        return S; // the rest is silence
                    ms64_OutPos = 0;
                }
                f_Left [S] = f_SrcL[ms64_OutPos] * mf_SoundGain;
                f_Right[S] = f_SrcR[ms64_OutPos] * mf_SoundGain;
                ms64_OutPos ++;
            }
            return s32_Count;
        }

        String PositionInfo
        {
            get
            {
                String s_Info = String.Format(CultureInfo.InvariantCulture, "{0}   {1:0.0} s / {2:0.0} s",
                                              FileName, ms64_Pos / md_Rate, TotalSeconds);
                if (mb_End)
                    s_Info += "   End of file";
                if (mb_SoundOn)
                    s_Info += "   Sound";
                if (ms_SoundError != null)
                    s_Info += "   " + ms_SoundError;
                return s_Info;
            }
        }

        // ===================================== IWaterfallSource =====================================

        public String Name
        {
            get { return FileName; }
        }

        public String[] Channels
        {
            get
            {
                List<String> i_Names = new List<String>();
                foreach (Channel i_Chan in mi_Analog)
                    i_Names.Add(i_Chan.ms_Name);
                return i_Names.ToArray();
            }
        }

        public Fourier.eUnit Unit
        {
            get { return mi_Capture.mb_FullScale ? Fourier.eUnit.FullScale : Fourier.eUnit.Volt; }
        }

        public String StepInfo
        {
            get { return PositionInfo; }
        }

        public double BlockTime
        {
            get { return md_BlockTime; }
        }

        public void Start()
        {
            mb_Abort   = false;
            mb_Playing = true;
            if (mb_End || ms64_Pos >= mi_Capture.ms32_Samples)
            {
                ms64_Pos = 0; // play again from the beginning
                mb_End   = false;
            }
            Restart();
        }

        /// <summary>
        /// Returns the next block of the channel. Waits until it has been "played" (real time).
        /// At the end of the capture: continues at the beginning (Loop) or returns null.
        /// </summary>
        public Capture Acquire(int s32_Channel)
        {
            mb_Abort = false;
            int s32_Block = Math.Min(int.Parse(mi_ComboBlock.Text), mi_Capture.ms32_Samples);

            if (ms64_Pos + s32_Block > mi_Capture.ms32_Samples)
            {
                if (!mi_CheckLoop.Checked)
                {
                    mb_End = true;
                    return null;
                }
                // The clock (and the sound) continues: the beginning of the capture is reached at TargetPos = length
                md_Anchor -= mi_Capture.ms32_Samples;
                ms64_Pos   = 0;
            }

            // Wait in real time (not for "Max")
            if (Speed > 0)
            {
                while (TargetPos < ms64_Pos + s32_Block)
                {
                    Application.DoEvents();
                    if (mb_Abort)
                        return null;
                    Thread.Sleep(5);
                }
            }

            float[] f_Source = mi_Analog[s32_Channel - 1].mf_Analog;
            float[] f_Block  = new float[s32_Block];
            Array.Copy(f_Source, (int)ms64_Pos, f_Block, 0, s32_Block);

            md_BlockTime = md_Played;
            md_Played   += s32_Block / md_Rate;
            ms64_Pos    += s32_Block;

            Channel i_Channel = new Channel(mi_Analog[s32_Channel - 1].ms_Name);
            i_Channel.mf_Analog = f_Block;

            Capture i_Capture = new Capture();
            i_Capture.mb_FullScale    = mi_Capture.mb_FullScale;
            i_Capture.ms32_AnalogRes  = mi_Capture.ms32_AnalogRes;
            i_Capture.ms32_Samples    = s32_Block;
            i_Capture.ms64_SampleDist = mi_Capture.ms64_SampleDist;
            i_Capture.mi_Channels.Add(i_Channel);
            return i_Capture;
        }

        public void Abort()
        {
            mb_Abort = true;
        }

        public void Stop()
        {
            mb_Playing = false;
            StopSound();
            mi_Watch.Stop();
        }

        // ===================================== IXYStream =====================================

        public int SampleRate
        {
            get { return (int)Math.Round(md_Rate); }
        }

        public double SamplesPerSecond
        {
            get { return md_Rate * Math.Max(Speed, 1e-6); }
        }

        public long SkippedSamples
        {
            get { return ms64_Skipped; }
        }

        public String StatusInfo
        {
            get { return PositionInfo; }
        }

        public bool CanSnapshot
        {
            get { return false; } // the data is already in the main window
        }

        public void StartStream()
        {
            Start();
        }

        /// <summary>
        /// Returns all samples that have been "played" since the last call
        /// </summary>
        public int ReadStream(float[] f_X, float[] f_Y, out bool b_Skipped)
        {
            b_Skipped = false;
            long s64_Target = (long)TargetPos;
            long s64_Count  = s64_Target - ms64_Pos;
            if (s64_Count <= 0)
                return 0;

            // The display is too slow or the speed is very high: skip the oldest samples
            if (s64_Count > f_X.Length)
            {
                ms64_Skipped += s64_Count - f_X.Length;
                ms64_Pos      = s64_Target - f_X.Length;
                s64_Count     = f_X.Length;
                b_Skipped     = true;
            }

            float[] f_SrcX  = mi_Analog[mi_ComboX.SelectedIndex].mf_Analog;
            float[] f_SrcY  = mi_Analog[mi_ComboY.SelectedIndex].mf_Analog;
            int     s32_Len = mi_Capture.ms32_Samples;
            int     s32_Out = 0;
            for (long P=ms64_Pos; P<ms64_Pos + s64_Count; P++)
            {
                long s64_Index = P;
                if (s64_Index >= s32_Len)
                {
                    if (!mi_CheckLoop.Checked)
                    {
                        mb_End = true;
                        break;
                    }
                    s64_Index %= s32_Len;
                }
                f_X[s32_Out] = f_SrcX[s64_Index];
                f_Y[s32_Out] = f_SrcY[s64_Index];
                s32_Out ++;
            }
            ms64_Pos += s64_Count;

            // Loop: keep the position small, the real time clock continues
            if (mi_CheckLoop.Checked && ms64_Pos >= s32_Len)
            {
                ms64_Pos  -= s32_Len;
                md_Anchor -= s32_Len;
            }
            else if (!mi_CheckLoop.Checked && ms64_Pos >= s32_Len)
            {
                ms64_Pos = s32_Len;
                mb_End   = true;
            }
            return s32_Out;
        }

        public void StopStream()
        {
            Stop();
        }

        public Capture GetLastSamples(int s32_Count)
        {
            return null;
        }
    }

    // ==================================================================================================================

    /// <summary>
    /// Right-click menu: plays the capture of the main window in the Waterfall FFT
    /// </summary>
    public class PlayWaterfall : IOperation
    {
        public void GetMenuItems(Channel i_Channel, bool b_Analog, List<GraphMenuItem> i_Items)
        {
            if (i_Channel == null || !b_Analog || i_Channel.mf_Analog == null)
                return;

            GraphMenuItem i_Item = new GraphMenuItem();
            i_Item.ms_MenuText  = "Waterfall FFT (play)";
            i_Item.ms_ImageFile = "Filter.ico";
            i_Items.Add(i_Item);
        }

        public String Execute(Channel i_Channel, int s32_Sample, bool b_Analog, Object o_Tag)
        {
            Capture i_Capture = OsziPanel.CurCapture;
            if (i_Capture.ms64_SampleDist <= 0)
                return "Error: The capture has no valid sample rate.";

            CaptureStream i_Stream = new CaptureStream(i_Capture, i_Channel, false);
            WaterfallFFT  i_Form   = new WaterfallFFT(i_Stream);
            i_Form.SelectChannel(i_Stream.StartChannel);
            i_Form.Show(Utils.FormMain); // not modal
            return null;
        }
    }

    /// <summary>
    /// Right-click menu: plays two channels of the capture of the main window in the Live X/Y display
    /// </summary>
    public class PlayLiveXY : IOperation
    {
        public void GetMenuItems(Channel i_Channel, bool b_Analog, List<GraphMenuItem> i_Items)
        {
            if (i_Channel == null || !b_Analog || i_Channel.mf_Analog == null)
                return;

            if (OsziPanel.CurCapture.ms32_AnalogCount < 2)
                return; // X/Y requires 2 analog channels

            GraphMenuItem i_Item = new GraphMenuItem();
            i_Item.ms_MenuText  = "Live X/Y (play)";
            i_Item.ms_ImageFile = "ArrowUpDown.ico";
            i_Items.Add(i_Item);
        }

        public String Execute(Channel i_Channel, int s32_Sample, bool b_Analog, Object o_Tag)
        {
            Capture i_Capture = OsziPanel.CurCapture;
            if (i_Capture.ms64_SampleDist <= 0)
                return "Error: The capture has no valid sample rate.";

            CaptureStream i_Stream = new CaptureStream(i_Capture, i_Channel, true);
            FormLiveXY    i_Form   = new FormLiveXY(i_Stream, !i_Capture.mb_FullScale, 1.0, 1.0);
            i_Form.Show(Utils.FormMain); // not modal
            return null;
        }
    }
}
