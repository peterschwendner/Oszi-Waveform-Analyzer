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
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

using eMark             = OsziWaveformAnalyzer.Utils.eMark;
using SmplMark          = OsziWaveformAnalyzer.Utils.SmplMark;
using Capture           = OsziWaveformAnalyzer.Utils.Capture;
using Channel           = OsziWaveformAnalyzer.Utils.Channel;
using eRegKey           = OsziWaveformAnalyzer.Utils.eRegKey;
using Utils             = OsziWaveformAnalyzer.Utils;
using CsvParser         = ExImport.CsvParser;

namespace Transfer
{
    /// <summary>
    /// This class manages transferring analog and digital channel data from an oscilloscope over USB.
    /// 
    /// In the future new Forms can be added here that have controls for other oscilloscope models.
    /// It does not amke any sense to derive them from a base class or interface because 
    /// the oscilloscopes and their SCPI commands are COMPLETELY different.
    /// Each company needs it's own classes and Forms.
    /// Each future Form should have a button "Install Driver".
    /// </summary>
    public class TransferManager
    {
        /// <summary>
        /// The Description of this enum is displayed in the same order in the Combobox
        /// Rigol is such an incredibly **STUPID** company that the SCPI commands are not even consistent
        /// within their OWN series of products. What a Chinese crap!
        /// </summary>
        public enum eOsziSerie
        {
            [Description("Rigol DS1000D / DS1000E Series")] // for example DS1102E
            Rigol_1000DE, 

            [Description("Rigol DS1000Z / MSO1000Z Series")] // for example DS1074Z
            Rigol_1000Z,  

            [Description("OWON VDS 1022 and VDS 2052")]
            OWON_1022,  

            [Description("Picoscope 3206 MSO")]
            Picoscope_3206,

            [Description("Hameg HMO1522 / HMO Compact Series")] // also HMO1002, HMO1202, HMO2022, HMO722/1022/2022
            Hameg_HMO1522,

            [Description("Hameg HM2008 / HM1008 / HM1508 CombiScope")]
            Hameg_HM2008,

            [Description("Hameg HM507 (RS232 only)")] // proprietary binary protocol, also HM504 (analog only, no waveforms)
            Hameg_HM507,

            // TODO: Add more oscilloscope brands like Tektronix, Rhode & Schwarz, Siglent, ...
        }

        /// <summary>
        /// Used for all UserControl's that are displayed in Form Transfer
        /// </summary>
        public interface ITransferPanel
        {
            // This is called when the Form is opened
            // Must not throw
            void OnLoad(eOsziSerie e_OsziSerie);

            // Open the connection to the oscilloscope, update GUI
            // Throws on error
            void OnOpenDevice(SCPI i_Scpi);

            // Close the connection to the oscilloscope, update GUI
            // Must not throw
            void OnCloseDevice();

            // Sends a command that the user has typed and displays the response in i_TextReponse.
            // Must not throw, but display error in i_TextReponse
            void SendManualCommand(String s_Command, TextBox i_TextReponse);
        }

        /// <summary>
        /// Default settings for the COM port "Baudrate Databits Parity Stopbits Handshake".
        /// The user can change them in FormTransfer. They must match the settings in the interface menu of the oscilloscope.
        /// </summary>
        public static String GetDefaultSerialSettings(eOsziSerie e_OsziSerie)
        {
            switch (e_OsziSerie)
            {
                // HO720 dual interface: sigrok uses 115200/8n1 with RTS/CTS for HMO scopes.
                case eOsziSerie.Hameg_HMO1522: return "115200 8N1 RTS";

                // The HM1508-2 / HM2008 manual specifies "N-8-2 no parity, 8 bits data, 2 stop bits (RTS/CTS hardware protocol)".
                // The baudrate is selectable in SETTINGS > Interface on the oscilloscope.
                case eOsziSerie.Hameg_HM2008:  return "19200 8N2 RTS";

                // HM507 command description: "keine Paritaet, Datenlaenge 8 Bit, 2 Stoppbit, RTS/CTS Handshake"
                // The baudrate (110 ... 115200) is detected automatically from the first SPACE CR after power on.
                case eOsziSerie.Hameg_HM507:   return "19200 8N2 RTS";

                default:                       return "9600 8N1 NONE";
            }
        }

        /// <summary>
        /// true if the oscilloscope has only a serial port (no USB TMC, no VXI)
        /// </summary>
        public static bool RequiresSerial(eOsziSerie e_OsziSerie)
        {
            return e_OsziSerie == eOsziSerie.Hameg_HM507;
        }

        public static void FillComboOsziModel(ComboBox i_ComboOsziModel)
        {
            i_ComboOsziModel.Sorted = false; // IMPORTANT!
            i_ComboOsziModel.Items.Clear();
            foreach (eOsziSerie e_Serie in Enum.GetValues(typeof(eOsziSerie)))
            {
                String s_Descript = Utils.GetDescriptionAttribute(e_Serie);
                i_ComboOsziModel.Items.Add(s_Descript);
            }
            Utils.ComboAdjustDropDownWidth(i_ComboOsziModel);

            i_ComboOsziModel.Text = Utils.RegReadString(eRegKey.OsziSerie);
            if (i_ComboOsziModel.SelectedIndex < 0)
                i_ComboOsziModel.SelectedIndex = 0;
        }

        /// <summary>
        /// Opens a Form that allows to capture from the selected oscilloscope over the USB SCPI protocol
        /// Throws
        /// </summary>
        public static void Transfer(ComboBox i_ComboOsziModel)
        {
            Utils.RegWriteString(eRegKey.OsziSerie, i_ComboOsziModel.Text);

            ITransferPanel i_Panel;
            eOsziSerie e_OsziSerie = (eOsziSerie)i_ComboOsziModel.SelectedIndex;
            switch (e_OsziSerie)
            {
                case eOsziSerie.Rigol_1000DE:
                case eOsziSerie.Rigol_1000Z:  
                    i_Panel = new PanelRigol(); 
                    break;
                
                case eOsziSerie.OWON_1022:
                    throw new Exception("For OWON the SCPI transfer is not yet implemented.\nBut you can import a CSV, BIN or CAP file.");

                case eOsziSerie.Picoscope_3206:
                    throw new Exception("For Picoscope 3206 the SCPI transfer is not yet implemented.\nBut you can import a CSV file.");

                case eOsziSerie.Hameg_HMO1522:
                case eOsziSerie.Hameg_HM2008:
                case eOsziSerie.Hameg_HM507:
                    i_Panel = new PanelHameg();
                    break;

                // TODO: Add Tektronix, Rhode & Schwarz, Siglent, ...

                default:
                    Debug.Assert(false, "Programming Error: OsziSerie not implemented");
                    return;
            }

            Form i_Form = new FormTransfer(e_OsziSerie, i_Panel);
            i_Form.ShowDialog(Utils.FormMain);
        }

        /// <summary>
        /// Parse proprietary vendor-specific CSV or BIN files
        /// Throws
        /// </summary>
        public static Capture ParseVendorFile(String s_Path, ComboBox i_ComboOsziModel, ref bool b_Abort)
        {
            Utils.RegWriteString(eRegKey.OsziSerie, i_ComboOsziModel.Text);

            String     s_FileExt   = Path.GetExtension(s_Path).ToLower();
            eOsziSerie e_OsziSerie = (eOsziSerie)i_ComboOsziModel.SelectedIndex;

            if (s_FileExt == ".csv")
            {
                CsvParser i_Parser = new CsvParser();
                return i_Parser.Parse(s_Path, e_OsziSerie, ref b_Abort); // throws if CSV import is not implemented
            }

            // Import other formats than CSV
            switch (e_OsziSerie)
            {
                case eOsziSerie.Rigol_1000DE:
                case eOsziSerie.Rigol_1000Z:
                case eOsziSerie.Picoscope_3206:
                    throw new Exception("For " + i_ComboOsziModel.Text + " only CSV file import is implemented.\nDid you select the correct Oszi Model?");

                case eOsziSerie.Hameg_HMO1522:
                case eOsziSerie.Hameg_HM2008:
                case eOsziSerie.Hameg_HM507:
                    throw new Exception("For " + i_ComboOsziModel.Text + " file import is not implemented.\nUse 'Transfer' to read the waveforms over RS232, USB or TCP.");

                case eOsziSerie.OWON_1022:
                    if (s_FileExt == ".cap") return ExImport.OWON.ParseBinaryFile(s_Path, ref b_Abort);
                    if (s_FileExt == ".bin") return ExImport.OWON.ParseBinaryFile(s_Path, ref b_Abort);
                    throw new Exception("For " + i_ComboOsziModel.Text + " only CSV, BIN and CAP file import is implemented.\nDid you select the correct Oszi Model?");

                // TODO: Add Tektronix, Rhode & Schwarz, Siglent, ... which probably use their own proprietary CSV format

                default:
                    Debug.Assert(false, "Programming Error: OsziSerie not implemented");
                    return null;
            }
        }
    }
}
