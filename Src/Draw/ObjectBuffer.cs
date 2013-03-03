#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Gondola.Draw{
    internal class ObjectBuffer<TIdentifier> : GeometryBuffer<VertexPositionNormalTexture> where TIdentifier : IEquatable<TIdentifier> {
        readonly int[] _indicies;
        readonly int _indiciesPerObject;
        readonly bool[] _isSlotOccupied;
        readonly int _maxObjects;
        readonly List<ObjectData> _objectData;
        readonly VertexPositionNormalTexture[] _verticies;
        readonly int _verticiesPerObject;

        public bool UpdateBufferManually;

        public ObjectBuffer(int maxObjects, int primitivesPerObject, int verticiesPerObject, int indiciesPerObject, string settingsFileName) :
            base(indiciesPerObject * maxObjects, verticiesPerObject * maxObjects, primitivesPerObject * maxObjects, settingsFileName, PrimitiveType.TriangleList) {
            Rasterizer = new RasterizerState { CullMode = CullMode.None };

            _objectData = new List<ObjectData>();
            _indicies = new int[maxObjects * indiciesPerObject];
            _verticies = new VertexPositionNormalTexture[maxObjects * verticiesPerObject];

            _indiciesPerObject = indiciesPerObject;
            _verticiesPerObject = verticiesPerObject;
            _maxObjects = maxObjects;
            _isSlotOccupied = new bool[maxObjects];
            UpdateBufferManually = false;
        }

        public void UpdateBuffers() {
            Debug.Assert(UpdateBufferManually, "cannot update a buffer that's set to automatic updating");
            base.BaseIndexBuffer.SetData(_indicies);
            base.BaseVertexBuffer.SetData(_verticies);
        }

        public void AddObject(IEquatable<TIdentifier> identifier, int[] indicies, VertexPositionNormalTexture[] verticies) {
            Debug.Assert(indicies.Length == _indiciesPerObject);
            Debug.Assert(verticies.Length == _verticiesPerObject);

            int index = -1;
            for (int i = 0; i < _maxObjects; i++) {
                if (_isSlotOccupied[i] == false) {
                    //add buffer offset to the indice list
                    for (int indice = 0; indice < indicies.Length; indice++) {
                        indicies[indice] += i * _verticiesPerObject;
                    }

                    _objectData.Add(new ObjectData(identifier, i, indicies, verticies));
                    _isSlotOccupied[i] = true;
                    index = i;
                    break;
                }
            }
            Debug.Assert(index != -1, "not enough space in object buffer to add new object");

            indicies.CopyTo(_indicies, index * _indiciesPerObject);
            verticies.CopyTo(_verticies, index * _verticiesPerObject);
            if (!UpdateBufferManually) {
                base.BaseIndexBuffer.SetData(_indicies);
                base.BaseVertexBuffer.SetData(_verticies);
            }
        }

        public void RemoveObject(TIdentifier identifier) {
            ObjectData objectToRemove = (
                                            from obj in _objectData
                                            where obj.Identifier.Equals(identifier)
                                            select obj
                                        ).FirstOrDefault();

            if (objectToRemove == null)
                return;

            _isSlotOccupied[objectToRemove.ObjectOffset] = false;
            for (int i = 0; i < _indiciesPerObject; i++) {
                _indicies[objectToRemove.ObjectOffset * _indiciesPerObject + i] = 0;
            }
            if (!UpdateBufferManually) {
                base.BaseIndexBuffer.SetData(_indicies);
            }
            _objectData.Remove(objectToRemove);
        }

        public void ClearObjects() {
            _objectData.Clear();
            for (int i = 0; i < _maxObjects; i++) {
                _isSlotOccupied[i] = false;
            }
            for (int i = 0; i < _maxObjects * _indiciesPerObject; i++) {
                _indicies[i] = 0;
            }
            base.BaseIndexBuffer.SetData(_indicies);
        }

        public bool EnableObject(IEquatable<TIdentifier> identifier) {
            ObjectData objToEnable = null;
            foreach (var obj in _objectData) {
                if (obj.Identifier.Equals(identifier)) {
                    objToEnable = obj;
                }
            }
            if (objToEnable == null)
                return false;

            objToEnable.Enabled = true;
            objToEnable.Indicies.CopyTo(_indicies, objToEnable.ObjectOffset * _indiciesPerObject);
            if (!UpdateBufferManually) {
                base.BaseIndexBuffer.SetData(_indicies);
            }
            return true;
        }

        public bool DisableObject(TIdentifier identifier) {
            ObjectData objToDisable = null;
            foreach (var obj in _objectData) {
                if (obj.Identifier.Equals(identifier)) {
                    objToDisable = obj;
                }
            }
            if (objToDisable == null)
                return false;

            objToDisable.Enabled = false;
            var indicies = new int[_indiciesPerObject];
            indicies.CopyTo(_indicies, objToDisable.ObjectOffset * _indiciesPerObject);
            if (!UpdateBufferManually) {
                base.BaseIndexBuffer.SetData(_indicies);
            }
            return true;
        }

        /// <summary>
        ///   really cool method that will take another objectbuffer and absorb its objects into this objectbuffer. also clears the other buffer afterwards.
        /// </summary>
        public void AbsorbBuffer(ObjectBuffer<TIdentifier> buffer) {
            bool buffUpdateState = UpdateBufferManually;
            UpdateBufferManually = true; //temporary for this heavy copy algo

            foreach (var objectData in buffer._objectData) {
                bool isDuplicate = false;
                foreach (var data in _objectData) {
                    if (data.Identifier.Equals(objectData.Identifier))
                        isDuplicate = true;
                }
                if (isDuplicate)
                    continue;

                int offset = objectData.ObjectOffset * _verticiesPerObject;
                var indicies = from index in objectData.Indicies
                               select index - offset;

                AddObject(objectData.Identifier, indicies.ToArray(), objectData.Verticies);
            }
            UpdateBuffers();
            UpdateBufferManually = buffUpdateState;
            buffer.ClearObjects();
        }

        public ObjectData[] DumpObjectData() {
            return _objectData.ToArray();
        }

        public VertexPositionNormalTexture[] DumpVerticies() {
            var data = new VertexPositionNormalTexture[base.BaseVertexBuffer.VertexCount];
            BaseVertexBuffer.GetData(data);
            return data;
        }

        public int[] DumpIndicies() {
            var data = new int[base.BaseIndexBuffer.IndexCount];
            BaseIndexBuffer.GetData(data);
            return data;
        }

        #region Nested type: ObjectData

        public class ObjectData {
            // ReSharper disable MemberCanBePrivate.Local
            public readonly IEquatable<TIdentifier> Identifier;
            public readonly int[] Indicies;
            public readonly int ObjectOffset;
            public readonly VertexPositionNormalTexture[] Verticies;
            public bool Enabled;
            // ReSharper restore MemberCanBePrivate.Local

            public ObjectData(IEquatable<TIdentifier> identifier, int objectOffset, int[] indicies, VertexPositionNormalTexture[] verticies) {
                Enabled = true;
                Identifier = identifier;
                ObjectOffset = objectOffset;
                Indicies = indicies;
                Verticies = verticies;
            }
        }

        #endregion
    }
}