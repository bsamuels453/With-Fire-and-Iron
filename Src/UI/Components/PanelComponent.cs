#region

using Gondola.Logic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;

#endregion

namespace Gondola.UI.Components{
    /// <summary>
    ///   prevents mouse interactions from falling through the owner's bounding box
    /// </summary>
    internal class PanelComponent : IUIComponent, IAcceptLeftButtonPressEvent, IAcceptLeftButtonReleaseEvent, IAcceptMouseScrollEvent {
        IUIInteractiveElement _owner;

        public PanelComponent() {
            Enabled = true;
        }

        #region IAcceptLeftButtonPressEvent Members

        public void OnLeftButtonPress(ref bool allowInterpretation, Point mousePos, Point prevMousePos) {
            PreventClickFallthrough(ref allowInterpretation, mousePos);
        }

        #endregion

        #region IAcceptLeftButtonReleaseEvent Members

        public void OnLeftButtonRelease(ref bool allowInterpretation, Point mousePos, Point prevMousePos) {
            PreventClickFallthrough(ref allowInterpretation, mousePos);
        }

        #endregion

        #region IAcceptMouseScrollEvent Members

        public void OnMouseScrollwheel(ref bool allowInterpretation, float wheelChange, Point mousePos) {
            PreventClickFallthrough(ref allowInterpretation, mousePos);
        }

        #endregion

        #region IUIComponent Members

        public void ComponentCtor(IUIElement owner, ButtonEventDispatcher ownerEventDispatcher) {
            _owner = (IUIInteractiveElement)owner;
            ownerEventDispatcher.OnGlobalLeftPress.Add(this);
            ownerEventDispatcher.OnGlobalLeftRelease.Add(this);
            ownerEventDispatcher.OnMouseScroll.Add(this);
        }

        public bool Enabled { get; set; }

        public void Update() {
        }

        public string Identifier { get; private set; }

        #endregion

        void PreventClickFallthrough(ref bool allowLeftButtonInterpretation, Point mousePos) {
            if (allowLeftButtonInterpretation) {
                if (_owner.HitTest(mousePos.X, mousePos.Y)) {
                    allowLeftButtonInterpretation = false;
                }
            }
        }

        public static PanelComponent ConstructFromObject(JObject obj) {
            return new PanelComponent();
        }
    }
}