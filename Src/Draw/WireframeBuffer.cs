#region

using System;
using Gondola.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Gondola.Draw{
    internal class WireframeBuffer : BaseBufferObject<VertexPositionTexture>{
        /// <summary>
        /// this wireframe buffer expects the same parameters of those passed to a trianglelist buffer
        /// </summary>
        /// <param name="numIndicies"></param>
        /// <param name="numVerticies"></param>
        /// <param name="numPrimitives"></param>
        public WireframeBuffer(int numIndicies, int numVerticies, int numPrimitives)
            : base(numIndicies*2, numVerticies, numPrimitives*3 , PrimitiveType.LineList){
            BufferRasterizer = new RasterizerState();
            BufferRasterizer.CullMode = CullMode.None;
            BufferEffect = Gbl.ContentManager.Load<Effect>(Gbl.RawLookup["Std_WireframeShader"]);

            BufferEffect.Parameters["Projection"].SetValue(Gbl.ProjectionMatrix);
            BufferEffect.Parameters["World"].SetValue(Matrix.Identity);
        }

        public void SetData(
            VertexPositionTexture[] verticies,
            int[] indicies
            ){
            //we need to explode the indice list from triangle list to line list
            int indiceListSz = indicies.Length;
            var newInds = new int[indiceListSz*2];

            int srcIdx = 0;
            for (int i = 0; i < indicies.Length*2; i+=6) {
                newInds[i] = indicies[srcIdx];
                newInds[i+1] = indicies[srcIdx+1];
                newInds[i + 2] = indicies[srcIdx + 1];
                newInds[i + 3] = indicies[srcIdx + 2];
                newInds[i + 4] = indicies[srcIdx + 2];
                newInds[i + 5] = indicies[srcIdx];
                srcIdx += 3;
            }

            Indexbuffer.SetData(newInds);
            Vertexbuffer.SetData(verticies);
        }
    }
}