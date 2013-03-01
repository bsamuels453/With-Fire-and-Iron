#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Gondola.Draw;
using Gondola.GameState.ObjectEditor;
using Gondola.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Gondola.GameState.Airship{
    internal static class AirshipPackager{
        const int _version = 0;

        public static void Export(string fileName, HullDataManager hullData){
            JObject jObj = new JObject();
            jObj["Version"] = _version;
            jObj["NumDecks"] = hullData.NumDecks;

            var hullInds = new int[hullData.NumDecks][];
            var hullVerts = new VertexPositionNormalTexture[hullData.NumDecks][];

            for (int i = 0; i < hullData.NumDecks; i++){
                hullInds[i] = hullData.HullBuffers[i].DumpIndicies();
                hullVerts[i] = hullData.HullBuffers[i].DumpVerticies();
            }

            var center = CalculateCenter(hullVerts);
            jObj["Centroid"] = JToken.FromObject(center);

            jObj["HullVerticies"] = JToken.FromObject(hullVerts);
            jObj["HullIndicies"] = JToken.FromObject(hullInds);


            var deckPlateInds = new List<int>[hullData.NumDecks];
            var deckPlateVerts = new List<VertexPositionNormalTexture>[hullData.NumDecks];

            for (int i = 0; i < hullData.NumDecks; i++){
                ConcatDeckPlates(
                    hullData.DeckBuffers[i].DumpObjectData(),
                    0.5f,
                    out deckPlateInds[i],
                    out deckPlateVerts[i]
                    );
            }

            jObj["DeckVerticies"] = JToken.FromObject(deckPlateVerts);
            jObj["DeckIndicies"] = JToken.FromObject(deckPlateInds);

            var sw = new StreamWriter(Directory.GetCurrentDirectory() + "\\Data\\" + fileName);
            sw.Write(JsonConvert.SerializeObject(jObj, Formatting.Indented));
            sw.Close();
        }

        static Vector3 CalculateCenter(VertexPositionNormalTexture[][] airshipVertexes){
            var ret = new Vector3(0, 0, 0);
            int numVerts = 0;
            foreach (var layer in airshipVertexes){
                numVerts += layer.Length;
                foreach (var vert in layer){
                    ret += vert.Position;
                }
            }
            ret /= numVerts;
            return ret;
        }

        static void ConcatDeckPlates(
            ObjectBuffer<ObjectIdentifier>.ObjectData[] objectData,
            float deckPlateWidth,
            out List<int> indicies,
            out List<VertexPositionNormalTexture> verticies){
            //this identifies deck boards that aren't part of the main mesh
            var nullIdentifier = new ObjectIdentifier(ObjectType.Misc, Vector3.Zero);

            //get extrema
            float minX = float.MaxValue, minZ = float.MaxValue, maxX = 0, maxZ = 0;
            foreach (var data in objectData){
                if (data.Identifier.Equals(nullIdentifier))
                    continue;

                foreach (var point in data.Verticies){
                    if (point.Position.X > maxX)
                        maxX = point.Position.X;
                    if (point.Position.Z > maxZ)
                        maxZ = point.Position.Z;
                    if (point.Position.X < minX)
                        minX = point.Position.X;
                    if (point.Position.Z < minZ)
                        minZ = point.Position.Z;
                }
            }
            float y = objectData[0].Verticies[0].Position.Y;
            float mult = 1/deckPlateWidth;

            Func<float, int> toArrX = f => (int) ((f - minX)*mult);
            Func<float, int> toArrZ = f => (int) ((f - minZ)*mult);

            Func<int, float> fromArrX = f => (float) f/mult + minX;
            Func<int, float> fromArrZ = f => (float) f/mult + minZ;

            var vertArr = new bool[toArrX(maxX) + 1,toArrZ(maxZ) + 1];
            var disabledVerts = new List<Tuple<int, int>>();
            //generate reference array
            foreach (var data in objectData){
                if (data.Identifier.Equals(nullIdentifier))
                    continue;

                if (data.Enabled){
                    foreach (var vertex in data.Verticies){
                        int x = toArrX(vertex.Position.X);
                        int z = toArrZ(vertex.Position.Z);
                        vertArr[x, z] = true;
                    }
                }
                else{
                    foreach (var vertex in data.Verticies){
                        disabledVerts.Add(new Tuple<int, int>(
                                              toArrX(vertex.Position.X),
                                              toArrZ(vertex.Position.Z)
                                              ));
                    }
                }
            }

            //generate strips of deck based on reference array
            var listInds = new List<int>();
            var listVerts = new List<VertexPositionNormalTexture>();
            int numPlates = 0;
            var idxWinding = new[]{0, 2, 1, 0, 3, 2};
            for (int xIdx = 0; xIdx < vertArr.GetLength(0) - 1; xIdx++){
                int zIdx = 0;

                while (true){
                    while (!vertArr[xIdx, zIdx] || !vertArr[xIdx + 1, zIdx])
                        zIdx++;

                    int initlZ = zIdx;

                    while ((vertArr[xIdx, zIdx] && vertArr[xIdx + 1, zIdx])){
                        zIdx++;
                        if (zIdx + 1 > vertArr.GetLength(1))
                            break;
                    }

                    Func<int, int, int, int, bool> addVertex = (x, z, texU, texV) => {
                        listVerts.Add(new VertexPositionNormalTexture(
                                          new Vector3(
                                              fromArrX(x),
                                              y,
                                              fromArrZ(z)
                                              ),
                                          Vector3.Up,
                                          new Vector2(texU, texV)
                                          )
                            );
                        return true;
                    };

                    zIdx--;
                    addVertex(xIdx, initlZ, 0, 0);
                    addVertex(xIdx, zIdx, 1, 0);
                    addVertex(xIdx + 1, zIdx, 1, 1);
                    addVertex(xIdx + 1, initlZ, 0, 1);
                    int offset = numPlates*4;

                    var winding = (int[]) idxWinding.Clone();
                    for (int i = 0; i < 6; i++){
                        winding[i] += offset;
                    }
                    listInds.AddRange(winding);
                    numPlates++;

                    //xxxx untested
                    if (!disabledVerts.Contains(new Tuple<int, int>(xIdx, zIdx))){
                        break;
                    }
                }
            }

            //now add the plates that aren't part of the main mesh
            var otherPlates = new List<ObjectBuffer<ObjectIdentifier>.ObjectData>();
            foreach (var data in objectData){
                if (data.Identifier.Equals(nullIdentifier))
                    otherPlates.Add(data);
            }
            foreach (var data in otherPlates){
                int lowest = data.Indicies[0];
                int offset = numPlates*4;

                for (int i = 0; i < 6; i++){
                    data.Indicies[i] -= lowest;
                    data.Indicies[i] += offset;
                }

                listInds.AddRange(data.Indicies);
                listVerts.AddRange(data.Verticies);
                numPlates++;
            }
            /*
            var bmp = new Bitmap(vertArr.GetLength(0), vertArr.GetLength(1));
            for (int x = 0; x < vertArr.GetLength(0); x++){
                for (int z = 0; z < vertArr.GetLength(1); z++){
                    if (vertArr[x, z]){
                        bmp.SetPixel(x, z, Color.Red);
                    }
                }
            }
            bmp.Save("hello.png");
            */

            indicies = listInds;
            verticies = listVerts;
        }

        public static AirshipModel Import(string fileName){
            var sw = new Stopwatch();
            sw.Start();
            var sr = new StreamReader(Directory.GetCurrentDirectory() + "\\Data\\" + fileName);
            var jObj = JObject.Parse(sr.ReadToEnd());
            sr.Close();

            var ret = new AirshipModel();
            int numDecks = jObj["NumDecks"].ToObject<int>();
            ret.Centroid = jObj["Centroid"].ToObject<Vector3>();

            var hullVerts = jObj["HullVerticies"].ToObject<VertexPositionNormalTexture[][]>();
            var hullInds = jObj["HullIndicies"].ToObject<int[][]>();

            var deckVerts = jObj["DeckVerticies"].ToObject<VertexPositionNormalTexture[][]>();
            var deckInds = jObj["DeckIndicies"].ToObject<int[][]>();

            ret.Decks = new GeometryBuffer<VertexPositionNormalTexture>[numDecks];
            ret.HullLayers = new GeometryBuffer<VertexPositionNormalTexture>[numDecks];

            //offset the airship
            foreach (var layer in hullVerts){
                for (int i = 0; i < layer.Length; i++){
                    layer[i].Position = new Vector3(0, 1000, 0) + layer[i].Position;
                }
            }
            foreach (var layer in deckVerts) {
                for (int i = 0; i < layer.Length; i++) {
                    layer[i].Position = new Vector3(0, 1000, 0) + layer[i].Position;
                }
            }

            for (int i = 0; i < numDecks; i++){
                ret.Decks[i] = new GeometryBuffer<VertexPositionNormalTexture>(deckInds[i].Length, deckVerts[i].Length, deckVerts[i].Length / 2, "Shader_AirshipDeck");
                ret.Decks[i].IndexBuffer.SetData(deckInds[i]);
                ret.Decks[i].VertexBuffer.SetData(deckVerts[i]);


                ret.HullLayers[i] = new GeometryBuffer<VertexPositionNormalTexture>(hullInds[i].Length, hullVerts[i].Length, hullVerts[i].Length / 2, "Shader_AirshipHull");
                ret.HullLayers[i].IndexBuffer.SetData(hullInds[i]);
                ret.HullLayers[i].VertexBuffer.SetData(hullVerts[i]);
            }

            sw.Stop();
            double d = sw.ElapsedMilliseconds;
            return ret;
        }
    }

    internal class AirshipModel{
        public Vector3 Centroid;
        public GeometryBuffer<VertexPositionNormalTexture>[] Decks;
        public GeometryBuffer<VertexPositionNormalTexture>[] HullLayers;
    }
}