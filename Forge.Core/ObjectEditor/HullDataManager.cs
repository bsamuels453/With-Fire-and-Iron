#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Forge.Core.Airship.Data;
using Forge.Core.Airship.Generation;
using Forge.Core.Logic;
using Forge.Core.ObjectEditor.Tools;
using Forge.Framework.Draw;
using MonoGameUtility;

#endregion

namespace Forge.Core.ObjectEditor{
    /// <summary>
    ///   NOTICE: the next time work is done on the editor, encapsulate ObjectModelBuffer, WallIdentifiers, and WallBuffer
    /// </summary>
    internal class HullDataManager : IDisposable{
        #region Delegates

        public delegate void CurDeckChanged(int oldDeck, int newDeck);

        #endregion

        public readonly Vector3 CenterPoint;

        public readonly float DeckHeight;
        public readonly DeckSectionContainer DeckSectionContainer;
        public readonly HullSectionContainer HullSectionContainer;
        public readonly int NumDecks;
        public readonly ObjectModelBuffer<AirshipObjectIdentifier>[] ObjectBuffers;
        public readonly ObjectBuffer<WallSegmentIdentifier>[] WallBuffers;
        public readonly List<WallSegmentIdentifier>[] WallIdentifiers;
        public readonly float WallResolution;
        int _curDeck;
        bool _disposed;

        public HullDataManager(HullGeometryInfo geometryInfo){
            NumDecks = geometryInfo.NumDecks;
            VisibleDecks = NumDecks;
            DeckSectionContainer = geometryInfo.DeckSectionContainer;
            DeckHeight = geometryInfo.DeckHeight;
            WallResolution = geometryInfo.WallResolution;
            CenterPoint = geometryInfo.CenterPoint;
            HullSectionContainer = geometryInfo.HullSections;


            ObjectBuffers = new ObjectModelBuffer<AirshipObjectIdentifier>[NumDecks];

            for (int i = 0; i < ObjectBuffers.Count(); i++){
                ObjectBuffers[i] = new ObjectModelBuffer<AirshipObjectIdentifier>(100, "Shader_TintedModel");
            }

            WallBuffers = new ObjectBuffer<WallSegmentIdentifier>[NumDecks];
            for (int i = 0; i < WallBuffers.Count(); i++){
                int potentialWalls = DeckSectionContainer.DeckVertexesByDeck[i].Count()*2;
                WallBuffers[i] = new ObjectBuffer<WallSegmentIdentifier>(potentialWalls, 10, 20, 30, "Shader_AirshipWalls");
            }

            WallIdentifiers = new List<WallSegmentIdentifier>[NumDecks];
            for (int i = 0; i < WallIdentifiers.Length; i++){
                WallIdentifiers[i] = new List<WallSegmentIdentifier>();
            }
            CurDeck = 0;
        }

        public ObjectBuffer<WallSegmentIdentifier> CurWallBuffer { get; private set; }
        public List<WallSegmentIdentifier> CurWallIdentifiers { get; private set; }
        public ObjectModelBuffer<AirshipObjectIdentifier> CurObjBuffer { get; private set; }

        public int VisibleDecks { get; private set; }

        public int CurDeck{
            get { return _curDeck; }
            set{
                //higher curdeck means a lower deck is displayed
                //low curdeck means higher deck displayed
                //highest deck is 0
                int diff = -(value - _curDeck);
                int oldDeck = _curDeck;
                VisibleDecks += diff;
                _curDeck = value;

                CurWallBuffer = WallBuffers[_curDeck];
                CurWallIdentifiers = WallIdentifiers[_curDeck];
                CurObjBuffer = ObjectBuffers[_curDeck];

                foreach (var buffer in ObjectBuffers){
                    buffer.Enabled = false;
                }

                for (int i = _curDeck; i < NumDecks; i++){
                    ObjectBuffers[i].Enabled = true;
                }

                if (OnCurDeckChange != null){
                    OnCurDeckChange.Invoke(oldDeck, _curDeck);
                }
            }
        }

        #region IDisposable Members

        public void Dispose(){
            Debug.Assert(!_disposed);
            foreach (var buffer in WallBuffers){
                buffer.Dispose();
            }
            foreach (var buffer in ObjectBuffers){
                buffer.Dispose();
            }
            DeckSectionContainer.Dispose();
            HullSectionContainer.Dispose();
            _disposed = true;
        }

        #endregion

        public event CurDeckChanged OnCurDeckChange;

        public void MoveUpOneDeck(){
            CurDeck = HullSectionContainer.SetTopVisibleDeck(CurDeck - 1);
            DeckSectionContainer.SetTopVisibleDeck(CurDeck);
        }

        public void MoveDownOneDeck(){
            CurDeck = HullSectionContainer.SetTopVisibleDeck(CurDeck + 1);
            DeckSectionContainer.SetTopVisibleDeck(CurDeck);
        }

        ~HullDataManager(){
            Debug.Assert(_disposed);
        }
    }
}