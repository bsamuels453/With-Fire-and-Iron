#region

using System;
using System.Diagnostics;
using System.IO;
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
        static HLSLShaderLoader _shaderLoader;
        public static GameObjectLoader GameObjectLoader { get; private set; }
        static bool _disposed;
        public static GraphicsDevice Device { get; private set; }

        public static void Initialize(ContentManager content, GraphicsDevice device){
            Device = device;
            _contentManager = content;
            _openCLScriptLoader = new OpenCLScriptLoader();
            _configLoader = new ConfigLoader();
            _shaderLoader = new HLSLShaderLoader(_contentManager, _configLoader);
            GameObjectLoader = new GameObjectLoader();
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
            _shaderLoader.LoadShader(configLocation, out effect);
        }

        public static void Dispose(){
            Debug.Assert(!_disposed);
            _openCLScriptLoader.Dispose();
            _contentManager.Dispose();
            GameObjectLoader.Dispose();
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