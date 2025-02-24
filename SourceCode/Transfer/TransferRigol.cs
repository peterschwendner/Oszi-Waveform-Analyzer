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
using System.Threading;
using System.Text;
using System.Windows.Forms;

using WForms            = System.Windows.Forms;
using Capture           = OsziWaveformAnalyzer.Utils.Capture;
using Channel           = OsziWaveformAnalyzer.Utils.Channel;
using eOsziSerie        = Transfer.TransferManager.eOsziSerie;
using eOperation        = Transfer.Rigol.eOperation;
using OsziModel         = Transfer.Rigol.OsziModel;
using OsziConfig        = Transfer.Rigol.OsziConfig;
using ScpiCombo         = Transfer.SCPI.ScpiCombo;
using eRegKey           = OsziWaveformAnalyzer.Utils.eRegKey;
using Utils             = OsziWaveformAnalyzer.Utils;
using PlatformManager   = Platform.PlatformManager;

// This class implements communication with a Rigol oscilloscope
// Sadly there is no standard for this communication so each vendor cooks his own soup.
// It neither make sense to derive this class from a base class nor an interface because for another brand everything is different.
// So to add a future oscilloscope brand write a new communication class like class "Rigol" and create a new Form with the control elements.
// Rigol is so fucking inconsistent that for each oscilloscope serie other commands are used and different code must be written!
namespace Transfer
{
    public partial class TransferRigol : Form
    {
        eOsziSerie     me_OsziSerie;
        Rigol          mi_Rigol;
        WForms.Timer   mi_StatusTimer;
        WForms.Timer   mi_ConfigTimer;

        /// <summary>
        /// Constructor
        /// </summary>
        public TransferRigol(eOsziSerie e_OsziSerie)
        {
            me_OsziSerie = e_OsziSerie;
            InitializeComponent();

            textCommand.KeyDown += new KeyEventHandler(OnTextCommandKeyDown);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            statusLabel.Text  = "";
            statusLabel.Width = ClientSize.Width - 4;
            ClearGroupBox();

            if (Utils.RegReadBool(eRegKey.CaptureMemory))
                radioMemory.Checked = true;
            else
                radioScreen.Checked = true;

            textCommand.Text = Utils.RegReadString(eRegKey.SendCommand, "*IDN?");

            String s_Serie = Utils.GetDescriptionAttribute(me_OsziSerie);           
            this.Text += "  —  " + s_Serie; // Window Title
            lblSerie.Text = s_Serie;

            mi_StatusTimer = new WForms.Timer();
            mi_StatusTimer.Tick += new EventHandler(OnStatusTimer);
            mi_StatusTimer.Interval = 4000;

            mi_ConfigTimer = new WForms.Timer();
            mi_ConfigTimer.Tick += new EventHandler(OnConfigTimer);
            mi_ConfigTimer.Interval = 500;

            btnRefreshUSB_Click(null, null);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (Utils.IsBusy)
            {
                e.Cancel = true;
                MessageBox.Show(this, "An operation is still active.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            mi_StatusTimer.Stop();
            mi_ConfigTimer.Stop();

            // Close the USB connection
            if (mi_Rigol != null)
            {
                mi_Rigol.Dispose();
                mi_Rigol = null;
            }
        }

        // ==========================================================

        public void PrintStatus(String s_Text, Color c_Color)
        {
            statusLabel.Text      = s_Text;
            statusLabel.ForeColor = c_Color;
            Application.DoEvents();

            mi_StatusTimer.Stop();
            mi_StatusTimer.Start();
        }

        void OnStatusTimer(object sender, EventArgs e)
        {
            mi_StatusTimer.Stop();
            statusLabel.Text = "";
        }

        void OnConfigTimer(object sender, EventArgs e)
        {
            // The displayed values change when START/STOP is changing or more channels are enabled or the memory is configured on the scope
            try
            {
                OsziConfig i_Config = mi_Rigol.GetOsziConfiguration(radioMemory.Checked);

                SetLabel(lblSampleRate, false, Utils.FormatFrequency(i_Config.md_SampleRate));

                if (i_Config.ms32_SamplePoints == 0)
                    SetLabel(lblSamplePoints, true, "NOT READY");
                else
                    SetLabel(lblSamplePoints, false, i_Config.ms32_SamplePoints.ToString("N0"));
                
                if (i_Config.ms64_Duration == 0)
                    SetLabel(lblTotDuration, true, "NOT READY");
                else
                    SetLabel(lblTotDuration, false, Utils.FormatTimePico(i_Config.ms64_Duration));
            }
            catch (Exception Ex)
            {
                SetLabel(lblSampleRate,   true, "ERROR");
                SetLabel(lblSamplePoints, true, "ERROR");
                SetLabel(lblTotDuration,  true, "ERROR");

                // TimeoutException:
                // This happens when the wrong oscilloscope serie is selected.
                // The oscillosope does not respond to an unknown SCPI command and a timeout is the result.
                // Stop the timer to avoid sending the same wrong command again and again.
                if (Ex is TimeoutException)
                    mi_ConfigTimer.Stop();
            }
        }

        void SetLabel(Label i_Label, bool b_Error, String s_Text)
        {
            if (b_Error) i_Label.ForeColor = Color.FromArgb(0xFF, 0xA0, 0x80); // Red is too dark
            else         i_Label.ForeColor = Color.Cyan;
            i_Label.Text = s_Text;
        }

        // ==========================================================

        private void btnRefreshUSB_Click(object sender, EventArgs e)
        {
            try
            {
                PlatformManager.Instance.EnumerateScpiDevices(comboUsbDevice);
            }
            catch (Exception Ex)
            {
                Utils.ShowExceptionBox(this, Ex);
            }
        }

        private void btnInstallDriver_Click(object sender, EventArgs e)
        {
            PlatformManager.Instance.InstallDriver(this);
        }

        private void linkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            PlatformManager.Instance.ShowHelp(this, "SCPI");
        }

        // ==========================================================

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (btnOpen.Text == "Close")
            {
                mi_ConfigTimer.Stop();

                mi_Rigol.Dispose(); // IMPORTANT: Close native handle
                mi_Rigol = null;

                btnOpen.Text = "Open";
                ClearGroupBox();
                return;
            }

            if (comboUsbDevice.Items.Count == 0)
            {
                MessageBox.Show(this, "Please connect the oscilloscope over USB, turn it on and click 'Refresh'. "
                                    + "If it does not appear, click 'Install Diver'.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // If any device exists it has already been pre-selected in EnumerateScpiDevices()
                ScpiCombo i_Combo = (ScpiCombo)comboUsbDevice.SelectedItem;
            
                mi_Rigol = new Rigol(me_OsziSerie);
                mi_Rigol.Open(this, i_Combo); // throws

                btnOpen.Text = "Close";
                LoadGroupBox();
            }
            catch (Exception Ex)
            {
                mi_Rigol.Dispose(); // IMPORTANT: Close native handle
                mi_Rigol = null;

                Utils.ShowExceptionBox(this, Ex);
            }
        }

        // ==========================================================

        void LoadGroupBox()
        {
            lblBrand   .Text = mi_Rigol.Model.ms_Brand;
            lblModel   .Text = mi_Rigol.Model.ms_Model;
            lblSerial  .Text = mi_Rigol.Model.ms_Serial;
            lblFirmware.Text = mi_Rigol.Model.ms_Firmware;

            btnReset    .BackColor = Color.BlanchedAlmond;
            btnClear    .BackColor = Color.BlanchedAlmond;
            btnAuto     .BackColor = Color.LightSkyBlue;
            btnRun      .BackColor = Color.PaleGreen;
            btnStop     .BackColor = Color.Salmon;
            btnSingle   .BackColor = Color.BlanchedAlmond;
            btnForceTrig.BackColor = Color.BlanchedAlmond;

            groupCommand  .Enabled = true;
            groupCapture  .Enabled = true;
            comboUsbDevice.Enabled = false;
            btnRefreshUSB .Enabled = false;

            // OnConfigTimer() stops the timer on error
            mi_ConfigTimer.Start();    // FIRST
            OnConfigTimer(null, null); // AFTER (show samplerate)
        }

        void ClearGroupBox()
        {
            lblBrand       .Text = "";
            lblModel       .Text = "";
            lblSerial      .Text = "";
            lblFirmware    .Text = "";
            lblSampleRate  .Text = "";
            lblSamplePoints.Text = "";
            lblTotDuration .Text = "";
            textResponse   .Text = "";

            btnReset    .BackColor = SystemColors.Control;
            btnClear    .BackColor = SystemColors.Control;
            btnAuto     .BackColor = SystemColors.Control;
            btnRun      .BackColor = SystemColors.Control;
            btnStop     .BackColor = SystemColors.Control;
            btnSingle   .BackColor = SystemColors.Control;
            btnForceTrig.BackColor = SystemColors.Control;

            groupCommand  .Enabled = false;
            groupCapture  .Enabled = false;
            comboUsbDevice.Enabled = true;
            btnRefreshUSB .Enabled = true;
        }

        // ==========================================================

        private void btnClear_Click(object sender, EventArgs e)
        {
            OnButtonOperation(eOperation.Clear);
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
        private void btnForceTrig_Click(object sender, EventArgs e)
        {
            OnButtonOperation(eOperation.ForceTrigger);
        }
        private void btnReset_Click(object sender, EventArgs e)
        {
            OnButtonOperation(eOperation.Reset);
        }
        void OnButtonOperation(eOperation e_Operation)
        {
            if (!Utils.StartBusyOperation(this))
                return;

            mi_ConfigTimer.Stop();
            try
            {
                mi_Rigol.ExecuteOperation(e_Operation);
            }
            catch (Exception Ex)
            {
                Utils.ShowExceptionBox(this, Ex);
            }
            mi_ConfigTimer.Start();

            Utils.EndBusyOperation(this);
        }

        // ==========================================================

        private void btnTransfer_Click(object sender, EventArgs e)
        {
            if (btnTransfer.Text == "Abort")
            {
                mi_Rigol.AbortTransfer();
                return;
            }

            if (!Utils.StartBusyOperation(this))
                return;

            Utils.RegWriteBool(eRegKey.CaptureMemory, radioMemory.Checked);

            btnTransfer.Text = "Abort";
            Application.DoEvents();

            mi_ConfigTimer.Stop();
            try
            {
                Capture i_Capture = mi_Rigol.CaptureAllChannels(radioMemory.Checked);
                if (i_Capture != null)
                {
                    Utils.FormMain.StoreNewCapture(i_Capture);
                    PrintStatus("Ready", Color.Green);
                }
                else
                {
                    PrintStatus("Aborted", Color.Red);
                }
            }
            catch (Exception Ex)
            {
                Utils.ShowExceptionBox(this, Ex);
            }

            btnTransfer.Text = "Transfer";
            mi_ConfigTimer.Start();

            Utils.EndBusyOperation(this);
        }

        // ==========================================================

        void OnTextCommandKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnSend_Click(null, null);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (textCommand.Text.Length == 0)
            {
                MessageBox.Show(this, "Enter a command!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Utils.RegWriteString(eRegKey.SendCommand, textCommand.Text);

            if (!Utils.StartBusyOperation(this))
                return;

            mi_ConfigTimer.Stop();
            btnSend.Enabled   = false;
            textResponse.Text = "";
            Application.DoEvents();
            try
            {
                // There may be multiple "last errors" in the queue. Delete them all before sending the command.
                while (mi_Rigol.GetLastError() != null)
                {
                }

                textResponse.Text = mi_Rigol.Scpi.SendStringCommand(textCommand.Text, 2000);
                textResponse.ForeColor = Color.Black;
            }
            catch (TimeoutException)
            {
                String s_ErrorCause = mi_Rigol.GetLastError();
                if (s_ErrorCause == null)
                    s_ErrorCause = "Timeout. The cause may be that the SCPI command was invalid.";

                textResponse.Text = s_ErrorCause;
                textResponse.ForeColor = Color.Red;
            }
            catch (Exception Ex)
            {
                textResponse.Text = Ex.Message;
                textResponse.ForeColor = Color.Red;
            }
            btnSend.Enabled = true;
            mi_ConfigTimer.Start();

            Utils.EndBusyOperation(this);
        }
    }
}


