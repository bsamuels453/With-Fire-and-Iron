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

        public static void InitalizeConsole(Game game){
            _wrapper = new IOWrapper();
            _consoleProcess = new Process();
            _consoleProcess.StartInfo.FileName = "DebugConsole.exe";
            _consoleProcess.StartInfo.UseShellExecute = true;
            _consoleProcess.Start();
            Thread.Sleep(20); //takes a bit of time for console to initalize, so hold it here so none of the startup debug info is lost

            _wrapper.FileWriter = new StreamWriter("debuglog.txt");
            _wrapper.FileWriter.AutoFlush = true;

            _client = new TcpClient();
            AsyncCallback callback = ar => { };
            var asyncResult = _client.BeginConnect("127.0.0.1", 10965, callback, null);

            asyncResult.AsyncWaitHandle.WaitOne(200);
            if (asyncResult.IsCompleted){
                _wrapper.ExternConsoleEnabled = true;
                _wrapper.ConsoleWriter = new StreamWriter(_client.GetStream());
                _wrapper.ConsoleWriter.AutoFlush = true;
            }
            else{
                _wrapper.ExternConsoleEnabled = false;
            }
        }

        public static void WriteLine(string s){
            if (_wrapper.ExternConsoleEnabled) {
                try{
                    _wrapper.ConsoleWriter.WriteLine(s);
                }
                catch{
                    _wrapper.ExternConsoleEnabled = false;
                }
            }
            _wrapper.FileWriter.WriteLine(s);
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