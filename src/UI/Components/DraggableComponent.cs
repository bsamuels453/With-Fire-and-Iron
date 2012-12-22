#region

using Gondola.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;

#endregion

namespace Gondola.UI.Components{
    internal delegate void DraggableObjectClamp(Button owner, ref int x, ref int y, int oldX, int oldY);

    internal delegate void OnComponentDrag(object caller, int dx, int dy);


    /// <summary>
    ///   allows a UI element to be dragged. Required element to be IUIInteractiveComponent
    /// </summary>
    internal class DraggableComponent : IUIComponent{
        bool _isEnabled;
        bool _isMoving;
        Vector2 _mouseOffset;
        Button _owner;

        #region properties

        #endregion

        #region ctor

        public DraggableComponent(){
            _mouseOffset = new Vector2();
        }

        public void ComponentCtor(Button owner){
            _owner = owner;
            _isEnabled = true;
            _isMoving = false;
        }

        #endregion

        #region IUIComponent Members

        public bool IsEnabled{
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }

        public void Update(InputState state, double timeDelta) {
            if (IsEnabled){
                //see if object will start moving
                if (!_isMoving &&
                    state.AllowLeftButtonInterpretation &&
                    state.LeftButtonChange &&
                    state.LeftButtonState == ButtonState.Pressed){

                    if (_owner.BoundingBox.Contains(state.MousePos.X, state.MousePos.Y)) {
                        _isMoving = true;
                        ////UIElementCollection.Collection.DisableEntryHandlers = true;
                        _mouseOffset.X = _owner.X - state.MousePos.X;
                        _mouseOffset.Y = _owner.Y - state.MousePos.Y;
                    }
                }

                //see if should stop object from moving
                if (_isMoving &&
                    state.AllowLeftButtonInterpretation &&
                    state.LeftButtonChange &&
                    state.LeftButtonState == ButtonState.Released){

                    _isMoving = false;
                    ////UIElementCollection.Collection.DisableEntryHandlers = false;
                }

                //if moving, apply movement stuff
                if (_isMoving &&
                    state.AllowMouseMovementInterpretation){
                    var oldX = (int)_owner.X;
                    var oldY = (int)_owner.Y;
                    var x = (int)(state.MousePos.X + _mouseOffset.X);
                    var y = (int)(state.MousePos.Y + _mouseOffset.Y);

                    if (DragMovementClamp != null) {
                        DragMovementClamp(_owner, ref x, ref y, oldX, oldY);
                    }

                    //this block checks if a drag clamp is preventing the owner from moving, if thats the case then kill the drag
                    var tempRect = new Rectangle(x - (int)_owner.BoundingBox.Width * 2, y - (int)_owner.BoundingBox.Height * 2, (int)_owner.BoundingBox.Width * 6, (int)_owner.BoundingBox.Height * 6);
                    if (!tempRect.Contains(state.MousePos.X, state.MousePos.Y)) {
                        //_isMoving = false;
                        //_owner.Owner.DisableEntryHandlers = false;
                    }

                    _owner.X = x;
                    _owner.Y = y;

                    if (DragMovementDispatcher != null) {
                        DragMovementDispatcher(_owner, x - oldX, y - oldY);
                    }
                    state.AllowMouseMovementInterpretation = false;
                }
            }
        }

        public void Draw(){
        }

        public string Identifier { get; private set; }

        #endregion

        public event OnComponentDrag DragMovementDispatcher;
        public event DraggableObjectClamp DragMovementClamp;

        public static DraggableComponent ConstructFromObject(JObject obj){
            return new DraggableComponent();
        }
    }
}