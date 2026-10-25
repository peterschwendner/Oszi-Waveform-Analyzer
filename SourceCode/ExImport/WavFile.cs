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
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using Channel           = OsziWaveformAnalyzer.Utils.Channel;
using Capture           = OsziWaveformAnalyzer.Utils.Capture;
using Utils             = OsziWaveformAnalyzer.Utils;

namespace ExImport
{
    /// <summary>
    /// This class imports WAV audio files (RIFF / WAVE).
    /// Each audio channel becomes an analog channel. The values are normalized: +1.0 / -1.0 = full scale (0 dBFS).
    /// A stereo file gives the channels "Left" and "Right". Oscilloscope music uses Left = X and Right = Y,
    /// which can be displayed with the X/Y Plot.
    ///
    /// Supported: PCM 8, 16, 24, 32 bit integer, IEEE float 32 and 64 bit, WAVE_FORMAT_EXTENSIBLE.
    /// Not supported: compressed formats (ADPCM, MP3,...), RF64 files larger than 4 GB.
    ///
    /// Files that do not fit into the free memory of the PC (see MaxFrames): the user selects a section
    /// (start and duration) in FormWavSection. Only this section is read from the file.
    /// </summary>
    public static class WavFile
    {
        const int FORMAT_PCM        = 0x0001;
        const int FORMAT_FLOAT      = 0x0003;
        const int FORMAT_EXTENSIBLE = 0xFFFE;

        // Used if the available memory cannot be determined (e.g. Mono on Linux): 24 M samples per channel.
        const int FALLBACK_FRAMES = 24 * 1000 * 1000;

        // The loaded samples are copied by the FFT and other operations --> use only a part of the free memory.
        const double MEMORY_FRACTION = 0.4;
        const long   MAX_BYTES_32BIT = 600L * 1024 * 1024; // the address space of a 32 bit process is limited
        const int    MAX_ARRAY_LEN   = 0x1F000000;         // a float[] must stay below 2 GB (.NET 4.0): 520 M samples

        [StructLayout(LayoutKind.Sequential)]
        struct MEMORYSTATUSEX
        {
            public uint  dwLength;
            public uint  dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX r_Status);

        /// <summary>
        /// The maximum number of samples per channel that can be loaded. Only limited by the free memory of the PC.
        /// </summary>
        public static int MaxFrames(int s32_Channels)
        {
            long s64_Bytes;
            try
            {
                MEMORYSTATUSEX r_Status = new MEMORYSTATUSEX();
                r_Status.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
                if (!GlobalMemoryStatusEx(ref r_Status))
                    return FALLBACK_FRAMES;

                s64_Bytes = (long)(r_Status.ullAvailPhys * MEMORY_FRACTION);
                if (IntPtr.Size == 4)
                    s64_Bytes = Math.Min(s64_Bytes, MAX_BYTES_32BIT);
            }
            catch // not Windows
            {
                return FALLBACK_FRAMES;
            }

            long s64_Frames = s64_Bytes / (sizeof(float) * Math.Max(1, s32_Channels));
            return (int)Math.Max(FALLBACK_FRAMES / 4, Math.Min(s64_Frames, MAX_ARRAY_LEN));
        }

        /// <summary>
        /// Throws on error. Returns null if the user has aborted.
        /// </summary>
        public static Capture Load(String s_Path, ref bool b_Abort)
        {
            using (FileStream i_Stream = new FileStream(s_Path, FileMode.Open, FileAccess.Read, FileShare.Read, 0x10000))
            using (BinaryReader i_Reader = new BinaryReader(i_Stream))
            {
                if (i_Stream.Length < 44 || ReadId(i_Reader) != "RIFF")
                    throw new Exception("The file is not a WAV file (RIFF header missing).");

                i_Reader.ReadUInt32(); // RIFF size (not reliable)
                if (ReadId(i_Reader) != "WAVE")
                    throw new Exception("The file is not a WAV file (WAVE header missing).");

                int s32_Format   = 0;
                int s32_Channels = 0;
                int s32_Rate     = 0;
                int s32_Align    = 0;
                int s32_Bits     = 0;

                // Search the chunks "fmt " and "data". Other chunks (LIST, fact, bext, ...) are skipped.
                while (i_Stream.Position + 8 <= i_Stream.Length)
                {
                    String s_Id    = ReadId(i_Reader);
                    long   s64_Len = i_Reader.ReadUInt32();
                    long   s64_Pos = i_Stream.Position;

                    if (s_Id == "fmt ")
                    {
                        s32_Format   = i_Reader.ReadUInt16();
                        s32_Channels = i_Reader.ReadUInt16();
                        s32_Rate     = i_Reader.ReadInt32();
                        i_Reader.ReadInt32(); // bytes per second
                        s32_Align    = i_Reader.ReadUInt16();
                        s32_Bits     = i_Reader.ReadUInt16();

                        if (s32_Format == FORMAT_EXTENSIBLE && s64_Len >= 40)
                        {
                            i_Reader.ReadUInt16(); // cbSize
                            i_Reader.ReadUInt16(); // valid bits per sample
                            i_Reader.ReadUInt32(); // channel mask
                            s32_Format = i_Reader.ReadUInt16(); // the first 2 bytes of the SubFormat GUID are the format
                        }
                    }
                    else if (s_Id == "data")
                    {
                        if (s32_Channels == 0)
                            throw new Exception("The WAV file has no 'fmt' chunk before the 'data' chunk.");

                        // Some programs that write a stream do not set the length --> use the rest of the file
                        long s64_Avail = i_Stream.Length - s64_Pos;
                        if (s64_Len == 0 || s64_Len > s64_Avail)
                            s64_Len = s64_Avail;

                        return ReadData(i_Reader, s_Path, s64_Len, s32_Format, s32_Channels, s32_Rate, s32_Align, s32_Bits, ref b_Abort);
                    }

                    // Chunks are aligned to 2 bytes
                    i_Stream.Position = s64_Pos + s64_Len + (s64_Len & 1);
                }
                throw new Exception("The WAV file has no 'data' chunk.");
            }
        }

        static Capture ReadData(BinaryReader i_Reader, String s_Path, long s64_Len, int s32_Format, int s32_Channels,
                                int s32_Rate, int s32_Align, int s32_Bits, ref bool b_Abort)
        {
            bool b_Float = s32_Format == FORMAT_FLOAT && (s32_Bits == 32 || s32_Bits == 64);
            bool b_Pcm   = s32_Format == FORMAT_PCM   && (s32_Bits == 8 || s32_Bits == 16 || s32_Bits == 24 || s32_Bits == 32);
            if (!b_Float && !b_Pcm)
                throw new Exception(String.Format("WAV format {0} with {1} bit is not supported.\n"
                                                + "Only uncompressed PCM (8, 16, 24, 32 bit) and float (32, 64 bit) can be imported.",
                                                  s32_Format, s32_Bits));

            int s32_Bytes = s32_Bits / 8;
            if (s32_Channels < 1 || s32_Rate < 1 || s32_Align != s32_Channels * s32_Bytes)
                throw new Exception("The WAV file has an invalid format header.");

            long s64_Frames  = s64_Len / s32_Align;
            long s64_Start   = 0;
            int  s32_MaxFrames = MaxFrames(s32_Channels);
            int  s32_Samples   = (int)Math.Min(s64_Frames, s32_MaxFrames);

            // A file that does not fit into the free memory: the user selects the section to be loaded
            if (s64_Frames > s32_MaxFrames)
            {
                using (FormWavSection i_Form = new FormWavSection(Path.GetFileName(s_Path), s64_Frames, s32_Rate, s32_Channels, s32_MaxFrames))
                {
                    if (i_Form.ShowDialog(Utils.FormMain) != DialogResult.OK)
                        return null; // aborted

                    s64_Start   = i_Form.StartFrame;
                    s32_Samples = i_Form.FrameCount;
                }
                // Skip the frames before the section (the stream is at the begin of the data chunk)
                i_Reader.BaseStream.Position += s64_Start * s32_Align;
            }

            if (s32_Samples < Utils.MIN_VALID_SAMPLES)
                throw new Exception("The WAV file contains only " + s32_Samples + " samples.");

            Capture i_Capture = new Capture();
            i_Capture.ms_Path         = s_Path;
            i_Capture.mb_FullScale    = true;
            i_Capture.ms32_Samples    = s32_Samples;
            i_Capture.ms64_SampleDist = (Int64)Math.Round((double)Utils.PICOS_PER_SECOND / s32_Rate);
            i_Capture.ms32_AnalogRes  = Math.Max(Utils.MIN_ANAL_RES, Math.Min(Utils.MAX_ANAL_RES, b_Float ? 16 : s32_Bits));

            float[][] f_Data = new float[s32_Channels][];
            for (int C=0; C<s32_Channels; C++)
            {
                f_Data[C] = new float[s32_Samples];
                Channel i_Channel = new Channel(GetChannelName(C, s32_Channels));
                i_Channel.mf_Analog = f_Data[C];
                i_Capture.mi_Channels.Add(i_Channel);
            }

            // Full scale of integer PCM: 8 bit is unsigned (128 = zero), 16...32 bit are signed
            double d_Scale = b_Float ? 1.0 : 1.0 / (1L << (s32_Bits - 1));

            const int FRAMES_PER_BLOCK = 0x4000;
            Byte[] u8_Block = new Byte[FRAMES_PER_BLOCK * s32_Align];
            int s32_Frame = 0;
            while (s32_Frame < s32_Samples)
            {
                int s32_Count = Math.Min(FRAMES_PER_BLOCK, s32_Samples - s32_Frame);
                int s32_Need  = s32_Count * s32_Align;
                int s32_Read  = 0;
                while (s32_Read < s32_Need)
                {
                    int s32_Got = i_Reader.Read(u8_Block, s32_Read, s32_Need - s32_Read);
                    if (s32_Got <= 0)
                        throw new Exception("The WAV file is truncated.");
                    s32_Read += s32_Got;
                }

                int s32_Pos = 0;
                for (int F=0; F<s32_Count; F++)
                {
                    for (int C=0; C<s32_Channels; C++)
                    {
                        double d_Value;
                        if (b_Float)
                        {
                            d_Value = (s32_Bits == 32) ? BitConverter.ToSingle(u8_Block, s32_Pos) : BitConverter.ToDouble(u8_Block, s32_Pos);
                        }
                        else
                        {
                            switch (s32_Bits)
                            {
                                case 8:  d_Value = (u8_Block[s32_Pos] - 128) * d_Scale; break;
                                case 16: d_Value = BitConverter.ToInt16(u8_Block, s32_Pos) * d_Scale; break;
                                case 24: d_Value = ((u8_Block[s32_Pos] | (u8_Block[s32_Pos + 1] << 8) | ((sbyte)u8_Block[s32_Pos + 2] << 16))) * d_Scale; break;
                                default: d_Value = BitConverter.ToInt32(u8_Block, s32_Pos) * d_Scale; break;
                            }
                        }
                        f_Data[C][s32_Frame + F] = (float)d_Value;
                        s32_Pos += s32_Bytes;
                    }
                }
                s32_Frame += s32_Count;

                if ((s32_Frame & 0xFFFFF) < FRAMES_PER_BLOCK) // approx every million samples
                {
                    Utils.FormMain.PrintStatus("Reading sample " + s32_Frame.ToString("N0") + ". Please wait.", Color.Black);
                    if (b_Abort)
                        return null;
                }
            }

            if (s64_Frames > s32_MaxFrames)
            {
                Utils.FormMain.PrintStatus(String.Format("Loaded the section {0} ... {1} of {2} (total {3})", FormatTime((double)s64_Start / s32_Rate),
                                           FormatTime((double)(s64_Start + s32_Samples) / s32_Rate), Path.GetFileName(s_Path),
                                           FormatTime((double)s64_Frames / s32_Rate)), Color.Green);
            }
            return i_Capture;
        }

        /// <summary>
        /// 200.5 --> "3:20.5", 3723 --> "1:02:03.0"
        /// </summary>
        public static String FormatTime(double d_Seconds)
        {
            int    s32_Hours = (int)(d_Seconds / 3600);
            int    s32_Min   = (int)(d_Seconds / 60) % 60;
            double d_Sec     = d_Seconds - s32_Hours * 3600 - (int)(d_Seconds / 60) % 60 * 60;
            String s_Sec     = d_Sec.ToString("00.0", CultureInfo.InvariantCulture);
            if (s_Sec == "60.0") s_Sec = "59.9"; // rounding
            if (s32_Hours > 0)
                return String.Format("{0}:{1:00}:{2}", s32_Hours, s32_Min, s_Sec);
            return String.Format("{0}:{1}", s32_Min, s_Sec);
        }

        /// <summary>
        /// "3:20.5" or "1:02:03" or "200.5" or "200,5" --> seconds. returns -1 if invalid.
        /// </summary>
        public static double ParseTime(String s_Text)
        {
            String[] s_Parts = s_Text.Trim().Replace(',', '.').Split(':');
            if (s_Parts.Length > 3)
                return -1;

            double d_Seconds = 0;
            foreach (String s_Part in s_Parts)
            {
                double d_Value;
                if (!double.TryParse(s_Part, NumberStyles.Float, CultureInfo.InvariantCulture, out d_Value) || d_Value < 0)
                    return -1;
                d_Seconds = d_Seconds * 60 + d_Value;
            }
            return d_Seconds;
        }

        static String GetChannelName(int s32_Chan, int s32_Channels)
        {
            if (s32_Channels == 1) return "Mono";
            if (s32_Channels == 2) return s32_Chan == 0 ? "Left" : "Right";
            return "Channel " + (s32_Chan + 1);
        }

        static String ReadId(BinaryReader i_Reader)
        {
            return Encoding.ASCII.GetString(i_Reader.ReadBytes(4));
        }
    }
}
