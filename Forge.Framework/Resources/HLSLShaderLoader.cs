#region

using System;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;
using Newtonsoft.Json.Linq;

#endregion

namespace Forge.Framework.Resources{
    internal class HLSLShaderLoader : ResourceLoader{
        readonly ConfigLoader _configLoader;
        readonly ContentManager _contentManager;

        public HLSLShaderLoader(ContentManager contentManager, ConfigLoader configLoader){
            _contentManager = contentManager;
            _configLoader = configLoader;
        }

        public void LoadShader(string configLocation, out Effect effect, out DepthStencilState depthStencil, out RasterizerState rasterizer){
            effect = null;
            var configs = _configLoader.LoadConfig(configLocation);

            foreach (var config in configs){
                var key = config.Key;
                var val = config.Value.ToObject<string>();
                if (key.Equals("Shader")){
                    lock (Resource.Device){
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

                if (name == "Rasterizer" || name == "DepthStencil")
                    continue;

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
                    lock (Resource.Device){
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
            depthStencil = ExtractDepthStencil(configs);
            rasterizer = ExtractRasterizer(configs);
        }

        DepthStencilState ExtractDepthStencil(JObject shaderConfigs){
            JToken token;
            shaderConfigs.TryGetValue("DepthStencil", out token);
            if (token == null){
                return new DepthStencilState();
            }
            return token.ToObject<DepthStencilState>();
        }

        RasterizerState ExtractRasterizer(JObject shaderConfigs){
            JToken token;
            shaderConfigs.TryGetValue("Rasterizer", out token);
            if (token == null){
                return new RasterizerState();
            }
            return token.ToObject<RasterizerState>();
        }

        public override void Dispose(){
        }
    }
}