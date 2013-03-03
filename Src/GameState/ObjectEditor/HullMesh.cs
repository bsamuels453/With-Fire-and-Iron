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
        ObjectBuffer<SectionIdentifier> _fillBuffer;
        Vector3[][] _structureVerts;//necessary?

        public HullMesh(int layersPerDeck, int[] indicies, VertexPositionNormalTexture[] verts){
            _structureVerts = new Vector3[layersPerDeck][];

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
            foreach (SectionIdentifier section in _structureBuffer){
                if (section.ContainsPoint(new Vector2(v.X, v.Y)))
                    _structureBuffer.DisableObject(section);
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

            public void CutRectangle(BoundingBox box, out VertexPositionNormalTexture[] verts, out int[] inds){
                var indsLi = new List<int>();
                var vertsLi = new List<VertexPositionNormalTexture>();
                Vector2 max = new Vector2(box.Max.Y, box.Max.Y);
                Vector2 min = new Vector2(box.Min.X, box.Min.Y);
                float sliceMin, sliceMax;
                sliceMin = min.X;
                sliceMax  = max.X;

                //starting @ min
                float progressive = 0;
                //left to right
                if (sliceMin < _lowerPts[0].X && sliceMin < _upperPts[0].X){
                    //we know sliceMin is out of the picture
                    if (sliceMax > _lowerPts[0].X && sliceMax < _upperPts[0].X ||
                        sliceMax < _lowerPts[0].X && sliceMax > _upperPts[0].X){
                        //near end clipped off
                        //generate intermediate
                        progressive = _lowerPts[0].X > _upperPts[0].X ? _lowerPts[0].X : _upperPts[0].X;
                        //return
                    }
                    if (sliceMax > _lowerPts[0].X && sliceMax > _upperPts[0].X &&
                        sliceMax < _lowerPts[1].X && sliceMax < _upperPts[1].X) {
                        //near half clipped off
                        //generate intermediate
                        progressive = sliceMax;
                        //return
                    }
                    if (sliceMax > _lowerPts[1].X && sliceMax < _upperPts[1].X ||
                        sliceMax < _lowerPts[1].X && sliceMax > _upperPts[1].X){
                        //near half engulfed
                        progressive = sliceMax;
                        //return
                    }
                    //entire engulfed?
                    //return
                }

                //opposite render direction
                //right to left
                if (sliceMax < _lowerPts[1].X && sliceMax < _upperPts[1].X){
                    //we know sliceMax is out of the picture
                    if (sliceMin > _lowerPts[1].X && sliceMin < _upperPts[1].X ||
                        sliceMin < _lowerPts[1].X && sliceMin > _upperPts[1].X) {
                        //far end clipped off
                        //generate intermediate
                        progressive = _lowerPts[1].X < _upperPts[1].X ? _lowerPts[1].X : _upperPts[1].X;
                        //return
                    }
                    if (sliceMin > _lowerPts[0].X && sliceMin > _upperPts[0].X &&
                        sliceMin < _lowerPts[1].X && sliceMin < _upperPts[1].X) {
                        //far half clipped off
                        //generate intermediate
                        progressive = sliceMin;
                        //return
                    }
                    if (sliceMin > _lowerPts[0].X && sliceMin < _upperPts[0].X ||
                        sliceMin < _lowerPts[0].X && sliceMin > _upperPts[0].X) {
                        //far half engulfed
                        progressive = sliceMin;
                        //return
                    }
                    //entire engulfed?
                    //return
                }

                //now the center outlier
                if (sliceMin > _lowerPts[0].X && sliceMin < _upperPts[0].X ||
                    sliceMin < _lowerPts[0].X && sliceMin > _upperPts[0].X){
                    //slicemin is between near

                    if(sliceMax < _lowerPts[1].X && sliceMax < _upperPts[1].X){
                        //cut from between near to middle
                        //return
                    }

                    if (sliceMax > _lowerPts[1].X && sliceMax < _upperPts[1].X ||
                        sliceMax < _lowerPts[1].X && sliceMax > _upperPts[1].X){
                        //cut from between near to between far
                        //return
                    }
                    //return
                }

                if (sliceMax > _lowerPts[1].X && sliceMax < _upperPts[1].X ||
                    sliceMax < _lowerPts[1].X && sliceMax > _upperPts[1].X){
                    //slicemax is between far

                    if (sliceMin > _lowerPts[0].X && sliceMin > _upperPts[0].X){
                        //cut from between far to middle
                        //return
                    }
                    //dont need cut between near and between far because covered above
                    //return
                }

                if (sliceMin > _lowerPts[0].X && sliceMin > _upperPts[0].X &&
                    sliceMax < _lowerPts[1].X && sliceMax < _upperPts[1].X){
                    //center cut
                }


                verts = null;
                inds = null;
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