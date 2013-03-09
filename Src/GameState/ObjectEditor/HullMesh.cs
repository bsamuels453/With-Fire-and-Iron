#region

using System;
using System.Collections;
using System.Collections.Generic;
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
        readonly ObjectBuffer<HullSection> _structureBuffer;
        ObjectBuffer<int> _fillBuffer;
        ObjectBuffer<int> _buff;

        public HullMesh( float boundingWidth, int[] indicies, VertexPositionNormalTexture[] verts) {
            //_structureVerts = new Vector3[layersPerDeck][];

            //_structureBuffer = new ObjectBuffer<HullSection>(indicies.Length, 2, 4, 6, "Shader_AirshipHull");
            var subDividedVerts = new List<VertexPositionNormalTexture>();

            var groupedPanels = new List<IEnumerable<VertexPositionNormalTexture>>();
            for (int panelIdx = 0; panelIdx < verts.Length; panelIdx += 4){
                groupedPanels.Add(verts.Skip(panelIdx).Take(4));
            }

            var sortedPanels = (from grp in groupedPanels
                                orderby grp.Min(vert => vert.Position.X) ascending
                                group grp by grp.ElementAt(0).Position.Y into layers
                                select layers.ToArray()).ToArray();

            var vertss = new List<VertexPositionNormalTexture>();
            foreach (var layer in sortedPanels){
                float layerMin = layer.Min(g => g.Min(vert => vert.Position.X));
                int bboxesSkipped = (int)(layerMin / boundingWidth);
                float partialBBox = 1-((layerMin / boundingWidth) - bboxesSkipped);

                for (int panelIdx = 0; panelIdx < 5; panelIdx++){


                }



                //var bottomRight = Lerp.Trace3X(_lowerPts[0], _lowerPts[1], rBound);
                var orderedPts = layer.ElementAt(0).OrderByDescending(vert => vert.Position.X).ToArray();
                
                var panelStartPt = orderedPts[2].Position.X < orderedPts[3].Position.X ? orderedPts[2].Position.X : orderedPts[3].Position.X;
                var panelEndPt = orderedPts[0].Position.X > orderedPts[1].Position.X ? orderedPts[1].Position.X : orderedPts[0].Position.X;
                float subStartPt = panelStartPt;
                float subEndPt = partialBBox * boundingWidth + layerMin;



                while (subEndPt < panelEndPt){
                    int gf = 5;

                    //var bottomRight = Lerp.Trace3X(_lowerPts[0], _lowerPts[1], rBound);
                    //var upperRight = Lerp.Trace3X(_upperPts[0], _upperPts[1], rBound);
                    //var upperLeft = Lerp.Trace3X(_upperPts[0], _upperPts[1], lBound);
                    //var bottomLeft = Lerp.Trace3X(_lowerPts[0], _lowerPts[1], lBound);
                    /*
                    vertss.Add(new VertexPositionNormalTexture(bottomRight, new Vector3(0, 0, 0), new Vector2(0, 0)));
                    vertss.Add(new VertexPositionNormalTexture(upperRight, new Vector3(0, 0, 0), new Vector2(0, 0)));
                    vertss.Add(new VertexPositionNormalTexture(upperLeft, new Vector3(0, 0, 0), new Vector2(0, 0)));

                    vertss.Add(new VertexPositionNormalTexture(upperLeft, new Vector3(0, 0, 0), new Vector2(0, 0)));
                    vertss.Add(new VertexPositionNormalTexture(bottomLeft, new Vector3(0, 0, 0), new Vector2(0, 0)));
                    vertss.Add(new VertexPositionNormalTexture(bottomRight, new Vector3(0, 0, 0), new Vector2(0, 0)));
                    */

                    subEndPt += boundingWidth;
                }
                //panelIdx++;
                orderedPts = layer.ElementAt(0).OrderByDescending(vert => vert.Position.X).ToArray();
                panelEndPt = orderedPts[0].Position.X > orderedPts[1].Position.X ? orderedPts[1].Position.X : orderedPts[0].Position.X;

                int f = 4;
            }


            int idcIdx = 0;
            int vertIdx = 0;
            for (int obj = 0; obj < indicies.Length/6; obj++){
                var subInds = new int[6];
                var subVerts = new VertexPositionNormalTexture[4];

                Array.Copy(indicies, idcIdx, subInds, 0, 6);
                Array.Copy(verts, vertIdx, subVerts, 0, 4);

                for (int i = 0; i < 6; i++){
                    subInds[i] = subInds[i] - obj*4;
                }

                //_structureBuffer.AddObject(new HullSection(subVerts), subInds, subVerts);

                idcIdx += 6;
                vertIdx += 4;
            }
        }

        void SubdividePanel(ObjectBuffer<HullSection> buff, VertexPositionNormalTexture[] panelVerts, float width) {
            HullSection.Side side = panelVerts[0].Position.Z > 0 ? HullSection.Side.Right : HullSection.Side.Left;

            var upperPts = new Vector3[2];
            var lowerPts = new Vector3[2];

            if (panelVerts[0].Position.X < panelVerts[1].Position.X) {
                upperPts[0] = panelVerts[0].Position;
                upperPts[1] = panelVerts[1].Position;
            }
            else {
                upperPts[0] = panelVerts[1].Position;
                upperPts[1] = panelVerts[0].Position;
            }
            if (panelVerts[2].Position.X < panelVerts[3].Position.X) {
                lowerPts[0] = panelVerts[2].Position;
                lowerPts[1] = panelVerts[3].Position;
            }
            else {
                lowerPts[0] = panelVerts[3].Position;
                lowerPts[1] = panelVerts[2].Position;
            }

            float frontMin = lowerPts[0].X < upperPts[0].X ? lowerPts[0].X : upperPts[0].X;
            float frontMax = lowerPts[0].X > upperPts[0].X ? lowerPts[0].X : upperPts[0].X;

            float backMin = lowerPts[1].X < upperPts[1].X ? lowerPts[1].X : upperPts[1].X;
            float backMax = lowerPts[1].X > upperPts[1].X ? lowerPts[1].X : upperPts[1].X;

            float subBoxStart = 0;
            while (subBoxStart + width < frontMin) {
                subBoxStart += width;
            }
            float subBoxEnd = subBoxStart + width;

            {
                if (subBoxEnd > lowerPts[0].X && subBoxEnd < upperPts[0].X ||
                    subBoxEnd < lowerPts[0].X && subBoxEnd > upperPts[0].X) {
                    //box terminates between the near points

                    //generate geometry (triangle) :bound to subBoxend

                    //generate partial quad bound from subBoxEnd to frontMax
                }
                else {
                    //the only other option is that the box terminates in the body of the panel

                    //generate geometry (triangle) frontMin to frontMax
                    //generate quad bound from (end of triangle) to subBoxEnd
                }
            }

            {
                while (subBoxStart + width < backMin) {
                    subBoxStart += width;
                    subBoxEnd += width;

                    //generate intermediate quads
                }
            }

            {
                subBoxStart += width;
                subBoxEnd += width;
                //generate quad from subBoxStart to backMin

                if (subBoxEnd > lowerPts[1].X && subBoxEnd < upperPts[1].X ||
                    subBoxEnd < lowerPts[1].X && subBoxEnd > upperPts[1].X) {

                    //generate partial quad from backMin to subBoxEnd
                    //generate triangle from subBoxEnd to backMax

                }
                else{
                    //the box terminates in the next panel
                    //generate triangle from backMin to backMax
                }

            }


        }

        List<VertexPositionNormalTexture> GenEdgeQuad(
            VertexPositionNormalTexture[] upperPts, //0 is lower, 1 is higher
            VertexPositionNormalTexture[] lowerPts,
            float cuttingLine,
            bool useNearPts = true){

            var ret = new List<VertexPositionNormalTexture>();
            Vector3 p1, p2, p3, p4;
            if (useNearPts) {
                if (lowerPts[0].Position.X < upperPts[0].Position.X) {
                    p1 = lowerPts[0].Position;
                    p2 = Lerp.Trace3X(upperPts[1].Position, upperPts[0].Position, lowerPts[0].Position.X);
                    p3 = Lerp.Trace3X(upperPts[1].Position, upperPts[0].Position, cuttingLine);
                    p4 = Lerp.Trace3X(lowerPts[0].Position, upperPts[0].Position, cuttingLine);
                }
                else {
                    p1 = upperPts[0].Position;
                    p2 = Lerp.Trace3X(upperPts[0].Position, lowerPts[0].Position, cuttingLine);
                    p3 = Lerp.Trace3X(lowerPts[1].Position, lowerPts[0].Position, cuttingLine);
                    p4 = Lerp.Trace3X(lowerPts[1].Position, lowerPts[0].Position, upperPts[0].Position.X);
                }
            }
            else{
                if (lowerPts[1].Position.X < upperPts[1].Position.X) {
                    p1 = Lerp.Trace3X(upperPts[1].Position, upperPts[0].Position, cuttingLine);
                    p2 = Lerp.Trace3X(upperPts[1].Position, upperPts[0].Position, lowerPts[0].Position.X);
                    p3 = lowerPts[1].Position;
                    p4 = Lerp.Trace3X(upperPts[1].Position, lowerPts[1].Position, cuttingLine);
                }
                else{
                    p1 = upperPts[1].Position;
                    p2 = Lerp.Trace3X(lowerPts[1].Position, lowerPts[0].Position, upperPts[1].Position.X);
                    p3 = Lerp.Trace3X(lowerPts[1].Position, lowerPts[0].Position, cuttingLine);
                    p4 = Lerp.Trace3X(lowerPts[1].Position, upperPts[1].Position, cuttingLine);
                }
            }

            ret.Add(new VertexPositionNormalTexture(p1, new Vector3(), new Vector2()));
            ret.Add(new VertexPositionNormalTexture(p2, new Vector3(), new Vector2()));
            ret.Add(new VertexPositionNormalTexture(p3, new Vector3(), new Vector2()));
            ret.Add(new VertexPositionNormalTexture(p4, new Vector3(), new Vector2()));
            return ret;
        }



        public CullMode CullMode{
            set { _structureBuffer.CullMode = value; }
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator(){
            return _structureBuffer.GetEnumerator();
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
    
