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
#if DEBUG
//   #define TRACE_OUTPUT
#endif

// --------------------------------------------------------------

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using Utils             = OsziWaveformAnalyzer.Utils;
using IDevice           = Platform.PlatformManager.IDevice;
using PlatformManager   = Platform.PlatformManager;

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
    /// or the huge Rigol UltraSigma which is bad quality primitive software.
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
        #region enums

        /// <summary>
        /// This enum is only used for transferring binary data from :WAVEFORM:DATA? over a TCP connection.
        /// As SCPI is extremely primitive the only way to detect the last data packet is the Linefeed at the end.
        /// Unlike for USB there is no struct kTmcHeader that has a flag indicating the last packet.
        /// How to make sure that a byte of 0x0A within the binary data does not terminate the transmission?
        /// This enum defines how to detect the end of the binary data for a TCP connection.
        /// ATTENTION:
        /// The option to receive data until a timeout is no option because it would make the transfer EXTREMELY slow.
        /// </summary>
        public enum eBinaryTCP
        {
            // It seems that the Rigol serie DS1000DE does not send linefeed bytes inside the binary data.
            // At least the file "Rigol DS1000E Waveform Guide.htm" in subfolder Documentation says so:
            // "....each byte should have a value of between 15 and 240."
            // "The top of the LCD display of the scope represents byte value 25 and the bottom is 225."
            // So it seems that they take care not to send linefeeds withinh the data.
            // In this mode the transmission ends when a packet ends with a linefeed.
            Linefeed,

            // The Rigol serie DS1000Z definitely sends bytes of value 0x0A within the binary data.
            // But the oscilloscope sends reliably the count of samples that are to be transmitted for the current configuration.
            // In this mode the transmission ends when the given minimum count of bytes was received.
            // Behind the last binary byte comes the linefeed and sometimes a completely useless padding byte.
            // These are eliminated here.
            MinSize,
        }

        public enum eConnectMode
        {
            USB = 0,
            TCP = 1,
            VXI = 2,
        }

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

        #region ScpiCombo

        /// <summary>
        /// This class is stored in ComboBox.Items of the combobox that shows the connected USB devices
        /// </summary>
        public class ScpiCombo
        {
            // Windows: Microsoft's unique device string from the registry (mostly this is the serial number of the USB device)
            // Linux:   Any string that uniquely identifies the oscilloscope among all connected SCPI devices
            public String ms_Display;    
            
            // Windows: "\\?\USB#VID_1AB1&PID_04CE#DS1ZC204807063#{A9FDBB24-128A-11D5-9961-00108335E361}"
            // Linux:   "/dev/usbtmc0"
            public String ms_DevicePath;

            public ScpiCombo(String s_Display, String s_DevicePath)
            {
                ms_Display    = s_Display;
                ms_DevicePath = s_DevicePath;
            }

            public override string ToString()
            {
                return ms_Display;
            }
        }

        #endregion

        // 128 byte buffer is sufficient for all SCPI commands that return an ASCII response 
        const int BUF_SIZE_ASCII = 128;

        // The default timeout for TCP connection is 20 seconds which is much too long.
        const int TCP_CONNECT_TIMEOUT = 1500;
        const int WSAETIMEDOUT        = 10060; // SocketException

        eConnectMode me_Mode;
        Byte         mu8_Tag; 
        int          ms32_OpcReplaceDelay;
        IntPtr       mp_HeaderMem;
        IDevice      mi_UsbDevice;
        Socket       mi_TcpSocket;
        VxiClient    mi_VxiClient;

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

        // =============================================================================================

        /// <summary>
        /// i_Combo comes from EnumerateScpiDevices()
        /// </summary>
        public void ConnectUsb(ScpiCombo i_Combo)
        {
            #if TRACE_OUTPUT
                Debug.Print("Open USB device \"" + i_Combo + "\"");
            #endif

            Debug.Assert(Marshal.SizeOf(typeof(kTmcHeader)) == SIZE_OF_TMC_HEADER, "struct compilation error");

            me_Mode      = eConnectMode.USB;
            mi_UsbDevice = PlatformManager.Instance.OpenUsbDevice(i_Combo);
            mp_HeaderMem = Marshal.AllocHGlobal(SIZE_OF_TMC_HEADER);
        }

        /// <summary>
        /// s_Endpoint   = "192.168.0.240 : 618" --> Connect directly to the control port 618.
        /// s_Endpoint   = "192.168.0.240"       --> Request the control port from the portmapper and connect to it.
        /// s_DeviceName = "inst0" for Rigol     ATTENTION: CASE SENSITIVE!
        /// </summary>
        public void ConnectVxi(String s_Endpoint, String s_DeviceName)
        {
            #if TRACE_OUTPUT
                Debug.Print("Open VXI connection to " + s_Endpoint);
            #endif

            UInt16 u16_Port; // Port is allowed to be 0 here
            IPAddress i_IpAddr = ParseEndpoint(s_Endpoint, out u16_Port);

            me_Mode      = eConnectMode.VXI;
            mi_VxiClient = new VxiClient();
            mi_VxiClient.ConnectDevice(i_IpAddr, u16_Port);

            mi_VxiClient.CreateLink(s_DeviceName);
        }

        public void ConnectTcp(String s_Endpoint)
        {
            #if TRACE_OUTPUT
                Debug.Print("Open TCP connection to " + s_Endpoint);
            #endif

            UInt16 u16_TcpPort;
            IPAddress i_IpAddress = ParseEndpoint(s_Endpoint, out u16_TcpPort);

            if (u16_TcpPort == 0)
                Throw("Enter IP address and port separated by colon like: \"192.168.0.240 : 1234\"");

            me_Mode      = eConnectMode.TCP;
            mi_TcpSocket = ConnectTcpSocketAsync(i_IpAddress, u16_TcpPort);
        }

        /// <summary>
        /// Parse enpoint string "192.168.0.240 : 618" into IP Address and port.
        /// If the port is missing or invalid --> return u16_Port = 0.
        /// If the IP Address is invalid --> throw exception
        /// </summary>
        IPAddress ParseEndpoint(String s_Endpoint, out UInt16 u16_Port)
        {
            u16_Port = 0;
            String[] s_Parts = s_Endpoint.Split(':');

            String s_IpAddress = s_Endpoint.Trim();
            if (s_Parts.Length == 2)
            {
                s_IpAddress =   s_Parts[0].Trim();
                UInt16.TryParse(s_Parts[1].Trim(), out u16_Port);
            }
            return IPAddress.Parse(s_IpAddress);
        }

        // =============================================================================================

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
            if (mi_UsbDevice != null)
            {
                #if TRACE_OUTPUT
                    Debug.Print("Close USB device");
                #endif
                mi_UsbDevice.Dispose();
                mi_UsbDevice = null;
            }

            if (mi_TcpSocket != null)
            {
                #if TRACE_OUTPUT
                    Debug.Print("Close TCP connection");
                #endif
                mi_TcpSocket.Dispose();
                mi_TcpSocket = null;
            }

            if (mi_VxiClient != null)
            {
                #if TRACE_OUTPUT
                    Debug.Print("Close VXI connection");
                #endif
                mi_VxiClient.Dispose();
                mi_VxiClient = null;
            }

            if (mp_HeaderMem != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(mp_HeaderMem);
                mp_HeaderMem = IntPtr.Zero;
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
                    // This is EXTREMELY important! If it is missing you get TIMEOUT's again and again
                    if (mi_UsbDevice != null)
                        mi_UsbDevice.CancelTransfer();

                    Throw("The SCPI command did not execute within the timeout.", true);
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
                Throw("The oscilloscope has returned an invalid floating point value: " + s_Float);

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
        /// ATTENTION: If s32_BlockSize is too small to receive the ENTIRE response, the USB communication will crash.
        /// s32_Timeout = timeout used for sending and for receiving one chunk
        /// </summary>
        public Byte[] SendByteCommand(eBinaryTCP e_BinaryTcp, int s32_BlockSize, String s_Command, int s32_Timeout = DEFAULT_TIMEOUT)
        {
            TransmitString(s_Command, s32_Timeout);

            #if TRACE_OUTPUT
                Debug.Print(">> SendByteCommand() timeout= "+s32_Timeout);
            #endif

            Byte[] u8_Data = null;
            switch (me_Mode)
            {
                case eConnectMode.VXI: u8_Data = mi_VxiClient.DeviceRead(s32_Timeout);   break;
                case eConnectMode.USB: u8_Data = ReceiveUsb(s32_BlockSize, s32_Timeout); break;
                case eConnectMode.TCP: u8_Data = ReceiveTcp(e_BinaryTcp, s32_BlockSize, s32_Timeout); break;
            }

            #if TRACE_OUTPUT
                Debug.Print("<< SendByteCommand() response= {0:N0} byte", u8_Data.Length);
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
            switch (me_Mode)
            {
                case eConnectMode.VXI: mi_VxiClient.DeviceWrite(u8_TxCommand); break;
                case eConnectMode.USB: SendUsbPacket    (u8_TxCommand, 0, s32_Timeout); break;
                case eConnectMode.TCP: mi_TcpSocket.Send(u8_TxCommand, 0, u8_TxCommand.Length, SocketFlags.None); break;
            }
        
            #if TRACE_OUTPUT
                Debug.Print("<< TransmitString() finished");
            #endif
        }

        private String ReceiveString(int s32_Timeout = DEFAULT_TIMEOUT)
        {
            #if TRACE_OUTPUT
                Debug.Print(">> ReceiveString() timeout= "+s32_Timeout);
            #endif

            Byte[] u8_RxData = null;
            switch (me_Mode)
            {
                case eConnectMode.VXI: u8_RxData = mi_VxiClient.DeviceRead(s32_Timeout); break;
                case eConnectMode.USB: u8_RxData = ReceiveUsb(BUF_SIZE_ASCII, s32_Timeout); break;
                case eConnectMode.TCP: u8_RxData = ReceiveTcp(eBinaryTCP.Linefeed, 0, s32_Timeout); break;
            }
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

        // ================================== USB ===================================

        /// <summary>
        /// ATTENTION: If s32_MaxRxData is too small to receive the entire response, the USB communication will crash.
        /// s32_Timeout is for a chunk of data received in one TMC frame.
        /// USB High-Speed bulk data transfer is extremely fast: approx 100 kB in less than 20 ms.
        /// </summary>
        private Byte[] ReceiveUsb(int s32_MaxRxData, int s32_Timeout = DEFAULT_TIMEOUT)
        {
            Byte[] u8_RxBuffer = new Byte[s32_MaxRxData + SIZE_OF_TMC_HEADER];

            MemoryStream i_Stream = new MemoryStream();

            // This loop runs until the device sets the EndOfMessage bit.
            // When requesting 100.000 bytes WaveForm data, Rigol sends one respone packet of 512 byte,
            // then another TMC header with 99524 bytes which are received in one single block.
            while (true)
            {
                // Request response from device
                SendUsbPacket(null, s32_MaxRxData, s32_Timeout);

                #if TRACE_OUTPUT
                    Debug.Print("  >> Receive() requesting " + s32_MaxRxData + " data bytes ...");
                #endif

                // The IVI driver does not allow to call Receive() first to request only the TMC header and then again to read the data.
                // This is no problem with other drivers, but you will screw up the entire communication when you try this here.
                int s32_BytesRead = mi_UsbDevice.Receive(u8_RxBuffer, s32_Timeout);
                if (s32_BytesRead < SIZE_OF_TMC_HEADER)
                {
                    Throw("The USB device has sent a crippled response of " + s32_BytesRead + " bytes.\n"
                        + "This may happen if the receive buffer is too small.");
                }
                // Copy the first bytes of u8_RxBuffer into k_Header
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
                {
                    Throw("The USB device has sent an invalid response header");
                }

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
		private void SendUsbPacket(Byte[] u8_TxCommand, int s32_MaxRxData, int s32_Timeout)
		{
            bool b_Command = u8_TxCommand != null;
            #if TRACE_OUTPUT
                if (b_Command) Debug.Print("  >> SendUsbPacket() sending command (TxData= " + u8_TxCommand.Length + " byte)");
                else           Debug.Print("  >> SendUsbPacket() requesting response (MaxRxData= " + s32_MaxRxData + " byte)");
            #endif

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
                k_Header.s32_DataLen   = s32_MaxRxData; // maximum data length for the requested Bulk IN packet
                k_Header.u8_Attributes = 0; // 0 = Device must ignore u8_TermChar
            }

            // Copy k_Header into u8_Header
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

            mi_UsbDevice.Send(i_Transfer.ToArray(), s32_Timeout);

            #if TRACE_OUTPUT
                Debug.Print("  << SendUsbPacket() finished");
            #endif
        }

        // ================================== TCP ===================================

        /// <summary>
        /// Microsoft forgot to implement a function Socket.Connect(int Timeout)
        /// The default connect timeout is 20 seconds which is much too long --> Use TCP_CONNECT_TIMEOUT instead.
        /// </summary>
        public static Socket ConnectTcpSocketAsync(IPAddress i_IpAddress, UInt16 u16_TcpPort)
        {
            Socket i_TcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            i_TcpSocket.ReceiveTimeout = 2000; // changed later
            i_TcpSocket.SendTimeout    = 1000; // all commands are very short (< 50 byte)

            ManualResetEvent     i_Event = new ManualResetEvent(false);
            SocketAsyncEventArgs i_Args  = new SocketAsyncEventArgs();
            i_Args.RemoteEndPoint = new IPEndPoint(i_IpAddress, u16_TcpPort);
            i_Args.SocketError    = SocketError.TimedOut;
            i_Args.Completed     += delegate(Object o_Sender, SocketAsyncEventArgs i_EvArgs)
            {
                i_Event.Set();
            };

            if (i_TcpSocket.ConnectAsync(i_Args) &&   // returns true if connection is pending
               !i_Event.WaitOne(TCP_CONNECT_TIMEOUT)) // returns false on timeout
                Throw("Could not connect to " + i_Args.RemoteEndPoint + "  (Timeout)");

            if (i_Args.SocketError != SocketError.Success)
                Throw("Could not connect to " + i_Args.RemoteEndPoint + "  (" + i_Args.SocketError + ")");

            return i_TcpSocket;
        }

        /// <summary>
        /// While for USB the struct kTmcHeader has a flag indicating the last packet,
        /// over TCP no such information is available and the linefeed at the end is the only way to detect the end.
        /// But the binary data may also contain multiple values of 0x0A within the data.
        /// The oscilloscope sends the data in many separate packets. Rigol sends packets of 5840 bytes.
        /// See comment of eBinaryTCP for more details.
        /// s32_MinSize is only used in mode eBinaryTCP.MinSize
        /// </summary>
        private Byte[] ReceiveTcp(eBinaryTCP e_BinaryTcp, int s32_MinSize, int s32_Timeout)
        {
            #if TRACE_OUTPUT
                if (e_BinaryTcp == eBinaryTCP.MinSize) Debug.Print("  >> ReceiveTcp(MinSize = {0:N0} byte)", s32_MinSize);
                else                                   Debug.Print("  >> ReceiveTcp(Linefeed)");
            #endif

            MemoryStream i_Stream = new MemoryStream();
            Byte[] u8_Buffer = new Byte[0x8000];

            mi_TcpSocket.ReceiveTimeout = s32_Timeout;
            while (true)
            {
                // s32_RxCount will never be zero. A timeout exception is thrown in case nothing was received.  
                int s32_RxCount = 0;
                try
                {
                    s32_RxCount = mi_TcpSocket.Receive(u8_Buffer, u8_Buffer.Length, SocketFlags.None);
                }
                catch (SocketException Ex)
                {
                    // In case of Timeout the SocketException must be converted into a TimeoutException. 
                    if (Ex.ErrorCode == WSAETIMEDOUT)
                        Throw("Timeout. No response from the oscilloscope.\nRead the Help file!", true);

                    #if TRACE_OUTPUT
                        Debug.Print("*** " + Ex.Message);
                    #endif
                    throw Ex;
                }

                #if TRACE_OUTPUT
                    Debug.Print("     Rx block of {0} byte, Total Rx = {1:N0} byte", s32_RxCount, i_Stream.Length + s32_RxCount);
                #endif
                   
                if (e_BinaryTcp == eBinaryTCP.Linefeed || 
                   (e_BinaryTcp == eBinaryTCP.MinSize && i_Stream.Length + s32_RxCount >= s32_MinSize))
                {
                    // Abort when the block ends with linefeed.
                    // ATTENTION: There may be padding bytes behind the linefeed.
                    // The linefeed is not necessarily the last byte --> search the last 4 bytes for a linefeed.
                    int s32_LF = Utils.FindByteReverse(u8_Buffer, s32_RxCount, 4, 0x0A);
                    if (s32_LF > -1)
                    {
                        // Append all received bytes except the linefeed (and padding) at the end
                        i_Stream.Write(u8_Buffer, 0, s32_LF);
                        break;
                    }
                }

                // Append the entire block, more data will follow.
                i_Stream.Write(u8_Buffer, 0, s32_RxCount);
            }

            #if TRACE_OUTPUT
                Debug.Print("  << ReceiveTcp() --> received {0:N0} bytes", i_Stream.Length);
            #endif
            return i_Stream.ToArray();
        }

        // ================================== Helper ====================================

        /// <summary>
        /// The TimeoutException has a sepcial treatment: 
        /// It is is handled in PanelRigol.SendManualCommand() when manually sending an invalid command.
        /// </summary>
        static void Throw(String s_Message, bool b_Timeout = false)
        {
            #if TRACE_OUTPUT
                Debug.Print("*** " + s_Message);
            #endif

            if (b_Timeout) throw new TimeoutException(s_Message);
            else           throw new Exception(s_Message);
        }
    }
}
