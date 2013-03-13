#region

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Forge.Framework.Draw{
    public abstract class BaseGeometryBuffer<T> : IDrawableBuffer, IDisposable{
        protected readonly IndexBuffer BaseIndexBuffer;
        protected readonly VertexBuffer BaseVertexBuffer;
        readonly int _numIndicies;
        readonly int _numPrimitives;
        readonly PrimitiveType _primitiveType;
        protected Matrix BaseWorldMatrix;

        public bool Enabled;
        protected RasterizerState Rasterizer;
        protected Effect Shader;
        bool _isDisposed;

        protected BaseGeometryBuffer(int numIndicies, int numVerticies, int numPrimitives, string shader, PrimitiveType primitiveType, CullMode cullMode = CullMode.None){
            Enabled = true;
            _numPrimitives = numPrimitives;
            _numIndicies = numIndicies;
            _primitiveType = primitiveType;
            BaseWorldMatrix = Matrix.Identity;

            Rasterizer = new RasterizerState{CullMode = cullMode};

            BaseIndexBuffer = new IndexBuffer(
                Gbl.Device,
                typeof (int),
                numIndicies,
                BufferUsage.None
                );

            BaseVertexBuffer = new VertexBuffer(
                Gbl.Device,
                typeof (T),
                numVerticies,
                BufferUsage.None
                );

            Gbl.LoadShader(shader, out Shader);
            Shader.Parameters["Projection"].SetValue(Gbl.ProjectionMatrix);
            Shader.Parameters["World"].SetValue(Matrix.Identity);

            RenderTarget.Buffers.Add(this);
        }

        public EffectParameterCollection ShaderParams{
            get { return Shader.Parameters; }
        }

        #region IDisposable Members

        public void Dispose(){
            if (!_isDisposed){
                RenderTarget.Buffers.Remove(this);
                BaseIndexBuffer.Dispose();
                BaseVertexBuffer.Dispose();
                _isDisposed = true;
            }
        }

        #endregion

        #region IDrawableBuffer Members

        public void Draw(Matrix viewMatrix){
            if (Enabled){
                Shader.Parameters["View"].SetValue(viewMatrix);
                Shader.Parameters["World"].SetValue(BaseWorldMatrix);
                Gbl.Device.RasterizerState = Rasterizer;

                foreach (EffectPass pass in Shader.CurrentTechnique.Passes){
                    pass.Apply();
                    Gbl.Device.Indices = BaseIndexBuffer;
                    Gbl.Device.SetVertexBuffer(BaseVertexBuffer);
                    Gbl.Device.DrawIndexedPrimitives(_primitiveType, 0, 0, _numIndicies, 0, _numPrimitives);
                }
                Gbl.Device.SetVertexBuffer(null);
            }
        }

        #endregion

        ~BaseGeometryBuffer(){
            if (!_isDisposed){
                //throw new Exception("Dispose your buffers, scrub");
            }
        }
    }
}