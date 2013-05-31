#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Forge.Framework.Draw{
    public class ObjectBuffer<TIdentifier> : GeometryBuffer<VertexPositionNormalTexture>, IEnumerable where TIdentifier : IEquatable<TIdentifier>{
        readonly int[] _indicies;
        readonly bool[] _isSlotOccupied;
        public readonly int MaxObjects;
        public readonly int VerticiesPerObject;
        public readonly int IndiciesPerObject;
        readonly List<ObjectData> _objectData;
        readonly VertexPositionNormalTexture[] _verticies;

        public bool UpdateBufferManually;

        public ObjectBuffer(int maxObjects, int primitivesPerObject, int verticiesPerObject, int indiciesPerObject, string settingsFileName) :
            base(indiciesPerObject*maxObjects, verticiesPerObject*maxObjects, primitivesPerObject*maxObjects, settingsFileName, PrimitiveType.TriangleList){
            Rasterizer = new RasterizerState{CullMode = CullMode.None};

            _objectData = new List<ObjectData>(maxObjects);
            _indicies = new int[maxObjects*indiciesPerObject];
            _verticies = new VertexPositionNormalTexture[maxObjects*verticiesPerObject];

            IndiciesPerObject = indiciesPerObject;
            VerticiesPerObject = verticiesPerObject;
            MaxObjects = maxObjects;
            _isSlotOccupied = new bool[maxObjects];
            UpdateBufferManually = false;
        }

        public int ActiveObjects{
            get { return _objectData.Count(data => data.Enabled); }
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator(){
            var enumLi = from data in _objectData
                select data.Identifier;
            return enumLi.GetEnumerator();
        }

        #endregion

        public void UpdateBuffers(){
            Debug.Assert(UpdateBufferManually, "cannot update a buffer that's set to automatic updating");
            base.BaseIndexBuffer.SetData(_indicies);
            base.BaseVertexBuffer.SetData(_verticies);
        }

        public void AddObject(IEquatable<TIdentifier> identifier, int[] indicies, VertexPositionNormalTexture[] verticies){
            Debug.Assert(indicies.Length == IndiciesPerObject);
            Debug.Assert(verticies.Length == VerticiesPerObject);

            int index = -1;
            for (int i = 0; i < MaxObjects; i++){
                if (_isSlotOccupied[i] == false){
                    //add buffer offset to the indice list
                    for (int indice = 0; indice < indicies.Length; indice++){
                        indicies[indice] += i*VerticiesPerObject;
                    }

                    _objectData.Add(new ObjectData(identifier, i, indicies, verticies));
                    _isSlotOccupied[i] = true;
                    index = i;
                    break;
                }
            }
            Debug.Assert(index != -1, "not enough space in object buffer to add new object");

            indicies.CopyTo(_indicies, index*IndiciesPerObject);
            verticies.CopyTo(_verticies, index*VerticiesPerObject);
            if (!UpdateBufferManually){
                base.BaseIndexBuffer.SetData(_indicies);
                base.BaseVertexBuffer.SetData(_verticies);
            }
        }

        public void RemoveObject(TIdentifier identifier){
            var objectToRemove = (
                from obj in _objectData
                where obj.Identifier.Equals(identifier)
                select obj).ToArray();

            if (!objectToRemove.Any())
                return;
            for (int objIdx = 0; objIdx < objectToRemove.Count(); objIdx++){
                _isSlotOccupied[objectToRemove[objIdx].ObjectOffset] = false;
                for (int i = 0; i < IndiciesPerObject; i++){
                    _indicies[objectToRemove[objIdx].ObjectOffset*IndiciesPerObject + i] = 0;
                }
                if (!UpdateBufferManually){
                    base.BaseIndexBuffer.SetData(_indicies);
                }
                _objectData.Remove(objectToRemove[objIdx]);
            }
        }

        public void ClearObjects(){
            _objectData.Clear();
            for (int i = 0; i < MaxObjects; i++){
                _isSlotOccupied[i] = false;
            }
            for (int i = 0; i < MaxObjects*IndiciesPerObject; i++){
                _indicies[i] = 0;
            }
            base.BaseIndexBuffer.SetData(_indicies);
        }

        public bool EnableObject(IEquatable<TIdentifier> identifier){
            var objToEnable = new List<ObjectData>();
            foreach (var obj in _objectData){
                if (obj.Identifier.Equals(identifier)){
                    objToEnable.Add(obj);
                }
            }
            if (objToEnable.Count == 0)
                return false;

            objToEnable.ForEach(o => o.Enabled = true);
            foreach (var obj in objToEnable){
                obj.Indicies.CopyTo(_indicies, obj.ObjectOffset*IndiciesPerObject);
            }
            if (!UpdateBufferManually){
                base.BaseIndexBuffer.SetData(_indicies);
            }
            return true;
        }

        public bool DisableObject(TIdentifier identifier){
            var objToDisable = new List<ObjectData>();
            foreach (var obj in _objectData){
                if (obj.Identifier.Equals(identifier)){
                    objToDisable.Add(obj);
                }
            }
            if (objToDisable.Count == 0)
                return false;

            objToDisable.ForEach(o => o.Enabled = false);
            var indicies = new int[IndiciesPerObject];

            foreach (var obj in objToDisable){
                indicies.CopyTo(_indicies, obj.ObjectOffset*IndiciesPerObject);
            }
            if (!UpdateBufferManually){
                base.BaseIndexBuffer.SetData(_indicies);
            }
            return true;
        }

        public void ConstructFromObjectDump(ObjectData[] objData){
            Debug.Assert(objData[0].Indicies.Length == IndiciesPerObject);
            Debug.Assert(objData[0].Verticies.Length == VerticiesPerObject);
            Debug.Assert(objData[0].Identifier.GetType() == typeof(TIdentifier));

            foreach (var data in objData){
                int offset = data.ObjectOffset * VerticiesPerObject;
                var fixedInds = from index in data.Indicies
                                select index - offset;
                AddObject(data.Identifier, fixedInds.ToArray(), data.Verticies);
            }
        }

        public void ApplyTransform(Func<VertexPositionNormalTexture, VertexPositionNormalTexture> transform) {
            for (int i = 0; i < _verticies.Length; i++){
                _verticies[i] = transform.Invoke(_verticies[i]);
            }
            foreach (var objectData in _objectData){
                int offset = objectData.ObjectOffset * VerticiesPerObject;
                for (int i = 0; i < VerticiesPerObject; i++){
                    objectData.Verticies[i] = _verticies[offset + i];
                }
            }
            VertexBuffer.SetData(_verticies);
        }

        public bool Contains(IEquatable<TIdentifier> identifier){
            var li = from o in _objectData where o.Identifier.Equals(identifier) select o;
            if (li.Any())
                return true;
            return false;
        }

        public bool IsObjectEnabled(IEquatable<TIdentifier> identifier){
            Debug.Assert(Contains(identifier));
            var objArr = from o in _objectData where o.Identifier.Equals(identifier) select o;
            return objArr.First().Enabled;
        }

        /// <summary>
        ///   really cool method that will take another objectbuffer and absorb its objects into this objectbuffer. also clears the other buffer afterwards.
        /// </summary>
        public void AbsorbBuffer(ObjectBuffer<TIdentifier> buffer, bool allowDuplicateIDs = false, bool clearOtherbuff = true) {
            bool buffUpdateState = UpdateBufferManually;
            UpdateBufferManually = true; //temporary for this heavy copy algo

            foreach (var objectData in buffer._objectData){
                if (!allowDuplicateIDs){
                    bool isDuplicate = false;
                    foreach (var data in _objectData){
                        if (data.Identifier.Equals(objectData.Identifier))
                            isDuplicate = true;
                    }
                    if (isDuplicate)
                        continue;
                }

                int offset = objectData.ObjectOffset*VerticiesPerObject;
                var indicies = from index in objectData.Indicies
                    select index - offset;

                AddObject(objectData.Identifier, indicies.ToArray(), objectData.Verticies);
            }
            UpdateBuffers();
            UpdateBufferManually = buffUpdateState;
            if (clearOtherbuff){
                buffer.ClearObjects();
            }
        }

        public ObjectData[] DumpObjectData(){
            return _objectData.ToArray();
        }

        public VertexPositionNormalTexture[] DumpVerticies(){
            var data = new VertexPositionNormalTexture[base.BaseVertexBuffer.VertexCount];
            BaseVertexBuffer.GetData(data);
            return data;
        }

        public int[] DumpIndicies(){
            var data = new int[base.BaseIndexBuffer.IndexCount];
            BaseIndexBuffer.GetData(data);
            return data;
        }

        #region Nested type: ObjectData

        public class ObjectData{
            // ReSharper disable MemberCanBePrivate.Local
            public readonly IEquatable<TIdentifier> Identifier;
            public readonly int[] Indicies;
            public readonly int ObjectOffset;
            public readonly VertexPositionNormalTexture[] Verticies;
            public bool Enabled;
            // ReSharper restore MemberCanBePrivate.Local

            public ObjectData(IEquatable<TIdentifier> identifier, int objectOffset, int[] indicies, VertexPositionNormalTexture[] verticies){
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