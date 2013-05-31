#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Matrix = MonoGameUtility.Matrix;
using Rectangle = MonoGameUtility.Rectangle;
using Vector2 = MonoGameUtility.Vector2;

#endregion

namespace Forge.Framework.Draw{
    public class RenderTarget : IDisposable{
        static readonly DepthStencilState _universalDepthStencil;
        static readonly SpriteBatch _cumulativeSpriteBatch;
        static readonly List<RenderTarget> _renderTargets;
        public static RenderTarget CurTarg;
        public readonly Rectangle BoundingBox;
        public readonly SpriteBatch SpriteBatch;
        readonly List<IDrawableBuffer> _buffers;
        readonly List<IDrawableSprite> _sprites;

        readonly RenderTarget2D _targetCanvas;
        public float Depth;
        public Vector2 Offset;

        static RenderTarget(){
            _cumulativeSpriteBatch = new SpriteBatch(Resource.Device);
            _renderTargets = new List<RenderTarget>();

            _universalDepthStencil = new DepthStencilState();
            _universalDepthStencil.DepthBufferEnable = true;
            _universalDepthStencil.DepthBufferWriteEnable = true;
        }

        public RenderTarget(int x, int y, int width, int height, float depth = 1){
            SpriteBatch = new SpriteBatch(Resource.Device);
            _targetCanvas = new RenderTarget2D(
                Resource.Device,
                width,
                height,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24Stencil8
                );
            Depth = depth;
            Offset = new Vector2(x, y);
            BoundingBox = new Rectangle(x, y, width, height);
            _buffers = new List<IDrawableBuffer>();
            _sprites = new List<IDrawableSprite>();
            _renderTargets.Add(this);
        }

        /// <summary>
        ///   lower depth is closer to screen
        /// </summary>
        /// <param name="depth"> </param>
        public RenderTarget(float depth = 1){
            SpriteBatch = new SpriteBatch(Resource.Device);
            _targetCanvas = new RenderTarget2D(
                Resource.Device,
                Resource.ScreenSize.X,
                Resource.ScreenSize.Y,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24Stencil8
                );
            Depth = depth;

            Offset = new Vector2(0, 0);
            BoundingBox = new Rectangle(0, 0, Resource.ScreenSize.X, Resource.ScreenSize.Y);
            _buffers = new List<IDrawableBuffer>();
            _sprites = new List<IDrawableSprite>();
            _renderTargets.Add(this);
        }

        public static List<IDrawableBuffer> Buffers{
            get { return CurTarg._buffers; }
        }

        public static List<IDrawableSprite> Sprites{
            get { return CurTarg._sprites; }
        }

        public static SpriteBatch CurSpriteBatch{
            get { return CurTarg.SpriteBatch; }
        }

        #region IDisposable Members

        bool _disposed;

        public void Dispose(){
            Debug.Assert(!_disposed);
            _renderTargets.Remove(this);
            SpriteBatch.Dispose();
            _targetCanvas.Dispose();
            _disposed = true;
        }

        ~RenderTarget(){
            if (!_disposed)
                throw new ResourceNotDisposedException();
        }

        #endregion

        public void Bind(){
            CurTarg = this;
        }

        public void Unbind(){
            CurTarg = null;
        }

        public void Draw(Matrix viewMatrix, Color fillColor){
            bool dontUnbindTarget = CurTarg != null;//this has to exist because of globalrendertarget abomination

            CurTarg = this;
            Resource.Device.SetRenderTarget(_targetCanvas);
            Resource.Device.Clear(fillColor);
            Resource.Device.DepthStencilState = _universalDepthStencil;
            SpriteBatch.Begin(
                SpriteSortMode.BackToFront,
                BlendState.AlphaBlend,
                SamplerState.LinearWrap,
                DepthStencilState.Default,
                RasterizerState.CullNone
                );
            foreach (var buffer in _buffers){
                buffer.Draw(viewMatrix);
            }
            foreach (var sprite in _sprites){
                sprite.Draw();
            }
            SpriteBatch.End();
            Resource.Device.SetRenderTarget(null);
            if (!dontUnbindTarget){
                CurTarg = null;
            }
        }

        public static void BeginDraw(){
        }

        public static void EndDraw(){
            Resource.Device.SetRenderTarget(null);
            _cumulativeSpriteBatch.Begin(
                SpriteSortMode.BackToFront,
                BlendState.NonPremultiplied,
                SamplerState.LinearWrap,
                DepthStencilState.Default,
                RasterizerState.CullNone
                );
            foreach (var target in _renderTargets){
                _cumulativeSpriteBatch.Draw(
                    target._targetCanvas,
                    target.Offset,
                    null,
                    Color.White,
                    0,
                    Vector2.Zero,
                    1,
                    SpriteEffects.None,
                    target.Depth
                    );
            }
            _cumulativeSpriteBatch.End();
        }
    }
}