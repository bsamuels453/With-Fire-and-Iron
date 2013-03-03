#region

using System;
using Gondola.Draw;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Gondola.GameState.ObjectEditor{
    /// <summary>
    ///   This class handles the meshes that make up the hull on each deck of the airship. This class is used for projecting shapes into the mesh to allow for objects such as portholes. Each deck's hull is broken in two parts split down the center.
    /// </summary>
    internal class HullMesh{
        readonly ObjectBuffer<Vector3> _structureBuffer;
        ObjectBuffer<Vector3> _fillBuffer;
        Vector3[][] _structureVerts;

        public HullMesh(int layersPerDeck, int[] indicies, VertexPositionNormalTexture[] verts){
            _structureVerts = new Vector3[layersPerDeck][];

            _structureBuffer = new ObjectBuffer<Vector3>(indicies.Length, 2, 4, 6, "Shader_AirshipHull");
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

                _structureBuffer.AddObject(Vector3.Zero, subInds, subVerts);

                idcIdx += 6;
                vertIdx += 4;
            }
        }
    }
}