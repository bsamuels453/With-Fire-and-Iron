#region

using Gondola.Common;
using Gondola.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Gondola.Draw{
    internal class Sprite2D : IDrawableSprite {
        readonly FloatingRectangle _srcRect;
        public float Depth;
        public int Height;
        public bool Enabled;
        public float Opacity;
        public int Width;
        public int X;
        public int Y;

        Rectangle _destRect;
        bool _isDisposed;
        Texture2D _texture;

        /// <summary>
        ///   constructor for a normal sprite
        /// </summary>
        public Sprite2D(string textureName, int x, int y, int width, int height, float depth = 0.5f, float opacity = 1, float spriteRepeatX = 1, float spriteRepeatY = 1) {
            _texture = Gbl.LoadContent<Texture2D>(textureName);
            _srcRect = new FloatingRectangle(0f, 0f, _texture.Height * spriteRepeatX, _texture.Width * spriteRepeatY);
            _destRect = new Rectangle();
            _isDisposed = false;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Depth = depth;
            Opacity = opacity;
            Enabled = true;
            RenderTarget.Sprites.Add(this);
        }

        #region IDrawableSprite Members

        public Texture2D Texture {
            set { _texture = value; }
            get { return _texture; }
        }

        public void Dispose() {
            if (!_isDisposed) {
                _isDisposed = true;
                RenderTarget.Sprites.Remove(this);
            }
        }

        public void SetTextureFromString(string textureName) {
            _texture = Gbl.ContentManager.Load<Texture2D>(textureName);
        }

        public void Draw() {
            if (Enabled) {
                _destRect.X = X;
                _destRect.Y = Y;
                _destRect.Width = Width;
                _destRect.Height = Height;
                RenderTarget.CurSpriteBatch.Draw(
                    _texture,
                    _destRect,
                    (Rectangle?)_srcRect,
                    Color.White * Opacity,
                    0,
                    Vector2.Zero,
                    SpriteEffects.None,
                    Depth
                    );
            }
        }

        #endregion

        ~Sprite2D() {
            if (!_isDisposed) {
                _isDisposed = true;
            }
        }
    }
}