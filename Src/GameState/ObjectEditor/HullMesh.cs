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
        readonly ObjectBuffer<SectionIdentifier> _structureBuffer;
        ObjectBuffer<int> _fillBuffer;
        ObjectBuffer<int> _buff; 

        public HullMesh(int layersPerDeck, int[] indicies, VertexPositionNormalTexture[] verts){
            //_structureVerts = new Vector3[layersPerDeck][];

            _structureBuffer = new ObjectBuffer<SectionIdentifier>(indicies.Length, 2, 4, 6, "Shader_AirshipHull");
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

                _structureBuffer.AddObject(new SectionIdentifier(subVerts), subInds, subVerts);

                idcIdx += 6;
                vertIdx += 4;
            }
        }

        public CullMode CullMode{
            set { _structureBuffer.CullMode = value; }
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator(){
            return _structureBuffer.GetEnumerator();
        }

        #endregion

        public void Cut(Vector3 v){
            var toremove = new List<SectionIdentifier>();
            foreach (SectionIdentifier section in _structureBuffer){
                var bb = new List<BoundingBox>();
                bb.Add(new BoundingBox(v, v+new Vector3(0.6f,0,0)));
                List<VertexPositionNormalTexture> totVerts;
                VertexPositionNormalTexture[] verts;
                totVerts = new List<VertexPositionNormalTexture>();
                var verts2 = new List<VertexPositionNormalTexture>();

                if (section.ContainsPoint(new Vector2(v.X, v.Y))){
                    section.CutRectangle(bb, out verts, verts2);
                    totVerts.AddRange(verts);
                    toremove.Add(section);


                    //break;
                }
                if (totVerts.Count > 0) {
                    var inds = MeshHelper.CreateTriangleIndiceArray(totVerts.Count / 3);
                    _fillBuffer = new ObjectBuffer<int>(1, totVerts.Count / 3, totVerts.Count, inds.Count(), "Shader_AirshipHull");
                    _fillBuffer.AddObject(0, inds, totVerts.ToArray());
                }

                if (verts2.Count > 0) {
                    var inds = MeshHelper.CreateTriangleIndiceArray(verts2.Count / 3);
                    _buff = new ObjectBuffer<int>(1, verts2.Count / 3, verts2.Count, inds.Count(), "Shader_TintedModel");
                    _buff.AddObject(1, inds, verts2.ToArray());
                }

            }
            foreach (var identifier in toremove){
                _structureBuffer.DisableObject(identifier);
            }
        }

        #region Nested type: SectionIdentifier

        public class SectionIdentifier : IEquatable<SectionIdentifier>{
            readonly OverlapState _farOverlap;
            readonly Vector3[] _lowerPts;

            readonly OverlapState _nearOverlap;
            readonly Vector3[] _upperPts;

            public SectionIdentifier(VertexPositionNormalTexture[] verts){
                _upperPts = new Vector3[2];
                _lowerPts = new Vector3[2];

                if (verts[0].Position.X < verts[1].Position.X){
                    _upperPts[0] = verts[0].Position;
                    _upperPts[1] = verts[1].Position;
                }
                else{
                    _upperPts[0] = verts[1].Position;
                    _upperPts[1] = verts[0].Position;
                }
                if (verts[2].Position.X < verts[3].Position.X){
                    _lowerPts[0] = verts[2].Position;
                    _lowerPts[1] = verts[3].Position;
                }
                else{
                    _lowerPts[0] = verts[3].Position;
                    _lowerPts[1] = verts[2].Position;
                }

                //case: (this is hanging)
                // -------/ 
                //       /
                //      /
                // ----/
                //case: (this is building)
                // ----\
                //      \
                //       \
                // -------\
                _farOverlap = _upperPts[1].X > _lowerPts[1].X ? OverlapState.Hanging : OverlapState.Building;
                _nearOverlap = _upperPts[0].X > _lowerPts[0].X ? OverlapState.Building : OverlapState.Hanging;

                //bool b = ContainsPoint(new Vector2(41.0f, -0.5f));
                //int f = 4;
            }

            #region IEquatable<SectionIdentifier> Members

            public bool Equals(SectionIdentifier other){
                if (_lowerPts.SequenceEqual(other._lowerPts) && _upperPts.SequenceEqual(other._upperPts)){
                    return true;
                }
                return false;
            }

            #endregion

            public bool ContainsPoint(Vector2 p1){
                /*
                if (p1.Y > _upperPts[0].Y)
                    return false;
                if (p1.Y < _lowerPts[0].Y)
                    return false;
                */

                if (p1.X < _lowerPts[0].X && p1.X < _upperPts[0].X)
                    return false;

                if (p1.X > _lowerPts[1].X && p1.X > _upperPts[1].X)
                    return false;

                //at this point we know that the segment is within the orthogonal projection of this section

                if (p1.X > _lowerPts[0].X && p1.X > _upperPts[0].X && p1.X < _lowerPts[1].X && p1.X < _upperPts[1].X)
                    return true; //it's within the inner bounds

                var nearSample = DeterminePointPos(_lowerPts[0], _upperPts[0], p1, _nearOverlap);
                var farSample = DeterminePointPos(_lowerPts[1], _upperPts[1], p1, _farOverlap);

                if (nearSample == Side.RightSide && farSample == Side.LeftSide)
                    return true;
                return false;
            }

            Side DeterminePointPos(Vector3 v1, Vector3 v2, Vector2 p1, OverlapState state){
                var linePt = Lerp.Trace3X(v1, v2, p1.X);
                switch (state){
                    case OverlapState.Building:
                        if (linePt.Y < p1.Y)
                            return Side.RightSide;
                        return Side.LeftSide;
                    case OverlapState.Hanging:
                        if (linePt.Y < p1.Y)
                            return Side.RightSide;
                        return Side.LeftSide;
                }
                throw new Exception();
            }

            public void CutRectangle(List<BoundingBox> boxes, out VertexPositionNormalTexture[] verts, List<VertexPositionNormalTexture> verts2 ){
                var vertsLi = new List<VertexPositionNormalTexture>();

                var orderedBoxes = from b in boxes
                                   orderby b.Min.X
                                   select b;

                int boxesHandled = 0;

                int startIdx = 0;
                float nearStartPt = -1;
                float sliceMin = orderedBoxes.ElementAt(0).Min.X;
                float sliceMax = orderedBoxes.ElementAt(0).Max.X;
                if (sliceMin < _lowerPts[0].X && sliceMin < _upperPts[0].X) {
                    nearStartPt = NearBasedCut(sliceMin, sliceMax, vertsLi);
                    startIdx++;
                    boxesHandled++;
                }

                int endIdx = 0;
                float farStartPt = -1;
                sliceMin = orderedBoxes.Last().Min.X;
                sliceMax = orderedBoxes.Last().Max.X;
                if (sliceMax > _lowerPts[1].X && sliceMax > _upperPts[1].X) {
                    farStartPt = FarBasedCut(sliceMin, sliceMax, verts2);
                    endIdx++;
                    boxesHandled++;
                }

                if (boxesHandled == orderedBoxes.Count()){
                    //return;
                }

                var midBoxes = orderedBoxes.Skip(startIdx).Take(orderedBoxes.Count() - endIdx - startIdx);
                GenerateMidGeometry(nearStartPt, farStartPt, midBoxes.ToList(), vertsLi, verts2);

                verts = vertsLi.ToArray();
            }

            void GenerateMidGeometry(
                float centSliceStart,
                float centSliceEnd,
                List<BoundingBox> boxes,
                List<VertexPositionNormalTexture> verts, List<VertexPositionNormalTexture> verts2 ){
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (centSliceStart == -1){
                    //need to generate the near section
                    float pSliceStart = _upperPts[0].X < _lowerPts[0].X ? _upperPts[0].X : _lowerPts[0].X;

                    //potential slice end
                    float pSliceEnd = _upperPts[0].X > _lowerPts[0].X ? _upperPts[0].X : _lowerPts[0].X;
                    centSliceStart = pSliceEnd;
                    BoundingBox? bboxToRemove = null;
                    foreach (var box in boxes){
                        if (box.Min.X < pSliceEnd){
                            centSliceStart = box.Max.X;//might have to reverse
                            pSliceEnd = box.Min.Y;
                            bboxToRemove = box;
                            break;
                        }
                    }
                    if (bboxToRemove != null){
                        boxes.Remove((BoundingBox)bboxToRemove);
                    }
                    //gentri
                    verts.Add(new VertexPositionNormalTexture(new Vector3(pSliceEnd, _lowerPts[0].Y, _lowerPts[0].Z), new Vector3(0, 0, 0), new Vector2(0, 0)));
                    var v = Lerp.Trace3X(_upperPts[0], _upperPts[1], pSliceEnd);
                    verts.Add(new VertexPositionNormalTexture(new Vector3(pSliceEnd, _upperPts[0].Y, v.Z), new Vector3(0, 0, 0), new Vector2(0, 0)));
                    verts.Add(new VertexPositionNormalTexture(new Vector3(pSliceStart, _upperPts[0].Y, _upperPts[0].Z), new Vector3(0, 0, 0), new Vector2(0, 0)));
                    
                }
                if (centSliceEnd == -1) {
                    //need to generate the far section
                    float pSliceEnd = _upperPts[1].X > _lowerPts[1].X ? _upperPts[1].X : _lowerPts[1].X;

                    float pSliceBegin = _upperPts[1].X < _lowerPts[1].X ? _upperPts[1].X : _lowerPts[1].X;
                    centSliceEnd = pSliceBegin;

                    BoundingBox? bboxToRemove = null;
                    foreach (var box in boxes) {
                        if (box.Max.X > pSliceBegin) {
                            centSliceEnd = box.Min.X;
                            pSliceBegin = box.Max.X;
                            bboxToRemove = box;
                            break;
                        }
                    }
                    if (bboxToRemove != null) {
                        boxes.Remove((BoundingBox)bboxToRemove);
                    }
                    //gentri
                    verts.Add(new VertexPositionNormalTexture(new Vector3(pSliceEnd, _lowerPts[0].Y, _lowerPts[1].Z), new Vector3(0, 0, 0), new Vector2(0, 0)));
                    var v = Lerp.Trace3X(_lowerPts[0], _lowerPts[1], pSliceBegin);
                    verts.Add(new VertexPositionNormalTexture(new Vector3(pSliceBegin, _lowerPts[0].Y, v.Z), new Vector3(0, 0, 0), new Vector2(0, 0)));
                    verts.Add(new VertexPositionNormalTexture(new Vector3(pSliceBegin, _upperPts[0].Y, _upperPts[1].Z), new Vector3(0, 0, 0), new Vector2(0, 0)));
                     
                }

                float leftBound = centSliceStart;
                float rightBound = centSliceEnd;

                Func<float, float, bool> generateQuad = (rBound, lBound) => {
                    var bottomRight = Lerp.Trace3X(_lowerPts[0], _lowerPts[1], rBound);
                    var upperRight = Lerp.Trace3X(_upperPts[0], _upperPts[1], rBound);
                    var upperLeft = Lerp.Trace3X(_upperPts[0], _upperPts[1], lBound);
                    var bottomLeft = Lerp.Trace3X(_lowerPts[0], _lowerPts[1], lBound);

                    verts.Add(new VertexPositionNormalTexture(bottomRight, new Vector3(0, 0, 0), new Vector2(0, 0)));
                    verts.Add(new VertexPositionNormalTexture(upperRight, new Vector3(0, 0, 0), new Vector2(0, 0)));
                    verts.Add(new VertexPositionNormalTexture(upperLeft, new Vector3(0, 0, 0), new Vector2(0, 0)));

                    verts.Add(new VertexPositionNormalTexture(upperLeft, new Vector3(0, 0, 0), new Vector2(0, 0)));
                    verts.Add(new VertexPositionNormalTexture(bottomLeft, new Vector3(0, 0, 0), new Vector2(0, 0)));
                    verts.Add(new VertexPositionNormalTexture(bottomRight, new Vector3(0, 0, 0), new Vector2(0, 0)));
                    return true;
                };
                while (boxes.Count > 0){
                    float min = boxes.Min(bo => bo.Min.X);
                    var minBox = (
                                     from b in boxes
                                     where b.Min.X == min
                                     select b).First();
                    boxes.Remove(minBox);
                    rightBound = minBox.Min.X;
                    generateQuad(leftBound, rightBound);
                    leftBound = minBox.Max.X;
                    if (boxes.Count == 0){
                        rightBound = centSliceEnd;
                    }
                }
                generateQuad(leftBound, rightBound);
                // ReSharper restore CompareOfFloatsByEqualityOperator
            }


            float NearBasedCut(float sliceMin, float sliceMax, List<VertexPositionNormalTexture> verts) {
                //we know sliceMin is out of the picture
                //left to right
                
                if (sliceMax > _lowerPts[0].X && sliceMax < _upperPts[0].X ||
                    sliceMax < _lowerPts[0].X && sliceMax > _upperPts[0].X){
                    //near end clipped off
                    //verts.Add(new VertexPositionNormalTexture(new Vector3(0,0,0), new Vector3(0,0,0), new Vector2(0,0));
                    float cutEndPt = _lowerPts[0].X < _upperPts[0].X ? _lowerPts[0].X : _upperPts[0].X;
                    float cutStartPt = sliceMax;
                    var v1 = Lerp.Intersection(_lowerPts[0], _upperPts[0], new Vector3(sliceMin, 5, 0), new Vector3(sliceMin, 0, 0));

                                
                    //generate intermediate
                    return cutEndPt;
                }
                if (sliceMax > _lowerPts[0].X && sliceMax > _upperPts[0].X &&
                    sliceMax < _lowerPts[1].X && sliceMax < _upperPts[1].X){
                    //near half clipped off
                    //generate intermediate
                    return sliceMax;
                }
                if (sliceMax > _lowerPts[1].X && sliceMax < _upperPts[1].X ||
                    sliceMax < _lowerPts[1].X && sliceMax > _upperPts[1].X){
                    //near half engulfed
                    return sliceMax;
                }
                //entire engulfed?
                throw new Exception();
            }

            float FarBasedCut(float sliceMin, float sliceMax, List<VertexPositionNormalTexture> verts) {
                //we know sliceMax is out of the picture
                //right to left
                if (sliceMin > _lowerPts[1].X && sliceMin < _upperPts[1].X ||
                    sliceMin < _lowerPts[1].X && sliceMin > _upperPts[1].X){
                    //far end clipped off
                    //generate intermediate
                    float cutEndPt = sliceMin;
                    float cutStartPt = _lowerPts[1].X < _upperPts[1].X ? _lowerPts[1].X : _upperPts[1].X;
                    var v1 = Lerp.Trace3X(_lowerPts[1], _upperPts[1], cutEndPt);
                    Vector3 v2, v3, v4;
                    if (_lowerPts[1].X > _upperPts[1].X) {
                        //building
                        v2 = Lerp.Trace3X(_lowerPts[0], _lowerPts[1], cutEndPt);
                        var v3Z = Lerp.Trace3X(_lowerPts[0], _lowerPts[1], _upperPts[1].X).Z;
                        v3 = new Vector3(_upperPts[1].X, _lowerPts[1].Y, v3Z);
                        v4 = _upperPts[1];
                    }
                    else{
                        v2 = _lowerPts[1];
                        v3 = new Vector3(_lowerPts[1].X, _upperPts[1].Y, _upperPts[1].Z);
                        v4 = Lerp.Trace3X(_upperPts[0], _upperPts[1], cutStartPt);
                    }

                    verts.Add(new VertexPositionNormalTexture(v1, new Vector3(), new Vector2()));
                    verts.Add(new VertexPositionNormalTexture(v2, new Vector3(), new Vector2()));
                    verts.Add(new VertexPositionNormalTexture(v3, new Vector3(), new Vector2()));
                    verts.Add(new VertexPositionNormalTexture(v3, new Vector3(), new Vector2()));
                    verts.Add(new VertexPositionNormalTexture(v4, new Vector3(), new Vector2()));
                    verts.Add(new VertexPositionNormalTexture(v1, new Vector3(), new Vector2()));

                    return _lowerPts[1].X < _upperPts[1].X ? _lowerPts[1].X : _upperPts[1].X;
                }
                if (sliceMin < _lowerPts[1].X && sliceMin < _upperPts[1].X &&
                    sliceMin > _lowerPts[0].X && sliceMin > _upperPts[0].X ) {
                    //far half clipped off
                    return sliceMin;
                }
                if (sliceMin > _lowerPts[1].X && sliceMin < _upperPts[1].X ||
                    sliceMin < _lowerPts[1].X && sliceMin > _upperPts[1].X){
                    //far half engulfed
                    return sliceMin;
                }
                throw new Exception();
            }

            #region Nested type: OverlapState

            enum OverlapState{
                Hanging,
                Building
            }

            #endregion

            #region Nested type: Side

            enum Side{
                LeftSide,
                RightSide
            }

            #endregion
        }

        #endregion
    }
}
    
