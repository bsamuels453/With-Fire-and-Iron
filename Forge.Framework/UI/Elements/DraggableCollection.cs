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
        /// <summary>
        /// caller collection, new point, old point.
        /// return clampped point.
        /// </summary>
        public Func<DraggableCollection, Point, Point, Point> ConstrainDrag;

        bool _dragging;
        bool _draggable;
        Point _mouseOffset;

        public DraggableCollection(UIElementCollection parent, FrameStrata.Level depth, Rectangle boundingBox, string alias) :
            base(parent, depth, boundingBox, alias + "_Draggable"){
            this.OnLeftDown += OnLeftMouseDown;
            this.OnLeftRelease += OnLeftMouseUp;
            this.OnMouseFocusLost += OnFocusLost;
            this.OnMouseMovement += OnMouseMove;
            _draggable = true;
        }

        protected bool Draggable{
            get { return _draggable; }
            set{
                _draggable = value;
                _dragging = false;
            }
        }

        void OnLeftMouseDown(ForgeMouseState state, float timeDelta, UIElementCollection caller){
            if (!state.BlockLeftMButton){
                if (ContainsMouse && _draggable){
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
                    newPos = ConstrainDrag(this, newPos, new Point(state.X, state.Y));
                }

                this.X = newPos.X;
                this.Y = newPos.Y;
            }
        }

        public event Action<DraggableCollection> OnDragStart;
        public event Action<DraggableCollection> OnDragEnd;
    }
}