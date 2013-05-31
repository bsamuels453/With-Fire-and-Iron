#region

using MonoGameUtility;

#endregion

namespace Forge.Framework.UI{
    public delegate void OnBasicMouseEvent(int identifier);

    public interface IUIInteractiveElement : IUIElement{
        bool Enabled { get; set; }
        bool ContainsMouse { get; }

        event OnBasicMouseEvent OnLeftClickDispatcher;
        event OnBasicMouseEvent OnLeftPressDispatcher;
        event OnBasicMouseEvent OnLeftReleaseDispatcher;
    }

    #region element-component event handling interfaces

    //each mouse based event accepts something along the lines of (ref bool allowInterpretation, Vector2 mousePosition, Vector2 mousePosChange)
    public interface IAcceptLeftButtonClickEvent{
        void OnLeftButtonClick(ref bool allowInterpretation, Point mousePos, Point prevMousePos);
    }

    public interface IAcceptLeftButtonPressEvent{
        void OnLeftButtonPress(ref bool allowInterpretation, Point mousePos, Point prevMousePos);
    }

    public interface IAcceptLeftButtonReleaseEvent{
        void OnLeftButtonRelease(ref bool allowInterpretation, Point mousePos, Point prevMousePos);
    }

    public interface IAcceptMouseEntryEvent{
        void OnMouseEntry(ref bool allowInterpretation, Point mousePos, Point prevMousePos);
    }

    public interface IAcceptMouseExitEvent{
        void OnMouseExit(ref bool allowInterpretation, Point mousePos, Point prevMousePos);
    }

    public interface IAcceptMouseMovementEvent{
        void OnMouseMovement(ref bool allowInterpretation, Point mousePos, Point prevMousePos);
    }

    public interface IAcceptMouseScrollEvent{
        void OnMouseScrollwheel(ref bool allowInterpretation, float wheelChange, Point mousePos);
    }

    #endregion
}