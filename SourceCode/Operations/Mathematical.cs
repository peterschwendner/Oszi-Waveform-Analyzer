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
using Utils             = OsziWaveformAnalyzer.Utils;
using OsziPanel         = OsziWaveformAnalyzer.OsziPanel;
using Capture           = OsziWaveformAnalyzer.Utils.Capture;
using Channel           = OsziWaveformAnalyzer.Utils.Channel;
using GraphMenuItem     = Operations.OperationManager.GraphMenuItem;

namespace Operations
{
    public partial class Mathematical : Form, IOperation
    {
        const String MATH_SUBTRACT_AB = "Subtract Channel A - B";
        const String MATH_SUBTRACT_BA = "Subtract Channel B - A";

        Channel mi_ChannelA;
        Channel mi_ChannelB;
        bool    mb_Analog;

        /// <summary>
        /// Implementation of interface IOperation
        /// </summary>
        public void GetMenuItems(Channel i_Channel, bool b_Analog, List<GraphMenuItem> i_Items)
        {
            if (i_Channel == null)
                return; // user did not click on a Channel

            if (b_Analog && Utils.OsziPanel.CommonAnalogDrawing)
                return; // Analog channels cannot be distinguished while drawn one on top of the other

            if (OsziPanel.CurCapture.mi_Channels.Count < 2)
                return; // at least 2 channels are required

            GraphMenuItem i_Item = new GraphMenuItem();
            i_Item.ms_MenuText  = "Math Operations";
            i_Item.ms_ImageFile = "Calculator.ico";
            i_Items.Add(i_Item);
        }

        /// <summary>
        /// Implementation of interface IOperation
        /// </summary>
        public String Execute(Channel i_ChannelA, int s32_Sample, bool b_Analog, Object o_Tag)
        {
            mb_Analog   = b_Analog;
            mi_ChannelA = i_ChannelA;

            InitializeComponent();
            ShowDialog(Utils.FormMain);
            return null;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            comboMath.Items.Add(MATH_SUBTRACT_AB);
            comboMath.Items.Add(MATH_SUBTRACT_BA);

            lblMathMode.Text = mb_Analog ? "Analog" : "Digital";

            lblNameA.Text = mi_ChannelA.ms_Name;
            lblNameA   .ForeColor = OsziPanel.GetChannelColor(mi_ChannelA);
            lblChannelA.ForeColor = lblNameA.ForeColor;

            foreach (Channel i_ChannelB in OsziPanel.CurCapture.mi_Channels)
            {
                if (i_ChannelB != mi_ChannelA)
                    comboChannelB.Items.Add(i_ChannelB);
            }

            comboChannelB.SelectedIndex = 0;
            comboMath.SelectedIndex     = 0;
        }

        private void linkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Utils.ShowHelp(this, "SplitHalfDuplex");
        }

        private void comboChannelB_SelectedIndexChanged(object sender, EventArgs e)
        {
            mi_ChannelB = (Channel)comboChannelB.SelectedItem;
            lblChannelB.ForeColor = OsziPanel.GetChannelColor(mi_ChannelB);
        }

        private void btnExecuteClose_Click(object sender, EventArgs e)
        {
            Execute(true);
        }
        private void btnExecute_Click(object sender, EventArgs e)
        {
            Execute(false);
        }
        void Execute(bool b_Close)
        {
            try
            {
                if (mb_Analog && (mi_ChannelA.mf_Analog == null || mi_ChannelB.mf_Analog == null))
                {
                    MessageBox.Show(this, "If you right-click on an analog channel you must select two analog channels.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!mb_Analog && (mi_ChannelA.mu8_Digital == null || mi_ChannelB.mu8_Digital == null))
                {
                    MessageBox.Show(this, "If you right-click on a digital channel you must select two digital channels.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Channel i_OperandA = mi_ChannelA;
                Channel i_OperandB = mi_ChannelB;
                Channel i_Result   = OsziPanel.CurCapture.FindOrCreateChannel(textResult.Text, mi_ChannelA);
                int    s32_Samples = OsziPanel.CurCapture.ms32_Samples;

                switch (comboMath.Text)
                {
                    case MATH_SUBTRACT_BA:
                //  case FUTURE_OPERAION:
                        i_OperandA = mi_ChannelB; // Swap channels if required
                        i_OperandB = mi_ChannelA;
                        break;
                }

                if (mb_Analog)
                {
                    i_Result.mf_Analog = new float[s32_Samples];
                    i_Result.mi_SampleMinMax.mb_AnalogOK = false; // must be re-calculated

                    switch (comboMath.Text)
                    {
                        case MATH_SUBTRACT_AB:
                        case MATH_SUBTRACT_BA:
                            for (int S=0; S<s32_Samples; S++)
                            {
                                i_Result.mf_Analog[S] = i_OperandA.mf_Analog[S] - i_OperandB.mf_Analog[S];
                            }
                            break;

                    //  case FUTURE_OPERATION:
                    //      ** TODO **
                    //      break;
                    }
                }
                else // Digital
                {
                    i_Result.mu8_Digital = new Byte[s32_Samples];
                    i_Result.mi_MarkRows = null;
                    i_Result.mi_SampleMinMax.mb_DigitalOK = false; // must be re-calculated

                    switch (comboMath.Text)
                    {
                        case MATH_SUBTRACT_AB:
                        case MATH_SUBTRACT_BA:
                            for (int S=0; S<s32_Samples; S++)
                            {
                                // Only bit 0 is used for digital data
                                i_Result.mu8_Digital[S] = (Byte)((i_OperandA.mu8_Digital[S] - i_OperandB.mu8_Digital[S]) & 0x01);
                            }
                            break;

                    //  case FUTURE_OPERATION:
                    //      ** TODO **
                    //      break;
                    }
                }

                Utils.OsziPanel.RecalculateEverything();

                if (b_Close)
                    DialogResult = DialogResult.OK;
            }
            catch (Exception Ex)
            {
                Utils.ShowExceptionBox(this, Ex);
            }
        }
    }
}

