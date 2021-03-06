﻿#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Cloo;
using Newtonsoft.Json;

#endregion

namespace Forge.Framework.Resources{
    /// <summary>
    ///   Used to wrap OpenCL script loading, compiling, and saving.
    /// </summary>
    internal class OpenCLScriptLoader : ResourceLoader{
        const string _openclScriptDir = "Scripts/Opencl";
        const string _hashFile = "Scripts/Hash.json";
        readonly Task _asyncLoadTask;
        readonly List<ComputeDevice> _devices;
        readonly ComputePlatform _platform;
        readonly ComputeContextPropertyList _properties;
        readonly List<OpenCLScript> _scripts;
        ComputeCommandQueue _commandQueue;
        ComputeContext _computeContext;

        public OpenCLScriptLoader(){
            //first initlze opencl
            _platform = ComputePlatform.Platforms[0];
            /*
#if CPU_DEBUG
            var platform = ComputePlatform.Platforms[1];
#else
            var platform = ComputePlatform.Platforms[0];
#endif
             */
            _devices = new List<ComputeDevice>();
            _devices.Add(_platform.Devices[0]);
            _properties = new ComputeContextPropertyList(_platform);
            _scripts = new List<OpenCLScript>();
            _asyncLoadTask = new Task(AsyncLoad);
            _asyncLoadTask.Start();
        }

        /// <summary>
        ///   The context in which openCL scripts will be run.
        /// </summary>
        public ComputeContext ComputeContext{
            get{
                BlockForLoader();
                return _computeContext;
            }
        }

        /// <summary>
        ///   The queue of commands for the context to execute.
        /// </summary>
        public ComputeCommandQueue CommandQueue{
            get{
                BlockForLoader();
                return _commandQueue;
            }
        }


        /// <summary>
        /// blocks the thread of execution until the _asyncLoadTask has completed
        /// </summary>
        void BlockForLoader(){
            if (_asyncLoadTask.Status != TaskStatus.RanToCompletion)
                _asyncLoadTask.Wait();
        }

        /// <summary>
        /// Asynchronously load the opencl context and compile scripts, if necessary.
        /// </summary>
        void AsyncLoad(){
            var timer = new Stopwatch();
            timer.Start();
            DebugConsole.WriteLine("Initializing OpenCL context");
            _computeContext = new ComputeContext(_devices, _properties, null, IntPtr.Zero);
            _commandQueue = new ComputeCommandQueue(_computeContext, _devices[0], ComputeCommandQueueFlags.None);
            DebugConsole.WriteLine("OpenCL context initialized in " + timer.ElapsedMilliseconds + " ms");

            //now we check to make sure none of the scripts have changed since the last time they were compiled.
            var scriptFiles = GetAllFilesInDirectory(_openclScriptDir);

            var jobj = Resource.LoadJObject(_hashFile);

            var oldhash = jobj["Hash"].ToObject<string>();
            var currenthash = GenerateCumulativeSHA(scriptFiles);

            bool compileScripts = !oldhash.Equals(currenthash);

            List<OpenCLScript> compiledScripts;

            if (compileScripts){
                DebugConsole.WriteLine("The hash of an OpenCL script has changed since last execution, recompiling OpenCL scripts...");
                timer.Restart();
                compiledScripts = CompileScripts(scriptFiles);
                DebugConsole.WriteLine("OpenCL script recompilation completed in " + timer.ElapsedMilliseconds + " ms");
                SaveBinaries(compiledScripts);
                SaveSHA(currenthash);
            }
            else{
                compiledScripts = LoadScripts(scriptFiles);
                DebugConsole.WriteLine("Loaded OpenCL script binaries.");
            }
            _scripts.AddRange(compiledScripts);
        }

        /// <summary>
        ///   Saves hash to the _hashFile file under the Scripts category.
        /// </summary>
        /// <param name="hash"> </param>
        public static void SaveSHA(string hash){
            var jobj = Resource.LoadJObject(_hashFile);

            jobj["Hash"] = hash;
            var sw = new StreamWriter(_hashFile);
            string ss = JsonConvert.SerializeObject(jobj, Formatting.Indented);
            sw.Write(ss);
            sw.Close();
        }

        /// <summary>
        ///   Returns an OpenCL script that was compiled/loaded at the start of the program.
        /// </summary>
        /// <param name="fileName"> The relative file location of the script. </param>
        /// <returns> </returns>
        public ComputeProgram LoadOpenclScript(string fileName){
            BlockForLoader();
            var scriptEnumerable = from s in _scripts where s.SrcFileInfo.RelativeFileLocation.Equals(fileName) select s;
            var script = scriptEnumerable.Single(); //defensive precaution
            return script.Program;
        }

        /// <summary>
        ///   Generates a hash that represents the contents of all of the files in the files parameter. The hash for each individual file is appended together into one string and returned.
        /// </summary>
        /// <param name="files"> </param>
        /// <returns> </returns>
        static string GenerateCumulativeSHA(List<FileAttributes> files){
            var shaGen = SHA256.Create();

            string cumulative = "";

            foreach (var file in files){
                var strmRdr = new StreamReader(file.FullFileLocation);
                cumulative += strmRdr.ReadToEnd();
                strmRdr.Close();
            }

            var byteRepresen = new byte[cumulative.Length];

            for (int i = 0; i < cumulative.Length; i++){
                byteRepresen[i] = Convert.ToByte(cumulative[i]);
            }

            var hash = shaGen.ComputeHash(byteRepresen);
            var strHash = Convert.ToBase64String(hash);

            return strHash;
        }

        /// <summary>
        ///   Loads openCL binaries from the "Compiled" folder. Only loads scripts specified by the RelativeFileLocation field of the files parameter.
        /// </summary>
        /// <param name="files"> </param>
        /// <returns> </returns>
        List<OpenCLScript> LoadScripts(ICollection<FileAttributes> files){
            var ret = new List<OpenCLScript>(files.Count);

            foreach (var file in files){
                string address = "Compiled/" + file.RelativeFileLocation;
                address += "c"; //the file extension for a compiled binary is .clc for .cl files

                var binaryFormatter = new BinaryFormatter();
                var fileStrm = new FileStream(address, FileMode.Open, FileAccess.Read, FileShare.None);
                var binary = (ReadOnlyCollection<byte[]>) binaryFormatter.Deserialize(fileStrm);
                fileStrm.Close();
                ret.Add
                    (new OpenCLScript
                        (
                        new ComputeProgram
                            (
                            _computeContext,
                            binary,
                            _devices
                            ),
                        file
                        ));
                ret.Last().Program.Build(null, "", null, IntPtr.Zero);
            }

            return ret;
        }

        /// <summary>
        ///   Saves openCL binaries to the "Compiled" folder.
        /// </summary>
        /// <param name="scripts"> </param>
        static void SaveBinaries(IEnumerable<OpenCLScript> scripts){
            foreach (var clScript in scripts){
                var directory = Directory.GetCurrentDirectory() + "/Compiled/" + clScript.SrcFileInfo.RelativeFileLocation;
                directory += "c"; //the file extension for a compiled binary is .clc for .cl files
                var binaryFormatter = new BinaryFormatter();
                var fileStrm = new FileStream(directory, FileMode.Create, FileAccess.Write, FileShare.None);
                binaryFormatter.Serialize(fileStrm, clScript.Program.Binaries);
                fileStrm.Close();
            }
        }

        /// <summary>
        ///   Compile openCL scripts by loading the files theyre located in and compiling them using platform compiler.
        /// </summary>
        /// <param name="scripts"> File attributes for the scripts that are to be loaded. One script per file for OpenCL. </param>
        /// <returns> </returns>
        List<OpenCLScript> CompileScripts(ICollection<FileAttributes> scripts){
            var ret = new List<OpenCLScript>(scripts.Count);
            foreach (var file in scripts){
                string scriptContents;
                using (var sr = new StreamReader(file.FullFileLocation)){
                    scriptContents = sr.ReadToEnd();
                }
                var program = new ComputeProgram(_computeContext, scriptContents);
                program.Build(null, "", null, IntPtr.Zero);

                ret.Add(new OpenCLScript(program, file));
            }

            return ret;
            //old build method that might be useful later
            /*
#if CPU_DEBUG
                _generationPrgm.Build(null, @"-g -s D:\Projects\Forge\Scripts\GenTerrain.cl", null, IntPtr.Zero); //use option -I + scriptDir for header search
#else
                 */
        }

        public override void Dispose(){
            foreach (var openCLScript in _scripts){
                openCLScript.Dispose();
            }
            CommandQueue.Dispose();
            ComputeContext.Dispose();
        }

        #region Nested type: OpenCLScript

        struct OpenCLScript : IDisposable{
            public readonly ComputeProgram Program;
            public readonly FileAttributes SrcFileInfo;

            public OpenCLScript(ComputeProgram program, FileAttributes srcFileInfo){
                Program = program;
                SrcFileInfo = srcFileInfo;
            }

            #region IDisposable Members

            public void Dispose(){
                Program.Dispose();
            }

            #endregion
        }

        #endregion
    }
}