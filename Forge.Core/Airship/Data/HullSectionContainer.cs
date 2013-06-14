#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Forge.Core.Util;
using Forge.Framework.Draw;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;
using ProtoBuf;

#endregion

namespace Forge.Core.Airship.Data{
    public class HullSectionContainer : IEnumerable, IDisposable{
        public readonly ObjectBuffer<int>[] HullBuffersByDeck;
        public readonly int NumDecks;
        readonly HullSection[] _hullSections;
        readonly TextureBlitter _textureBlitter;
        bool _disposed;

        /// <summary>
        /// Provides an interface to hull geometry and hull geometry modification methods.
        /// </summary>
        /// <param name="hullSections"></param>
        /// <param name="hullBuffersByDeck"></param>
        /// <param name="modelAttributes"> </param>
        /// <param name="damagePoints">Points at which the airship has been damaged. Measured in texel space of the decal.</param>
        public HullSectionContainer(
            List<HullSection> hullSections,
            ObjectBuffer<int>[] hullBuffersByDeck,
            ModelAttributes modelAttributes,
            List<Vector2> damagePoints
            ){
            _hullSections = hullSections.ToArray();
            HullBuffersByDeck = hullBuffersByDeck;
            TopExpIdx = 0;
            TopExposedHullLayer = HullBuffersByDeck[TopExpIdx];
            NumDecks = HullBuffersByDeck.Length;

            int width = (int) (modelAttributes.Length*32*2);
            int height = (int) (modelAttributes.Depth*32);
            _textureBlitter = new TextureBlitter(width, height, "Materials/ImpactCross");

            foreach (var point in damagePoints){
                _textureBlitter.BlitLocations.Add(point);
            }

            UpdateDecalTexture(_textureBlitter.GetBlittedTexture());
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

        void UpdateDecalTexture(Texture2D texture){
            foreach (var buffer in HullBuffersByDeck){
                buffer.ShaderParams["tex_DecalWrap"].SetValue(texture);
            }
        }

        /*
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
         */

        public void AddDamageDecal(Vector2 position){
            _textureBlitter.BlitLocations.Add(position);
            UpdateDecalTexture(_textureBlitter.GetBlittedTexture());
        }

        public int SetTopVisibleDeck(int deck){
            if (deck < 0 || deck >= NumDecks)
                return TopExpIdx;

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

            TopExpIdx = 0;
            TopExposedHullLayer = HullBuffersByDeck[TopExpIdx];
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
                hullBuffData,
                _textureBlitter.BlitLocations,
                _textureBlitter.TargWidth,
                _textureBlitter.TargHeight
                );
            return ret;
        }

        [ProtoContract]
        public struct Serialized{
            [ProtoMember(1)] public readonly ObjectBuffer<int>.Serialized[] HullBuffersByDeck;
            [ProtoMember(2)] public readonly HullSection[] HullSections;
            [ProtoMember(3)] public readonly int NumDecks;
            [ProtoMember(4)] public readonly List<Vector2> DamageDecalPositions;
            [ProtoMember(5)] public readonly int DamageDecalWidth;
            [ProtoMember(6)] public readonly int DamageDecalHeight;

            public Serialized(int numDecks, HullSection[] hullSections, ObjectBuffer<int>.Serialized[] hullBuffersByDeck, List<Vector2> damageDecalPositions, int damageDecalWidth, int damageDecalHeight){
                NumDecks = numDecks;
                HullSections = hullSections;
                HullBuffersByDeck = hullBuffersByDeck;
                DamageDecalPositions = damageDecalPositions;
                DamageDecalWidth = damageDecalWidth;
                DamageDecalHeight = damageDecalHeight;
            }
        }

        #endregion
    }
}