#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Forge.Core.ObjectEditor;
using Forge.Framework;
using Forge.Framework.Draw;
using MonoGameUtility;
using ProtoBuf;

#endregion

namespace Forge.Core.Airship.Data{
    /// <summary>
    /// Acts as a wrapper for the geometry of an airship's deck.
    /// </summary>
    public class DeckSectionContainer : IDisposable{
        public readonly ObjectBuffer<DeckPlateIdentifier>[] DeckBufferByDeck;
        public readonly int NumDecks;
        bool _disposed;

        public DeckSectionContainer(
            List<BoundingBox>[] deckBoundingBoxes,
            ObjectBuffer<DeckPlateIdentifier>[] deckBuffers,
            List<Vector3>[] deckVerts){
            DeckBufferByDeck = deckBuffers;
            BoundingBoxesByDeck = deckBoundingBoxes;
            DeckVertexesByDeck = deckVerts;

            TopExpIdx = 0;
            TopExposedDeck = DeckBufferByDeck[TopExpIdx];
            TopExposedBoundingBoxes = BoundingBoxesByDeck[TopExpIdx];
            TopExposedVertexes = DeckVertexesByDeck[TopExpIdx];

            NumDecks = DeckBufferByDeck.Length;
            foreach (var buff in DeckBufferByDeck){
                Debug.Assert(buff != null);
            }

            Debug.Assert(DeckBufferByDeck.Length == BoundingBoxesByDeck.Length);
            Debug.Assert(DeckBufferByDeck.Length == DeckVertexesByDeck.Length);
        }

        public int TopExpIdx { get; private set; }
        public ObjectBuffer<DeckPlateIdentifier> TopExposedDeck { get; private set; }
        public List<Vector3> TopExposedVertexes { get; private set; }
        public List<BoundingBox> TopExposedBoundingBoxes { get; private set; }
        public List<BoundingBox>[] BoundingBoxesByDeck { get; private set; }
        public List<Vector3>[] DeckVertexesByDeck { get; private set; }

        #region IDisposable Members

        public void Dispose(){
            Debug.Assert(!_disposed);
            foreach (var buffer in DeckBufferByDeck){
                buffer.Dispose();
            }

            _disposed = true;
        }

        #endregion

        public int SetTopVisibleDeck(int deck){
            if (deck < 0 || deck >= NumDecks)
                return TopExpIdx;
            //Debug.Assert(deck >= 0);
            //Debug.Assert(deck < NumDecks);

            for (int i = NumDecks - 1; i >= deck; i--){
                DeckBufferByDeck[i].Enabled = true;
            }
            for (int i = deck - 1; i >= 0; i--){
                DeckBufferByDeck[i].Enabled = false;
            }
            TopExpIdx = deck;
            TopExposedDeck = DeckBufferByDeck[TopExpIdx];
            TopExposedBoundingBoxes = BoundingBoxesByDeck[TopExpIdx];
            TopExposedVertexes = DeckVertexesByDeck[TopExpIdx];
            return TopExpIdx;
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
            for (int i = 0; i < NumDecks; i++){
                DeckVertexesByDeck[i] = s.DeckVertexes[i].Vertexes;
            }

            DeckBufferByDeck = new ObjectBuffer<DeckPlateIdentifier>[NumDecks];
            for (int i = 0; i < NumDecks; i++){
                DeckBufferByDeck[i] = new ObjectBuffer<DeckPlateIdentifier>(s.DeckBuffers[i]);
            }

            TopExpIdx = 0;
            TopExposedDeck = DeckBufferByDeck[TopExpIdx];
            TopExposedBoundingBoxes = BoundingBoxesByDeck[TopExpIdx];
            TopExposedVertexes = DeckVertexesByDeck[TopExpIdx];

            foreach (var buff in DeckBufferByDeck){
                Debug.Assert(buff != null);
            }

            Debug.Assert(DeckBufferByDeck.Length == BoundingBoxesByDeck.Length);
            Debug.Assert(DeckBufferByDeck.Length == DeckVertexesByDeck.Length);
        }

        public Serialized ExtractSerializationStruct(){
            var deckBuffData = new ObjectBuffer<DeckPlateIdentifier>.Serialized[NumDecks];
            for (int i = 0; i < NumDecks; i++){
                deckBuffData[i] = DeckBufferByDeck[i].ExtractSerializationStruct(true);
            }

            var deckVertexes = new ProtoBuffWrappers.Vector3Container[NumDecks];
            var deckBBoxes = new ProtoBuffWrappers.BoundingBoxContainer[NumDecks];

            for (int i = 0; i < NumDecks; i++){
                deckVertexes[i] = new ProtoBuffWrappers.Vector3Container(DeckVertexesByDeck[i]);
            }
            for (int i = 0; i < NumDecks; i++){
                deckBBoxes[i] = new ProtoBuffWrappers.BoundingBoxContainer(BoundingBoxesByDeck[i]);
            }

            var ret = new Serialized
                (
                NumDecks,
                deckBuffData,
                deckVertexes,
                deckBBoxes
                );
            return ret;
        }

        [ProtoContract]
        public struct Serialized{
            [ProtoMember(4)] public readonly ProtoBuffWrappers.BoundingBoxContainer[] DeckBoundingBoxes;
            [ProtoMember(2)] public readonly ObjectBuffer<DeckPlateIdentifier>.Serialized[] DeckBuffers;
            [ProtoMember(3)] public readonly ProtoBuffWrappers.Vector3Container[] DeckVertexes;
            [ProtoMember(1)] public readonly int NumDecks;

            public Serialized(int numDecks, ObjectBuffer<DeckPlateIdentifier>.Serialized[] deckBuffers, ProtoBuffWrappers.Vector3Container[] deckVertexes,
                ProtoBuffWrappers.BoundingBoxContainer[] deckBoundingBoxes){
                NumDecks = numDecks;
                DeckBuffers = deckBuffers;
                DeckVertexes = deckVertexes;
                DeckBoundingBoxes = deckBoundingBoxes;
            }
        }

        #endregion
    }
}