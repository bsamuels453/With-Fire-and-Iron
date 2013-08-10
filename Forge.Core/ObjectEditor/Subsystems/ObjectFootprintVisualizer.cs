#region

using System;
using Forge.Core.GameObjects;
using Forge.Core.Util;
using Forge.Framework.Draw;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Forge.Core.ObjectEditor.Subsystems{
    public class ObjectFootprintVisualizer : IDisposable{
        readonly ObjectBuffer<GameObject>[] _footprintBuffers;
        readonly int _numDecks;

        public ObjectFootprintVisualizer(GameObjectEnvironment gameObjEnv, HullEnvironment hullEnv){
            _footprintBuffers = new ObjectBuffer<GameObject>[hullEnv.NumDecks];
            _numDecks = hullEnv.NumDecks;
            for (int i = 0; i < hullEnv.NumDecks; i++){
                _footprintBuffers[i] = new ObjectBuffer<GameObject>(300, 10, 20, 30, "Config/Shaders/ObjectPostPlacementFootprint.config");
            }
            hullEnv.OnCurDeckChange += OnVisibleDeckChange;

            gameObjEnv.AddOnObjectPlacement(OnObjectAdded);
            gameObjEnv.AddOnObjectRemove(OnObjectRemoved);
        }

        #region IDisposable Members

        public void Dispose(){
            foreach (var buffer in _footprintBuffers){
                buffer.Dispose();
            }
        }

        #endregion

        void OnObjectAdded(GameObject obj){
            var dims = StatisticProvider.GetObjectDims(obj);
            float length = dims.X/2f;
            float width = dims.Z/2f;
            VertexPositionNormalTexture[] verts;
            int[] inds;
            MeshHelper.GenerateCube(out verts, out inds, obj.ModelspacePosition, length, 0.01f, width);
            _footprintBuffers[obj.Deck].AddObject(obj, inds, verts);
        }

        void OnObjectRemoved(GameObject obj){
            throw new NotImplementedException();
        }

        void OnVisibleDeckChange(int old, int newDeck){
            foreach (var buffer in _footprintBuffers){
                buffer.Enabled = false;
            }
            for (int i = _numDecks - 1; i >= newDeck; i--){
                _footprintBuffers[i].Enabled = true;
            }
        }
    }
}