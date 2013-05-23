using System.Collections.Generic;
using System.Linq;
using Forge.Core.Airship;
using Forge.Core.ObjectEditor.Tools;
using Forge.Framework.Draw;
using Forge.Core.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Forge.Core.ObjectEditor {
    internal class HullDataManager {
        #region Delegates

        public delegate void CurDeckChanged(int oldDeck, int newDeck);

        #endregion

        public readonly Vector3 CenterPoint;

        public readonly List<BoundingBox>[] DeckBoundingBoxes;
        public readonly ObjectBuffer<ObjectIdentifier>[] DeckBuffers;
        public readonly float DeckHeight;
        public readonly List<Vector3>[] DeckVertexes;
        public readonly ObjectBuffer<int>[] HullBuffers;
        public readonly int NumDecks;
        public readonly ObjectBuffer<WallSegmentIdentifier>[] WallBuffers;
        public readonly List<WallSegmentIdentifier>[] WallIdentifiers;
        public readonly ObjectModelBuffer<ObjectIdentifier>[] ObjectBuffers;
        public readonly float WallResolution;
        int _curDeck;

        public HullDataManager(HullGeometryInfo geometryInfo) {
            NumDecks = geometryInfo.NumDecks;
            VisibleDecks = NumDecks;
            HullBuffers = geometryInfo.HullMeshes;
            DeckBuffers = geometryInfo.DeckFloorBuffers;
            DeckBoundingBoxes = geometryInfo.DeckFloorBoundingBoxes;
            DeckVertexes = geometryInfo.FloorVertexes;
            DeckHeight = geometryInfo.DeckHeight;
            WallResolution = geometryInfo.WallResolution;
            CenterPoint = geometryInfo.CenterPoint;

            ObjectBuffers = new ObjectModelBuffer<ObjectIdentifier>[NumDecks];

            for (int i = 0; i < ObjectBuffers.Count(); i++) {
                ObjectBuffers[i] = new ObjectModelBuffer<ObjectIdentifier>(100, "Shader_TintedModel");
            }

            WallBuffers = new ObjectBuffer<WallSegmentIdentifier>[NumDecks];
            for (int i = 0; i < WallBuffers.Count(); i++) {
                int potentialWalls = geometryInfo.FloorVertexes[i].Count() * 2;
                WallBuffers[i] = new ObjectBuffer<WallSegmentIdentifier>(potentialWalls, 10, 20, 30, "Shader_AirshipWalls");
            }

            WallIdentifiers = new List<WallSegmentIdentifier>[NumDecks];
            for (int i = 0; i < WallIdentifiers.Length; i++) {
                WallIdentifiers[i] = new List<WallSegmentIdentifier>();
            }
            CurDeck = 0;
        }

        //these will save from having to do array[curDeck] all the time elsewhere in the editor
        public ObjectBuffer<ObjectIdentifier> CurDeckBuffer { get; private set; }
        public ObjectBuffer<WallSegmentIdentifier> CurWallBuffer { get; private set; }
        public ObjectBuffer<int> CurHullBuffer { get; private set; }
        public List<WallSegmentIdentifier> CurWallIdentifiers { get; private set; }
        public List<BoundingBox> CurDeckBoundingBoxes { get; private set; }
        public List<Vector3> CurDeckVertexes { get; private set; }
        public ObjectModelBuffer<ObjectIdentifier> CurObjBuffer { get; private set; }

        public int VisibleDecks { get; private set; }

        public int CurDeck {
            get { return _curDeck; }
            set {
                //higher curdeck means a lower deck is displayed
                //low curdeck means higher deck displayed
                //highest deck is 0
                int diff = -(value - _curDeck);
                int oldDeck = _curDeck;
                VisibleDecks += diff;
                _curDeck = value;

                CurDeckBuffer = DeckBuffers[_curDeck];
                CurWallBuffer = WallBuffers[_curDeck];
                if (_curDeck != 0) {
                    CurHullBuffer = HullBuffers[_curDeck - 1];
                }
                CurWallIdentifiers = WallIdentifiers[_curDeck];
                CurDeckBoundingBoxes = DeckBoundingBoxes[_curDeck];
                CurDeckVertexes = DeckVertexes[_curDeck];
                CurObjBuffer = ObjectBuffers[_curDeck];

                foreach (var buffer in ObjectBuffers) {
                    buffer.Enabled = false;
                }

                for (int i = _curDeck; i < NumDecks; i++) {
                    ObjectBuffers[i].Enabled = true;
                }

                if (OnCurDeckChange != null) {
                    OnCurDeckChange.Invoke(oldDeck, _curDeck);
                }
            }
        }

        public event CurDeckChanged OnCurDeckChange;

        public void MoveUpOneDeck() {
            if (CurDeck != 0) {
                var tempFloorBuff = DeckBuffers.Reverse().ToArray();
                var tempWallBuff = HullBuffers.Reverse().ToArray();
                var tempWWallBuff = WallBuffers.Reverse().ToArray();

                for (int i = 0; i < tempFloorBuff.Count(); i++) {
                    if (tempFloorBuff[i].Enabled == false) {
                        CurDeck--;
                        tempFloorBuff[i].Enabled = true;
                        tempWWallBuff[i].Enabled = true;
                        tempWallBuff[i].CullMode = CullMode.None;
                        break;
                    }
                }
            }
        }

        public void MoveDownOneDeck() {
            if (CurDeck < NumDecks - 1) {
                for (int i = 0; i < DeckBuffers.Count(); i++) {
                    if (DeckBuffers[i].Enabled) {
                        CurDeck++;
                        DeckBuffers[i].Enabled = false;
                        HullBuffers[i].CullMode = CullMode.CullCounterClockwiseFace; 
                        WallBuffers[i].Enabled = false;
                        break;
                    }
                }
            }
        }
    }
}
