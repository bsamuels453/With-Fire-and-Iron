#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Forge.Framework.Control;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework.Input;
using MonoGameUtility;

#endregion

namespace Forge.Framework.UI{
    /// <summary>
    /// Groups together UI elements into a collection that can be manipulated in special ways.
    /// This class allows basic UI elements like sprites to be extended upon to create fancy
    /// stuff like fading buttons, among other things. UIElementCollections can be further grouped
    /// together to create advanced UI objects such as dialogue boxes, menus, and tooltips.
    /// </summary>
    internal class UIElementCollection : IUIElement{
        const float _hoverTime = 200;
        readonly PriorityQueue<IUIElement> _elements;
        readonly FrameStrata _frameStrata;
        readonly Stopwatch _hoverTimer;
        readonly MouseController _mouseController;
        readonly MouseManager _mouseManager;
        readonly UIElementCollection _parentCollection;
        float _alpha;
        Rectangle _boundingBox;

        /// <summary>
        /// parent constructor
        /// </summary>
        public UIElementCollection(MouseManager mouseManager){
            _frameStrata = new FrameStrata();
            _elements = new PriorityQueue<IUIElement>();
            _boundingBox = new Rectangle(0, 0, Resource.ScreenSize.X, Resource.ScreenSize.Y);
            _parentCollection = null;
            _mouseManager = mouseManager;
            _mouseController = new MouseController(this);
            _alpha = 1;
            _hoverTimer = new Stopwatch();
            Debug.Assert(_globalUIParent == null);
            _globalUIParent = this;
            SetupEventPropagation();
            SetupEventPropagationToChildren();
        }

        /// <summary>
        /// standard collection constructor
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="depth">The target depth for this collection.</param>
        /// <param name="boundingBox">
        /// A boundingbox that represents the max bounds of the collection. Contained
        /// IUIElements may not exceede the limits of the boundingbox of its collection.
        /// </param>
        /// <param name="alias">Debugging alias</param>
        public UIElementCollection(UIElementCollection parent, FrameStrata.FrameStratum depth, Rectangle boundingBox, string alias = "Unnamed"){
            _frameStrata = new FrameStrata(depth, parent.FrameStrata, alias);
            _elements = new PriorityQueue<IUIElement>();
            _boundingBox = boundingBox;
            _parentCollection = parent;
            _mouseManager = _parentCollection._mouseManager;
            _mouseController = new MouseController(this);
            _alpha = 1;
            _hoverTimer = new Stopwatch();
            SetupEventPropagation();
            SetupEventPropagationToChildren();
        }

        /// <summary>
        /// Defines whether or not this collection should be considered transparent by the hit detection functions.
        /// This doesn't actually define whether this collection is visually transparent, just  whether or not it's
        /// transparent to hittests.
        /// </summary>
        public bool IsTransparent { get; set; }

        public bool ContainsMouse { get; private set; }
        public bool MouseHovering { get; private set; }

        #region IUIElement Members

        public int Width{
            get { return _boundingBox.Width; }
        }

        public int Height{
            get { return _boundingBox.Height; }
        }

        public FrameStrata FrameStrata{
            get { return _frameStrata; }
        }

        public int X{
            get { return _boundingBox.X; }
            set{
                _boundingBox.X = value;
                foreach (var element in _elements){
                    element.X = X;
                }
            }
        }

        public int Y{
            get { return _boundingBox.Y; }
            set{
                _boundingBox.Y = value;
                foreach (var element in _elements){
                    element.Y = Y;
                }
            }
        }

        public float Alpha{
            get { return _alpha; }
            set{
                _alpha = value;
                foreach (var element in _elements){
                    element.Alpha = _alpha;
                }
            }
        }

        /// <summary>
        /// Provides an interface to access raw mouse input events.
        /// </summary>
        public MouseController MouseController{
            get { return _mouseController; }
        }

        /// <summary>
        /// Calculates whether or not the point at the specified xy coordinates intercepts with
        /// any of the interface elements in this collection.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool HitTest(int x, int y){
            if (!IsTransparent){
                return Contains(x, y);
            }
            return false;
        }

        public List<IUIElement> GetElementStackAtPoint(int x, int y){
            if (Contains(x, y)){
                var ret = new List<IUIElement>();
                foreach (var element in _elements){
                    ret.AddRange(element.GetElementStackAtPoint(x, y));
                }
                return ret;
            }
            return new List<IUIElement>();
        }

        #endregion

        #region static

        static readonly List<UIElementCollection> _bindOrder;

        static UIElementCollection _globalUIParent;

        static UIElementCollection(){
            _bindOrder = new List<UIElementCollection>();
        }

        /// <summary>
        /// Gets the element collection that's currently bound.
        /// </summary>
        public static UIElementCollection BoundCollection{
            get{
                if (_bindOrder.Count == 0){
                    throw new Exception("There is no element collection currently bound");
                }
                return _bindOrder[0];
            }
        }

        public static List<IUIElement> GetGlobalElementStack(int x, int y){
            return _globalUIParent.GetElementStackAtPoint(x, y);
        }

        #endregion

        public event Action<UIElementCollection> OnMouseFocusLost;
        public event Action<UIElementCollection> OnMouseFocusGained;
        public event Action<float, UIElementCollection> OnMouseHover;
        public event Action<ForgeMouseState, float, UIElementCollection> OnMouseMovement;
        public event Action<ForgeMouseState, float, UIElementCollection> OnMouseScroll;
        public event Action<ForgeMouseState, float, UIElementCollection> OnLeftClick;
        public event Action<ForgeMouseState, float, UIElementCollection> OnLeftDown;
        public event Action<ForgeMouseState, float, UIElementCollection> OnLeftRelease;
        public event Action<ForgeMouseState, float, UIElementCollection> OnRightClick;
        public event Action<ForgeMouseState, float, UIElementCollection> OnRightDown;
        public event Action<ForgeMouseState, float, UIElementCollection> OnRightRelease;
        public event Action<ForgeMouseState, float, UIElementCollection> OnMouseEntry;
        public event Action<ForgeMouseState, float, UIElementCollection> OnMouseExit;

        /// <summary>
        /// Calculates whether or not the point at the specified xy coordinates falls within
        /// the bounds of any of the elements inside this collection. This test ignores any
        /// transparency settings.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Contains(int x, int y){
            if (_boundingBox.Contains(x, y)){
                foreach (var elem in _elements){
                    if (elem.HitTest(x, y)){
                        return true;
                    }
                }
            }
            return false;
        }

        void SetupEventPropagation(){
            _mouseController.OnMouseButton +=
                (state, timeDelta) =>{
                    if (state.LeftButtonChange && !state.BlockLeftMButton){
                        if (state.LeftButtonState == ButtonState.Pressed){
                            if (OnLeftDown != null){
                                OnLeftDown.Invoke(state, timeDelta, this);
                            }
                        }
                        else{
                            if (OnLeftRelease != null){
                                OnLeftRelease.Invoke(state, timeDelta, this);
                            }
                        }
                        if (state.LeftButtonClick){
                            if (OnLeftClick != null){
                                OnLeftClick.Invoke(state, timeDelta, this);
                            }
                        }
                    }
                    if (state.RightButtonChange && !state.BlockRightMButton){
                        if (state.RightButtonState == ButtonState.Pressed){
                            if (OnRightDown != null){
                                OnRightDown.Invoke(state, timeDelta, this);
                            }
                        }
                        else{
                            if (OnRightRelease != null){
                                OnRightRelease.Invoke(state, timeDelta, this);
                            }
                        }
                        if (state.RightButtonClick){
                            if (OnRightClick != null){
                                OnRightClick.Invoke(state, timeDelta, this);
                            }
                        }
                    }
                };
            _mouseController.OnMouseFocusLost +=
                () =>{
                    if (OnMouseFocusLost != null){
                        OnMouseFocusLost.Invoke(this);
                    }
                };
            _mouseController.OnMouseFocusGained +=
                () =>{
                    if (OnMouseFocusGained != null){
                        OnMouseFocusGained.Invoke(this);
                    }
                };
            _mouseController.OnMouseMovement +=
                (state, timeDelta) =>{
                    if (!state.BlockMPosition){
                        //this event is only called when the mouse moves, so can safely turn off hover
                        MouseHovering = false;
                        bool containsNewMouse = Contains(state.X, state.Y);
                        //entry distpatcher
                        if (containsNewMouse && !ContainsMouse){
                            if (OnMouseEntry != null){
                                OnMouseEntry.Invoke(state, timeDelta, this);
                            }
                            _hoverTimer.Restart();
                        }

                        //exit dispatcher
                        if (!containsNewMouse && ContainsMouse){
                            if (OnMouseExit != null){
                                OnMouseExit.Invoke(state, timeDelta, this);
                            }
                            _hoverTimer.Reset();
                        }
                        else{
                            if (ContainsMouse){
                                //movement disrupts any hover effect, and we're certain that the mouse isnt exiting
                                _hoverTimer.Restart();
                            }
                        }

                        ContainsMouse = containsNewMouse;
                        if (OnMouseMovement != null){
                            OnMouseMovement.Invoke(state, timeDelta, this);
                        }
                    }
                };
            _mouseController.OnMouseScroll +=
                (state, timeDelta) =>{
                    if (!state.BlockScrollWheel){
                        if (OnMouseScroll != null){
                            OnMouseScroll.Invoke(state, timeDelta, this);
                        }
                    }
                };
        }

        /// <summary>
        /// Adds event subscriptions to _mouseController so that when this collection has an
        /// event called, that event is also propagated to subcomponents.
        /// </summary>
        void SetupEventPropagationToChildren(){
            _mouseController.OnMouseButton +=
                (state, timeDelta) =>{
                    foreach (var element in _elements){
                        element.MouseController.SafeInvokeOnMouseButton(state, timeDelta);
                    }
                };
            _mouseController.OnMouseFocusLost +=
                () =>{
                    foreach (var element in _elements){
                        element.MouseController.SafeInvokeOnFocusLost();
                    }
                };
            _mouseController.OnMouseFocusGained +=
                () =>{
                    foreach (var element in _elements){
                        element.MouseController.SafeInvokeOnFocusRegained();
                    }
                };
            _mouseController.OnMouseMovement +=
                (state, timeDelta) =>{
                    foreach (var element in _elements){
                        element.MouseController.SafeInvokeOnMouseMovement(state, timeDelta);
                    }
                };
            _mouseController.OnMouseScroll +=
                (state, timeDelta) =>{
                    foreach (var element in _elements){
                        element.MouseController.SafeInvokeOnMouseScroll(state, timeDelta);
                    }
                };
        }

        /// <summary>
        /// Binds this collection so that any newly generated ui elements are automatically added
        /// to it. If another collection calls bind while this collection is bound, the binds will
        /// nest.
        /// </summary>
        public void Bind(){
            Debug.Assert(!_bindOrder.Contains(this));
            _bindOrder.Insert(0, this);
        }

        /// <summary>
        /// Unbinds this collection.
        /// </summary>
        public void Unbind(){
            Debug.Assert(_bindOrder[0] == this);
            _bindOrder.RemoveAt(0);
        }

        public void Update(float timeDelta){
            if (ContainsMouse){
                if (_hoverTimer.ElapsedMilliseconds >= _hoverTime){
                    MouseHovering = true;
                    _hoverTimer.Reset();
                    if (OnMouseHover != null){
                        OnMouseHover.Invoke(timeDelta, this);
                    }
                }
            }
        }
    }
}