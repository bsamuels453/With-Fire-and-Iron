#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forge.Core.Util;
using Forge.Framework;

#endregion

namespace Forge.Core.Terrain{
    public class TerrainUpdater : IDisposable{
        readonly TerrainGen _generator;
        readonly List<TerrainChunk> _loadedChunks;
        List<Task> _generationTasks;

        public TerrainUpdater(){
            _loadedChunks = new List<TerrainChunk>();
            _generationTasks = new List<Task>(400);

            _generator = new TerrainGen();
            for (int x = 0; x < 10; x++){
                for (int z = 0; z < 10; z++){
                    int x1 = x;
                    int z1 = z;
                    _generationTasks.Add
                        (new Task
                            (() =>{
                                 TerrainChunk ret;
                                 lock (_generator){
                                     //NESTED LOCK WARNING: child locks Resource.Device
                                     ret = _generator.GenerateChunk(new XZPair(x1, z1));
                                 }
                                 lock (_loadedChunks){
                                     _loadedChunks.Add(ret);
                                 }
                             }
                            ));
                    //var chunk = _generator.GenerateChunk(new XZPair(x, z));
                    //_loadedChunks.Add(chunk);

                    //var sw = new StreamWriter("chunk"+x+" "+z);
                    //var jobj = new JObject();
                    //jobj["Norms"] = JToken.FromObject(chunk.Normals);
                    //jobj["Binorms"] = JToken.FromObject(chunk.Binormals);
                    //jobj["Tangs"] = JToken.FromObject(chunk.Tangents);
                    //jobj["Verts"] = JToken.FromObject(chunk._verticies);
                    //jobj["Inds"] = JToken.FromObject(chunk._indicies);

                    //sw.Write(JsonConvert.SerializeObject(jobj, Formatting.Indented));
                    //sw.Close();
                }
            }

            foreach (var generationTask in _generationTasks){
                generationTask.Start();
            }

            /*
            for (int x = 0; x < 2; x++){
                for (int z = 0; z < 2; z++){
                    var sr = new StreamReader("chunk" + x + " " + z);
                    var jObj = JsonConvert.DeserializeObject<JObject>(sr.ReadToEnd());
                    var norms = jObj["Norms"].ToObject<ushort[]>();
                    var bins = jObj["Binorms"].ToObject<byte[]>();
                    var tangs = jObj["Tangs"].ToObject<byte[]>();
                    var indxs = jObj["Inds"].ToObject<int[]>();
                    var verts = jObj["Verts"].ToObject<VertexPositionTexture[]>();

                    var normTex = new Texture2D(Resource.Device, 129, 129, false, SurfaceFormat.Rgba64);
                    var binsTex = new Texture2D(Resource.Device, 129, 129, false, SurfaceFormat.Color);
                    var tangsTex = new Texture2D(Resource.Device, 129, 129, false, SurfaceFormat.Color);

                    normTex.SetData(norms);
                    binsTex.SetData(bins);
                    tangsTex.SetData(tangs);

                    var chunk = new TerrainChunk(new XZPair(x, z), verts, indxs, normTex, binsTex, tangsTex);
                    _loadedChunks.Add(chunk);
                }
            }*/
        }

        #region IDisposable Members

        public void Dispose(){
            foreach (var chunk in _loadedChunks){
                chunk.Dispose();
            }
            _loadedChunks.Clear();
            _generator.Dispose();
        }

        #endregion

        public void Update(InputState state, double timeDelta){
            lock (_generationTasks){
                _generationTasks = (from task in _generationTasks
                    where task.Status != TaskStatus.RanToCompletion
                    select task).ToList();
            }
        }
    }
}