#region

using Forge.Framework.Control;
using MonoGameUtility;

#endregion

namespace Forge.Framework.UI.Elements{
    /// <summary>
    /// Inherit from this class in order to enable dragging for a UIElementCollection object.
    /// </summary>
    public class DraggableCollection : UIElementCollection{
        bool _dragging;
        bool _enableDrag;
        Point _mouseOffset;

        public DraggableCollection(UIElementCollection parent, FrameStrata.Level depth, Rectangle boundingBox, string alias) :
            base(parent, depth, boundingBox, alias + "_Draggable"){
            this.OnLeftDown += OnLeftMouseDown;
            this.OnLeftRelease += OnLeftMouseUp;
            this.OnMouseFocusLost += OnFocusLost;
            this.OnMouseMovement += OnMouseMove;
            _enableDrag = true;
        }

        protected bool EnableDrag{
            get { return _enableDrag; }
            set{
                _enableDrag = value;
                _dragging = false;
            }
        }

        void OnLeftMouseDown(ForgeMouseState state, float timeDelta, UIElementCollection caller){
            if (!state.BlockLeftMButton){
                if (ContainsMouse && _enableDrag){
                    StartDrag(state);
                }
            }
        }

        void OnLeftMouseUp(ForgeMouseState state, float timeDelta, UIElementCollection caller){
            if (!state.BlockLeftMButton){
                if (_dragging){
                    EndDrag();
                }
            }
        }

        void OnFocusLost(UIElementCollection caller){
            if (_dragging){
                EndDrag();
            }
        }

        void StartDrag(ForgeMouseState state){
            _dragging = true;
            this.MouseManager.ObtainExclusiveFocus(this.MouseController);
            _mouseOffset.X = this.X - state.X;
            _mouseOffset.Y = this.Y - state.Y;
            state.BlockLeftMButton = true;
        }

        void EndDrag(){
            _dragging = false;
            this.MouseManager.ReleaseExclusiveFocus(this.MouseController);
        }

        void OnMouseMove(ForgeMouseState state, float timeDelta, UIElementCollection caller){
            if (_dragging){
                var x = state.X + _mouseOffset.X;
                var y = state.Y + _mouseOffset.Y;

                //implement mouse clamping here

                this.X = x;
                this.Y = y;
            }
        }
    }
}