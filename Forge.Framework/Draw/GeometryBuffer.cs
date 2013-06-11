#region

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
                UpdateWorldMatrix();
            }
        }

        public Vector3 Position{
            get { return _position; }
            set{
                _position = value;
                UpdateWorldMatrix();
            }
        }

        public Matrix WorldMatrix{
            get { return BaseWorldMatrix; }
            set { BaseWorldMatrix = value; }
        }

        public void SetIndexBufferData(int[] data){
            BaseIndexBuffer.SetData(data);
        }

        public void SetVertexBufferData(T[] data){
            BaseVertexBuffer.SetData(data);
        }

        public CullMode CullMode{
            set { Rasterizer = new RasterizerState{CullMode = value}; }
        }

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

        public void Translate(Vector3 diff){
            _position += diff;
            UpdateWorldMatrix();
        }

        public void Rotate(Angle3 diff){
            _rotation += diff.ToVec();
            UpdateWorldMatrix();
        }

        void UpdateWorldMatrix(){
            BaseWorldMatrix = Matrix.Identity;
            BaseWorldMatrix *= Matrix.CreateRotationX(Rotation.X)*Matrix.CreateRotationY(Rotation.Y)*Matrix.CreateRotationZ(Rotation.Z);
            BaseWorldMatrix *= Matrix.CreateTranslation(Position.X, Position.Y, Position.Z);
        }
    }
}