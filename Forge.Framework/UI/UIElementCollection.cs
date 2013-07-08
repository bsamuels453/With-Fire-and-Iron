#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Forge.Framework.Control;
using Forge.Framework.Draw;
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
    public class UIElementCollection : IUIElement{
        const float _hoverTime = 200;
        protected readonly MouseManager MouseManager;
        readonly string _alias;
        readonly PriorityQueue<IUIElement> _elements;
        readonly FrameStrata _frameStrata;
        readonly Stopwatch _hoverTimer;
        readonly MouseController _mouseController;
        readonly UIElementCollection _parentCollection;
        float _alpha;
        Rectangle _boundingBox;
        Sprite2D _boundingTexture;
        bool _disposed;

        /// <summary>
        /// parent constructor
        /// </summary>
        public UIElementCollection(MouseManager mouseManager){
            _frameStrata = new FrameStrata();
            _elements = new PriorityQueue<IUIElement>();
            _boundingBox = new Rectangle(0, 0, Resource.ScreenSize.X, Resource.ScreenSize.Y);
            _parentCollection = null;
            MouseManager = mouseManager;
            _mouseController = new MouseController(this);
            _alpha = 1;
            _hoverTimer = new Stopwatch();

            //mousemanager is only null for very special uielementcollections like the one used for debug text
            if (mouseManager != null){
                Debug.Assert(_globalUIParent == null);
                _globalUIParent = this;
                SetupEventPropagation();
                SetupEventPropagationToChildren();
                MouseManager.AddGlobalController(_mouseController, 0);
            }
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
        public UIElementCollection(UIElementCollection parent, FrameStrata.Level depth, Rectangle boundingBox, string alias = "Unnamed"){
            _frameStrata = new FrameStrata(depth, parent.FrameStrata, alias);
            _elements = new PriorityQueue<IUIElement>();
            _boundingBox = boundingBox;
            _parentCollection = parent;
            _parentCollection.AddCollection(this);
            MouseManager = _parentCollection.MouseManager;
            _mouseController = new MouseController(this);
            _alpha = 1;
            _hoverTimer = new Stopwatch();
            _alias = alias;
            SetupEventPropagation();
            SetupEventPropagationToChildren();
        }

        /// <summary>
        /// Try not to write to this. Only do so under special circumstances such as delayed definition of
        /// collection's bounding box later in inheritee ctor.
        /// </summary>
        protected Rectangle BoundingBox{
            get { return _boundingBox; }
            set{
                var newbbox = value;
                foreach (var element in _elements){
                    var childBbox = new Rectangle(element.X, element.Y, element.Width, element.Height);
                    Debug.Assert(newbbox.Contains(childBbox));
                }
                _boundingBox = newbbox;
#if DEBUG
                if (_boundingTexture != null){
                    _boundingTexture.X = _boundingBox.X;
                    _boundingTexture.Y = _boundingBox.Y;
                    _boundingTexture.Width = _boundingBox.Width;
                    _boundingTexture.Height = _boundingBox.Height;
                }
#endif
            }
        }

        /// <summary>
        /// Defines whether or not this collection should be considered transparent by the hit detection functions.
        /// This doesn't actually define whether this collection is visually transparent, just  whether or not it's
        /// transparent to hittests.
        /// </summary>
        public bool IsTransparent { get; set; }

        /// <summary>
        /// Whether or not this collection contains the mouse within any of its child sprites.
        /// This is updated when the OnMouseMovement event is invoked. This ignores any transparency
        /// settings. This will be false when there is another element covering this one, despite
        /// whether or not the mouse is within the element.
        /// </summary>
        public bool ContainsMouse { get; private set; }

        /// <summary>
        /// Whether or not the mouse is currently hovering over any of the elements in this collection.
        /// In order for the mouse to be hovering, it has to be within the bounds of a child sprite and
        /// must not have moved for _hovertime (200ms). This ignores any transparency settings.
        /// </summary>
        public bool MouseHovering { get; private set; }

        #region IUIElement Members

        public void Dispose(){
            if (!_disposed){
                foreach (var element in _elements){
                    element.Dispose();
                }
                if (_parentCollection != null){
                    _parentCollection.RemoveElement(this);
                }
                _disposed = true;
            }
            else{
                throw new Exception();
            }
        }

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
                int diff = value - _boundingBox.X;
                _boundingBox.X = value;
                foreach (var element in _elements){
                    element.X += diff;
                }
            }
        }

        public int Y{
            get { return _boundingBox.Y; }
            set{
                int diff = value - _boundingBox.Y;
                _boundingBox.Y = value;
                foreach (var element in _elements){
                    element.Y += diff;
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
        public virtual bool HitTest(int x, int y){
            if (!IsTransparent){
                return ContainsPoint(x, y);
            }
            return false;
        }

        public List<IUIElement> GetElementStackAtPoint(int x, int y){
            if (CollectionContains(x, y)){
                var ret = new List<IUIElement>();
                ret.Add(this);
                foreach (var element in _elements){
                    ret.AddRange(element.GetElementStackAtPoint(x, y));
                }
                return ret;
            }
            return new List<IUIElement>();
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
            UpdateChild(timeDelta);
            foreach (var element in _elements){
                element.Update(timeDelta);
            }
        }

        #endregion

        /// <summary>
        /// adds a background texture to the collection so that its boundingbox is visible.
        /// only use this for debugging.
        /// </summary>
        public void EnableBoundingTexture(bool propagateToChildCollections = false){
            _boundingTexture = new Sprite2D
                (
                "Materials/SolidBlack",
                _boundingBox.X,
                _boundingBox.Y,
                _boundingBox.Width,
                _boundingBox.Height,
                this.FrameStrata,
                FrameStrata.Level.DebugHigh,
                true
                );
            _boundingTexture.Alpha = 0.2f;
            this.AddElement(_boundingTexture);
            if (propagateToChildCollections){
                foreach (var element in _elements){
                    var collection = element as UIElementCollection;
                    if (collection != null){
                        collection.EnableBoundingTexture(true);
                    }
                }
            }
        }

        /// <summary>
        /// Does a hit test based off of collection bounding boxes, rather than contained sprites.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool CollectionContains(int x, int y){
            Debug.Assert(_boundingBox.Width != 0 && _boundingBox.Height != 0);
            return _boundingBox.Contains(x, y);
        }

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
        public bool ContainsPoint(int x, int y){
            if (_boundingBox.Contains(x, y)){
                foreach (var elem in _elements){
                    if (elem.HitTest(x, y)){
                        return true;
                    }
                }
            }
            return false;
        }

        public void AddElement(IUIElement element){
            var collection = element as UIElementCollection;
            //element collections add themselves to their parents using a private method.
            Debug.Assert(collection == null, "Element collections add themselves to their parent collection. Do not add them manually.");

            Debug.Assert(_boundingBox.Contains(new Rectangle(element.X, element.Y, element.Width, element.Height)));

            _elements.Add(element, element.FrameStrata.FrameStrataValue);
        }

        void AddCollection(UIElementCollection collection){
            _elements.Add(collection, collection.FrameStrata.FrameStrataValue);
        }

        public void RemoveElement(IUIElement element){
            _elements.Remove(element);
        }

        /// <summary>
        /// Sets up the conditions for this collection's personal events to fire.
        /// </summary>
        void SetupEventPropagation(){
            //even though we do a lot of checking the BlockWhateverButton state when getting ready
            //to dispatch events, it's critical that any delegates these events call check for 
            //themselves whether or not an input device/method is blocked. This code has no way of
            //preventing event invocation if an earlier invoked event set an input device to blocked.
            _mouseController.OnMouseButton +=
                (state, timeDelta) =>{
                    if (!state.BlockLeftMButton){
                        if (state.LeftButtonChange){
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
                            if (!state.BlockLeftMButton){
                                if (state.LeftButtonClick){
                                    if (OnLeftClick != null){
                                        OnLeftClick.Invoke(state, timeDelta, this);
                                    }
                                }
                            }
                        }
                    }
                    if (!state.BlockRightMButton){
                        if (state.RightButtonChange && !state.BlockRightMButton){
                            if (state.RightButtonState == ButtonState.Released){
                                if (OnRightRelease != null){
                                    OnRightRelease.Invoke(state, timeDelta, this);
                                }
                            }
                            else{
                                if (OnRightDown != null){
                                    OnRightDown.Invoke(state, timeDelta, this);
                                }
                            }
                            if (!state.BlockRightMButton){
                                if (state.RightButtonClick){
                                    if (OnRightClick != null){
                                        OnRightClick.Invoke(state, timeDelta, this);
                                    }
                                }
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
                        bool onTopOfStack = false;
                        var stack = GetGlobalElementStack(state.X, state.Y);
                        stack = stack.OrderBy(o => o.FrameStrata.FrameStrataValue).ToList();
                        stack = (
                            from element in stack
                            where !_elements.Contains(element)
                            select element
                            ).ToList();

                        if (stack.Count > 0){
                            if (stack[0] == this){
                                onTopOfStack = true;
                            }
                        }

                        //this event is only called when the mouse moves, so can safely turn off hover
                        MouseHovering = false;
                        bool containsNewMouse = ContainsPoint(state.X, state.Y);

                        //this means there's another object overlapping this one that prevents the mouse
                        //from interacting with it
                        if (!onTopOfStack && containsNewMouse){
                            containsNewMouse = false;
                        }

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

        protected virtual void UpdateChild(float timeDelta){
        }

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
    }
}