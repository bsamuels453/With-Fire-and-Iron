using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forge.Framework.Draw;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Forge.Framework.UI.Widgets {
    public class TextBox : IDrawableSprite {
        public readonly SpriteFont Font;
        public bool Enabled;
        readonly int _maxLines;
        readonly int _width;
        readonly Vector2 _position;
        readonly float _absoluteDepth;
        readonly Color _fontColor;
        readonly StringBuilder _builder;

        public TextBox(int x, int y, DepthLevel depth, Color fontColor, int width=999999, string font = "Fonts/Monospace10", int maxLines = 99999999) {
            try {
                Font = Gbl.ContentManager.Load<SpriteFont>(Gbl.RawLookup[font]);
            }
            catch {
                Font = Gbl.ContentManager.Load<SpriteFont>(font);
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

        public void SetText(string text){
            var lines = new List<string>();
            var splitText = text.Split(' ');
            int start = 0, end = 0;
            _builder.Clear();

            if (splitText.Length == 1){
                _builder.AppendLine(splitText[0]);
                return;
            }

            Func<int, int, string> genPhrase = (s,e) => {
                string p = "";
                for (int i = s; i < e; i++) {
                    p += splitText[i] + ' ';
                }
                p = p.TrimEnd(' ');
                return p;
            };

            while (end <= splitText.Length) {
                string phrase = genPhrase(start,end);

                if (Font.MeasureString(phrase).X > _width) {
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

        public Texture2D Texture{
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public void Draw(){
            if (Enabled){
                RenderTarget.CurSpriteBatch.DrawString(
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
    }
}
