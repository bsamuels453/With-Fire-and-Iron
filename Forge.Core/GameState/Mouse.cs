#region

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNAMouse = Microsoft.Xna.Framework.Input.Mouse;
using XNAMouseState = Microsoft.Xna.Framework.Input.MouseState;

#endregion

namespace Forge.Core.GameState{
    /// <summary>
    ///   Used to obtain and release control of the mouse. This class dispatches mouse updates via events passed to it through ObtainFocus.
    /// </summary>
    internal class Mouse{
        /// <summary>
        ///   The currently active controller that the Mouse class is dispatching events to.
        /// </summary>
        MouseController _curController;

        /// <summary>
        ///   The current state of the mouse. This is recorded here so that it can be used as the "prevstate" at next tick.
        /// </summary>
        MouseState _curState;

        /// <summary>
        ///   The previous active controller. This is recorded when cachePreviousController=true is passed to ObtainFocus. When the current controller releases focus, this controller will become the current controller again.
        /// </summary>
        MouseController _prevController;

        public Mouse(){
            _curState = new MouseState();
        }

        /// <summary>
        ///   Main method used for obtaining the mouse's focus so that events can be dispatched.
        /// </summary>
        /// <param name="controller"> The structure containing all of the events that the mouse will invoke. </param>
        /// <param name="cachePreviousController"> Boolean representing whether or not the the current controller should be cached when it is replaced by the calling of this method. If the controller is cached, it will be reactivated when focus is released by the class that called the ObtainFocus method that originally displaced it. </param>
        public void ObtainFocus(
            MouseController controller,
            bool cachePreviousController = false){
            if (_curController != null){
                _curController.SafeInvokeOnFocusLost(isRestorationPossible: cachePreviousController);
            }

            //save controller so we can switch back to it after focus lost
            if (cachePreviousController){
                Debug.Assert(_prevController == null);
                _prevController = _curController;
            }
            _curController = controller;
        }

        /// <summary>
        ///   Releases focus of the mouse so that the current controller will no longer recieve event updates. If true was passed to cachePreviousController when focus was obtained, then the previous controller will be made active.
        /// </summary>
        public void ReleaseFocus(){
            _curController = null;
            if (_prevController != null){
                _prevController.SafeInvokeOnFocusRegained();
                _curController = _prevController;
            }
        }

        /// <summary>
        ///   Updates the mouse and invokes any of the controller's events that are relevant to the update.
        /// </summary>
        /// <param name="timeDelta"> Time since the last tick, in milliseconds. </param>
        public void UpdateMouse(double timeDelta){
            var prevState = _curState;
            _curState = new MouseState(prevState, timeDelta);
            if (_curController != null){
                if (_curState.LeftButtonChange || _curState.RightButtonChange){
                    _curController.SafeInvokeOnMouseButton(_curState);
                }
                if (_curState.MouseScrollChange != 0){
                    _curController.SafeInvokeOnMouseScroll(_curState);
                }
                if (_curState.MouseMoved){
                    _curController.SafeInvokeOnMouseMovement(_curState);
                }
            }
        }

        #region Nested type: MouseController

        /// <summary>
        ///   Container that holds all of the events that the mouse can invoke.
        /// </summary>
        public class MouseController{
            public readonly object ControllerOwner;

            public MouseController(object owner){
                ControllerOwner = owner;
            }

            /// <summary>
            ///   Invoked when the mouse moves.
            /// </summary>
            public event Action<MouseState> OnMouseMovement;

            /// <summary>
            ///   Invoked when either of the two mouse buttons encounter a state change.
            /// </summary>
            public event Action<MouseState> OnMouseButton;

            /// <summary>
            ///   Invoked when the scrollwheel on the mouse moves.
            /// </summary>
            public event Action<MouseState> OnMouseScroll;

            /// <summary>
            ///   Invoked when this controller and its events are deactivated by another controller obtaining mouse focus. Boolean parameter represents whether or not the focus was obtained with cachePreviousController=true.
            /// </summary>
            public event Action<bool> OnFocusLost;

            /// <summary>
            ///   Invoked when this controller regains focus after being defocus'd by another controller.
            /// </summary>
            public event Action OnFocusRegained;

            public void SafeInvokeOnMouseMovement(MouseState state){
                if (OnMouseMovement != null){
                    OnMouseMovement.Invoke(state);
                }
            }

            public void SafeInvokeOnMouseButton(MouseState state){
                if (OnMouseButton != null){
                    OnMouseButton.Invoke(state);
                }
            }

            public void SafeInvokeOnMouseScroll(MouseState state){
                if (OnMouseScroll != null){
                    OnMouseScroll.Invoke(state);
                }
            }

            public void SafeInvokeOnFocusLost(bool isRestorationPossible){
                if (OnFocusLost != null){
                    OnFocusLost.Invoke(isRestorationPossible);
                }
            }

            public void SafeInvokeOnFocusRegained(){
                if (OnFocusRegained != null){
                    OnFocusRegained.Invoke();
                }
            }
        }

        #endregion

        #region Nested type: MouseState

        /// <summary>
        ///   Represents the current state of the mouse.
        /// </summary>
        public class MouseState{
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

            static MouseState(){
                _rightclickTimer = new Stopwatch();
                _leftclickTimer = new Stopwatch();
            }

            /// <summary>
            ///   this constructor is only called during the first tick, it creates a fake prevState
            /// </summary>
            public MouseState(bool fillPrevState = true){
                _leftButtonChange = false;
                _leftButtonClick = false;
                _rightButtonChange = false;
                _rightButtonClick = false;
                _leftButtonState = XNAMouse.GetState().LeftButton;
                _rightButtonState = XNAMouse.GetState().RightButton;
                _mouseScrollChange = 0;
                _mousePos = new Point(XNAMouse.GetState().X, XNAMouse.GetState().Y);
                _mouseMoved = false;
                if (fillPrevState){
                    PrevState = new MouseState(false);
                }
            }

            /// <summary>
            ///   normal mouseState constructor
            /// </summary>
            /// <param name="prevState"> The state of the mouse during the previous tick. </param>
            /// <param name="timeDelta"> The time between the current tick and the previous tick </param>
            public MouseState(MouseState prevState, double timeDelta){
                BlockLeftMButton = false;
                BlockRightMButton = false;
                BlockScrollWheel = false;
                BlockMPosition = false;

                PrevState = prevState;
                var curState = XNAMouse.GetState();

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
            ///   Boolean representing whether or not the mouse's XY coordinates have changed since the last tick.
            /// </summary>
            public bool MouseMoved{
                get{
                    Debug.Assert(!BlockMPosition);
                    return _mouseMoved;
                }
            }

            /// <summary>
            ///   The MouseState of the previous tick.
            /// </summary>
            public MouseState PrevState { get; private set; }
        }

        #endregion
    }
}