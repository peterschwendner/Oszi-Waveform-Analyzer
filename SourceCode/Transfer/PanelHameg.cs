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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

using WForms            = System.Windows.Forms;
using ITransferPanel    = Transfer.TransferManager.ITransferPanel;
using eOsziSerie        = Transfer.TransferManager.eOsziSerie;
using eRegKey           = OsziWaveformAnalyzer.Utils.eRegKey;
using Utils             = OsziWaveformAnalyzer.Utils;
using Capture           = OsziWaveformAnalyzer.Utils.Capture;
using eOperation        = Transfer.Hameg.eOperation;
using OsziConfig        = Transfer.Hameg.OsziConfig;
using WaterfallFFT      = Operations.WaterfallFFT;
using IWaterfallSource  = Operations.IWaterfallSource;
using Fourier           = Operations.Fourier;

namespace Transfer
{
    /// <summary>
    /// The controls in FormTransfer for Hameg oscilloscopes HMO1522 (HMO Compact series) and HM2008 (CombiScopes).
    /// These are connected over RS232 (Hameg HO720 interface) or USB virtual COM port or Ethernet (HO730).
    /// </summary>
    public partial class PanelHameg : UserControl, ITransferPanel
    {
        SCPI         mi_Scpi;
        IHamegScope  mi_Hameg;
        eOsziSerie   me_OsziSerie;
        FormTransfer mi_Form;
        WForms.Timer mi_RefreshTimer;

        public PanelHameg()
        {
            InitializeComponent();
        }

        // This is called when FormTransfer is opened
        public void OnLoad(eOsziSerie e_OsziSerie)
        {
            me_OsziSerie = e_OsziSerie;
            mi_Form      = (FormTransfer)Parent;

            // RS232 is slow. Do not poll the scope too often, otherwise it is busy answering instead of acquiring.
            mi_RefreshTimer = new WForms.Timer();
            mi_RefreshTimer.Tick += new EventHandler(OnRefreshTimer);
            mi_RefreshTimer.Interval = 1500;

            lblSerie.Text = Utils.GetDescriptionAttribute(e_OsziSerie);
            ClearGroupBox();

            if (Utils.RegReadBool(eRegKey.HamegPoints))
                radioMemory.Checked = true;
            else
                radioScreen.Checked = true;
        }

        // Open the connection to the oscilloscope
        // May throw
        public void OnOpenDevice(SCPI i_Scpi)
        {
            mi_Scpi = i_Scpi;
            if (me_OsziSerie == eOsziSerie.Hameg_HM507)
                mi_Hameg = new HamegHM507();
            else
                mi_Hameg = new Hameg(me_OsziSerie);

            mi_Hameg.Connect(mi_Form, i_Scpi); // may throw

            LoadGroupBox();

            OnRefreshTimer(null, null); // starts the refresh timer
        }

        // Close the connection to the oscilloscope
        public void OnCloseDevice()
        {
            if (mi_Hameg != null)
            {
                mi_Hameg.Disconnect();
                mi_Hameg = null;
            }

            mi_RefreshTimer.Stop();
            ClearGroupBox();
        }

        // This is called periodically to update the oscilloscope settings in the Panel
        void OnRefreshTimer(object sender, EventArgs e)
        {
            mi_RefreshTimer.Stop();
            if (mi_Hameg == null)
                return;

            try
            {
                OsziConfig i_Config = mi_Hameg.GetOsziConfiguration();

                if (i_Config.md_TimeBase > 0)
                    SetLabel(lblTimeBase, false, Utils.FormatTimePico(i_Config.md_TimeBase * Utils.PICOS_PER_SECOND) + " / div");
                else
                    SetLabel(lblTimeBase, true, "UNKNOWN");

                if (i_Config.md_SampleRate > 0)
                    SetLabel(lblSampleRate, false, Utils.FormatFrequency(i_Config.md_SampleRate));
                else
                    SetLabel(lblSampleRate, false, "not reported");

                if (i_Config.ms_AcqState != null)
                    SetLabel(lblAcqState, i_Config.ms_AcqState == "RUN", i_Config.ms_AcqState);
                else
                    SetLabel(lblAcqState, false, "not reported");

                mi_RefreshTimer.Interval = 1500;
            }
            catch (Exception Ex)
            {
                SetLabel(lblTimeBase,   true, "ERROR");
                SetLabel(lblSampleRate, true, "ERROR");
                SetLabel(lblAcqState,   true, "ERROR");

                // TimeoutException:
                // This may happen when the wrong oscilloscope serie is selected or the baudrate is wrong.
                // Try again after a longer interval to avoid sending the same wrong command again and again.
                if (Ex is TimeoutException)
                    mi_RefreshTimer.Interval = 5000;
            }

            mi_RefreshTimer.Start();
        }

        void SetLabel(Label i_Label, bool b_Error, String s_Text)
        {
            if (b_Error) i_Label.ForeColor = Color.FromArgb(0xFF, 0xA0, 0x80); // Red is too dark
            else         i_Label.ForeColor = Color.Cyan;
            i_Label.Text = s_Text;
        }

        void LoadGroupBox()
        {
            if (mi_Hameg.Model != null)
            {
                SetLabel(lblBrand,    false, mi_Hameg.Model.ms_Brand);
                SetLabel(lblModel,    false, mi_Hameg.Model.ms_Model);
                SetLabel(lblSerial,   false, mi_Hameg.Model.ms_Serial);
                SetLabel(lblFirmware, false, mi_Hameg.Model.ms_Firmware);
            }
            else
            {
                SetLabel(lblBrand,    true, "ERROR");
                SetLabel(lblModel,    true, "ERROR");
                SetLabel(lblSerial,   true, "ERROR");
                SetLabel(lblFirmware, true, "ERROR");
            }

            btnReset .BackColor = Color.BlanchedAlmond;
            btnAuto  .BackColor = Color.LightSkyBlue;
            btnRun   .BackColor = Color.PaleGreen;
            btnStop  .BackColor = Color.Salmon;
            btnSingle.BackColor = Color.BlanchedAlmond;
            btnWaterfall.BackColor = Color.Plum;

            btnReset   .Visible = mi_Hameg.CanReset;
            radioMemory.Enabled = mi_Hameg.HasDeepMemory;
            if (!mi_Hameg.HasDeepMemory)
                radioScreen.Checked = true;

            groupTransfer.Enabled = true;
        }

        void ClearGroupBox()
        {
            lblBrand     .Text = "";
            lblModel     .Text = "";
            lblSerial    .Text = "";
            lblFirmware  .Text = "";
            lblTimeBase  .Text = "";
            lblSampleRate.Text = "";
            lblAcqState  .Text = "";

            btnReset .BackColor = SystemColors.Control;
            btnAuto  .BackColor = SystemColors.Control;
            btnRun   .BackColor = SystemColors.Control;
            btnStop  .BackColor = SystemColors.Control;
            btnSingle.BackColor = SystemColors.Control;
            btnWaterfall.BackColor = SystemColors.Control;

            groupTransfer.Enabled = false;
        }

        // ==========================================================

        private void btnReset_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(mi_Form, "This resets all settings of the oscilloscope to factory defaults.\nContinue?",
                                "Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                OnButtonOperation(eOperation.Reset);
        }
        private void btnAuto_Click(object sender, EventArgs e)
        {
            OnButtonOperation(eOperation.Auto);
        }
        private void btnRun_Click(object sender, EventArgs e)
        {
            OnButtonOperation(eOperation.Run);
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            OnButtonOperation(eOperation.Stop);
        }
        private void btnSingle_Click(object sender, EventArgs e)
        {
            OnButtonOperation(eOperation.Single);
        }
        void OnButtonOperation(eOperation e_Operation)
        {
            if (!Utils.StartBusyOperation(mi_Form))
                return;

            mi_RefreshTimer.Stop();
            try
            {
                mi_Hameg.ExecuteOperation(e_Operation);
            }
            catch (Exception Ex)
            {
                Utils.ShowExceptionBox(mi_Form, Ex);
            }
            OnRefreshTimer(null, null);

            Utils.EndBusyOperation(mi_Form);
        }

        // ==========================================================

        private void btnTransfer_Click(object sender, EventArgs e)
        {
            if (btnTransfer.Text == "Abort")
            {
                mi_Hameg.AbortTransfer();
                return;
            }

            if (Utils.FormMain.HasUnsavedChanges())
                return;

            if (!Utils.StartBusyOperation(mi_Form))
                return;

            Utils.RegWriteBool(eRegKey.HamegPoints, radioMemory.Checked);

            mi_RefreshTimer.Stop();
            btnTransfer.Text = "Abort";
            mi_Form.PrintStatus("Start Tansfer...", Color.Blue);

            try
            {
                Capture i_Capture = mi_Hameg.TransferAllChannels(radioMemory.Checked);
                if (i_Capture != null)
                {
                    Utils.FormMain.StoreNewCapture(i_Capture);
                    mi_Form.PrintStatus("Ready", Color.Green);

                    if (mi_Hameg.TransferWarnings != null)
                        MessageBox.Show(mi_Form, mi_Hameg.TransferWarnings, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    mi_Form.PrintStatus("Aborted", Color.Red);
                }
            }
            catch (Exception Ex)
            {
                Utils.ShowExceptionBox(mi_Form, Ex);
            }

            btnTransfer.Text = "Transfer";
            OnRefreshTimer(null, null);

            Utils.EndBusyOperation(mi_Form);
        }

        // ==========================================================

        /// <summary>
        /// Opens the Waterfall FFT, which acquires one channel after the other in a loop until the user clicks "Cancel".
        /// The oscilloscope is in STOP mode afterwards.
        /// </summary>
        private void btnWaterfall_Click(object sender, EventArgs e)
        {
            if (!Utils.StartBusyOperation(mi_Form))
                return;

            mi_RefreshTimer.Stop();

            String s_Source = lblSerie.Text;
            if (mi_Hameg.Model != null && mi_Hameg.Model.ms_Model != null)
                s_Source = (mi_Hameg.Model.ms_Brand + " " + mi_Hameg.Model.ms_Model).Trim();

            using (WaterfallFFT i_Waterfall = new WaterfallFFT(new HamegWaterfallSource(mi_Hameg, s_Source)))
            {
                i_Waterfall.ShowDialog(mi_Form);
            }

            OnRefreshTimer(null, null);
            Utils.EndBusyOperation(mi_Form);
        }

        /// <summary>
        /// The Hameg oscilloscope as source of the Waterfall FFT
        /// </summary>
        class HamegWaterfallSource : IWaterfallSource
        {
            IHamegScope mi_Hameg;
            String      ms_Name;

            public HamegWaterfallSource(IHamegScope i_Hameg, String s_Name)
            {
                mi_Hameg = i_Hameg;
                ms_Name  = s_Name;
            }

            public String        Name     { get { return ms_Name; } }
            public String[]      Channels { get { return new String[] { "CH1", "CH2" }; } }
            public Fourier.eUnit Unit     { get { return Fourier.eUnit.Volt; } }
            public String        StepInfo  { get { return null; } }
            public double        BlockTime { get { return double.NaN; } }

            public void    AddControls(FlowLayoutPanel i_Bar) {}
            public void    Start() {}
            public Capture Acquire(int s32_Channel) { return mi_Hameg.AcquireChannel(s32_Channel); }
            public void    Abort() { mi_Hameg.AbortTransfer(); }
            public void    Stop()  {}
        }

        // ==========================================================

        /// <summary>
        /// FormTransfer calls StartBusyOperation() before calling this function
        /// </summary>
        public void SendManualCommand(String s_Command, TextBox i_TextReponse)
        {
            mi_RefreshTimer.Stop();
            try
            {
                i_TextReponse.Text = mi_Hameg.SendManualCommand(s_Command);
                i_TextReponse.ForeColor = Color.Black;
            }
            catch (TimeoutException)
            {
                // It is not an error if a command like ":STOP" does not send a response
                i_TextReponse.Text = "No response received.";
                i_TextReponse.ForeColor = Color.Black;
            }
            catch (Exception Ex)
            {
                i_TextReponse.Text = Ex.Message;
                i_TextReponse.ForeColor = Color.Red;
            }
            OnRefreshTimer(null, null);
        }
    }
}
