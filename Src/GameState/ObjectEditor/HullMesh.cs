#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Gondola.Draw;
using Gondola.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Gondola.GameState.ObjectEditor{
    /// <summary>
    ///   This class handles the meshes that make up the hull on each deck of the airship. This class is used for projecting shapes into the mesh to allow for objects such as portholes. Each deck's hull is broken in two parts split down the center.
    /// </summary>
    internal class HullMesh : IEnumerable{
        readonly ObjectBuffer<HullSection> _hullBuff;

        public HullMesh( float boundingWidth, VertexPositionNormalTexture[] verts) {
            var groupedPanels = new List<IEnumerable<VertexPositionNormalTexture>>();
            for (int panelIdx = 0; panelIdx < verts.Length; panelIdx += 4){
                groupedPanels.Add(verts.Skip(panelIdx).Take(4));
            }

            var sortedPanels = (from grp in groupedPanels
                                orderby grp.Min(vert => vert.Position.X) ascending
                                group grp by grp.ElementAt(0).Position.Y into layers
                                select layers.ToArray()).ToArray();

            //xxx need better estimation for number of objects
            var tempBuff = new ObjectBuffer<HullSection>((int)(400*5/boundingWidth), 1, 3, 3, "Shader_AirshipHull");
            tempBuff.UpdateBufferManually = true;

            foreach (var layer in sortedPanels){
                foreach (var quad in layer){
                    SubdividePanel(tempBuff, quad.ToArray(), boundingWidth);
                }
            }

            _hullBuff = new ObjectBuffer<HullSection>(tempBuff.ActiveObjects, 1, 3, 3, "Shader_AirshipHull");
            _hullBuff.CullMode = CullMode.None;
            _hullBuff.AbsorbBuffer(tempBuff, true);
        }

        void SubdividePanel(ObjectBuffer<HullSection> buff, VertexPositionNormalTexture[] panelVerts, float width){
            var side = panelVerts[0].Position.Z > 0 ? HullSection.Side.Right : HullSection.Side.Left;
            var upperPts = new VertexPositionNormalTexture[2];
            var lowerPts = new VertexPositionNormalTexture[2];

            if (panelVerts[0].Position.X < panelVerts[1].Position.X){
                upperPts[0] = panelVerts[0];
                upperPts[1] = panelVerts[1];
            }
            else{
                upperPts[0] = panelVerts[1];
                upperPts[1] = panelVerts[0];
            }
            if (panelVerts[2].Position.X < panelVerts[3].Position.X){
                lowerPts[0] = panelVerts[2];
                lowerPts[1] = panelVerts[3];
            }
            else{
                lowerPts[0] = panelVerts[3];
                lowerPts[1] = panelVerts[2];
            }

            float frontMin = lowerPts[0].Position.X < upperPts[0].Position.X ?
                lowerPts[0].Position.X : upperPts[0].Position.X;

            float backMin = lowerPts[1].Position.X < upperPts[1].Position.X ?
                lowerPts[1].Position.X : upperPts[1].Position.X;

            float subBoxStart = 0;
            while (subBoxStart + width < frontMin){
                subBoxStart += width;
            }
            float subBoxEnd = subBoxStart + width;

            GenerateFrontSection(
                side,
                buff,
                ref subBoxStart,
                ref subBoxEnd,
                width,
                lowerPts,
                upperPts
                );

            while (subBoxEnd + width < backMin){
                subBoxStart += width;
                subBoxEnd += width;
                GenerateMidQuads(buff, side, width, upperPts, lowerPts, subBoxStart, subBoxEnd);
            }
            GenerateMidQuads(buff, side, width, upperPts, lowerPts, subBoxEnd, backMin);

            GenerateEndSection(
                side,
                buff,
                ref subBoxStart,
                ref subBoxEnd,
                width,
                lowerPts,
                upperPts
                );
        }

        #region generation
        void GenerateFrontSection(
            HullSection.Side side,
            ObjectBuffer<HullSection> buff, 
            ref float subBoxStart, 
            ref float subBoxEnd, 
            float width,
            VertexPositionNormalTexture[] lowerPts,
            VertexPositionNormalTexture[] upperPts) {

            float frontMin = lowerPts[0].Position.X < upperPts[0].Position.X ?
                lowerPts[0].Position.X : upperPts[0].Position.X;
            float frontMax = lowerPts[0].Position.X > upperPts[0].Position.X ?
                lowerPts[0].Position.X : upperPts[0].Position.X;

            Func<float, float, HullSection> genSection = (f, f1) =>
                new HullSection(
                    side: side,
                    xStart: f,
                    xEnd: f1
                );

            var curBox = genSection(subBoxStart, subBoxEnd);
            var nextBox = genSection(subBoxEnd, subBoxEnd + width);

            var idxWinding = new[] { 0, 1, 2 };
            if (subBoxEnd > lowerPts[0].Position.X && subBoxEnd < upperPts[0].Position.X ||
                subBoxEnd < lowerPts[0].Position.X && subBoxEnd > upperPts[0].Position.X) {
                //box terminates between the near points

                //generate triangle bound to subBoxend
                var edgeTri = GenEdgeTriangle(upperPts, lowerPts, subBoxEnd, true);
                buff.AddObject(curBox, (int[])idxWinding.Clone(), edgeTri.ToArray());

                //generate partial quad bound from subBoxEnd to frontMax
                var edgeQuad = GenEdgeQuad(upperPts, lowerPts, subBoxEnd, true);
                buff.AddObject(nextBox, (int[])idxWinding.Clone(), edgeQuad[0].ToArray());
                buff.AddObject(nextBox, (int[])idxWinding.Clone(), edgeQuad[1].ToArray());
            }
            else {
                //the only other option is that the box terminates in the body of the panel
                //generate triangle frontMin to frontMax

                Debug.Assert(subBoxEnd > frontMax);
                var edgeTri = GenEdgeTriangle(upperPts, lowerPts, frontMax, true);
                buff.AddObject(curBox, (int[])idxWinding.Clone(), edgeTri.ToArray());

                //generate quad bound from (end of triangle) to subBoxEnd
                var quad = GenIntermediateQuad(upperPts, lowerPts, frontMax, subBoxEnd);
                buff.AddObject(curBox, (int[])idxWinding.Clone(), quad[0].ToArray());
                buff.AddObject(curBox, (int[])idxWinding.Clone(), quad[1].ToArray());
            }

            bool first = true;
            //move subBoxEnd to after frontMax
            while (subBoxEnd < frontMax) {
                if (first) {
                    var edgeTri = GenEdgeTriangle(upperPts, lowerPts, subBoxEnd, true);
                    buff.AddObject(curBox, (int[])idxWinding.Clone(), edgeTri.ToArray());
                    first = false;
                }
                else {
                    //xx this will give overlapping geometry to multiple boxes because bounds for genedgeQuad cant be specified
                    var edgeQuad = GenEdgeQuad(upperPts, lowerPts, subBoxEnd, true);
                    buff.AddObject(curBox, (int[])idxWinding.Clone(), edgeQuad[0].ToArray());
                    buff.AddObject(curBox, (int[])idxWinding.Clone(), edgeQuad[1].ToArray());
                }
                subBoxStart += width;
                subBoxEnd += width;
                curBox = genSection(subBoxStart, subBoxEnd);
            }

            if (subBoxEnd > frontMax && subBoxStart < frontMax && subBoxStart > frontMin) {
                //generate cross boxes
                var eedgeQuad = GenEdgeQuad(upperPts, lowerPts, frontMax, true);
                buff.AddObject(curBox, (int[])idxWinding.Clone(), eedgeQuad[0].ToArray());
                buff.AddObject(curBox, (int[])idxWinding.Clone(), eedgeQuad[1].ToArray());
                GenerateMidQuads(buff, side, width, upperPts, lowerPts, frontMax, subBoxEnd);
            }
        }

        void GenerateEndSection(
            HullSection.Side side,
            ObjectBuffer<HullSection> buff,
            ref float subBoxStart,
            ref float subBoxEnd,
            float width,
            VertexPositionNormalTexture[] lowerPts,
            VertexPositionNormalTexture[] upperPts) {

            var idxWinding = new[] { 0, 1, 2 };
            float backMin = lowerPts[1].Position.X < upperPts[1].Position.X ?
                lowerPts[1].Position.X : upperPts[1].Position.X;

            Func<float, float, HullSection> genSection = (f, f1) =>
                new HullSection(
                    side: side,
                    xStart: f,
                    xEnd: f1
                );

            var curBox = genSection(subBoxStart, subBoxEnd);
            var nextBox = genSection(subBoxEnd, subBoxEnd + width);

            if (subBoxEnd > lowerPts[1].Position.X && subBoxEnd < upperPts[1].Position.X ||
                subBoxEnd < lowerPts[1].Position.X && subBoxEnd > upperPts[1].Position.X) {

                //generate partial quad from backMin to subBoxEnd
                var quad = GenEdgeQuad(upperPts, lowerPts, subBoxEnd, false);
                buff.AddObject(curBox, (int[])idxWinding.Clone(), quad[0].ToArray());
                buff.AddObject(curBox, (int[])idxWinding.Clone(), quad[1].ToArray());

                //generate triangle from subBoxEnd to backMax
                var edgeTri = GenEdgeTriangle(upperPts, lowerPts, subBoxEnd, false);
                buff.AddObject(nextBox, (int[])idxWinding.Clone(), edgeTri.ToArray());
            }
            else {
                //the box terminates in the next panel
                //generate triangle from backMin to backMax
                var edgeTri = GenEdgeTriangle(upperPts, lowerPts, backMin, false);
                buff.AddObject(curBox, (int[])idxWinding.Clone(), edgeTri.ToArray());
            }
        }
        #endregion

        #region primitive interpolation
        void GenerateMidQuads(
            ObjectBuffer<HullSection> buff, 
            HullSection.Side side, 
            float width, 
            VertexPositionNormalTexture[] upperPts,
            VertexPositionNormalTexture[] lowerPts, 
            float start, 
            float end){

            var idxWinding = new [] { 0, 1, 2 };
            var curBox = new HullSection(
                side: side,
                xStart: start,
                xEnd: start + width
                );

            //generate intermediate quads
            var quad = GenIntermediateQuad(upperPts, lowerPts, start, end);
            buff.AddObject(curBox, (int[]) idxWinding.Clone(), quad[0].ToArray());
            buff.AddObject(curBox, (int[]) idxWinding.Clone(), quad[1].ToArray());
        }

        List<VertexPositionNormalTexture>[] GenIntermediateQuad(
            VertexPositionNormalTexture[] upperPts,
            VertexPositionNormalTexture[] lowerPts,
            float begin,
            float end){

            var ret = new List<VertexPositionNormalTexture>[2];
            ret[0] = new List<VertexPositionNormalTexture>();
            ret[1] = new List<VertexPositionNormalTexture>();

            Vector3 p1 = Lerp.Trace3X(upperPts[0].Position, upperPts[1].Position, begin);
            Vector3 p2 = Lerp.Trace3X(upperPts[0].Position, upperPts[1].Position, end);
            Vector3 p3 = Lerp.Trace3X(lowerPts[0].Position, lowerPts[1].Position, end);
            Vector3 p4 = Lerp.Trace3X(lowerPts[0].Position, lowerPts[1].Position, begin);

            Vector3 n1 = InterpolateNorm(upperPts[0].Normal, upperPts[1].Normal, upperPts[0].Position, upperPts[1].Position, p1);
            Vector3 n2 = InterpolateNorm(upperPts[0].Normal, upperPts[1].Normal, upperPts[0].Position, upperPts[1].Position, p2);
            Vector3 n3 = InterpolateNorm(lowerPts[0].Normal, lowerPts[1].Normal, lowerPts[0].Position, lowerPts[1].Position, p3);
            Vector3 n4 = InterpolateNorm(lowerPts[0].Normal, lowerPts[1].Normal, lowerPts[0].Position, lowerPts[1].Position, p4);

            Vector2 u1 = InterpolateUV(upperPts[0].TextureCoordinate, upperPts[1].TextureCoordinate, upperPts[0].Position, upperPts[1].Position, p1);
            Vector2 u2 = InterpolateUV(upperPts[0].TextureCoordinate, upperPts[1].TextureCoordinate, upperPts[0].Position, upperPts[1].Position, p2);
            Vector2 u3 = InterpolateUV(lowerPts[0].TextureCoordinate, lowerPts[1].TextureCoordinate, lowerPts[0].Position, lowerPts[1].Position, p3);
            Vector2 u4 = InterpolateUV(lowerPts[0].TextureCoordinate, lowerPts[1].TextureCoordinate, lowerPts[0].Position, lowerPts[1].Position, p4);
            /*
            if (p1.Z > 0){
                if (p2.Z < 0 || p3.Z < 0 || p4.Z < 0){
                    int f = 3;
                }
            }
            else {
                if (p2.Z > 0 || p3.Z > 0 || p4.Z > 0) {
                    int f = 3;
                }
            }
             */
            ret[0].Add(new VertexPositionNormalTexture(p1, n1, u1));
            ret[0].Add(new VertexPositionNormalTexture(p2, n2, u2));
            ret[0].Add(new VertexPositionNormalTexture(p3, n3, u3));
            ret[1].Add(new VertexPositionNormalTexture(p3, n3, u3));
            ret[1].Add(new VertexPositionNormalTexture(p4, n4, u4));
            ret[1].Add(new VertexPositionNormalTexture(p1, n1, u1));
            /*
            ret[0].Add(new VertexPositionNormalTexture(p1, Vector3.Zero, Vector2.Zero));
            ret[0].Add(new VertexPositionNormalTexture(p2, Vector3.Zero, Vector2.Zero));
            ret[0].Add(new VertexPositionNormalTexture(p3, Vector3.Zero, Vector2.Zero));
            ret[1].Add(new VertexPositionNormalTexture(p3, Vector3.Zero, Vector2.Zero));
            ret[1].Add(new VertexPositionNormalTexture(p4, Vector3.Zero, Vector2.Zero));
            ret[1].Add(new VertexPositionNormalTexture(p1, Vector3.Zero, Vector2.Zero));
            */
            return ret;
        }

        List<VertexPositionNormalTexture>[] GenEdgeQuad(
            VertexPositionNormalTexture[] upperPts,
            VertexPositionNormalTexture[] lowerPts,
            float cuttingLine,
            bool useNearPts){

            var ret = new List<VertexPositionNormalTexture>[2];
            ret[0] = new List<VertexPositionNormalTexture>();
            ret[1] = new List<VertexPositionNormalTexture>();

            Vector3 p1, p2, p3, p4;
            Vector3 n1, n2, n3, n4;
            Vector2 u1, u2, u3, u4;
            if (useNearPts) {
                if (lowerPts[0].Position.X < upperPts[0].Position.X) {
                    p1 = upperPts[0].Position;
                    p2 = Lerp.Trace3X(lowerPts[0].Position, lowerPts[1].Position, upperPts[0].Position.X);
                    p3 = Lerp.Trace3X(lowerPts[0].Position, lowerPts[1].Position, cuttingLine);
                    p4 = Lerp.Trace3X(upperPts[0].Position, lowerPts[0].Position, cuttingLine);
                    
                    n1 = upperPts[0].Normal;
                    n2 = InterpolateNorm(lowerPts[0].Normal, lowerPts[1].Normal, lowerPts[0].Position, lowerPts[1].Position, p4);
                    n3 = InterpolateNorm(lowerPts[0].Normal, lowerPts[1].Normal, lowerPts[0].Position, lowerPts[1].Position, p3);
                    n4 = InterpolateNorm(upperPts[0].Normal, lowerPts[0].Normal, upperPts[0].Position, lowerPts[0].Position, p2);
                    
                    u1 = upperPts[0].TextureCoordinate;
                    u2 = InterpolateUV(lowerPts[0].TextureCoordinate, lowerPts[1].TextureCoordinate, lowerPts[0].Position, lowerPts[1].Position, p4);
                    u3 = InterpolateUV(lowerPts[0].TextureCoordinate, lowerPts[1].TextureCoordinate, lowerPts[0].Position, lowerPts[1].Position, p3);
                    u4 = InterpolateUV(upperPts[0].TextureCoordinate, lowerPts[0].TextureCoordinate, upperPts[0].Position, lowerPts[0].Position, p2);
                }
                else {
                    p1 = Lerp.Trace3X(upperPts[1].Position, upperPts[0].Position, lowerPts[0].Position.X);
                    p2 = lowerPts[0].Position;
                    p3 = Lerp.Trace3X(lowerPts[0].Position, upperPts[0].Position, cuttingLine);
                    p4 = Lerp.Trace3X(upperPts[1].Position, upperPts[0].Position, cuttingLine);

                    n1 = InterpolateNorm(upperPts[0].Normal, upperPts[1].Normal, upperPts[0].Position, upperPts[1].Position, p2);
                    n2 = lowerPts[0].Normal;
                    n3 = InterpolateNorm(lowerPts[0].Normal, upperPts[0].Normal, lowerPts[0].Position, upperPts[0].Position, p4);
                    n4 = InterpolateNorm(upperPts[0].Normal, upperPts[1].Normal, upperPts[0].Position, upperPts[1].Position, p3);

                    u1 = InterpolateUV(upperPts[0].TextureCoordinate, upperPts[1].TextureCoordinate, upperPts[0].Position, upperPts[1].Position, p2);
                    u2 = lowerPts[0].TextureCoordinate;
                    u3 = InterpolateUV(lowerPts[0].TextureCoordinate, upperPts[0].TextureCoordinate, lowerPts[0].Position, upperPts[0].Position, p4);
                    u4 = InterpolateUV(upperPts[0].TextureCoordinate, upperPts[1].TextureCoordinate, upperPts[0].Position, lowerPts[1].Position, p3);
                }
            }
            else{
                if (lowerPts[1].Position.X < upperPts[1].Position.X) {
                    p1 = upperPts[1].Position;
                    p2 = Lerp.Trace3X(lowerPts[1].Position, lowerPts[0].Position, upperPts[1].Position.X);
                    p3 = Lerp.Trace3X(lowerPts[1].Position, lowerPts[0].Position, cuttingLine);
                    p4 = Lerp.Trace3X(lowerPts[1].Position, upperPts[1].Position, cuttingLine);

                    n1 = upperPts[1].Normal;
                    n2 = InterpolateNorm(lowerPts[0].Normal, lowerPts[1].Normal, lowerPts[0].Position, lowerPts[1].Position, p2);
                    n3 = InterpolateNorm(lowerPts[0].Normal, lowerPts[1].Normal, lowerPts[0].Position, lowerPts[1].Position, p3);
                    n4 = InterpolateNorm(lowerPts[0].Normal, upperPts[1].Normal, lowerPts[0].Position, upperPts[1].Position, p4);

                    u1 = upperPts[1].TextureCoordinate;
                    u2 = InterpolateUV(lowerPts[0].TextureCoordinate, lowerPts[1].TextureCoordinate, lowerPts[0].Position, lowerPts[1].Position, p2);
                    u3 = InterpolateUV(lowerPts[0].TextureCoordinate, lowerPts[1].TextureCoordinate, lowerPts[0].Position, lowerPts[1].Position, p3);
                    u4 = InterpolateUV(lowerPts[0].TextureCoordinate, upperPts[1].TextureCoordinate, lowerPts[0].Position, upperPts[1].Position, p4);
                }
                else{
                    p1 = Lerp.Trace3X(upperPts[1].Position, upperPts[0].Position, cuttingLine);
                    p2 = Lerp.Trace3X(upperPts[1].Position, upperPts[0].Position, lowerPts[0].Position.X);
                    p3 = lowerPts[1].Position;
                    p4 = Lerp.Trace3X(upperPts[1].Position, lowerPts[1].Position, cuttingLine);

                    n1 = InterpolateNorm(upperPts[0].Normal, upperPts[1].Normal, upperPts[0].Position, upperPts[1].Position, p1);
                    n2 = InterpolateNorm(upperPts[0].Normal, upperPts[1].Normal, upperPts[0].Position, upperPts[1].Position, p2);
                    n3 = lowerPts[1].Normal;
                    n4 = InterpolateNorm(upperPts[1].Normal, lowerPts[1].Normal, upperPts[1].Position, lowerPts[1].Position, p4);

                    u1 = InterpolateUV(upperPts[0].TextureCoordinate, upperPts[1].TextureCoordinate, upperPts[0].Position, upperPts[1].Position, p1);
                    u2 = InterpolateUV(upperPts[0].TextureCoordinate, upperPts[1].TextureCoordinate, upperPts[0].Position, upperPts[1].Position, p2);
                    u3 = lowerPts[1].TextureCoordinate;
                    u4 = InterpolateUV(upperPts[1].TextureCoordinate, lowerPts[1].TextureCoordinate, upperPts[1].Position, lowerPts[1].Position, p4);
                }
            }

            //1 2 3 3 4 1
            
            ret[0].Add(new VertexPositionNormalTexture(p1, n1, u1));
            ret[0].Add(new VertexPositionNormalTexture(p2, n2, u2));
            ret[0].Add(new VertexPositionNormalTexture(p3, n3, u3));
            ret[1].Add(new VertexPositionNormalTexture(p3, n3, u3));
            ret[1].Add(new VertexPositionNormalTexture(p4, n4, u4));
            ret[1].Add(new VertexPositionNormalTexture(p1, n1, u1));
            /*
            ret[0].Add(new VertexPositionNormalTexture(p1, Vector3.Zero, Vector2.Zero));
            ret[0].Add(new VertexPositionNormalTexture(p2, Vector3.Zero, Vector2.Zero));
            ret[0].Add(new VertexPositionNormalTexture(p3, Vector3.Zero, Vector2.Zero));
            ret[1].Add(new VertexPositionNormalTexture(p3, Vector3.Zero, Vector2.Zero));
            ret[1].Add(new VertexPositionNormalTexture(p4, Vector3.Zero, Vector2.Zero));
            ret[1].Add(new VertexPositionNormalTexture(p1, Vector3.Zero, Vector2.Zero));
            */
            return ret;
        }

        List<VertexPositionNormalTexture> GenEdgeTriangle(
            VertexPositionNormalTexture[] upperPts,
            VertexPositionNormalTexture[] lowerPts,
            float cuttingLine,
            bool useNearPts){

            var ret = new List<VertexPositionNormalTexture>();
            Vector3 p1, p2, p3;
            Vector3 n1 = Vector3.Zero, n2 = Vector3.Zero, n3 = Vector3.Zero;
            Vector2 u1, u2, u3;
            if (useNearPts) {
                if (lowerPts[0].Position.X < upperPts[0].Position.X) {
                    p1 = lowerPts[0].Position;
                    p2 = Lerp.Trace3X(lowerPts[0].Position, upperPts[0].Position, cuttingLine);
                    p3 = Lerp.Trace3X(lowerPts[0].Position, lowerPts[1].Position, cuttingLine);
                    
                    n1 = lowerPts[0].Normal;
                    n2 = InterpolateNorm(lowerPts[0].Normal, upperPts[0].Normal, lowerPts[0].Position, upperPts[0].Position, p2);
                    n3 = InterpolateNorm(lowerPts[0].Normal, lowerPts[1].Normal, lowerPts[0].Position, lowerPts[1].Position, p2);
                    
                    u1 = lowerPts[0].TextureCoordinate;
                    u2 = InterpolateUV(lowerPts[0].TextureCoordinate, upperPts[0].TextureCoordinate, lowerPts[0].Position, upperPts[0].Position, p2);
                    u3 = InterpolateUV(lowerPts[0].TextureCoordinate, lowerPts[1].TextureCoordinate, lowerPts[0].Position, lowerPts[1].Position, p2);
                }
                else {
                    p1 = Lerp.Trace3X(upperPts[0].Position, lowerPts[0].Position, cuttingLine);
                    p2 = upperPts[0].Position;
                    p3 = Lerp.Trace3X(upperPts[0].Position, upperPts[1].Position, cuttingLine);

                    n1 = InterpolateNorm(upperPts[0].Normal, lowerPts[0].Normal, upperPts[0].Position, lowerPts[0].Position, p2);
                    n2 = upperPts[0].Normal;
                    n3 = InterpolateNorm(upperPts[0].Normal, upperPts[1].Normal, upperPts[0].Position, upperPts[1].Position, p2);
                    
                    u1 = InterpolateUV(upperPts[0].TextureCoordinate, lowerPts[0].TextureCoordinate, upperPts[0].Position, lowerPts[0].Position, p2);
                    u2 = upperPts[0].TextureCoordinate;
                    u3 = InterpolateUV(upperPts[0].TextureCoordinate, upperPts[1].TextureCoordinate, upperPts[0].Position, upperPts[1].Position, p2);
                }
            }
            else {
                if (lowerPts[1].Position.X < upperPts[1].Position.X) {
                    p1 = upperPts[1].Position;
                    p2 = Lerp.Trace3X(upperPts[1].Position, lowerPts[1].Position, cuttingLine);
                    p3 = Lerp.Trace3X(upperPts[1].Position, upperPts[0].Position, cuttingLine);
                    
                    n1 = upperPts[1].Normal;
                    n2 = InterpolateNorm(upperPts[1].Normal, lowerPts[1].Normal, upperPts[1].Position, lowerPts[1].Position, p2);
                    n3 = InterpolateNorm(upperPts[0].Normal, upperPts[1].Normal, upperPts[0].Position, upperPts[1].Position, p2);
                    
                    u1 = upperPts[1].TextureCoordinate;
                    u2 = InterpolateUV(upperPts[1].TextureCoordinate, lowerPts[1].TextureCoordinate, lowerPts[1].Position, lowerPts[1].Position, p2);
                    u3 = InterpolateUV(upperPts[0].TextureCoordinate, upperPts[1].TextureCoordinate, upperPts[0].Position, upperPts[1].Position, p2);
                }
                else {
                    p1 = lowerPts[1].Position;
                    p2 = Lerp.Trace3X(lowerPts[0].Position, lowerPts[1].Position, cuttingLine);
                    p3 = Lerp.Trace3X(lowerPts[1].Position, upperPts[1].Position, cuttingLine);

                    n1 = lowerPts[1].Normal;
                    n2 = InterpolateNorm(lowerPts[0].Normal, lowerPts[1].Normal, lowerPts[0].Position, lowerPts[1].Position, p2);
                    n3 = InterpolateNorm(lowerPts[1].Normal, upperPts[1].Normal, lowerPts[1].Position, upperPts[1].Position, p2);
                    
                    u1 = lowerPts[1].TextureCoordinate;
                    u2 = InterpolateUV(lowerPts[0].TextureCoordinate, lowerPts[1].TextureCoordinate, lowerPts[0].Position, lowerPts[1].Position, p2);
                    u3 = InterpolateUV(lowerPts[1].TextureCoordinate, upperPts[1].TextureCoordinate, lowerPts[1].Position, upperPts[1].Position, p2);
                }
            }
            
            ret.Add(new VertexPositionNormalTexture(p1, n1, u1));
            ret.Add(new VertexPositionNormalTexture(p2, n2, u2));
            ret.Add(new VertexPositionNormalTexture(p3, n3, u3));
            /*
            ret.Add(new VertexPositionNormalTexture(p1, Vector3.Zero, Vector2.Zero));
            ret.Add(new VertexPositionNormalTexture(p2, Vector3.Zero, Vector2.Zero));
            ret.Add(new VertexPositionNormalTexture(p3, Vector3.Zero, Vector2.Zero));
            */
            return ret;
        }

        Vector3 InterpolateNorm(Vector3 n1, Vector3 n2, Vector3 p1, Vector3 p2, Vector3 mid){
            float d1 = Vector3.Distance(p1, p2);
            float d2 = Vector3.Distance(p1, mid);
            float t = d2 / d1;
            return n1 * t + n2 * (1 - t);
        }

        Vector2 InterpolateUV(Vector2 u1, Vector2 u2, Vector3 p1, Vector3 p2, Vector3 mid){
            float d1 = Vector3.Distance(p1, p2);
            float d2 = Vector3.Distance(p1, mid);
            float t = d2 / d1;
            return u1 * t + u2 * (1 - t);
        }


        #endregion

        public CullMode CullMode{
            set { _hullBuff.CullMode = value; }
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator(){
            return _hullBuff.GetEnumerator();
        }

        #endregion

        #region Nested type: HullSection

        public class HullSection : IEquatable<HullSection>{
            public enum Side{
                Left,
                Right
            }

            readonly float _xStart;
            readonly float _xEnd;
            readonly Side _side;

            public HullSection(Side side, float xStart, float xEnd){
                _xStart = xStart;
                _xEnd = xEnd;
                _side = side;
            }

            #region IEquatable<HullSection> Members
            // ReSharper disable CompareOfFloatsByEqualityOperator
            public bool Equals(HullSection other){
                if (_xStart == other._xStart && _xEnd == other._xEnd && _side == other._side) {
                    return true;
                }
                return false;
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator
            #endregion
        }

        #endregion
    }
}
    
