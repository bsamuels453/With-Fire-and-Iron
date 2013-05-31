#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cloo;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;
using Newtonsoft.Json;

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

        public static GraphicsDevice Device { get; private set; }
        static ContentManager _contentManager;
        static Dictionary<string, string> _configLookup;
        public static Matrix ProjectionMatrix;
        public static ScreenSize ScreenSize;
        static OpenCLScriptLoader _openCLScriptLoader;

        public static void Initialize(ContentManager content, GraphicsDevice device){
            Device = device;
            _contentManager = content;
            _openCLScriptLoader = new OpenCLScriptLoader();

            _configLookup = new Dictionary<string, string>();

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
            foreach (string directory in directories){
                string[] files = Directory.GetFiles(directory);
                foreach (string file in files){
                    var sr = new StreamReader(file);
                    var newConfigVals = JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd());
                    sr.Close();

                    string prefix = newConfigVals["InternalAbbreviation"] + "_";
                    newConfigVals.Remove("InternalAbbreviation");

                    foreach (var configVal in newConfigVals){
                        try{
                            _configLookup.Add(prefix + configVal.Key, configVal.Value);
                        }
                        catch (ArgumentException e){
                            DebugConsole.WriteLine("Error: a configuration value of the same identifier has already been added: " + prefix + configVal.Key);
                        }
                    }
                }

            }
        }

        /// <summary>
        /// Loads content straight from XNA's contentManager.
        /// </summary>
        /// <typeparam name="T">The type of the content to be returned.</typeparam>
        /// <param name="identifier">The name of the content to be loaded. Be sure to include subfolders, if relevant./</param>
        /// <returns></returns>
        public static T LoadContent<T>(string identifier){
            return _contentManager.Load<T>(identifier);
        }

        /// <summary>
        /// Load a config value.
        /// </summary>
        /// <typeparam name="T">The type of the configuration value.</typeparam>
        /// <param name="identifier">The config value's identifier, including the specified InternalAbbreviation.</param>
        /// <returns></returns>
        public static T LoadConfig<T>(string identifier){
            var configValue = _configLookup[identifier];
            T obj;
            try{
                //First try to deserialize the object with the standard Json deserializers
                obj = JsonConvert.DeserializeObject<T>(configValue);
            }
            catch{
                //Vectors can't be deserialized by json, so deserialize it separately.
                obj = VectorParser.Parse<T>(configValue);
            }
            return obj;
        }

        /// <summary>
        /// Loads the specified shader into the effect parameter.
        /// </summary>
        /// <param name="shaderName">The shader's alias, as specified by the InternalAbbreviation of the shader's config file.</param>
        /// <param name="effect">The loaded effect. New effects are cloned to prevent dupe issues.</param>
        public static void LoadShader(string shaderName, out Effect effect){
            effect = null;
            var configs = new List<string>();
            var configValues = new List<string>();
            foreach (var valuePair in _configLookup){
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
                    effect = _contentManager.Load<Effect>(configValues[i]).Clone();
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
                    var texture = _contentManager.Load<Texture2D>(configVal);
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

        #region opencl
        /// <summary>
        /// Returns the program-wide OpenCL context.
        /// </summary>
        public static ComputeContext CLContext{
            get { return _openCLScriptLoader.ComputeContext; }
        }

        /// <summary>
        /// Returns the program-wide OpenCL command queue.
        /// </summary>
        public static ComputeCommandQueue CLQueue{
            get { return _openCLScriptLoader.CommandQueue; }
        }

        /// <summary>
        /// Load an OpenCL script.
        /// </summary>
        /// <param name="scriptName">The filename of the script, relative to the binary's directory. Example Path: \\Scripts\\Quadtree.cl</param>
        /// <returns>Compiled ComputeProgram of the script.</returns>
        public static ComputeProgram LoadCLScript(string scriptName){
            return _openCLScriptLoader.LoadOpenclScript(scriptName);
        }
        #endregion

        /*
        public static string GetScriptDirectory(string identifier){
            string curDir = Directory.GetCurrentDirectory();
            string address = ConfigLookup[identifier];
            string directory = "";
            for (int i = address.Length - 1; i >= 0; i--){
                if (address[i] == '\\'){
                    directory = address.Substring(0, i);
                }
            }
            return curDir + "\\" + directory;
        }
         */
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