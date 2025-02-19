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
    /// This class is only a demo for an UART post decoder.
    /// If you want to analyze ISO14230 data download ElmüSoft HUD ECU Hacker.
    /// </summary>
    public class ISO14230 : IPostDecode
    {
        #region enums

        public enum eService : byte
        {
            // All protocols: (see ISO 14229)
            OBD2_Get_Life_Data                  = 0x01,
            OBD2_Get_Freeze_Frame_Data          = 0x02,
            OBD2_Get_Stored_DTC                 = 0x03,
            OBD2_Clear_DTC                      = 0x04,
            OBD2_Test_Results_Non_CAN           = 0x05,
            OBD2_Test_Results_CAN               = 0x06,
            OBD2_Get_Pending_DTC                = 0x07,
            OBD2_Control_Operation              = 0x08,
            OBD2_Get_Vehicle_Info               = 0x09,
            OBD2_Get_Permanent_DTC              = 0x0A,
            Read_DTC                            = 0x13,
            Clear_DTC                           = 0x14,
            Read_DTC_by_Status                  = 0x18,
            Read_Data_by_Local_Id               = 0x21,
            Read_Memory_by_Address              = 0x23,
            Keep_Alive                          = 0x3E,
            Start_Communication                 = 0x81, 
            Stop_Communication                  = 0x82, 
            Access_Timing_Parameters            = 0x83,
            // and more....
        }

        public enum eFailure : byte 
        {
            General_reject                             = 0x10,
            Service_not_supported                      = 0x11,
            Subfunction_not_supported                  = 0x12,
			Incorrect_message_length_or_invalid_format = 0x13,
            Request_out_of_range                       = 0x31,
            // and more....
        }

        #endregion

        /// <summary>
        /// If the packet contains valid data, one or multiple lines are added to the RTF output,
        /// otherwise the packet will be ignored.
        /// </summary>
        public void DecodeUART(UartPacket i_Packet, RtfBuilder i_RtfBuilder)
        {
            Byte u8_DestAdr;
            Byte u8_SrcAdr;

            List<Byte> i_Payload = ExtractPayload(i_Packet, out u8_DestAdr, out u8_SrcAdr);
            if (i_Payload == null)
                return; // invalid packet

            i_RtfBuilder.AppendFormat(Color.White, "{0:X2} --> {1:X2} ", u8_SrcAdr, u8_DestAdr);

            Byte u8_Service = i_Payload[0];
            if (u8_Service == 0x7F) // ECU responds with failure code
            {
                Byte  u8_ErrCode = i_Payload[2];
                String s_Failure;
                if (Enum.IsDefined(typeof(eFailure), u8_ErrCode))
                    s_Failure = ((eFailure)u8_ErrCode).ToString().Replace('_',' ');
                else
                    s_Failure = "Code " + u8_ErrCode.ToString("X2");

                i_RtfBuilder.AppendLine(Utils.ERROR_COLOR, "ECU Error: " + s_Failure);
                return;
            }

            // Response code = Command code | 0x40
            String s_Cmd = "Command  ";
            Color  c_Col = Color.White;
            if ((u8_Service & 0x40) > 0)
            {
                s_Cmd = "Response ";
                c_Col = Color.Lime;
                u8_Service -= 0x40;
            }

            eService e_Service = (eService)u8_Service;
            String   s_Service;
            if (Enum.IsDefined(typeof(eService), u8_Service))
                s_Service = e_Service.ToString().Replace('_',' ');
            else
                s_Service = "Service " + u8_Service.ToString("X2");

            if (e_Service == eService.OBD2_Get_Life_Data ||  // service 1
                e_Service == eService.OBD2_Get_Vehicle_Info) // service 9
            {
                if ((i_Payload[1] & 0x1F) == 0)
                    s_Service += " - Get Supported PID's";
            }

            // Not implemented: Decode command + response parameters
            // If you want to analyze ISO14230 data download ElmüSoft HUD ECU Hacker !

            i_RtfBuilder.AppendLine(c_Col, s_Cmd + s_Service);
        }

        public void DecodeSPI(SpiPacket i_Packet, RtfBuilder i_RtfBuilder)
        {
            throw new NotImplementedException();
        }
        public void DecodeI2C(I2CPacket i_Packet, RtfBuilder i_RtfBuilder)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// ISO 14230 packets consist of a header, payload and checksum
        /// The length of the payload is stored in the lower 6 bits of the first byte (the "Format" byte) 
        /// if the packet has less than 0x3F bytes. For longer packets an additional "Length byte" is inserted.
        /// NOTE: 
        /// This is a very simple decoder which relies on pauses between Command and Response packets.
        /// If the ISO 14230 traffic is very fast without pauses between packets this decoder will fail.
        /// For professional ISO 14230 analysis use my software HUD ECU Hacker.
        /// </summary>
        List<Byte> ExtractPayload(UartPacket i_Packet, out Byte u8_DestAdr, out Byte u8_SrcAdr)
        {
            u8_DestAdr = 0;
            u8_SrcAdr  = 0;
            int s32_HeadLen = 0;

            if (i_Packet.mi_Data.Count < 3)
                return null; // packet incomplete or corrupt

            // Read the "Format" byte which contains information about the packet length
            int s32_Format = i_Packet.mi_Data[s32_HeadLen++];

            if ((s32_Format & 0x80) != 0) // target and source address present in packet
            {
                if (i_Packet.mi_Data.Count < s32_HeadLen + 2)
                    return null; // packet incomplete or corrupt

                u8_DestAdr = (Byte)i_Packet.mi_Data[s32_HeadLen++];
                u8_SrcAdr  = (Byte)i_Packet.mi_Data[s32_HeadLen++];
            }

            if (i_Packet.mi_Data.Count < s32_HeadLen + 1)
                return null; // packet incomplete or corrupt

            int s32_DataLen = s32_Format & 0x3F;
            if (s32_DataLen == 0) 
                s32_DataLen = i_Packet.mi_Data[s32_HeadLen++]; // read extra length byte

            int s32_TotLen = s32_HeadLen + s32_DataLen + 1;
            if (i_Packet.mi_Data.Count < s32_TotLen)
                return null; // packet incomplete or corrupt

            Byte u8_CheckSum = 0;
            for (int C=0; C<s32_TotLen -1; C++)
            {
                u8_CheckSum += (Byte)i_Packet.mi_Data[C];
            }

            if (u8_CheckSum != i_Packet.mi_Data[s32_TotLen -1])
                return null; // invalid checksum

            List<Byte> i_Payload = new List<Byte>();
            for (int D=0; D<s32_DataLen; D++)
            {
                i_Payload.Add((Byte)i_Packet.mi_Data[s32_HeadLen + D]);
            }            
            return i_Payload;
        }
    }
}
