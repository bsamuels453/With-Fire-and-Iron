#define ENABLE_DEBUG_CONSOLE

#region

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;

#endregion

namespace Forge.Framework{
    public static class DebugConsole{
        static IOWrapper _wrapper;
        const int _port = 10964;

        public static void InitalizeConsole(){
            _wrapper = new IOWrapper();

            _wrapper.FileWriter = new StreamWriter("Output.txt");
            _wrapper.FileWriter.AutoFlush = true;

#if ENABLE_DEBUG_CONSOLE
            var ip = IPAddress.Parse("127.0.0.1");
            _wrapper.Endpoint = new IPEndPoint(ip, _port);
            _wrapper.Client = new UdpClient();
            _wrapper.ExternConsoleEnabled = true;
            WriteLine("---------- GAME STARTING ----------");
#else
            _wrapper.ExternConsoleEnabled = false;
            WriteLine("Debug console not responding, text log only");
#endif
        }

        public static void WriteLine(string s){
            string timeStamp = "[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "] ";
            var line = timeStamp + s;
            if (_wrapper.ExternConsoleEnabled) {
                try{
                    _wrapper.SendToConsole(line);
                    _wrapper.FileWriter.WriteLine(line);
                }
                catch{
                    _wrapper.FileWriter.WriteLine(line);
                    _wrapper.ExternConsoleEnabled = false;
                }
            }
        }

        public static void DisposeStatic(){
            _wrapper.Dispose();
        }

        /// <summary>
        /// this is used for hackish destructor
        /// </summary>
        internal class IOWrapper : IDisposable{
            public UdpClient Client;
            public StreamWriter FileWriter;
            public bool ExternConsoleEnabled;
            public IPEndPoint Endpoint;
            public Process ConsoleProcess;
            bool _disposed;

            public IOWrapper(){
                _disposed = false;
            }

            public void SendToConsole(string str){
                var bytes = Encoding.ASCII.GetBytes(str);
                Client.Send(bytes, bytes.Length, Endpoint);
            }

            public void Dispose(){
                if (ExternConsoleEnabled) {
                    try {
                        SendToConsole("---------- GAME TERMINATED ----------");
                    }
                    catch {
                        int test = 5;
                    }
                    Client.Close();
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