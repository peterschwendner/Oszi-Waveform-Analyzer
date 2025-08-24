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
using System.Globalization;
using System.Windows.Forms;

using OperationManager  = Operations.OperationManager;
using IOperation        = Operations.OperationManager.IOperation;
using Utils             = OsziWaveformAnalyzer.Utils;
using OsziPanel         = OsziWaveformAnalyzer.OsziPanel;
using Capture           = OsziWaveformAnalyzer.Utils.Capture;
using Channel           = OsziWaveformAnalyzer.Utils.Channel;
using GraphMenuItem     = Operations.OperationManager.GraphMenuItem;
using PlatformManager   = Platform.PlatformManager;

namespace Operations
{
    public partial class Mathematical : Form, IOperation
    {
        // Single Channel Analog
        const String MATH_FACTOR      = "Multiply with factor";
        const String MATH_OFFSET      = "Add Offset";
        // Dual Channel Analog
        const String MATH_ADD_AB      = "Add Channel A + B";
        const String MATH_SUBTRACT_AB = "Subtract Channel A - B";
        const String MATH_SUBTRACT_BA = "Subtract Channel B - A";
        const String MATH_MULTI_AB    = "Multiply Channel A x B";
        // Dual Channel Digital
        const String MATH_LOGIC_AND   = "Logical AND";
        const String MATH_LOGIC_OR    = "Logical OR";
        const String MATH_LOGIC_XOR   = "Logical XOR";

        Channel mi_ChannelA;
        Channel mi_ChannelB;
        Channel mi_Result;
        bool    mb_Analog;
        bool    mb_Single;

        /// <summary>
        /// Implementation of interface IOperation
        /// </summary>
        public void GetMenuItems(Channel i_Channel, bool b_Analog, List<GraphMenuItem> i_Items)
        {
            if (i_Channel == null)
                return; // user did not click on a Channel

            if (b_Analog && Utils.OsziPanel.CommonAnalogDrawing)
                return; // Analog channels cannot be distinguished while drawn one on top of the other

            if (!b_Analog && OsziPanel.CurCapture.ms32_DigitalCount < 2)
                return; // at least 2 digital channels are required for AND, OR, XOR

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

            if (mb_Analog)
            {
                if (OsziPanel.CurCapture.mi_Channels.Count > 1)
                {
                    comboMath.Items.Add(MATH_ADD_AB);
                    comboMath.Items.Add(MATH_SUBTRACT_AB);
                    comboMath.Items.Add(MATH_SUBTRACT_BA);
                    comboMath.Items.Add(MATH_MULTI_AB);
                }
                comboMath.Items.Add(MATH_FACTOR);
                comboMath.Items.Add(MATH_OFFSET);
            }
            else // Digital
            {
                comboMath.Items.Add(MATH_LOGIC_AND);
                comboMath.Items.Add(MATH_LOGIC_OR);
                comboMath.Items.Add(MATH_LOGIC_XOR); // This is the same as subtracting digital A - B  or  B - A
            }

            lblMathMode.Text = mb_Analog ? "Analog" : "Digital";

            // Dual
            lblNameA   .Text      = mi_ChannelA.ms_Name;
            lblNameA   .ForeColor = OsziPanel.GetChannelColor(mi_ChannelA);
            lblChannelA.ForeColor = lblNameA.ForeColor;

            // Single
            lblName    .Text      = lblNameA.Text;
            lblName    .ForeColor = lblNameA.ForeColor;
            lblChannel .ForeColor = lblNameA.ForeColor;

            foreach (Channel i_ChannelB in OsziPanel.CurCapture.mi_Channels)
            {
                if (i_ChannelB != mi_ChannelA)
                    comboChannelB.Items.Add(i_ChannelB);
            }

            comboChannelB.SelectedIndex = 0;
            comboMath.SelectedIndex     = 0;
        }

        private void comboMath_SelectedIndexChanged(object sender, EventArgs e)
        {
            mb_Single = false;
            switch (comboMath.Text)
            {
                case MATH_FACTOR:
                    mb_Single = true;
                    lblParam.Text = "Factor:";
                    lblUnit .Text = "";
                    break;
                case MATH_OFFSET:
                    mb_Single = true;
                    lblParam.Text = "Offset:";
                    lblUnit .Text = "Volt";
                    break;
            }

            groupSingle.Visible =  mb_Single;
            groupDual  .Visible = !mb_Single;

            GroupBox i_GroupVisible = mb_Single ? groupSingle : groupDual;
            i_GroupVisible.Top = lblMathMode.Bottom + 10;

            Size k_Client   = ClientSize;
            k_Client.Height = i_GroupVisible.Bottom + 45;
            ClientSize      = k_Client;
        }

        private void linkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            PlatformManager.Instance.ShowHelp(this, "SplitHalfDuplex");
        }

        private void comboChannelB_SelectedIndexChanged(object sender, EventArgs e)
        {
            mi_ChannelB = (Channel)comboChannelB.SelectedItem;
            lblChannelB.ForeColor = OsziPanel.GetChannelColor(mi_ChannelB);
        }

        private void btnExecuteClose_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            Calculate(true);
            Cursor = Cursors.Arrow;
        }
        private void btnExecute_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            Calculate(false);
            Cursor = Cursors.Arrow;
        }
        void Calculate(bool b_Close)
        {
            try
            {
                Prepare(); // throws

                int s32_Samples = OsziPanel.CurCapture.ms32_Samples;

                if (mb_Analog)
                {
                    switch (comboMath.Text)
                    {
                        case MATH_ADD_AB: // dual
                            for (int S=0; S<s32_Samples; S++)
                                mi_Result.mf_Analog[S] = mi_ChannelA.mf_Analog[S] + mi_ChannelB.mf_Analog[S];
                            break;

                        case MATH_SUBTRACT_AB: // dual
                            for (int S=0; S<s32_Samples; S++)
                                mi_Result.mf_Analog[S] = mi_ChannelA.mf_Analog[S] - mi_ChannelB.mf_Analog[S];
                            break;

                        case MATH_SUBTRACT_BA: // dual
                            for (int S=0; S<s32_Samples; S++)
                                mi_Result.mf_Analog[S] = mi_ChannelB.mf_Analog[S] - mi_ChannelA.mf_Analog[S];
                            break;

                        case MATH_MULTI_AB: // dual
                            for (int S=0; S<s32_Samples; S++)
                                mi_Result.mf_Analog[S] = mi_ChannelA.mf_Analog[S] * mi_ChannelB.mf_Analog[S];
                            break;

                        case MATH_FACTOR: // single
                            float f_Factor;
                            if (!float.TryParse(textParameter.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out f_Factor))
                                throw new Exception("Enter a valid factor.");

                            for (int S=0; S<s32_Samples; S++)
                                mi_ChannelA.mf_Analog[S] *= f_Factor;

                            mi_ChannelA.mi_SampleMinMax.mb_AnalogOK = false;
                            break;

                        case MATH_OFFSET: // single
                            float f_Offset;
                            if (!float.TryParse(textParameter.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out f_Offset))
                                throw new Exception("Enter a valid offset.");

                            for (int S=0; S<s32_Samples; S++)
                                mi_ChannelA.mf_Analog[S] += f_Offset;

                            mi_ChannelA.mi_SampleMinMax.mb_AnalogOK = false;
                            break;
                    }
                }
                else // Digital (Only bit 0 is used)
                {
                    switch (comboMath.Text)
                    {
                        case MATH_LOGIC_AND: // dual
                            for (int S=0; S<s32_Samples; S++)
                                mi_Result.mu8_Digital[S] = (Byte)((mi_ChannelA.mu8_Digital[S] & mi_ChannelB.mu8_Digital[S]) & 0x01);
                            break;

                        case MATH_LOGIC_OR: // dual
                            for (int S=0; S<s32_Samples; S++)
                                mi_Result.mu8_Digital[S] = (Byte)((mi_ChannelA.mu8_Digital[S] | mi_ChannelB.mu8_Digital[S]) & 0x01);
                            break;

                        case MATH_LOGIC_XOR: // dual (This is the same as subtracting A - B  or  B - A)
                            for (int S=0; S<s32_Samples; S++)
                                mi_Result.mu8_Digital[S] = (Byte)((mi_ChannelA.mu8_Digital[S] ^ mi_ChannelB.mu8_Digital[S]) & 0x01);
                            break;
                    }
                }

                OsziPanel.CurCapture.mb_Dirty = true; // user has unsaved changes
                Utils.OsziPanel.RecalculateEverything();

                if (b_Close)
                    DialogResult = DialogResult.OK;
            }
            catch (Exception Ex)
            {
                Utils.ShowExceptionBox(this, Ex);
            }
        }

        void Prepare()
        {
            if (mb_Single)
            {
                mi_Result = null;
                return;
            }

            int s32_Samples = OsziPanel.CurCapture.ms32_Samples;
            mi_Result       = OsziPanel.CurCapture.FindOrCreateChannel(textResult.Text, mi_ChannelA, mi_ChannelB);
            
            if (mb_Analog)
            {
                if (mi_ChannelA.mf_Analog == null || mi_ChannelB.mf_Analog == null)
                    throw new Exception("If you right-click on an analog channel you must select two analog channels.");

                if (mi_Result.mf_Analog == null)
                    mi_Result.mf_Analog = new float[s32_Samples];

                mi_Result.mi_SampleMinMax.mb_AnalogOK = false; // must be re-calculated
            }
            else // digital
            {
                if (mi_ChannelA.mu8_Digital == null || mi_ChannelB.mu8_Digital == null)
                    throw new Exception("If you right-click on a digital channel you must select two digital channels.");

                if (mi_Result.mu8_Digital == null)
                    mi_Result.mu8_Digital = new Byte[s32_Samples];

                mi_Result.mi_MarkRows = null;
                mi_Result.mi_SampleMinMax.mb_DigitalOK = false; // must be re-calculated
            }

            if (!comboChannelB.Items.Contains(mi_Result))
                 comboChannelB.Items.Add(mi_Result);
        }
    }
}

