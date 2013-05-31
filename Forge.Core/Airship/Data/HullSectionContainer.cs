#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Forge.Framework.Draw;
using Microsoft.Xna.Framework.Graphics;
using ProtoBuf;

#endregion

namespace Forge.Core.Airship.Data{
    internal class HullSectionContainer : IEnumerable, IDisposable{
        public readonly ObjectBuffer<int>[] HullBuffersByDeck;
        public readonly HullSection[][] HullSectionByDeck;
        public readonly int NumDecks;
        readonly HullSection[] _hullSections;
        bool _disposed;

        public HullSectionContainer(List<HullSection> hullSections, ObjectBuffer<int>[] hullBuffersByDeck){
            HullBuffersByDeck = hullBuffersByDeck;
            _hullSections = hullSections.ToArray();
            HullSectionByDeck = GroupSectionsByDeck(_hullSections);

            NumDecks = HullBuffersByDeck.Length;
            TopExpIdx = 0;
            TopExposedHullLayer = HullBuffersByDeck[TopExpIdx];
            Debug.Assert(HullBuffersByDeck.Length == HullSectionByDeck.Length);
        }

        public int TopExpIdx { get; private set; }
        public ObjectBuffer<int> TopExposedHullLayer { get; private set; }

        #region IDisposable Members

        public void Dispose(){
            Debug.Assert(!_disposed);
            foreach (var buffer in HullBuffersByDeck){
                buffer.Dispose();
            }

            _disposed = true;
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator(){
            return GetEnumerator();
        }

        #endregion

        static HullSection[][] GroupSectionsByDeck(HullSection[] hullSections){
            var groupedByDeck = (from section in hullSections
                group section by section.Deck).ToArray();

            var hullSectionByDeck = new HullSection[groupedByDeck.Length][];
            foreach (var grouping in groupedByDeck){
                hullSectionByDeck[grouping.Key] = grouping.ToArray();
            }

            foreach (var layer in hullSectionByDeck){
                Debug.Assert(layer != null);
            }

            return hullSectionByDeck;
        }

        public int SetTopVisibleDeck(int deck){
            if (deck < 0 || deck >= NumDecks)
                return TopExpIdx;
            //Debug.Assert(deck >= 0);
            //Debug.Assert(deck < NumDecks);

            for (int i = NumDecks - 1; i >= deck; i--){
                HullBuffersByDeck[i].CullMode = CullMode.None;
            }
            for (int i = deck - 1; i >= 0; i--){
                HullBuffersByDeck[i].CullMode = CullMode.CullCounterClockwiseFace;
            }
            TopExpIdx = deck;
            TopExposedHullLayer = HullBuffersByDeck[TopExpIdx];
            return TopExpIdx;
        }

        public IEnumerator GetEnumerator(){
            return _hullSections.GetEnumerator();
        }

        ~HullSectionContainer(){
            Debug.Assert(_disposed);
        }

        #region serialization

        public HullSectionContainer(Serialized serializedStruct){
            NumDecks = serializedStruct.NumDecks;
            _hullSections = serializedStruct.HullSections;

            HullBuffersByDeck = new ObjectBuffer<int>[NumDecks];
            for (int i = 0; i < NumDecks; i++){
                HullBuffersByDeck[i] = new ObjectBuffer<int>(serializedStruct.HullBuffersByDeck[i]);
            }

            foreach (var hullSection in _hullSections){
                hullSection.SetDelegates(HullBuffersByDeck[hullSection.Deck]);
            }

            HullSectionByDeck = GroupSectionsByDeck(_hullSections);
            TopExpIdx = 0;
            TopExposedHullLayer = HullBuffersByDeck[TopExpIdx];
            Debug.Assert(HullBuffersByDeck.Length == HullSectionByDeck.Length);
        }

        public Serialized ExtractSerializationStruct(){
            var hullBuffData = new ObjectBuffer<int>.Serialized[NumDecks];
            for (int i = 0; i < NumDecks; i++){
                hullBuffData[i] = HullBuffersByDeck[i].ExtractSerializationStruct();
            }

            var ret = new Serialized
                (
                NumDecks,
                _hullSections,
                hullBuffData
                );
            return ret;
        }

        [ProtoContract]
        public struct Serialized{
            [ProtoMember(3)] public readonly ObjectBuffer<int>.Serialized[] HullBuffersByDeck;
            [ProtoMember(2)] public readonly HullSection[] HullSections;
            [ProtoMember(1)] public readonly int NumDecks;

            public Serialized(int numDecks, HullSection[] hullSections, ObjectBuffer<int>.Serialized[] hullBuffersByDeck){
                NumDecks = numDecks;
                HullSections = hullSections;
                HullBuffersByDeck = hullBuffersByDeck;
            }
        }

        #endregion
    }
}