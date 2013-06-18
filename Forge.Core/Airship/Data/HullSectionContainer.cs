#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Forge.Framework.Draw;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;
using ProtoBuf;

#endregion

namespace Forge.Core.Airship.Data{
    /// <summary>
    /// Acts as a wrapper for the geometry of an airship's hull. Any kind of modifications
    /// to the airship's hull have to go through this class.
    /// </summary>
    public class HullSectionContainer : IEnumerable, IDisposable{
        DecalBlitter _decalBlitter;
        bool _disposed;
        HullSection[] _hullSections;

        #region ctor

        /// <summary>
        /// Provides an interface to hull geometry and hull geometry modification methods.
        /// </summary>
        /// <param name="hullSections"></param>
        /// <param name="hullBuffersByDeck"></param>
        /// <param name="modelAttributes"> </param>
        /// <param name="hullTextureUVMult">Multiplier used to scale the uvcoords of airship's hull</param>
        public HullSectionContainer(
            List<HullSection> hullSections,
            ObjectBuffer<int>[] hullBuffersByDeck,
            ModelAttributes modelAttributes,
            float hullTextureUVMult
            ){
            Initialize
                (
                    hullSections.ToArray(),
                    hullBuffersByDeck,
                    modelAttributes.Length,
                    modelAttributes.Depth,
                    hullTextureUVMult
                );
        }

        public HullSectionContainer(Serialized serializedStruct){
            int numDecks = serializedStruct.NumDecks;
            var hullBuffersByDeck = new ObjectBuffer<int>[numDecks];
            for (int i = 0; i < numDecks; i++){
                hullBuffersByDeck[i] = new ObjectBuffer<int>(serializedStruct.HullBuffersByDeck[i]);
            }

            _hullSections = serializedStruct.HullSections;
            TopExpIdx = 0;
            HullBuffersByDeck = hullBuffersByDeck;
            NumDecks = HullBuffersByDeck.Length;
            TopExposedHullLayer = HullBuffersByDeck[TopExpIdx];

            var totShaderParams = from buffer in HullBuffersByDeck
                select buffer.ShaderParams;

            _decalBlitter = new DecalBlitter(serializedStruct.DecalBlitter);
            _decalBlitter.SetShaderParamBatch(totShaderParams);
        }

        void Initialize(
            HullSection[] hullSections,
            ObjectBuffer<int>[] hullBuffersByDeck,
            float airshipLength,
            float airshipDepth,
            float textureToDecal
            ){
            _hullSections = hullSections;
            TopExpIdx = 0;
            HullBuffersByDeck = hullBuffersByDeck;
            NumDecks = HullBuffersByDeck.Length;
            TopExposedHullLayer = HullBuffersByDeck[TopExpIdx];

            var totShaderParams = from buffer in HullBuffersByDeck
                select buffer.ShaderParams;

            _decalBlitter = new DecalBlitter(airshipLength, airshipDepth, textureToDecal);
            _decalBlitter.SetShaderParamBatch(totShaderParams);
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

        public void AddDamageDecal(Vector2 position, Quadrant.Side side){
            _decalBlitter.Add(position, side);
            _decalBlitter.RegenerateDecalTextures();
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
                _decalBlitter.ExtractSerializationStruct(),
                hullBuffData,
                _hullSections,
                NumDecks
                );
            return ret;
        }

        [ProtoContract]
        public struct Serialized{
            [ProtoMember(1)] public readonly DecalBlitter.Serialized DecalBlitter;
            [ProtoMember(2)] public readonly ObjectBuffer<int>.Serialized[] HullBuffersByDeck;
            [ProtoMember(3)] public readonly HullSection[] HullSections;
            [ProtoMember(4)] public readonly int NumDecks;

            public Serialized(DecalBlitter.Serialized decalBlitter, ObjectBuffer<int>.Serialized[] hullBuffersByDeck, HullSection[] hullSections, int numDecks)
                : this(){
                DecalBlitter = decalBlitter;
                HullBuffersByDeck = hullBuffersByDeck;
                HullSections = hullSections;
                NumDecks = numDecks;
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