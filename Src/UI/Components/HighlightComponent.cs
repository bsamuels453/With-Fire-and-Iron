#region

using System;
using System.Diagnostics;
using Gondola.Draw;
using Gondola.Logic;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;

#endregion

namespace Gondola.UI.Components{
    internal class HighlightComponent : IUIComponent{
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

        Sprite2D _highlightSprite;
        bool _isEnabled;
        Button _owner;

        public HighlightComponent(string highlightTexture, HighlightTrigger highlightTrigger, float highlightTexOpacity = 0.3f, string identifier = ""){
            _highlightTexture = highlightTexture;
            _highlightTrigger = highlightTrigger;
            _highlightTexOpacity = highlightTexOpacity;
            Identifier = identifier;
            _isEnabled = true;
            Debug.Assert(_highlightTrigger != HighlightTrigger.InvalidTrigger);
        }

        #region IUIComponent Members

        public bool IsEnabled{
            get { return _isEnabled; }
            set{
                _isEnabled = value;
                _highlightSprite.Enabled = value;
            }
        }

        public void ComponentCtor(Button owner){
            _owner = owner;

            //event stuff
            if (owner.DoesComponentExist<DraggableComponent>()){
                var dcomponent = _owner.GetComponent<DraggableComponent>();
                dcomponent.DragMovementDispatcher += OnOwnerDrag;
            }

            //create sprite
            _highlightSprite = new Sprite2D(_highlightTexture, (int) _owner.X, (int) _owner.Y, (int) _owner.Width, (int) _owner.Height, 0);
        }

        public void Update(InputState state, double timeDelta){
            if (IsEnabled){
                if (_highlightTrigger == HighlightTrigger.MousePressRelease && state.AllowLeftButtonInterpretation) {
                    if (_owner.ContainsMouse){
                        if (state.LeftButtonChange){
                            if (state.LeftButtonState == ButtonState.Pressed){
                                //button has been pressed, highlight it
                                ProcHighlight();
                            }
                            else{
                                //button has been released, unhighlight it
                                UnprocHighlight();
                            }
                        }
                    }
                    else{
                        //mouse moved out of the button area, unhighlight
                        UnprocHighlight();
                    }
                }

                if (_highlightTrigger == HighlightTrigger.MouseEntryExit && state.AllowMouseMovementInterpretation){
                    if (_owner.ContainsMouse){
                        ProcHighlight();
                    }
                    else{
                        UnprocHighlight();
                    }
                }
            }
        }

        public void Draw(){
            _highlightSprite.Draw();
        }

        public string Identifier { get; private set; }

        #endregion

        public void ProcHighlight(){
            _highlightSprite.Opacity = _highlightTexOpacity;
        }

        public void UnprocHighlight(){
            _highlightSprite.Opacity = 0;
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