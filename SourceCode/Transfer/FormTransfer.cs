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
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Text;
using System.Windows.Forms;

using WForms            = System.Windows.Forms;
using eOsziSerie        = Transfer.TransferManager.eOsziSerie;
using ITransferPanel    = Transfer.TransferManager.ITransferPanel;
using ScpiCombo         = Transfer.SCPI.ScpiCombo;
using eRegKey           = OsziWaveformAnalyzer.Utils.eRegKey;
using Utils             = OsziWaveformAnalyzer.Utils;
using PlatformManager   = Platform.PlatformManager;

// This class implements communication with an oscilloscope
// Sadly there is no standard for this communication so each vendor cooks his own soup.
// Therefore the vendor specific code is in a UserControl that implements ITransferPanel.
namespace Transfer
{
    public partial class FormTransfer : Form
    {
        eOsziSerie     me_OsziSerie;
        SCPI           mi_Scpi;
        ITransferPanel mi_Panel;
        WForms.Timer   mi_StatusTimer;

        /// <summary>
        /// Constructor
        /// </summary>
        public FormTransfer(eOsziSerie e_OsziSerie, ITransferPanel i_Panel)
        {
            me_OsziSerie = e_OsziSerie;
            mi_Panel     = i_Panel;

            InitializeComponent();

            Control i_Ctrl = (Control)i_Panel;
            i_Ctrl.Top  = btnInstallDriver.Bottom + 3;
            i_Ctrl.Left = 10;
            Controls.Add(i_Ctrl);

            Height += i_Ctrl.Height - 5;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.Text = "Transfer  —  " + Utils.GetDescriptionAttribute(me_OsziSerie); // Window Title

            mi_Panel.OnLoad(me_OsziSerie);
            EnableGui(false);

            textCommand.Text = Utils.RegReadString(eRegKey.SendCommand, "*IDN?");
            textCommand.KeyDown += new KeyEventHandler(OnTextCommandKeyDown);

            statusLabel.Text  = "";
            statusLabel.Width = ClientSize.Width - 4;           

            mi_StatusTimer = new WForms.Timer();
            mi_StatusTimer.Tick += new EventHandler(OnStatusTimer);
            mi_StatusTimer.Interval = 4000;

            textIpAddr.Width = comboUsbDevice.Width -2;
            textPort  .Width = btnRefreshUSB .Width -4;
            textIpAddr.Text = Utils.RegReadString(eRegKey.ConnectIpAddr, "192.168.1.127");
            textPort  .Text = Utils.RegReadString(eRegKey.ConnectPort,   "5555");

            if (Utils.RegReadBool(eRegKey.ConnectTCP))
                radioTCP.Checked = true;
            else // USB
                radioTCP.Checked = true;

            btnRefreshUSB_Click    (null, null); // refresh USB devices
            radioUSB_CheckedChanged(null, null); // toggle controls
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

            Disconnect();
            mi_StatusTimer.Stop();
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

        private void radioUSB_CheckedChanged(object sender, EventArgs e)
        {
            comboUsbDevice.Visible =  radioUSB.Checked;
            btnRefreshUSB .Visible =  radioUSB.Checked;
            textIpAddr    .Visible = !radioUSB.Checked;
            textPort      .Visible = !radioUSB.Checked;
            lblPort       .Visible = !radioUSB.Checked;
            lblUsbIP.Text = radioUSB.Checked ? "TMC USB Devices:" : "IP Address:";
        }

        // ==========================================================

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (btnOpen.Text == "Close")
            {
                // Do not allow to close the connection while a Tansfer is running
                if (!Utils.StartBusyOperation(this))
                    return;

                Utils.EndBusyOperation(this);

                Disconnect();
                return;
            }

            Utils.RegWriteBool  (eRegKey.ConnectTCP,    radioTCP.Checked);
            Utils.RegWriteString(eRegKey.ConnectIpAddr, textIpAddr.Text);
            Utils.RegWriteString(eRegKey.ConnectPort,   textPort  .Text);

            if (!Utils.StartBusyOperation(this))
                return;

            btnOpen.Enabled = false;
            PrintStatus("Connecting to oscilloscope...", Color.Blue);
            try
            {
                if (radioUSB.Checked)
                {
                    ScpiCombo i_Combo = (ScpiCombo)comboUsbDevice.SelectedItem;
                    if (i_Combo == null)
                        throw new Exception("Please connect the oscilloscope over USB, turn it on and click 'Refresh'.\n"
                                          + "If it does not appear, click 'Install Diver'.");

                    mi_Scpi = new SCPI(i_Combo); // opens USB device, throws
                }
                else // TCP
                {
                    IPAddress i_IpAddr = System.Net.IPAddress.Parse(textIpAddr.Text);

                    UInt16 u16_Port;
                    if (!UInt16.TryParse(textPort.Text, out u16_Port) || u16_Port == 0)
                        throw new Exception("Enter a valid port.");

                    mi_Scpi = new SCPI(i_IpAddr, u16_Port); // opens network connection, throws
                }
                
                mi_Panel.OnOpenDevice(mi_Scpi);
                EnableGui(true);
                PrintStatus("Connected", Color.Black);
            }
            catch (Exception Ex)
            {
                Disconnect();
                PrintStatus("Connect Error", Color.Red);
                Utils.ShowExceptionBox(this, Ex);
            }

            btnOpen.Enabled = true;
            Utils.EndBusyOperation(this);
        }

        void Disconnect()
        {
            mi_Panel.OnCloseDevice();
            if (mi_Scpi != null)
            {
                mi_Scpi.Dispose(); // close native handles
                mi_Scpi = null;
            }
            EnableGui(false);
        }

        void EnableGui(bool b_Open)
        {
            groupCommand  .Enabled =  b_Open;
            comboUsbDevice.Enabled = !b_Open;
            btnRefreshUSB .Enabled = !b_Open;
            radioTCP      .Enabled = !b_Open;
            radioUSB      .Enabled = !b_Open;
            textIpAddr    .Enabled = !b_Open;
            textPort      .Enabled = !b_Open;
            lblUsbIP      .Enabled = !b_Open;
            lblPort       .Enabled = !b_Open;
            textResponse  .Text    = "";
            btnOpen       .Text    = b_Open ? "Close" : "Open";
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

            btnSend.Enabled = false;
            textResponse.Text      = "Please wait...";
            textResponse.ForeColor = Color.Blue;
            Application.DoEvents();

            mi_Panel.SendManualCommand(textCommand.Text, textResponse);

            btnSend.Enabled = true;
            Utils.EndBusyOperation(this);
        }
    }
}


