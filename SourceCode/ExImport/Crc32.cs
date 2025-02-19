/*
------------------------------------------------------------
Oscilloscope Waveform Analyzer by ElmüSoft (www.netcult.ch/elmue)
This class is a helper for the OSZI file format.
The OSZI file format is free of any license.
You can use this code even in a closed-source commercial software.
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
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace ExImport
{
    public class Crc32
    {
        const UInt32 CRC_INIT  = 0xFFAA6622;
        const UInt32 CRC_POLY  = 0x04C11DB7;
        const UInt32 CRC_EXOR  = 0xF88DD88F;

        // ---------------- STATIC ----------------

        static UInt32[] mu32_LookupTbl;

        static Crc32()
        {
            mu32_LookupTbl = new UInt32[256];
            for (UInt32 i=0; i<=0xFF; i++)
            {
                mu32_LookupTbl[i] = Reflect32(i, 8) << 24;
                for (int j=0; j<8; j++) // Shift in 8 Bits
                {
                    mu32_LookupTbl[i] = (mu32_LookupTbl[i] << 1) ^ ((mu32_LookupTbl[i] & (1 << 31)) != 0 ? CRC_POLY : 0);
                }
                mu32_LookupTbl[i] = Reflect32(mu32_LookupTbl[i], 32);
            } 
        }

        private static UInt32 Reflect32(UInt32 u32_Value, Byte u8_Bits)
        {
            UInt32 u32_Ret = 0;
            UInt32 u32_OR  = (UInt32)(1 << (u8_Bits-1));
            do
            {
                if ((u32_Value & 1) != 0) u32_Ret |= u32_OR;
                u32_OR    >>= 1;
                u32_Value >>= 1;
            }
            while (u32_OR != 0);
            return u32_Ret;
        } 

        // ---------------- MEMBER ----------------

        UInt32 mu32_CRC = CRC_INIT;

        public void Init()
        {
            mu32_CRC = CRC_INIT;
        }

        public void Calc(Byte u8_Value)
        {
            mu32_CRC = (mu32_CRC >> 8) ^ mu32_LookupTbl[(Byte)mu32_CRC ^ u8_Value];
        }

        public void Calc(Byte[] u8_Values, int s32_Count)
        {
            // This may happen if the OSZI file is shorter than expected and Stream.Read() returns insufficient bytes.
            if (s32_Count > u8_Values.Length)
                throw new Exception("The input file is corrupt.");

            for (int i=0; i<s32_Count; i++)
            {
                mu32_CRC = (mu32_CRC >> 8) ^ mu32_LookupTbl[(Byte)mu32_CRC ^ u8_Values[i]];
            }
        }

        public UInt32 Finish()
        {
            return mu32_CRC ^ CRC_EXOR; 
        }
    }

    // ====================================================================================================

    public class BitShifter
    {
        public readonly int ms32_Bits;
        public readonly int ms32_Mask;
        public          int ms32_Shift; // internal shift register
        public          int ms32_Used;  // internal bit counter
        public         bool mb_Flush;   // set this to flush the remainig bits to the stream when all data is written

        public BitShifter(int s32_Bits)
        {
            ms32_Bits = s32_Bits;
            ms32_Mask = (1 << s32_Bits) -1; // 8 bit --> Mask = 0xFF
        }
    }

    // ====================================================================================================

    /// <summary>
    /// A BinaryWriter that calculates the CRC32 of all data written to the Stream
    /// </summary>
    public class CrcWriter : BinaryWriter
    {
        Crc32 mi_CRC = new Crc32();

        public CrcWriter(Stream i_Stream) : base(i_Stream)
        {   
        }

        // ------------------------------------

        public override void Write(Byte[] u8_Array)
        {
            mi_CRC.Calc(u8_Array, u8_Array.Length);
            base.Write (u8_Array);
        }

        public void Write(Byte[] u8_Array, int s32_Count)
        {
            mi_CRC.Calc(u8_Array, s32_Count);
            base.Write (u8_Array, 0, s32_Count);
        }

        // ------------------------------------

        public override void Write(Byte u8_Value)
        {
            mi_CRC.Calc(u8_Value); // 1 byte
            base.Write (u8_Value);
        }

        public override void Write(SByte s8_Value)
        {
            mi_CRC.Calc((Byte)s8_Value); // 1 byte
            base.Write (s8_Value);
        }

        // ------------------------------------

        public override void Write(UInt16 u16_Value)
        {
            Byte[] u8_Data = BitConverter.GetBytes(u16_Value);
            mi_CRC.Calc(u8_Data, u8_Data.Length); // 2 byte
            base.Write (u8_Data);
        }

        public override void Write(Int16 s16_Value)
        {
            Byte[] u8_Data = BitConverter.GetBytes(s16_Value);
            mi_CRC.Calc(u8_Data, u8_Data.Length); // 2 byte
            base.Write (u8_Data);
        }

        // ------------------------------------

        public override void Write(UInt32 u32_Value)
        {
            Byte[] u8_Data = BitConverter.GetBytes(u32_Value);
            mi_CRC.Calc(u8_Data, u8_Data.Length); // 4 byte
            base.Write (u8_Data);
        }

        public override void Write(int s32_Value)
        {
            Byte[] u8_Data = BitConverter.GetBytes(s32_Value);
            mi_CRC.Calc(u8_Data, u8_Data.Length); // 4 byte
            base.Write (u8_Data);
        }

        // ------------------------------------

        public override void Write(UInt64 u64_Value)
        {
            Byte[] u8_Data = BitConverter.GetBytes(u64_Value);
            mi_CRC.Calc(u8_Data, u8_Data.Length); // 8 byte
            base.Write (u8_Data);
        }

        public override void Write(Int64 s64_Value)
        {
            Byte[] u8_Data = BitConverter.GetBytes(s64_Value);
            mi_CRC.Calc(u8_Data, u8_Data.Length); // 8 byte
            base.Write (u8_Data);
        }

        // ------------------------------------

        public override void Write(float f_Value)
        {
            Byte[] u8_Data = BitConverter.GetBytes(f_Value);
            mi_CRC.Calc(u8_Data, u8_Data.Length); // 4 byte
            base.Write (u8_Data);
        }

        public override void Write(double d_Value)
        {
            Byte[] u8_Data = BitConverter.GetBytes(d_Value);
            mi_CRC.Calc(u8_Data, u8_Data.Length); // 8 byte
            base.Write (u8_Data);
        }

        public override void Write(decimal d_Value)
        {
            foreach (int s32_Part in Decimal.GetBits(d_Value))
            {
                Write(s32_Part); // 16 byte
            }
        }

        // ------------------------------------

        /// <summary>
        /// Write a length-prefixed Unicode string (max 65536 characters)
        /// The base class BinaryWriter does not support unicode strings
        /// </summary>
        public override void Write(String s_Text)
        {
            Write((UInt16)s_Text.Length);
            foreach (Char c_Char in s_Text)
            {
                Write((UInt16)c_Char);
            }
        }

        // ------------------------------------

        /// <summary>
        /// Writes i_Shifter.ms32_Bits of s32_Value to the stream.
        /// This allows the highest data compression when for example 10 bit analog data has to be written.
        /// Data is shifted in until a byte is full and flushed to the stream.
        /// The remaining bits stay in the BitShifter.
        /// After all data has been written this function must be called once with mb_Flush = true.
        /// </summary>
        public void Write(int s32_Value, BitShifter i_Shifter)
        {
            if (i_Shifter.mb_Flush)
            {
                if (i_Shifter.ms32_Used > 0)
                    Write((Byte)i_Shifter.ms32_Shift);
                return;
            }

            if (i_Shifter.ms32_Bits == 8)
            {
                Write((Byte)s32_Value);
                return;
            }

            s32_Value &=  i_Shifter.ms32_Mask;
            s32_Value <<= i_Shifter.ms32_Used;
            i_Shifter.ms32_Shift |= s32_Value;
            i_Shifter.ms32_Used  += i_Shifter.ms32_Bits;

            while (i_Shifter.ms32_Used >= 8)
            {
                Write((Byte)i_Shifter.ms32_Shift);
                i_Shifter.ms32_Shift >>= 8;
                i_Shifter.ms32_Used   -= 8;
            }
        }

        /// <summary>
        /// This must be the last command executed on the stream
        /// </summary>
        public void WriteCRC()
        {
            base.Write(mi_CRC.Finish());
        }
    }

    // ====================================================================================================

    /// <summary>
    /// A BinaryReader that calculates the CRC32 of all data read from the stream
    /// </summary>
    public class CrcReader : BinaryReader
    {
        Crc32 mi_CRC = new Crc32();

        public CrcReader(Stream i_Stream) : base(i_Stream)
        {   
        }

        public override Byte[] ReadBytes(int s32_Count)
        {
            Byte[] u8_Data = base.ReadBytes(s32_Count);
            mi_CRC.Calc(u8_Data, s32_Count);
            return u8_Data;
        }

        // ------------------------------------

        public override Byte ReadByte()
        {
            Byte u8_Data = base.ReadByte();
            mi_CRC.Calc(u8_Data);
            return u8_Data;
        }

        public override SByte ReadSByte()
        {
            Byte u8_Data = base.ReadByte();
            mi_CRC.Calc(u8_Data);
            return (SByte)u8_Data;
        }

        // ------------------------------------

        public override UInt16 ReadUInt16()
        {
            Byte[] u8_Data = base.ReadBytes(2);
            mi_CRC.Calc(u8_Data, 2);
            return BitConverter.ToUInt16(u8_Data, 0);
        }

        public override Int16 ReadInt16()
        {
            Byte[] u8_Data = base.ReadBytes(2);
            mi_CRC.Calc(u8_Data, 2);
            return BitConverter.ToInt16(u8_Data, 0);
        }

        // ------------------------------------

        public override UInt32 ReadUInt32()
        {
            Byte[] u8_Data = base.ReadBytes(4);
            mi_CRC.Calc(u8_Data, 4);
            return BitConverter.ToUInt32(u8_Data, 0);
        }

        public override Int32 ReadInt32()
        {
            Byte[] u8_Data = base.ReadBytes(4);
            mi_CRC.Calc(u8_Data, 4);
            return BitConverter.ToInt32(u8_Data, 0);
        }

        // ------------------------------------

        public override UInt64 ReadUInt64()
        {
            Byte[] u8_Data = base.ReadBytes(8);
            mi_CRC.Calc(u8_Data, 8);
            return BitConverter.ToUInt64(u8_Data, 0);
        }

        public override Int64 ReadInt64()
        {
            Byte[] u8_Data = base.ReadBytes(8);
            mi_CRC.Calc(u8_Data, 8);
            return BitConverter.ToInt64(u8_Data, 0);
        }

        // ------------------------------------

        public override Single ReadSingle()
        {
            Byte[] u8_Data = base.ReadBytes(4);
            mi_CRC.Calc(u8_Data, 4);
            return BitConverter.ToSingle(u8_Data, 0);
        }

        public override Double ReadDouble()
        {
            Byte[] u8_Data = base.ReadBytes(8);
            mi_CRC.Calc(u8_Data, 8);
            return BitConverter.ToDouble(u8_Data, 0);
        }

        public override Decimal ReadDecimal()
        {
            Byte[] u8_Data = base.ReadBytes(16);
            mi_CRC.Calc(u8_Data, 16);

            int P1 = BitConverter.ToInt32(u8_Data,  0);
            int P2 = BitConverter.ToInt32(u8_Data,  4);
            int P3 = BitConverter.ToInt32(u8_Data,  8);
            int P4 = BitConverter.ToInt32(u8_Data, 12);
            return new decimal(new int[]{ P1, P2, P3, P4 });   
        }

        // ------------------------------------

        /// <summary>
        /// The base class BinaryReader does not support unicode strings
        /// </summary>
        public override String ReadString()
        {
            StringBuilder i_Builder = new StringBuilder();
            int s32_Length = ReadUInt16();
            for (int i=0; i<s32_Length; i++)
            {
                i_Builder.Append((Char)ReadUInt16());
            }
            return i_Builder.ToString();
        }

        // ------------------------------------

        /// <summary>
        /// Reads i_Shifter.ms32_Bits from the stream.
        /// This allows the highest data compression when for example 10 bit analog data has to be stored in a stream.
        /// The requested data is shifted out and the remaining bits stay in the BitShifter.
        /// </summary>
        public int Read(BitShifter i_Shifter)
        {
            if (i_Shifter.ms32_Bits == 8)
                return ReadByte();

            while (i_Shifter.ms32_Used < i_Shifter.ms32_Bits)
            {
                int s32_Byte = ReadByte();
                s32_Byte <<= i_Shifter.ms32_Used;
                i_Shifter.ms32_Shift |= s32_Byte;
                i_Shifter.ms32_Used  += 8;
            }

            int s32_Value = i_Shifter.ms32_Shift & i_Shifter.ms32_Mask;
            i_Shifter.ms32_Shift >>= i_Shifter.ms32_Bits;
            i_Shifter.ms32_Used   -= i_Shifter.ms32_Bits;
            return s32_Value;
        }

        // ------------------------------------

        /// <summary>
        /// returns false if the last 4 bytes of the Stream are not the calculated CRC
        /// </summary>
        public bool CheckCRC()
        {
            UInt32 u32_CalcCRC   = mi_CRC.Finish();
            UInt32 u32_StreamCRC = base.ReadUInt32();

            if (u32_CalcCRC == u32_StreamCRC)
                return true;

            // The debug version allows a dummy CRC
            #if DEBUG
                if (u32_StreamCRC == 0x12345678)
                    return true;
            #endif
            return false;
        }
    }
}
