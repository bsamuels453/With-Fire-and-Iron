#region

using System;
using System.Collections.Generic;
using System.Text;
using Forge.Framework.Draw;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = MonoGameUtility.Vector2;

#endregion

namespace Forge.Framework.UI.Widgets{
    public class TextBox : IDrawableSprite{
        public readonly SpriteFont Font;
        readonly float _absoluteDepth;
        readonly StringBuilder _builder;
        readonly Color _fontColor;
        readonly int _maxLines;
        readonly Vector2 _position;
        readonly int _width;
        public bool Enabled;

        public TextBox(int x, int y, DepthLevel depth, Color fontColor, int width = 999999, string font = "Fonts/Monospace10", int maxLines = 99999999){
            try{
                Font = Resource.LoadContent<SpriteFont>(font);
            }
            catch{
                //Font = Resource.ContentManager.Load<SpriteFont>(font);
            }
            _absoluteDepth = UIElementCollection.BoundCollection.GetAbsoluteDepth(depth);
            _position = new Vector2(x, y);
            _maxLines = maxLines;
            _width = width;
            _builder = new StringBuilder();
            _fontColor = fontColor;
            Enabled = true;
            RenderTarget.Sprites.Add(this);
        }

        #region IDrawableSprite Members

        public Texture2D Texture{
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public void Draw(){
            if (Enabled){
                RenderTarget.CurSpriteBatch.DrawString
                    (
                        Font,
                        _builder,
                        _position,
                        _fontColor,
                        0,
                        Vector2.Zero,
                        1,
                        SpriteEffects.None,
                        _absoluteDepth
                    );
            }
        }

        public void SetTextureFromString(string textureName){
            throw new NotImplementedException();
        }

        public void Dispose(){
            throw new NotImplementedException();
        }

        #endregion

        public void SetText(string text){
            var lines = new List<string>();
            var splitText = text.Split(' ');
            int start = 0, end = 0;
            _builder.Clear();

            if (splitText.Length == 1){
                _builder.AppendLine(splitText[0]);
                return;
            }

            Func<int, int, string> genPhrase = (s, e) =>{
                                                   string p = "";
                                                   for (int i = s; i < e; i++){
                                                       p += splitText[i] + ' ';
                                                   }
                                                   p = p.TrimEnd(' ');
                                                   return p;
                                               };

            while (end <= splitText.Length){
                string phrase = genPhrase(start, end);

                if (Font.MeasureString(phrase).X > _width){
                    phrase = genPhrase(start, end - 1);
                    lines.Add(phrase);
                    start = end - 1;
                }
                else{
                    end++;
                    if (end > splitText.Length){
                        lines.Add(phrase);
                    }
                }
            }

            int lineNum = 0;
            foreach (var line in lines){
                lineNum++;
                if (lineNum > _maxLines)
                    break;
                _builder.AppendLine(line);
            }
        }
    }
}