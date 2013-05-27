#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Forge.Core.Airship;
using Forge.Core.Airship.Data;
using Forge.Framework.Draw;
using Forge.Core.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Forge.Core.ObjectEditor{
    /// <summary>
    /// This class is used to subdivide sections of hull into pieces of a static size. These hull pieces can be toggled on and off through the objectbuffer that is returned.
    /// </summary>
    internal static class HullSplitter{
        static Quadrant.Side _side;
        static float _boxWidth;

        public static List<Tuple<VertexPositionNormalTexture[], int[], HullSectionIdentifier>>
            SplitLayerGeometry(float boundingWidth, VertexPositionNormalTexture[] verts, int deck) {
            if (verts[1].Position.Z > 0) {
                _side = Quadrant.Side.Port;
            }
            else {
                _side = Quadrant.Side.Starboard;
            }
            _boxWidth = boundingWidth;

            var groupedPanels = new List<IEnumerable<VertexPositionNormalTexture>>();
            for (int panelIdx = 0; panelIdx < verts.Length; panelIdx += 4) {
                var temp = verts.Skip(panelIdx).Take(4).ToArray();
                groupedPanels.Add(new[] { temp[0], temp[1], temp[2] });
                groupedPanels.Add(new[] { temp[2], temp[3], temp[0] });
            }

            var sortedPanels = (from grp in groupedPanels
                                group grp by grp.Max(x => x.Position.Y) into layers
                                select layers.ToArray()).ToArray();

            var retTuple = new List<Tuple<VertexPositionNormalTexture[], int[], HullSectionIdentifier>>((int)(400 * 7 / boundingWidth));

            int layerIdx = 0;
            foreach (var layer in sortedPanels) {
                var totalLayerVerts = (from tri in layer
                                       from vert in tri
                                       select vert).ToArray();
                retTuple.AddRange(SubdividePanel(totalLayerVerts, layerIdx, deck));
                layerIdx++;
            }

            //used for rendering reference geometry. not taking this out after done debugging because it produces a really cool z fighting effect.
            /*
            foreach (var layer in sortedPanels){
                foreach (var tri in layer){
                    var q = tri.ToArray();
                    var va1 = new []{q[0],q[1],q[2]};
                    va1[0].Normal = Vector3.Zero;
                    va1[1].Normal = Vector3.Zero;
                    va1[2].Normal = Vector3.Zero;

                    var hullid = new HullSection(-1, -1, 1);
                    tempBuff.AddObject(hullid, new int[]{0,1,2}, va1);
                }
            }
            */

            return retTuple;
        }

        static List<Tuple<VertexPositionNormalTexture[], int[], HullSectionIdentifier>>
            SubdividePanel(VertexPositionNormalTexture[] panelVerts, int panelLayer, int deck) {

            var ret = new List<Tuple<VertexPositionNormalTexture[], int[], HullSectionIdentifier>>(5);

            var groupedTris = new VertexPositionNormalTexture[panelVerts.Length / 3][];
            float subBoxBeginX;
            float subBoxEndX;
            float maxX;
            {
                //sort the panelVerts into triangles
                int srcIdx = 0;
                for (int i = 0; i < groupedTris.Length; i++) {
                    groupedTris[i] = new VertexPositionNormalTexture[3];
                    for (int triIdx = 0; triIdx < 3; triIdx++) {
                        groupedTris[i][triIdx] = panelVerts[srcIdx + triIdx];
                    }
                    srcIdx += 3;
                }
            }
            {
                //generate  statistics on this layer
                float minX = panelVerts.Min(x => x.Position.X);
                maxX = panelVerts.Max(x => x.Position.X);
                //float topY = panelVerts.Max(y => y.Position.Y);
                //float bottomY = panelVerts.Min(y => y.Position.Y);

                subBoxBeginX = 0;
                while (subBoxBeginX + _boxWidth < minX){
                    subBoxBeginX += _boxWidth;
                }
                subBoxEndX = subBoxBeginX + _boxWidth;
            }

            while (subBoxBeginX < maxX){

                var sortedTris = CullTriangles(groupedTris, subBoxBeginX, subBoxEndX);

                var id = new HullSectionIdentifier(subBoxBeginX, panelLayer, _side, deck);

                //handling zero enclosed verts cases
                foreach (var triangle in sortedTris[0]){
                    ret.AddRange(SliceZeroEnclosureTriangle(triangle, subBoxBeginX, subBoxEndX, id));
                }

                //handling case where one vert is enclosed within the box
                foreach (var triangle in sortedTris[1]) {
                    ret.AddRange(SliceSingleEnclosureTriangle(triangle, subBoxBeginX, subBoxEndX, id));
                }

                //handling case where two verts are enclosed within the box
                foreach (var triangle in sortedTris[2]) {
                    ret.AddRange(SliceDoubleEnclosureTriangle(triangle, subBoxBeginX, subBoxEndX, id));
                }

                foreach (var triangle in sortedTris[3]) {
                    var idxLi = GenerateIndiceList(triangle);
                    ret.Add(new Tuple<VertexPositionNormalTexture[], int[], HullSectionIdentifier>(
                        triangle, idxLi, id));
                }


                subBoxBeginX += _boxWidth;
                subBoxEndX += _boxWidth;
            }
            return ret;
        }

        static List<Tuple<VertexPositionNormalTexture[], int[], HullSectionIdentifier>> SliceZeroEnclosureTriangle(VertexPositionNormalTexture[] triangle, float subBoxBegin, float subBoxEnd, HullSectionIdentifier identifier) {
            //first have to figure out the "anchor vertex"
            VertexPositionNormalTexture anchor;
            VertexPositionNormalTexture[] satellites;
            float anchorBoundary, satelliteBoundary;

            var side1 = from vert in triangle
                where vert.Position.X >= subBoxEnd
                select vert;
            var side2 = from vert in triangle
                where vert.Position.X <= subBoxBegin
                select vert;
            // ReSharper disable PossibleMultipleEnumeration db query too lightweight to care
            if (side1.Count() == 1){
                anchor = side1.Single();
                satellites = side2.ToArray();
                anchorBoundary = subBoxEnd;
                satelliteBoundary = subBoxBegin;
            }
            else{
                anchor = side2.Single();
                satellites = side1.ToArray();
                anchorBoundary = subBoxBegin;
                satelliteBoundary = subBoxEnd;
            }
            // ReSharper restore PossibleMultipleEnumeration

            if (satellites[0].Position.Y < satellites[1].Position.Y){
                var temp = satellites[0];
                satellites[0] = satellites[1];
                satellites[1] = temp;
            }

            var s1Pos = Lerp.Trace3X(satellites[0].Position, anchor.Position, satelliteBoundary);
            var s2Pos = Lerp.Trace3X(satellites[1].Position, anchor.Position, satelliteBoundary);

            var a1Pos = Lerp.Trace3X(satellites[0].Position, anchor.Position, anchorBoundary);
            var a2Pos = Lerp.Trace3X(satellites[1].Position, anchor.Position, anchorBoundary);

            var s1Norm = InterpolateNorm(satellites[0].Normal, anchor.Normal, satellites[0].Position, anchor.Position, s1Pos);
            var s2Norm = InterpolateNorm(satellites[1].Normal, anchor.Normal, satellites[1].Position, anchor.Position, s2Pos);

            var a1Norm = InterpolateNorm(satellites[0].Normal, anchor.Normal, satellites[0].Position, anchor.Position, a1Pos);
            var a2Norm = InterpolateNorm(satellites[1].Normal, anchor.Normal, satellites[1].Position, anchor.Position, a2Pos);

            var s1UV = InterpolateUV(satellites[0].TextureCoordinate, anchor.TextureCoordinate, satellites[0].Position, anchor.Position, s1Pos);
            var s2UV = InterpolateUV(satellites[1].TextureCoordinate, anchor.TextureCoordinate, satellites[1].Position, anchor.Position, s2Pos);

            var a1UV = InterpolateUV(satellites[0].TextureCoordinate, anchor.TextureCoordinate, satellites[0].Position, anchor.Position, a1Pos);
            var a2UV = InterpolateUV(satellites[1].TextureCoordinate, anchor.TextureCoordinate, satellites[1].Position, anchor.Position, a2Pos);

            var s1 = new VertexPositionNormalTexture(s1Pos, s1Norm, s1UV);
            var s2 = new VertexPositionNormalTexture(s2Pos, s2Norm, s2UV);

            var a1 = new VertexPositionNormalTexture(a1Pos, a1Norm, a1UV);
            var a2 = new VertexPositionNormalTexture(a2Pos, a2Norm, a2UV);
            
            var t1 = new[] { s1, a1, s2 };
            var t2 = new[] { a2, s2, a1 };


            var t1I = GenerateIndiceList(t1);
            var t2I = GenerateIndiceList(t2);

            var ret = new List<Tuple<VertexPositionNormalTexture[], int[], HullSectionIdentifier>>(2);
            ret.Add(new Tuple<VertexPositionNormalTexture[], int[], HullSectionIdentifier>(t1, t1I, identifier));
            ret.Add(new Tuple<VertexPositionNormalTexture[], int[], HullSectionIdentifier>(t2, t2I, identifier));
            return ret;
        }

        static List<Tuple<VertexPositionNormalTexture[], int[], HullSectionIdentifier>> 
            SliceSingleEnclosureTriangle(VertexPositionNormalTexture[] triangle, float subBoxBegin, float subBoxEnd, HullSectionIdentifier identifier) {
            var leftSide = (from vert in triangle
                            where vert.Position.X > subBoxEnd
                            select vert).ToArray();
            var rightSide = (from vert in triangle
                             where vert.Position.X < subBoxBegin
                             select vert).ToArray();
            var middle = (from vert in triangle
                          where vert.Position.X >= subBoxBegin && vert.Position.X <= subBoxEnd
                          select vert).Single();

            if (leftSide.Length == 2 || leftSide.Length == 0) {
                VertexPositionNormalTexture[] anchorVerts;
                float interpLimit;

                if (leftSide.Length == 2) {
                    //handle case with two verts on left, one in mid, zero on right
                    anchorVerts = leftSide;
                    interpLimit = subBoxEnd;
                }
                else{
                    //handle case with two verts on right, one in mid, zero on left
                    anchorVerts = rightSide;
                    interpLimit = subBoxBegin;
                }
                var interpPos1 = Lerp.Trace3X(anchorVerts[0].Position, middle.Position, interpLimit);
                var interpPos2 = Lerp.Trace3X(anchorVerts[1].Position, middle.Position, interpLimit);

                var interpNorm1 = InterpolateNorm(anchorVerts[0].Normal, middle.Normal, anchorVerts[0].Position, middle.Position, interpPos1);
                var interpNorm2 = InterpolateNorm(anchorVerts[1].Normal, middle.Normal, anchorVerts[1].Position, middle.Position, interpPos2);

                var interpUV1 = InterpolateUV(anchorVerts[0].TextureCoordinate, middle.TextureCoordinate, anchorVerts[0].Position, middle.Position, interpPos1);
                var interpUV2 = InterpolateUV(anchorVerts[1].TextureCoordinate, middle.TextureCoordinate, anchorVerts[1].Position, middle.Position, interpPos2);

                var interpVert1 = new VertexPositionNormalTexture(interpPos1, interpNorm1, interpUV1);
                var interpVert2 = new VertexPositionNormalTexture(interpPos2, interpNorm2, interpUV2);

                var t1 = new[] { interpVert1, middle, interpVert2 };
                var t1I = GenerateIndiceList(t1);
                var ret = new List<Tuple<VertexPositionNormalTexture[], int[], HullSectionIdentifier>>(2);
                ret.Add(new Tuple<VertexPositionNormalTexture[], int[], HullSectionIdentifier>(t1, t1I, identifier));
                return ret;
                 
            }
            else{
                var leftVert = leftSide.Single();
                var rightVert = rightSide.Single();

                var l1Pos = Lerp.Trace3X(leftVert.Position, middle.Position, subBoxEnd);
                var l2Pos = Lerp.Trace3X(leftVert.Position, rightVert.Position, subBoxEnd);

                var r1Pos = Lerp.Trace3X(rightVert.Position, middle.Position, subBoxBegin);
                var r2Pos = Lerp.Trace3X(rightVert.Position, leftVert.Position, subBoxBegin);

                var s1Norm = InterpolateNorm(leftVert.Normal, middle.Normal, leftVert.Position, middle.Position, l1Pos);
                var s2Norm = InterpolateNorm(leftVert.Normal, rightVert.Normal, leftVert.Position, rightVert.Position, l2Pos);

                var a1Norm = InterpolateNorm(rightVert.Normal, middle.Normal, rightVert.Position, middle.Position, r1Pos);
                var a2Norm = InterpolateNorm(rightVert.Normal, leftVert.Normal, rightVert.Position, leftVert.Position, r2Pos);

                var s1UV = InterpolateUV(leftVert.TextureCoordinate, middle.TextureCoordinate, leftVert.Position, middle.Position, l1Pos);
                var s2UV = InterpolateUV(leftVert.TextureCoordinate, rightVert.TextureCoordinate, leftVert.Position, rightVert.Position, l2Pos);

                var a1UV = InterpolateUV(rightVert.TextureCoordinate, middle.TextureCoordinate, rightVert.Position, middle.Position, r1Pos);
                var a2UV = InterpolateUV(rightVert.TextureCoordinate, leftVert.TextureCoordinate, rightVert.Position, leftVert.Position, r2Pos);

                var l1 = new VertexPositionNormalTexture(l1Pos, s1Norm, s1UV);
                var l2 = new VertexPositionNormalTexture(l2Pos, s2Norm, s2UV);

                var r1 = new VertexPositionNormalTexture(r1Pos, a1Norm, a1UV);
                var r2 = new VertexPositionNormalTexture(r2Pos, a2Norm, a2UV);

                var t1 = new[] { r1, r2, middle };
                var t2 = new[] { l1, l2, middle };

                //we want to use the two edge vertexes that are furthest from the middle
                var t3V1 = Vector3.Distance(l1.Position, middle.Position) < Vector3.Distance(l2.Position, middle.Position) ? l2 : l1;
                var t3V2 = Vector3.Distance(r1.Position, middle.Position) < Vector3.Distance(r2.Position, middle.Position) ? r2 : r1;
                var t3 = new[] { t3V1, t3V2, middle };

                var t1I = GenerateIndiceList(t1);
                var t2I = GenerateIndiceList(t2);
                var t3I = GenerateIndiceList(t3);

                var ret = new List<Tuple<VertexPositionNormalTexture[], int[], HullSectionIdentifier>>(2);
                ret.Add(new Tuple<VertexPositionNormalTexture[], int[], HullSectionIdentifier>(t1, t1I, identifier));
                ret.Add(new Tuple<VertexPositionNormalTexture[], int[], HullSectionIdentifier>(t2, t2I, identifier));
                ret.Add(new Tuple<VertexPositionNormalTexture[], int[], HullSectionIdentifier>(t3, t3I, identifier));
                return ret;
            }
        }

        static List<Tuple<VertexPositionNormalTexture[], int[], HullSectionIdentifier>>
            SliceDoubleEnclosureTriangle(VertexPositionNormalTexture[] triangle, float subBoxBegin, float subBoxEnd, HullSectionIdentifier identifier) {
            var sideVert = (from vert in triangle
                            where vert.Position.X <= subBoxBegin || vert.Position.X >= subBoxEnd
                            select vert).Single();
            var middle = (from vert in triangle
                          where vert.Position.X > subBoxBegin && vert.Position.X < subBoxEnd
                          select vert).ToArray();

            float interpLimit = sideVert.Position.X > subBoxEnd ? subBoxEnd : subBoxBegin;

            var interpPos1 = Lerp.Trace3X(sideVert.Position, middle[0].Position, interpLimit);
            var interpPos2 = Lerp.Trace3X(sideVert.Position, middle[1].Position, interpLimit);

            var interpNorm1 = InterpolateNorm(sideVert.Normal, middle[0].Normal, sideVert.Position, middle[0].Position, interpPos1);
            var interpNorm2 = InterpolateNorm(sideVert.Normal, middle[1].Normal, sideVert.Position, middle[1].Position, interpPos2);

            var interpUV1 = InterpolateUV(sideVert.TextureCoordinate, middle[0].TextureCoordinate, sideVert.Position, middle[0].Position, interpPos1);
            var interpUV2 = InterpolateUV(sideVert.TextureCoordinate, middle[1].TextureCoordinate, sideVert.Position, middle[1].Position, interpPos2);

            var interpVert1 = new VertexPositionNormalTexture(interpPos1, interpNorm1, interpUV1);
            var interpVert2 = new VertexPositionNormalTexture(interpPos2, interpNorm2, interpUV2);

            var t1 = new [] { interpVert1, interpVert2, middle[0] };
            var t2 = new[] { middle[0], middle[1], interpVert2 };

            var t1I = GenerateIndiceList(t1);
            var t2I = GenerateIndiceList(t2);

            var ret = new List<Tuple<VertexPositionNormalTexture[], int[], HullSectionIdentifier>>(2);
            ret.Add(new Tuple<VertexPositionNormalTexture[], int[], HullSectionIdentifier>(t1, t1I, identifier));
            ret.Add(new Tuple<VertexPositionNormalTexture[], int[], HullSectionIdentifier>(t2, t2I, identifier));
            return ret;
        }

        /// <summary>
        /// Culls the triangles that don't cross the x-region defined by subboxbegin and subboxend. Then sorts these triangles into arrays based on how many of their verts intersect the bounding region.
        /// </summary>
        /// <param name="groupedTriangles"></param>
        /// <param name="subBoxBegin"></param>
        /// <param name="subBoxEnd"></param>
        /// <returns></returns>
        static List<VertexPositionNormalTexture[]>[] CullTriangles(
            VertexPositionNormalTexture[][] groupedTriangles,
            float subBoxBegin,
            float subBoxEnd) {

            var retList = new List<VertexPositionNormalTexture[]>[4];

            #region anon methods
            Func<VertexPositionNormalTexture[], float, float, int> numEnclosedVerts =
                (triangle, begin, end) => {
                    int tot = 0;
                    foreach (var vert in triangle) {
                        if (vert.Position.X <= end && vert.Position.X >= begin)
                            tot++;
                    }
                    return tot;
                };

            Func<VertexPositionNormalTexture[], float, float, bool> isTriRelevant =
                (triangle, end, start) => {
                    //test to see if any of the points of the triangle fall within decal area
                    if (numEnclosedVerts(triangle, end, start) > 0)
                        return true;

                    //test to see if the triangle engulfs the decal area
                    foreach (var v1 in triangle) {
                        foreach (var v2 in triangle) {
                            //through the magic of looping only one if statement is required
                            if (v1.Position.X >= end && v2.Position.X <= start)
                                return true;
                        }
                    }

                    return false;
                };
            #endregion

            //get rid of irrelevant triangles
            var filteredTris = (from triangle in groupedTriangles
                                where isTriRelevant(triangle, subBoxBegin, subBoxEnd)
                                select triangle).ToList();

            //sorting triangles by number of points enclosed within the decal
            for (int i = 0; i < retList.Length; i++) {
                retList[i] = new List<VertexPositionNormalTexture[]>();
            }
            foreach (var triangle in filteredTris) {
                int numContained = numEnclosedVerts(triangle, subBoxBegin, subBoxEnd);
                retList[numContained].Add(triangle);
            }
            return retList;
        }

        /// <summary>
        /// Generates correctly wound indice list for the provided vertexes. Only actually requires the position of the VertexPositionNormalTexture to be defined.
        /// </summary>
        /// <param name="verts"></param>
        /// <returns></returns>
        static int[] GenerateIndiceList(VertexPositionNormalTexture[] verts) {
            Debug.Assert(verts != null);
            var cross = Vector3.Cross(verts[1].Position - verts[0].Position, verts[2].Position - verts[0].Position);

            switch (_side) {
                case Quadrant.Side.Port:
                    if (cross.Z > 0) {
                        return new[] { 0,2, 1 };
                    }
                    return new[] { 0, 1, 2 };
                case Quadrant.Side.Starboard:
                    if (cross.Z > 0) {
                        return new[] { 0,1, 2 };
                    }
                    return new[] { 0, 2, 1 };
            }
            Debug.Assert(false);
            return null;
        }

        static Vector3 InterpolateNorm(Vector3 n1, Vector3 n2, Vector3 p1, Vector3 p2, Vector3 mid){
            float d1 = Vector3.Distance(p1, p2);
            float d2 = Vector3.Distance(p1, mid);
            float t = d2 / d1;
            return n1 * (1-t) + n2 * (t);
        }

        static Vector2 InterpolateUV(Vector2 u1, Vector2 u2, Vector3 p1, Vector3 p2, Vector3 mid){
            float d1 = Vector3.Distance(p1, p2);
            float d2 = Vector3.Distance(p1, mid);
            float t = d2 / d1;
            Vector2 ret = u1 * (1-t) + u2 * (t);
            /*
            if (ret.X > 1)
                ret.X = 1;
            if (ret.Y > 1)
                ret.Y = 1;
            if (ret.X < 0)
                ret.X = 0;
            if(ret.Y < 0)
                ret.Y = 0;
             */
            return ret;
        }
    }
}