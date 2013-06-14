#region

using Forge.Framework.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = MonoGameUtility.Vector2;

#endregion

namespace Forge.Core.Util{
    /// <summary>
    /// Two channel texture blitting class.
    /// </summary>
    internal class TextureBlitter{
        public readonly Texture2D TargetTexture;
        readonly SpriteBatch _batch;
        readonly RenderTarget2D _renderTarget;
        readonly Texture2D _srcTexture;
        readonly int _targHeight;
        readonly int _targWidth;

        public TextureBlitter(int targetWidth, int targetHeight, string srcTexture){
            _targWidth = targetWidth;
            _targHeight = targetHeight;

            lock (Resource.Device){
                _batch = new SpriteBatch(Resource.Device);
                _renderTarget = new RenderTarget2D(Resource.Device, _targWidth, _targHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                var tempTarg = new RenderTarget2D(Resource.Device, _targWidth, _targHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

                Resource.Device.SetRenderTarget(tempTarg);
                Resource.Device.Clear(Color.Transparent);
                TargetTexture = tempTarg;
                Resource.Device.SetRenderTarget(null);
            }
            _srcTexture = Resource.LoadContent<Texture2D>(srcTexture);
        }

        public void Blit(Vector2 target){
            lock (Resource.Device){
                Resource.Device.SetRenderTarget(_renderTarget);
                _batch.Begin
                    (
                        SpriteSortMode.BackToFront,
                        BlendState.Opaque,
                        SamplerState.PointWrap,
                        DepthStencilState.Default,
                        RasterizerState.CullNone
                    );

                _batch.Draw(TargetTexture, new Vector2(0, 0), Color.White);
                _batch.Draw(_srcTexture, target, Color.White);

                _batch.End();
                Resource.Device.SetRenderTarget(null);
                /*
                var streamWriter = new FileStream("finalDecal.png", FileMode.Create);
                _renderTarget.SaveAsPng(streamWriter, _targWidth, _targHeight);
                 */
            }
        }
    }
}