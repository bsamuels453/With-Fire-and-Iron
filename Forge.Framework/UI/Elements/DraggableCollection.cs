#region

using System;
using Forge.Framework.Control;
using MonoGameUtility;

#endregion

namespace Forge.Framework.UI.Elements{
    /// <summary>
    /// Inherit from this class in order to enable dragging for a UIElementCollection object.
    /// </summary>
    public class DraggableCollection : UIElementCollection{
        public Func<DraggableCollection, Point, Point> ConstrainDrag;
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
            if (OnDragStart != null){
                OnDragStart.Invoke(this);
            }
        }

        void EndDrag(){
            _dragging = false;
            this.MouseManager.ReleaseExclusiveFocus(this.MouseController);
            if (OnDragEnd != null){
                OnDragEnd.Invoke(this);
            }
        }

        void OnMouseMove(ForgeMouseState state, float timeDelta, UIElementCollection caller){
            if (_dragging){
                var newPos = new Point(state.X + _mouseOffset.X, state.Y + _mouseOffset.Y);

                if (ConstrainDrag != null){
                    newPos = ConstrainDrag(this, newPos);
                }

                this.X = newPos.X;
                this.Y = newPos.Y;
            }
        }

        public event Action<DraggableCollection> OnDragStart;
        public event Action<DraggableCollection> OnDragEnd;
    }
}