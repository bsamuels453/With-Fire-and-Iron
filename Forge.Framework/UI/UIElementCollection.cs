#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        readonly ElementPriorityQueue _elements;
        readonly FrameStrata _frameStrata;
        readonly UIElementCollection _parentCollection;
        Rectangle _boundingBox;

        /// <summary>
        /// parent constructor
        /// </summary>
        public UIElementCollection(){
            _frameStrata = new FrameStrata();
            _elements = new ElementPriorityQueue();
            _boundingBox = new Rectangle(0, 0, Resource.ScreenSize.X, Resource.ScreenSize.Y);
            _parentCollection = null;
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
            _elements = new ElementPriorityQueue();
            _boundingBox = boundingBox;
            _parentCollection = parent;
        }

        /// <summary>
        /// Defines whether or not this collection should be considered transparent by the hit detection functions.
        /// This doesn't actually define whether this collection is visually transparent, just  whether or not it's
        /// transparent to hittests.
        /// </summary>
        public bool IsTransparent { get; set; }

        #region IUIElement Members

        public FrameStrata FrameStrata{
            get { return _frameStrata; }
        }

        public float X{
            get { return _boundingBox.X; }
            set { throw new NotImplementedException(); }
        }

        public float Y{
            get { return _boundingBox.Y; }
            set { throw new NotImplementedException(); }
        }

        public float Width{
            get { return _boundingBox.Width; }
            set { throw new NotImplementedException(); }
        }

        public float Height{
            get { return _boundingBox.Height; }
            set { throw new NotImplementedException(); }
        }

        public float Alpha{
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
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

        #region Nested type: ElementPriorityQueue

        class ElementPriorityQueue : IEnumerable<IUIElement>{
            List<IUIElement> _objList;

            public ElementPriorityQueue(){
                _objList = new List<IUIElement>();
            }

            public int Count{
                get { return _objList.Count; }
            }

            public IUIElement this[int index]{
                get { return _objList[index]; }
            }

            #region IEnumerable<IUIElement> Members

            public IEnumerator<IUIElement> GetEnumerator(){
                return _objList.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator(){
                return GetEnumerator();
            }

            #endregion

            public void Add(float depth, IUIElement element){
                _objList.Add(element);
                _objList = _objList.OrderBy(o => o.FrameStrata).ToList();
            }

            public void Clear(){
                _objList.Clear();
            }

            public void RemoveAt(int index){
                _objList.RemoveAt(index);
            }

            public void Remove(IUIElement element){
                _objList.Remove(element);
            }
        }

        #endregion
    }
}