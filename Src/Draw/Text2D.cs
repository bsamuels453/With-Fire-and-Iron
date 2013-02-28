#region

using Gondola.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Gondola.Draw{
    internal class Text2D : IDrawableSprite{
        readonly SpriteFont _font;
        public Vector2 Position;
        public string Str;

        public Color Color;

        public Text2D(int x, int y, string str, string font = "Fonts/SpriteFont"){
            Position = new Vector2(x, y);
            Str = str;
            Color = Color.Black;
            try {
                _font = Gbl.ContentManager.Load<SpriteFont>(Gbl.RawLookup[font]);
            }
            catch{
                _font = Gbl.ContentManager.Load<SpriteFont>(font);
            }
            RenderTarget.Sprites.Add(this);
        }

        #region IDrawableSprite Members

        public Texture2D Texture{
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public void Draw(){
            RenderTarget.CurSpriteBatch.DrawString(
                _font,
                Str,
                Position,
                Color
                );
        }

        public void SetTextureFromString(string textureName){
            throw new System.NotImplementedException();
        }

        public void Dispose(){
            throw new System.NotImplementedException();
        }

        #endregion
    }
}