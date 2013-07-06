#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    public class TextBox : IUIElement, IDrawableSprite{
        #region Justification enum

        public enum Justification{
            Left,
            Center,
            Right
        }

        #endregion

        public readonly SpriteFont Font;
        public readonly float FontHeight;
        readonly StringBuilder _builder;
        readonly Color _fontColor;
        readonly Justification _justification;
        readonly int _maxLines;
        readonly int _textFieldHeight;
        readonly int _wrapWidth;

        public bool Enabled;
        bool _disposed;
        Vector2 _offset;
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
        /// <param name="justification"> </param>
        public TextBox(
            Point position,
            FrameStrata.Level targLevel,
            Color fontColor,
            string font = "Fonts/Monospace10",
            int wrapWidth = -1,
            int maxLines = -1,
            Justification justification = Justification.Left
            )
            : this(position, UIElementCollection.BoundCollection, targLevel, fontColor, font, wrapWidth, maxLines, justification){
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
        /// <param name="justification"> </param>
        public TextBox(
            Point position,
            UIElementCollection parent,
            FrameStrata.Level targLevel,
            Color fontColor,
            string font = "Fonts/Monospace10",
            int wrapWidth = -1,
            int maxLines = -1,
            Justification justification = Justification.Left
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
            _justification = justification;
            _offset = new Vector2();
            RenderTarget.AddSprite(this);

            FontHeight = Font.MeasureString(".").Y*0.65f; //apparently theres ridic font padding

            if (maxLines != -1){
                if (maxLines == 1){
                    _textFieldHeight = (int) FontHeight;
                }
                else{
                    _textFieldHeight = (int) (Font.MeasureString(".").Y*maxLines);
                }
            }
            else{
                //we're assuming that text drifting outside of the uielementcollection doesn't matter, so this is fine.
                _textFieldHeight = 1;
            }

            if (_wrapWidth != -1){
                Debug.Assert(_wrapWidth > Font.MeasureString(".").X);
            }
        }

        public int NumLines{
            get{
                var newlines = from c in _builder.ToString()
                    where c == '\n'
                    select c;
                return newlines.Count() + 1;
            }
        }

        #region IDrawableSprite Members

        public void Draw(){
            if (Enabled){
                RenderTarget.CurSpriteBatch.DrawString
                    (
                        Font,
                        _builder,
                        _position + _offset,
                        new Color(_fontColor.R, _fontColor.G, _fontColor.B, Alpha),
                        0,
                        Vector2.Zero,
                        1,
                        SpriteEffects.None,
                        FrameStrata.FrameStrataValue
                    );
            }
        }

        #endregion

        #region IUIElement Members

        public void Dispose(){
            if (!_disposed){
                _disposed = true;
                RenderTarget.RemoveSprite(this);
            }
        }

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
            get { return _wrapWidth == -1 ? (int) MeasureTextWidth() : _wrapWidth; }
        }

        public int Height{
            get { return _textFieldHeight; }
        }

        public float Alpha { get; set; }

        public MouseController MouseController { get; private set; }

        public bool HitTest(int x, int y){
            return false;
        }

        public List<IUIElement> GetElementStackAtPoint(int x, int y){
            return new List<IUIElement>();
        }

        public void Update(float timeDelta){
        }

        #endregion

        public void SetText(string text){
            var lines = new List<string>();
            var splitText = text.Split(' ');
            int start = 0, end = 0;
            _builder.Clear();

            if (splitText.Length == 1){
                _builder.AppendLine(splitText[0]);
                UpdateJustification();
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

                if (Font.MeasureString(phrase).X > _wrapWidth && _wrapWidth != -1){
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
                if (lineNum > _maxLines && _maxLines != -1)
                    break;
                _builder.AppendLine(line);
            }
            UpdateJustification();
        }

        void UpdateJustification(){
            switch (_justification){
                case Justification.Right:
                    throw new NotImplementedException("right justify");
                    break;
                case Justification.Center:
                    Debug.Assert(_wrapWidth != -1);
                    _offset.X = _wrapWidth/2f;
                    var width = MeasureTextWidth();
                    _offset.X -= width/2;
                    break;
                case Justification.Left:
                    //we cool
                    break;
            }
        }

        float MeasureTextWidth(){
            string line = "";
            for (int i = 0; i < _builder.Length; i++){
                if (_builder[i] == '\n'){
                    break;
                }
                line += _builder[i];
            }
            return Font.MeasureString(line).X;
        }

        public static Vector2 MeasureString(string fontAddr, string text){
            var font = Resource.LoadContent<SpriteFont>(fontAddr);
            return font.MeasureString(text);
        }
    }
}