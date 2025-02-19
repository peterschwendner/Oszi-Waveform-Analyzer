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
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using OperationManager  = Operations.OperationManager;
using IOperation        = Operations.OperationManager.IOperation;
using Capture           = OsziWaveformAnalyzer.Utils.Capture;
using Channel           = OsziWaveformAnalyzer.Utils.Channel;
using GraphMenuItem     = Operations.OperationManager.GraphMenuItem;
using Utils             = OsziWaveformAnalyzer.Utils;
using OsziPanel         = OsziWaveformAnalyzer.OsziPanel;

namespace Operations
{
    public class InvertChannel : IOperation
    {
        /// <summary>
        /// Implementation of interface IOperation
        /// </summary>
        public void GetMenuItems(Channel i_Channel, bool b_Analog, List<GraphMenuItem> i_Items)
        {
            if (i_Channel == null)
                return; // user did not click on a Channel

            if (b_Analog && Utils.OsziPanel.CommonAnalogDrawing)
                return; // Analog channels cannot be distinguished while drawn one on top of the other

            GraphMenuItem i_Item = new GraphMenuItem();
            i_Item.ms_MenuText  = "Invert Channel";
            i_Item.ms_ImageFile = "ArrowUpDown.ico";
            i_Items.Add(i_Item);
        }

        /// <summary>
        /// Implementation of interface IOperation
        /// </summary>
        public String Execute(Channel i_Channel, int s32_Sample, bool b_Analog, Object o_Tag)
        {
            if (i_Channel.mf_Analog != null && b_Analog)
            {
                float f_Sum = i_Channel.mf_Min + i_Channel.mf_Max;
                for (int i=0; i<i_Channel.mf_Analog.Length; i++)
                {
                    i_Channel.mf_Analog[i] = f_Sum - i_Channel.mf_Analog[i];
                }

                // must be re-calculated
                i_Channel.mi_SampleMinMax.mb_AnalogOK = false; 
            }

            if (i_Channel.mu8_Digital != null && !b_Analog)
            {
                for (int i=0; i<i_Channel.mu8_Digital.Length; i++)
                {
                    i_Channel.mu8_Digital[i] ^= 0x01;
                }

                // must be recalculated
                i_Channel.mi_SampleMinMax.mb_DigitalOK = false;
                i_Channel.mi_MarkRows = null;
            }

            Utils.OsziPanel.RecalculateEverything();
            return "Channel inverted.";
        }
    }
}

