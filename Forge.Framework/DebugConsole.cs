//#define ENABLE_DEBUG_CONSOLE

#region

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Xna.Framework;

#endregion

namespace Forge.Framework{
    public static class DebugConsole{
        static TcpClient _client;
        static IOWrapper _wrapper;
        static Process _consoleProcess;

        public static void InitalizeConsole(){
            _wrapper = new IOWrapper();

            _wrapper.FileWriter = new StreamWriter("Output.txt");
            _wrapper.FileWriter.AutoFlush = true;

#if ENABLE_DEBUG_CONSOLE
            _consoleProcess = new Process();
            _consoleProcess.StartInfo.FileName = "DebugConsole.exe";
            _consoleProcess.StartInfo.UseShellExecute = false;
            _consoleProcess.Start();
            _client = new TcpClient();

            var asyncResult = _client.BeginConnect("127.0.0.1", 10964, null, null);

            int numSleeps = 0;
            while (!asyncResult.IsCompleted && numSleeps<150){
                Thread.Sleep(10);
                numSleeps++;
            }
            if (asyncResult.IsCompleted){
                _wrapper.ExternConsoleEnabled = true;
                _wrapper.ConsoleWriter = new StreamWriter(_client.GetStream());
                _wrapper.ConsoleWriter.AutoFlush = true;
                WriteLine("Debug console connection established");
            }
            else{
                _wrapper.ExternConsoleEnabled = false;
                WriteLine("Debug console not responding, text log only");
            }
#else
            _wrapper.ExternConsoleEnabled = false;
            WriteLine("Debug console not responding, text log only");
#endif
        }

        public static void WriteLine(string s){
            string timeStamp = "[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "] ";
            if (_wrapper.ExternConsoleEnabled) {
                try{
                    _wrapper.ConsoleWriter.WriteLine(timeStamp + s);
                }
                catch{
                    _wrapper.ExternConsoleEnabled = false;
                }
            }
            _wrapper.FileWriter.WriteLine(timeStamp + s);
        }

        public static void Dispose(){
            _wrapper.Dispose();
        }

        /// <summary>
        /// this is used for hackish destructor
        /// </summary>
        internal class IOWrapper : IDisposable{
            public StreamWriter ConsoleWriter;
            public StreamWriter FileWriter;
            public bool ExternConsoleEnabled;
            public Process ConsoleProcess;
            bool _disposed;

            public IOWrapper(){
                _disposed = false;
            }

            public void Dispose(){
                if (ExternConsoleEnabled) {
                    try {
                        ConsoleWriter.WriteLine("KILLCONSOLE");
                    }
                    catch {
                        int f = 3;
                    }
                    ConsoleWriter.Close();
                    _client.Close();
                }
                FileWriter.Close();
                _disposed = true;
            }

            ~IOWrapper(){
                if (!_disposed){
                    Dispose();
                }
            }
        }
    }


}