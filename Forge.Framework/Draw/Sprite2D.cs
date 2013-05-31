#region

using Forge.Framework.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rectangle = MonoGameUtility.Rectangle;
using Vector2 = MonoGameUtility.Vector2;

#endregion

namespace Forge.Framework.Draw{
    internal class Sprite2D : IDrawableSprite{
        readonly FloatingRectangle _srcRect;
        public float Alpha;
        public float Depth;
        public bool Enabled;
        public int Height;
        public int Width;
        public int X;
        public int Y;

        Rectangle _destRect;
        bool _isDisposed;
        Texture2D _texture;

        /// <summary>
        ///   constructor for a normal sprite
        /// </summary>
        public Sprite2D(string textureName, int x, int y, int width, int height, float depth = 0.5f, float alpha = 1, float spriteRepeatX = 1, float spriteRepeatY = 1){
            _texture = Resource.LoadContent<Texture2D>(textureName);
            _srcRect = new FloatingRectangle(0f, 0f, _texture.Height*spriteRepeatX, _texture.Width*spriteRepeatY);
            _destRect = new Rectangle();
            _isDisposed = false;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Depth = depth;
            Alpha = alpha;
            Enabled = true;
            RenderTarget.Sprites.Add(this);
        }

        #region IDrawableSprite Members

        public Texture2D Texture{
            set { _texture = value; }
            get { return _texture; }
        }

        public void Dispose(){
            if (!_isDisposed){
                _isDisposed = true;
                RenderTarget.Sprites.Remove(this);
            }
        }

        public void SetTextureFromString(string textureName){
            _texture = Resource.LoadContent<Texture2D>(textureName);
        }

        public void Draw(){
            if (Enabled){
                _destRect.X = X;
                _destRect.Y = Y;
                _destRect.Width = Width;
                _destRect.Height = Height;
                RenderTarget.CurSpriteBatch.Draw(
                    _texture,
                    _destRect,
                    (Rectangle?) _srcRect,
                    Color.White*Alpha,
                    0,
                    Vector2.Zero,
                    SpriteEffects.None,
                    Depth
                    );
            }
        }

        #endregion

        ~Sprite2D(){
            if (!_isDisposed){
                _isDisposed = true;
            }
        }
    }
}