#region

using System;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;

#endregion

namespace Forge.Framework.Draw{
    public class GeometryBuffer<T> : BaseGeometryBuffer<T> where T : struct{
        Vector3 _position;
        Vector3 _rotation;

        public GeometryBuffer(
            int numIndicies,
            int numVerticies,
            int numPrimitives,
            string settingsFileName,
            PrimitiveType primitiveType = PrimitiveType.TriangleList,
            CullMode cullMode = CullMode.None
            )
            : base(numIndicies, numVerticies, numPrimitives, settingsFileName, primitiveType, cullMode){
            Rotation = new Vector3();
            Position = new Vector3();
        }

        public Vector3 Rotation{
            get { return _rotation; }
            set{
                _rotation = value;
                UpdateWorldTransform();
            }
        }

        public Vector3 Position{
            get { return _position; }
            set{
                _position = value;
                UpdateWorldTransform();
            }
        }

        public Matrix WorldTransform{
            get { return BaseWorldTransform; }
            set { BaseWorldTransform = value; }
        }

        public CullMode CullMode{
            set { Rasterizer = new RasterizerState{CullMode = value}; }
        }

        public void ApplyTransform(Func<T, T> transform, bool updateSynchronously = true){
            var vertData = base.DumpVertexBuffer();
            for (int i = 0; i < vertData.Length; i++){
                vertData[i] = transform.Invoke(vertData[i]);
            }
            SetVertexBufferData(vertData, updateSynchronously);
        }

        public new void SetIndexBufferData(int[] data, bool updateSynchronously = true){
            base.SetIndexBufferData(data, updateSynchronously);
        }

        public new void SetVertexBufferData(T[] data, bool updateSynchronously = true){
            base.SetVertexBufferData(data, updateSynchronously);
        }

        /*
        public T[] DumpVerticies(){
            T[] data = new T[BaseVertexBuffer.VertexCount];
            base.BaseVertexBuffer.GetData(data);
            return data;
        }

        public int[] DumpIndicies(){
            int[] data = new int[BaseIndexBuffer.IndexCount];
            base.BaseIndexBuffer.GetData(data);
            return data;
        }
         */

        public void Translate(Vector3 diff){
            _position += diff;
            UpdateWorldTransform();
        }

        public void Rotate(Angle3 diff){
            _rotation += diff.ToVec();
            UpdateWorldTransform();
        }

        void UpdateWorldTransform(){
            BaseWorldTransform = Matrix.Identity;
            BaseWorldTransform *= Matrix.CreateRotationX(Rotation.X)*Matrix.CreateRotationY(Rotation.Y)*Matrix.CreateRotationZ(Rotation.Z);
            BaseWorldTransform *= Matrix.CreateTranslation(Position.X, Position.Y, Position.Z);
        }
    }
}