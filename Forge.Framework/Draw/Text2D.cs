#region

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Forge.Framework.Draw{
    public class Text2D : IDrawableSprite{
        readonly SpriteFont _font;
        public Color Color;
        public Vector2 Position;
        public string Str;

        public Text2D(int x, int y, string str, string font = "Fonts/SpriteFont"){
            Position = new Vector2(x, y);
            Str = str;
            Color = Color.Black;
            try{
                _font = Gbl.ContentManager.Load<SpriteFont>(Gbl.RawLookup[font]);
            }
            catch{
                _font = Gbl.ContentManager.Load<SpriteFont>(font);
            }
            RenderTarget.Sprites.Add(this);
        }

        #region IDrawableSprite Members

        public Texture2D Texture{
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
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
            throw new NotImplementedException();
        }

        public void Dispose(){
            throw new NotImplementedException();
        }

        #endregion
    }
}