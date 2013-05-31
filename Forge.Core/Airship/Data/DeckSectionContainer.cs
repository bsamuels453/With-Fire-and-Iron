using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Forge.Core.Logic;
using Forge.Framework;
using Forge.Framework.Draw;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;
using ProtoBuf;

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

        #region serialization

        public DeckSectionContainer(Serialized s){
            NumDecks = s.NumDecks;

            BoundingBoxesByDeck = new List<BoundingBox>[NumDecks];
            for (int i = 0; i < NumDecks; i++){
                BoundingBoxesByDeck[i] = s.DeckBoundingBoxes[i].BoundingBoxes;
            }

            DeckVertexesByDeck = new List<Vector3>[NumDecks];
            for (int i = 0; i < NumDecks; i++) {
                DeckVertexesByDeck[i] = s.DeckVertexes[i].Vertexes;
            }

            DeckBufferByDeck = new ObjectBuffer<AirshipObjectIdentifier>[NumDecks];
            for (int i = 0; i < NumDecks; i++) {
                DeckBufferByDeck[i] = new ObjectBuffer<AirshipObjectIdentifier>(s.DeckBuffers[i]);
            }

            TopExpIdx = 0;
            TopExposedDeck = DeckBufferByDeck[TopExpIdx];
            TopExposedBoundingBoxes = BoundingBoxesByDeck[TopExpIdx];
            TopExposedVertexes = DeckVertexesByDeck[TopExpIdx];

            foreach (var buff in DeckBufferByDeck) {
                Debug.Assert(buff != null);
            }

            Debug.Assert(DeckBufferByDeck.Length == BoundingBoxesByDeck.Length);
            Debug.Assert(DeckBufferByDeck.Length == DeckVertexesByDeck.Length);
        }

        public Serialized ExtractSerializationStruct() {
            var deckBuffData = new ObjectBuffer<AirshipObjectIdentifier>.Serialized[NumDecks];
            for (int i = 0; i < NumDecks; i++) {
                deckBuffData[i] = DeckBufferByDeck[i].ExtractSerializationStruct();
            }

            var deckVertexes = new ProtoBuffWrappers.Vector3Container[NumDecks];
            var deckBBoxes = new ProtoBuffWrappers.BoundingBoxContainer[NumDecks];

            for (int i = 0; i < NumDecks; i++){
                deckVertexes[i] = new ProtoBuffWrappers.Vector3Container(DeckVertexesByDeck[i]);
            }
            for (int i = 0; i < NumDecks; i++) {
                deckBBoxes[i] = new ProtoBuffWrappers.BoundingBoxContainer(BoundingBoxesByDeck[i]);
            }

            var ret = new Serialized(
                NumDecks,
                deckBuffData,
                deckVertexes,
                deckBBoxes
                );
            return ret;
        }

        [ProtoContract]
        public struct Serialized{
            [ProtoMember(1)]
            public readonly int NumDecks;
            [ProtoMember(2)]
            public readonly ObjectBuffer<AirshipObjectIdentifier>.Serialized[] DeckBuffers;
            [ProtoMember(3)]
            public readonly ProtoBuffWrappers.Vector3Container[] DeckVertexes;
            [ProtoMember(4)]
            public readonly ProtoBuffWrappers.BoundingBoxContainer[] DeckBoundingBoxes;

            public Serialized(int numDecks, ObjectBuffer<AirshipObjectIdentifier>.Serialized[] deckBuffers, ProtoBuffWrappers.Vector3Container[] deckVertexes, ProtoBuffWrappers.BoundingBoxContainer[] deckBoundingBoxes) {
                NumDecks = numDecks;
                DeckBuffers = deckBuffers;
                DeckVertexes = deckVertexes;
                DeckBoundingBoxes = deckBoundingBoxes;
            }
        }

        #endregion
    }
}
