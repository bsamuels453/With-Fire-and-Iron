#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Forge.Framework.Control;
using Forge.Framework.Resources;
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
        readonly PriorityQueue<IUIElement> _elements;
        readonly FrameStrata _frameStrata;
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
            SetupEventPropagation();
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
            SetupEventPropagation();
        }

        /// <summary>
        /// Defines whether or not this collection should be considered transparent by the hit detection functions.
        /// This doesn't actually define whether this collection is visually transparent, just  whether or not it's
        /// transparent to hittests.
        /// </summary>
        public bool IsTransparent { get; set; }

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
                if (_boundingBox.Contains(x, y)){
                    foreach (var elem in _elements){
                        if (elem.HitTest(x, y)){
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        #endregion

        /// <summary>
        /// Adds event subscriptions to _mouseController so that when this collection has an
        /// event called, that event is also propagated to subcomponents.
        /// </summary>
        void SetupEventPropagation(){
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
            _mouseController.OnMouseFocusRegained +=
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

        #region static

        static readonly List<UIElementCollection> _bindOrder;

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

        #endregion
    }
}