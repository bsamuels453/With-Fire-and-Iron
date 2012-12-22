#region

using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#endregion

namespace Gondola.Logic{
    internal class InputHandler{
        readonly Stopwatch _clickTimer;
        public InputState CurrentControlState;
        MouseState _prevMouseState;

        public InputHandler(){
            _prevMouseState = Mouse.GetState();
            _clickTimer = new Stopwatch();
        }

        public void Update(){
            //all this crap updates the CurrentControlState to whatever the hell is going on

            //notice the conditions for whether fields such as AllowMouseMovementInterpretation should be true or not
            //these were originally intended to reduce overhead for when no difference between current state and previous existed
            //but they caused loads of problems. the actual bool state they were supposed to be set to has been commented out and set to true
            //maybe fix in future if run out of updateinput allowance

            var curMouseState = Mouse.GetState();
            var curKeyboardState = Keyboard.GetState();

            var curControlState = new InputState();

            if (_prevMouseState.X != curMouseState.X || _prevMouseState.Y != curMouseState.Y){
                curControlState.AllowMouseMovementInterpretation = true;
            }
            else{
                curControlState.AllowMouseMovementInterpretation = true;
                //curControlState.AllowMouseMovementInterpretation = false;
            }
            //mouse movement stuff needs to be updated every time, regardless of change
            curControlState.MousePos = new Point();
            curControlState.MousePos.X = curMouseState.X;
            curControlState.MousePos.Y = curMouseState.Y;
            curControlState.LeftButtonState = curMouseState.LeftButton;
            curControlState.RightButtonState = curMouseState.RightButton;
            curControlState.MouseScrollChange = curMouseState.ScrollWheelValue - _prevMouseState.ScrollWheelValue;

            if (_prevMouseState.LeftButton != curMouseState.LeftButton){
                curControlState.AllowLeftButtonInterpretation = true;
                if (curMouseState.LeftButton == ButtonState.Released){
                    //check if this qualifies as a click
                    if (_clickTimer.ElapsedMilliseconds < 200){
                        curControlState.LeftButtonClick = true;
                        _clickTimer.Reset();
                    }
                    else{
                        _clickTimer.Reset();
                        curControlState.LeftButtonClick = false;
                    }
                }
                else{ //button was pressed so start the click timer
                    _clickTimer.Start();
                }
            }
            else
                curControlState.AllowLeftButtonInterpretation = true;

            if (_prevMouseState.RightButton != curMouseState.RightButton){
            }
            else
                curControlState.AllowRightButtonInterpretation = true;

            if (_prevMouseState.ScrollWheelValue != curMouseState.ScrollWheelValue)
                curControlState.AllowMouseScrollInterpretation = true;
            else
                curControlState.AllowMouseScrollInterpretation = true;

            curControlState.KeyboardState = curKeyboardState;

            _prevMouseState = curMouseState;

            curControlState.ViewMatrix = Matrix.CreateLookAt(Gbl.CameraPosition, Gbl.CameraTarget, Vector3.Up);


            if (CurrentControlState != null){
                curControlState.PrevState = CurrentControlState;
                CurrentControlState.PrevState = null;
            }
            else{
                curControlState.PrevState = new InputState();
            }
            CurrentControlState = curControlState;
        }
    }
}