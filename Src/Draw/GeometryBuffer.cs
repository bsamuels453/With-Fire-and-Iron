using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Gondola.Draw {
    internal class GeometryBuffer<T> : BaseGeometryBuffer<T> where T : struct {
        public GeometryBuffer(
            int numIndicies,
            int numVerticies,
            int numPrimitives,
            string settingsFileName,
            PrimitiveType primitiveType = PrimitiveType.TriangleList,
            CullMode cullMode = CullMode.None
            )
            : base(numIndicies, numVerticies, numPrimitives, settingsFileName, primitiveType, cullMode) {

        }

        public IndexBuffer IndexBuffer {
            get { return base.BaseIndexBuffer; }
        }

        public VertexBuffer VertexBuffer {
            get { return base.BaseVertexBuffer; }
        }

        public CullMode CullMode {
            set { Rasterizer = new RasterizerState { CullMode = value }; }
        }

        public T[] DumpVerticies() {
            T[] data = new T[BaseVertexBuffer.VertexCount];
            base.BaseVertexBuffer.GetData(data);
            return data;
        }

        public int[] DumpIndicies() {
            int[] data = new int[BaseIndexBuffer.IndexCount];
            base.BaseIndexBuffer.GetData(data);
            return data;
        }
    }
}
