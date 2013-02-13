﻿#region

using System;
using Gondola.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Gondola.Draw{
    internal class WireframeBuffer : BaseBufferObject<VertexPositionColor>{
        public WireframeBuffer(int numIndicies, int numVerticies, int numPrimitives)
            : base(numIndicies, numVerticies, numPrimitives, PrimitiveType.LineList){
            BufferRasterizer = new RasterizerState();
            BufferRasterizer.CullMode = CullMode.None;
            BufferEffect = Gbl.ContentManager.Load<Effect>(Gbl.RawLookup["WireframeEffect"]).Clone();

            BufferEffect.Parameters["Projection"].SetValue(Gbl.ProjectionMatrix);
            BufferEffect.Parameters["World"].SetValue(Matrix.Identity);
        }
    }
}