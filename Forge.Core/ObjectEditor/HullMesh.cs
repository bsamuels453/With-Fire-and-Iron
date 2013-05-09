#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
                groupedPanels.Add(verts.Skip(panelIdx).Take(4));
            }

            var sortedPanels = (from grp in groupedPanels
                                orderby grp.Min(vert => vert.Position.X) ascending
                                group grp by grp.ElementAt(0).Position.Y into layers
                                select layers.ToArray()).ToArray();

            _panelLookup = new Dictionary<float, int>(5);
            foreach (var layer in sortedPanels.Reverse()) {
                _panelLookup.Add(layer[0].ElementAt(2).Position.Y, _panelLookup.Count);
            }

            //xxx need better estimation for number of objects
            var tempBuff = new ObjectBuffer<HullSection>((int)(400*5/boundingWidth), 1, 3, 3, "Shader_AirshipHull");
            tempBuff.UpdateBufferManually = true;

            foreach (var layer in sortedPanels){
                foreach (var quad in layer){
                    SubdividePanel(tempBuff, quad.ToArray());
                }
            }

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

        void SubdividePanel(ObjectBuffer<HullSection> buff, VertexPositionNormalTexture[] panelVerts){

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