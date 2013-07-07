#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Cloo;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;
using Newtonsoft.Json.Linq;

#endregion

namespace Forge.Framework.Resources{
    public static class Resource{
        static ContentManager _contentManager;
        public static Matrix ProjectionMatrix;
        public static ScreenSize ScreenSize;
        static OpenCLScriptLoader _openCLScriptLoader;
        static ConfigLoader _configLoader;
        static bool _disposed;
        public static GraphicsDevice Device { get; private set; }

        public static void Initialize(ContentManager content, GraphicsDevice device){
            Device = device;
            _contentManager = content;
            _openCLScriptLoader = new OpenCLScriptLoader();
            _configLoader = new ConfigLoader();
        }

        /// <summary>
        ///   Loads content straight from XNA's contentManager.
        /// </summary>
        /// <typeparam name="T"> The type of the content to be returned. </typeparam>
        /// <param name="identifier"> The name of the content to be loaded. Be sure to include subfolders, if relevant./ </param>
        /// <returns> </returns>
        public static T LoadContent<T>(string identifier){
            T ret;
            lock (Device){
                ret = _contentManager.Load<T>(identifier);
            }
            return ret;
        }

        /// <summary>
        /// Load a configuration file cached in memory.
        /// </summary>
        /// <param name="fileAddress"></param>
        /// <returns></returns>
        public static JObject LoadConfig(string fileAddress){
            return _configLoader.LoadConfig(fileAddress);
        }

        public static T DeserializeField<T>(JToken token){
            T obj;
            try{
                //First try to deserialize the object with the standard Json deserializers
                obj = token.ToObject<T>();
            }
            catch{
                //Vectors can't be deserialized by json, so deserialize it separately.
                obj = VectorParser.Parse<T>(token.ToObject<string>());
            }
            return obj;
        }

        /// <summary>
        /// Loads a JSON object from a json settings file at the location specified with fileAddress, relative to bin dir.
        /// </summary>
        /// <param name="fileAddress"></param>
        /// <returns></returns>
        public static JObject LoadJObject(string fileAddress){
            var strmrdr = new StreamReader(fileAddress);
            var contents = strmrdr.ReadToEnd();
            strmrdr.Close();
            contents = FormatJsonString(contents);

            var jobj = JObject.Parse(contents);
            return jobj;
        }

        /// <summary>
        /// Loads a JSON object from a json settings file from the stream provided. Does not close reader after reading.
        /// </summary>
        /// <param name="reader"> </param>
        /// <returns></returns>
        public static JObject LoadJObject(StreamReader reader){
            var contents = reader.ReadToEnd();
            contents = FormatJsonString(contents);

            var jobj = JObject.Parse(contents);
            return jobj;
        }

        /// <summary>
        /// Reformats input json data to match with a format that's readable by json.net.
        /// For now, method filters out comments.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        static string FormatJsonString(string str){
            int openComment;
            while ((openComment = str.IndexOf("///", StringComparison.InvariantCulture)) != -1){
                int terminator = str.IndexOf('\r', openComment);
                if (terminator != -1){
                    str = str.Substring(0, openComment) + str.Substring(terminator);
                }
                else{
                    str = str.Substring(0, openComment);
                }
            }

            return str;
        }

        /// <summary>
        ///   Loads the specified shader into the effect parameter.
        /// </summary>
        /// <param name="configLocation"> The location of the shader's configuration file. </param>
        /// <param name="effect"> The loaded effect. New effects are cloned to prevent dupe issues. </param>
        public static void LoadShader(string configLocation, out Effect effect){
            effect = null;

            var configs = _configLoader.LoadConfig(configLocation);

            foreach (var config in configs){
                var key = config.Key;
                var val = config.Value.ToObject<string>();
                if (key.Equals("Shader")){
                    lock (Device){
                        effect = _contentManager.Load<Effect>(val).Clone();
                    }
                    if (effect == null){
                        throw new Exception("Shader not found");
                    }
                    break;
                }
            }

            if (effect == null){
                throw new Exception("Shader not specified for " + configLocation);
            }

            //figure out its datatype
            foreach (var config in configs){
                string name = config.Key;
                string configVal = config.Value.ToObject<string>();

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
                    Texture2D texture;
                    lock (Device){
                        texture = _contentManager.Load<Texture2D>(configVal);
                    }
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


        public static void Dispose(){
            Debug.Assert(!_disposed);
            _openCLScriptLoader.Dispose();
            _contentManager.Dispose();
            _disposed = true;
        }

        #region opencl

        /// <summary>
        ///   Returns the program-wide OpenCL context.
        /// </summary>
        public static ComputeContext CLContext{
            get { return _openCLScriptLoader.ComputeContext; }
        }

        /// <summary>
        ///   Returns the program-wide OpenCL command queue.
        /// </summary>
        public static ComputeCommandQueue CLQueue{
            get { return _openCLScriptLoader.CommandQueue; }
        }

        /// <summary>
        ///   Load an OpenCL script.
        /// </summary>
        /// <param name="scriptName"> The filename of the script, relative to the binary's directory. Example Path: /Scripts/Quadtree.cl </param>
        /// <returns> Compiled ComputeProgram of the script. </returns>
        public static ComputeProgram LoadCLScript(string scriptName){
            return _openCLScriptLoader.LoadOpenclScript(scriptName);
        }

        #endregion
    }

    public struct ScreenSize{
        public int X;
        public int Y;

        public ScreenSize(int x, int y){
            X = x;
            Y = y;
        }

        public void GetScreenValue(float percentX, float percentY, out int x, out int y){
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