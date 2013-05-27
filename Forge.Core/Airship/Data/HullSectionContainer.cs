using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Forge.Core.Logic;
using Forge.Framework.Draw;
using Microsoft.Xna.Framework.Graphics;

namespace Forge.Core.Airship.Data {
    class HullSectionContainer :IEnumerable, IDisposable{
        public readonly int NumDecks;
        public int TopExpIdx { get; private set; }
        readonly HullSection[] _hullSections;
        public readonly ObjectBuffer<int>[] HullBuffersByDeck;
        public readonly HullSection[][] HullSectionByDeck;
        public ObjectBuffer<int> TopExposedHullLayer { get; private set; }

        public HullSectionContainer(List<HullSection> hullSections, ObjectBuffer<int>[] hullBuffersByDeck){
            HullBuffersByDeck = hullBuffersByDeck;
            _hullSections = hullSections.ToArray();
            var groupedByDeck = (from section in hullSections
                                 group section by section.Deck).ToArray();

            HullSectionByDeck = new HullSection[groupedByDeck.Length][];
            foreach (var grouping in groupedByDeck){
                HullSectionByDeck[grouping.Key] = grouping.ToArray();
            }
            foreach (var hullBuff in HullSectionByDeck){
                Debug.Assert(hullBuff != null);
            }
            NumDecks = HullBuffersByDeck.Length;
            TopExpIdx = 0;
            TopExposedHullLayer = HullBuffersByDeck[TopExpIdx];
            Debug.Assert(HullBuffersByDeck.Length == HullSectionByDeck.Length);
        }

        public int SetTopVisibleDeck(int deck){
            if (deck < 0 || deck >= NumDecks)
                return TopExpIdx;
            //Debug.Assert(deck >= 0);
            //Debug.Assert(deck < NumDecks);

            for (int i = NumDecks-1; i >= deck; i--){
                HullBuffersByDeck[i].CullMode = CullMode.None;
            }
            for (int i = deck-1; i >= 0; i--) {
                HullBuffersByDeck[i].CullMode = CullMode.CullCounterClockwiseFace;
            }
            TopExpIdx = deck;
            TopExposedHullLayer = HullBuffersByDeck[TopExpIdx];
            return TopExpIdx;
        }

        public IEnumerator GetEnumerator() {
            return _hullSections.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator(){
            return GetEnumerator();
        }

        bool _disposed;

        public void Dispose(){
            Debug.Assert(!_disposed);
            foreach (var buffer in HullBuffersByDeck){
                buffer.Dispose();
            }

            _disposed = true;
        }

        ~HullSectionContainer(){
            Debug.Assert(_disposed);
        }
    }
}
