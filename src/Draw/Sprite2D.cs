#region

using Gondola.Common;
using Gondola.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Gondola.Draw{
    internal class Sprite2D : IDrawableSprite{
        readonly SpriteBatch _spriteBatch;
        readonly FloatingRectangle _srcRect;
        public int Height;
        public bool IsEnabled;
        public float Opacity;
        public int Width;
        public int X;
        public int Y;

        Rectangle _destRect;
        Texture2D _texture;

        /// <summary>
        ///   constructor for a normal sprite
        /// </summary>
        public Sprite2D(RenderTarget target, string textureName, int x, int y, int width, int height, float opacity = 1, float spriteRepeatX = 1, float spriteRepeatY = 1){
            _spriteBatch = target.SpriteBatch;
            _texture = Gbl.ContentManager.Load<Texture2D>(Gbl.ContentStrLookup[textureName]);
            _srcRect = new FloatingRectangle(0f, 0f, _texture.Height*spriteRepeatX, _texture.Width*spriteRepeatY);
            _destRect = new Rectangle();
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Opacity = opacity;
            IsEnabled = true;
        }

        public Texture2D Texture{
            set { _texture = value; }
            get { return _texture; }
        }

        #region IDrawableSprite Members

        public void Draw(){
            if (IsEnabled){
                _destRect.X = X;
                _destRect.Y = Y;
                _destRect.Width = Width;
                _destRect.Height = Height;
                _spriteBatch.Draw(
                    _texture,
                    _destRect,
                    (Rectangle?) _srcRect,
                    Color.White*Opacity
                    );
            }
        }

        #endregion

        public void SetTextureFromString(string textureName){
            _texture = Gbl.ContentManager.Load<Texture2D>(textureName);
        }
    }
}