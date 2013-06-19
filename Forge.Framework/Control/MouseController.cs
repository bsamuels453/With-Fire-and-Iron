#region

using System;

#endregion

namespace Forge.Framework.Control{
    /// <summary>
    ///   Container that holds all of the events that the mouse can invoke.
    /// </summary>
    public class MouseController{
        public readonly object ControllerOwner;

        public MouseController(object owner){
            ControllerOwner = owner;
        }

        #region events

        /// <summary>
        ///   Invoked when the mouse moves.
        /// </summary>
        public event Action<ForgeMouseState> OnMouseMovement;

        /// <summary>
        ///   Invoked when either of the two mouse buttons encounter a state change.
        /// </summary>
        public event Action<ForgeMouseState> OnMouseButton;

        /// <summary>
        ///   Invoked when the scrollwheel on the mouse moves.
        /// </summary>
        public event Action<ForgeMouseState> OnMouseScroll;

        /// <summary>
        ///   Invoked when this controller and its events are deactivated by another controller obtaining mouse focus. Boolean parameter represents whether or not the focus was obtained with cachePreviousController=true.
        /// </summary>
        public event Action<bool> OnFocusLost;

        /// <summary>
        ///   Invoked when this controller regains focus after being defocus'd by another controller.
        /// </summary>
        public event Action OnFocusRegained;

        #endregion

        #region invokations

        public void SafeInvokeOnMouseMovement(ForgeMouseState state){
            if (OnMouseMovement != null){
                OnMouseMovement.Invoke(state);
            }
        }

        public void SafeInvokeOnMouseButton(ForgeMouseState state){
            if (OnMouseButton != null){
                OnMouseButton.Invoke(state);
            }
        }

        public void SafeInvokeOnMouseScroll(ForgeMouseState state){
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

        #endregion
    }
}