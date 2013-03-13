#region

using System;
using Forge.Framework.Draw;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;

#endregion

namespace Forge.Framework.UI.Components{
    internal class HighlightComponent : IUIComponent, IAcceptMouseMovementEvent, IAcceptLeftButtonPressEvent, IAcceptLeftButtonReleaseEvent{
        #region HighlightTrigger enum

        public enum HighlightTrigger{
            InvalidTrigger,
            MouseEntryExit,
            MousePressRelease,
            None
        }

        #endregion

        readonly float _highlightTexOpacity;
        readonly string _highlightTexture;
        readonly HighlightTrigger _highlightTrigger;

        bool _Enabled;
        Sprite2D _highlightSprite;
        IUIInteractiveElement _owner;

        public HighlightComponent(string highlightTexture, HighlightTrigger highlightTrigger, float highlightTexOpacity = 0.3f, string identifier = ""){
            _highlightTexture = highlightTexture;
            _highlightTrigger = highlightTrigger;
            _highlightTexOpacity = highlightTexOpacity;
            Identifier = identifier;
            _Enabled = true;
        }

        #region IAcceptLeftButtonPressEvent Members

        public void OnLeftButtonPress(ref bool allowInterpretation, Point mousePos, Point prevMousePos){
            if (Enabled){
                if (_owner.ContainsMouse){
                    ProcHighlight();
                }
                else{
                    UnprocHighlight();
                }
            }
        }

        #endregion

        #region IAcceptLeftButtonReleaseEvent Members

        public void OnLeftButtonRelease(ref bool allowInterpretation, Point mousePos, Point prevMousePos){
            if (Enabled){
                UnprocHighlight();
            }
        }

        #endregion

        #region IAcceptMouseMovementEvent Members

        public void OnMouseMovement(ref bool allowInterpretation, Point mousePos, Point prevMousePos){
            if (Enabled){
                if (_owner.ContainsMouse){
                    ProcHighlight();
                }
                else{
                    UnprocHighlight();
                }
            }
        }

        #endregion

        #region IUIComponent Members

        public bool Enabled{
            get { return _Enabled; }
            set{
                _Enabled = value;
                _highlightSprite.Enabled = value;
            }
        }

        public void ComponentCtor(IUIElement owner, ButtonEventDispatcher ownerEventDispatcher){
            _owner = (IUIInteractiveElement) owner;

            //event stuff
            if (owner.DoesComponentExist<DraggableComponent>()){
                var dcomponent = _owner.GetComponent<DraggableComponent>();
                dcomponent.DragMovementDispatcher += OnOwnerDrag;
            }
            switch (_highlightTrigger){
                case HighlightTrigger.MouseEntryExit:
                    ownerEventDispatcher.OnMouseMovement.Add(this);
                    break;
                case HighlightTrigger.MousePressRelease:
                    ownerEventDispatcher.OnGlobalLeftPress.Add(this);
                    ownerEventDispatcher.OnGlobalLeftRelease.Add(this);
                    break;
                case HighlightTrigger.InvalidTrigger:
                    throw new Exception("invalid highlight trigger");
            }

            //create sprite
            _highlightSprite = new Sprite2D(_highlightTexture, (int) _owner.X, (int) _owner.Y, (int) _owner.Width, (int) _owner.Height, _owner.Depth - 0.01f, 0);
        }

        public void Update(){
        }

        public string Identifier { get; private set; }

        #endregion

        public void ProcHighlight(){
            _highlightSprite.Alpha = _highlightTexOpacity;
        }

        public void UnprocHighlight(){
            _highlightSprite.Alpha = 0;
        }

        //xxx untested
        void OnOwnerDrag(object caller, int dx, int dy){
            _highlightSprite.X += dx;
            _highlightSprite.Y += dy;
        }

        public static HighlightComponent ConstructFromObject(JObject obj, string identifier = ""){
            var data = obj.ToObject<HighlightComponentCtorData>();

            if (data.HighlightTexture == null || data.HighlightTrigger == HighlightTrigger.InvalidTrigger)
                throw new Exception("not enough information to generate highlight component");

            return new HighlightComponent(data.HighlightTexture, data.HighlightTrigger, data.HighlightTexOpacity, identifier);
        }

        #region Nested type: HighlightComponentCtorData

        struct HighlightComponentCtorData{
#pragma warning disable 649
            public float HighlightTexOpacity;
            public string HighlightTexture;
            public HighlightTrigger HighlightTrigger;
#pragma warning restore 649
        }

        #endregion
    }
}