#region

using System;
using System.Collections.Generic;
using Gondola.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Gondola.Draw{
    internal class RenderTarget : IDisposable{
        static readonly DepthStencilState _universalDepthStencil;
        static readonly SpriteBatch _cumulativeSpriteBatch;
        static readonly List<RenderTarget> _renderTargets;
        public readonly Rectangle BoundingBox;
        public readonly SpriteBatch SpriteBatch;

        readonly RenderTarget2D _targetCanvas;
        public float Depth;
        public Vector2 Offset;

        static RenderTarget(){
            _cumulativeSpriteBatch = new SpriteBatch(Gbl.Device);
            _renderTargets = new List<RenderTarget>();

            _universalDepthStencil = new DepthStencilState();
            _universalDepthStencil.DepthBufferEnable = true;
            _universalDepthStencil.DepthBufferWriteEnable = true;
        }

        public RenderTarget(int x, int y, int width, int height, float depth = 1){
            SpriteBatch = new SpriteBatch(Gbl.Device);
            _targetCanvas = new RenderTarget2D(
                Gbl.Device,
                width,
                height,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24Stencil8
                );
            Depth = depth;
            Offset = new Vector2(x, y);
            BoundingBox = new Rectangle(x, y, width, height);
            _renderTargets.Add(this);
        }

        /// <summary>
        /// lower depth is closer to screen
        /// </summary>
        /// <param name="depth"></param>
        public RenderTarget(float depth = 1){
            SpriteBatch = new SpriteBatch(Gbl.Device);
            _targetCanvas = new RenderTarget2D(
                Gbl.Device,
                Gbl.ScreenSize.X,
                Gbl.ScreenSize.Y,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24Stencil8
                );
            Depth = depth;

            Offset = new Vector2(0, 0);
            BoundingBox = new Rectangle(0, 0, Gbl.ScreenSize.X, Gbl.ScreenSize.Y);
            _renderTargets.Add(this);
        }

        #region IDisposable Members

        public void Dispose(){
            _renderTargets.Remove(this);
            SpriteBatch.Dispose();
            _targetCanvas.Dispose();
        }

        #endregion

        public void Bind(){
            Gbl.Device.SetRenderTarget(_targetCanvas);
            Gbl.Device.Clear(Color.Transparent);
            Gbl.Device.DepthStencilState = _universalDepthStencil;
            SpriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                SamplerState.LinearWrap,
                DepthStencilState.Default,
                RasterizerState.CullNone
                );
        }

        public void Unbind(){
            SpriteBatch.End();
            Gbl.Device.SetRenderTarget(null);
        }

        public static void BeginDraw(){
        }

        public static void EndDraw(){
            Gbl.Device.SetRenderTarget(null);
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