#define OUTPUT_GENERATION_TIMINGS

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Forge.Core.Airship;
using Forge.Framework;
using Forge.Framework.Draw;
using Forge.Core.Logic;
using Forge.Core.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Forge.Core.ObjectEditor {
    /// <summary>
    /// Generates the geometry for airship hulls. This differs from PreviewRenderer in
    /// that this class generates the geometry so that things like windows, portholes, or
    /// other extremities can be added easily without modifying/removing much of the geometry.
    /// In more mathematical terms, it means that the horizontal boundaries between adjacent
    /// quads are parallel to the XZ plane. This class can also take a few seconds to do its
    /// thing because it isnt going to be updating every tick like previewrenderer does.
    /// </summary>
    internal static class HullGeometryGenerator{
        const int _primHeightPerDeck = 5;
        const int _horizontalPrimDivisor = 2;

        //note: less than 1 deck breaks prolly
        //note that this entire geometry generator runs on the standard curve assumptions
        public static HullGeometryInfo GenerateShip(List<BezierInfo> backCurveInfo, List<BezierInfo> sideCurveInfo, List<BezierInfo> topCurveInfo){
#if OUTPUT_GENERATION_TIMINGS
            var sw = new Stopwatch();
            sw.Start();
#endif
            const float deckHeight = 2.13f;
            const float bBoxWidth = 0.5f;
            var genResults = GenerateHull(new GenerateHullParams{
                BackCurveInfo = backCurveInfo,
                SideCurveInfo = sideCurveInfo,
                TopCurveInfo = topCurveInfo,
                DeckHeight = deckHeight,
                BoundingBoxWidth = bBoxWidth,
                PrimitivesPerDeck = _primHeightPerDeck
            }
                );
            var normalGenResults = GenerateHullNormals(genResults.LayerSilhouetteVerts);
            var deckFloorPlates = GenerateDeckPlates(genResults.LayerSilhouetteVerts, genResults.NumDecks, _primHeightPerDeck);
            var boundingBoxResults = GenerateDeckBoundingBoxes(bBoxWidth, deckFloorPlates);
            var hullBuffers = GenerateDeckWallBuffers(
                genResults.DeckSilhouetteVerts, 
                normalGenResults.NormalMesh, 
                genResults.NumDecks, 
                _primHeightPerDeck,
                boundingBoxResults.BoxMin,
                boundingBoxResults.BoxMax
            );
            var deckFloorBuffers = GenerateDeckFloorMesh(genResults.DeckSilhouetteVerts, boundingBoxResults.DeckBoundingBoxes, genResults.NumDecks);

            var resultant = new HullGeometryInfo();
            resultant.CenterPoint = normalGenResults.Centroid;
            resultant.DeckFloorBoundingBoxes = boundingBoxResults.DeckBoundingBoxes;
            resultant.DeckFloorBuffers = deckFloorBuffers;
            resultant.FloorVertexes = boundingBoxResults.DeckVertexes;
            //resultant.HullWallTexBuffers = hullBuffers;
            resultant.HullMeshes = hullBuffers;
            resultant.NumDecks = genResults.NumDecks;
            resultant.WallResolution = bBoxWidth;
            resultant.DeckHeight = deckHeight;
            resultant.MaxBoundingBoxDims = new Vector2((int) (genResults.Length/bBoxWidth), (int) (genResults.Berth/bBoxWidth));

#if OUTPUT_GENERATION_TIMINGS
            double d = sw.ElapsedMilliseconds;
            DebugConsole.WriteLine("Airship generated in " + d + " ms");
#endif
            return resultant;
        }

        //todo: break up this method into submethods for the sake of cleanliness.
        static GenerateHullResults GenerateHull(GenerateHullParams input){
            var sideCurveInfo = input.SideCurveInfo;
            var backCurveInfo = input.BackCurveInfo;
            var topCurveInfo = input.TopCurveInfo;
            float deckHeight = input.DeckHeight;
            float boundingBoxWidth = input.BoundingBoxWidth;
            int primitivesPerDeck = input.PrimitivesPerDeck;
            var results = new GenerateHullResults();

            float metersPerPrimitive = deckHeight/primitivesPerDeck;
            var sidePtGen = new BruteBezierGenerator(sideCurveInfo);

            topCurveInfo.RemoveAt(0); //make this curve set pass vertical line test
            backCurveInfo.RemoveAt(0); //make this curve pass the horizontal line test
            var topPtGen = new BruteBezierGenerator(topCurveInfo);
            var geometryYvalues = new List<float>(); //this list contains all the valid Y values for the airship's primitives
            var xzHullIntercepts = new List<List<Vector2>>();

            //get the draft and the berth
            float draft = sideCurveInfo[1].Pos.Y;
            results.Berth = topCurveInfo[1].Pos.Y;
            results.Length = sideCurveInfo[2].Pos.X;
            results.NumDecks = (int)(draft / deckHeight) + 1;
            int numVerticalVertexes = (results.NumDecks-1)*primitivesPerDeck + primitivesPerDeck + 1;

            //get the y values for the hull
            for (int i = 0; i < numVerticalVertexes - primitivesPerDeck; i++){
                geometryYvalues.Add(i*metersPerPrimitive);
            }
            float bottomDeck = geometryYvalues[geometryYvalues.Count - 1];

            //the bottom part of ship (false deck) will not have height of _metersPerDeck so we need to use a different value for metersPerPrimitive
            float bottomPrimHeight = (draft - bottomDeck)/primitivesPerDeck;
            for (int i = 1; i <= primitivesPerDeck; i++){
                geometryYvalues.Add(i*bottomPrimHeight + bottomDeck);
            }

            foreach (float t in geometryYvalues){
                xzHullIntercepts.Add(sidePtGen.GetValuesFromDependent(t));
                if (xzHullIntercepts.Last().Count != 2){
                    if (xzHullIntercepts.Last().Count == 1) { //this happens at the very bottom of the ship
                        xzHullIntercepts.Last().Add(xzHullIntercepts[xzHullIntercepts.Count - 1][0]);
                    }
                    else
                        throw new Exception("more/less than two independent solutions found for a given dependent value");
                }
            }
            //xzHullIntecepts represents a list where each element is a pair of vertexes, the first one specifies the point
            //where the hull begins, and the second one represents the point at which the hull ends.
            //ironically enough, x and y of the vectors involve map correctly

            //this list contains slices of the airship which contain all the vertexes for the specific layer of the airship
            var ySliceVerts = new List<List<Vector3>>();

            int numHorizontalPrimitives = (int)(results.Length / boundingBoxWidth + 1) / _horizontalPrimDivisor;
            if (numHorizontalPrimitives%2 != 0){
                numHorizontalPrimitives++;
            }
            //in the future we can parameterize x differently to comphensate for dramatic curves on the keel
            for (int y = 0; y < numVerticalVertexes; y++){
                float xStart = xzHullIntercepts[y][0].X;
                float xEnd = xzHullIntercepts[y][1].X;
                float xDiff = xEnd - xStart;

                var strip = new List<Vector3>();

                for (int x = 0; x < numHorizontalPrimitives; x++){
                    var point = new Vector3();
                    point.Y = geometryYvalues[y];

                    //here is where x is parameterized, and converted into a relative x value
                    float tx = x/(float) (numHorizontalPrimitives - 1);
                    float xPos = tx*xDiff + xStart;
                    //

                    var keelIntersect = sidePtGen.GetValueFromIndependent(xPos);
                    float profileYScale = keelIntersect.Y/draft;
                    point.X = keelIntersect.X;

                    var topIntersect = topPtGen.GetValueFromIndependent(xPos);
                    float profileXScale = (topIntersect.Y - topCurveInfo[0].Pos.Y)/(results.Berth/2f);
                    //float profileXScale = topIntersect.Y  / berth;

                    var scaledProfile = new List<BezierInfo>();

                    foreach (BezierInfo t in backCurveInfo){
                        scaledProfile.Add(t.CreateScaledCopy(profileXScale, profileYScale));
                    }


                    var pointGen = new BruteBezierGenerator(scaledProfile);
                    var profileIntersect = pointGen.GetValuesFromDependent(point.Y);
                    if (profileIntersect.Count != 1){
                        throw new Exception("curve does not pass the horizontal line test");
                    }

                    float diff = scaledProfile[0].Pos.X;
                    if (x == numHorizontalPrimitives - 1 || x == 0){
                        diff = profileIntersect[0].X;
                    }
                    point.Z = profileIntersect[0].X - diff;

                    if (y == numVerticalVertexes - 1){
                        point.Z = 0;
                    }

                    strip.Add(point);
                }
                ySliceVerts.Add(strip);
            }

            foreach (List<Vector3> t in ySliceVerts){ //mystery NaN detecter
                if (float.IsNaN(t[0].Z)){
                    throw new Exception("NaN Z coordinate in mesh");
                }
            }
            /*foreach (List<Vector3> t in ySliceVerts){//remove mystery NaNs
                for(int i=0; i<t.Count; i++){
                    if (float.IsNaN(t[i].Z)){
                        t[i] = new Vector3(t[i].X, t[i].Y, 0);
                    }
                }
            }*/

            var geometry = ySliceVerts;

            //reflect+dupe the geometry across the x axis to complete the opposite side of the ship
            results.LayerSilhouetteVerts = new Vector3[geometry.Count][];
            for (int i = 0; i < geometry.Count; i++){
                results.LayerSilhouetteVerts[i] = new Vector3[geometry[0].Count*2];

                geometry[i].Reverse();
                int destIdx = 0;
                for (int si = 0; si < geometry[0].Count; si++){
                    results.LayerSilhouetteVerts[i][destIdx] = geometry[i][si];
                    destIdx++;
                }
                geometry[i].Reverse();
                for (int si = 0; si < geometry[0].Count; si++){
                    results.LayerSilhouetteVerts[i][destIdx] = new Vector3(geometry[i][si].X, geometry[i][si].Y, -geometry[i][si].Z);
                    destIdx++;
                }
            }

            //reflect the layers across the Y axis so that the opening is pointing up
            foreach (var layerVert in results.LayerSilhouetteVerts){
                for (int i = 0; i < layerVert.GetLength(0); i++){
                    layerVert[i] = new Vector3(layerVert[i].X, -layerVert[i].Y, layerVert[i].Z);
                }
            }

            //this fixes the ordering of the lists so that normals generate correctly
            for (int i = 0; i < results.LayerSilhouetteVerts.GetLength(0); i++){
                results.LayerSilhouetteVerts[i] = results.LayerSilhouetteVerts[i].Reverse().ToArray();
            }

            //now enumerate the ships layer verts into levels for each deck
            results.DeckSilhouetteVerts = new Vector3[results.NumDecks][][];
            for (int i = 0; i < results.NumDecks; i++){
                results.DeckSilhouetteVerts[i] = new Vector3[primitivesPerDeck + 1][];

                for (int level = 0; level < primitivesPerDeck + 1; level++){
                    results.DeckSilhouetteVerts[i][level] = results.LayerSilhouetteVerts[i*(primitivesPerDeck) + level];
                }
            }
            //edge case for the final deck, the "bottom"
            //results.DeckSilhouetteVerts[results.NumDecks] = new Vector3[primitivesPerDeck + 1][];
            //for (int level = 0; level < primitivesPerDeck + 1; level++){
            //    results.DeckSilhouetteVerts[results.NumDecks][level] = results.LayerSilhouetteVerts[results.NumDecks*(primitivesPerDeck) + level];
            //}
            return results;
        }

        static GenerateNormalsResults GenerateHullNormals(Vector3[][] layerSVerts){
            //generate a normals array for the entire ship, rather than per-deck
            var totalMesh = new Vector3[layerSVerts.Length,layerSVerts[0].Length];
            var retMesh = new Vector3[layerSVerts.Length,layerSVerts[0].Length];
            MeshHelper.Encode2DListIntoArray(layerSVerts.Length, layerSVerts[0].Length, ref totalMesh, layerSVerts);
            MeshHelper.GenerateMeshNormals(totalMesh, ref retMesh);

            //since we have generated totalmesh, might as well get the centerpoint now
            var centroid = GenerateCenterPoint(totalMesh);

            var ret = new GenerateNormalsResults();
            ret.NormalMesh = retMesh;
            ret.Centroid = centroid;

            return ret;
        }

        static Vector3[][,] GenerateDeckPlates(Vector3[][] layerSVerts, int numDecks, int primitivesPerDeck){
            var retMesh = new Vector3[4][,];
            int vertsInSilhouette = layerSVerts[0].Length;
            for (int deck = 0; deck < numDecks; deck++){
                retMesh[deck] = new Vector3[3,vertsInSilhouette/2];
                for (int vert = 0; vert < vertsInSilhouette/2; vert++){
                    retMesh[deck][0, vert] = layerSVerts[deck*primitivesPerDeck][vertsInSilhouette/2 + vert];

                    retMesh[deck][1, vert] = layerSVerts[deck*primitivesPerDeck][vertsInSilhouette/2 + vert];
                    retMesh[deck][2, vert] = layerSVerts[deck*primitivesPerDeck][vertsInSilhouette/2 - vert - 1];
                    retMesh[deck][1, vert].Z = 0;

                    retMesh[deck][2, vert] = layerSVerts[deck*primitivesPerDeck][vertsInSilhouette/2 - vert - 1];
                }
            }
            return retMesh;
        }

        static ObjectBuffer<ObjectIdentifier>[] GenerateDeckFloorMesh(Vector3[][][] deckSVerts, List<BoundingBox>[] deckBoundingBoxes, int numDecks) {
            float boundingBoxWidth = Math.Abs(deckBoundingBoxes[0][0].Max.X - deckBoundingBoxes[0][0].Min.X);
            var ret = new ObjectBuffer<ObjectIdentifier>[numDecks];

            for (int deck = 0; deck < numDecks; deck++){
                var deckBBoxes = deckBoundingBoxes[deck];
                Vector3[] deckSilhouette = deckSVerts[deck][0];
                float y = deckSilhouette[0].Y;
                var verts = new List<Vector3>();
                //first, an outline is generated of the bounding box grid
                var upperMaxima = new Dictionary<float, float>();
                var lowerMaxima = new Dictionary<float, float>(); //dontuse?
                {
                    for (float i = 0; i < deckSilhouette.Last().X; i += boundingBoxWidth){
                        upperMaxima.Add(i, 0);
                        lowerMaxima.Add(i, 0);
                    }
                    foreach (var box in deckBoundingBoxes[deck]){
                        if (Math.Abs(box.Max.Z) > upperMaxima[box.Max.X])
                            upperMaxima[box.Max.X] = Math.Abs(box.Max.Z);
                    }
                    foreach (var box in deckBoundingBoxes[deck]){
                        if (Math.Abs(box.Min.Z) > lowerMaxima[box.Min.X])
                            lowerMaxima[box.Min.X] = Math.Abs(box.Min.Z);
                    }
                }
                //now the border-filler quads are generated. They are assigned ID of null.
                //start at halfway point because of deckSilhouette reflection
                int prevVert = deckSilhouette.Length/2;
                int nextVert = prevVert + 1;
                float length = deckSilhouette[0].X;

                //get the starting position for x
                float x = boundingBoxWidth;
                while (x < deckSilhouette[prevVert].X)
                    x += boundingBoxWidth;

                for (; x < length; x += boundingBoxWidth){ //yolo

                    var interpolator = new Interpolate(
                        deckSilhouette[prevVert].Z,
                        deckSilhouette[nextVert].Z,
                        deckSilhouette[nextVert].X - deckSilhouette[prevVert].X
                        );

                    if (x > deckSilhouette[nextVert].X){
                        //split along boundary
                        float prevX = x - boundingBoxWidth;
                        float curX = deckSilhouette[nextVert].X;
                        float prevFracX = prevX - deckSilhouette[prevVert].X;
                        float fracX = deckSilhouette[nextVert].X - deckSilhouette[prevVert].X;
                        float z0 = interpolator.GetLinearValue(prevFracX);
                        float z1 = interpolator.GetLinearValue(fracX);
                        verts.Add(new Vector3(prevX, y, lowerMaxima[prevX]));
                        verts.Add(new Vector3(prevX, y, z0));
                        verts.Add(new Vector3(curX, y, z1));
                        verts.Add(new Vector3(curX, y, lowerMaxima[prevX]));
                        prevVert++;
                        nextVert++;
                        float ppx = prevX;
                        interpolator = new Interpolate(
                            deckSilhouette[prevVert].Z,
                            deckSilhouette[nextVert].Z,
                            deckSilhouette[nextVert].X - deckSilhouette[prevVert].X
                            );
                        prevX = curX;
                        prevFracX = 0;
                        fracX = x - deckSilhouette[prevVert].X;
                        z0 = interpolator.GetLinearValue(prevFracX);
                        z1 = interpolator.GetLinearValue(fracX);
                        verts.Add(new Vector3(prevX, y, lowerMaxima[ppx]));
                        verts.Add(new Vector3(prevX, y, z0));
                        verts.Add(new Vector3(x, y, z1));
                        verts.Add(new Vector3(x, y, lowerMaxima[ppx]));
                    }
                    else{
                        float prevX = x - boundingBoxWidth;
                        float prevFracX = prevX - deckSilhouette[prevVert].X;
                        float fracX = x - deckSilhouette[prevVert].X;
                        float z0 = interpolator.GetLinearValue(prevFracX);
                        float z1 = interpolator.GetLinearValue(fracX);
                        verts.Add(new Vector3(prevX, y, lowerMaxima[prevX]));
                        verts.Add(new Vector3(prevX, y, z0));
                        verts.Add(new Vector3(x, y, z1));
                        verts.Add(new Vector3(x, y, lowerMaxima[prevX]));
                    }
                }

                var buff = new ObjectBuffer<ObjectIdentifier>(verts.Count + deckBBoxes.Count, 2, 4, 6, "Shader_AirshipDeck");

                //add border quads to objectbuffer
                var nullidentifier = new ObjectIdentifier(ObjectType.Misc, Vector3.Zero);
                var idxWinding = new[]{0, 1, 2, 2, 3, 0};
                var vertli = new List<VertexPositionNormalTexture>();
                for (int i = 0; i < verts.Count; i += 4){
                    vertli.Clear();
                    vertli.Add(new VertexPositionNormalTexture(verts[i], Vector3.Up, new Vector2(0, 0)));
                    vertli.Add(new VertexPositionNormalTexture(verts[i + 1], Vector3.Up, new Vector2(1, 0)));
                    vertli.Add(new VertexPositionNormalTexture(verts[i + 2], Vector3.Up, new Vector2(1, 1)));
                    vertli.Add(new VertexPositionNormalTexture(verts[i + 3], Vector3.Up, new Vector2(0, 1)));
                    buff.AddObject(nullidentifier, (int[])idxWinding.Clone(), vertli.ToArray());
                    //reflect across Z axis
                    vertli.Clear();
                    var reflectVector = new Vector3(1, 1, -1);
                    vertli.Add(new VertexPositionNormalTexture(verts[i]*reflectVector, Vector3.Up, new Vector2(0, 0)));
                    vertli.Add(new VertexPositionNormalTexture(verts[i + 1]*reflectVector, Vector3.Up, new Vector2(1, 0)));
                    vertli.Add(new VertexPositionNormalTexture(verts[i + 2]*reflectVector, Vector3.Up, new Vector2(1, 1)));
                    vertli.Add(new VertexPositionNormalTexture(verts[i + 3]*reflectVector, Vector3.Up, new Vector2(0, 1)));
                    buff.AddObject(nullidentifier, (int[])idxWinding.Clone(), vertli.ToArray());
                }

                //add boundingbox defined quads to objectbuffer
                foreach (var boundingBox in deckBBoxes){
                    var min = boundingBox.Min;
                    var xWidth = new Vector3(boundingBox.Max.X - boundingBox.Min.X, 0, 0);
                    var zWidth = new Vector3(0, 0, boundingBox.Max.Z - boundingBox.Min.Z);
                    vertli.Clear();
                    vertli.Add(new VertexPositionNormalTexture(min, Vector3.Up, new Vector2(0, 0)));
                    vertli.Add(new VertexPositionNormalTexture(min + xWidth, Vector3.Up, new Vector2(1, 0)));
                    vertli.Add(new VertexPositionNormalTexture(min + xWidth + zWidth, Vector3.Up, new Vector2(1, 1)));
                    vertli.Add(new VertexPositionNormalTexture(min + zWidth, Vector3.Up, new Vector2(0, 1)));
                    buff.AddObject(new ObjectIdentifier(ObjectType.Deckboard, min), (int[])idxWinding.Clone(), vertli.ToArray());
                }
                ret[deck] = buff;
            }
            return ret;
        }

        static List<ObjectBuffer<HullSection>>[] GenerateDeckWallBuffers(Vector3[][][] deckSVerts, Vector3[,] normalMesh, int numDecks, int primitivesPerDeck, float boxMin, float boxMax) {
            int vertsInSilhouette = deckSVerts[0][0].Length;

            var hullMeshBuffs = new List<ObjectBuffer<HullSection>>[numDecks];
            for (int i = 0; i < hullMeshBuffs.Length; i++)
                hullMeshBuffs[i] = new List<ObjectBuffer<HullSection>>(2);

            //now set up the display buffer for each deck's wall
            for (int i = 0; i < deckSVerts.Length; i++){
                // ReSharper disable AccessToModifiedClosure
                #region generateBuff
                Func<int, int, ObjectBuffer<HullSection>> generateBuff = (start, end) => {
                    var hullMesh = new Vector3[primitivesPerDeck + 1, vertsInSilhouette / 2];
                    var hullNormals = new Vector3[primitivesPerDeck + 1, vertsInSilhouette / 2];
                    //int[] hullIndicies = MeshHelper.CreateQuadIndiceArray((primitivesPerDeck) * (vertsInSilhouette / 2-1));
                    VertexPositionNormalTexture[] hullVerticies = MeshHelper.CreateTexcoordedVertexList((primitivesPerDeck) * (vertsInSilhouette / 2-1));

                    //get the hull normals for this part of the hull from the total normals
                    for (int x = 0; x < primitivesPerDeck + 1; x++) {
                        for (int z = start; z < end; z++) {
                            hullNormals[x, z - start] = normalMesh[i * primitivesPerDeck + x, z];
                        }
                    }
                    //convert the 2d list heightmap into a 2d array heightmap
                    //this gets hella messy because gotta subsection the 2d array
                    var sVerts = new Vector3[deckSVerts[i].Length][];
                    for (int j = 0; j < deckSVerts[i].Length; j++){
                        sVerts[j] = new Vector3[end-start];
                        for (int k = start; k < end; k++){
                            sVerts[j][k - start] = deckSVerts[i][j][k];
                        }
                    }

                    MeshHelper.Encode2DListIntoArray(primitivesPerDeck + 1, (vertsInSilhouette / 2), ref hullMesh, sVerts);
                    //take the 2d array of vertexes and 2d array of normals and stick them in the vertexpositionnormaltexture 
                    MeshHelper.ConvertMeshToVertList(hullMesh, hullNormals, ref hullVerticies);
                    if (i != deckSVerts.Length) {
                        return HullSplitter.SplitLayerGeometry(0.5f, hullVerticies);
                    }
                    return null;
                };
                // ReSharper restore AccessToModifiedClosure
                #endregion

                hullMeshBuffs[i].Add(generateBuff(0, vertsInSilhouette / 2));
                hullMeshBuffs[i].Add(generateBuff(vertsInSilhouette / 2, vertsInSilhouette));
            }
            return hullMeshBuffs;
        }

        static Vector3 GenerateCenterPoint(Vector3[,] totalMesh){
            var p = new Vector3();

            p += totalMesh[0, 0];
            p += totalMesh[totalMesh.GetLength(0) - 1, totalMesh.GetLength(1) - 1];
            p /= 4;

            return p;
        }

        static BoundingBoxResult GenerateDeckBoundingBoxes(float floorBBoxWidth, Vector3[][,] deckFloorMesh){
            int numHorizontalPrimitives = deckFloorMesh[0].GetLength(1);
            var ret = new BoundingBoxResult();
            var deckBoundingBoxes = new List<BoundingBox>[deckFloorMesh.Length]; //deckFloorMesh.Length = numdecks+1

            for (int layer = 0; layer < deckFloorMesh.Length; layer++){
                var layerBBoxes = new List<BoundingBox>();
                float yLayer = deckFloorMesh[layer][0, 0].Y;

                float boxCreatorPos = 0;
                while (boxCreatorPos < deckFloorMesh[layer][1, 0].X)
                    boxCreatorPos += floorBBoxWidth;

                while (boxCreatorPos < deckFloorMesh[layer][1, numHorizontalPrimitives - 1].X){
                    int index = -1; //index of the first of the two set of vertexes to use
                    for (int i = 0; i < numHorizontalPrimitives; i++){
                        if (boxCreatorPos >= deckFloorMesh[layer][1, i].X && boxCreatorPos < deckFloorMesh[layer][1, i + 1].X){
                            index = i;
                            break;
                        }
                    }
                    Debug.Assert(index != -1);

                    float startX = deckFloorMesh[layer][0, index].X;
                    float endX = deckFloorMesh[layer][0, index + 1].X;
                    float startZ = deckFloorMesh[layer][0, index].Z;
                    float endZ = deckFloorMesh[layer][0, index + 1].Z;
                    float zBounding1, zBounding2;

                    var interpolator = new Interpolate(
                        startZ,
                        endZ,
                        endX - startX
                        );

                    if (boxCreatorPos + floorBBoxWidth < endX){ //easy scenario where we only have to take one line into consideration when finding how many boxes wide should be
                        zBounding1 = interpolator.GetLinearValue(boxCreatorPos - startX);
                        zBounding2 = interpolator.GetLinearValue(boxCreatorPos + floorBBoxWidth - startX);
                    }
                    else{
                        zBounding1 = interpolator.GetLinearValue(boxCreatorPos - startX);
                        if (index + 2 != numHorizontalPrimitives){
                            var interpolator2 = new Interpolate(
                                deckFloorMesh[layer][0, index + 1].Z,
                                deckFloorMesh[layer][0, index + 2].Z,
                                deckFloorMesh[layer][0, index + 2].X - deckFloorMesh[layer][0, index + 1].X
                                );

                            zBounding2 = interpolator2.GetLinearValue(boxCreatorPos + floorBBoxWidth - deckFloorMesh[layer][0, index + 1].X);
                        }
                        else{
                            zBounding2 = 0;
                        }
                    }

                    int zBoxes1 = (int) (zBounding1/floorBBoxWidth);
                    int zBoxes2 = (int) (zBounding2/floorBBoxWidth);

                    int numZBoxes;
                    if (zBoxes1 < zBoxes2)
                        numZBoxes = zBoxes1;
                    else
                        numZBoxes = zBoxes2;


                    for (int i = -numZBoxes; i < numZBoxes; i++){
                        layerBBoxes.Add(
                            new BoundingBox(
                                new Vector3(
                                    boxCreatorPos,
                                    yLayer,
                                    i*floorBBoxWidth
                                    ),
                                new Vector3(
                                    boxCreatorPos + floorBBoxWidth,
                                    yLayer,
                                    (i + 1)*floorBBoxWidth
                                    )
                                )
                            );
                    }

                    boxCreatorPos += floorBBoxWidth;
                }
                deckBoundingBoxes[layer] = layerBBoxes;
            }
            ret.DeckBoundingBoxes = deckBoundingBoxes;

            ret.BoxMin = ret.DeckBoundingBoxes.Min(
                layer => layer.Min(
                    box => box.Min.X
                    )
            );

            ret.BoxMax = ret.DeckBoundingBoxes.Max(
                layer => layer.Max(
                    box => box.Max.X
                    )
             );



            var wallSelectionBoxes = deckBoundingBoxes;
            var wallSelectionPoints = new List<List<Vector3>>();
            //generate vertexes of the bounding boxes

            for (int layer = 0; layer < wallSelectionBoxes.Count(); layer++){
                wallSelectionPoints.Add(new List<Vector3>());
                foreach (var box in wallSelectionBoxes[layer]){
                    wallSelectionPoints.Last().Add(box.Min);
                    wallSelectionPoints.Last().Add(box.Max);
                    wallSelectionPoints.Last().Add(new Vector3(box.Max.X, box.Max.Y, box.Min.Z));
                    wallSelectionPoints.Last().Add(new Vector3(box.Min.X, box.Max.Y, box.Max.Z));
                }

                //now we clear out all of the double entries (stupid time hog optimization)
                /*for (int box = 0; box < wallSelectionPoints[layer].Count(); box++){
                    for (int otherBox = 0; otherBox < wallSelectionPoints[layer].Count(); otherBox++){
                        if (box == otherBox)
                            continue;

                        if (wallSelectionPoints[layer][box] == wallSelectionPoints[layer][otherBox]){
                            wallSelectionPoints[layer].RemoveAt(otherBox);
                        }
                    }
                }*/
            }

            ret.DeckVertexes =
                (
                    from layer in wallSelectionPoints
                    select layer.ToList()
                ).ToArray();
            return ret;
        }

        #region Nested type: BoundingBoxResult

        struct BoundingBoxResult{
            public List<BoundingBox>[] DeckBoundingBoxes;
            public List<Vector3>[] DeckVertexes;
            public float BoxMin;
            public float BoxMax;
        }

        #endregion

        #region Nested type: GenerateHullParams

        struct GenerateHullParams{
            public List<BezierInfo> BackCurveInfo;
            public float BoundingBoxWidth;
            public float DeckHeight;
            public int PrimitivesPerDeck;
            public List<BezierInfo> SideCurveInfo;
            public List<BezierInfo> TopCurveInfo;
        }

        #endregion

        #region Nested type: GenerateHullResults

        struct GenerateHullResults{
            public float Berth;
            public Vector3[][][] DeckSilhouetteVerts;
            public Vector3[][] LayerSilhouetteVerts;
            public float Length;
            public int NumDecks;
        }

        #endregion

        #region Nested type: GenerateNormalsResults

        struct GenerateNormalsResults{
            public Vector3 Centroid;
            public Vector3[,] NormalMesh;
        }

        #endregion
    }

    internal class HullGeometryInfo{
        public Vector3 CenterPoint;
        public List<BoundingBox>[] DeckFloorBoundingBoxes;
        public ObjectBuffer<ObjectIdentifier>[] DeckFloorBuffers;
        public float DeckHeight;
        public List<Vector3>[] FloorVertexes;
        public GeometryBuffer<VertexPositionNormalTexture>[] HullWallTexBuffers;
        public Vector2 MaxBoundingBoxDims;
        public List<ObjectBuffer<HullSection>>[] HullMeshes;
        public int NumDecks;
        public float WallResolution;
    }
}