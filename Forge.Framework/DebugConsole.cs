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
        static StreamWriter _consoleWriter;
        static StreamWriter _fileWriter;
        static bool _externConsoleEnabled;
        static Process _consoleProcess;

        public static void InitalizeConsole(Game game){
            _consoleProcess = new Process();
            _consoleProcess.StartInfo.FileName = "DebugConsole.exe";
            _consoleProcess.StartInfo.UseShellExecute = true;
            _consoleProcess.Start();
            Thread.Sleep(20); //takes a bit of time for console to initalize, so hold it here so none of the startup debug info is lost

            _fileWriter = new StreamWriter("debuglog.txt");
            _fileWriter.AutoFlush = true;

            _client = new TcpClient();
            AsyncCallback callback = ar => { };
            var asyncResult = _client.BeginConnect("127.0.0.1", 10965, callback, null);

            asyncResult.AsyncWaitHandle.WaitOne(200);
            if (asyncResult.IsCompleted){
                _externConsoleEnabled = true;
                _consoleWriter = new StreamWriter(_client.GetStream());
                _consoleWriter.AutoFlush = true;
            }
            else{
                _externConsoleEnabled = false;
            }
        }

        public static void WriteLine(string s){
            if (_externConsoleEnabled){
                try{
                    _consoleWriter.WriteLine(s);
                }
                catch{
                    _externConsoleEnabled = false;
                }
            }
            _fileWriter.WriteLine(s);
        }

        public static void Dispose(){
            if (_externConsoleEnabled){
                try{
                    _consoleWriter.WriteLine("KILLCONSOLE");
                }
                catch{

                }
                _consoleWriter.Close();
                _client.Close();
            }
            _fileWriter.Close();
        }
    }
}