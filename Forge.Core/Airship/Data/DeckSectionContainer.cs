using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Forge.Core.Logic;
using Forge.Framework.Draw;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Forge.Core.Airship.Data{
    class DeckSectionContainer  : IDisposable{
        public readonly int NumDecks;
        public int TopExpIdx { get; private set; }
        public readonly ObjectBuffer<AirshipObjectIdentifier>[] DeckBufferByDeck;
        public ObjectBuffer<AirshipObjectIdentifier> TopExposedDeck { get; private set; }
        public List<Vector3> TopExposedVertexes { get; private set; }
        public List<BoundingBox> TopExposedBoundingBoxes { get; private set; }
        public List<BoundingBox>[] BoundingBoxesByDeck { get; private set; }
        public List<Vector3>[] DeckVertexesByDeck { get; private set; }

        public DeckSectionContainer(
            List<BoundingBox>[] deckBoundingBoxes,
            ObjectBuffer<AirshipObjectIdentifier>[] deckBuffers, 
            List<Vector3>[] deckVerts) {

                DeckBufferByDeck = deckBuffers;
                BoundingBoxesByDeck = deckBoundingBoxes;
                DeckVertexesByDeck = deckVerts;

                TopExpIdx = 0;
                TopExposedDeck = DeckBufferByDeck[TopExpIdx];
                TopExposedBoundingBoxes = BoundingBoxesByDeck[TopExpIdx];
                TopExposedVertexes = DeckVertexesByDeck[TopExpIdx];

                NumDecks = DeckBufferByDeck.Length;
                foreach (var buff in DeckBufferByDeck) {
                    Debug.Assert(buff != null);
                }

                Debug.Assert(DeckBufferByDeck.Length == BoundingBoxesByDeck.Length);
                Debug.Assert(DeckBufferByDeck.Length == DeckVertexesByDeck.Length);
        }

        public int SetTopVisibleDeck(int deck) {
            if (deck < 0 || deck >= NumDecks)
                return TopExpIdx;
            //Debug.Assert(deck >= 0);
            //Debug.Assert(deck < NumDecks);

            for (int i = NumDecks - 1; i >= deck; i--) {
                DeckBufferByDeck[i].Enabled = true;
            }
            for (int i = deck - 1; i >= 0; i--) {
                DeckBufferByDeck[i].Enabled = false;
            }
            TopExpIdx = deck;
            TopExposedDeck = DeckBufferByDeck[TopExpIdx];
            TopExposedBoundingBoxes = BoundingBoxesByDeck[TopExpIdx];
            TopExposedVertexes = DeckVertexesByDeck[TopExpIdx];
            return TopExpIdx;
        }

        bool _disposed;

        public void Dispose(){
            Debug.Assert(!_disposed);
            foreach (var buffer in DeckBufferByDeck){
                buffer.Dispose();
            }

            _disposed = true;
        }

        ~DeckSectionContainer(){
            Debug.Assert(_disposed);
        }
    }
}
