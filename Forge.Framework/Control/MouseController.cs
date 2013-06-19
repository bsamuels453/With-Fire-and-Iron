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
        public event Action<ForgeMouseState, float> OnMouseMovement;

        /// <summary>
        ///   Invoked when either of the two mouse buttons encounter a state change.
        /// </summary>
        public event Action<ForgeMouseState, float> OnMouseButton;

        /// <summary>
        ///   Invoked when the scrollwheel on the mouse moves.
        /// </summary>
        public event Action<ForgeMouseState, float> OnMouseScroll;

        /// <summary>
        ///   Invoked when this controller and its events are deactivated by another controller obtaining mouse focus.
        /// </summary>
        public event Action OnMouseFocusLost;

        /// <summary>
        ///   Invoked when this controller regains focus after being defocus'd by another controller.
        ///   Typically this is only called when the mouse's exclusivity status is removed, and all
        ///   registered controllers may recieve mouse input.
        /// </summary>
        public event Action OnMouseFocusGained;

        #endregion

        #region invokations

        public void SafeInvokeOnMouseMovement(ForgeMouseState state, float timeDelta){
            if (OnMouseMovement != null){
                OnMouseMovement.Invoke(state, timeDelta);
            }
        }

        public void SafeInvokeOnMouseButton(ForgeMouseState state, float timeDelta){
            if (OnMouseButton != null){
                OnMouseButton.Invoke(state, timeDelta);
            }
        }

        public void SafeInvokeOnMouseScroll(ForgeMouseState state, float timeDelta){
            if (OnMouseScroll != null){
                OnMouseScroll.Invoke(state, timeDelta);
            }
        }

        public void SafeInvokeOnFocusLost(){
            if (OnMouseFocusLost != null){
                OnMouseFocusLost.Invoke();
            }
        }

        public void SafeInvokeOnFocusRegained(){
            if (OnMouseFocusGained != null){
                OnMouseFocusGained.Invoke();
            }
        }

        #endregion
    }
}