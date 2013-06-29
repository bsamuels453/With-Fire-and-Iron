#region

using System;
using System.Collections.Generic;
using System.Text;
using Forge.Framework.Control;
using Forge.Framework.Draw;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Point = MonoGameUtility.Point;

#endregion

namespace Forge.Framework.UI.Elements{
    /// <summary>
    /// Defines a textbox that can contain various kinds of text.
    /// </summary>
    internal class TextBox : IUIElement, IDrawableSprite{
        public readonly SpriteFont Font;
        readonly StringBuilder _builder;
        readonly Color _fontColor;
        readonly int _fontSize;
        readonly int _maxLines;
        readonly int _wrapWidth;
        public bool Enabled;
        bool _disposed;
        Vector2 _position;

        /// <summary>
        /// Constructor to use when there's a UIElementCollection currently bound.
        /// </summary>
        /// <param name="position">Position of the top left corner of the text box (origin).</param>
        /// <param name="targLevel"></param>
        /// <param name="fontColor"></param>
        /// <param name="font">Default font is monospace 10.</param>
        /// <param name="wrapWidth">How many pixels wide this textbox is allowed to get before wrapping to a new line. By default, doesnt wrap.</param>
        /// <param name="maxLines">Max number of lines of text that can be displayed. By default, there is no limit.</param>
        public TextBox(
            Point position,
            FrameStrata.Level targLevel,
            Color fontColor,
            string font = "Fonts/Monospace10",
            int wrapWidth = -1,
            int maxLines = -1
            )
            : this(position, UIElementCollection.BoundCollection, targLevel, fontColor, font, wrapWidth, maxLines){
        }

        /// <summary>
        /// Constructor to use when there is no bound UIElementCollection.
        /// </summary>
        /// <param name="position">Position of the top left corner of the text box (origin).</param>
        /// <param name="parent"></param>
        /// <param name="targLevel"></param>
        /// <param name="fontColor"></param>
        /// <param name="font">Default font is monospace 10.</param>
        /// <param name="wrapWidth">How many pixels wide this textbox is allowed to get before wrapping to a new line. By default, doesnt wrap.</param>
        /// <param name="maxLines">Max number of lines of text that can be displayed. By default, there is no limit.</param>
        public TextBox(
            Point position,
            UIElementCollection parent,
            FrameStrata.Level targLevel,
            Color fontColor,
            string font = "Fonts/Monospace10",
            int wrapWidth = -1,
            int maxLines = -1
            ){
            MouseController = new MouseController(this);
            Font = Resource.LoadContent<SpriteFont>(font);
            FrameStrata = new FrameStrata(targLevel, parent.FrameStrata, "TextBox");
            Enabled = true;
            _position = new Vector2(position.X, position.Y);
            _maxLines = maxLines;
            _wrapWidth = wrapWidth;
            _builder = new StringBuilder();
            _fontColor = fontColor;
            Alpha = 1;
            RenderTarget.Sprites.Add(this);
        }

        #region IDrawableSprite Members

        public void Draw(){
            if (Enabled){
                RenderTarget.CurSpriteBatch.DrawString
                    (
                        Font,
                        _builder,
                        _position,
                        new Color(_fontColor.R, _fontColor.G, _fontColor.B, Alpha),
                        0,
                        Vector2.Zero,
                        1,
                        SpriteEffects.None,
                        FrameStrata.FrameStrataValue
                    );
            }
        }

        public void Dispose(){
            if (!_disposed){
                _disposed = true;
                RenderTarget.Sprites.Remove(this);
            }
        }

        #endregion

        #region IUIElement Members

        public FrameStrata FrameStrata { get; private set; }

        public int X{
            get { return (int) _position.X; }
            set { _position.X = value; }
        }

        public int Y{
            get { return (int) _position.Y; }
            set { _position.Y = value; }
        }

        public int Width{
            get { return _wrapWidth == -1 ? 1 : _wrapWidth; }
        }

        //todo: fix this
        public int Height{
            get { return 1; }
        }

        public float Alpha { get; set; }

        public MouseController MouseController { get; private set; }

        public bool HitTest(int x, int y){
            return false;
        }

        public List<IUIElement> GetElementStackAtPoint(int x, int y){
            return new List<IUIElement>();
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

            Func<int, int, string> genPhrase =
                (s, e) =>{
                    string p = "";
                    for (int i = s; i < e; i++){
                        p += splitText[i] + ' ';
                    }
                    p = p.TrimEnd(' ');
                    return p;
                };

            while (end <= splitText.Length){
                string phrase = genPhrase(start, end);

                if (Font.MeasureString(phrase).X > _wrapWidth){
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