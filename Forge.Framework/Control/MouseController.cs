﻿#region

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
        ///   Invoked when this controller and its events are deactivated by another controller obtaining mouse focus.
        /// </summary>
        public event Action OnMouseFocusLost;

        /// <summary>
        ///   Invoked when this controller regains focus after being defocus'd by another controller.
        ///   Typically this is only called when the mouse's exclusivity status is removed, and all
        ///   registered controllers may recieve mouse input.
        /// </summary>
        public event Action OnMouseFocusRegained;

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

        public void SafeInvokeOnFocusLost(){
            if (OnMouseFocusLost != null){
                OnMouseFocusLost.Invoke();
            }
        }

        public void SafeInvokeOnFocusRegained(){
            if (OnMouseFocusRegained != null){
                OnMouseFocusRegained.Invoke();
            }
        }

        #endregion
    }
}