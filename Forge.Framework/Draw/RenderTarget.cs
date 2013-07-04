#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Forge.Framework.Resources;
using Forge.Framework.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Matrix = MonoGameUtility.Matrix;
using Rectangle = MonoGameUtility.Rectangle;
using Vector2 = MonoGameUtility.Vector2;

#endregion

namespace Forge.Framework.Draw{
    /// <summary>
    /// This class is used to organize multiple rendertargets, and is used to dispatch draw calls to all buffers and sprites.
    /// </summary>
    public class RenderTarget : IDisposable{
        static readonly DepthStencilState _universalDepthStencil;
        static readonly SpriteBatch _cumulativeSpriteBatch;
        static readonly List<RenderTarget> _renderTargets;
        public static RenderTarget CurTarg;
        static readonly List<Task> _bufferUpdateTasks;
        public readonly Rectangle BoundingBox;
        public readonly SpriteBatch SpriteBatch;
        readonly List<IDrawableBuffer> _buffers;
        readonly List<IDrawableSprite> _sprites;

        readonly RenderTarget2D _targetCanvas;
        public float Depth;
        public Vector2 Offset;
        bool _disposed;

        static RenderTarget(){
            _cumulativeSpriteBatch = new SpriteBatch(Resource.Device);
            _renderTargets = new List<RenderTarget>();

            _universalDepthStencil = new DepthStencilState();
            _universalDepthStencil.DepthBufferEnable = true;
            _universalDepthStencil.DepthBufferWriteEnable = true;
            Resource.Device.BlendState = BlendState.Opaque;
            Resource.Device.SamplerStates[0] = SamplerState.LinearWrap;

            _bufferUpdateTasks = new List<Task>();
        }

        public RenderTarget(int x, int y, int width, int height, float depth = 1){
            SpriteBatch = new SpriteBatch(Resource.Device);
            _targetCanvas = new RenderTarget2D
                (
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
            _targetCanvas = new RenderTarget2D
                (
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

        public void Dispose(){
            Debug.Assert(!_disposed);
            _renderTargets.Remove(this);
            SpriteBatch.Dispose();
            _targetCanvas.Dispose();
            _disposed = true;
        }

        #endregion

        public static void AddAsynchronousBufferUpdate(Task update){
            lock (_bufferUpdateTasks){
                update.Start();
                _bufferUpdateTasks.Add(update);
            }
        }

        ~RenderTarget(){
            if (!_disposed)
                throw new ResourceNotDisposedException();
        }

        public void Bind(){
            CurTarg = this;
        }

        public void Unbind(){
            CurTarg = null;
        }

        public void Draw(Matrix viewMatrix, Color fillColor){
            lock (_bufferUpdateTasks){
                foreach (var updateTask in _bufferUpdateTasks){
                    updateTask.Wait();
                }
                _bufferUpdateTasks.Clear();
            }

            lock (Resource.Device){
                bool dontUnbindTarget = CurTarg != null; //this has to exist because of globalrendertarget abomination

                CurTarg = this;
                Resource.Device.SetRenderTarget(_targetCanvas);
                Resource.Device.Clear(fillColor);
                Resource.Device.DepthStencilState = _universalDepthStencil;
                Resource.Device.BlendState = BlendState.Opaque;
                Resource.Device.SamplerStates[0] = SamplerState.LinearWrap;
                foreach (var buffer in _buffers){
                    buffer.Draw(viewMatrix);
                }

                //here comes a nice little present from the xna team. Typically when you call spritebatch.begin, the rendering
                //settings stay associated with the spritebatch and cannot be changed until spritebatch.end is called. Our little
                //friend DrawString likes to change these settings in the middle of the begin/end without telling anyone, and
                //doesn't change the settings back to what they originally were. The new settings that the DrawString method sets
                //up in the spritebatch are typically shit and ruin all the depth detection stuff.
                //To bypass this, all DrawableSprites that make use of the DrawString function must be separated from all other
                //sprites. Sprites with DrawString get their own begin/end group so that we can fix the spritebatch settings
                //for all the other sprites.

                #region draw sprites

                SpriteBatch.Begin
                    (
                        SpriteSortMode.BackToFront,
                        BlendState.AlphaBlend,
                        SamplerState.LinearWrap,
                        DepthStencilState.Default,
                        RasterizerState.CullNone
                    );

                var sprites = (from s in _sprites
                    where s is Sprite2D
                    select s);


                foreach (var sprite in sprites){
                    sprite.Draw();
                }

                SpriteBatch.End();

                SpriteBatch.Begin
                    (
                        SpriteSortMode.BackToFront,
                        BlendState.AlphaBlend,
                        SamplerState.LinearWrap,
                        DepthStencilState.Default,
                        RasterizerState.CullNone
                    );

                sprites = (from s in _sprites
                    where s is TextBox
                    select s);

                foreach (var sprite in sprites){
                    sprite.Draw();
                }
                SpriteBatch.End();

                #endregion

                Resource.Device.SetRenderTarget(null);
                if (!dontUnbindTarget){
                    CurTarg = null;
                }
            }
        }

        public static void BeginDraw(){
        }

        public static void EndDraw(){
            Resource.Device.SetRenderTarget(null);
            _cumulativeSpriteBatch.Begin
                (
                    SpriteSortMode.BackToFront,
                    BlendState.NonPremultiplied,
                    SamplerState.LinearWrap,
                    DepthStencilState.Default,
                    RasterizerState.CullNone
                );
            foreach (var target in _renderTargets){
                _cumulativeSpriteBatch.Draw
                    (
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