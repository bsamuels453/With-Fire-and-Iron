using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Gondola.Draw;
using Gondola.Logic;
using Gondola.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Gondola.GameState.TerrainManager {
    class TerrainManager : IGameState {
        readonly List<TerrainChunk> _loadedChunks;
        readonly TerrainGen _generator;
        readonly RenderTarget _renderTarget;

        public TerrainManager(){
            _renderTarget = new RenderTarget(0.0f);
            _renderTarget.Bind();
            _loadedChunks = new List<TerrainChunk>();
            /*
            _generator = new TerrainGen();
            for (int x = 0; x < 3; x++){
                for (int z = 0; z < 3; z++){
                    var chunk = _generator.GenerateChunk(new XZPair(x, z));
                    _loadedChunks.Add(chunk);

                    var sw = new StreamWriter("chunk"+x+" "+z);
                    var jobj = new JObject();
                    jobj["Norms"] = JToken.FromObject(chunk.Normals);
                    jobj["Binorms"] = JToken.FromObject(chunk.Binormals);
                    jobj["Tangs"] = JToken.FromObject(chunk.Tangents);
                    jobj["Verts"] = JToken.FromObject(chunk._verticies);
                    jobj["Inds"] = JToken.FromObject(chunk._indicies);

                    sw.Write(JsonConvert.SerializeObject(jobj, Formatting.Indented));
                    sw.Close();
                }
            }
             */

            for (int x = 0; x < 3; x++){
                for (int z = 0; z < 3; z++){
                    var sr = new StreamReader("chunk" + x + " " + z);
                    var jObj = JsonConvert.DeserializeObject<JObject>(sr.ReadToEnd());
                    var norms = jObj["Norms"].ToObject<ushort[]>();
                    var bins = jObj["Binorms"].ToObject<byte[]>();
                    var tangs = jObj["Tangs"].ToObject<byte[]>();
                    var indxs = jObj["Inds"].ToObject<int[]>();
                    var verts = jObj["Verts"].ToObject<VertexPositionTexture[]>();

                    var normTex = new Texture2D(Gbl.Device, 129, 129, false, SurfaceFormat.Rgba64);
                    var binsTex = new Texture2D(Gbl.Device, 129, 129, false, SurfaceFormat.Color);
                    var tangsTex = new Texture2D(Gbl.Device, 129, 129, false, SurfaceFormat.Color);

                    normTex.SetData(norms);
                    binsTex.SetData(bins);
                    tangsTex.SetData(tangs);

                    var chunk = new TerrainChunk(new XZPair(x, z), verts, indxs, normTex, binsTex, tangsTex);
                    _loadedChunks.Add(chunk);
                }
            }
            _renderTarget.Unbind();
        }

        public void Dispose(){
            foreach (var chunk in _loadedChunks){
                chunk.Dispose();
            }
            _loadedChunks.Clear();
        }

        public void Update(InputState state, double timeDelta){
            _renderTarget.Bind();
            var playerPos = (Vector3)GamestateManager.QuerySharedData(SharedStateData.PlayerPosition);
            _renderTarget.Unbind();
        }

        public void Draw(){
            var playerPos = (Vector3)GamestateManager.QuerySharedData(SharedStateData.PlayerPosition);
            var playerLook = (Angle3)GamestateManager.QuerySharedData(SharedStateData.PlayerLook);
            var matrix = RenderHelper.CalculateViewMatrix(playerPos, playerLook);
            _renderTarget.Draw(matrix, Color.Transparent);
        }
    }
}
