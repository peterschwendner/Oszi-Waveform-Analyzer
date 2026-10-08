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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using WAVEFORMAT        = Transfer.AudioInput.WAVEFORMAT;
using WAVEHDR           = Transfer.AudioInput.WAVEHDR;

// Plays a stereo signal on the Windows default audio output (Windows Multimedia API, waveOut of winmm.dll).
// A background thread fills a chain of small buffers with the samples of a callback function.
// PlayedSamples is the count of samples that have been played (heard) since the start.
// It is used as the clock of the display, so that the picture is synchronous with the sound.
namespace Transfer
{
    public class AudioOutput
    {
        #region winmm.dll

        const int TIME_BYTES   = 4;
        const int TIME_SAMPLES = 2;

        [StructLayout(LayoutKind.Sequential)]
        struct MMTIME
        {
            public UInt32 wType;
            public UInt32 u;      // union: sample or cb (the SMPTE structure is 8 byte)
            public UInt32 u2;
        }

        [DllImport("winmm.dll")]
        static extern int waveOutOpen(out IntPtr h_WaveOut, IntPtr u_DeviceID, ref WAVEFORMAT k_Format, IntPtr p_Callback, IntPtr p_Instance, int s32_Flags);
        [DllImport("winmm.dll")]
        static extern int waveOutPrepareHeader(IntPtr h_WaveOut, IntPtr p_Header, int s32_Size);
        [DllImport("winmm.dll")]
        static extern int waveOutUnprepareHeader(IntPtr h_WaveOut, IntPtr p_Header, int s32_Size);
        [DllImport("winmm.dll")]
        static extern int waveOutWrite(IntPtr h_WaveOut, IntPtr p_Header, int s32_Size);
        [DllImport("winmm.dll")]
        static extern int waveOutReset(IntPtr h_WaveOut);
        [DllImport("winmm.dll")]
        static extern int waveOutClose(IntPtr h_WaveOut);
        [DllImport("winmm.dll")]
        static extern int waveOutGetPosition(IntPtr h_WaveOut, ref MMTIME k_Time, int s32_Size);
        [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
        static extern int waveOutGetErrorText(int s32_Error, StringBuilder s_Text, int s32_Size);

        #endregion

        const int BUFFER_COUNT = 8;   // 8 x 20 ms = 160 ms latency
        const int BUFFER_MS    = 20;

        /// <summary>
        /// Called in the background thread. Fills up to s32_Count samples. Returns the count filled, the rest is silence.
        /// </summary>
        public delegate int delFill(float[] f_Left, float[] f_Right, int s32_Count);

        IntPtr    mh_WaveOut;
        IntPtr[]  mp_Headers;
        int       ms32_HdrSize;
        int       ms32_Frames;      // per buffer
        int       ms32_NextBuffer;
        bool      mb_Float;
        int       ms32_BlockAlign;
        delFill   mf_Fill;
        float[]   mf_Left, mf_Right;
        Thread    mi_Thread;
        volatile bool   mb_StopThread;
        volatile String ms_ThreadError;
        UInt32    mu32_LastPos;
        long      ms64_Played;

        /// <summary>
        /// Starts the playback. Throws.
        /// </summary>
        public void Start(int s32_Rate, delFill f_Fill)
        {
            Stop();
            mf_Fill     = f_Fill;
            ms64_Played = 0;
            mu32_LastPos = 0;

            WAVEFORMAT k_Format = AudioInput.CreateFormat(true, s32_Rate);
            int s32_Error = waveOutOpen(out mh_WaveOut, new IntPtr(AudioInput.WAVE_MAPPER), ref k_Format, IntPtr.Zero, IntPtr.Zero, AudioInput.CALLBACK_NULL);
            mb_Float = true;
            if (s32_Error == AudioInput.WAVERR_BADFORMAT)
            {
                k_Format  = AudioInput.CreateFormat(false, s32_Rate);
                s32_Error = waveOutOpen(out mh_WaveOut, new IntPtr(AudioInput.WAVE_MAPPER), ref k_Format, IntPtr.Zero, IntPtr.Zero, AudioInput.CALLBACK_NULL);
                mb_Float  = false;
            }
            if (s32_Error != AudioInput.MMSYSERR_NOERROR)
            {
                mh_WaveOut = IntPtr.Zero;
                throw new Exception("Error opening the audio output with " + s32_Rate + " Hz: " + GetErrorText(s32_Error));
            }

            ms32_BlockAlign = k_Format.nBlockAlign;
            ms32_Frames     = Math.Max(64, s32_Rate * BUFFER_MS / 1000);
            mf_Left         = new float[ms32_Frames];
            mf_Right        = new float[ms32_Frames];
            ms32_HdrSize    = Marshal.SizeOf(typeof(WAVEHDR));
            mp_Headers      = new IntPtr[BUFFER_COUNT];
            ms32_NextBuffer = 0;

            for (int B=0; B<BUFFER_COUNT; B++)
            {
                WAVEHDR k_Hdr = new WAVEHDR();
                k_Hdr.lpData         = Marshal.AllocHGlobal(ms32_Frames * ms32_BlockAlign);
                k_Hdr.dwBufferLength = (UInt32)(ms32_Frames * ms32_BlockAlign);

                mp_Headers[B] = Marshal.AllocHGlobal(ms32_HdrSize);
                Marshal.StructureToPtr(k_Hdr, mp_Headers[B], false);
                Check(waveOutPrepareHeader(mh_WaveOut, mp_Headers[B], ms32_HdrSize));
            }
            // Fill all buffers before the playback starts
            for (int B=0; B<BUFFER_COUNT; B++)
                FillAndWrite(mp_Headers[B]);

            mb_StopThread  = false;
            ms_ThreadError = null;
            mi_Thread = new Thread(new ThreadStart(FeedThread));
            mi_Thread.IsBackground = true;
            mi_Thread.Priority     = ThreadPriority.AboveNormal;
            mi_Thread.Start();
        }

        /// <summary>
        /// Stops the playback immediately. Does not throw.
        /// </summary>
        public void Stop()
        {
            if (mh_WaveOut == IntPtr.Zero)
                return;

            if (mi_Thread != null)
            {
                mb_StopThread = true;
                mi_Thread.Join();
                mi_Thread = null;
            }

            waveOutReset(mh_WaveOut);
            foreach (IntPtr p_Hdr in mp_Headers)
            {
                if (p_Hdr == IntPtr.Zero)
                    continue;
                WAVEHDR k_Hdr = (WAVEHDR)Marshal.PtrToStructure(p_Hdr, typeof(WAVEHDR));
                waveOutUnprepareHeader(mh_WaveOut, p_Hdr, ms32_HdrSize);
                Marshal.FreeHGlobal(k_Hdr.lpData);
                Marshal.FreeHGlobal(p_Hdr);
            }
            waveOutClose(mh_WaveOut);
            mh_WaveOut = IntPtr.Zero;
            mp_Headers = null;
        }

        public String Error
        {
            get { return ms_ThreadError; }
        }

        /// <summary>
        /// The count of samples that have been played since Start()
        /// </summary>
        public long PlayedSamples
        {
            get
            {
                if (mh_WaveOut == IntPtr.Zero)
                    return ms64_Played;

                MMTIME k_Time = new MMTIME();
                k_Time.wType = TIME_SAMPLES;
                if (waveOutGetPosition(mh_WaveOut, ref k_Time, Marshal.SizeOf(typeof(MMTIME))) != AudioInput.MMSYSERR_NOERROR)
                    return ms64_Played;

                // The driver may return another format if TIME_SAMPLES is not supported
                UInt32 u32_Pos = k_Time.u;
                if (k_Time.wType == TIME_BYTES)
                    u32_Pos /= (UInt32)ms32_BlockAlign;

                // The 32 bit counter wraps around after 24 hours at 48 kHz
                ms64_Played += (UInt32)(u32_Pos - mu32_LastPos);
                mu32_LastPos = u32_Pos;
                return ms64_Played;
            }
        }

        // ============================================================================================

        void FeedThread()
        {
            while (!mb_StopThread)
            {
                try
                {
                    while (true)
                    {
                        IntPtr  p_Hdr = mp_Headers[ms32_NextBuffer];
                        WAVEHDR k_Hdr = (WAVEHDR)Marshal.PtrToStructure(p_Hdr, typeof(WAVEHDR));
                        if ((k_Hdr.dwFlags & AudioInput.WHDR_DONE) == 0)
                            break;

                        FillAndWrite(p_Hdr);
                    }
                }
                catch (Exception Ex)
                {
                    ms_ThreadError = Ex.Message;
                    return;
                }
                Thread.Sleep(5);
            }
        }

        /// <summary>
        /// Fills the buffer with the next samples and gives it to the driver
        /// </summary>
        void FillAndWrite(IntPtr p_Hdr)
        {
            int s32_Count = Math.Max(0, Math.Min(ms32_Frames, mf_Fill(mf_Left, mf_Right, ms32_Frames)));
            for (int F=s32_Count; F<ms32_Frames; F++)
            {
                mf_Left [F] = 0; // silence
                mf_Right[F] = 0;
            }

            WAVEHDR k_Hdr = (WAVEHDR)Marshal.PtrToStructure(p_Hdr, typeof(WAVEHDR));
            if (mb_Float)
            {
                float[] f_Data = new float[ms32_Frames * 2];
                for (int F=0; F<ms32_Frames; F++)
                {
                    f_Data[2 * F]     = mf_Left [F];
                    f_Data[2 * F + 1] = mf_Right[F];
                }
                Marshal.Copy(f_Data, 0, k_Hdr.lpData, f_Data.Length);
            }
            else
            {
                Int16[] s16_Data = new Int16[ms32_Frames * 2];
                for (int F=0; F<ms32_Frames; F++)
                {
                    s16_Data[2 * F]     = ToInt16(mf_Left [F]);
                    s16_Data[2 * F + 1] = ToInt16(mf_Right[F]);
                }
                Marshal.Copy(s16_Data, 0, k_Hdr.lpData, s16_Data.Length);
            }

            k_Hdr.dwFlags &= ~(UInt32)AudioInput.WHDR_DONE;
            Marshal.StructureToPtr(k_Hdr, p_Hdr, false);

            int s32_Error = waveOutWrite(mh_WaveOut, p_Hdr, ms32_HdrSize);
            if (s32_Error != AudioInput.MMSYSERR_NOERROR)
                throw new Exception("Audio output error: " + GetErrorText(s32_Error));

            ms32_NextBuffer = (ms32_NextBuffer + 1) % BUFFER_COUNT;
        }

        static Int16 ToInt16(float f_Value)
        {
            return (Int16)Math.Max(-32768, Math.Min(32767, Math.Round(f_Value * 32767)));
        }

        void Check(int s32_Error)
        {
            if (s32_Error != AudioInput.MMSYSERR_NOERROR)
            {
                Stop();
                throw new Exception("Audio output error: " + GetErrorText(s32_Error));
            }
        }

        static String GetErrorText(int s32_Error)
        {
            StringBuilder s_Text = new StringBuilder(256);
            waveOutGetErrorText(s32_Error, s_Text, s_Text.Capacity);
            return s_Text.ToString() + " (" + s32_Error + ")";
        }
    }
}
