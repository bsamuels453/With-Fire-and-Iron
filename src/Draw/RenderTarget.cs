#region

using System;
using System.Diagnostics;
using Gondola.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

#endregion

namespace Gondola.Draw{
    internal class RenderTarget : IDisposable{
        static readonly DepthStencilState _universalDepthStencil;
        public readonly Rectangle BoundingBox;
        public readonly SpriteBatch SpriteBatch;
        public float Depth;

        readonly RenderTarget2D _targetCanvas;
        public Vector2 Offset;

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

        public RenderTarget(){
            SpriteBatch = new SpriteBatch(Gbl.Device);
            _targetCanvas = new RenderTarget2D(
                Gbl.Device,
                Gbl.ScreenSize.X,
                Gbl.ScreenSize.Y,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24Stencil8
                );
            Depth = 1;

            Offset = new Vector2(0, 0);
            BoundingBox = new Rectangle(0, 0, Gbl.ScreenSize.X, Gbl.ScreenSize.Y);
            _renderTargets.Add(this);
        }

        public void Bind(){
            Gbl.Device.SetRenderTarget(_targetCanvas);
            Gbl.Device.Clear(Color.CornflowerBlue);
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

        public void Dispose(){
            _renderTargets.Remove(this);
        }

        static readonly SpriteBatch _cumulativeSpriteBatch;
        static readonly List<RenderTarget> _renderTargets;

        static RenderTarget(){
            _cumulativeSpriteBatch = new SpriteBatch(Gbl.Device);
            _renderTargets = new List<RenderTarget>();

            _universalDepthStencil = new DepthStencilState();
            _universalDepthStencil.DepthBufferEnable = true;
            _universalDepthStencil.DepthBufferWriteEnable = true;
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