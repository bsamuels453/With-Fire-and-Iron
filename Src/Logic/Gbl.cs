#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using Gondola.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Gondola.Logic{
    internal static class Gbl{
        public static GraphicsDevice Device;
        public static ContentManager ContentManager;
        public static Dictionary<string, string> RawLookup;
        public static Matrix ProjectionMatrix;
        public static Point ScreenSize;

        /// <summary>
        /// whenever the md5 doesn't match up to the new one, the value of RawDir in the following structure is set to true
        /// </summary>
        public static Dictionary<RawDir, bool> HasRawHashChanged;

        /// <summary>
        /// If the md5 has changed, the new md5 won't be written to file until its RawDir in this dictionary is true.
        /// This prevents the md5 from updating when certain scripts havent been compiled/processed before the program terminates.
        /// </summary>
        public static Dictionary<RawDir, bool> AllowMD5Refresh;

        static Dictionary<RawDir, string> _fileHashes;

        //todo: write jsonconverter for this enum
        public enum RawDir{
            Config,
            Scripts,
            Templates
        }

        static Gbl(){
            CheckHashes();
            RawLookup = new Dictionary<string, string>();
            var files = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Config\\");
            foreach (var file in files){
                var sr = new StreamReader(file);

                var newConfigVals = JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd());
                sr.Close();

                string prefix = newConfigVals["InternalAbbreviation"] + "_";
                newConfigVals.Remove("InternalAbbreviation");

                foreach (var configVal in newConfigVals){
                    RawLookup.Add(prefix + configVal.Key, configVal.Value);
                }
            }
        }

        static void CheckHashes(){
            HasRawHashChanged = new Dictionary<RawDir, bool>();
            AllowMD5Refresh = new Dictionary<RawDir, bool>();
            _fileHashes = new Dictionary<RawDir, string>();
            var sr = new StreamReader((Directory.GetCurrentDirectory() + "\\Data\\Hashes.json"));
            var jobj = JObject.Parse(sr.ReadToEnd());
            sr.Close();
            MD5 md5Gen = MD5.Create();
            foreach (var hashEntry in jobj){
                string fileDir = hashEntry.Key;
                var hash = hashEntry.Value.ToObject<string>();
                var files = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\" + fileDir + "\\");
                string newHash = "";
                foreach (var file in files){
                    var fr = new FileStream(file, FileMode.Open, FileAccess.Read);
                    var fileHash = md5Gen.ComputeHash(fr);
                    sr.Close();

                    for (int i = 0; i < 16; i++){
                        string temp = "";
                        foreach (var b in fileHash){
                            temp += b.ToString();
                        }
                        newHash += temp;
                    }
                }

                RawDir dir = RawDir.Templates;
                switch (fileDir){
                    case "Config":
                        dir = RawDir.Config;
                        break;
                    case "Templates":
                        dir = RawDir.Templates;
                        break;
                    case "Scripts":
                        dir = RawDir.Scripts;
                        break;
                }

                _fileHashes.Add(dir, newHash);
                if (!hash.SequenceEqual(newHash)){
                    HasRawHashChanged.Add(dir, true);
                    AllowMD5Refresh.Add(dir, false);
                }
                else{
                    HasRawHashChanged.Add(dir, false);
                }
            }
        }

        public static void CommitHashChanges(){
            var sr = new StreamReader((Directory.GetCurrentDirectory() + "\\Data\\Hashes.json"));
            var jobj = JObject.Parse(sr.ReadToEnd());
            sr.Close();

            foreach (var hashEntry in AllowMD5Refresh){
                var dir = hashEntry.Key;
                bool updateHash = hashEntry.Value;
                string s = dir.ToString();
                if (updateHash){
                    jobj[s] = _fileHashes[dir];
                }
            }
            var sw = new StreamWriter((Directory.GetCurrentDirectory() + "\\Data\\Hashes.json"));
            string ss = JsonConvert.SerializeObject(jobj, Formatting.Indented);
            sw.Write(ss);
            sw.Close();

        }

        public static T LoadContent<T>(string str){
            string objValue = "";
            try{
                objValue = RawLookup[str];
                return ContentManager.Load<T>(objValue);
            }
            catch (Exception e){
                T obj;
                try{
                    obj = JsonConvert.DeserializeObject<T>(objValue);
                }
                catch (Exception ee){
                    obj = JsonFallbackParser.Parse<T>(objValue);
                }
                return obj;
            }
        }

        public static string LoadScript(string str){
            string address = RawLookup[str];
            var sr = new StreamReader(address);
            string scriptText = sr.ReadToEnd();
            sr.Close();
            return scriptText;
        }

        public static ReadOnlyCollection<byte[]> LoadBinary(string str){
            string address = "Compiled\\" + RawLookup[str];
            address += "c"; //the file extension for a compiled binary is .clc for .cl files

            var binaryFormatter = new BinaryFormatter();
            var fileStrm = new FileStream(address, FileMode.Open, FileAccess.Read, FileShare.None);
            var binary = (ReadOnlyCollection<byte[]>) binaryFormatter.Deserialize(fileStrm);
            fileStrm.Close();
            return binary;
        }

        public static void SaveBinary(ReadOnlyCollection<byte[]> binary, string str){
            string address = "Compiled\\" + RawLookup[str];
            address += "c"; //the file extension for a compiled binary is .clc for .cl files

            var binaryFormatter = new BinaryFormatter();
            var fileStrm = new FileStream(address, FileMode.Create, FileAccess.Write, FileShare.None);
            binaryFormatter.Serialize(fileStrm, binary);
            fileStrm.Close();
        }


        public static string GetScriptDirectory(string str){
            string curDir = Directory.GetCurrentDirectory();
            string address = RawLookup[str];
            string directory = "";
            for (int i = address.Length - 1; i >= 0; i--){
                if (address[i] == '\\'){
                    directory = address.Substring(0, i);
                }
            }
            return curDir + "\\" + directory;
        }
    }
}