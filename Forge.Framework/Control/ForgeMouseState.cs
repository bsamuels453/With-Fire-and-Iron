//#define RECORD_MOUSE
#define READ_MOUSE_FROM_FILE

#region

using System;
using System.Diagnostics;
using System.IO;
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
        static readonly StreamWriter _mouseRecord;
        static readonly StreamReader _mouseReader;

        readonly bool _leftButtonChange;
        readonly bool _leftButtonClick;
        readonly ButtonState _leftButtonState;
        readonly bool _mouseMoved;
        readonly Point _mousePos;
        readonly int _mouseScrollChange;
        readonly bool _rightButtonChange;
        readonly bool _rightButtonClick;
        readonly ButtonState _rightButtonState;
        readonly int _scrollWheelValue;

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
#if RECORD_MOUSE
            _mouseRecord = new StreamWriter("mouse.dat");
            _mouseRecord.AutoFlush = true;
#endif

#if READ_MOUSE_FROM_FILE
            _mouseReader = new StreamReader("mouse.dat");
#endif
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
            _scrollWheelValue = 0;
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
            PrevState.BlockLeftMButton = false;
            PrevState.BlockMPosition = false;
            PrevState.BlockRightMButton = false;
            PrevState.BlockScrollWheel = false;
            var curState = Mouse.GetState();


#if RECORD_MOUSE
            WriteMouseStateToRecord(curState);
#endif

#if READ_MOUSE_FROM_FILE
            curState = ReadMouseStateFromRecord();
#endif

            _mousePos = new Point(curState.X, curState.Y);

            _mouseScrollChange = curState.ScrollWheelValue - PrevState._scrollWheelValue;
            _scrollWheelValue = curState.ScrollWheelValue;

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

        static void WriteMouseStateToRecord(MouseState state){
            _mouseRecord.WriteLine
                (state.X + " " + state.Y + " " + state.LeftButton + " " + state.RightButton + " " + state.MiddleButton + " " + state.ScrollWheelValue);
        }

        static MouseState ReadMouseStateFromRecord(){
            var line = _mouseReader.ReadLine();
            if (line.Length == 0){
                throw new Exception("End of input stream");
            }
            var elements = line.Split(' ');

            int x = int.Parse(elements[0]);
            int y = int.Parse(elements[1]);
            ButtonState left = (ButtonState) Enum.Parse(typeof (ButtonState), elements[2]);
            ButtonState right = (ButtonState) Enum.Parse(typeof (ButtonState), elements[3]);
            ButtonState middle = (ButtonState) Enum.Parse(typeof (ButtonState), elements[4]);
            int scrollwheel = int.Parse(elements[5]);

            var state = new MouseState(x, y, scrollwheel, left, middle, right, ButtonState.Released, ButtonState.Released);
            return state;
        }

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