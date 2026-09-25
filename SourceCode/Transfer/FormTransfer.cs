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
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Windows.Forms;

using WForms            = System.Windows.Forms;
using eOsziSerie        = Transfer.TransferManager.eOsziSerie;
using ITransferPanel    = Transfer.TransferManager.ITransferPanel;
using ScpiCombo         = Transfer.SCPI.ScpiCombo;
using eConnectMode      = Transfer.SCPI.eConnectMode;
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
        eConnectMode   me_Mode;
        eOsziSerie     me_OsziSerie;
        SCPI           mi_Scpi;
        ITransferPanel mi_Panel;
        WForms.Timer   mi_StatusTimer;
        bool           mb_ComPortsLoaded;

        /// <summary>
        /// Constructor
        /// </summary>
        public FormTransfer(eOsziSerie e_OsziSerie, ITransferPanel i_Panel)
        {
            me_OsziSerie = e_OsziSerie;
            mi_Panel     = i_Panel;

            InitializeComponent();

            Control i_Ctrl = (Control)i_Panel;
            i_Ctrl.Top  = radioRS232.Bottom + 3;
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

            statusLabel .Text  = "";
            statusLabel .Width = ClientSize.Width - 4;           

            // Load Combobox with USB devices
            try { PlatformManager.Instance.EnumerateUsbDevices(comboDevices); }                  // FIRST
            catch {}

            // Load Combobox with COM port settings
            String s_DefaultSerial = TransferManager.GetDefaultSerialSettings(me_OsziSerie);
            comboSerial.Items.Add(s_DefaultSerial);
            foreach (String s_Settg in new String[] { "115200 8N1 RTS", "57600 8N1 RTS", "38400 8N1 RTS", "19200 8N1 RTS", "9600 8N1 RTS",
                                                      "115200 8N2 RTS", "57600 8N2 RTS", "38400 8N2 RTS", "19200 8N2 RTS", "9600 8N2 RTS",
                                                      "115200 8N1 NONE", "9600 8N1 NONE" })
            {
                if (!comboSerial.Items.Contains(s_Settg))
                    comboSerial.Items.Add(s_Settg);
            }
            Utils.ComboAdjustDropDownWidth(comboSerial);
            comboSerial.Text = ReadSerialSetting(1, s_DefaultSerial);

            int s32_Mode = Utils.RegReadInteger(eRegKey.ConnectMode, (int)eConnectMode.USB) % 4; // AFTER
            switch ((eConnectMode)s32_Mode)
            {
                case eConnectMode.USB:   radioUSB  .Checked = true; break; // fires OnRadioButton_CheckedChanged()
                case eConnectMode.VXI:   radioVXI  .Checked = true; break; // fires OnRadioButton_CheckedChanged()
                case eConnectMode.TCP:   radioTCP  .Checked = true; break; // fires OnRadioButton_CheckedChanged()
                case eConnectMode.RS232: radioRS232.Checked = true; break; // fires OnRadioButton_CheckedChanged()
            }

            // Old oscilloscopes like HM507 have only a RS232 port. (TCP is allowed for RS232 to Ethernet converters)
            if (TransferManager.RequiresSerial(me_OsziSerie) && !radioTCP.Checked)
                radioRS232.Checked = true;

            textVxiLink.Text = Utils.RegReadString(eRegKey.LinkVXI, "inst0");

            mi_StatusTimer = new WForms.Timer();
            mi_StatusTimer.Tick += new EventHandler(OnStatusTimer);
            mi_StatusTimer.Interval = 4000;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (Utils.IsBusy)
            {
                e.Cancel = true;
                MessageBox.Show(this, "An operation is still in progress.\nAbort the operation to close the window.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (!Utils.StartBusyOperation(this))
                return;

            comboDevices.Items.Clear();
            comboDevices.Text = "";
            btnSearch.Enabled = false;
            PrintStatus("Please wait ...", Color.Blue);

            try
            {
                if (me_Mode == eConnectMode.USB)
                {
                    PlatformManager.Instance.EnumerateUsbDevices(comboDevices); // throws
                    if (comboDevices.Items.Count == 0)
                        throw new Exception("No USB device of type 'Test and Measurement Class' is connected.");

                    PrintStatus("Loaded " + comboDevices.Items.Count + " USB device(s) into Combobox", Color.Green);
                }
                else if (me_Mode == eConnectMode.RS232)
                {
                    SCPI.EnumerateSerialPorts(comboDevices);
                    if (comboDevices.Items.Count == 0)
                        throw new Exception("No COM port was found on this computer.");

                    comboDevices.SelectedIndex = 0;
                    PrintStatus("Loaded " + comboDevices.Items.Count + " COM port(s) into Combobox", Color.Green);
                }
                else // TCP / VXI
                {
                    VxiClient i_VxiCLient = new VxiClient();
                    i_VxiCLient.EnumerateVxiDevices(comboDevices); // throws

                    if (comboDevices.Items.Count == 0)
                        throw new Exception("No device has responded to the VXI broadcast request.");

                    PrintStatus("Loaded " + comboDevices.Items.Count + " VXI device(s) into Combobox", Color.Green);
                }
            }
            catch (Exception Ex)
            {
                Utils.ShowExceptionBox(this, Ex);
            }

            Utils.EndBusyOperation(this);
            btnSearch.Enabled = true;
        }

        private void btnInstallDriver_Click(object sender, EventArgs e)
        {
            PlatformManager.Instance.InstallDriver(this);
        }

        private void linkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            switch (me_OsziSerie)
            {
                case eOsziSerie.Hameg_HMO1522:
                case eOsziSerie.Hameg_HM2008:
                case eOsziSerie.Hameg_HM507:
                    PlatformManager.Instance.ShowHelp(this, "Hameg");
                    break;
                default:
                    PlatformManager.Instance.ShowHelp(this, "SCPI");
                    break;
            }
        }

        /// <summary>
        /// Called from all 4 RadioButtons
        /// </summary>
        private void OnRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            // Remove the COM ports when switching from RS232 to another mode
            if (mb_ComPortsLoaded && !radioRS232.Checked)
            {
                mb_ComPortsLoaded = false;
                comboDevices.Items.Clear();
                if (radioUSB.Checked)
                {
                    try { PlatformManager.Instance.EnumerateUsbDevices(comboDevices); }
                    catch {}
                }
            }

            if (radioUSB.Checked)
            {
                me_Mode = eConnectMode.USB;
                comboDevices.DropDownStyle = ComboBoxStyle.DropDownList;
                comboDevices.Text = Utils.RegReadString(eRegKey.ConnectUSB);
                lblUsbEndp  .Text = "USB Device";
            }
            if (radioTCP.Checked)
            {
                me_Mode = eConnectMode.TCP;
                comboDevices.DropDownStyle = ComboBoxStyle.DropDown;
                comboDevices.Text = Utils.RegReadString(eRegKey.ConnectTCP, "192.168.0.240 : 5555");
                lblUsbEndp  .Text = "IP Address : Port";
            }
            if (radioVXI.Checked)
            {
                me_Mode = eConnectMode.VXI;
                comboDevices.DropDownStyle = ComboBoxStyle.DropDown;
                comboDevices.Text = Utils.RegReadString(eRegKey.ConnectVXI, "192.168.0.240");
                lblUsbEndp  .Text = "IP Address";
            }
            if (radioRS232.Checked)
            {
                me_Mode = eConnectMode.RS232;
                comboDevices.DropDownStyle = ComboBoxStyle.DropDown; // allow entering "/dev/ttyUSB0" on Linux
                comboDevices.Items.Clear();
                try { SCPI.EnumerateSerialPorts(comboDevices); } // enumerating COM ports is instantaneous
                catch {}
                mb_ComPortsLoaded = true;
                comboDevices.Text = ReadSerialSetting(0, comboDevices.Items.Count > 0 ? comboDevices.Items[0].ToString() : "COM1");
                lblUsbEndp  .Text = "COM Port";
            }

            // COM port names are short: make room for the port settings
            comboDevices.Width  = radioRS232.Checked ? comboSerial.Left - comboDevices.Left - 5 : btnSearch.Left - comboDevices.Left - 3;

            lblVxiLink .Visible = radioVXI.Checked;
            textVxiLink.Visible = radioVXI.Checked;
            lblSerial  .Visible = radioRS232.Checked;
            comboSerial.Visible = radioRS232.Checked;
            btnSearch  .Visible = !radioRS232.Checked; // COM ports are loaded when the RadioButton is checked
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

            if (!Utils.StartBusyOperation(this))
                return;

            Utils.RegWriteInteger(eRegKey.ConnectMode,  (int)me_Mode);

            textVxiLink.Text = textVxiLink.Text.Trim();

            btnOpen.Enabled = false;
            PrintStatus("Connecting to oscilloscope...", Color.Blue);
            mi_Scpi = new SCPI();
            try
            {
                switch (me_Mode)
                {
                    case eConnectMode.USB:
                        ScpiCombo i_Combo = (ScpiCombo)comboDevices.SelectedItem;
                        if (i_Combo == null)
                            throw new Exception("Please connect the oscilloscope over USB, turn it on and click 'Search'.\n"
                                              + "If it does not appear, click 'Install Diver'.\n"
                                              + "If it still does not work read the help file.");

                        mi_Scpi.ConnectUsb(i_Combo); // opens USB device, throws
                        Utils.RegWriteString(eRegKey.ConnectUSB, comboDevices.Text);
                        break;

                    case eConnectMode.VXI:
                        mi_Scpi.ConnectVxi(comboDevices.Text, textVxiLink.Text); // opens network connection, throws
                        Utils.RegWriteString(eRegKey.ConnectVXI, comboDevices.Text);
                        Utils.RegWriteString(eRegKey.LinkVXI,    textVxiLink.Text);
                        break;

                    case eConnectMode.TCP:
                        mi_Scpi.ConnectTcp(comboDevices.Text); // opens network connection, throws
                        Utils.RegWriteString(eRegKey.ConnectTCP, comboDevices.Text);
                        break;

                    case eConnectMode.RS232:
                        mi_Scpi.ConnectSerial(comboDevices.Text, comboSerial.Text); // opens COM port, throws
                        WriteSerialSetting(comboDevices.Text.Trim(), comboSerial.Text.Trim());
                        break;
                }
                
                mi_Panel.OnOpenDevice(mi_Scpi);
                EnableGui(true);
                PrintStatus("Connected", Color.Black);
            }
            catch (Exception Ex)
            {
                PrintStatus("Connect Error", Color.Red);
                Utils.ShowExceptionBox(this, Ex);
                Disconnect();
            }

            btnOpen.Enabled = true;
            Utils.EndBusyOperation(this);
        }

        // ==========================================================

        /// <summary>
        /// Each oscilloscope model has its own COM port and port settings, because they differ (e.g. HMO: 8N1, HM507: 8N2 RTS).
        /// Stored in one registry value: "Hameg_HMO1522=COM6|115200 8N1 NONE;Hameg_HM507=COM1|19200 8N2 RTS"
        /// s32_Index = 0 --> COM port, 1 --> port settings
        /// </summary>
        String ReadSerialSetting(int s32_Index, String s_Default)
        {
            String s_Prefix = me_OsziSerie + "=";
            foreach (String s_Entry in Utils.RegReadString(eRegKey.SerialSettings).Split(';'))
            {
                if (!s_Entry.StartsWith(s_Prefix))
                    continue;

                String[] s_Parts = s_Entry.Substring(s_Prefix.Length).Split('|');
                if (s_Parts.Length == 2 && s_Parts[s32_Index].Length > 0)
                    return s_Parts[s32_Index];
            }
            return s_Default;
        }

        void WriteSerialSetting(String s_Port, String s_Settings)
        {
            String s_Prefix = me_OsziSerie + "=";
            List<String> i_Entries = new List<String>();
            foreach (String s_Entry in Utils.RegReadString(eRegKey.SerialSettings).Split(';'))
            {
                // Keep the entries of the other models. Skip legacy values without model name.
                if (s_Entry.Contains("=") && !s_Entry.StartsWith(s_Prefix))
                    i_Entries.Add(s_Entry);
            }
            i_Entries.Add(s_Prefix + s_Port.Replace("|", "").Replace(";", "") + "|" + s_Settings.Replace("|", "").Replace(";", ""));
            Utils.RegWriteString(eRegKey.SerialSettings, String.Join(";", i_Entries.ToArray()));
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
            groupCommand.Enabled =  b_Open;
            comboDevices.Enabled = !b_Open;
            btnSearch   .Enabled = !b_Open;
            bool b_Serial = TransferManager.RequiresSerial(me_OsziSerie);
            radioTCP    .Enabled = !b_Open;
            radioUSB    .Enabled = !b_Open && !b_Serial;
            radioVXI    .Enabled = !b_Open && !b_Serial;
            radioRS232  .Enabled = !b_Open;
            comboSerial .Enabled = !b_Open;
            lblSerial   .Enabled = !b_Open;
            lblUsbEndp  .Enabled = !b_Open;
            textVxiLink .Enabled = !b_Open;
            lblVxiLink  .Enabled = !b_Open;
            textResponse.Text    = "";
            btnOpen     .Text    = b_Open ? "Close" : "Open";
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


