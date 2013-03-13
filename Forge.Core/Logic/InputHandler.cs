#region

using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Forge.Framework;

#endregion

namespace Forge.Core.Logic{
    internal class InputHandler{
        readonly Stopwatch _clickTimer;
        public InputState CurrentInputState;
        MouseState _prevMouseState;

        public InputHandler(){
            _prevMouseState = Mouse.GetState();
            _clickTimer = new Stopwatch();
        }

        public void Update(){
            //all this crap updates the CurrentInputState to whatever the hell is going on

            //xxx right button stuff isnt implemented 
            var newMouseState = Mouse.GetState();
            var newKeyboardState = Keyboard.GetState();

            var newInputState = new InputState();

            newInputState.AllowKeyboardInterpretation = true;
            newInputState.AllowLeftButtonInterpretation = true;
            newInputState.AllowMouseMovementInterpretation = true;
            newInputState.AllowMouseScrollInterpretation = true;
            newInputState.AllowRightButtonInterpretation = true;

            //mouse movement stuff needs to be updated every time, regardless of change
            newInputState.MousePos = new Point();
            newInputState.MousePos.X = newMouseState.X;
            newInputState.MousePos.Y = newMouseState.Y;
            newInputState.LeftButtonState = newMouseState.LeftButton;
            newInputState.RightButtonState = newMouseState.RightButton;
            newInputState.MouseScrollChange = newMouseState.ScrollWheelValue - _prevMouseState.ScrollWheelValue;
            newInputState.KeyboardState = newKeyboardState;

            if (_prevMouseState.X != newMouseState.X || _prevMouseState.Y != newMouseState.Y) {
                newInputState.MouseMoved = true;
            }

            if (_prevMouseState.LeftButton != newMouseState.LeftButton) {
                newInputState.LeftButtonChange = true;

                newInputState.AllowLeftButtonInterpretation = true;
                if (newMouseState.LeftButton == ButtonState.Released) {
                    //check if this qualifies as a click
                    if (_clickTimer.ElapsedMilliseconds < 200) {
                        newInputState.LeftButtonClick = true;
                        _clickTimer.Reset();
                    }
                    else {
                        _clickTimer.Reset();
                        newInputState.LeftButtonClick = false;
                    }
                }
                else { //button was pressed so start the click timer
                    _clickTimer.Start();
                }
            }

            _prevMouseState = newMouseState;

            if (CurrentInputState != null){
                newInputState.PrevState = CurrentInputState;
                CurrentInputState.PrevState = null;
            }
            else{
                newInputState.PrevState = new InputState();
            }
            CurrentInputState = newInputState;
        }
    }
}