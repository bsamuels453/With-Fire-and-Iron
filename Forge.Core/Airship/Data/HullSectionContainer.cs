#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Forge.Core.Util;
using Forge.Framework.Draw;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;
using ProtoBuf;

#endregion

namespace Forge.Core.Airship.Data{
    public class HullSectionContainer : IEnumerable, IDisposable{
        const int _decalSizeMultiplier = 32;
        bool _disposed;
        float _hullLength;
        HullSection[] _hullSections;
        TextureBlitter _textureBlitter;
        float _textureToDecalTexel;

        #region ctor

        /// <summary>
        /// Provides an interface to hull geometry and hull geometry modification methods.
        /// </summary>
        /// <param name="hullSections"></param>
        /// <param name="hullBuffersByDeck"></param>
        /// <param name="modelAttributes"> </param>
        /// <param name="damagePoints">Points at which the airship has been damaged. Measured in texel space of the decal.</param>
        /// <param name="hullTextureUVMult">Multiplier used to scale the uvcoords of airship's hull</param>
        public HullSectionContainer(
            List<HullSection> hullSections,
            ObjectBuffer<int>[] hullBuffersByDeck,
            ModelAttributes modelAttributes,
            List<Vector2> damagePoints,
            float hullTextureUVMult
            ){
            int width = (int) (modelAttributes.Length*_decalSizeMultiplier);
            int height = (int) (modelAttributes.Depth*_decalSizeMultiplier);
            float texToDecal = modelAttributes.Length/hullTextureUVMult;
            Initialize
                (
                    hullSections.ToArray(),
                    hullBuffersByDeck,
                    damagePoints,
                    width,
                    height,
                    texToDecal,
                    modelAttributes.Length
                );
        }

        public HullSectionContainer(Serialized serializedStruct){
            int numDecks = serializedStruct.NumDecks;
            var hullBuffersByDeck = new ObjectBuffer<int>[numDecks];
            for (int i = 0; i < numDecks; i++){
                hullBuffersByDeck[i] = new ObjectBuffer<int>(serializedStruct.HullBuffersByDeck[i]);
            }

            var damagePts = serializedStruct.DamageDecalPositions ?? new List<Vector2>();

            Initialize
                (
                    serializedStruct.HullSections,
                    hullBuffersByDeck,
                    damagePts,
                    serializedStruct.DecalWidth,
                    serializedStruct.DecalHeight,
                    serializedStruct.TextureToDecalTexel,
                    serializedStruct.HullLength
                );
        }

        void Initialize(
            HullSection[] hullSections,
            ObjectBuffer<int>[] hullBuffersByDeck,
            List<Vector2> damagePoints,
            int decalWidth,
            int decalHeight,
            float textureToDecal,
            float hullLength
            ){
            _hullSections = hullSections;
            _hullLength = hullLength;
            _textureToDecalTexel = textureToDecal;
            TopExpIdx = 0;
            HullBuffersByDeck = hullBuffersByDeck;
            NumDecks = HullBuffersByDeck.Length;
            TopExposedHullLayer = HullBuffersByDeck[TopExpIdx];

            foreach (var buffer in hullBuffersByDeck){
                buffer.ShaderParams["f_DecalScaleMult"].SetValue(_textureToDecalTexel);
            }

            _textureBlitter = new TextureBlitter(decalWidth, decalHeight, "Materials/ImpactCross");

            foreach (var point in damagePoints){
                _textureBlitter.BlitLocations.Add(point);
            }
            UpdateDecalTexture(_textureBlitter.GetBlittedTexture());
        }

        #endregion

        public ObjectBuffer<int>[] HullBuffersByDeck { get; private set; }
        public int NumDecks { get; private set; }

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
            lock (Resource.Device){
                foreach (var buffer in HullBuffersByDeck){
                    buffer.ShaderParams["tex_DecalWrap"].SetValue(texture);
                }
            }
        }

        /// <summary>
        /// Adds a damage decal to the airship's hull at the specified coordinates in model space.
        /// </summary>
        /// <param name="position">
        /// The position of the decal, where the origin is located at the top rear extrema of the hull.
        /// The X axis goes down the length of the airship from stern to bow.
        /// The Y axis goes from the top deck down to the keel of the ship.
        /// </param>
        public void AddDamageDecal(Vector2 position){
            //need to convert position from model space to decal texture space
            position = (position/_hullLength)*_textureBlitter.TargWidth;

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
                _textureBlitter.TargHeight,
                _textureToDecalTexel,
                _hullLength
                );
            return ret;
        }

        [ProtoContract]
        public struct Serialized{
            [ProtoMember(1)] public readonly List<Vector2> DamageDecalPositions;
            [ProtoMember(2)] public readonly int DecalHeight;
            [ProtoMember(3)] public readonly int DecalWidth;
            [ProtoMember(4)] public readonly ObjectBuffer<int>.Serialized[] HullBuffersByDeck;
            [ProtoMember(8)] public readonly float HullLength;
            [ProtoMember(5)] public readonly HullSection[] HullSections;
            [ProtoMember(6)] public readonly int NumDecks;
            [ProtoMember(7)] public readonly float TextureToDecalTexel;

            public Serialized(
                int numDecks,
                HullSection[] hullSections,
                ObjectBuffer<int>.Serialized[] hullBuffersByDeck,
                List<Vector2> damageDecalPositions,
                int decalWidth,
                int decalHeight,
                float textureToDecalTexel, float hullLength){
                NumDecks = numDecks;
                HullSections = hullSections;
                HullBuffersByDeck = hullBuffersByDeck;
                DamageDecalPositions = damageDecalPositions;
                DecalWidth = decalWidth;
                DecalHeight = decalHeight;
                TextureToDecalTexel = textureToDecalTexel;
                HullLength = hullLength;
            }
        }

        #endregion

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
    }
}