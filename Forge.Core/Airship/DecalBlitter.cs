#region

using System.Collections.Generic;
using System.Diagnostics;
using Forge.Core.Airship.Data;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProtoBuf;
using Vector2 = MonoGameUtility.Vector2;

#endregion

namespace Forge.Core.Airship{
    /// <summary>
    /// This class is used to blit damage-decals to an 8 bit surface texture.
    /// </summary>
    public class DecalBlitter{
        const float _decalSizeMultiplier = 32;
        readonly float _airshipDepth;
        readonly float _airshipLength;
        readonly SpriteBatch _batch;
        readonly float _decalScaleMult;
        readonly Texture2D _decalTexture;
        readonly List<Decal> _decals;
        readonly float _hullTextureMult;
        readonly RenderTarget2D _portDecalTexture;
        readonly RenderTarget2D _starboardDecalTexture;
        IEnumerable<EffectParameterCollection> _shaderParamBatch;

        public DecalBlitter(float airshipLength, float airshipDepth, float hullTextureMultiplier, List<Decal> startingDecals = null){
            _batch = new SpriteBatch(Resource.Device);
            _decals = startingDecals ?? new List<Decal>();
            _airshipLength = airshipLength;
            _airshipDepth = airshipDepth;
            _hullTextureMult = hullTextureMultiplier;
            TexturesOutOfDate = false;
            _decalScaleMult = airshipLength/hullTextureMultiplier;

            int width = (int) (airshipLength*_decalSizeMultiplier);
            int height = (int) (airshipDepth*_decalSizeMultiplier);
            Debug.Assert(width < 4096 && height < 4096);

            _portDecalTexture = new RenderTarget2D(Resource.Device, width, width, false, SurfaceFormat.Alpha8, DepthFormat.None);
            _starboardDecalTexture = new RenderTarget2D(Resource.Device, width, width, false, SurfaceFormat.Alpha8, DepthFormat.None);
            _decalTexture = Resource.LoadContent<Texture2D>("Materials/ImpactMask");
        }

        public bool TexturesOutOfDate { get; private set; }

        public void SetShaderParamBatch(IEnumerable<EffectParameterCollection> param){
            _shaderParamBatch = param;
            foreach (var shader in _shaderParamBatch){
                shader["f_DecalScaleMult"].SetValue(_decalScaleMult);
            }
            UpdateShaderTextures();
        }

        void UpdateShaderTextures(){
            foreach (var shader in _shaderParamBatch){
                shader["tex_PortDecalMask"].SetValue(_portDecalTexture);
                shader["tex_StarboardDecalMask"].SetValue(_starboardDecalTexture);
            }
        }

        public void Add(Vector2 position, Quadrant.Side side){
            //need to convert position from model space to decal texture space
            position = (position/_airshipLength)*_portDecalTexture.Width;

            //center the position
            position -= new Vector2(_decalTexture.Width/2f, _decalTexture.Width/2f);

            _decals.Add(new Decal(position, side));
            TexturesOutOfDate = true;
        }

        public void RegenerateDecalTextures(){
            BlitDecals(_portDecalTexture, Quadrant.Side.Port);
            BlitDecals(_starboardDecalTexture, Quadrant.Side.Starboard);
            Debug.Assert(_shaderParamBatch != null);
            UpdateShaderTextures();

            TexturesOutOfDate = false;
        }

        void BlitDecals(RenderTarget2D target, Quadrant.Side side){
            Resource.Device.SetRenderTarget(target);
            Resource.Device.Clear(Color.Transparent);
            _batch.Begin
                (
                    SpriteSortMode.Immediate,
                    BlendState.AlphaBlend,
                    SamplerState.PointWrap,
                    DepthStencilState.Default,
                    RasterizerState.CullNone
                );

            foreach (var decal in _decals){
                if (decal.Side == side){
                    _batch.Draw(_decalTexture, decal.Position, Color.White);
                }
            }
            _batch.End();
            Resource.Device.SetRenderTarget(null);

#if OUTPUT_DECAL_MASKS
            var streamWriter = new FileStream("decalMask" + side + ".png", FileMode.Create);
            target.SaveAsPng(streamWriter, target.Width, target.Width);
            streamWriter.Close();
#endif
        }

        #region serialization

        public DecalBlitter(Serialized serialized)
            : this
                (
                serialized.AirshipLength,
                serialized.AirshipDepth,
                serialized.HullTextureMult,
                serialized.Decals
                ){
        }

        public Serialized ExtractSerializationStruct(){
            return new Serialized
                (
                _airshipDepth,
                _airshipLength,
                _hullTextureMult,
                _decals
                );
        }

        [ProtoContract]
        public struct Serialized{
            [ProtoMember(1)] public readonly float AirshipDepth;
            [ProtoMember(2)] public readonly float AirshipLength;
            [ProtoMember(4)] public readonly List<Decal> Decals;
            [ProtoMember(3)] public readonly float HullTextureMult;

            public Serialized(float airshipDepth, float airshipLength, float hullTextureMult, List<Decal> decals) : this(){
                AirshipDepth = airshipDepth;
                AirshipLength = airshipLength;
                HullTextureMult = hullTextureMult;
                Decals = decals;
            }
        }

        #endregion

        #region Nested type: Decal

        [ProtoContract]
        public struct Decal{
            [ProtoMember(1)] public readonly Vector2 Position;
            [ProtoMember(2)] public readonly Quadrant.Side Side;

            public Decal(Vector2 pos, Quadrant.Side side){
                Position = pos;
                Side = side;
            }
        }

        #endregion
    }
}