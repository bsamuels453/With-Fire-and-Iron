#region

using System;
using System.Collections.Generic;
using System.Linq;
using Forge.Framework.Draw;

#endregion

namespace Forge.Core.ObjectEditor{
    public class InternalWallEnvironment : IDisposable{
        readonly int _numDecks;
        readonly ObjectBuffer<WallSegmentIdentifier>[] _wallBuffers;
        readonly List<WallSegmentIdentifier>[] _wallIdentifiers;

        public InternalWallEnvironment(HullEnvironment hullEnv){
            var deckSecContainer = hullEnv.DeckSectionContainer;
            _numDecks = hullEnv.NumDecks;

            _wallBuffers = new ObjectBuffer<WallSegmentIdentifier>[_numDecks];
            for (int i = 0; i < _wallBuffers.Count(); i++){
                int potentialWalls = deckSecContainer.DeckVertexesByDeck[i].Count()*2;
                _wallBuffers[i] = new ObjectBuffer<WallSegmentIdentifier>(potentialWalls, 10, 20, 30, "Config/Shaders/Airship_InternalWalls.config");
            }

            _wallIdentifiers = new List<WallSegmentIdentifier>[_numDecks];
            for (int i = 0; i < _wallIdentifiers.Length; i++){
                _wallIdentifiers[i] = new List<WallSegmentIdentifier>();
            }

            hullEnv.OnCurDeckChange += OnVisibleDeckChange;
            OnVisibleDeckChange(0, 0);
        }

        public List<WallSegmentIdentifier> CurWallIdentifiers { get; private set; }
        public ObjectBuffer<WallSegmentIdentifier> CurWallBuffer { get; private set; }

        #region IDisposable Members

        public void Dispose(){
            foreach (var buffer in _wallBuffers){
                buffer.Dispose();
            }
        }

        #endregion

        public void AddWalls(ObjectBuffer<WallSegmentIdentifier> wallBuffer){
            foreach (WallSegmentIdentifier segment in wallBuffer){
                if (!CurWallBuffer.Contains(segment)){
                    CurWallIdentifiers.Add(segment);
                }
            }

            CurWallBuffer.AbsorbBuffer(wallBuffer);
        }

        public void DisableSegment(WallSegmentIdentifier identifier){
            CurWallBuffer.DisableObject(identifier);
        }

        public void EnableSegment(WallSegmentIdentifier identifier){
            CurWallBuffer.EnableObject(identifier);
        }

        public void RemoveSegment(WallSegmentIdentifier identifier){
            CurWallBuffer.RemoveObject(identifier);
            CurWallIdentifiers.Remove(identifier);
        }

        void OnVisibleDeckChange(int old, int newDeck){
            CurWallBuffer = _wallBuffers[newDeck];
            CurWallIdentifiers = _wallIdentifiers[newDeck];
            foreach (var buffer in _wallBuffers){
                buffer.Enabled = false;
            }
            for (int i = _numDecks - 1; i >= newDeck; i--){
                _wallBuffers[i].Enabled = true;
            }
        }
    }
}