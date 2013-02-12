#region

using System;
using System.Collections.Generic;
using System.IO;
using Gondola.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

#endregion

namespace Gondola.Logic{
    internal static class Gbl{
        public static Vector3 CameraPosition;
        public static Vector3 CameraTarget;
        public static GraphicsDevice Device;
        public static ContentManager ContentManager;
        public static Dictionary<string, string> RawLookup;
        public static Matrix ProjectionMatrix;
        public static Point ScreenSize;

        static Gbl(){
            RawLookup = new Dictionary<string, string>();
            var files = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Raw\\Config\\");
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

        public static T LoadContent<T>(string str){
            string objValue = "";
            try {
                objValue = RawLookup[str];
                return ContentManager.Load<T>(objValue);
            }
            catch (Exception e){
                T obj;
                try {
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
            var sr = new StreamReader("Raw\\" + address);
            string scriptText = sr.ReadToEnd();
            sr.Close();
            return scriptText;
        }

        public static string GetScriptDirectory(string str) {
            string curDir = Directory.GetCurrentDirectory();
            string address = RawLookup[str];
            string directory = "";
            for (int i = address.Length - 1; i >= 0; i--){
                if( address[i] == '\\'){
                    directory = address.Substring(0, i);
                }
            }
            return curDir + "\\Raw\\" + directory;
        }
    }
}