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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

using OperationManager  = Operations.OperationManager;
using IOperation        = Operations.OperationManager.IOperation;
using Capture           = OsziWaveformAnalyzer.Utils.Capture;
using Channel           = OsziWaveformAnalyzer.Utils.Channel;
using SmplMark          = OsziWaveformAnalyzer.Utils.SmplMark;
using eMark             = OsziWaveformAnalyzer.Utils.eMark;
using GraphMenuItem     = Operations.OperationManager.GraphMenuItem;
using Utils             = OsziWaveformAnalyzer.Utils;
using OsziPanel         = OsziWaveformAnalyzer.OsziPanel;

namespace Operations
{
    /// <summary>
    /// Delete the samples before or after the click location
    /// </summary>
    public class DeleteSamples : IOperation
    {
        /// <summary>
        /// Implementation of interface IOperation
        /// </summary>
        public void GetMenuItems(Channel i_Channel, bool b_Analog, List<GraphMenuItem> i_Items)
        {
            GraphMenuItem i_Before = new GraphMenuItem();
            i_Before.ms_MenuText   = "Delete all samples before the mouse position";
            i_Before.ms_ImageFile  = "ArrowLeft.ico";
            i_Before.mo_Tag        = "Before";
            i_Items.Add(i_Before);

            GraphMenuItem i_After  = new GraphMenuItem();
            i_After.ms_MenuText    = "Delete all samples after the mouse position";
            i_After.ms_ImageFile   = "ArrowRight.ico";
            i_After.mo_Tag         = "After";
            i_Items.Add(i_After);
            
            GraphMenuItem i_Between = new GraphMenuItem();
            i_Between.ms_MenuText   = "Delete all samples between the cursor and the mouse position";
            i_Between.ms_ImageFile  = "ArrowLeftRight.ico";
            i_Between.mo_Tag        = "Between";
            i_Items.Add(i_Between);
        }

        /// <summary>
        /// Implementation of interface IOperation
        /// </summary>
        public String Execute(Channel i_ClickChannel, int s32_Sample, bool b_Analog, Object o_Tag)
        {
            Capture i_Capt = OsziPanel.CurCapture;
            int s32_Delete;

            if ((String)o_Tag == "Between")
            {
                int s32_CursorSpl = Utils.OsziPanel.CursorSample;
                if (s32_CursorSpl < 0)
                    throw new Exception("You must set the cursor to the start sample, then right-click on the end sample to delete the range between.");

                int s32_Before = Math.Min(s32_Sample, s32_CursorSpl);
                int s32_After  = Math.Max(s32_Sample, s32_CursorSpl);
                int s32_End    = i_Capt.ms32_Samples - s32_After;

                s32_Delete = s32_After - s32_Before;
                i_Capt.ms32_Samples -= s32_Delete;

                foreach (Channel i_Channel in i_Capt.mi_Channels)
                {
                    if (i_Channel.mf_Analog != null)
                    {
                        float[] f_NewAnalog = new float[i_Capt.ms32_Samples];
                        Array.Copy(i_Channel.mf_Analog, 0,         f_NewAnalog, 0,          s32_Before);
                        Array.Copy(i_Channel.mf_Analog, s32_After, f_NewAnalog, s32_Before, s32_End);
                        i_Channel.mf_Analog = f_NewAnalog;
                    }

                    if (i_Channel.mu8_Digital != null)
                    {
                        Byte[] u8_NewDigital = new Byte[i_Capt.ms32_Samples];
                        Array.Copy(i_Channel.mu8_Digital, 0,         u8_NewDigital, 0,          s32_Before);
                        Array.Copy(i_Channel.mu8_Digital, s32_After, u8_NewDigital, s32_Before, s32_End);
                        i_Channel.mu8_Digital = u8_NewDigital;
                    }

                    // Must be calculated anew
                    i_Channel.mi_MarkRows  = null;
                    i_Channel.mb_Threshold = false;
                }

                // Delete and adjust separators
                List<int> i_SepList = new List<int>();
                foreach (int s32_Sep in i_Capt.ms32_Separators)
                {
                    if (s32_Sep < s32_Before)
                        i_SepList.Add(s32_Sep); // copy unchanged

                    if (s32_Sep > s32_After)
                        i_SepList.Add(s32_Sep - s32_Delete); // move left
                }
                i_Capt.ms32_Separators = i_SepList.ToArray();
            }
            else // Before / After
            {
                int s32_Begin = 0;
                int s32_End   = 0;
                switch ((String)o_Tag)
                {
                    case "Before": s32_Begin = s32_Sample; break;
                    case "After":  s32_End   = i_Capt.ms32_Samples - s32_Sample; break;
                    default: throw new ArgumentException();
                }

                // Either Begin or End is zero here.
                s32_Delete = s32_Begin + s32_End;
                i_Capt.ms32_Samples -= s32_Delete;

                foreach (Channel i_Channel in i_Capt.mi_Channels)
                {
                    if (i_Channel.mf_Analog != null)
                    {
                        float[] f_NewAnalog = new float[i_Capt.ms32_Samples];
                        Array.Copy(i_Channel.mf_Analog, s32_Begin, f_NewAnalog, 0, i_Capt.ms32_Samples);
                        i_Channel.mf_Analog = f_NewAnalog;
                    }

                    if (i_Channel.mu8_Digital != null)
                    {
                        Byte[] u8_NewDigital = new Byte[i_Capt.ms32_Samples];
                        Array.Copy(i_Channel.mu8_Digital, s32_Begin, u8_NewDigital, 0, i_Capt.ms32_Samples);
                        i_Channel.mu8_Digital = u8_NewDigital;
                    }

                    // Must be calculated anew
                    i_Channel.mi_MarkRows  = null;
                    i_Channel.mb_Threshold = false;
                }

                // Delete and adjust separators
                List<int> i_SepList = new List<int>();
                foreach (int s32_Sep in i_Capt.ms32_Separators)
                {
                    if (s32_Begin > 0 && s32_Sep > s32_Sample)
                        i_SepList.Add(s32_Sep - s32_Delete); // move left

                    if (s32_End > 0 && s32_Sep < s32_Sample)
                        i_SepList.Add(s32_Sep); // copy
                }
                i_Capt.ms32_Separators = i_SepList.ToArray();
            }

            // force recalculation of all Min/Max values of all channels
            i_Capt.ResetSampleMinMax(); 

            // Remove cursor if behind the waveform
            if (Utils.OsziPanel.CursorSample >= i_Capt.ms32_Samples)
                Utils.OsziPanel.SetCursor(-1, -1m); 

            Utils.OsziPanel.RecalculateEverything();

            return s32_Delete + " samples deleted";
        }
    }
}

