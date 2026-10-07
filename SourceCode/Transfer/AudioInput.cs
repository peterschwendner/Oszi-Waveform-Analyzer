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
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using IWaterfallSource  = Operations.IWaterfallSource;
using Fourier           = Operations.Fourier;
using Capture           = OsziWaveformAnalyzer.Utils.Capture;
using Channel           = OsziWaveformAnalyzer.Utils.Channel;
using eRegKey           = OsziWaveformAnalyzer.Utils.eRegKey;
using Utils             = OsziWaveformAnalyzer.Utils;

// Live input from an audio interface or sound card (e.g. Focusrite Scarlett, MOTU M2/M4) for the Waterfall FFT.
// Uses the Windows Multimedia API (waveIn) of winmm.dll which is available on all Windows versions without additional libraries.
// Windows converts the format of the device into the requested format (32 bit float, which keeps the full 24 bit resolution).
//
// The recording runs continuously: the driver fills a chain of small buffers, which a background thread copies into a ring buffer.
// The thread is independent of the GUI: a slow display (e.g. the main window painting millions of samples) does not cause gaps.
// Each Acquire() returns the next block of samples, so consecutive spectra have no gaps.
// If the computer is too slow to display every block, the oldest blocks are skipped (the waterfall stays in real time).
//
// The values are relative to full scale: 1.0 = 0 dBFS. They are not calibrated in Volt.
namespace Transfer
{
    public class AudioInput : IWaterfallSource
    {
        #region winmm.dll

        const int  MMSYSERR_NOERROR = 0;
        const int  WAVERR_BADFORMAT = 32;
        const int  CALLBACK_NULL    = 0;
        const int  WHDR_DONE        = 0x01;
        const int  WAVE_MAPPER      = -1;
        const UInt16 WAVE_FORMAT_PCM        = 1;
        const UInt16 WAVE_FORMAT_EXTENSIBLE = 0xFFFE;

        // KSDATAFORMAT_SUBTYPE_IEEE_FLOAT
        static readonly Guid SUBTYPE_FLOAT = new Guid("00000003-0000-0010-8000-00aa00389b71");

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct WAVEINCAPS
        {
            public UInt16 wMid;
            public UInt16 wPid;
            public UInt32 vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public String szPname;
            public UInt32 dwFormats;
            public UInt16 wChannels;
            public UInt16 wReserved1;
        }

        // WAVEFORMATEXTENSIBLE (40 byte). With wFormatTag = WAVE_FORMAT_PCM and cbSize = 0 it is used as WAVEFORMATEX.
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        struct WAVEFORMAT
        {
            public UInt16 wFormatTag;
            public UInt16 nChannels;
            public UInt32 nSamplesPerSec;
            public UInt32 nAvgBytesPerSec;
            public UInt16 nBlockAlign;
            public UInt16 wBitsPerSample;
            public UInt16 cbSize;
            public UInt16 wValidBitsPerSample;
            public UInt32 dwChannelMask;
            public Guid   SubFormat;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct WAVEHDR
        {
            public IntPtr lpData;
            public UInt32 dwBufferLength;
            public UInt32 dwBytesRecorded;
            public IntPtr dwUser;
            public UInt32 dwFlags;
            public UInt32 dwLoops;
            public IntPtr lpNext;
            public IntPtr reserved;
        }

        [DllImport("winmm.dll")]
        static extern int waveInGetNumDevs();
        [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
        static extern int waveInGetDevCaps(IntPtr u_DeviceID, ref WAVEINCAPS k_Caps, int s32_Size);
        [DllImport("winmm.dll")]
        static extern int waveInOpen(out IntPtr h_WaveIn, IntPtr u_DeviceID, ref WAVEFORMAT k_Format, IntPtr p_Callback, IntPtr p_Instance, int s32_Flags);
        [DllImport("winmm.dll")]
        static extern int waveInPrepareHeader(IntPtr h_WaveIn, IntPtr p_Header, int s32_Size);
        [DllImport("winmm.dll")]
        static extern int waveInUnprepareHeader(IntPtr h_WaveIn, IntPtr p_Header, int s32_Size);
        [DllImport("winmm.dll")]
        static extern int waveInAddBuffer(IntPtr h_WaveIn, IntPtr p_Header, int s32_Size);
        [DllImport("winmm.dll")]
        static extern int waveInStart(IntPtr h_WaveIn);
        [DllImport("winmm.dll")]
        static extern int waveInReset(IntPtr h_WaveIn);
        [DllImport("winmm.dll")]
        static extern int waveInClose(IntPtr h_WaveIn);
        [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
        static extern int waveInGetErrorText(int s32_Error, StringBuilder s_Text, int s32_Size);

        #endregion

        const int BUFFER_COUNT = 50;  // driver buffers (1 second)
        const int BUFFER_MS    = 20;  // milliseconds per driver buffer

        public static readonly int[] SAMPLE_RATES = { 44100, 48000, 96000, 192000 };
        static readonly int[] BLOCK_SIZES   = { 2048, 4096, 8192, 16384, 32768, 65536 };

        ComboBox  mi_ComboDevice;
        ComboBox  mi_ComboRate;
        ComboBox  mi_ComboBlock;

        IntPtr    mh_WaveIn;
        IntPtr[]  mp_Headers;
        int       ms32_HdrSize;
        int       ms32_NextBuffer;   // the buffers are completed in the order they were added
        bool      mb_Float;
        int       ms32_Rate;
        int       ms32_Block;
        bool      mb_Abort;

        float[][] mf_Ring;           // [channel][sample] the last samples
        long      ms64_Written;      // total samples per channel written into the ring buffer
        long      ms64_BlockEnd;     // end of the block returned by the last Acquire()
        long      ms64_Skipped;      // samples skipped because the display was too slow

        Thread    mi_Thread;         // copies the driver buffers into the ring buffer
        volatile bool mb_StopThread;
        volatile String ms_ThreadError;
        Object    mi_Lock = new Object(); // protects mf_Ring and ms64_Written

        long Written
        {
            get { lock (mi_Lock) return ms64_Written; }
        }

        // ===================================== IWaterfallSource =====================================

        public String Name
        {
            get
            {
                String s_Device = (mi_ComboDevice != null) ? mi_ComboDevice.Text : "";
                return "Audio: " + s_Device;
            }
        }

        public String[] Channels
        {
            get { return new String[] { "Left", "Right" }; }
        }

        public Fourier.eUnit Unit
        {
            get { return Fourier.eUnit.FullScale; }
        }

        /// <summary>
        /// Status of the last step for the status bar of the waterfall
        /// </summary>
        public String StepInfo
        {
            get
            {
                if (ms64_Skipped == 0)
                    return "Gapless";
                return String.Format(CultureInfo.InvariantCulture, "Skipped: {0:0.0} s (display too slow)", (double)ms64_Skipped / ms32_Rate);
            }
        }

        /// <summary>
        /// Device, sample rate and block size in the toolbar of the waterfall
        /// </summary>
        public void AddControls(FlowLayoutPanel i_Bar)
        {
            mi_ComboDevice = AddCombo(i_Bar, "Device:", 200);
            mi_ComboDevice.Items.Add("Windows default");
            foreach (String s_Name in EnumerateDevices())
                mi_ComboDevice.Items.Add(s_Name);
            Utils.ComboAdjustDropDownWidth(mi_ComboDevice);

            mi_ComboRate = AddCombo(i_Bar, "Rate:", 70);
            foreach (int s32_Rate in SAMPLE_RATES)
                mi_ComboRate.Items.Add(s32_Rate.ToString());

            mi_ComboBlock = AddCombo(i_Bar, "Samples:", 65);
            foreach (int s32_Size in BLOCK_SIZES)
                mi_ComboBlock.Items.Add(s32_Size.ToString());

            // Stored as "Device name|Rate|Samples"
            String[] s_Settg = Utils.RegReadString(eRegKey.AudioInput, "Windows default|48000|8192").Split('|');
            mi_ComboDevice.Text = s_Settg[0];
            if (mi_ComboDevice.SelectedIndex < 0) mi_ComboDevice.SelectedIndex = 0;
            mi_ComboRate.Text   = s_Settg.Length > 1 ? s_Settg[1] : "48000";
            if (mi_ComboRate.SelectedIndex < 0)   mi_ComboRate.SelectedIndex = 1;
            mi_ComboBlock.Text  = s_Settg.Length > 2 ? s_Settg[2] : "8192";
            if (mi_ComboBlock.SelectedIndex < 0)  mi_ComboBlock.SelectedIndex = 2;
        }

        /// <summary>
        /// Opens the device and starts the recording. Throws.
        /// </summary>
        public void Start()
        {
            Stop();
            mb_Abort      = false;
            ms32_Rate     = int.Parse(mi_ComboRate.Text);
            ms32_Block    = int.Parse(mi_ComboBlock.Text);
            ms64_Written  = 0;
            ms64_BlockEnd = 0;
            ms64_Skipped  = 0;

            Utils.RegWriteString(eRegKey.AudioInput, mi_ComboDevice.Text + "|" + mi_ComboRate.Text + "|" + mi_ComboBlock.Text);

            // index 0 = "Windows default" --> WAVE_MAPPER
            // Ring buffer for at least 4 blocks or 2 seconds
            Open(mi_ComboDevice.SelectedIndex - 1, mi_ComboDevice.Text, Math.Max(4 * ms32_Block, 2 * ms32_Rate));
        }

        /// <summary>
        /// Opens the device and starts the recording into a ring buffer of s32_RingSize samples per channel.
        /// s32_Device = -1 --> Windows default device. ms32_Rate must be set before.
        /// </summary>
        void Open(int s32_Device, String s_DeviceName, int s32_RingSize)
        {
            IntPtr u_Device = new IntPtr(s32_Device);

            // 32 bit float preserves the 24 bit resolution of the converter. Fallback: 16 bit integer.
            WAVEFORMAT k_Format = CreateFormat(true);
            int s32_Error = waveInOpen(out mh_WaveIn, u_Device, ref k_Format, IntPtr.Zero, IntPtr.Zero, CALLBACK_NULL);
            mb_Float = true;
            if (s32_Error == WAVERR_BADFORMAT)
            {
                k_Format  = CreateFormat(false);
                s32_Error = waveInOpen(out mh_WaveIn, u_Device, ref k_Format, IntPtr.Zero, IntPtr.Zero, CALLBACK_NULL);
                mb_Float  = false;
            }
            if (s32_Error != MMSYSERR_NOERROR)
            {
                mh_WaveIn = IntPtr.Zero;
                throw new Exception("Error opening the audio input '" + s_DeviceName + "' with " + ms32_Rate + " Hz:\n"
                                  + GetErrorText(s32_Error));
            }

            mf_Ring = new float[][] { new float[s32_RingSize], new float[s32_RingSize] };

            int s32_BufBytes = ms32_Rate * BUFFER_MS / 1000 * k_Format.nBlockAlign;
            ms32_HdrSize     = Marshal.SizeOf(typeof(WAVEHDR));
            mp_Headers       = new IntPtr[BUFFER_COUNT];
            ms32_NextBuffer  = 0;
            for (int B=0; B<BUFFER_COUNT; B++)
            {
                WAVEHDR k_Hdr = new WAVEHDR();
                k_Hdr.lpData         = Marshal.AllocHGlobal(s32_BufBytes);
                k_Hdr.dwBufferLength = (UInt32)s32_BufBytes;

                mp_Headers[B] = Marshal.AllocHGlobal(ms32_HdrSize);
                Marshal.StructureToPtr(k_Hdr, mp_Headers[B], false);
                Check(waveInPrepareHeader(mh_WaveIn, mp_Headers[B], ms32_HdrSize));
                Check(waveInAddBuffer    (mh_WaveIn, mp_Headers[B], ms32_HdrSize));
            }
            Check(waveInStart(mh_WaveIn));

            mb_StopThread  = false;
            ms_ThreadError = null;
            mi_Thread = new Thread(new ThreadStart(CollectThread));
            mi_Thread.IsBackground = true;
            mi_Thread.Priority     = ThreadPriority.AboveNormal;
            mi_Thread.Start();
        }

        /// <summary>
        /// Waits until the next block of samples has been recorded and returns it as a Capture with one channel.
        /// s32_Channel = 1 (Left) or 2 (Right)
        /// returns null if the user has aborted.
        /// </summary>
        public Capture Acquire(int s32_Channel)
        {
            mb_Abort = false;
            if (mh_WaveIn == IntPtr.Zero)
                throw new Exception("Programming Error: Start() was not called.");

            Stopwatch i_Watch = Stopwatch.StartNew();
            int s32_Timeout = 2000 + 2000 * ms32_Block / ms32_Rate;
            while (Written - ms64_BlockEnd < ms32_Block)
            {
                if (ms_ThreadError != null)
                    throw new Exception(ms_ThreadError);

                if (i_Watch.ElapsedMilliseconds > s32_Timeout)
                    throw new Exception("The audio input does not deliver data.\nIs the device still connected?");

                Application.DoEvents();
                if (mb_Abort)
                    return null;
                Thread.Sleep(5);
            }

            float[] f_Block = new float[ms32_Block];
            lock (mi_Lock)
            {
                // The display is too slow: skip the old blocks, keep only the newest one
                if (ms64_Written - ms64_BlockEnd >= 2 * ms32_Block)
                {
                    long s64_NewEnd = ms64_Written - ms32_Block;
                    ms64_Skipped += s64_NewEnd - ms64_BlockEnd;
                    ms64_BlockEnd = s64_NewEnd;
                }

                float[] f_Ring = mf_Ring[s32_Channel - 1];
                for (int S=0; S<ms32_Block; S++)
                {
                    f_Block[S] = f_Ring[(int)((ms64_BlockEnd + S) % f_Ring.Length)];
                }
            }
            ms64_BlockEnd += ms32_Block;

            Channel i_Channel = new Channel(Channels[s32_Channel - 1]);
            i_Channel.mf_Analog = f_Block;

            Capture i_Capture = new Capture();
            i_Capture.ms32_AnalogRes  = mb_Float ? 24 : 16;
            i_Capture.ms32_Samples    = ms32_Block;
            i_Capture.ms64_SampleDist = (Int64)(Utils.PICOS_PER_SECOND / ms32_Rate);
            i_Capture.mi_Channels.Add(i_Channel);
            return i_Capture;
        }

        public void Abort()
        {
            mb_Abort = true;
        }

        // ===================================== Recording into the main window =====================================

        /// <summary>
        /// Called while recording. returns true to abort.
        /// </summary>
        public delegate bool delProgress(int s32_Recorded, int s32_Total);

        /// <summary>
        /// Records s32_Samples stereo samples and returns them as a Capture with the channels "Left" and "Right".
        /// s32_Device = -1 --> Windows default device
        /// returns null if f_Progress has returned true (abort). Throws on error.
        /// </summary>
        public Capture Record(int s32_Device, String s_DeviceName, int s32_Rate, int s32_Samples, delProgress f_Progress)
        {
            Stop();
            mb_Abort     = false;
            ms32_Rate    = s32_Rate;
            ms64_Written = 0;

            // The first 100 ms are not used: some converters need time to settle after the start
            int s32_Skip = s32_Rate / 10;

            // The ring buffer is larger than the recording: it does not wrap around
            Open(s32_Device, s_DeviceName, s32_Skip + s32_Samples + s32_Rate);
            try
            {
                Stopwatch i_Watch = Stopwatch.StartNew();
                long s64_LastWritten = 0;
                while (true)
                {
                    long s64_Written = Written;
                    if (s64_Written >= s32_Skip + s32_Samples)
                        break;

                    if (ms_ThreadError != null)
                        throw new Exception(ms_ThreadError);

                    if (s64_Written != s64_LastWritten)
                    {
                        s64_LastWritten = s64_Written;
                        i_Watch.Reset();
                        i_Watch.Start();
                    }
                    else if (i_Watch.ElapsedMilliseconds > 2000)
                    {
                        throw new Exception("The audio input does not deliver data.\nIs the device still connected?");
                    }

                    int s32_Done = (int)Math.Max(0, Math.Min(s32_Samples, s64_Written - s32_Skip));
                    if (f_Progress(s32_Done, s32_Samples))
                        return null;

                    Thread.Sleep(10);
                }
            }
            finally
            {
                Stop();
            }

            Capture i_Capture = new Capture();
            i_Capture.ms32_Samples    = s32_Samples;
            i_Capture.ms64_SampleDist = (Int64)Math.Round((double)Utils.PICOS_PER_SECOND / s32_Rate);
            i_Capture.ms32_AnalogRes  = Utils.MAX_ANAL_RES; // 24 bit, but the display uses at most 16 bit

            for (int C=0; C<2; C++)
            {
                float[] f_Data = new float[s32_Samples];
                Array.Copy(mf_Ring[C], s32_Skip, f_Data, 0, s32_Samples);

                Channel i_Channel = new Channel(Channels[C]);
                i_Channel.mf_Analog = f_Data;
                i_Capture.mi_Channels.Add(i_Channel);
            }
            mf_Ring = null; // free memory
            return i_Capture;
        }

        /// <summary>
        /// Stops the recording and closes the device. Does not throw.
        /// </summary>
        public void Stop()
        {
            if (mh_WaveIn == IntPtr.Zero)
                return;

            if (mi_Thread != null)
            {
                mb_StopThread = true;
                mi_Thread.Join();
                mi_Thread = null;
            }

            waveInReset(mh_WaveIn); // marks all buffers as done
            foreach (IntPtr p_Hdr in mp_Headers)
            {
                if (p_Hdr == IntPtr.Zero)
                    continue; // Start() has failed

                WAVEHDR k_Hdr = (WAVEHDR)Marshal.PtrToStructure(p_Hdr, typeof(WAVEHDR));
                waveInUnprepareHeader(mh_WaveIn, p_Hdr, ms32_HdrSize);
                Marshal.FreeHGlobal(k_Hdr.lpData);
                Marshal.FreeHGlobal(p_Hdr);
            }
            waveInClose(mh_WaveIn);
            mh_WaveIn  = IntPtr.Zero;
            mp_Headers = null;
        }

        // ===================================== Recording =====================================

        /// <summary>
        /// Background thread: polls the driver buffers every 5 ms.
        /// </summary>
        void CollectThread()
        {
            while (!mb_StopThread)
            {
                try
                {
                    CollectBuffers();
                }
                catch (Exception Ex)
                {
                    ms_ThreadError = Ex.Message; // the GUI thread throws it in Acquire() / Record()
                    return;
                }
                Thread.Sleep(5);
            }
        }

        /// <summary>
        /// Copies all buffers that the driver has filled into the ring buffer and gives them back to the driver.
        /// Called in the background thread.
        /// </summary>
        void CollectBuffers()
        {
            while (true)
            {
                IntPtr  p_Hdr = mp_Headers[ms32_NextBuffer];
                WAVEHDR k_Hdr = (WAVEHDR)Marshal.PtrToStructure(p_Hdr, typeof(WAVEHDR));
                if ((k_Hdr.dwFlags & WHDR_DONE) == 0)
                    return;

                int s32_BytesPerFrame = mb_Float ? 8 : 4; // 2 channels
                int s32_Frames = (int)k_Hdr.dwBytesRecorded / s32_BytesPerFrame;
                lock (mi_Lock)
                {
                    AppendToRing(k_Hdr.lpData, s32_Frames);
                }

                k_Hdr.dwFlags        &= ~(UInt32)WHDR_DONE;
                k_Hdr.dwBytesRecorded = 0;
                Marshal.StructureToPtr(k_Hdr, p_Hdr, false);

                // Do not call Check() here: it would call Stop() which waits for this thread
                int s32_Error = waveInAddBuffer(mh_WaveIn, p_Hdr, ms32_HdrSize);
                if (s32_Error != MMSYSERR_NOERROR)
                    throw new Exception("Audio input error: " + GetErrorText(s32_Error));

                ms32_NextBuffer = (ms32_NextBuffer + 1) % BUFFER_COUNT;
            }
        }

        void AppendToRing(IntPtr p_Data, int s32_Frames)
        {
            int s32_Size = mf_Ring[0].Length;
            if (mb_Float)
            {
                float[] f_Data = new float[s32_Frames * 2];
                Marshal.Copy(p_Data, f_Data, 0, f_Data.Length);
                for (int F=0; F<s32_Frames; F++)
                {
                    int s32_Pos = (int)((ms64_Written + F) % s32_Size);
                    mf_Ring[0][s32_Pos] = f_Data[2 * F];
                    mf_Ring[1][s32_Pos] = f_Data[2 * F + 1];
                }
            }
            else
            {
                Int16[] s16_Data = new Int16[s32_Frames * 2];
                Marshal.Copy(p_Data, s16_Data, 0, s16_Data.Length);
                for (int F=0; F<s32_Frames; F++)
                {
                    int s32_Pos = (int)((ms64_Written + F) % s32_Size);
                    mf_Ring[0][s32_Pos] = s16_Data[2 * F]     / 32768f;
                    mf_Ring[1][s32_Pos] = s16_Data[2 * F + 1] / 32768f;
                }
            }
            ms64_Written += s32_Frames;
        }

        WAVEFORMAT CreateFormat(bool b_Float)
        {
            WAVEFORMAT k_Format = new WAVEFORMAT();
            k_Format.nChannels      = 2;
            k_Format.nSamplesPerSec = (UInt32)ms32_Rate;
            if (b_Float)
            {
                k_Format.wFormatTag          = WAVE_FORMAT_EXTENSIBLE;
                k_Format.wBitsPerSample      = 32;
                k_Format.cbSize              = 22;
                k_Format.wValidBitsPerSample = 32;
                k_Format.dwChannelMask       = 3; // front left + front right
                k_Format.SubFormat           = SUBTYPE_FLOAT;
            }
            else
            {
                k_Format.wFormatTag     = WAVE_FORMAT_PCM;
                k_Format.wBitsPerSample = 16;
            }
            k_Format.nBlockAlign     = (UInt16)(k_Format.nChannels * k_Format.wBitsPerSample / 8);
            k_Format.nAvgBytesPerSec = k_Format.nSamplesPerSec * k_Format.nBlockAlign;
            return k_Format;
        }

        // ===================================== Helper =====================================

        /// <summary>
        /// Returns the names of all audio input devices. Windows truncates the names to 31 characters.
        /// Does not throw (returns an empty list on Linux).
        /// </summary>
        public static List<String> EnumerateDevices()
        {
            List<String> i_Names = new List<String>();
            try
            {
                int s32_Count = waveInGetNumDevs();
                for (int D=0; D<s32_Count; D++)
                {
                    WAVEINCAPS k_Caps = new WAVEINCAPS();
                    if (waveInGetDevCaps(new IntPtr(D), ref k_Caps, Marshal.SizeOf(typeof(WAVEINCAPS))) == MMSYSERR_NOERROR)
                        i_Names.Add(k_Caps.szPname);
                    else
                        i_Names.Add("Device " + D);
                }
            }
            catch {}
            return i_Names;
        }

        void Check(int s32_Error)
        {
            if (s32_Error != MMSYSERR_NOERROR)
            {
                Stop();
                throw new Exception("Audio input error: " + GetErrorText(s32_Error));
            }
        }

        static String GetErrorText(int s32_Error)
        {
            StringBuilder s_Text = new StringBuilder(256);
            waveInGetErrorText(s32_Error, s_Text, s_Text.Capacity);
            return s_Text.ToString() + " (" + s32_Error + ")";
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
    }
}
