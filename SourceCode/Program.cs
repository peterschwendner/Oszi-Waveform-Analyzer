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
using System.IO.Pipes;
using System.Threading;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;

namespace OsziWaveformAnalyzer
{
    static class Program
    {
        const String PIPE_NAME    = "OsziWaveformAnalyer_Pipe";
        const String CONFIRM_EXEC = "I take over";
        const String DENY_EXEC    = "User has denied";

        static NamedPipeServerStream mi_PipeServer;
        static String                ms_CmdLineOpenFile;

        /// <summary>
        /// The path to an Oszi file in the commandline that the user has double clicked
        /// </summary>
        public static String CmdOpenFile
        {
            get { return ms_CmdLineOpenFile; }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Debug.Print(">> Main()");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // The user has double clicked an .OSZI file which results in a Commandline like:
            // "C:\Program Files\OsziWaveformAnalyzer.exe" -open "D:\Temp\MyCapture.oszi" 
            String s_Cmd = Environment.CommandLine;
            int s32_Open = s_Cmd.IndexOf(Utils.CMD_LINE_ACTION);
            if (s32_Open > 0)
            {
                String s_OsziFile = s_Cmd.Substring(s32_Open + Utils.CMD_LINE_ACTION.Length).Trim('"');
                if (File.Exists(s_OsziFile) && Path.GetExtension(s_OsziFile).ToLower() == ".oszi")
                    ms_CmdLineOpenFile = s_OsziFile;
            }

            // Check if there is already an instance of Oszi Waveform Analyer running.
            // If so, send the path of the double-clicked file over a pipe.
            if (ms_CmdLineOpenFile != null)
            {
                try
                {
                    NamedPipeClientStream i_PipeClient = new NamedPipeClientStream(PIPE_NAME);
                    i_PipeClient.Connect(100);
                    Debug.Print("Main() Successfully connected to server");

                    Byte[] u8_Path = Encoding.Unicode.GetBytes(ms_CmdLineOpenFile);
                    i_PipeClient.Write(u8_Path, 0, u8_Path.Length);
                    Debug.Print("Main() Path sent: " + ms_CmdLineOpenFile);

                    Byte[] u8_Response = new Byte[50];
                    int    s32_Read   = i_PipeClient.Read(u8_Response, 0, u8_Response.Length);
                    String s_Response = Encoding.Unicode.GetString(u8_Response, 0, s32_Read);
                    Debug.Print("Main() Response received: " + s_Response);

                    if (s_Response == CONFIRM_EXEC)
                    {
                        i_PipeClient.Close();
                        Debug.Print("<< Main() --> Exit. The server process will open the file");
                        return;
                    }
                }
                catch (Exception Ex) // Timeout
                {
                    Debug.Print("Main() Error connecting to PipeServer: " + Ex.Message);
                } 
            }

            Thread i_Thread = new Thread(new ThreadStart(PipeServerThread));
            i_Thread.IsBackground = true;
            i_Thread.Start();

            Application.Run(new FormMain());
            Debug.Print("<< Main()");
        }

        static void PipeServerThread()
        {
            Debug.Print(">> PipeServerThread()");
            try
            {
                mi_PipeServer = new NamedPipeServerStream(PIPE_NAME);
            }
            catch (Exception Ex) // IoException "All pipe instances are busy"
            {
                Debug.Print("<< PipeServerThread() Error starting pipe server: " + Ex.Message);
                return; // only one PipeServer can run with the same pipe name at the same time
            }

            Byte[] u8_Path = new Byte[1000];
            while (true)
            {
                try
                {
                    Debug.Print("PipeServerThread() Wait for connection...");
                    mi_PipeServer.WaitForConnection();
                    Debug.Print("PipeServerThread() --> Client has connected");

                    int s32_Read = mi_PipeServer.Read(u8_Path, 0, u8_Path.Length);
                    String s_PathToOpen = Encoding.Unicode.GetString(u8_Path, 0, s32_Read);
                    Debug.Print("PipeServerThread() --> Received open path: " + s_PathToOpen);

                    // If user has unsaved changes, ask if he wants top open the file.
                    bool   b_Confirm   = Utils.FormMain.RemoteOpenRequest(s_PathToOpen);
                    String s_Response  = b_Confirm ? CONFIRM_EXEC : DENY_EXEC;
                    Byte[] u8_Response = Encoding.Unicode.GetBytes(s_Response);
                    mi_PipeServer.Write(u8_Response, 0, u8_Response.Length);
                    Debug.Print("PipeServerThread() --> Response sent: " + s_Response);

                    mi_PipeServer.Disconnect();
                }
                catch (Exception Ex)
                {
                    Debug.Print("PipeServerThread() Error : " + Ex.Message);
                    mi_PipeServer.Disconnect();
                    Thread.Sleep(100);
                }
            }
        }
    }
}
