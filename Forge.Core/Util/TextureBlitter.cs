#region

using System.Collections.Generic;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = MonoGameUtility.Vector2;

#endregion

namespace Forge.Core.Util{
    /// <summary>
    /// This class is used to blit textures onto a blank rgba64 transparent texture.
    /// </summary>
    internal class TextureBlitter{
        public readonly List<Vector2> BlitLocations;
        readonly SpriteBatch _batch;
        readonly RenderTarget2D _finalTexture;
        readonly Texture2D _srcTexture;
        readonly int _targHeight;
        readonly int _targWidth;

        public TextureBlitter(int targetWidth, int targetHeight, string srcTexture){
            _targWidth = targetWidth;
            _targHeight = targetHeight;
            BlitLocations = new List<Vector2>();

            lock (Resource.Device){
                _batch = new SpriteBatch(Resource.Device);
                _finalTexture = new RenderTarget2D(Resource.Device, _targWidth, _targHeight, false, SurfaceFormat.Rgba64, DepthFormat.Depth24Stencil8);
            }
            _srcTexture = Resource.LoadContent<Texture2D>(srcTexture);
        }

        public Texture2D GetBlittedTexture(){
            lock (Resource.Device){
                Resource.Device.SetRenderTarget(_finalTexture);
                Resource.Device.Clear(Color.Transparent);
                _batch.Begin
                    (
                        SpriteSortMode.Immediate,
                        BlendState.AlphaBlend,
                        SamplerState.PointWrap,
                        DepthStencilState.Default,
                        RasterizerState.CullNone
                    );
                foreach (var vector2 in BlitLocations){
                    _batch.Draw(_srcTexture, vector2, Color.White);
                }

                _batch.End();
                Resource.Device.SetRenderTarget(null);
                /*
                var streamWriter = new FileStream("finalDecal.png", FileMode.Create);
                _finalTexture.SaveAsPng(streamWriter, _targWidth, _targHeight);
                streamWriter.Close();
                */
            }
            return _finalTexture;
        }
    }
}