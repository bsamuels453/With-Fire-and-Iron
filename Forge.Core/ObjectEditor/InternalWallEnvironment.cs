#region

using System;
using System.Collections.Generic;
using System.Linq;
using Forge.Core.GameObjects;
using Forge.Framework.Draw;
using MonoGameUtility;

#endregion

namespace Forge.Core.ObjectEditor{
    public class InternalWallEnvironment : IDisposable{
        readonly int _numDecks;
        readonly Dictionary<ObjectIdentifier, XZPoint>[] _objectFootprints;
        readonly ObjectBuffer<WallSegmentIdentifier>[] _wallBuffers;
        readonly List<WallSegmentIdentifier>[] _wallIdentifiers;
        int _curDeck;

        public InternalWallEnvironment(HullEnvironment hullEnv, GameObjectEnvironment gameEnv){
            var deckSecContainer = hullEnv.DeckSectionContainer;
            _numDecks = hullEnv.NumDecks;
            _objectFootprints = gameEnv.ObjectFootprints;

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
            _curDeck = 0;
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

        public void AddWalls(ObjectBuffer<WallSegmentIdentifier> wallBuffer, List<WallSegmentIdentifier> segments){
            var footprints = _objectFootprints[_curDeck];

            foreach (var segment in segments){
                var ref1 = new Point((int) (segment.RefPoint1.X*2), (int) (segment.RefPoint1.Z*2));
                var ref2 = new Point((int) (segment.RefPoint2.X*2), (int) (segment.RefPoint2.Z*2));

                bool segmentValid = true;
                foreach (var footprint in footprints){
                    var origin = footprint.Key.Origin;
                    var bbox = new Rectangle(origin.X + 1, origin.Z, footprint.Value.X - 1, footprint.Value.Z - 1);
                    if (bbox.Contains(ref1) || bbox.Contains(ref2)){
                        segmentValid = false;
                        break;
                    }
                }
                if (!segmentValid)
                    return;
            }

            foreach (WallSegmentIdentifier segment in wallBuffer){
                if (!CurWallBuffer.Contains(segment)){
                    CurWallIdentifiers.Add(segment);
                }
            }

            CurWallBuffer.AbsorbBuffer(wallBuffer);
        }

        public bool IsObjectPlacementValid(Vector3 pos, XZPoint dims, int deck){
            //throw new NotImplementedException();
            return true;
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
            _curDeck = newDeck;
        }
    }
}