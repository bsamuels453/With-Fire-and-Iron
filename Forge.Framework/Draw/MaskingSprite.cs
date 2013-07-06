#region

using Forge.Framework.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Forge.Framework.Draw{
    /// <summary>
    /// Special sprite class to be used specifically for masking effects.
    /// This differs from the normal Sprite2D in that it always is rendered on top of all
    /// other sprites. It is also able to mask on top of text without overwriting the text.
    /// </summary>
    internal class MaskingSprite : IDrawableSprite{
        readonly Rectangle _srcRect;
        readonly Texture2D _texture;

        public bool Enabled;

        Rectangle _destRect;
        bool _isDisposed;

        #region ctors

        public MaskingSprite(
            string textureName,
            Rectangle boundingBox,
            float alpha = 1
            )
            : this(
                Resource.LoadContent<Texture2D>(textureName),
                boundingBox,
                alpha){
        }


        MaskingSprite(
            Texture2D texture,
            Rectangle boundingBox,
            float alpha = 1
            ){
            _texture = texture;
            _srcRect = new Rectangle(0, 0, _texture.Width, _texture.Height);
            _destRect = new Rectangle();
            _isDisposed = false;
            _destRect = boundingBox;
            Alpha = alpha;
            Enabled = true;
            RenderTarget.AddSprite(this);
        }

        #endregion

        public int X{
            get { return _destRect.X; }
            set { _destRect.X = value; }
        }

        public int Y{
            get { return _destRect.Y; }
            set { _destRect.Y = value; }
        }

        public float Alpha { get; set; }

        #region IDrawableSprite Members

        public void Dispose(){
            if (!_isDisposed){
                _isDisposed = true;
                RenderTarget.RemoveSprite(this);
            }
        }

        public void Draw(){
            if (Enabled){
                RenderTarget.CurSpriteBatch.Draw
                    (
                        _texture,
                        _destRect,
                        _srcRect,
                        Color.White*Alpha,
                        0,
                        Vector2.Zero,
                        SpriteEffects.None,
                        0
                    );
            }
        }

        #endregion

        ~MaskingSprite(){
            if (!_isDisposed){
                throw new ResourceNotDisposedException();
                _isDisposed = true;
            }
        }
    }
}