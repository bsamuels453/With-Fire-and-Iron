#region

using System;
using System.Collections.Generic;
using System.IO;
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
            try {
                string realName = RawLookup[str];
                return ContentManager.Load<T>(realName);
            }
            catch (Exception){
                return (T)Convert.ChangeType(RawLookup[str], typeof(T));
            }
        }

        public static string LoadScript(string str){
            string address = RawLookup[str];
            var sr = new StreamReader("Raw\\" + address);
            string scriptText = sr.ReadToEnd();
            sr.Close();
            return scriptText;
        }
    }
}