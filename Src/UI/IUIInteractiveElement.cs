using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gondola.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Gondola.UI {
    internal delegate void OnBasicMouseEvent(int identifier);

    internal interface IUIInteractiveElement : IUIElement, IInputUpdates {
        bool Enabled { get; set; }
        bool ContainsMouse { get; }

        event OnBasicMouseEvent OnLeftClickDispatcher;
        event OnBasicMouseEvent OnLeftPressDispatcher;
        event OnBasicMouseEvent OnLeftReleaseDispatcher;

    }

    #region element-component event handling interfaces

    //each mouse based event accepts something along the lines of (ref bool allowInterpretation, Vector2 mousePosition, Vector2 mousePosChange)
    internal interface IAcceptLeftButtonClickEvent {
        void OnLeftButtonClick(ref bool allowInterpretation, Point mousePos, Point prevMousePos);
    }

    internal interface IAcceptLeftButtonPressEvent {
        void OnLeftButtonPress(ref bool allowInterpretation, Point mousePos, Point prevMousePos);
    }

    internal interface IAcceptLeftButtonReleaseEvent {
        void OnLeftButtonRelease(ref bool allowInterpretation, Point mousePos, Point prevMousePos);
    }

    internal interface IAcceptMouseEntryEvent {
        void OnMouseEntry(ref bool allowInterpretation, Point mousePos, Point prevMousePos);
    }

    internal interface IAcceptMouseExitEvent {
        void OnMouseExit(ref bool allowInterpretation, Point mousePos, Point prevMousePos);
    }

    internal interface IAcceptMouseMovementEvent {
        void OnMouseMovement(ref bool allowInterpretation, Point mousePos, Point prevMousePos);
    }

    internal interface IAcceptMouseScrollEvent {
        void OnMouseScrollwheel(ref bool allowInterpretation, float wheelChange, Point mousePos);
    }

    #endregion
}
