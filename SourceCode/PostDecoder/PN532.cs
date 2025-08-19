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
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

using SpiPacket         = Operations.DecodeSPI.SpiPacket;
using I2CPacket         = Operations.DecodeI2C.I2CPacket;
using UartPacket        = Operations.DecodeUART.UartPacket;
using RtfDocument       = OsziWaveformAnalyzer.RtfDocument;
using RtfBuilder        = OsziWaveformAnalyzer.RtfBuilder;
using Utils             = OsziWaveformAnalyzer.Utils;
using IPostDecode       = PostDecoder.PostDecoderManager.IPostDecode;

namespace PostDecoder
{
    /// <summary>
    /// The PN532 from Philips is an ISO 14443 RFID and NFC transmission chip operating at 13.56 MHz.
    /// It can be configured to communicate over SPI or I2C or UART.
    /// </summary>
    public class PN532 : IPostDecode
    {
        #region enums

        enum eFrameType
        {
            Invalid,
            Data,
            ACK,
            NAK,
            Error, // Error Frame contains one payload byte with error code
        }

        #endregion

        /// <summary>
        /// If the packet contains valid data, one or multiple lines are added to the RTF output,
        /// otherwise the packet will be ignored.
        /// </summary>
        public void DecodeSPI(SpiPacket i_Packet, RtfBuilder i_RtfBuilder)
        {
            List<Byte> i_Mosi = Utils.ConvertList64To8Bit(i_Packet.mi_MOSI);
            List<Byte> i_Miso = Utils.ConvertList64To8Bit(i_Packet.mi_MISO);

            if (!i_Packet.mb_ChipSel || i_Mosi == null || i_Miso == null)
                return; // with wrong user settings post decoding will not work

            switch (i_Mosi[0])
            {
                case 0x01: // PN532_SPI_DATAWRITE
                    DecodeCommand(i_Mosi, i_RtfBuilder);
                    break;

                case 0x03: // PN532_SPI_DATAREAD
                    DecodeResponse(i_Miso, i_RtfBuilder);
                    break;

                case 0x02: // PN532_SPI_STATUSREAD
                    if (i_Miso.Count == 2)
                    {
                        // MOSI = 02 XX,  MISO = XX 01 --> ready
                        String s_Status = (i_Miso[1] & 0x01) == 1 ? "ready" : "busy";
                        i_RtfBuilder.AppendLine(Color.White, "PN532 is " + s_Status);
                        return;
                    }
                    break;
            }
        }

        /// <summary>
        /// If the packet contains valid data, one or multiple lines are added to the RTF output,
        /// otherwise the packet will be ignored.
        /// </summary>
        public void DecodeI2C(I2CPacket i_Packet, RtfBuilder i_RtfBuilder)
        {
            // The PN532 has a fix I2C slave address of 0x48
            if (i_Packet.mu8_Address != 0x24)
                return;

            if (i_Packet.mb_Write)
            {
                DecodeCommand(i_Packet.mi_Data, i_RtfBuilder);
            }
            else // Read
            {
                // The first byte is always 00 or 01 indicating if the chip is ready
                if (i_Packet.mi_Data.Count == 0 || (i_Packet.mi_Data[0] & 0x01) == 0)
                {
                    i_RtfBuilder.AppendLine(Color.White, "PN532 is busy");
                    return;
                }

                if (i_Packet.mi_Data.Count == 1 && (i_Packet.mi_Data[0] & 0x01) == 1)
                {
                    i_RtfBuilder.AppendLine(Color.White, "PN532 is ready");
                    return;
                }

                DecodeResponse(i_Packet.mi_Data, i_RtfBuilder);
            }
        }

        public void DecodeUART(UartPacket i_Packet, RtfBuilder i_RtfBuilder)
        {
            throw new NotImplementedException();
        }

        // ========================================================================================

        /// <summary>
        /// Commands sent to the PN532 chip
        /// </summary>
        void DecodeCommand(List<Byte> i_Data, RtfBuilder i_RtfBuilder)
        {
            eFrameType e_Frame;
            List<Byte> i_Payload = ExtractPayload(i_Data, out e_Frame);
            if (e_Frame != eFrameType.Data || i_Payload[0] != 0xD4)
                return;

            String s_Cmd = "PN532 Command " + i_Payload[1].ToString("X2");
            switch (i_Payload[1]) // Command code
            {
                case 0x32: // PN532_COMMAND_RFCONFIGURATION 
                    s_Cmd += ": RF Configuration";
                    switch (i_Payload[2]) // First Parameter
                    {
                        case 0x01: s_Cmd += " (Turn RF field on / off)";     break;
                        case 0x02: s_Cmd += " (Set Timeouts)";               break;
                        case 0x04:
                        case 0x05: s_Cmd += " (Set Max Retries)";            break;
                        case 0x0A: s_Cmd += " (Settings for 106 kBaud)";     break;
                        case 0x0B: s_Cmd += " (Settings for 212/424 kBaud)"; break;
                        case 0x0C: s_Cmd += " (Settings for Type B)";        break;
                        case 0x0D: s_Cmd += " (Settings for ISO 14443-4)";   break;
                    }
                    break;

                case 0x4A: // PN532_COMMAND_INLISTPASSIVETARGET 
                    s_Cmd += ": Read Passive Target ID"; 
                    break;

                // TODO: Implement more commands
            }

            i_RtfBuilder.AppendLine(Color.White, s_Cmd);
        }

        /// <summary>
        /// Responses received from the PN532 chip
        /// </summary>
        void DecodeResponse(List<Byte> i_Data, RtfBuilder i_RtfBuilder)
        {
            eFrameType e_Frame;
            List<Byte> i_Payload = ExtractPayload(i_Data, out e_Frame);

            switch (e_Frame)
            {
                case eFrameType.Invalid:
                    return;
                case eFrameType.ACK:
                    i_RtfBuilder.AppendLine(Color.White, "PN532 ACK Frame");
                    return;
                case eFrameType.NAK:
                    i_RtfBuilder.AppendLine(Color.White, "PN532 NAK Frame");
                    return;
                case eFrameType.Error:
                    i_RtfBuilder.AppendLine(Color.White, "PN532 Error Frame, Code: " + i_Payload[0].ToString("X2"));
                    return;
            }

            if (i_Payload[0] != 0xD5)
                return;

            String s_Resp = "PN532 Response " + i_Payload[1].ToString("X2");
            switch (i_Payload[1]) // Response code
            {
                case 0x33: // PN532_COMMAND_RFCONFIGURATION + 1
                    s_Resp += ": RF Configuration";
                    break;

                case 0x4B: // PN532_COMMAND_INLISTPASSIVETARGET + 1
                    int s32_SAK = i_Payload[6];
                    String s_CardType = "???";
                    switch (s32_SAK)
                    {
                        case 0x00: s_CardType = "Ultralight"; break;
                        case 0x08: s_CardType = "Classic";    break;
                        case 0x18: s_CardType = "Classic";    break;
                        case 0x09: s_CardType = "Mini";       break;
                        case 0x20: s_CardType = "DESfire";    break;
                        // TODO: Implement more card types
                    }
                    s_Resp += String.Format(": Read Passive Target ID\nCards Found: {0}\nCard  Type:  ATQA= {1:X2}{2:X2}, SAK= {3:X2} (Mifare {4})\nCard  UID:   ",
                                            i_Payload[2], i_Payload[4], i_Payload[5], s32_SAK, s_CardType);

                    for (int U=0; U<i_Payload[7]; U++) // Payload[7] = UID length
                    {
                        s_Resp += i_Payload[U+8].ToString("X2");
                    }
                    break;

                // TODO: Implement more responses
            }

            i_RtfBuilder.AppendLine(Color.White, s_Resp);
        }

        // ========================================================================================

        /// <summary>
        /// PN532 Command and Response packets have the following format:
        /// -- any possible leading bytes, 
        /// -- Preamble   (00), 
        /// -- StartCode1 (00), 
        /// -- StartCode2 (FF), 
        /// -- PayloadLength, 
        /// -- LengthComplement (PayloadLength + LengthComplement = 0x100), 
        /// -- PayloadData      (D4/D5 + Command code/Response code + n Params), 
        /// -- Checksum, 
        /// -- Postamble (00), 
        /// -- any possible trailing bytes.
        /// 
        /// The first  payload byte is either D4 = HostToPn532 for commands or D5 = Pn532ToHost for responses.
        /// The second payload byte is the command code for commands or command code + 1 for responses.
        /// Error frames contain one payload byte with the error code.
        /// 
        /// Exceptions from this rule are:
        /// ACK:   00, 00, FF, 00, FF, 00
        /// NACK:  00, 00, FF, FF, 00, 00
        /// </summary>
        List<Byte> ExtractPayload(List<Byte> i_Data, out eFrameType e_Frame)
        {
            e_Frame = eFrameType.Invalid;

            if (i_Data == null || i_Data.Count < 6) // ACK, NACK have 6 bytes
                return null; // invalid data

            int s32_Startcode = Utils.FindBytes(i_Data, 0, 0x00, 0xFF); // Startcode1 + Startcode2
            if (s32_Startcode < 0)
                return null; // corrupt data

            int s32_PayloadStart  = s32_Startcode + 4;
            int s32_PayloadLength = i_Data[s32_Startcode + 2];
            if (s32_PayloadLength == 0x00)
            {
                e_Frame = eFrameType.ACK;
                return null;
            }

            if (s32_PayloadLength == 0xFF)
            {
                e_Frame = eFrameType.NAK;
                return null;
            }

            List<Byte> i_Payload = Utils.ExtractBytes(i_Data, s32_PayloadStart, s32_PayloadLength);
            if (i_Payload == null)
                return null; // packet too short

            // -----------------------------

            int s32_CheckByte = s32_PayloadStart + s32_PayloadLength;
            Byte u8_Checksum  = 0;
            for (int B=s32_Startcode; B<s32_CheckByte; B++)
            {
                u8_Checksum += i_Data[B];
            }
            if (u8_Checksum != (Byte)(~i_Data[s32_CheckByte]))
                return null; // invalid checksum

            // -----------------------------

            // Error frames contain only 1 payload byte (= error code)
            if (i_Payload.Count == 1)
                e_Frame = eFrameType.Error;

            // Data frames must have at least D4/D5 + Command code/Response code
            if (i_Payload.Count >= 2)
                e_Frame = eFrameType.Data;
            
            return i_Payload;
        }
    }
}
