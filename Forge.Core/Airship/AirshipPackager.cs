#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Forge.Core.ObjectEditor;
using Forge.Core.Util;
using Forge.Framework.Draw;
using Forge.Core.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Forge.Core.Airship{
    internal static class AirshipPackager{
        const int _version = 0;

        public static void Export(string fileName, BezierInfo[] backCurveInfo, BezierInfo[] sideCurveInfo, BezierInfo[] topCurveInfo) {
            JObject jObj = new JObject();
            jObj["Version"] = _version;
            jObj["FrontBezierSurf"] = JToken.FromObject(backCurveInfo);
            jObj["SideBezierSurf"] = JToken.FromObject(sideCurveInfo);
            jObj["TopBezierSurf"] = JToken.FromObject(topCurveInfo);
            /*
            jObj["NumDecks"] = hullData.NumDecks;

            var hullInds = new int[hullData.NumDecks][];
            var hullVerts = new VertexPositionNormalTexture[hullData.NumDecks][];

            for (int i = 0; i < hullData.NumDecks; i++){
                //fixme
                //hullInds[i] = hullData.HullBuffers[i].DumpIndicies();
                //hullVerts[i] = hullData.HullBuffers[i].DumpVerticies();
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

             */
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

        public static Airship Import(string fileName){
            var sw = new Stopwatch();
            sw.Start();
            var sr = new StreamReader(Directory.GetCurrentDirectory() + "\\Data\\" + fileName);
            var jObj = JObject.Parse(sr.ReadToEnd());
            sr.Close();

            var backInfo = jObj["FrontBezierSurf"].ToObject<List<BezierInfo>>();
            var sideInfo = jObj["SideBezierSurf"].ToObject<List<BezierInfo>>();
            var topInfo = jObj["TopBezierSurf"].ToObject<List<BezierInfo>>();

            var hullData = HullGeometryGenerator.GenerateShip(
                backInfo,
                sideInfo,
                topInfo
                );

            var modelAttribs = new ModelAttributes();
            //in the future these attributes will be defined based off analyzing the hull
            modelAttribs.Length = 50;
            modelAttribs.MaxAscentSpeed = 10;
            modelAttribs.MaxForwardSpeed = 30;
            modelAttribs.MaxReverseSpeed = 10;
            modelAttribs.MaxTurnSpeed = 4f;
            modelAttribs.Berth = 13.95f;
            modelAttribs.NumDecks = hullData.NumDecks;
            modelAttribs.Centroid = new Vector3(modelAttribs.Length / 3, 0, 0);

            /*
            int numDecks = jObj["NumDecks"].ToObject<int>();
            modelAttribs.NumDecks = numDecks;
            modelAttribs.Centroid = jObj["Centroid"].ToObject<Vector3>();

            var hullVerts = jObj["HullVerticies"].ToObject<VertexPositionNormalTexture[][]>();
            var hullInds = jObj["HullIndicies"].ToObject<int[][]>();

            var deckVerts = jObj["DeckVerticies"].ToObject<VertexPositionNormalTexture[][]>();
            var deckInds = jObj["DeckIndicies"].ToObject<int[][]>();

            var deckBuffs = new GeometryBuffer<VertexPositionNormalTexture>[numDecks];
            var hullBuffs = new GeometryBuffer<VertexPositionNormalTexture>[numDecks];

            //reflect vertexes to fix orientation
            //xxx THIS BREAKS THE NORMALS
            var reflection = new Vector3(1, 0, 0);
            for (int i = 0; i < numDecks; i++){
                for (int vert = 0; vert < deckVerts[i].Length; vert++){
                    deckVerts[i][vert].Position = Vector3.Reflect(deckVerts[i][vert].Position, reflection);
                }
                for (int vert = 0; vert < hullVerts[i].Length; vert++) {
                    hullVerts[i][vert].Position = Vector3.Reflect(hullVerts[i][vert].Position, reflection);
                }
            }


            for (int i = 0; i < numDecks; i++){
                deckBuffs[i] = new GeometryBuffer<VertexPositionNormalTexture>(deckInds[i].Length, deckVerts[i].Length, deckVerts[i].Length / 2, "Shader_AirshipDeck");
                deckBuffs[i].IndexBuffer.SetData(deckInds[i]);
                deckBuffs[i].VertexBuffer.SetData(deckVerts[i]);


                hullBuffs[i] = new GeometryBuffer<VertexPositionNormalTexture>(hullInds[i].Length, hullVerts[i].Length, hullVerts[i].Length / 2, "Shader_AirshipHull");
                hullBuffs[i].IndexBuffer.SetData(hullInds[i]);
                hullBuffs[i].VertexBuffer.SetData(hullVerts[i]);
            }
            */

            foreach (var buffer in hullData.HullMeshes) {
                buffer.ApplyTransform((vert) => {
                    vert.Position.X *= -1;
                    return vert;
                }
                );
            }

            foreach (var buffer in hullData.DeckFloorBuffers) {
                buffer.ApplyTransform((vert) => {
                    vert.Position.X *= -1;
                    return vert;
                }
                );
            }





            var ret = new Airship(modelAttribs, hullData.DeckFloorBuffers, hullData.HullMeshes);
            sw.Stop();
            double d = sw.ElapsedMilliseconds;

            return ret;
        }
    }


}