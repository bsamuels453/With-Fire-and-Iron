#region

using System;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;

#endregion

namespace Forge.Framework.Draw{
    public abstract class BaseGeometryBuffer<T> : IDrawableBuffer, IDisposable{
        protected readonly IndexBuffer BaseIndexBuffer;
        protected readonly VertexBuffer BaseVertexBuffer;
        protected readonly string ShaderName;
        readonly int _numIndicies;
        readonly int _numPrimitives;
        readonly PrimitiveType _primitiveType;
        protected Matrix BaseWorldMatrix;

        public bool Enabled;
        protected RasterizerState Rasterizer;
        protected Effect Shader;
        bool _disposed;

        protected BaseGeometryBuffer(int numIndicies, int numVerticies, int numPrimitives, string shader, PrimitiveType primitiveType,
            CullMode cullMode = CullMode.None){
            Enabled = true;
            _numPrimitives = numPrimitives;
            _numIndicies = numIndicies;
            _primitiveType = primitiveType;
            BaseWorldMatrix = Matrix.Identity;

            Rasterizer = new RasterizerState{CullMode = cullMode};

            BaseIndexBuffer = new IndexBuffer
                (
                Resource.Device,
                typeof (int),
                numIndicies,
                BufferUsage.None
                );

            BaseVertexBuffer = new VertexBuffer
                (
                Resource.Device,
                typeof (T),
                numVerticies,
                BufferUsage.None
                );

            ShaderName = shader;
            Resource.LoadShader(shader, out Shader);
            Shader.Parameters["Projection"].SetValue(Resource.ProjectionMatrix);
            Shader.Parameters["World"].SetValue(Matrix.Identity);

            RenderTarget.Buffers.Add(this);
        }

        public EffectParameterCollection ShaderParams{
            get { return Shader.Parameters; }
        }

        #region IDisposable Members

        public void Dispose(){
            if (!_disposed){
                RenderTarget.Buffers.Remove(this);
                BaseIndexBuffer.Dispose();
                BaseVertexBuffer.Dispose();
                _disposed = true;
            }
        }

        #endregion

        #region IDrawableBuffer Members

        public void Draw(Matrix viewMatrix){
            if (Enabled){
                Shader.Parameters["View"].SetValue(viewMatrix);
                Shader.Parameters["World"].SetValue(BaseWorldMatrix);
                Resource.Device.RasterizerState = Rasterizer;

                foreach (EffectPass pass in Shader.CurrentTechnique.Passes){
                    pass.Apply();
                    Resource.Device.Indices = BaseIndexBuffer;
                    Resource.Device.SetVertexBuffer(BaseVertexBuffer);
                    Resource.Device.DrawIndexedPrimitives(_primitiveType, 0, 0, _numIndicies, 0, _numPrimitives);
                }
                Resource.Device.SetVertexBuffer(null);
            }
        }

        #endregion

        ~BaseGeometryBuffer(){
            if (!_disposed)
                throw new ResourceNotDisposedException();
        }
    }
}