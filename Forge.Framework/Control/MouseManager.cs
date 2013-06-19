#region

using System.Diagnostics;

#endregion

namespace Forge.Framework.Control{
    /// <summary>
    ///   Used to obtain and release control of the mouse. This class dispatches mouse updates via events passed to it through ObtainExclusiveFocus.
    /// </summary>
    public class MouseManager{
        /// <summary>
        ///   The currently active controller that the MouseManager class is dispatching events to.
        /// </summary>
        MouseController _curController;

        /// <summary>
        ///   The current state of the mouse. This is recorded here so that it can be used as the "prevstate" at next tick.
        /// </summary>
        ForgeMouseState _curState;

        /// <summary>
        ///   The previous active controller. This is recorded when cachePreviousController=true is passed to ObtainExclusiveFocus. When the current controller releases focus, this controller will become the current controller again.
        /// </summary>
        MouseController _prevController;

        public MouseManager(){
            _curState = new ForgeMouseState();
        }

        /// <summary>
        ///   Main method used for obtaining the mouse's focus so that events can be dispatched.
        /// </summary>
        /// <param name="controller"> The structure containing all of the events that the mouse will invoke. </param>
        /// <param name="cachePreviousController"> 
        /// Boolean representing whether or not the the current controller should
        /// be cached when it is replaced by the calling of this method. If the controller is cached, it will be
        /// reactivated when focus is released by the class that called the ObtainExclusiveFocus method that originally
        /// displaced it. 
        /// </param>
        public void ObtainExclusiveFocus(
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
        ///   Releases focus of the mouse so that the current controller will no longer recieve event updates. 
        ///   If true was passed to cachePreviousController when focus was obtained, then the previous controller
        ///   will be made active.
        /// </summary>
        public void ReleaseExclusiveFocus(){
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
            _curState = new ForgeMouseState(prevState, timeDelta);
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
    }
}