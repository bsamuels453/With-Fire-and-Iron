#define PROFILE_AIRSHIP_GENERATION

#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Forge.Core.Airship.Data;
using Forge.Core.Logic;
using Forge.Core.Util;
using Forge.Framework;
using Forge.Framework.Draw;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;

#endregion

namespace Forge.Core.Airship.Generation{
    /// <summary>
    ///   Generates the geometry for airship hulls. This differs from PreviewRenderer in that this class generates the geometry so that things like windows, portholes, or other extremities can be added easily without modifying/removing much of the geometry. In more mathematical terms, it means that the horizontal boundaries between adjacent quads are parallel to the XZ plane. This class can also take a few seconds to do its thing because it isnt going to be updating every tick like previewrenderer does.
    /// </summary>
    public static class HullGeometryGenerator{
        const int _primHeightPerDeck = 5;
        const int _horizontalPrimDivisor = 2;
        const float _deckHeight = 2.13f;
        const float _bBoxWidth = 0.5f;
        const float _hullTextureTilingSize = 4f;

        //note: less than 1 deck breaks prolly
        //note that this entire geometry generator runs on the standard curve assumptions
        public static HullGeometryInfo GenerateShip(List<BezierInfo> backCurveInfo, List<BezierInfo> sideCurveInfo, List<BezierInfo> topCurveInfo){
#if PROFILE_AIRSHIP_GENERATION
            var sw = new Stopwatch();
            sw.Start();
#endif
            var genResults = GenerateHull
                (new GenerateHullParams{
                    BackCurveInfo = backCurveInfo,
                    SideCurveInfo = sideCurveInfo,
                    TopCurveInfo = topCurveInfo,
                    DeckHeight = _deckHeight,
                    BoundingBoxWidth = _bBoxWidth,
                    PrimitivesPerDeck = _primHeightPerDeck
                }
                );
            var normalGenResults = GenerateHullNormals(genResults.LayerSilhouetteVerts);
            var deckFloorPlates = GenerateDeckPlates(genResults.LayerSilhouetteVerts, genResults.NumDecks, _primHeightPerDeck);
            var boundingBoxResults = GenerateDeckBoundingBoxes(_bBoxWidth, deckFloorPlates);
            var hullBuffResults = GenerateHullBuffers
                (
                    genResults.DeckSilhouetteVerts,
                    normalGenResults.NormalMesh,
                    genResults.NumDecks,
                    genResults.Length
                );

            var deckFloorBuffers = GenerateDeckFloorMesh(genResults.DeckSilhouetteVerts, boundingBoxResults.DeckBoundingBoxes, genResults.NumDecks);

            //reflect everything around the X axis
            foreach (var buffer in hullBuffResults.Item1){
                buffer.ApplyTransform
                    ((vert) =>{
                         vert.Position.X *= -1;
                         return vert;
                     }
                    );
            }

            foreach (var buffer in deckFloorBuffers){
                buffer.ApplyTransform
                    ((vert) =>{
                         vert.Position.X *= -1;
                         return vert;
                     }
                    );
            }

            var reflectionVector = new Vector3(-1, 1, 1);
            foreach (var boxArray in boundingBoxResults.DeckBoundingBoxes){
                for (int boxIdx = 0; boxIdx < boxArray.Count; boxIdx++){
                    boxArray[boxIdx] = new BoundingBox(boxArray[boxIdx].Min*reflectionVector, boxArray[boxIdx].Max*reflectionVector);
                }
            }
            foreach (var vertArray in boundingBoxResults.DeckVertexes){
                for (int vertIdx = 0; vertIdx < vertArray.Count; vertIdx++){
                    vertArray[vertIdx] = vertArray[vertIdx]*reflectionVector;
                }
            }

            var hullSections = GenerateHullSections(hullBuffResults.Item2, hullBuffResults.Item1);

            var resultant = new HullGeometryInfo();
            resultant.CenterPoint = normalGenResults.Centroid*reflectionVector;
            resultant.DeckSectionContainer = new DeckSectionContainer
                (
                boundingBoxResults.DeckBoundingBoxes,
                deckFloorBuffers,
                boundingBoxResults.DeckVertexes
                );
            resultant.NumDecks = genResults.NumDecks;
            resultant.WallResolution = _bBoxWidth;
            resultant.DeckHeight = _deckHeight;
            resultant.MaxBoundingBoxDims = new Vector2((int) (genResults.Length/_bBoxWidth), (int) (genResults.Berth/_bBoxWidth));
            resultant.HullSections = hullSections;

#if PROFILE_AIRSHIP_GENERATION
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
            results.NumDecks = (int) (draft/deckHeight) + 1;
            results.Depth = sideCurveInfo[1].Pos.Y;
            int numVerticalVertexes = (results.NumDecks - 1)*primitivesPerDeck + primitivesPerDeck + 1;

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
                    if (xzHullIntercepts.Last().Count == 1){ //this happens at the very bottom of the ship
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

            int numHorizontalPrimitives = (int) (results.Length/boundingBoxWidth + 1)/_horizontalPrimDivisor;
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

        static ObjectBuffer<AirshipObjectIdentifier>[] GenerateDeckFloorMesh(Vector3[][][] deckSVerts, List<BoundingBox>[] deckBoundingBoxes, int numDecks){
            float boundingBoxWidth = Math.Abs(deckBoundingBoxes[0][0].Max.X - deckBoundingBoxes[0][0].Min.X);
            Vector3 reflection = new Vector3(-1, 1, 1);
            var ret = new ObjectBuffer<AirshipObjectIdentifier>[numDecks];

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

                    var interpolator = new Interpolate
                        (
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
                        interpolator = new Interpolate
                            (
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

                var buff = new ObjectBuffer<AirshipObjectIdentifier>(verts.Count + deckBBoxes.Count, 2, 4, 6, "Shader_AirshipDeck");

                //add border quads to objectbuffer
                var nullidentifier = new AirshipObjectIdentifier(ObjectType.Misc, Vector3.Zero);
                var idxWinding = new[]{0, 1, 2, 2, 3, 0};
                var vertli = new List<VertexPositionNormalTexture>();
                for (int i = 0; i < verts.Count; i += 4){
                    vertli.Clear();
                    vertli.Add(new VertexPositionNormalTexture(verts[i], Vector3.Up, new Vector2(0, 0)));
                    vertli.Add(new VertexPositionNormalTexture(verts[i + 1], Vector3.Up, new Vector2(1, 0)));
                    vertli.Add(new VertexPositionNormalTexture(verts[i + 2], Vector3.Up, new Vector2(1, 1)));
                    vertli.Add(new VertexPositionNormalTexture(verts[i + 3], Vector3.Up, new Vector2(0, 1)));
                    buff.AddObject(nullidentifier, (int[]) idxWinding.Clone(), vertli.ToArray());
                    //reflect across Z axis
                    vertli.Clear();
                    var reflectVector = new Vector3(1, 1, -1);
                    vertli.Add(new VertexPositionNormalTexture(verts[i]*reflectVector, Vector3.Up, new Vector2(0, 0)));
                    vertli.Add(new VertexPositionNormalTexture(verts[i + 1]*reflectVector, Vector3.Up, new Vector2(1, 0)));
                    vertli.Add(new VertexPositionNormalTexture(verts[i + 2]*reflectVector, Vector3.Up, new Vector2(1, 1)));
                    vertli.Add(new VertexPositionNormalTexture(verts[i + 3]*reflectVector, Vector3.Up, new Vector2(0, 1)));
                    buff.AddObject(nullidentifier, (int[]) idxWinding.Clone(), vertli.ToArray());
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
                    buff.AddObject(new AirshipObjectIdentifier(ObjectType.Deckboard, min*reflection), (int[]) idxWinding.Clone(), vertli.ToArray());
                }
                ret[deck] = buff;
            }
            return ret;
        }

        static Tuple<ObjectBuffer<int>[], Dictionary<IEquatable<HullSectionIdentifier>, int>>
            GenerateHullBuffers(Vector3[][][] deckSVerts, Vector3[,] normalMesh, int numDecks, float length){
            //first thing we do is generate a dictionary that links HullSectionIdentifier and section uid
            int estDictSize = (int) (numDecks*_primHeightPerDeck*(length/_bBoxWidth))*2;
            var hullSectionLookup = new Dictionary<IEquatable<HullSectionIdentifier>, int>(estDictSize);
            int id = 0;
            for (float x = 0; x < length; x += _bBoxWidth){
                for (int deck = 0; deck < numDecks; deck++){
                    for (int panel = 0; panel < _primHeightPerDeck; panel++){
                        hullSectionLookup.Add(new HullSectionIdentifier(x, panel, Quadrant.Side.Port, deck), id);
                        id++;
                        hullSectionLookup.Add(new HullSectionIdentifier(x, panel, Quadrant.Side.Starboard, deck), id);
                        id++;
                    }
                }
            }

            int vertsInSilhouette = deckSVerts[0][0].Length;
            var hullMeshBuffs = new ObjectBuffer<int>[numDecks];
            //now set up the display buffer for each deck's wall
            for (int i = 0; i < deckSVerts.Length; i++){
                // ReSharper disable AccessToModifiedClosure

                #region generateBuff

                Func<int, int, List<Tuple<VertexPositionNormalTexture[], int[], HullSectionIdentifier>>> generateBuff =
                    (start, end) =>{
                        var hullMesh =
                            new Vector3[
                                _primHeightPerDeck + 1,
                                vertsInSilhouette/2];
                        var hullNormals =
                            new Vector3[
                                _primHeightPerDeck + 1,
                                vertsInSilhouette/2];
                        //int[] hullIndicies = MeshHelper.CreateQuadIndiceArray((primitivesPerDeck) * (vertsInSilhouette / 2-1));
                        VertexPositionNormalTexture[]
                            hullVerticies =
                                MeshHelper.
                                    CreateTexcoordedVertexList
                                    ((_primHeightPerDeck)*
                                        (vertsInSilhouette/2 -
                                            1));

                        //get the hull normals for this part of the hull from the total normals
                        for (int x = 0;
                            x < _primHeightPerDeck + 1;
                            x++){
                            for (int z = start;
                                z < end;
                                z++){
                                hullNormals[x, z - start] =
                                    normalMesh[
                                        i*_primHeightPerDeck +
                                            x, z];
                            }
                        }
                        //convert the 2d list heightmap into a 2d array heightmap
                        //this gets hella messy because gotta subsection the 2d array
                        var sVerts =
                            new Vector3[deckSVerts[i].Length
                                ][];
                        for (int j = 0;
                            j < deckSVerts[i].Length;
                            j++){
                            sVerts[j] =
                                new Vector3[end - start];
                            for (int k = start;
                                k < end;
                                k++){
                                sVerts[j][k - start] =
                                    deckSVerts[i][j][k];
                            }
                        }

                        MeshHelper.Encode2DListIntoArray
                            (_primHeightPerDeck + 1,
                                (vertsInSilhouette/2),
                                ref hullMesh, sVerts);

                        int f = 5;
                        //Now we need to assign the real texcoords.
                        //We have to loop through the mesh in an awkward way in order to make it so the 
                        //hull texture's edge meets at the bottom of airship perfectly.
                        //If we were to loop through this in any other way, the polygons that make up the 
                        //bottom of the ship would meet halfway through the texture, depending on the airship depth.

                        var horizontalDistances = new float[hullMesh.GetLength(0),hullMesh.GetLength(1)];

                        //oh god this hack is so ugly.
                        //this happens because this function is called twice, and each time it's called the "front" vertex
                        //is at a different side of the array. In order for this mirroring to work, we have to iterate in
                        //the opposite direction for one of the buffers
                        bool reverseVertexIteration = hullMesh[0, 0].X == 0;

                        for (int layerIdx = hullMesh.GetLength(0) - 1; layerIdx >= 0; layerIdx--){
                            if (!reverseVertexIteration){
                                for (int vertexIdx = 0; vertexIdx < hullMesh.GetLength(1) - 1; vertexIdx++){
                                    /*
                                    horizontalDistances[layerIdx, vertexIdx] =
                                        Vector3.Distance
                                            (
                                                hullMesh[layerIdx, vertexIdx],
                                                hullMesh[layerIdx, vertexIdx + 1]
                                            );
                                     */
                                    horizontalDistances[layerIdx, vertexIdx] = Math.Abs(hullMesh[layerIdx, vertexIdx].X - hullMesh[layerIdx, vertexIdx + 1].X);
                                }
                            }
                            else{
                                for (int vertexIdx = hullMesh.GetLength(1) - 1; vertexIdx > 0; vertexIdx--){
                                    /*
                                    horizontalDistances[layerIdx, vertexIdx] =
                                        Vector3.Distance
                                            (
                                                hullMesh[layerIdx, vertexIdx],
                                                hullMesh[layerIdx, vertexIdx - 1]
                                            );
                                     
                                     */
                                    horizontalDistances[layerIdx, vertexIdx] = Math.Abs(hullMesh[layerIdx, vertexIdx].X - hullMesh[layerIdx, vertexIdx - 1].X);
                                }
                            }
                        }
                        //float _texCoordXMulti = _hullTextureTilingSize / length;
                        //float _texCoordYMulti = _hullTextureTilingSize / length;

                        var texCoords = new Vector2[hullMesh.GetLength(0),hullMesh.GetLength(1)];
                        for (int layerIdx = hullMesh.GetLength(0) - 1; layerIdx >= 0; layerIdx--){
                            float distSum = 0;
                            if (!reverseVertexIteration){
                                for (int vertexIdx = 0; vertexIdx < hullMesh.GetLength(1); vertexIdx++){
                                    Vector3 vertPos = hullMesh[layerIdx, vertexIdx];

                                    texCoords[layerIdx, vertexIdx] = new Vector2
                                        (
                                        vertPos.X/(_hullTextureTilingSize),
                                        Math.Abs(vertPos.Y/(_hullTextureTilingSize))
                                        );
                                    distSum += horizontalDistances[layerIdx, vertexIdx];
                                }
                            }
                            else{
                                for (int vertexIdx = hullMesh.GetLength(1) - 1; vertexIdx >= 0; vertexIdx--){
                                    Vector3 vertPos = hullMesh[layerIdx, vertexIdx];

                                    texCoords[layerIdx, vertexIdx] = new Vector2
                                        (
                                        vertPos.X/(_hullTextureTilingSize),
                                        Math.Abs(vertPos.Y/_hullTextureTilingSize)
                                        );
                                    distSum += horizontalDistances[layerIdx, vertexIdx];
                                }
                            }
                        }


                        //take the 2d array of vertexes and 2d array of normals and stick them in the vertexpositionnormaltexture[]
                        MeshHelper.ConvertMeshToVertList
                            (
                                hullMesh,
                                hullNormals,
                                texCoords,
                                ref hullVerticies
                            );

                        if (i != deckSVerts.Length){
                            return
                                HullSplitter.
                                    SplitLayerGeometry
                                    (0.5f, hullVerticies, i);
                        }
                        return null;
                    };
                // ReSharper restore AccessToModifiedClosure

                #endregion

                var triangles = (generateBuff(0, vertsInSilhouette/2));
                triangles.AddRange(generateBuff(vertsInSilhouette/2, vertsInSilhouette));

                var buff = new ObjectBuffer<int>(triangles.Count, 1, 3, 3, "Shader_AirshipHull");

                foreach (var triangle in triangles){
                    buff.AddObject(hullSectionLookup[triangle.Item3], triangle.Item2, triangle.Item1);
                }

                hullMeshBuffs[i] = buff;
            }
            return new Tuple<ObjectBuffer<int>[], Dictionary<IEquatable<HullSectionIdentifier>, int>>(hullMeshBuffs, hullSectionLookup);
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

                    var interpolator = new Interpolate
                        (
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
                            var interpolator2 = new Interpolate
                                (
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
                        layerBBoxes.Add
                            (
                                new BoundingBox
                                    (
                                    new Vector3
                                        (
                                        boxCreatorPos,
                                        yLayer,
                                        i*floorBBoxWidth
                                        ),
                                    new Vector3
                                        (
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

            ret.BoxMin = ret.DeckBoundingBoxes.Min
                (
                    layer => layer.Min
                        (
                            box => box.Min.X
                        )
                );

            ret.BoxMax = ret.DeckBoundingBoxes.Max
                (
                    layer => layer.Max
                        (
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

        static HullSectionContainer GenerateHullSections(Dictionary<IEquatable<HullSectionIdentifier>, int> hullIdRef, ObjectBuffer<int>[] buffers){
            var hullIdRefInv = hullIdRef.ToDictionary(pair => pair.Value, pair => pair.Key);

            //first obtain all the shard data via objectbuffer dump
            var estTotSize = buffers.Sum(b => b.MaxObjects);
            var totalShardData = new List<ObjectBuffer<int>.ObjectData>(estTotSize);
            foreach (var buffer in buffers){
                var dump = buffer.DumpObjectData();
                foreach (ObjectBuffer<int>.ObjectData obj in dump){
                    totalShardData.Add(obj);
                }
            }
            totalShardData.TrimExcess();

            //group the shard data into sections
            var groupedSections = (from section in totalShardData
                group section by section.Identifier).ToArray();


            var hullSections = new List<HullSection>(groupedSections.Count());
            foreach (var section in groupedSections){
                Vector3[] aliasedVertexes;

                {
                    //first obtain the aliased vertexes of the section
                    var cumulativeVerts = new List<Vector3>(30);
                    foreach (var shard in section){
                        cumulativeVerts.AddRange(from v in shard.Verticies select (Vector3) v.Position);
                    }
                    float maxY = cumulativeVerts.Max(v => v.Y);
                    float minY = cumulativeVerts.Min(v => v.Y);

                    //wish there was a way to linq  this next part, but there isnt
                    Vector3 invalidVert = new Vector3(-1, -1, -1);
                    Vector3 maxXmaxY = invalidVert;
                    Vector3 minXmaxY = invalidVert;
                    Vector3 maxXminY = invalidVert;
                    Vector3 minXminY = invalidVert;
                    float maxXtop = float.MinValue;
                    float minXtop = float.MaxValue;
                    float maxXbot = float.MinValue;
                    float minXbot = float.MaxValue;
                    // ReSharper disable CompareOfFloatsByEqualityOperator
                    foreach (var vert in cumulativeVerts){
                        if (vert.Y == maxY){
                            if (vert.X > maxXtop){
                                maxXmaxY = vert;
                                maxXtop = vert.X;
                            }
                            if (vert.X < minXtop){
                                minXmaxY = vert;
                                minXtop = vert.X;
                            }
                        }
                        if (vert.Y == minY){
                            if (vert.X > maxXbot){
                                maxXminY = vert;
                                maxXbot = vert.X;
                            }
                            if (vert.X < minXbot){
                                minXminY = vert;
                                minXbot = vert.X;
                            }
                        }
                    }
                    // ReSharper restore CompareOfFloatsByEqualityOperator
                    Debug.Assert(maxXmaxY != invalidVert);
                    Debug.Assert(minXmaxY != invalidVert);
                    Debug.Assert(maxXminY != invalidVert);
                    Debug.Assert(minXminY != invalidVert);

                    aliasedVertexes = new[]{maxXmaxY, minXmaxY, minXminY, maxXmaxY, maxXminY, minXminY};
                }

                int uid = (int) section.First().Identifier;

                var identifier = (HullSectionIdentifier) hullIdRefInv[uid];
                var homeBuffer = from buff in buffers where buff.Contains(uid) select buff;

                hullSections.Add
                    (new HullSection
                        (
                        uid,
                        aliasedVertexes,
                        identifier,
                        homeBuffer.Single()
                        )
                    );
            }

            return new HullSectionContainer(hullSections, buffers);
        }

        #region Nested type: BoundingBoxResult

        struct BoundingBoxResult{
            public float BoxMax;
            public float BoxMin;
            public List<BoundingBox>[] DeckBoundingBoxes;
            public List<Vector3>[] DeckVertexes;
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
            public float Depth;
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

    public class HullGeometryInfo{
        public Vector3 CenterPoint;
        public float DeckHeight;
        public DeckSectionContainer DeckSectionContainer;
        public HullSectionContainer HullSections;
        public Vector2 MaxBoundingBoxDims;
        public int NumDecks;
        public float WallResolution;
    }
}