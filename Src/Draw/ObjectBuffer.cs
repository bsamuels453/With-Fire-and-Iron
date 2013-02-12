#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Gondola.Draw{
    internal class ObjectBuffer<T> : StandardEffect where T : IEquatable<T>{
        //key=identifier
        readonly int[] _indicies;
        readonly int _indiciesPerObject;
        readonly bool[] _isSlotOccupied;
        readonly int _maxObjects;
        readonly List<ObjectData> _objectData;
        readonly VertexPositionNormalTexture[] _verticies;
        readonly int _verticiesPerObject;

        public bool UpdateBufferManually;

        public ObjectBuffer(int maxObjects, int primitivesPerObject, int verticiesPerObject, int indiciesPerObject, string textureName) :
            base(indiciesPerObject*maxObjects, verticiesPerObject*maxObjects, primitivesPerObject*maxObjects, textureName){
            BufferRasterizer = new RasterizerState{CullMode = CullMode.None};

            _objectData = new List<ObjectData>();
            _indicies = new int[maxObjects*indiciesPerObject];
            _verticies = new VertexPositionNormalTexture[maxObjects*verticiesPerObject];
            _indiciesPerObject = indiciesPerObject;
            _verticiesPerObject = verticiesPerObject;
            _maxObjects = maxObjects;
            _isSlotOccupied = new bool[maxObjects];
            UpdateBufferManually = false;
        }

        public void UpdateBuffers(){
            Debug.Assert(UpdateBufferManually, "cannot update a buffer that's set to automatic updating");
            base.Indexbuffer.SetData(_indicies);
            base.Vertexbuffer.SetData(_verticies);
        }

        public void AddObject(IEquatable<T> identifier, int[] indicies, VertexPositionNormalTexture[] verticies){
            Debug.Assert(indicies.Length == _indiciesPerObject);
            Debug.Assert(verticies.Length == _verticiesPerObject);

            int index = -1;
            for (int i = 0; i < _maxObjects; i++){
                if (_isSlotOccupied[i] == false){
                    //add buffer offset to the indice list
                    for (int indice = 0; indice < indicies.Length; indice++){
                        indicies[indice] += i*_verticiesPerObject;
                    }

                    _objectData.Add(new ObjectData(identifier, i, indicies, verticies));
                    _isSlotOccupied[i] = true;
                    index = i;
                    break;
                }
            }
            Debug.Assert(index != -1, "not enough space in object buffer to add new object");

            indicies.CopyTo(_indicies, index*_indiciesPerObject);
            verticies.CopyTo(_verticies, index*_verticiesPerObject);
            if (!UpdateBufferManually){
                base.Indexbuffer.SetData(_indicies);
                base.Vertexbuffer.SetData(_verticies);
            }
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
            for (int i = 0; i < _indiciesPerObject; i++){
                _indicies[objectToRemove.ObjectOffset*_indiciesPerObject + i] = 0;
            }
            if (!UpdateBufferManually){
                base.Indexbuffer.SetData(_indicies);
            }
            _objectData.Remove(objectToRemove);
        }

        public void ClearObjects(){
            _objectData.Clear();
            for (int i = 0; i < _maxObjects; i++){
                _isSlotOccupied[i] = false;
            }
            for (int i = 0; i < _maxObjects*_indiciesPerObject; i++){
                _indicies[i] = 0;
            }
            base.Indexbuffer.SetData(_indicies);
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

            objToEnable.IsEnabled = true;
            objToEnable.Indicies.CopyTo(_indicies, objToEnable.ObjectOffset*_indiciesPerObject);
            if (!UpdateBufferManually){
                base.Indexbuffer.SetData(_indicies);
            }
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

            objToDisable.IsEnabled = false;
            var indicies = new int[_indiciesPerObject];
            indicies.CopyTo(_indicies, objToDisable.ObjectOffset*_indiciesPerObject);
            if (!UpdateBufferManually){
                base.Indexbuffer.SetData(_indicies);
            }
            return true;
        }

        /// <summary>
        ///   really cool method that will take another objectbuffer and absorb its objects into this objectbuffer. also clears the other buffer afterwards.
        /// </summary>
        public void AbsorbBuffer(ObjectBuffer<T> buffer){
            bool buffUpdateState = UpdateBufferManually;
            UpdateBufferManually = true; //temporary for this heavy copy algo

            foreach (var objectData in buffer._objectData){
                bool isDuplicate = false;
                foreach (var data in _objectData){
                    if (data.Identifier.Equals(objectData.Identifier))
                        isDuplicate = true;
                }
                if (isDuplicate)
                    continue;

                int offset = objectData.ObjectOffset*_verticiesPerObject;
                var indicies = from index in objectData.Indicies
                               select index - offset;

                AddObject(objectData.Identifier, indicies.ToArray(), objectData.Verticies);
            }
            UpdateBuffers();
            UpdateBufferManually = buffUpdateState;
            buffer.ClearObjects();
        }

        #region Nested type: ObjectData

        class ObjectData{
            // ReSharper disable MemberCanBePrivate.Local
            public readonly IEquatable<T> Identifier;
            public readonly int[] Indicies;
            public readonly int ObjectOffset;
            public readonly VertexPositionNormalTexture[] Verticies;
            public bool IsEnabled;
            // ReSharper restore MemberCanBePrivate.Local

            public ObjectData(IEquatable<T> identifier, int objectOffset, int[] indicies, VertexPositionNormalTexture[] verticies){
                IsEnabled = true;
                Identifier = identifier;
                ObjectOffset = objectOffset;
                Indicies = indicies;
                Verticies = verticies;
            }
        }

        #endregion
    }
}