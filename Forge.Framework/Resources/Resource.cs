#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using Cloo;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Forge.Framework.Resources{
    public static class Resource{
        #region RawDir enum

        public enum RawDir{
            Config,
            Scripts,
            Templates
        }

        #endregion

        public static GraphicsDevice Device;
        public static ContentManager ContentManager;
        public static Dictionary<string, string> RawLookup;
        public static Matrix ProjectionMatrix;
        public static ScreenSize ScreenSize;
        static OpenCLScriptLoader _openCLScriptLoader;

        /// <summary>
        ///   whenever the md5 doesn't match up to the new one, the value of RawDir in the following structure is set to true
        /// </summary>
        public static Dictionary<RawDir, bool> HasRawHashChanged;

        /// <summary>
        ///   If the md5 has changed, the new md5 won't be written to file until its RawDir in this dictionary is true. This prevents the md5 from updating when certain scripts havent been compiled/processed before the program terminates.
        /// </summary>
        public static Dictionary<RawDir, bool> AllowMD5Refresh;

        //todo: write jsonconverter for this enum

        public static void Initialize(){

            _openCLScriptLoader = new OpenCLScriptLoader();


            RawLookup = new Dictionary<string, string>();

            List<string> directories = new List<string>();
            List<string> directoriesToSearch = new List<string>();

            string currentDirectory = Directory.GetCurrentDirectory() + "\\Config\\";
            directoriesToSearch.Add(currentDirectory);

            while (directoriesToSearch.Count > 0){
                string dir = directoriesToSearch[0];
                directoriesToSearch.RemoveAt(0);
                directoriesToSearch.AddRange(Directory.GetDirectories(dir).ToList());
                directories.Add(dir);
            }
            string s;
            try{
                foreach (string directory in directories){
                    s = directory;
                    string[] files = Directory.GetFiles(directory);
                    foreach (string file in files){
                        var sr = new StreamReader(file);
                        s = file;
                        var newConfigVals = JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd());
                        sr.Close();

                        string prefix = newConfigVals["InternalAbbreviation"] + "_";
                        newConfigVals.Remove("InternalAbbreviation");

                        foreach (var configVal in newConfigVals){
                            try{
                                RawLookup.Add(prefix + configVal.Key, configVal.Value);
                            }
                            catch (Exception e){
                                int f = 4;
                            }
                        }
                    }
                }
            }
            catch (Exception e){
                int g = 5;
            }
        }

        public static T LoadContent<T>(string str){
            string objValue = "";
            if (str.Contains('/')){
                return ContentManager.Load<T>(str);
            }

            try{
                objValue = RawLookup[str];
                return ContentManager.Load<T>(objValue);
            }
            catch{
                T obj;
                try{
                    obj = JsonConvert.DeserializeObject<T>(objValue);
                }
                catch{
                    obj = VectorParser.Parse<T>(objValue);
                }
                return obj;
            }
        }

        public static void LoadShader(string shaderName, out Effect effect){
            effect = null;
            var configs = new List<string>();
            var configValues = new List<string>();
            foreach (var valuePair in RawLookup){
                if (valuePair.Key.Length < shaderName.Length + 1)
                    continue;
                string sub = valuePair.Key.Substring(0, shaderName.Count());
                if (sub.Contains(shaderName)){
                    configs.Add(valuePair.Key.Substring(shaderName.Count() + 1));
                    configValues.Add(valuePair.Value);
                }
            }

            for (int i = 0; i < configs.Count; i++){
                if (configs[i] == "Shader"){
                    effect = ContentManager.Load<Effect>(configValues[i]).Clone();
                    if (effect == null){
                        throw new Exception("Shader not found");
                    }
                    break;
                }
            }
            if (effect == null){
                throw new Exception("Shader not specified for " + shaderName);
            }

            //figure out its datatype
            for (int i = 0; i < configs.Count; i++){
                string name = configs[i];
                string configVal = configValues[i];

                if (configVal.Contains(",")){
                    //it's a vector
                    var commas = from value in configVal
                        where value == ','
                        select value;
                    int commaCount = commas.Count();
                    switch (commaCount){
                        case 1:
                            var vec2 = VectorParser.Parse<Vector2>(configVal);
                            effect.Parameters[name].SetValue(vec2);
                            break;
                        case 2:
                            var vec3 = VectorParser.Parse<Vector3>(configVal);
                            effect.Parameters[name].SetValue(vec3);
                            break;
                        case 3:
                            var vec4 = VectorParser.Parse<Vector4>(configVal);
                            effect.Parameters[name].SetValue(vec4);
                            break;
                        default:
                            throw new Exception("vector4 is the largest dimension of vector supported");
                    }
                    continue;
                }
                //figure out if it's a string
                var alphanumerics = from value in configVal
                    where char.IsLetter(value)
                    select value;
                if (alphanumerics.Any()){
                    if (name == "Shader")
                        continue;
                    //it's a string, and in the context of shader settings, strings always coorespond with texture names
                    var texture = ContentManager.Load<Texture2D>(configVal);
                    effect.Parameters[name].SetValue(texture);
                    continue;
                }

                if (configVal.Contains(".")){
                    //it's a float
                    effect.Parameters[name].SetValue(float.Parse(configVal));
                    continue;
                }

                //assume its an integer
                effect.Parameters[name].SetValue(int.Parse(configVal));
            }
        }

        public static ComputeContext CLContext{
            get { return _openCLScriptLoader.ComputeContext; }
        }

        public static ComputeCommandQueue CLQueue{
            get { return _openCLScriptLoader.CommandQueue; }
        }

        public static ComputeProgram LoadCLScript(string scriptName){
            return _openCLScriptLoader.LoadOpenclScript(scriptName);
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

    public struct ScreenSize{
        public int X;
        public int Y;

        public ScreenSize(int x, int y){
            X = x;
            Y = y;
        }

        public void GetScreenValue(float percentX, float percentY, ref int x, ref int y){
            x = (int) (X*percentX);
            y = (int) (Y*percentY);
        }

        public int GetScreenValueX(float percentX){
            return (int) (X*percentX);
        }

        public int GetScreenValueY(float percentY){
            return (int) (Y*percentY);
        }
    }
}