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
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;

using SpiPacket         = Operations.DecodeSPI.SpiPacket;
using I2CPacket         = Operations.DecodeI2C.I2CPacket;
using UartPacket        = Operations.DecodeUART.UartPacket;
using RtfDocument       = OsziWaveformAnalyzer.RtfDocument;
using RtfBuilder        = OsziWaveformAnalyzer.RtfBuilder;

namespace PostDecoder
{
    /// <summary>
    /// This class can be called to do a chip specific post processing of decoded SPI or I2C data.
    /// </summary>
    public class PostDecoderManager
    {
        #region enums

        public enum eSpiChip
        {
            None,

            [Description("Philips RFID and NFC transmission chip")]
            PN532, 

            // TODO: implement more SPI chips here
        }

        public enum eI2cChip
        {
            None,

            [Description("Philips RFID and NFC transmission chip")]
            PN532, 

            // TODO: implement more I2C chips here
        }

        public enum eUartChip
        {
            None,

            [Description("K-Line protocol for automotive Engine Control Units")]
            ISO14230,

            [Description("Smartcard protocol for pinpad / chipcard communication")]
            ISO7816,

            // TODO: implement nore UART decoders here
        }

        #endregion

        #region interface IPostDecode

        /// <summary>
        /// Some chips support multiple protocols.
        /// For example the Philips PN532 can be configured to use SPI, I2C or UART.
        /// </summary>
        public interface IPostDecode
        {
            void DecodeUART(UartPacket i_Packet, RtfBuilder i_RtfBuilder);
            void DecodeSPI (SpiPacket  i_Packet, RtfBuilder i_RtfBuilder);
            void DecodeI2C (I2CPacket  i_Packet, RtfBuilder i_RtfBuilder);
        }

        #endregion

        // ==========================================================================================

        IPostDecode mi_Instance;

        /// <summary>
        /// Constructor UART
        /// </summary>
        public PostDecoderManager(eUartChip e_Chip)
        {
            switch (e_Chip)
            {
                case eUartChip.None:      return;
                case eUartChip.ISO7816:   mi_Instance = new ISO7816();  break;
                case eUartChip.ISO14230:  mi_Instance = new ISO14230(); break;

                // TODO: implement more UART decoders here
                default: Debug.Assert(false, "Programming Error: eUartChip not implemented"); break;
            }
        }

        /// <summary>
        /// Constructor SPI
        /// </summary>
        public PostDecoderManager(eSpiChip e_Chip)
        {
            switch (e_Chip)
            {
                case eSpiChip.None:   return;
                case eSpiChip.PN532:  mi_Instance = new PN532(); break;

                // TODO: implement more SPI decoders here
                default: Debug.Assert(false, "Programming Error: eSpiChip not implemented"); break;
            }
        }

        /// <summary>
        /// Constructor I2C
        /// </summary>
        public PostDecoderManager(eI2cChip e_Chip)
        {
            switch (e_Chip)
            {
                case eI2cChip.None:   return;
                case eI2cChip.PN532:  mi_Instance = new PN532(); break;

                // TODO: implement more I2C decoders here
                default: Debug.Assert(false, "Programming Error: eI2cChip not implemented"); break;
            }
        }

        // ==========================================================================================

        /// <summary>
        /// If the packet contains valid data, one or multiple lines are added to the RTF output, otherwise ignore packet.
        /// </summary>
        public void DecodeUART(UartPacket i_Packet, RtfBuilder i_RtfBuilder)
        {
            if (mi_Instance != null && i_Packet != null)
                mi_Instance.DecodeUART(i_Packet, i_RtfBuilder);
        }

        /// <summary>
        /// If the packet contains valid data, one or multiple lines are added to the RTF output, otherwise ignore packet.
        /// </summary>
        public void DecodeSPI(SpiPacket i_Packet, RtfBuilder i_RtfBuilder)
        {
            if (mi_Instance != null && i_Packet != null)
                mi_Instance.DecodeSPI(i_Packet, i_RtfBuilder);
        }

        /// <summary>
        /// If the packet contains valid data, one or multiple lines are added to the RTF output, otherwise ignore packet.
        /// </summary>
        public void DecodeI2C(I2CPacket i_Packet, RtfBuilder i_RtfBuilder)
        {
            if (mi_Instance != null && i_Packet != null)
                mi_Instance.DecodeI2C(i_Packet, i_RtfBuilder);
        }
    }
}
