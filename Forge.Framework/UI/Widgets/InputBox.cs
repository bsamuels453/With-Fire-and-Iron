using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Forge.Framework.UI.Widgets {
    public class InputBox : IUIElementBase{
        readonly Button _left;
        readonly Button _right;
        readonly Button _center;
        readonly Button _cursor;
        readonly TextBox _textBox;
        string _text;
        bool _boxFocused;
        Stopwatch _blinkTimer;
        int _cursorPosition;
        int _xAnchor;
        
        
        public InputBox(int x, int y, int width, string defaultText = ""){
            _text = defaultText;
            _boxFocused = false;

            _left = new Button(x, y, 2, 17, DepthLevel.Medium, "Materials/TextBoxLeft");
            _center = new Button(x + 2, y, width, 17, DepthLevel.Medium, "Materials/TextBoxMid");
            _right = new Button(x + 2 + width, y, 2, 17, DepthLevel.Medium, "Materials/TextBoxRight");

            _cursor = new Button(x + 5, y + 2, 2, 13, DepthLevel.High, "Materials/SolidBlack");
            _cursor.Enabled = false;

            _textBox = new TextBox(x + 2, y + 2, DepthLevel.High, Color.Black, width);
            _textBox.SetText(_text);

            _left.OnLeftClickDispatcher += identifier => OnClick();
            _center.OnLeftClickDispatcher += identifier => OnClick();
            _right.OnLeftClickDispatcher += identifier => OnClick();
            _blinkTimer = new Stopwatch();
            _cursorPosition = 0;
            _xAnchor = x + 2;
            _left.Alpha = 0.70f;
            _right.Alpha = 0.70f;
            _center.Alpha = 0.70f;
            UIElementCollection.BoundCollection.AddElement(this);
        }

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
            foreach (var word in _text){
                str += word;
                wordDists.Add(chrIdx, _textBox.Font.MeasureString(str).X);
                chrIdx++;
            }
            var ordered = wordDists.OrderBy(w=> Math.Abs(diff - w.Value));

            _cursorPosition = ordered.First().Key;
            _cursor.X = _xAnchor + ordered.First().Value;
            _left.Alpha = 1;
            _right.Alpha = 1;
            _center.Alpha = 1;
        }

        public void UpdateLogic(double timeDelta){
            if (_boxFocused){
                if (_blinkTimer.ElapsedMilliseconds > 400){
                    _cursor.Enabled = !_cursor.Enabled;
                    _blinkTimer.Restart();
                }
            }
        }

        public void UpdateInput(ref InputState state){
            bool containsMouse = false;
            if (state.LeftButtonChange && state.LeftButtonState == ButtonState.Pressed){
                if (_center.HitTest(state.MousePos.X, state.MousePos.Y)){
                    containsMouse = true;
                }
                if (_left.HitTest(state.MousePos.X, state.MousePos.Y)) {
                    containsMouse = true;
                }
                if (_right.HitTest(state.MousePos.X, state.MousePos.Y)) {
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
    }
}
