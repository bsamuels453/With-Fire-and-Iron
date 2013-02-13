#region

using Gondola.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Gondola.Draw{
    internal class Text2D : IDrawableSprite{
        readonly SpriteFont _font;
        readonly SpriteBatch _spriteBatch;
        public Vector2 Position;
        public string Str;

        public Color Color;

        public Text2D(RenderTarget target, int x, int y, string str, string font = "UI_SpriteFont"){
            Position = new Vector2(x, y);
            Str = str;
            Color = Color.Black;
            _spriteBatch = target.SpriteBatch;
            _font = Gbl.ContentManager.Load<SpriteFont>(Gbl.RawLookup[font]);
        }

        #region IDrawableSprite Members

        public void Draw(){
            _spriteBatch.DrawString(
                _font,
                Str,
                Position,
                Color
                );
        }

        #endregion
    }
}