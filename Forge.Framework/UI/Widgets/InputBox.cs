#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#endregion

namespace Forge.Framework.UI.Widgets{
    public class InputBox : IUIElementBase{
        readonly Stopwatch _blinkTimer;
        readonly Button _center;
        readonly Button _cursor;
        readonly Button _left;
        readonly Button _right;
        readonly TextBox _textBox;
        readonly int _width;
        readonly int _xAnchor;
        bool _boxFocused;
        int _cursorPosition;

        public InputBox(int x, int y, int width, string defaultText = ""){
            Text = defaultText;
            _boxFocused = false;

            _left = new Button(x, y, 2, 17, DepthLevel.Medium, "Materials/TextBoxLeft");
            _center = new Button(x + 2, y, width, 17, DepthLevel.Medium, "Materials/TextBoxMid");
            _right = new Button(x + 2 + width, y, 2, 17, DepthLevel.Medium, "Materials/TextBoxRight");

            _cursor = new Button(x + 5, y + 2, 2, 13, DepthLevel.High, "Materials/SolidBlack");
            _cursor.Enabled = false;

            _textBox = new TextBox(x + 2, y + 2, DepthLevel.High, Color.Black, width);
            _textBox.SetText(Text);

            _left.OnLeftClickDispatcher += identifier => OnClick();
            _center.OnLeftClickDispatcher += identifier => OnClick();
            _right.OnLeftClickDispatcher += identifier => OnClick();
            _blinkTimer = new Stopwatch();
            _cursorPosition = 0;
            _xAnchor = x + 2;
            _left.Alpha = 0.70f;
            _right.Alpha = 0.70f;
            _center.Alpha = 0.70f;
            _width = width;
            UIElementCollection.BoundCollection.AddElement(this);
        }

        public string Text { get; private set; }

        #region IUIElementBase Members

        public void UpdateLogic(double timeDelta){
            if (_boxFocused){
                if (_blinkTimer.ElapsedMilliseconds > 400){
                    _cursor.Enabled = !_cursor.Enabled;
                    _blinkTimer.Restart();
                }
            }
        }

        public void UpdateInput(ref InputState state){
            #region containment check

            bool containsMouse = false;
            if (state.LeftButtonChange && state.LeftButtonState == ButtonState.Pressed){
                if (_center.HitTest(state.MousePos.X, state.MousePos.Y)){
                    containsMouse = true;
                }
                if (_left.HitTest(state.MousePos.X, state.MousePos.Y)){
                    containsMouse = true;
                }
                if (_right.HitTest(state.MousePos.X, state.MousePos.Y)){
                    containsMouse = true;
                }

                if (!containsMouse){
                    _cursor.Enabled = false;
                    _boxFocused = false;
                    _left.Alpha = 0.70f;
                    _right.Alpha = 0.70f;
                    _center.Alpha = 0.70f;
                }
            }

            #endregion

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

        public float X{
            get { return _left.X; }
            set { throw new NotImplementedException(); }
        }

        public float Y{
            get { return _left.Y; }
            set { throw new NotImplementedException(); }
        }

        public float Width{
            get { return _left.Width + _center.Width + _right.Width; }
            set { throw new NotImplementedException(); }
        }

        public float Height{
            get { return _center.Height; }
            set { throw new NotImplementedException(); }
        }

        public float Alpha{
            get { return 1; }
            set { throw new NotImplementedException(); }
        }

        public float Depth{
            get { return _center.Depth; }
            set { throw new NotImplementedException(); }
        }

        public bool HitTest(int x, int y){
            return false;
        }

        public List<IUIElementBase> GetElementStack(int x, int y){
            return new List<IUIElementBase>();
        }

        #endregion

        public event Action<string> OnTextEntryFinalize;

        void OnClick(){
            _boxFocused = true;
            _cursor.Enabled = true;
            _blinkTimer.Start();
            var state = Mouse.GetState();
            int diff = state.X - _xAnchor;

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
            _cursor.X = _xAnchor + ordered.First().Value;
            _left.Alpha = 1;
            _right.Alpha = 1;
            _center.Alpha = 1;
        }

        float GetCursorPos(string str, int pos){
            string sub = new string(str.Take(pos).ToArray());
            return _xAnchor + _textBox.Font.MeasureString(sub).X;
        }

        bool ParseNumeric(KeyboardState state, KeyboardState prevState, out char result){
            result = '`'; //default
            if (state.IsKeyDown(Keys.D0) && !prevState.IsKeyDown(Keys.D0)){
                result = '0';
            }
            if (state.IsKeyDown(Keys.D1) && !prevState.IsKeyDown(Keys.D1)){
                result = '1';
            }
            if (state.IsKeyDown(Keys.D2) && !prevState.IsKeyDown(Keys.D2)){
                result = '2';
            }
            if (state.IsKeyDown(Keys.D3) && !prevState.IsKeyDown(Keys.D3)){
                result = '3';
            }
            if (state.IsKeyDown(Keys.D4) && !prevState.IsKeyDown(Keys.D4)){
                result = '4';
            }
            if (state.IsKeyDown(Keys.D5) && !prevState.IsKeyDown(Keys.D5)){
                result = '5';
            }
            if (state.IsKeyDown(Keys.D6) && !prevState.IsKeyDown(Keys.D6)){
                result = '6';
            }
            if (state.IsKeyDown(Keys.D7) && !prevState.IsKeyDown(Keys.D7)){
                result = '7';
            }
            if (state.IsKeyDown(Keys.D8) && !prevState.IsKeyDown(Keys.D8)){
                result = '8';
            }
            if (state.IsKeyDown(Keys.D9) && !prevState.IsKeyDown(Keys.D9)){
                result = '9';
            }
            if (result == '`')
                return false;
            return true;
        }

        bool ParseAlphabet(KeyboardState state, KeyboardState prevState, out char result){
            result = '`'; //default

            if (state.IsKeyDown(Keys.A) && !prevState.IsKeyDown(Keys.A)){
                result = 'a';
            }
            if (state.IsKeyDown(Keys.B) && !prevState.IsKeyDown(Keys.B)){
                result = 'b';
            }
            if (state.IsKeyDown(Keys.C) && !prevState.IsKeyDown(Keys.C)){
                result = 'c';
            }
            if (state.IsKeyDown(Keys.D) && !prevState.IsKeyDown(Keys.D)){
                result = 'd';
            }
            if (state.IsKeyDown(Keys.E) && !prevState.IsKeyDown(Keys.E)){
                result = 'e';
            }
            if (state.IsKeyDown(Keys.F) && !prevState.IsKeyDown(Keys.F)){
                result = 'f';
            }
            if (state.IsKeyDown(Keys.G) && !prevState.IsKeyDown(Keys.G)){
                result = 'g';
            }
            if (state.IsKeyDown(Keys.H) && !prevState.IsKeyDown(Keys.H)){
                result = 'h';
            }
            if (state.IsKeyDown(Keys.I) && !prevState.IsKeyDown(Keys.I)){
                result = 'i';
            }
            if (state.IsKeyDown(Keys.J) && !prevState.IsKeyDown(Keys.J)){
                result = 'j';
            }
            if (state.IsKeyDown(Keys.K) && !prevState.IsKeyDown(Keys.K)){
                result = 'k';
            }
            if (state.IsKeyDown(Keys.L) && !prevState.IsKeyDown(Keys.L)){
                result = 'l';
            }
            if (state.IsKeyDown(Keys.M) && !prevState.IsKeyDown(Keys.M)){
                result = 'm';
            }
            if (state.IsKeyDown(Keys.N) && !prevState.IsKeyDown(Keys.N)){
                result = 'n';
            }
            if (state.IsKeyDown(Keys.O) && !prevState.IsKeyDown(Keys.O)){
                result = 'o';
            }
            if (state.IsKeyDown(Keys.P) && !prevState.IsKeyDown(Keys.P)){
                result = 'p';
            }
            if (state.IsKeyDown(Keys.Q) && !prevState.IsKeyDown(Keys.Q)){
                result = 'q';
            }
            if (state.IsKeyDown(Keys.R) && !prevState.IsKeyDown(Keys.R)){
                result = 'r';
            }
            if (state.IsKeyDown(Keys.S) && !prevState.IsKeyDown(Keys.S)){
                result = 's';
            }
            if (state.IsKeyDown(Keys.T) && !prevState.IsKeyDown(Keys.T)){
                result = 't';
            }
            if (state.IsKeyDown(Keys.U) && !prevState.IsKeyDown(Keys.U)){
                result = 'u';
            }
            if (state.IsKeyDown(Keys.V) && !prevState.IsKeyDown(Keys.V)){
                result = 'v';
            }
            if (state.IsKeyDown(Keys.W) && !prevState.IsKeyDown(Keys.W)){
                result = 'w';
            }
            if (state.IsKeyDown(Keys.X) && !prevState.IsKeyDown(Keys.X)){
                result = 'x';
            }
            if (state.IsKeyDown(Keys.Y) && !prevState.IsKeyDown(Keys.Y)){
                result = 'y';
            }
            if (state.IsKeyDown(Keys.Z) && !prevState.IsKeyDown(Keys.Z)){
                result = 'z';
            }
            if (state.IsKeyDown(Keys.Space) && !prevState.IsKeyDown(Keys.Space)){
                result = ' ';
            }
            if (result == '`')
                return false;

            if (state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift))
                result = Char.ToUpper(result);

            return true;
        }
    }
}