#region

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#endregion

namespace Gondola.Logic{
    internal class InputState{
        public bool AllowKeyboardInterpretation;
        public bool AllowLeftButtonInterpretation;
        public bool AllowMouseMovementInterpretation;
        public bool AllowMouseScrollInterpretation;
        public bool AllowRightButtonInterpretation;

        public KeyboardState KeyboardState;
        public bool LeftButtonClick;
        public ButtonState LeftButtonState;
        public Point MousePos;
        public int MouseScrollChange;
        public InputState PrevState;
        public ButtonState RightButtonState;
        public Matrix ViewMatrix;
    }
}