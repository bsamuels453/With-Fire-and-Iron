#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Forge.Framework.Control;
using Forge.Framework.Draw;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;
using Point = MonoGameUtility.Point;
using Rectangle = MonoGameUtility.Rectangle;

#endregion

namespace Forge.Framework.UI.Elements{
    /// <summary>
    /// A single-line text input box.
    /// </summary>
    public class InputBox : UIElementCollection{
        const string _cursorMaterial = "Materials/WhitePixel";
        readonly int _backgroundInset = 1;
        readonly string _bgMaterial = "Materials/TextBoxBG";
        readonly Stopwatch _blinkTimer;
        readonly string _borderMaterial = "Materials/TextBoxBorder";
        readonly int _borderThickness = 2;
        readonly KeyboardController _controller;
        readonly string _cornerMaterial = "Materials/TextBoxCorner";
        readonly int _cornerSize = 2;
        readonly Sprite2D _cursor;
        readonly Color _cursorColor = Color.LimeGreen;
        readonly int _horizontalTextPadding = 2;
        readonly TextBox _textBox;
        readonly Color _textColor = Color.White;
        readonly int _textFontSize = 10;
        readonly string _textboxFont = "Fonts/Monospace10";
        readonly int _verticalTextPadding = 2;

        bool _boxFocused;
        int _cursorPosition;

        /// <param name="parent"></param>
        /// <param name="depth"></param>
        /// <param name="position">The position of the top left corner of the input box.</param>
        /// <param name="boxWidth">The full width of the text box, corner to corner.</param>
        /// <param name="defaultText"></param>
        /// <param name="template">The template to use to define padding/textures/fonts.</param>
        public InputBox(
            UIElementCollection parent,
            FrameStrata.Level depth,
            Point position,
            int boxWidth,
            string defaultText = "DefaultText",
            string template = "UiTemplates/InputBox.json"
            )
            : base(parent, depth, new Rectangle(position.X, position.Y, boxWidth, 0), "InputBox"){
            #region load template

            var strmrdr = new StreamReader(template);
            var contents = strmrdr.ReadToEnd();
            strmrdr.Close();

            var jobj = JObject.Parse(contents);

            _borderMaterial = jobj["BorderMaterial"].ToObject<string>();
            _bgMaterial = jobj["BackgroundMaterial"].ToObject<string>();
            _cornerMaterial = jobj["CornerMaterial"].ToObject<string>();
            //_cursorColor = (Color)jobj["CursorColor"].ToObject<string>();
            //_textColor = jobj["FontColor"].ToObject<Color>();
            _textboxFont = jobj["Font"].ToObject<string>();
            _backgroundInset = jobj["BackgroundInset"].ToObject<int>();
            _horizontalTextPadding = jobj["HorizontalTextPadding"].ToObject<int>();
            _verticalTextPadding = jobj["VerticalTextPadding"].ToObject<int>();
            _textFontSize = jobj["FontSize"].ToObject<int>();
            _borderThickness = jobj["BorderThickness"].ToObject<int>();
            _cornerSize = jobj["CornerSize"].ToObject<int>();

            #endregion

            #region set up sprites/spritedata

            int boxHeight = _textFontSize + _cornerSize*2 + _verticalTextPadding*2;

            var bg = new Sprite2D
                (
                GenerateBgSprite(boxWidth, boxHeight),
                position.X,
                position.Y,
                boxWidth,
                boxHeight,
                this.FrameStrata,
                FrameStrata.Level.Background
                );

            _cursor = new Sprite2D
                (
                _cursorMaterial,
                0,
                position.Y + _borderThickness + _verticalTextPadding,
                1,
                _textFontSize,
                this.FrameStrata,
                FrameStrata.Level.High
                );
            _cursor.ShadeColor = _cursorColor;
            _cursor.Enabled = false;

            _textBox = new TextBox
                (
                new Point(position.X + _horizontalTextPadding, position.Y + _verticalTextPadding),
                this,
                FrameStrata.Level.Medium,
                _textColor,
                _textboxFont,
                boxWidth - _borderThickness*2,
                1
                );

            _textBox.SetText(defaultText);
            Text = defaultText;

            AddElement(bg);
            AddElement(_textBox);
            AddElement(_cursor);

            _blinkTimer = new Stopwatch();
            _cursorPosition = 0;

            #endregion

            #region setup keyboard controller

            _controller = new KeyboardController();

            for (int i = 37; i <= 39; i++){
                _controller.CreateNewBind((Keys) i, (Keys) i, OnArrowKeyPress, BindCondition.OnKeyDown);
            }
            for (int i = 48; i <= 57; i++){
                _controller.CreateNewBind((Keys) i, (Keys) i, OnAlphaNumericPress, BindCondition.OnKeyDown);
            }
            for (int i = 65; i <= 90; i++){
                _controller.CreateNewBind((Keys) i, (Keys) i, OnAlphaNumericPress, BindCondition.OnKeyDown);
            }
            for (int i = 106; i <= 111; i++){
                _controller.CreateNewBind((Keys) i, (Keys) i, OnAlphaNumericPress, BindCondition.OnKeyDown);
            }

            _controller.CreateNewBind(Keys.Back, Keys.Back, OnBackspacePress, BindCondition.OnKeyDown);
            _controller.CreateNewBind(Keys.Enter, Keys.Enter, OnEnterPress, BindCondition.OnKeyDown);

            #endregion

            this.OnLeftDown += OnMouseClick;
        }

        public string Text { get; private set; }

        void LoadFromTemplate(string templateDir){
            int g = 5;
        }

        /// <summary>
        /// Generates the background of the inputbox. optimize: cache the sprites generated by this method in a static dict
        /// </summary>
        /// <param name="boxWidth"></param>
        /// <param name="boxHeight"></param>
        /// <returns></returns>
        Texture2D GenerateBgSprite(int boxWidth, int boxHeight){
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
            var bgBatch = new SpriteBatch(Resource.Device);
            var bgTexture = new RenderTarget2D(Resource.Device, boxWidth, boxHeight);
            Resource.Device.SetRenderTarget(bgTexture);
            Resource.Device.Clear(Color.Transparent);
            bgBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            bgBatch.Draw
                (
                    Resource.LoadContent<Texture2D>(_bgMaterial),
                    new Rectangle
                        (
                        _backgroundInset,
                        _backgroundInset,
                        boxWidth - _backgroundInset*2,
                        boxHeight - _backgroundInset*2
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
                        _textFontSize + _verticalTextPadding*2
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
                        _textFontSize + _verticalTextPadding*2
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
                        _borderThickness + _textFontSize + _verticalTextPadding*2,
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
                        _textFontSize + _borderThickness*2 - _cornerSize + _verticalTextPadding*2,
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
                        (_textFontSize + _borderThickness*2 + _verticalTextPadding*2 - _cornerSize),
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
            return bgTexture;
        }

        /// <summary>
        /// Called on global mouse click.
        /// </summary>
        void OnMouseClick(ForgeMouseState state, float timeDelta, UIElementCollection caller){
            if (caller.ContainsMouse && !state.BlockLeftMButton){
                KeyboardManager.SetActiveController(_controller);
                _boxFocused = true;
                _cursor.Enabled = true;
                _blinkTimer.Start();
                int diff = state.X - _textBox.X;

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
                _cursor.X = _textBox.X + (int) ordered.First().Value;
            }
            else{
                if (_boxFocused){
                    DefocusTextbox();
                }
            }
        }

        /// <summary>
        /// Defocuses the input box by disabling cursor sprite, turning off blink timer, releasing keyboard controller,
        /// and invoking OnTextEntryFinalize event. 
        /// </summary>
        void DefocusTextbox(){
            _cursor.Enabled = false;
            _boxFocused = false;
            _blinkTimer.Reset();
            KeyboardManager.ReleaseActiveController(_controller);
            if (OnTextEntryFinalize != null){
                OnTextEntryFinalize(Text);
            }
        }

        /// <summary>
        /// Converts a key from the xna Keys enum to its cooresponding unicode character.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="shiftModifier"> </param>
        /// <returns></returns>
        char ConvertKeyToChar(Keys key, bool shiftModifier){
            //48-57 inclusive is numkeys 0-9
            //65-90 inclusive is alphabet in order
            //96-105 is numpad 0-9
            //106 mult
            //107 add
            //108 separator
            //109 subtract
            //110 declimal
            //111 divide
            int keyCode = (int) key;

            char c = Convert.ToChar(keyCode);
            if (!shiftModifier){
                c = char.ToLower(c);
            }
            return c;
        }

        /// <summary>
        /// Called when inputbox's keyboard controller is active and an arrow key is pressed.
        /// </summary>
        void OnArrowKeyPress(object caller, int bindAlias, ForgeKeyState keyState){
            if (_cursorPosition > 0){
                if ((Keys) bindAlias == Keys.Left){
                    float diff = _textBox.Font.MeasureString(Char.ToString(Text[_cursorPosition - 1])).X;
                    _cursorPosition--;
                    _cursor.X -= (int) diff;
                }
            }
            if (_cursorPosition < Text.Length){
                if ((Keys) bindAlias == Keys.Right){
                    float diff = _textBox.Font.MeasureString(Char.ToString(Text[_cursorPosition])).X;
                    _cursorPosition++;
                    _cursor.X += (int) diff;
                }
            }
        }

        /// <summary>
        /// Called when inputbox's keyboard controller is active and the backspace key is pressed.
        /// </summary>
        void OnBackspacePress(object caller, int bindAlias, ForgeKeyState keyState){
            if (_cursorPosition > 0){
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

        /// <summary>
        /// Called when inputbox's keyboard controller is active and the enter key is pressed.
        /// </summary>
        void OnEnterPress(object caller, int bindAlias, ForgeKeyState keyState){
            DefocusTextbox();
        }

        /// <summary>
        /// Called when inputbox's keyboard controller is active and an alphanumeric key is pressed.
        /// </summary>
        void OnAlphaNumericPress(object caller, int bindAlias, ForgeKeyState keyState){
            bool shiftDown = Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift);
            char c = ConvertKeyToChar((Keys) bindAlias, shiftDown);
            string tempStr = Text.Insert(_cursorPosition, Char.ToString(c));

            if (_textBox.Font.MeasureString(tempStr).X < _textBox.Width){
                Text = tempStr;
                _textBox.SetText(Text);
                _cursorPosition++;
                float diff = _textBox.Font.MeasureString(Char.ToString(c)).X;
                _cursor.X += (int) diff;
            }
        }

        protected override void UpdateChild(float timeDelta){
            if (_boxFocused){
                if (_blinkTimer.ElapsedMilliseconds > 400){
                    _cursor.Enabled = !_cursor.Enabled;
                    _blinkTimer.Restart();
                }
            }
        }

        float GetCursorPos(string str, int pos){
            string sub = new string(str.Take(pos).ToArray());
            return _textBox.X + _textBox.Font.MeasureString(sub).X;
        }

        public event Action<string> OnTextEntryFinalize;

        //We don't need to check every child sprite to know that any mouse action within
        //the elementcollection's bounding box is a hit.
        public override bool HitTest(int x, int y){
            return this.BoundingBox.Contains(x, y);
        }
    }
}