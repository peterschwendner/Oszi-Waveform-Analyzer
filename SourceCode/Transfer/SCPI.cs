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

// --------------------------------------------------------------

// Writes debug output to the debugger (or SysInternals DbgView)
// See file "Logfile SCPI Commands DS1074Z.txt" in subfolder "Documentation" which shows a successful communication.

// #define TRACE_OUTPUT

// --------------------------------------------------------------


using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using Utils             = OsziWaveformAnalyzer.Utils;

namespace Transfer
{
    /// <summary>
    /// This class implements the TMC (Test and Measurement Class) 
    /// using the SCPI protocol (Standard Command for Programmable Instruments) over USB.
    /// https://en.wikipedia.org/wiki/Standard_Commands_for_Programmable_Instruments
    /// 
    /// SCPI is an additional layer on top of the IEEE 488.2 specification.
    /// See subfolder "Documentation" for more details.
    /// 
    /// All commands and responses are ASCII text. Only the command :WAVEFORM may be configured to return binary data.
    /// 
    /// You can enable TRACE_OUTPUT in this class to see debug output of the entire USB communication.
    /// 
    /// This class has not been designed to be thread safe.
    /// 
    /// This class makes it unnecessary to install huge software packets of one gigabyte from NI, IVI, Tektronix
    /// or the huge Rigol UltraSigma (500 MB) and Rigol UltraScope which are bad quality primitive software.
    /// 
    /// Only the tiny IVI USB driver of 24 kilobyte size is required. (ausbtmc.sys)
    /// It is included in this project.
    /// 
    /// This class communicates directly with the oscilloscope using the standard Windows API without any external dependencies. 
    /// It implements non-blocking API calls using Overlapped I/O which is controlled by a timeout.
    /// No fancy C# "async Task" or "await" or "CancellationToken" are required here which you find other projects.
    /// They would only bloat up the code to the double size and produce a lot of unnecessary thread switching.
    /// By the way: USB High-Speed runs at 480 MBit/s and the communications is very fast. The wait time is a few millisconds.
    /// 
    /// This class has been written by a software developer with 45 years of experience in programming and cracking.
    /// It has been tested with a Rigol DS1074Z, but it should also work with other brands, even with function generators, mulimeters, etc.
    /// 
    /// This class was inspired by klasyc/ScpiNet. However his code has several design errors, it was rewritten from scratch bu Elmü.
    /// </summary>
    public class SCPI : IDisposable
    {
        #region Native Windows API

        // ---------------------- NATIVE CONSTANTS ---------------------------

        static readonly IntPtr INVALID_HANDLE_VALUE = (IntPtr)(-1);
        const int              ERROR_FILE_NOT_FOUND =   2;
        const int              ERROR_GEN_FAILURE    =  31; // A device attached to the system is not functioning.
        const int              ERROR_MORE_DATA      = 234;
        const int              WAIT_TIMEOUT         = 258;
        const int              ERROR_NO_MORE_ITEMS  = 259;
        const int              ERROR_IO_PENDING     = 997;

        static Guid USBTMC_CLASS_GUID = new Guid("A9FDBB24-128A-11D5-9961-00108335E361");

        // ------------------------ NATIVE ENUMS ---------------------------

        [Flags]
        enum eFileAccess : uint
        {
            GenericRead  = 0x80000000,
            GenericWrite = 0x40000000,
        }

        [Flags]
        enum eFileShare : uint
        {
            None   = 0x00,
            Read   = 0x01,
            Write  = 0x02,
            Delete = 0x04
        }

        enum eCreationDisposition : uint
        {
            New              = 1,
            CreateAlways     = 2,
            OpenExisting     = 3,
            OpenAlways       = 4,
            TruncateExisting = 5
        }

        [Flags]
        enum eFileAttributes : uint
        {
            None       = 0,
            Readonly   = 0x00000001,
            Hidden     = 0x00000002,
            System     = 0x00000004,
            Directory  = 0x00000010,
            Archive    = 0x00000020,
            Overlapped = 0x40000000,
        }

        [Flags]
        enum eDiGetClassFlags : uint
        {
            None = 0,
            DIGCF_DEFAULT          = 0x01,
            DIGCF_PRESENT          = 0x02,
            DIGCF_ALL_CLASSES      = 0x04,
            DIGCF_PROFILE          = 0x08,
            DIGCF_DEVICE_INTERFACE = 0x10,
        }

        // ------------------------ NATIVE STRUCTS ---------------------------

        [StructLayout(LayoutKind.Sequential)]
        struct SP_DEVINFO_DATA
        {
            public uint   cbSize;
            public Guid   ClassGuid;
            public uint   DevInst;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SP_DEVICE_INTERFACE_DATA
        {
            public  int  cbSize;
            public  Guid interfaceClassGuid;
            public  uint flags;
            private UIntPtr reserved;
        }

        const int INTERFACE_DETAIL_BUF_SIZE = 256;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct SP_DEVICE_INTERFACE_DETAIL_DATA_W
        {
            public int cbSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = INTERFACE_DETAIL_BUF_SIZE)]
            public String DevicePath;
        }

        // ------------------------ KERNEL IMPORT ---------------------------

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr CreateFileW(String lpFileName, eFileAccess dwDesiredAccess, eFileShare dwShareMode, IntPtr lpSecurityAttributes, eCreationDisposition dwCreationDisposition, eFileAttributes dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadFile(IntPtr hHandle, Byte[] bytes, int numBytesToRead, out int numBytesRead, ref NativeOverlapped overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteFile(IntPtr hHandle, Byte[] bytes, int numBytesToWrite, out int numBytesWritten, ref NativeOverlapped overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateEventW(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int WaitForSingleObject(IntPtr hHandle, int dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetOverlappedResult(IntPtr hHandle, ref NativeOverlapped overlapped, out int lpNumberOfBytesTransferred, bool bWait);

		[DllImport("kernel32.dll")]
		private static extern bool CancelIo(IntPtr hHandle);

        // ------------------------ SETUPAPI IMPORT --------------------------

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr SetupDiGetClassDevsW(ref Guid ClassGuid, String Enumerator, IntPtr hWndParent, eDiGetClassFlags Flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, uint MemberIndex, ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern bool SetupDiEnumDeviceInterfaces(IntPtr hDevInfo, ref SP_DEVINFO_DATA devInfo, ref Guid interfaceClassGuid, uint memberIndex, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern bool SetupDiGetDeviceInterfaceDetailW(IntPtr hDevInfo, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, ref SP_DEVICE_INTERFACE_DETAIL_DATA_W deviceInterfaceDetailData, uint deviceInterfaceDetailDataSize, ref uint requiredSize, IntPtr deviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern bool SetupDiDestroyDeviceInfoList (IntPtr DeviceInfoSet);

        #endregion

        #region TMC

        enum eTmcMsgId : byte
        {
            DEV_DEP_MSG_OUT        = 1,
            REQUEST_DEV_DEP_MSG_IN = 2,
        }

        /// <summary>
        /// USB TMC frame header according to the TMC specification. It is sent with each read and write request.
        /// See USBTMC specification in subfolder "Documentation".
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct kTmcHeader 
        {
            public eTmcMsgId e_MsgID;   // Byte  0
            public byte u8_Tag;         // Byte  1  = Counter, must always be > 0
            public byte u8_TagInverse;  // Byte  2  = One's complement of Tag
            public byte u8_Reserved1;   // Byte  3  = must be 0
            public int  s32_DataLen;    // Byte 4-7 = sent or requested data length, not including header and padding
            public byte u8_Attributes;  // Byte  8  = meaning depends on e_MsgID
            public byte u8_TermChar;    // Byte  9  = optional termination character (not used)
            public byte u8_Reserved2;   // Byte 10  = must be 0
            public byte u8_Reserved3;   // Byte 11  = must be 0
        }
       
        const int SIZE_OF_TMC_HEADER = 12;   // Expected size of kTmcHeader (if compiled correctly) Never change this.
        const int DEFAULT_TIMEOUT    = 1000; // milliseconds (This is far more than enough. Usb High-Speed runs at 480 MBit/s)

        #endregion

        // ============================ Enumerate USB devices ================================

        /// <summary>
        /// Throws
        /// Gets the list of the DevicePaths of the connected Measurement USB devices, like
        /// "\\\\?\\usb#vid_1ab1&pid_04ce#ds1zc204807063#{a9fdbb24-128a-11d5-9961-00108335e361}"
        /// </summary>
        public static Dictionary<String, String> GetUsbDeviceList(ComboBox i_ComboUsbDevice)
        {
            IntPtr h_DevInfoSet = SetupDiGetClassDevsW(ref USBTMC_CLASS_GUID, null, IntPtr.Zero, 
                                                       eDiGetClassFlags.DIGCF_PRESENT | eDiGetClassFlags.DIGCF_DEVICE_INTERFACE);
            if (h_DevInfoSet == INVALID_HANDLE_VALUE) 
                throw new Win32Exception(Marshal.GetLastWin32Error());

            Dictionary<String, String> i_DeviceList = new Dictionary<String, String>();
            try
            {
                for (uint u32_DevIdx = 0; true; u32_DevIdx++) 
                {
                    SP_DEVINFO_DATA k_DevInfoData = new SP_DEVINFO_DATA();
                    k_DevInfoData.cbSize = (uint)Marshal.SizeOf(k_DevInfoData);

                    if (!SetupDiEnumDeviceInfo(h_DevInfoSet, u32_DevIdx, ref k_DevInfoData)) 
                    {
                        int s32_Error = Marshal.GetLastWin32Error();
                        if (s32_Error == ERROR_NO_MORE_ITEMS)
                            break;

                        throw new Win32Exception(s32_Error);
                    }

                    for (uint u32_InterfIdx = 0; true; u32_InterfIdx++) 
                    {
                        SP_DEVICE_INTERFACE_DATA k_InterfaceData = new SP_DEVICE_INTERFACE_DATA();
                        k_InterfaceData.cbSize = Marshal.SizeOf(k_InterfaceData);

                        if (!SetupDiEnumDeviceInterfaces(h_DevInfoSet, ref k_DevInfoData, ref USBTMC_CLASS_GUID, 
                                                         u32_InterfIdx, ref k_InterfaceData)) 
                        {
                            int s32_Error = Marshal.GetLastWin32Error();
                            if (s32_Error == ERROR_NO_MORE_ITEMS)
                                break;

                            throw new Win32Exception(s32_Error);
                        }

                        SP_DEVICE_INTERFACE_DETAIL_DATA_W k_DetailData = new SP_DEVICE_INTERFACE_DETAIL_DATA_W();

                        // The same native C structure has a differnet size depending if compiled as 32 bit or 64 bit.
                        // The 64 bit compiler adds padding, so the 32 bit SetupApi.dll expects another struct size than the 64 bit DLL.
                        // If the struct size is wrong, SetupDiGetDeviceInterfaceDetailW() returns ERROR_INVALID_USER_BUFFER (1784)
                        k_DetailData.cbSize = Environment.Is64BitProcess ? 8 : 6;

                        uint u32_RequBufSize = 0;
                        if (!SetupDiGetDeviceInterfaceDetailW(h_DevInfoSet, ref k_InterfaceData, ref k_DetailData, 
                                                              INTERFACE_DETAIL_BUF_SIZE, ref u32_RequBufSize, IntPtr.Zero)) 
                        {
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                        }

                        // Get the display name for the user by splitting
                        // "\\\\?\\usb#vid_1ab1&pid_04ce#ds1zc204807063#{a9fdbb24-128a-11d5-9961-00108335e361}"
                        // and use the serial number "DS1ZC204807063" as display name.
                        // Better would be to obtain the Oscilloscope Model from the USB descriptor (iProduct), but this requires
                        // a very huge code enumerating all USB hubs. Microsoft did not implement an easy way.
                        String[] s_Parts = k_DetailData.DevicePath.Split('#');

                        // Will it ever happen that 2 oscilloscopes with the same serial number are connected?
                        // Possible, because some vendors are too lazy to apply a unique serial number to each device.
                        // In this case return "Serial1", "Serial2", etc..
                        for (int i=1; true; i++)
                        {
                            String s_Serial = s_Parts[2].ToUpper();
                            if (i > 1)
                                s_Serial += i; // append index "2", "3", etc..

                            if (!i_DeviceList.ContainsKey(s_Serial))
                            {
                                i_DeviceList.Add(s_Serial, k_DetailData.DevicePath);
                                break;
                            }
                        }
                    }
                }
            }
            finally
            {
                SetupDiDestroyDeviceInfoList(h_DevInfoSet);
            }

            // ------------- optionally fill combobox ------------------

            if (i_ComboUsbDevice != null)
            {
                i_ComboUsbDevice.Items.Clear();
                foreach (KeyValuePair<String, String> i_Pair in i_DeviceList)
                {
                    i_ComboUsbDevice.Items.Add(i_Pair.Key);
                }
                Utils.ComboAdjustDropDownWidth(i_ComboUsbDevice);

                if (i_ComboUsbDevice.Items.Count > 0)
                    i_ComboUsbDevice.SelectedIndex = 0;
            }

            return i_DeviceList;
        }

        // ============================== Class SCPI ================================

        IntPtr           mh_Device;
        Byte             mu8_Tag; 
        NativeOverlapped mk_Overlap   = new NativeOverlapped();
        IntPtr           mp_HeaderMem = Marshal.AllocHGlobal(SIZE_OF_TMC_HEADER);
        int              ms32_OpcReplaceDelay;

        /// <summary>
        /// A delay which replaces the *OPC? command.
        /// For devices that do not support the command *OPC? it is required to make a fix delay which gives the device
        /// the required time to process the command. If you set OpcReplaceDelay = 100 and then call SendOpcCommand()
        /// the command will be sent and after a fix delay of 100 ms the funtion returns without
        /// sending the command *OPC? and ignoring the timeout passed to SendOpcCommand().
        /// If you set OpcReplaceDelay = 0 this delay is turned off and the command *OPC? will be sent instead.
        /// </summary>
        public int OpcReplaceDelay
        {
            set { ms32_OpcReplaceDelay = value; }
        }

        /// <summary>
        /// s_DevicePath comes from GetUsbDeviceList()
        /// s_DevicePath = "\\\\?\\usb#vid_1ab1&pid_04ce#ds1zc204807063#{a9fdbb24-128a-11d5-9961-00108335e361}"
        /// This corresponds to
        /// HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\USB\VID_1AB1&PID_04CE\DS1ZC204807063\....
        /// </summary>
        public void Open(String s_DevicePath)
        {
            Debug.Assert(mh_Device == IntPtr.Zero, "Do not call Open() multiple times!");
            Debug.Assert(Marshal.SizeOf(typeof(kTmcHeader)) == SIZE_OF_TMC_HEADER, "struct compilation error");

            #if TRACE_OUTPUT
                Debug.Print("Open device \"" + s_DevicePath + "\"");
            #endif

            // The handle mh_Device allows to directly send to the USB bulk OUT pipe using WriteFile() 
            // and receive from USB bulk IN pipe using ReadFile(). Programming is the same as for RS232.
            // See "USB Tutorial.chm" in subfolder "Documentation"
            mh_Device = CreateFileW(s_DevicePath,
                                    eFileAccess.GenericRead | eFileAccess.GenericWrite,
                                    eFileShare.Read | eFileShare.Write,
                                    IntPtr.Zero,
                                    eCreationDisposition.OpenExisting,
                                    eFileAttributes.Overlapped,  // use non-blocking I/O
                                    IntPtr.Zero);
            
            if (mh_Device == INVALID_HANDLE_VALUE)
            {
                int s32_Error = Marshal.GetLastWin32Error();

                // Avoid strange error message "File not found" if the device has been disconnected meanwhile.
                if (s32_Error == ERROR_FILE_NOT_FOUND)
                    throw new Exception("The device does not exist");

                throw new Win32Exception(s32_Error);
            }

            mu8_Tag = 0;

            // This event is set by Windows when the overloapped I/O operation has completed.
            mk_Overlap.EventHandle = CreateEventW(IntPtr.Zero, true, false, null);
        }

        /// <summary>
        /// Finalizer (called on garbage collection)
        /// </summary>
        ~SCPI()
        {
            Dispose();
        }

        /// <summary>
        /// IMPORTANT: This must be called before opening again.
        /// </summary>
        public void Dispose()
        {
            if (mp_HeaderMem != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(mp_HeaderMem);
                mp_HeaderMem = IntPtr.Zero;
            }

            if (mk_Overlap.EventHandle != IntPtr.Zero)
            {
                CloseHandle(mk_Overlap.EventHandle);
                mk_Overlap.EventHandle = IntPtr.Zero;
            }

            if (mh_Device != IntPtr.Zero)
            {
                #if TRACE_OUTPUT
                    Debug.Print("Close device");
                #endif
                CloseHandle(mh_Device);
                mh_Device = IntPtr.Zero;
            }
        }

        // ================================ USB Communication ==========================================

        /// <summary>
        /// The command *OPC? queries if the last operation has finished. 
        /// This can be used for all commands that do not return data, for example ":RUN"
        /// 
        /// The Rigol documentation says the scope returns "0" or "1".
        /// But the Rigol oscilloscopes do not even comply their own documentation.
        /// If the command ":AUTOSCALE" is still busy it never responds with "0".
        /// It sends nothing until the command has finished calibrating the scope and then it responds "1", after 6 seconds!
        /// A very long timeout is needed. Don't forget to show the wait cursor.
        /// 
        /// If OpcReplaceDelay has been set for devices that do not support the command *OPC?, a fix pause
        /// of OpcReplaceDelay milliseconds will be made instead of sending the command *OPC? and s32_Timeout is ignored.
        /// </summary>
        public void SendOpcCommand(String s_Command, int s32_Timeout = 10000)
        {
            TransmitString(s_Command);

            if (ms32_OpcReplaceDelay > 0)
            {
                #if TRACE_OUTPUT
                    Debug.Print("Replace *OPC? with delay of "+ms32_OpcReplaceDelay+ " ms");
                #endif

                Thread.Sleep(ms32_OpcReplaceDelay);
                return;
            }

            Stopwatch i_Watch = new Stopwatch();
            i_Watch.Start();

            while (true)
            {
                if (i_Watch.ElapsedMilliseconds > s32_Timeout)
                {
                    // ATTENTION:
                    // This CancelIo() is EXTREMELY important! If it is missing you get TIMEOUT's again and again
                    CancelIo(mh_Device);
                    throw new TimeoutException("The SCPI command did not execute within the timeout.");
                }

                String s_Status = SendStringCommand("*OPC?", s32_Timeout);
                if (s_Status == "1")
                    break;

                Thread.Sleep(200); // in case a "0" is received.
            }
        }

        // ----------------------------------------

        /// <summary>
        /// Send an ASCII command and return a double
        /// </summary>
        public double SendDoubleCommand(String s_Command, int s32_Timeout = DEFAULT_TIMEOUT)
        {
            String s_Float = SendStringCommand(s_Command, s32_Timeout);

            // On a German Windows doubles and floats use comma instead of dot!
            double d_Value;
            if (!Double.TryParse(s_Float, NumberStyles.Float, CultureInfo.InvariantCulture, out d_Value))
                throw new Exception("The oscilloscope has returned an invalid floating point value: " + s_Float);

            return d_Value;
        }

        /// <summary>
        /// Send an ASCII command and return a bool
        /// </summary>
        public bool SendBoolCommand(String s_Command, int s32_Timeout = DEFAULT_TIMEOUT)
        {
            String s_Status = SendStringCommand(s_Command, s32_Timeout);
            return s_Status == "1" || s_Status == "ON";
        }

        /// <summary>
        /// Send an ASCII command and return a string
        /// </summary>
        public String SendStringCommand(String s_Command, int s32_Timeout = DEFAULT_TIMEOUT)
        {
            TransmitString(s_Command, s32_Timeout);
            String s_Response = ReceiveString(s32_Timeout);

            // Debug.Print(String.Format("'{0}' --> '{1}'", s_Command, s_Response));
            return s_Response;
        }

        /// <summary>
        /// Send an ASCII command and return a byte array.
        /// ATTENTION: If s32_MaxRxData is too small to receive the ENTIRE response, the USB communication will crash.
        /// s32_Timeout = timeout used for sending and for receiving one chunk
        /// </summary>
        public Byte[] SendByteCommand(int s32_MaxRxData, String s_Command, int s32_Timeout = DEFAULT_TIMEOUT)
        {
            TransmitString(s_Command, s32_Timeout);

            #if TRACE_OUTPUT
                Debug.Print(">> SendByteCommand() timeout= "+s32_Timeout);
            #endif

            Byte[] u8_Data = Receive(s32_MaxRxData, s32_Timeout);

            #if TRACE_OUTPUT
                Debug.Print("<< SendByteCommand() response= " + u8_Data.Length + " bytes");
            #endif
            return u8_Data;
        }

        // ==================================== PRIVATE =====================================

        private void TransmitString(String s_Command, int s32_Timeout = DEFAULT_TIMEOUT)
        {
            #if TRACE_OUTPUT
                if (s_Command != "*OPC?") Debug.Print("-------------------------------");
                Debug.Print(">> TransmitString() command= \"" + s_Command + "\"");
            #endif

            Byte[] u8_TxCommand = Encoding.ASCII.GetBytes(s_Command + '\n');
            SendTmcPacket(u8_TxCommand, 0, s32_Timeout);

            #if TRACE_OUTPUT
                Debug.Print("<< TransmitString() finished");
            #endif
        }

        private String ReceiveString(int s32_Timeout = DEFAULT_TIMEOUT)
        {
            #if TRACE_OUTPUT
                Debug.Print(">> ReceiveString() timeout= "+s32_Timeout);
            #endif

            // All ASCII responses are shorter than 128 bytes
            Byte[] u8_RxData  = Receive(128, s32_Timeout);
            String s_Response = Encoding.ASCII.GetString(u8_RxData);

            // ATTENTION: There may be garbage behind the response: "1.000000e+09\nO"
            int s32_LF = s_Response.IndexOf('\n');
            if (s32_LF > 0)
                s_Response = s_Response.Substring(0, s32_LF);

            #if TRACE_OUTPUT
                Debug.Print("<< ReceiveString() response= \"" + s_Response + "\"");
            #endif
            return s_Response;
        }

        // ----------------------------------------

        /// <summary>
        /// ATTENTION: If s32_MaxRxData is too small to receive the entire response, the USB communication will crash.
        /// s32_Timeout is for a chunk of data received in one TMC frame.
        /// USB High-Speed bulk data transfer is extremely fast: approx 100 kB in less than 20 ms.
        /// </summary>
        private Byte[] Receive(int s32_MaxRxData, int s32_Timeout = DEFAULT_TIMEOUT)
        {
            Byte[] u8_RxBuffer = new Byte[s32_MaxRxData + SIZE_OF_TMC_HEADER];

            MemoryStream i_Stream = new MemoryStream();

            // This loop runs until the device sets the EndOfMessage bit.
            // When requesting 200.000 bytes WaveForm data, Rigol sends one respone packet of 512 byte,
            // then another TMC header with 99524 bytes which are received in one single block.
            while (true)
            {
                // Request response from device
                SendTmcPacket(null, s32_MaxRxData, s32_Timeout);

                #if TRACE_OUTPUT
                    Debug.Print("  >> Receive() requesting " + s32_MaxRxData + " data bytes ...");
                #endif

                int s32_BytesRead; // Read USB IN transfer
                if (!ReadFile(mh_Device, u8_RxBuffer, u8_RxBuffer.Length, out s32_BytesRead, ref mk_Overlap))
                {
                    int s32_Error = Marshal.GetLastWin32Error();
                    if (s32_Error != ERROR_IO_PENDING)
                        throw new Win32Exception(s32_Error);
                    
                    if (WAIT_TIMEOUT == WaitForSingleObject(mk_Overlap.EventHandle, s32_Timeout))
                    {
                        #if TRACE_OUTPUT
                            Debug.Print("  << Receive() --> TIMEOUT  This may happen when an invalid SCPI command was sent.");
                        #endif

                        // ATTENTION:
                        // This CancelIo() is EXTREMELY important! If it is missing you get TIMEOUT's again and again
                        CancelIo(mh_Device);
                        throw new TimeoutException("Timeout waiting for response from device.\nThis may happen when an invalid SCPI command was sent.");
                    }

                    if (!GetOverlappedResult(mh_Device, ref mk_Overlap, out s32_BytesRead, false))
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                if (s32_BytesRead < SIZE_OF_TMC_HEADER)
                    throw new Exception("The USB device has sent a crippled response of " + s32_BytesRead + " bytes.\n"
                                      + "This may happen if the receive buffer is too small.");

                Marshal.Copy(u8_RxBuffer, 0, mp_HeaderMem, SIZE_OF_TMC_HEADER);
                kTmcHeader k_Header = (kTmcHeader)Marshal.PtrToStructure(mp_HeaderMem, typeof(kTmcHeader));

                // The TMC USB specification says clearly that the reponse must meet the following criteria otherwise it is invalid.
                // This is the only way to check in the primitve TMC protocol if valid data was received as there are no checksums or other methods.
                // If any bytes remained in the driver's receive buffer from a previous aborted transfer, this is the only way to detect this.
                // If any sloppy devices like Keysight multimeters do not set the Tag and so do not comply with these minimum requirements, 
                // they are buggy crap and cannot be used with this class. Demand a firmware update from the vendor.
                // Even such a sloppy company as Rigol sets the Tag correctly.
                if (k_Header.e_MsgID       != eTmcMsgId.REQUEST_DEV_DEP_MSG_IN ||
                    k_Header.u8_Tag        != mu8_Tag                          ||
                    k_Header.u8_TagInverse != (Byte)(~mu8_Tag)                 ||
                    k_Header.s32_DataLen   > s32_MaxRxData)
                        throw new Exception("The USB device has sent an invalid response header");

                i_Stream.Write(u8_RxBuffer, SIZE_OF_TMC_HEADER, s32_BytesRead - SIZE_OF_TMC_HEADER);

                bool b_EndOfMsg = (k_Header.u8_Attributes & 1) > 0;
                #if TRACE_OUTPUT
                    Debug.Print("  << Receive() response= " + s32_BytesRead + " bytes, EndOfMsg= " + b_EndOfMsg);
                #endif

                if (b_EndOfMsg)
                    break;
            }
            return i_Stream.ToArray();
        }

        // ----------------------------------------

        /// <summary>
        /// ATTENTION: If s32_MaxRxData is too small to receive the entire response, the USB communication will crash.
        /// u8_TxCommand != null --> u8_TxCommand is appended to the 12 byte header and sent in one or multiple Bulk OUT packets of 512 bytes
        /// u8_TxCommand == null --> A response is requested from the device in a Bulk IN transfer
        /// </summary>
		private void SendTmcPacket(Byte[] u8_TxCommand, int s32_MaxRxData, int s32_Timeout)
		{
            bool b_Command = u8_TxCommand != null;
            #if TRACE_OUTPUT
                if (b_Command) Debug.Print("  >> SendTmcPacket() sending command (TxData= " + u8_TxCommand.Length + " bytes)");
                else           Debug.Print("  >> SendTmcPacket() requesting response (MaxRxData= " + s32_MaxRxData + " byte)");
            #endif

            // Remove any possibly remaining bytes from the driver's input buffer
            CancelIo(mh_Device);

            // incremet counter, always > 0
            mu8_Tag = (Byte)Math.Max(1, mu8_Tag + 1);

            // Create the write header:
            kTmcHeader k_Header    = new kTmcHeader();
            k_Header.u8_Tag        = mu8_Tag;
            k_Header.u8_TagInverse = (Byte)~mu8_Tag;

            if  (b_Command) // Send Command
            {
                k_Header.e_MsgID       = eTmcMsgId.DEV_DEP_MSG_OUT; // (Device Dependent Command Message, sent on Bulk OUT)
                k_Header.s32_DataLen   = u8_TxCommand.Length; // length of command, not including header + padding
                k_Header.u8_Attributes = 1; // 1 = EOM = End Of Message --> all data is sent in one packet
            }
            else // Request IN transfer
            {
                k_Header.e_MsgID       = eTmcMsgId.REQUEST_DEV_DEP_MSG_IN; // (command sent on Bulk OUT requesting the device to send a response on Bulk-IN)
                k_Header.s32_DataLen   = s32_MaxRxData; // maximum data length for the requested IN packet
                k_Header.u8_Attributes = 0; // 0 = Device must ignore u8_TermChar
            }

            Byte[] u8_Header = new Byte[SIZE_OF_TMC_HEADER];
            Marshal.StructureToPtr(k_Header, mp_HeaderMem, false);
            Marshal.Copy(mp_HeaderMem, u8_Header, 0, SIZE_OF_TMC_HEADER);

            List<Byte> i_Transfer = new List<Byte>();
            i_Transfer.AddRange(u8_Header);            
            if (b_Command)
                i_Transfer.AddRange(u8_TxCommand);

            // align to 4 byte boundary
            while ((i_Transfer.Count & 0x3) > 0) 
            {
                i_Transfer.Add(0);
            }

            int s32_BytesWritten; // Write USB OUT transfer
            if (!WriteFile(mh_Device, i_Transfer.ToArray(), i_Transfer.Count, out s32_BytesWritten, ref mk_Overlap))
            {
                int s32_Error = Marshal.GetLastWin32Error();
                if (s32_Error != ERROR_IO_PENDING)
                    throw new Win32Exception(s32_Error);
                    
                // Do NOT throw a TimeoutException here, which must only be used while waiting for a response.
                if (WAIT_TIMEOUT == WaitForSingleObject(mk_Overlap.EventHandle, s32_Timeout))
                {
                    // ATTENTION:
                    // This CancelIo() is EXTREMELY important! If it is missing you get TIMEOUT's again and again
                    CancelIo(mh_Device);
                    throw new Exception("Timeout sending command to device");
                }

                if (!GetOverlappedResult(mh_Device, ref mk_Overlap, out s32_BytesWritten, false))
                {
                    s32_Error = Marshal.GetLastWin32Error();

                    // This error happens frequently after Windows was in Sleep mode. The reason is mostly a buggy driver.
                    if (s32_Error == ERROR_GEN_FAILURE)
                        throw new Exception("The USB device is not functioning.\nThis may happen after the computer was in Sleep mode.\nPlease reconnect the USB cable.");

                    throw new Win32Exception(s32_Error);
                }
            }

            if (s32_BytesWritten != i_Transfer.Count)
                throw new Exception("Error sending USB OUT packet");

            #if TRACE_OUTPUT
                Debug.Print("  << SendTmcPacket() finished");
            #endif
        }
    }
}
