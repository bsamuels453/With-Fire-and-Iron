#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;

#endregion

namespace Forge.Framework.Resources{
    internal class ConfigLoader : ResourceLoader{
        const string _configDir = "Config";

        readonly Dictionary<string, JObject> _cachedConfigs;

        public ConfigLoader(){
            var timer = new Stopwatch();
            timer.Start();
            DebugConsole.WriteLine("Loading configuration files");
            var configFiles = GetAllFilesInDirectory(_configDir);

            configFiles = (
                from file in configFiles
                where file.Extension == ".config"
                select file
                ).ToList();

            _cachedConfigs = new Dictionary<string, JObject>(configFiles.Count);
            foreach (var file in configFiles){
                var jobj = Resource.LoadJObject(file.RelativeFileLocation);
                _cachedConfigs.Add(file.RelativeFileLocation, jobj);
            }
            timer.Stop();
            DebugConsole.WriteLine("Configuration files loaded in " + timer.ElapsedMilliseconds + " ms");
        }

        public JObject LoadConfig(string fileAddr){
            try{
                var ret = _cachedConfigs[fileAddr];
                return ret;
            }
            catch (Exception e){
                //we only catch this because i want it logged
                DebugConsole.WriteLine("FATAL: Configuration file queried that does not exist: " + fileAddr);
                DebugConsole.WriteLine("FATAL: Cache query returned " + e.Message);
                throw new Exception("Tried to load config file that doesnt exist: " + fileAddr);
            }
        }


        public override void Dispose(){
            _cachedConfigs.Clear();
        }
    }
}