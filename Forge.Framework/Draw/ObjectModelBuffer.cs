#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;

#endregion

namespace Forge.Framework.Draw{
    /// <summary>
    ///   this is nearly identical to ObjectBuffer with the exception that it's for handling non-geometric content (models)
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    public class ObjectModelBuffer<T> : IDrawableBuffer where T : IEquatable<T>{
        readonly DepthStencilState _depthStencil;
        readonly bool[] _isSlotOccupied;
        readonly int _maxObjects;
        readonly List<ObjectData> _objectData;
        readonly RasterizerState _rasterizer;
        readonly Effect _shader;
        public bool Enabled;
        public Matrix GlobalTransform;
        bool _disposed;

        public ObjectModelBuffer(int maxObjects, string shader){
            Resource.LoadShader(shader, out _shader, out _rasterizer, out _depthStencil);
            _objectData = new List<ObjectData>();
            _maxObjects = maxObjects;
            _isSlotOccupied = new bool[maxObjects];
            GlobalTransform = Matrix.Identity;
            Enabled = true;
            RenderTarget.Buffers.Add(this);
        }

        public EffectParameterCollection ShaderParams{
            get { return _shader.Parameters; }
        }

        #region IDrawableBuffer Members

        public void Draw(Matrix viewMatrix){
            if (!Enabled)
                return;
            Resource.Device.RasterizerState = _rasterizer;
            Resource.Device.DepthStencilState = _depthStencil;

            foreach (var obj in _objectData){
                if (!obj.Enabled)
                    continue;
                foreach (var mesh in obj.Model.Meshes){
                    foreach (var part in mesh.MeshParts){
                        part.Effect = _shader;
                    }
                    foreach (var effect in mesh.Effects){
                        effect.Parameters["mtx_Projection"].SetValue(Resource.ProjectionMatrix);
                        effect.Parameters["mtx_World"].SetValue(obj.Transform*GlobalTransform);
                        effect.Parameters["mtx_View"].SetValue(viewMatrix);
                    }
                    mesh.Draw();
                }
            }
        }

        #endregion

        public void AddObject(IEquatable<T> identifier, Model model, Matrix transform){
            int index = -1;
            for (int i = 0; i < _maxObjects; i++){
                if (_isSlotOccupied[i] == false){
                    _objectData.Add(new ObjectData(identifier, i, transform, model));
                    _isSlotOccupied[i] = true;
                    index = i;
                    break;
                }
            }
            Debug.Assert(index != -1, "not enough space in object buffer to add new object");
        }

        public void RemoveObject(IEquatable<T> identifier){
            ObjectData objectToRemove = (
                from obj in _objectData
                where obj.Identifier.Equals(identifier)
                select obj
                ).FirstOrDefault();

            if (objectToRemove == null)
                return;

            _isSlotOccupied[objectToRemove.ObjectOffset] = false;
            _objectData.Remove(objectToRemove);
        }

        public void ClearObjects(){
            _objectData.Clear();
            for (int i = 0; i < _maxObjects; i++){
                _isSlotOccupied[i] = false;
            }
        }

        public bool EnableObject(IEquatable<T> identifier){
            ObjectData objToEnable = null;
            foreach (var obj in _objectData){
                if (obj.Identifier.Equals(identifier)){
                    objToEnable = obj;
                }
            }
            if (objToEnable == null)
                return false;

            objToEnable.Enabled = true;
            return true;
        }

        public bool DisableObject(IEquatable<T> identifier){
            ObjectData objToDisable = null;
            foreach (var obj in _objectData){
                if (obj.Identifier.Equals(identifier)){
                    objToDisable = obj;
                }
            }
            if (objToDisable == null)
                return false;

            objToDisable.Enabled = false;
            return true;
        }

        public void TransformAll(Vector3 transform){
            GlobalTransform = Matrix.CreateTranslation(transform);
        }

        /// <summary>
        ///   really cool method that will take another objectbuffer and absorb its objects into this objectbuffer. also clears the other buffer afterwards.
        /// </summary>
        public void AbsorbBuffer(ObjectModelBuffer<T> buffer){
            foreach (var objectData in buffer._objectData){
                bool isDuplicate = false;
                foreach (var data in _objectData){
                    if (data.Identifier.Equals(objectData.Identifier))
                        isDuplicate = true;
                }
                if (isDuplicate)
                    continue;
                AddObject(objectData.Identifier, objectData.Model, objectData.Transform);
            }
            buffer.ClearObjects();
        }

        public void SetObjectTransform(IEquatable<T> identifier, Matrix transform){
            var objLi = from t in _objectData
                where t.Identifier.Equals(identifier)
                select t;

            Debug.Assert(objLi.Count() > 0);

            foreach (var obj in objLi){
                obj.Transform = transform;
            }
        }

        public void Dispose(){
            if (!_disposed){
                RenderTarget.Buffers.Remove(this);
                _disposed = true;
            }
        }

        ~ObjectModelBuffer(){
            if (!_disposed)
                throw new ResourceNotDisposedException();
        }

        #region Nested type: ObjectData

        class ObjectData{
            public readonly IEquatable<T> Identifier;
            public readonly Model Model;
            public readonly int ObjectOffset;
            public bool Enabled;
            public Matrix Transform;

            public ObjectData(IEquatable<T> identifier, int objectOffset, Matrix transform, Model model){
                Enabled = true;
                Identifier = identifier;
                ObjectOffset = objectOffset;
                Model = model;
                Transform = transform;
            }
        }

        #endregion
    }
}