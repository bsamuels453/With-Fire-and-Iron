#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Forge.Framework.Control;
using Forge.Framework.Draw;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Point = MonoGameUtility.Point;
using Rectangle = MonoGameUtility.Rectangle;

#endregion

namespace Forge.Framework.UI.Elements{
    public class InputBox : UIElementCollection{
        const int _borderThickness = 2;
        const int _cornerSize = 2;
        const string _borderMaterial = "Materials/TextBoxBorder";
        const string _bgMaterial = "Materials/TextBoxBG";
        const string _cornerMaterial = "Materials/TextBoxCorner";
        const string _cursorMaterial = "Materials/SolidBlack";
        const string _textboxFont = "Fonts/Monospace10";
        const int _backgroundInset = 1;
        const int _horizontalTextPadding = 0;
        const int _verticalTextPadding = 0;
        const int _textFontSize = 40;

        readonly Stopwatch _blinkTimer;

        readonly TextBox _textBox;
        readonly Color _textColor = Color.Black;
        bool _boxFocused;
        Sprite2D _cursor;
        int _cursorPosition;

        /// <param name="parent"></param>
        /// <param name="depth"></param>
        /// <param name="position">The position of the top left corner of the input box.</param>
        /// <param name="boxWidth">The full width of the text box, corner to corner.</param>
        /// <param name="defaultText"></param>
        public InputBox(
            UIElementCollection parent,
            FrameStrata.Level depth,
            Point position,
            int boxWidth,
            string defaultText = ""
            )
            : base(parent, depth, new Rectangle(position.X, position.Y, boxWidth, _textFontSize + 2*_borderThickness), "InputBox"){
            _textBox = new TextBox
                (
                new Point(position.X + _borderThickness + _horizontalTextPadding, position.Y + _borderThickness + _verticalTextPadding),
                this,
                FrameStrata.Level.High,
                Color.Black,
                _textboxFont,
                boxWidth - _borderThickness*2,
                1
                );

            #region create sprites

            //reflect border material
            var borderTexture = Resource.LoadContent<Texture2D>(_borderMaterial);
            RenderTarget2D reflectedBorder = new RenderTarget2D(Resource.Device, borderTexture.Height, borderTexture.Width);
                {
                    var sb = new SpriteBatch(Resource.Device);
                    Resource.Device.SetRenderTarget(reflectedBorder);
                    Resource.Device.Clear(Color.Transparent);
                    sb.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

                    sb.Draw
                        (
                            borderTexture,
                            new Vector2(borderTexture.Width/4f, borderTexture.Height/2f),
                            new Rectangle(0, 0, borderTexture.Width, borderTexture.Height),
                            Color.White,
                            MathHelper.PiOver2,
                            new Vector2(borderTexture.Width/4f, borderTexture.Height/2f),
                            1,
                            SpriteEffects.None,
                            0
                        );
                    sb.End();
                    Resource.Device.SetRenderTarget(null);
                }

            //now construct background sprite
            int boxHeight = _textFontSize + _cornerSize*2;

            var bgBatch = new SpriteBatch(Resource.Device);
            var bgText = new RenderTarget2D(Resource.Device, boxWidth, boxHeight);
            Resource.Device.SetRenderTarget(bgText);
            Resource.Device.Clear(Color.Transparent);
            bgBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            bgBatch.Draw
                (
                    Resource.LoadContent<Texture2D>(_bgMaterial),
                    new Rectangle
                        (
                        _backgroundInset,
                        _backgroundInset,
                        boxWidth - _backgroundInset,
                        boxHeight - _backgroundInset
                        ),
                    Color.White
                );

            //top border
            bgBatch.Draw
                (
                    reflectedBorder,
                    new Rectangle
                        (
                        _cornerSize,
                        0,
                        boxWidth - _cornerSize*2,
                        _borderThickness
                        ),
                    Color.White
                );

            //left  border
            bgBatch.Draw
                (
                    Resource.LoadContent<Texture2D>(_borderMaterial),
                    new Rectangle
                        (
                        0,
                        _cornerSize,
                        _borderThickness,
                        _textFontSize
                        ),
                    Color.White
                );

            //right border
            bgBatch.Draw
                (
                    Resource.LoadContent<Texture2D>(_borderMaterial),
                    new Rectangle
                        (
                        boxWidth - _borderThickness,
                        _cornerSize,
                        _borderThickness,
                        _textFontSize
                        ),
                    null,
                    Color.White,
                    0,
                    Vector2.Zero,
                    SpriteEffects.FlipHorizontally,
                    1
                );

            //bottom border
            bgBatch.Draw
                (
                    reflectedBorder,
                    new Rectangle
                        (
                        _cornerSize,
                        _borderThickness + _textFontSize,
                        boxWidth - _cornerSize*2,
                        _borderThickness
                        ),
                    null,
                    Color.White,
                    0,
                    Vector2.Zero,
                    SpriteEffects.FlipVertically,
                    1
                );


            //topleft corner
            bgBatch.Draw
                (
                    Resource.LoadContent<Texture2D>(_cornerMaterial),
                    new Rectangle
                        (
                        0,
                        0,
                        _cornerSize,
                        _cornerSize
                        ),
                    Color.White
                );

            //topright corner
            bgBatch.Draw
                (
                    Resource.LoadContent<Texture2D>(_cornerMaterial),
                    new Rectangle
                        (
                        boxWidth - _cornerSize,
                        0,
                        _cornerSize,
                        _cornerSize
                        ),
                    null,
                    Color.White,
                    0,
                    Vector2.Zero,
                    SpriteEffects.FlipVertically,
                    1
                );

            //bottomleft corner
            bgBatch.Draw
                (
                    Resource.LoadContent<Texture2D>(_cornerMaterial),
                    new Rectangle
                        (
                        0,
                        _textFontSize + _borderThickness*2 - _cornerSize,
                        _cornerSize,
                        _cornerSize
                        ),
                    null,
                    Color.White,
                    0,
                    Vector2.Zero,
                    SpriteEffects.FlipHorizontally,
                    1
                );

            //bottomright corner
            bgBatch.Draw
                (
                    Resource.LoadContent<Texture2D>(_cornerMaterial),
                    new Rectangle
                        (
                        boxWidth - _cornerSize,
                        (_textFontSize + _borderThickness*2 - _cornerSize),
                        _cornerSize,
                        _cornerSize
                        ),
                    null,
                    Color.White,
                    0,
                    Vector2.Zero,
                    SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically,
                    1
                );

            bgBatch.End();
            Resource.Device.SetRenderTarget(null);

            #endregion

            var sprt = new Sprite2D
                (
                bgText,
                position.X,
                position.Y,
                boxWidth,
                boxHeight,
                this.FrameStrata,
                FrameStrata.Level.Low
                );


            AddElement(_textBox);
            this.OnLeftDown += OnMouseClick;
        }


        int XAnchor{
            get { return this.X + 0; }
        }

        public string Text { get; private set; }

        void OnMouseClick(ForgeMouseState state, float timeDelta, UIElementCollection caller){
            if (caller.ContainsMouse){
                if (!_boxFocused){
                    ActivateTextbox();
                }
            }
            else{
                if (_boxFocused){
                    DeactivateTextbox();
                }
            }
        }

        void ActivateTextbox(){
            _cursor.Enabled = true;
            _boxFocused = true;
        }

        void DeactivateTextbox(){
            _cursor.Enabled = false;
            _boxFocused = false;
        }


        /*
        void OnKeyPress(object caller, int bindAlias, ForgeKeyState keyState){
            #region character addition

            char c;
            if (!ParseAlphabet(state.KeyboardState, state.PrevState.KeyboardState, out c)){
                ParseNumeric(state.KeyboardState, state.PrevState.KeyboardState, out c);
            }
            if (c != '`'){
                string tempStr = Text.Insert(_cursorPosition, Char.ToString(c));

                if (_textBox.Font.MeasureString(tempStr).X < _width){
                    Text = tempStr;
                    _textBox.SetText(Text);
                    _cursorPosition++;
                    float diff = _textBox.Font.MeasureString(Char.ToString(c)).X;
                    _cursor.X += diff;
                }
            }

            #endregion

            #region key navigation

            if (_cursorPosition > 0){
                if (state.KeyboardState.IsKeyDown(Keys.Left) && !state.PrevState.KeyboardState.IsKeyDown(Keys.Left)){
                    float diff = _textBox.Font.MeasureString(Char.ToString(Text[_cursorPosition - 1])).X;
                    _cursorPosition--;
                    _cursor.X -= diff;
                }
            }
            if (_cursorPosition < Text.Length){
                if (state.KeyboardState.IsKeyDown(Keys.Right) && !state.PrevState.KeyboardState.IsKeyDown(Keys.Right)){
                    float diff = _textBox.Font.MeasureString(Char.ToString(Text[_cursorPosition])).X;
                    _cursorPosition++;
                    _cursor.X += diff;
                }
            }

            #endregion

            #region backspace

            if (_cursorPosition > 0){
                if (state.KeyboardState.IsKeyDown(Keys.Back) && !state.PrevState.KeyboardState.IsKeyDown(Keys.Back)){
                    try{
                        Text = Text.Substring(0, _cursorPosition - 1) + Text.Substring(_cursorPosition, Text.Length - _cursorPosition);
                    }
                    catch{
                        Text = Text.Substring(0, _cursorPosition - 1);
                    }
                    _cursorPosition--;
                    _cursor.X = (int) GetCursorPos(Text, _cursorPosition);
                    _textBox.SetText(Text);
                }
            }

            #endregion

            #region entry-finalization

            if (state.KeyboardState.IsKeyDown(Keys.Enter) && !state.PrevState.KeyboardState.IsKeyDown(Keys.Enter)){
                _cursor.Enabled = false;
                _boxFocused = false;
                _left.Alpha = 0.70f;
                _right.Alpha = 0.70f;
                _center.Alpha = 0.70f;
                if (OnTextEntryFinalize != null){
                    OnTextEntryFinalize(Text);
                }
            }

            #endregion
        }
         */

        protected override void UpdateChild(float timeDelta){
            if (_boxFocused){
                if (_blinkTimer.ElapsedMilliseconds > 400){
                    _cursor.Enabled = !_cursor.Enabled;
                    _blinkTimer.Restart();
                }
            }
        }

        void OnClick(){
            _boxFocused = true;
            _cursor.Enabled = true;
            _blinkTimer.Start();
            var state = Mouse.GetState();
            int diff = state.X - XAnchor;

            var wordDists = new Dictionary<int, float>();
            string str = "";

            wordDists.Add(0, 0);
            int chrIdx = 1;
            foreach (var word in Text){
                str += word;
                wordDists.Add(chrIdx, _textBox.Font.MeasureString(str).X);
                chrIdx++;
            }
            var ordered = wordDists.OrderBy(w => Math.Abs(diff - w.Value));

            _cursorPosition = ordered.First().Key;
            _cursor.X = XAnchor + (int) ordered.First().Value;
            //_left.Alpha = 1;
            //_right.Alpha = 1;
            //_center.Alpha = 1;
        }

        float GetCursorPos(string str, int pos){
            string sub = new string(str.Take(pos).ToArray());
            return XAnchor + _textBox.Font.MeasureString(sub).X;
        }


        public event Action<string> OnTextEntryFinalize;

        //We don't need to check every child sprite to know that any mouse action within
        //the elementcollection's bounding box is a hit.
        public override bool HitTest(int x, int y){
            return this.BoundingBox.Contains(x, y);
        }
    }
}