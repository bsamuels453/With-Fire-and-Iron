#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Forge.Core.Airship;
using Forge.Framework.Draw;
using Forge.Core.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Forge.Core.ObjectEditor{
    /// <summary>
    ///   This class handles the meshes that make up the hull on each deck of the airship. This class is used for projecting shapes into the mesh to allow for objects such as portholes. Each deck's hull is broken in two parts split down the center.
    /// </summary>
    internal class HullMesh : IEnumerable{
        public ObjectBuffer<HullSection> HullBuff { get; private set; }
        readonly int[] _idxWinding;
        readonly Side _side;
        readonly float _boxWidth;
        readonly Dictionary<float, int> _panelLookup; 

        enum Side{
            Left,
            Right
        }

        public HullMesh( float boundingWidth, VertexPositionNormalTexture[] verts) {
            if (verts[1].Position.Z > 0) {
                _idxWinding = new []{ 0, 1, 2 };
                _side = Side.Right;
            }
            else{
                _idxWinding = new[] { 0, 2, 1};
                _side = Side.Left;
            }
            _boxWidth = boundingWidth;

            var groupedPanels = new List<IEnumerable<VertexPositionNormalTexture>>();
            for (int panelIdx = 0; panelIdx < verts.Length; panelIdx += 4){
                groupedPanels.Add(verts.Skip(panelIdx).Take(3));
                groupedPanels.Add(verts.Skip(panelIdx+1).Take(3));
            }

            var sortedPanels = (from grp in groupedPanels
                                group grp by grp.Min(x => x.Position.Y) into layers
                                select layers.ToArray()).ToArray();
            
            _panelLookup = new Dictionary<float, int>(5);
            foreach (var layer in sortedPanels.Reverse()) {
                _panelLookup.Add(layer[0].ElementAt(2).Position.Y, _panelLookup.Count);
            }

            //xxx need better estimation for number of objects
            var tempBuff = new ObjectBuffer<HullSection>((int)(400*5/boundingWidth), 1, 3, 3, "Shader_AirshipHull");
            tempBuff.UpdateBufferManually = true;

            foreach (var layer in sortedPanels){
                var totalLayerVerts = from tri in layer
                                      from vert in tri
                                      select vert;
                SubdividePanel(tempBuff, totalLayerVerts.ToArray());

            }

            foreach (var layer in sortedPanels){
                foreach (var quad in layer){
                    var q = quad.ToArray();
                    var va1 = new []{q[0],q[1],q[2]};
                    va1[0].Normal = Vector3.Zero;
                    va1[1].Normal = Vector3.Zero;
                    va1[2].Normal = Vector3.Zero;

                    var hullid = new HullSection(-1, -1, 1);
                    //tempBuff.AddObject(hullid, new int[]{0,1,2}, va1);
                    //tempBuff.AddObject(hullid, _idxWinding, va2);
                }
            }

            if (tempBuff.ActiveObjects == 0)
                return;

            HullBuff = new ObjectBuffer<HullSection>(tempBuff.ActiveObjects, 1, 3, 3, "Shader_AirshipHull");
            HullBuff.CullMode = CullMode.None;
            HullBuff.AbsorbBuffer(tempBuff, true);
        }

        public void DisablePanel(float xPos, float zPos, int yPanel){
            if (zPos > 0 && _side != Side.Right)
                return;
            if (zPos < 0 && _side != Side.Left)
                return;
            
            if(!HullBuff.DisableObject(
                new HullSection(xPos, xPos+_boxWidth, yPanel))){
                    throw new Exception("bad disable request, panel doesnt exist");
            }
        }

        public void EnablePanel(float xPos, float zPos, int yPanel) {
            if (zPos > 0 && _side != Side.Right)
                return;
            if (zPos < 0 && _side != Side.Left)
                return;

            if (!HullBuff.EnableObject(
                new HullSection(xPos, xPos + _boxWidth, yPanel))) {
                    throw new Exception("bad enable request, panel doesnt exist");
            }
        }

        void SubdividePanel(ObjectBuffer<HullSection> buff, VertexPositionNormalTexture[] panelVerts) {
            var sw = new Stopwatch();
            sw.Start();

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
                float topY = panelVerts.Max(y => y.Position.Y);
                float bottomY = panelVerts.Min(y => y.Position.Y);

                subBoxBeginX = 0;
                while (subBoxBeginX + _boxWidth < minX){
                    subBoxBeginX += _boxWidth;
                }
                subBoxEndX = subBoxBeginX + _boxWidth;
            }

            while (subBoxBeginX < maxX){

                var sortedTris = SortTriangles(groupedTris, subBoxBeginX, subBoxEndX);

                //handling zero enclosed verts cases
                foreach (var triangle in sortedTris[0]){
                    SliceZeroEnclosureTriangle(buff, triangle, subBoxBeginX, subBoxEndX);
                }

                subBoxBeginX += _boxWidth;
                subBoxEndX += _boxWidth;
            }

            double d = sw.ElapsedMilliseconds;
            int f = 4;
        }

        static List<VertexPositionNormalTexture[]>[] SortTriangles(
            VertexPositionNormalTexture[][] groupedTriangles,
            float subBoxBegin,
            float subBoxEnd){

            var retList = new List<VertexPositionNormalTexture[]>[3];

            #region anon methods
            Func<VertexPositionNormalTexture[], float, float, int> numEnclosedVerts =
                (triangle, start, end) => {
                    int tot = 0;
                    foreach (var vert in triangle) {
                        if (vert.Position.X <= end && vert.Position.X >= start)
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
                            if (v1.Position.X > end && v2.Position.X < start)
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
                retList[numEnclosedVerts(triangle, subBoxBegin, subBoxEnd)].Add(triangle);
            }
            return retList;
        }

        void SliceZeroEnclosureTriangle(ObjectBuffer<HullSection> buff, VertexPositionNormalTexture[] triangle, float subBoxBegin, float subBoxEnd){
            //first have to figure out the "anchor vertex"
            VertexPositionNormalTexture anchor;
            VertexPositionNormalTexture[] satellites;
            float anchorBoundary, satelliteBoundary;

            ////if (triangle[0].Position.X > 3)
            //    return;

            var side1 = from vert in triangle
                where vert.Position.X > subBoxEnd
                select vert;
            var side2 = from vert in triangle
                where vert.Position.X < subBoxBegin
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

            var s1Pos = Lerp.Trace3X(satellites[0].Position, anchor.Position, satelliteBoundary);//idx=satt[0].val
            var s2Pos = Lerp.Trace3X(satellites[1].Position, anchor.Position, satelliteBoundary);//idx=satt[1].val

            var a1Pos = Lerp.Trace3X(satellites[0].Position, anchor.Position, anchorBoundary);//idx=anch.val
            var a2Pos = Lerp.Trace3X(satellites[1].Position, anchor.Position, anchorBoundary);//idx=anch.val

            var s1Norm = InterpolateNorm(satellites[0].Normal, anchor.Normal, satellites[0].Position, anchor.Position, s1Pos);
            var s2Norm = InterpolateNorm(satellites[1].Normal, anchor.Normal, satellites[1].Position, anchor.Position, s2Pos);

            var a1Norm = InterpolateNorm(satellites[0].Normal, anchor.Normal, satellites[0].Position, anchor.Position, a1Pos);
            var a2Norm = InterpolateNorm(satellites[1].Normal, anchor.Normal, satellites[1].Position, anchor.Position, a2Pos);

            var s1UV = InterpolateUV(satellites[0].TextureCoordinate, anchor.TextureCoordinate, satellites[0].Position, anchor.Position, s1Pos);
            var s2UV = InterpolateUV(satellites[1].TextureCoordinate, anchor.TextureCoordinate, satellites[1].Position, anchor.Position, s2Pos);

            var a1UV = InterpolateUV(satellites[0].TextureCoordinate, anchor.TextureCoordinate, satellites[0].Position, anchor.Position, a1Pos);
            var a2UV = InterpolateUV(satellites[1].TextureCoordinate, anchor.TextureCoordinate, satellites[1].Position, anchor.Position, a2Pos);

            HullSection id = new HullSection(subBoxBegin, subBoxEnd, 2);
            var s1 = new VertexPositionNormalTexture(s1Pos, s1Norm, s1UV);
            var s2 = new VertexPositionNormalTexture(s2Pos, s2Norm, s2UV);

            var a1 = new VertexPositionNormalTexture(a1Pos, a1Norm, a1UV);
            var a2 = new VertexPositionNormalTexture(a2Pos, a2Norm, a2UV);
            
            var t1 = new[] { s1, a1, s2 };
            var t2 = new[] { a2, s2, a1 };


            var t1I = GenerateIndiceList(t1);
            var t2I = GenerateIndiceList(t2);

            buff.AddObject(id, t1I, t1);
            buff.AddObject(id, t2I, t2);
        }

        int[] GenerateIndiceList(VertexPositionNormalTexture[] verts) {
            var cross = Vector3.Cross(verts[1].Position - verts[0].Position, verts[2].Position - verts[0].Position);

            switch (_side) {
                case Side.Right:
                    if (cross.Z > 0) {
                        return new[] { 0,2, 1 };
                    }
                    return new[] { 0, 1, 2 };
                case Side.Left:
                    if (cross.Z > 0) {
                        return new[] { 0,1, 2 };
                    }
                    return new[] { 0, 2, 1 };
            }
            Debug.Assert(false);
            return null;
        }


        Vector3 InterpolateNorm(Vector3 n1, Vector3 n2, Vector3 p1, Vector3 p2, Vector3 mid){
            float d1 = Vector3.Distance(p1, p2);
            float d2 = Vector3.Distance(p1, mid);
            float t = d2 / d1;
            return n1 * (1-t) + n2 * (t);
        }

        Vector2 InterpolateUV(Vector2 u1, Vector2 u2, Vector3 p1, Vector3 p2, Vector3 mid){
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


        public CullMode CullMode{
            set { HullBuff.CullMode = value; }
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator(){
            return HullBuff.GetEnumerator();
        }

        #endregion

    }
}