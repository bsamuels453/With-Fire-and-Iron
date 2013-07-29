#region

using System;
using System.Diagnostics;
using Forge.Core.Airship.Data;
using Forge.Core.Airship.Export;
using Forge.Core.Airship.Generation;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;

#endregion

namespace Forge.Core.ObjectEditor{
    public class HullEnvironment : IDisposable{
        #region Delegates

        public delegate void CurDeckChanged(int oldDeck, int newDeck);

        #endregion

        public readonly Vector3 CenterPoint;
        public readonly float DeckHeight;
        public readonly DeckSectionContainer DeckSectionContainer;
        public readonly HullSectionContainer HullSectionContainer;
        public readonly int NumDecks;
        readonly HullSplitter _hullSplitter;

        int _curDeck;
        bool _disposed;

        public HullEnvironment(AirshipPackager.AirshipSerializationStruct data){
            NumDecks = data.ModelAttributes.NumDecks;
            VisibleDecks = NumDecks;
            DeckSectionContainer = new DeckSectionContainer(data.DeckSections);
            DeckHeight = data.ModelAttributes.DeckHeight;
            CenterPoint = data.ModelAttributes.Centroid;
            HullSectionContainer = new HullSectionContainer(data.HullSections);
            _hullSplitter = new HullSplitter(HullSectionContainer.HullBuffersByDeck);

            CurDeck = 0;
        }

        public int VisibleDecks { get; private set; }

        public int CurDeck{
            get { return _curDeck; }
            set{
                //higher curdeck means a lower deck is displayed
                //low curdeck means higher deck displayed
                //highest deck is 0

                if (value < 0)
                    value = 0;
                if (value >= NumDecks)
                    value = NumDecks - 1;

                int oldDeck = _curDeck;
                int diff = -(value - _curDeck);
                VisibleDecks += diff;
                _curDeck = value;

                HullSectionContainer.SetTopVisibleDeck(_curDeck);
                DeckSectionContainer.SetTopVisibleDeck(_curDeck);

                if (OnCurDeckChange != null){
                    OnCurDeckChange.Invoke(oldDeck, _curDeck);
                }
            }
        }

        #region IDisposable Members

        public void Dispose(){
            Debug.Assert(!_disposed);
            DeckSectionContainer.Dispose();
            HullSectionContainer.Dispose();
            _disposed = true;
        }

        #endregion

        public event CurDeckChanged OnCurDeckChange;

        public void MoveUpOneDeck(){
            CurDeck--;
        }

        public void MoveDownOneDeck(){
            CurDeck++;
        }

        ~HullEnvironment(){
            Debug.Assert(_disposed);
        }
    }
}