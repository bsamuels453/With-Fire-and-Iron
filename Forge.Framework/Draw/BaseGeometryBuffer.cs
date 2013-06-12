#region

using System;
using System.Threading.Tasks;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;

#endregion

namespace Forge.Framework.Draw{
    public abstract class BaseGeometryBuffer<T> : IDrawableBuffer, IDisposable where T : struct{
        protected readonly string ShaderName;
        readonly IndexBuffer _baseIndexBuffer;
        readonly VertexBuffer _baseVertexBuffer;
        readonly int _numIndicies;
        readonly int _numPrimitives;
        readonly PrimitiveType _primitiveType;
        protected Matrix BaseWorldTransform;

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
            BaseWorldTransform = Matrix.Identity;

            Rasterizer = new RasterizerState{CullMode = cullMode};

            lock (Resource.Device){
                _baseIndexBuffer = new IndexBuffer
                    (
                    Resource.Device,
                    typeof (int),
                    numIndicies,
                    BufferUsage.None
                    );

                _baseVertexBuffer = new VertexBuffer
                    (
                    Resource.Device,
                    typeof (T),
                    numVerticies,
                    BufferUsage.None
                    );
            }
            ShaderName = shader;
            Resource.LoadShader(shader, out Shader);
            Shader.Parameters["mtx_Projection"].SetValue(Resource.ProjectionMatrix);
            Shader.Parameters["mtx_World"].SetValue(Matrix.Identity);

            RenderTarget.Buffers.Add(this);
        }

        public EffectParameterCollection ShaderParams{
            get { return Shader.Parameters; }
        }

        #region IDisposable Members

        public void Dispose(){
            if (!_disposed){
                RenderTarget.Buffers.Remove(this);
                _baseIndexBuffer.Dispose();
                _baseVertexBuffer.Dispose();
                _disposed = true;
            }
        }

        #endregion

        #region IDrawableBuffer Members

        public void Draw(Matrix viewMatrix){
            if (Enabled){
                Shader.Parameters["mtx_View"].SetValue(viewMatrix);
                Shader.Parameters["mtx_World"].SetValue(BaseWorldTransform);
                Resource.Device.RasterizerState = Rasterizer;

                foreach (EffectPass pass in Shader.CurrentTechnique.Passes){
                    pass.Apply();
                    Resource.Device.Indices = _baseIndexBuffer;
                    Resource.Device.SetVertexBuffer(_baseVertexBuffer);
                    Resource.Device.DrawIndexedPrimitives(_primitiveType, 0, 0, _numIndicies, 0, _numPrimitives);
                }
                Resource.Device.SetVertexBuffer(null);
            }
        }

        #endregion

        protected void SetIndexBufferData(int[] data, bool updateSynchronously = false){
            if (updateSynchronously) {
                _baseIndexBuffer.SetData(data);
            }
            else{
                var update = new Task
                    (() =>{
                         lock (Resource.Device){
                             if (!_disposed){
                                 _baseIndexBuffer.SetData((int[]) data.Clone());
                             }
                         }
                     });
                RenderTarget.AddAsynchronousBufferUpdate(update);
            }
        }

        protected void SetVertexBufferData(T[] data, bool updateSynchronously = false) {
            if (updateSynchronously) {
                _baseVertexBuffer.SetData(data);
            }
            else{
                var update = new Task
                    (() =>{
                         lock (Resource.Device){
                             if (!_disposed){
                                 _baseVertexBuffer.SetData((T[]) data.Clone());
                             }
                         }
                     });
                RenderTarget.AddAsynchronousBufferUpdate(update);
            }

        }

        /// <summary>
        /// Gets the data currently stored in the index buffer. WILL NOT ACCOUNT FOR ASYNCHRONOUS BUFFER UPDATES THAT ARE NOT COMPLETED.
        /// </summary>
        /// <returns></returns>
        protected int[] DumpIndexBuffer(){
            var data = new int[_numIndicies];
            _baseIndexBuffer.GetData(data);
            return data;
        }

        /// <summary>
        /// Gets the data currently stored in the vertex buffer. WILL NOT ACCOUNT FOR ASYNCHRONOUS BUFFER UPDATES THAT ARE NOT COMPLETED.
        /// </summary>
        /// <returns></returns>
        protected T[] DumpVertexBuffer() {
            var data = new T[_numIndicies];
            _baseVertexBuffer.GetData(data);
            return data;
        }

        ~BaseGeometryBuffer(){
            if (!_disposed)
                throw new ResourceNotDisposedException();
        }
    }
}