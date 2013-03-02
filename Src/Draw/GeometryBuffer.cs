using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gondola.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gondola.Draw {
    internal class GeometryBuffer<T> : BaseGeometryBuffer<T> where T : struct {
        Vector3 _rotation;
        Vector3 _translation;

        public GeometryBuffer(
            int numIndicies,
            int numVerticies,
            int numPrimitives,
            string settingsFileName,
            PrimitiveType primitiveType = PrimitiveType.TriangleList,
            CullMode cullMode = CullMode.None
            )
            : base(numIndicies, numVerticies, numPrimitives, settingsFileName, primitiveType, cullMode) {
                _rotation = new Vector3();
                _translation = new Vector3();

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

        public void Translate(Vector3 diff){
            _translation += diff;
            BaseWorldMatrix = Matrix.Identity;
            BaseWorldMatrix *= Matrix.CreateRotationX(_rotation.X) * Matrix.CreateRotationY(_rotation.Y) * Matrix.CreateRotationZ(_rotation.Z);
            BaseWorldMatrix *= Matrix.CreateTranslation(_translation.X, _translation.Y, _translation.Z);
        }

        public void Rotate(Angle3 diff) {
            _rotation += diff.ToVec();
            BaseWorldMatrix = Matrix.Identity;
            BaseWorldMatrix *= Matrix.CreateRotationX(_rotation.X) * Matrix.CreateRotationY(_rotation.Y) * Matrix.CreateRotationZ(_rotation.Z);
            BaseWorldMatrix *= Matrix.CreateTranslation(_translation.X, _translation.Y, _translation.Z);
        }
    }
}
