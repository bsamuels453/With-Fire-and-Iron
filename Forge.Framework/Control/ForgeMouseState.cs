#region

using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#endregion

namespace Forge.Framework.Control{
    /// <summary>
    ///   Represents the current state of the mouse.
    /// </summary>
    public class ForgeMouseState{
        #region private fields

        static readonly Stopwatch _rightclickTimer;
        static readonly Stopwatch _leftclickTimer;

        readonly bool _leftButtonChange;
        readonly bool _leftButtonClick;
        readonly ButtonState _leftButtonState;
        readonly bool _mouseMoved;
        readonly Point _mousePos;
        readonly int _mouseScrollChange;
        readonly bool _rightButtonChange;
        readonly bool _rightButtonClick;
        readonly ButtonState _rightButtonState;

        #endregion

        #region public fields

        /// <summary>
        ///   Boolean representing whether or not access to state information about the left mouse button should be blocked.
        /// </summary>
        public bool BlockLeftMButton;

        /// <summary>
        ///   Boolean representing whether or not access to state information about the mouse's position should be blocked.
        /// </summary>
        public bool BlockMPosition;

        /// <summary>
        ///   Boolean representing whether or not access to state information about the right mouse button should be blocked.
        /// </summary>
        public bool BlockRightMButton;

        /// <summary>
        ///   Boolean representing whether or not access to state information about the mouse's scroll wheel should be blocked.
        /// </summary>
        public bool BlockScrollWheel;

        #endregion

        #region ctors

        static ForgeMouseState(){
            _rightclickTimer = new Stopwatch();
            _leftclickTimer = new Stopwatch();
        }

        /// <summary>
        ///   this constructor is only called during the first tick, it creates a fake prevState
        /// </summary>
        public ForgeMouseState(bool fillPrevState = true){
            _leftButtonChange = false;
            _leftButtonClick = false;
            _rightButtonChange = false;
            _rightButtonClick = false;
            _leftButtonState = Mouse.GetState().LeftButton;
            _rightButtonState = Mouse.GetState().RightButton;
            _mouseScrollChange = 0;
            _mousePos = new Point(Mouse.GetState().X, Mouse.GetState().Y);
            _mouseMoved = false;
            if (fillPrevState){
                PrevState = new ForgeMouseState(false);
            }
        }

        /// <summary>
        ///   normal mouseState constructor
        /// </summary>
        /// <param name="prevState"> The state of the mouse during the previous tick. </param>
        /// <param name="timeDelta"> The time between the current tick and the previous tick </param>
        public ForgeMouseState(ForgeMouseState prevState, double timeDelta){
            BlockLeftMButton = false;
            BlockRightMButton = false;
            BlockScrollWheel = false;
            BlockMPosition = false;

            PrevState = prevState;
            var curState = Mouse.GetState();

            _mousePos = new Point(curState.X, curState.Y);
            _mouseScrollChange = curState.ScrollWheelValue - PrevState._mouseScrollChange;
            _leftButtonState = curState.LeftButton;
            _rightButtonState = curState.RightButton;
            if (PrevState._mousePos.X != curState.X || PrevState._mousePos.Y != curState.Y){
                _mouseMoved = true;
            }

            if (PrevState._leftButtonState != curState.LeftButton){
                _leftButtonChange = true;
                if (curState.LeftButton == ButtonState.Released){
                    //check if this qualifies as a click
                    if (timeDelta < 200){
                        _leftButtonClick = true;
                        _leftclickTimer.Reset();
                    }
                    else{
                        _leftclickTimer.Reset();
                        _leftButtonClick = false;
                    }
                }
                else{ //button was pressed so start the click timer
                    _leftclickTimer.Start();
                }
            }
            if (PrevState._rightButtonState != curState.RightButton){
                _rightButtonChange = true;
                if (curState.RightButton == ButtonState.Released){
                    //check if this qualifies as a click
                    if (timeDelta < 200){
                        _rightButtonClick = true;
                        _rightclickTimer.Reset();
                    }
                    else{
                        _rightclickTimer.Reset();
                        _rightButtonClick = false;
                    }
                }
                else{ //button was pressed so start the click timer
                    _rightclickTimer.Start();
                }
            }
        }

        #endregion

        #region properties

        /// <summary>
        ///   boolean representing whether or not the left mouse button's state has changed since the previous tick.
        /// </summary>
        public bool LeftButtonChange{
            get{
                Debug.Assert(!BlockLeftMButton);
                return _leftButtonChange;
            }
        }

        /// <summary>
        ///   boolean representing whether or not the left mouse button has been "clicked" (pressed, then released). This is true when the button is released.
        /// </summary>
        public bool LeftButtonClick{
            get{
                Debug.Assert(!BlockLeftMButton);
                return _leftButtonClick;
            }
        }

        /// <summary>
        ///   XNA enum representing the left mouse button's current state.
        /// </summary>
        public ButtonState LeftButtonState{
            get{
                Debug.Assert(!BlockLeftMButton);
                return _leftButtonState;
            }
        }

        /// <summary>
        ///   boolean representing whether or not the right mouse button's state has changed since the previous tick.
        /// </summary>
        public bool RightButtonChange{
            get{
                Debug.Assert(!BlockRightMButton);
                return _rightButtonChange;
            }
        }

        /// <summary>
        ///   boolean representing whether or not the right mouse button has been "clicked" (pressed, then released). This is true when the button is released.
        /// </summary>
        public bool RightButtonClick{
            get{
                Debug.Assert(!BlockRightMButton);
                return _rightButtonClick;
            }
        }

        /// <summary>
        ///   XNA enum representing the right mouse button's current state.
        /// </summary>
        public ButtonState RightButtonState{
            get{
                Debug.Assert(!BlockRightMButton);
                return _rightButtonState;
            }
        }

        /// <summary>
        ///   Integer representing how much the mouse wheel has moved since the last tick. Scrolling forward provides a positive integer.
        /// </summary>
        public int MouseScrollChange{
            get{
                Debug.Assert(!BlockRightMButton);
                return _mouseScrollChange;
            }
        }

        /// <summary>
        ///   Struct representing the XY coordinates of the mouse.
        /// </summary>
        public Point MousePos{
            get{
                Debug.Assert(!BlockMPosition);
                return _mousePos;
            }
        }

        /// <summary>
        ///   Struct representing the X coordinates of the mouse.
        /// </summary>
        public int X{
            get{
                Debug.Assert(!BlockMPosition);
                return _mousePos.X;
            }
        }

        /// <summary>
        ///   Struct representing the Y coordinates of the mouse.
        /// </summary>
        public int Y{
            get{
                Debug.Assert(!BlockMPosition);
                return _mousePos.Y;
            }
        }

        /// <summary>
        ///   Boolean representing whether or not the mouse's XY coordinates have changed since the last tick.
        /// </summary>
        public bool MouseMoved{
            get{
                Debug.Assert(!BlockMPosition);
                return _mouseMoved;
            }
        }

        /// <summary>
        ///   The ForgeMouseState of the previous tick.
        /// </summary>
        public ForgeMouseState PrevState { get; private set; }

        #endregion
    }
}