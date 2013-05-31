#region

using Microsoft.Xna.Framework.Input;
using MonoGameUtility;

#endregion

namespace Forge.Framework{
    public class InputState{
        public bool AllowKeyboardInterpretation;
        public bool AllowLeftButtonInterpretation;
        public bool AllowMouseMovementInterpretation;
        public bool AllowMouseScrollInterpretation;
        public bool AllowRightButtonInterpretation;

        public KeyboardState KeyboardState;

        public bool LeftButtonChange;
        public bool LeftButtonClick;
        public ButtonState LeftButtonState;
        public bool MouseMoved;

        public Point MousePos;
        public int MouseScrollChange;

        public InputState PrevState;

        public bool RightButtonChange;
        public bool RightButtonClick;
        public ButtonState RightButtonState;
    }
}